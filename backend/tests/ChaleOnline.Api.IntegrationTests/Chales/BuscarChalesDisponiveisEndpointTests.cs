using System.Net;
using System.Net.Http.Json;
using ChaleOnline.Application.Chales;
using ChaleOnline.Domain;
using ChaleOnline.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace ChaleOnline.Api.IntegrationTests.Chales;

/// <summary>
/// Sem endpoint de criação de Reserva ainda (isso é Story 1.5), então os dados de conflito são
/// seedados diretamente no banco de teste via os construtores internos de Reserva/ReservaNoite
/// (liberados só para este assembly via InternalsVisibleTo em ChaleOnline.Domain).
///
/// IMPORTANTE: <see cref="InitializeAsync"/> limpa TODAS as linhas de Reserva/ReservaNoite em
/// chaleonline_test antes de cada teste desta classe. Por isso esta classe está na collection
/// "ReservaDbTests" (Story 1.5) junto com <see cref="Reservas.CriarReservaEndpointTests"/> — xUnit
/// roda classes de teste diferentes em paralelo por padrão, e as duas tocam Reserva/ReservaNoite.
/// </summary>
[Collection("ReservaDbTests")]
public class BuscarChalesDisponiveisEndpointTests : IClassFixture<ChalesApiFactory>, IAsyncLifetime
{
    private readonly ChalesApiFactory _factory;

    // Chalés seedados pela Story 1.1 (chaleonline_test): 1=Pinheiro Bravo (A), 2=Trilha da Neblina (A),
    // 3=Cabana do Vale (A), 7=Vista da Serra (B).
    private const int ChaleComConflito = 1;
    private const int ChaleComReservaForaDoIntervalo = 2;
    private const int ChaleComReservaCancelada = 3;

    public BuscarChalesDisponiveisEndpointTests(ChalesApiFactory factory)
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

    private static async Task<int> SeedReservaAsync(
        ChaleOnlineDbContext db,
        int chaleId,
        DateOnly checkin,
        DateOnly checkout,
        StatusReserva status)
    {
        var reserva = new Reserva(
            0,
            Guid.NewGuid(),
            chaleId,
            "Hóspede de Teste",
            "teste@example.com",
            checkin,
            checkout,
            500m,
            status,
            DateTime.UtcNow);

        db.Reservas.Add(reserva);
        await db.SaveChangesAsync();

        for (var data = checkin; data < checkout; data = data.AddDays(1))
        {
            db.ReservaNoites.Add(new ReservaNoite(chaleId, data, reserva.Id));
        }

        await db.SaveChangesAsync();
        return reserva.Id;
    }

    [Fact]
    public async Task Buscar_ChaleComReservaConflitante_NaoAparecceNosDisponiveis()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ChaleOnlineDbContext>();
        var checkin = new DateOnly(2026, 9, 10);
        var checkout = new DateOnly(2026, 9, 12);
        await SeedReservaAsync(db, ChaleComConflito, checkin, checkout, StatusReserva.Paga);

        using var client = _factory.CreateClient();
        var resposta = await client.GetAsync(
            $"/api/chales?checkin={checkin:yyyy-MM-dd}&checkout={checkout:yyyy-MM-dd}",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var chales = await resposta.Content.ReadFromJsonAsync<List<ChaleResumoDto>>(TestContext.Current.CancellationToken);
        Assert.NotNull(chales);
        Assert.DoesNotContain(chales, c => c.Id == ChaleComConflito);
    }

    [Fact]
    public async Task Buscar_ChaleComReservaForaDoIntervalo_ContinuaDisponivel()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ChaleOnlineDbContext>();
        // Reserva bem no futuro, fora da janela de busca abaixo.
        await SeedReservaAsync(db, ChaleComReservaForaDoIntervalo, new DateOnly(2026, 12, 1), new DateOnly(2026, 12, 3), StatusReserva.Paga);

        using var client = _factory.CreateClient();
        var resposta = await client.GetAsync(
            "/api/chales?checkin=2026-09-10&checkout=2026-09-12",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var chales = await resposta.Content.ReadFromJsonAsync<List<ChaleResumoDto>>(TestContext.Current.CancellationToken);
        Assert.NotNull(chales);
        Assert.Contains(chales, c => c.Id == ChaleComReservaForaDoIntervalo);
    }

    [Fact]
    public async Task Buscar_ChaleComReservaCancelada_ContinuaDisponivel()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ChaleOnlineDbContext>();
        var checkin = new DateOnly(2026, 9, 10);
        var checkout = new DateOnly(2026, 9, 12);
        await SeedReservaAsync(db, ChaleComReservaCancelada, checkin, checkout, StatusReserva.Cancelada);

        using var client = _factory.CreateClient();
        var resposta = await client.GetAsync(
            $"/api/chales?checkin={checkin:yyyy-MM-dd}&checkout={checkout:yyyy-MM-dd}",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var chales = await resposta.Content.ReadFromJsonAsync<List<ChaleResumoDto>>(TestContext.Current.CancellationToken);
        Assert.NotNull(chales);
        Assert.Contains(chales, c => c.Id == ChaleComReservaCancelada);
    }

    [Fact]
    public async Task Buscar_ComFiltroDeTipo_RetornaSoOsTiposSelecionados()
    {
        using var client = _factory.CreateClient();
        var resposta = await client.GetAsync(
            "/api/chales?checkin=2026-09-10&checkout=2026-09-12&tipos=B",
            TestContext.Current.CancellationToken);

        var chales = await resposta.Content.ReadFromJsonAsync<List<ChaleResumoDto>>(TestContext.Current.CancellationToken);
        Assert.NotNull(chales);
        Assert.NotEmpty(chales);
        Assert.All(chales, c => Assert.Equal("B", c.Tipo));
    }

    [Fact]
    public async Task Buscar_ComMultiplosTipos_RetornaAUniaoDosTipos()
    {
        using var client = _factory.CreateClient();
        var resposta = await client.GetAsync(
            "/api/chales?checkin=2026-09-10&checkout=2026-09-12&tipos=A&tipos=C",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var chales = await resposta.Content.ReadFromJsonAsync<List<ChaleResumoDto>>(TestContext.Current.CancellationToken);
        Assert.NotNull(chales);
        Assert.Contains(chales, c => c.Tipo == "A");
        Assert.Contains(chales, c => c.Tipo == "C");
        Assert.All(chales, c => Assert.True(c.Tipo is "A" or "C"));
    }

    [Fact]
    public async Task Buscar_ComTipoInvalidoSemDatas_Retorna400()
    {
        using var client = _factory.CreateClient();
        var resposta = await client.GetAsync(
            "/api/chales?tipos=Z",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task Buscar_ComCheckoutAntesDoCheckin_Retorna400()
    {
        using var client = _factory.CreateClient();
        var resposta = await client.GetAsync(
            "/api/chales?checkin=2026-09-12&checkout=2026-09-10",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task Buscar_ComApenasCheckinPreenchido_Retorna400()
    {
        using var client = _factory.CreateClient();
        var resposta = await client.GetAsync(
            "/api/chales?checkin=2026-09-10",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task Buscar_ComApenasCheckoutPreenchido_Retorna400()
    {
        using var client = _factory.CreateClient();
        var resposta = await client.GetAsync(
            "/api/chales?checkout=2026-09-12",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }
}
