using System.Net;
using System.Net.Http.Json;
using ChaleOnline.Application.Reservas;
using ChaleOnline.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChaleOnline.Api.IntegrationTests.Reservas;

/// <summary>Mesma collection "ReservaDbTests" — toca Reserva/ReservaNoite (ver CriarReservaEndpointTests).</summary>
[Collection("ReservaDbTests")]
public class ConsultarReservaEndpointTests(ChalesApiFactory factory) : IClassFixture<ChalesApiFactory>, IAsyncLifetime
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

    [Fact]
    public async Task GetReservaPorCodigo_ComReservaRecemCriada_Retorna200ComExpiradaFalse()
    {
        using var client = factory.CreateClient();
        var codigo = await CriarReservaAsync(client, 10);

        var resposta = await client.GetAsync($"/api/reservas/{codigo}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var dto = await resposta.Content.ReadFromJsonAsync<ReservaConsultaDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(dto);
        Assert.Equal(codigo, dto.CodigoConsulta);
        Assert.False(dto.Expirada);
        Assert.Equal("AguardandoPagamento", dto.Status);
        Assert.Equal("Recanto da Araucária", dto.NomeChale);
        Assert.Equal(890m, dto.ValorTotal);
    }

    [Fact]
    public async Task GetReservaPorCodigo_ComReservaExpiradaPorTempo_Retorna200ComExpiradaTrue()
    {
        using var client = factory.CreateClient();
        var codigo = await CriarReservaAsync(client, 20);
        await BackdatearCriadoEmAsync(codigo, TimeSpan.FromHours(49));

        var resposta = await client.GetAsync($"/api/reservas/{codigo}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var dto = await resposta.Content.ReadFromJsonAsync<ReservaConsultaDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(dto);
        Assert.True(dto.Expirada);
        Assert.Equal("AguardandoPagamento", dto.Status); // ainda não processada pelo job, mas já calculada como expirada
    }

    [Fact]
    public async Task GetReservaPorCodigo_ComCodigoInexistente_Retorna404ComCodigoReservaNotFound()
    {
        using var client = factory.CreateClient();

        var resposta = await client.GetAsync($"/api/reservas/{Guid.NewGuid()}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        var corpo = await resposta.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("RESERVA_NOT_FOUND", corpo);
    }

    /// <summary>Story 1.7, AC #1: a consulta precisa mostrar corretamente cada um dos 3 valores de Status.</summary>
    [Fact]
    public async Task GetReservaPorCodigo_ComReservaPaga_Retorna200ComStatusPaga()
    {
        using var client = factory.CreateClient();
        var codigo = await CriarReservaAsync(client, 40);

        var respostaPagamento = await client.PostAsJsonAsync(
            $"/api/reservas/{codigo}/pagamento",
            new ConfirmarPagamentoRequest("CartaoCredito"),
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, respostaPagamento.StatusCode);

        // Retrocede CriadoEm DEPOIS de já paga — se Expirada fosse calculado só por tempo (ignorando
        // Status), este teste pegaria isso: teria que dar Expirada=true, mas Paga nunca expira.
        await BackdatearCriadoEmAsync(codigo, TimeSpan.FromHours(1000));

        var resposta = await client.GetAsync($"/api/reservas/{codigo}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var dto = await resposta.Content.ReadFromJsonAsync<ReservaConsultaDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(dto);
        Assert.Equal("Paga", dto.Status);
        Assert.False(dto.Expirada); // Paga nunca expira, mesmo que CriadoEm seja antigo
    }

    /// <summary>Story 1.7, AC #1: a consulta precisa mostrar corretamente cada um dos 3 valores de Status.</summary>
    [Fact]
    public async Task GetReservaPorCodigo_ComReservaCancelada_Retorna200ComStatusCancelada()
    {
        using var client = factory.CreateClient();
        var codigo = await CriarReservaAsync(client, 41);
        await BackdatearCriadoEmAsync(codigo, TimeSpan.FromHours(49));

        using (var scope = factory.Services.CreateScope())
        {
            var job = scope.ServiceProvider.GetRequiredService<ChaleOnline.Infrastructure.Jobs.CancelarReservasExpiradasJob>();
            await job.ExecutarAsync(TestContext.Current.CancellationToken);
        }

        var resposta = await client.GetAsync($"/api/reservas/{codigo}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var dto = await resposta.Content.ReadFromJsonAsync<ReservaConsultaDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(dto);
        Assert.Equal("Cancelada", dto.Status);
        Assert.True(dto.Expirada);
    }
}
