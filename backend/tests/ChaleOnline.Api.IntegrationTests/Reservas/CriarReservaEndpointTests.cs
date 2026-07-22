using System.Net;
using System.Net.Http.Json;
using ChaleOnline.Application.Reservas;
using ChaleOnline.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChaleOnline.Api.IntegrationTests.Reservas;

/// <summary>
/// Nesta collection "ReservaDbTests" junto com BuscarChalesDisponiveisEndpointTests — ambas
/// truncam Reserva/ReservaNoite em chaleonline_test, xUnit roda classes diferentes em paralelo
/// por padrão, então precisam rodar sequencialmente pra não colidir.
/// </summary>
[Collection("ReservaDbTests")]
public class CriarReservaEndpointTests : IClassFixture<ChalesApiFactory>, IAsyncLifetime
{
    private readonly ChalesApiFactory _factory;

    // Chalés seedados pela Story 1.1 (chaleonline_test): 4=Recanto da Araucária, 5=Clareira
    // Dourada, 6=Refúgio do Riacho (todos Tipo A, 420-445), 11=Grande Refúgio (Tipo C).
    private const int ChaleValido = 4;
    private const int ChaleInexistente = 99999;
    private const int ChaleParaConcorrencia = 6;

    public CriarReservaEndpointTests(ChalesApiFactory factory)
    {
        _factory = factory;
    }

    public async ValueTask InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ChaleOnlineDbContext>();
        db.ReservaNoites.RemoveRange(db.ReservaNoites);
        db.Reservas.RemoveRange(db.Reservas);
        await db.SaveChangesAsync();
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    private static DateOnly DataFutura(int diasAPartirDeHoje) =>
        DateOnly.FromDateTime(DateTime.UtcNow).AddDays(diasAPartirDeHoje);

    [Fact]
    public async Task PostReservas_ComDadosValidos_Retorna201EPersisteReservaNoitePorDiaria()
    {
        using var client = _factory.CreateClient();
        var checkin = DataFutura(10);
        var request = new CriarReservaRequest(ChaleValido, "Maria Souza", "maria@example.com", checkin, 3);

        var resposta = await client.PostAsJsonAsync("/api/reservas", request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        var dto = await resposta.Content.ReadFromJsonAsync<ReservaCriadaDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(dto);
        Assert.NotEqual(Guid.Empty, dto.CodigoConsulta);
        Assert.Equal(checkin, dto.DataCheckin);
        Assert.Equal(checkin.AddDays(3), dto.DataCheckout);
        Assert.Equal("AguardandoPagamento", dto.Status);
        Assert.Equal(1335m, dto.ValorTotal); // Chalé 4 = Recanto da Araucária, 445m/noite × 3 diárias

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ChaleOnlineDbContext>();
        var noites = await db.ReservaNoites
            .Where(n => n.ChaleId == ChaleValido)
            .ToListAsync(TestContext.Current.CancellationToken);
        Assert.Equal(3, noites.Count);
    }

    [Fact]
    public async Task PostReservas_ComChaleInexistente_Retorna404ComCodigoChaleNotFound()
    {
        using var client = _factory.CreateClient();
        var request = new CriarReservaRequest(ChaleInexistente, "Maria Souza", "maria@example.com", DataFutura(10), 2);

        var resposta = await client.PostAsJsonAsync("/api/reservas", request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        var corpo = await resposta.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("CHALE_NOT_FOUND", corpo);
    }

    [Fact]
    public async Task PostReservas_ComQuantidadeDiariasZero_Retorna400ComCodigoValidationError()
    {
        using var client = _factory.CreateClient();
        var request = new CriarReservaRequest(ChaleValido, "Maria Souza", "maria@example.com", DataFutura(10), 0);

        var resposta = await client.PostAsJsonAsync("/api/reservas", request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        var corpo = await resposta.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("VALIDATION_ERROR", corpo);
    }

    [Fact]
    public async Task PostReservas_ComNomeVazio_Retorna400ComCodigoValidationError()
    {
        using var client = _factory.CreateClient();
        var request = new CriarReservaRequest(ChaleValido, "", "maria@example.com", DataFutura(10), 2);

        var resposta = await client.PostAsJsonAsync("/api/reservas", request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        var corpo = await resposta.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("VALIDATION_ERROR", corpo);
    }

    /// <summary>
    /// AC #2 — prova real de concorrência (AD-3, "Proof technique SM-1"). Dispara N requisições HTTP
    /// paralelas reais (não mock) pro mesmo Chalé/datas sobrepostas e assert que exatamente uma
    /// confirma; exercita a constraint UNIQUE(ChaleId, Data) de verdade em chaleonline_test.
    /// </summary>
    [Fact]
    public async Task PostReservas_ComTentativasSimultaneasConflitantes_ExatamenteUmaConfirma()
    {
        using var client = _factory.CreateClient();
        var checkin = DataFutura(60);

        var respostas = await Task.WhenAll(Enumerable.Range(0, 5).Select(indice =>
            client.PostAsJsonAsync(
                "/api/reservas",
                new CriarReservaRequest(ChaleParaConcorrencia, $"Hóspede {indice}", $"hospede{indice}@example.com", checkin, 2),
                TestContext.Current.CancellationToken)));

        Assert.Single(respostas, resposta => resposta.StatusCode == HttpStatusCode.Created);
        Assert.Equal(4, respostas.Count(resposta => resposta.StatusCode == HttpStatusCode.Conflict));

        foreach (var resposta in respostas.Where(r => r.StatusCode == HttpStatusCode.Conflict))
        {
            var corpo = await resposta.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            Assert.Contains("RESERVATION_CONFLICT", corpo);
        }

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ChaleOnlineDbContext>();
        var noites = await db.ReservaNoites
            .Where(n => n.ChaleId == ChaleParaConcorrencia)
            .ToListAsync(TestContext.Current.CancellationToken);
        Assert.Equal(2, noites.Count);
    }

    /// <summary>
    /// AC #2 exige explicitamente sobreposição "mesmo parcial" sob concorrência real — o teste acima
    /// só cobre datas idênticas em paralelo. Aqui 3 requisições paralelas têm datas de início
    /// escalonadas (D, D+1, D+2 com 3 diárias cada) que se sobrepõem em cadeia — todas tocam a noite
    /// D+2 — exercitando ordens de lock diferentes o suficiente pra provocar tanto violação de chave
    /// duplicada quanto deadlock genuíno do InnoDB (ver MySqlErroDeadlock em ReservaRepository).
    /// </summary>
    [Fact]
    public async Task PostReservas_ComTentativasSimultaneasParcialmenteSobrepostas_ExatamenteUmaConfirma()
    {
        const int chaleParaSobreposicaoConcorrente = 5; // Clareira Dourada — não usado em outros testes desta classe
        using var client = _factory.CreateClient();
        var checkin = DataFutura(120);

        var respostas = await Task.WhenAll(Enumerable.Range(0, 3).Select(indice =>
            client.PostAsJsonAsync(
                "/api/reservas",
                new CriarReservaRequest(
                    chaleParaSobreposicaoConcorrente,
                    $"Hóspede Sobreposto {indice}",
                    $"sobreposto{indice}@example.com",
                    checkin.AddDays(indice),
                    3),
                TestContext.Current.CancellationToken)));

        Assert.Single(respostas, resposta => resposta.StatusCode == HttpStatusCode.Created);
        Assert.Equal(2, respostas.Count(resposta => resposta.StatusCode == HttpStatusCode.Conflict));

        foreach (var resposta in respostas.Where(r => r.StatusCode == HttpStatusCode.Conflict))
        {
            var corpo = await resposta.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            Assert.Contains("RESERVATION_CONFLICT", corpo);
        }
    }

    [Fact]
    public async Task PostReservas_ComDatasParcialmenteSobrepostas_RetornaConflito()
    {
        using var client = _factory.CreateClient();
        var checkinExistente = DataFutura(90);
        var primeira = new CriarReservaRequest(ChaleValido, "Primeiro Hóspede", "primeiro@example.com", checkinExistente, 4);

        var respostaPrimeira = await client.PostAsJsonAsync("/api/reservas", primeira, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Created, respostaPrimeira.StatusCode);

        // Sobreposição parcial: começa 2 dias depois do checkin da primeira, mas antes do checkout dela.
        var segunda = new CriarReservaRequest(ChaleValido, "Segundo Hóspede", "segundo@example.com", checkinExistente.AddDays(2), 3);
        var respostaSegunda = await client.PostAsJsonAsync("/api/reservas", segunda, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, respostaSegunda.StatusCode);
        var corpo = await respostaSegunda.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("RESERVATION_CONFLICT", corpo);
    }
}
