using System.Net;
using System.Net.Http.Json;
using System.Text;
using ChaleOnline.Application.Reservas;
using ChaleOnline.Domain;
using ChaleOnline.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChaleOnline.Api.IntegrationTests.Reservas;

/// <summary>Mesma collection "ReservaDbTests" — toca Reserva/ReservaNoite (ver CriarReservaEndpointTests).</summary>
[Collection("ReservaDbTests")]
public class ConfirmarPagamentoEndpointTests(ChalesApiFactory factory) : IClassFixture<ChalesApiFactory>, IAsyncLifetime
{
    // Chalé 4 = Recanto da Araucária, 445m/noite (mesmo seed usado por CriarReservaEndpointTests).
    private const int ChaleValido = 4;

    public async ValueTask InitializeAsync()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ChaleOnlineDbContext>();
        db.ReservaNoites.RemoveRange(db.ReservaNoites);
        db.Reservas.RemoveRange(db.Reservas);
        await db.SaveChangesAsync();
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    private static DateOnly DataFutura(int diasAPartirDeHoje) =>
        DateOnly.FromDateTime(DateTime.UtcNow).AddDays(diasAPartirDeHoje);

    private async Task<Guid> CriarReservaAsync(HttpClient client, int diasAPartirDeHoje)
    {
        var resposta = await client.PostAsJsonAsync(
            "/api/reservas",
            new CriarReservaRequest(ChaleValido, "Hóspede Teste", "hospede@example.com", DataFutura(diasAPartirDeHoje), 2),
            TestContext.Current.CancellationToken);
        var dto = await resposta.Content.ReadFromJsonAsync<ReservaCriadaDto>(TestContext.Current.CancellationToken);
        return dto!.CodigoConsulta;
    }

    private async Task BackdatearCriadoEmAsync(Guid codigo, TimeSpan retroceder)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ChaleOnlineDbContext>();
        // SQL bruto em vez de ExecuteUpdateAsync — o ValueConverter de CriadoEm (ReservaConfiguration)
        // não traduz corretamente uma expressão não-constante (DateTime.UtcNow - retroceder) dentro de
        // um SetProperty (bug de tradução do EF Core), então contorna via SQL parametrizado direto.
        var novoCriadoEm = DateTime.UtcNow - retroceder;
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE Reserva SET CriadoEm = {novoCriadoEm} WHERE CodigoConsulta = {codigo}");
    }

    private async Task<StatusReserva> StatusAtualAsync(Guid codigo)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ChaleOnlineDbContext>();
        var reserva = await db.Reservas.SingleAsync(r => r.CodigoConsulta == codigo, TestContext.Current.CancellationToken);
        return reserva.Status;
    }

    [Fact]
    public async Task PostPagamento_ComFormaValida_Retorna200EStatusVaiParaPagaNoBanco()
    {
        using var client = factory.CreateClient();
        var codigo = await CriarReservaAsync(client, 10);

        var resposta = await client.PostAsJsonAsync(
            $"/api/reservas/{codigo}/pagamento",
            new ConfirmarPagamentoRequest("CartaoCredito"),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var dto = await resposta.Content.ReadFromJsonAsync<ConfirmarPagamentoResultadoDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(dto);
        Assert.True(dto.Aprovado);
        Assert.Equal("Paga", dto.Status);
        Assert.Equal(StatusReserva.Paga, await StatusAtualAsync(codigo));
    }

    [Fact]
    public async Task PostPagamento_ComCartaoRecusadoTeste_Retorna200Recusado_StatusContinuaAguardandoPagamentoENovaTentativaFunciona()
    {
        using var client = factory.CreateClient();
        var codigo = await CriarReservaAsync(client, 11);

        var respostaRecusada = await client.PostAsJsonAsync(
            $"/api/reservas/{codigo}/pagamento",
            new ConfirmarPagamentoRequest("CartaoRecusadoTeste"),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, respostaRecusada.StatusCode);
        var dtoRecusado = await respostaRecusada.Content.ReadFromJsonAsync<ConfirmarPagamentoResultadoDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(dtoRecusado);
        Assert.False(dtoRecusado.Aprovado);
        Assert.NotNull(dtoRecusado.MensagemRecusa);
        Assert.Equal(StatusReserva.AguardandoPagamento, await StatusAtualAsync(codigo));

        var respostaNovaTentativa = await client.PostAsJsonAsync(
            $"/api/reservas/{codigo}/pagamento",
            new ConfirmarPagamentoRequest("Pix"),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, respostaNovaTentativa.StatusCode);
        var dtoAprovado = await respostaNovaTentativa.Content.ReadFromJsonAsync<ConfirmarPagamentoResultadoDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(dtoAprovado);
        Assert.True(dtoAprovado.Aprovado);
        Assert.Equal(StatusReserva.Paga, await StatusAtualAsync(codigo));
    }

    [Fact]
    public async Task PostPagamento_ComCodigoInexistente_Retorna404ComCodigoReservaNotFound()
    {
        using var client = factory.CreateClient();

        var resposta = await client.PostAsJsonAsync(
            $"/api/reservas/{Guid.NewGuid()}/pagamento",
            new ConfirmarPagamentoRequest("CartaoCredito"),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        var corpo = await resposta.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("RESERVA_NOT_FOUND", corpo);
    }

    [Fact]
    public async Task PostPagamento_ComFormaPagamentoInvalida_Retorna400ComCodigoValidationError()
    {
        using var client = factory.CreateClient();
        var codigo = await CriarReservaAsync(client, 12);

        var resposta = await client.PostAsJsonAsync(
            $"/api/reservas/{codigo}/pagamento",
            new ConfirmarPagamentoRequest("BitcoinMagico"),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        var corpo = await resposta.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("VALIDATION_ERROR", corpo);
    }

    [Theory]
    [InlineData("")]
    [InlineData("{}")]
    [InlineData("{ isso não é json")]
    public async Task PostPagamento_ComBodyNuloVazioOuMalformado_Retorna400DentroDoEnvelope(string corpoBruto)
    {
        using var client = factory.CreateClient();
        var codigo = await CriarReservaAsync(client, 13);

        using var conteudo = new StringContent(corpoBruto, Encoding.UTF8, "application/json");
        var resposta = await client.PostAsync($"/api/reservas/{codigo}/pagamento", conteudo, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        var corpo = await resposta.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("VALIDATION_ERROR", corpo);
    }

    [Fact]
    public async Task PostPagamento_ComReservaExpiradaPorTempo_Retorna410ComCodigoReservaExpirada()
    {
        using var client = factory.CreateClient();
        var codigo = await CriarReservaAsync(client, 14);
        await BackdatearCriadoEmAsync(codigo, TimeSpan.FromHours(49));

        var resposta = await client.PostAsJsonAsync(
            $"/api/reservas/{codigo}/pagamento",
            new ConfirmarPagamentoRequest("CartaoCredito"),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Gone, resposta.StatusCode);
        var corpo = await resposta.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("RESERVA_EXPIRADA", corpo);
        Assert.Equal(StatusReserva.AguardandoPagamento, await StatusAtualAsync(codigo)); // job de cancelamento não rodou, mas o pagamento também não confirma
    }
}
