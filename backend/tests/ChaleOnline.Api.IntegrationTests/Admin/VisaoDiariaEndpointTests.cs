using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ChaleOnline.Application.Admin;
using ChaleOnline.Application.Reservas;
using ChaleOnline.Infrastructure;
using ChaleOnline.Infrastructure.Jobs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChaleOnline.Api.IntegrationTests.Admin;

/// <summary>
/// Mesma collection "ReservaDbTests" que CriarReservaEndpointTests/CancelamentoAutomaticoJobTests
/// — toca Reserva/ReservaNoite de verdade (precisa rodar sequencialmente com as outras classes que
/// tocam essas tabelas).
/// </summary>
[Collection("ReservaDbTests")]
public class VisaoDiariaEndpointTests(ChalesApiFactory factory) : IClassFixture<ChalesApiFactory>, IAsyncLifetime
{
    private const string AdminEmail = "admin@chaleonline.com";
    private const string SenhaCorreta = "ChaleOnline@2026";

    // Chalé 5 = Clareira Dourada, Chalé 6 = Refúgio do Riacho (seed da Story 1.1).
    private const int ChaleValido = 5;
    private const int ChaleVirada = 6;

    public async ValueTask InitializeAsync()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ChaleOnlineDbContext>();
        db.ReservaNoites.RemoveRange(db.ReservaNoites);
        db.Reservas.RemoveRange(db.Reservas);
        await db.SaveChangesAsync();

        // Defesa contra o teste de rate-limit de AdminLoginEndpointTests (mesma collection,
        // mesmo admin seedado) bloquear a conta por 15 min antes desta classe rodar — a ordem
        // de execução entre classes na mesma collection não é garantida pelo xUnit, só
        // serializada (achado de code review, 2026-07-20).
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var admin = await userManager.FindByEmailAsync(AdminEmail);
        if (admin is not null)
        {
            await userManager.ResetAccessFailedCountAsync(admin);
            await userManager.SetLockoutEndDateAsync(admin, null);
        }
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // Usa HorarioBrasil.DiaCorrente (não DateOnly.FromDateTime(DateTime.UtcNow) naïve) — a própria
    // história instrui a nunca hardcodar/aproximar "hoje" em UTC nos testes desta feature: entre
    // 00:00-02:59 UTC (21:00-23:59 em São Paulo), a data UTC naïve já seria "amanhã" em São Paulo,
    // o que faria estes testes falharem de forma intermitente (achado de code review, 2026-07-20).
    private static DateOnly DataRelativaAHoje(int diasAPartirDeHoje) =>
        HorarioBrasil.DiaCorrente(DateTime.UtcNow).AddDays(diasAPartirDeHoje);

    private async Task<string> ObterTokenValidoAsync(HttpClient client)
    {
        var resposta = await client.PostAsJsonAsync(
            "/api/admin/login",
            new AdminLoginRequest(AdminEmail, SenhaCorreta),
            TestContext.Current.CancellationToken);
        var dto = await resposta.Content.ReadFromJsonAsync<AdminLoginResultadoDto>(TestContext.Current.CancellationToken);
        return dto!.Token;
    }

    private async Task<Guid> CriarReservaAsync(HttpClient client, int chaleId, DateOnly checkin, int quantidadeDiarias)
    {
        var resposta = await client.PostAsJsonAsync(
            "/api/reservas",
            new CriarReservaRequest(chaleId, "Hóspede Teste", "hospede@example.com", checkin, quantidadeDiarias),
            TestContext.Current.CancellationToken);
        var dto = await resposta.Content.ReadFromJsonAsync<ReservaCriadaDto>(TestContext.Current.CancellationToken);
        return dto!.CodigoConsulta;
    }

    private Task<Guid> CriarReservaComCheckinHojeAsync(HttpClient client) =>
        CriarReservaAsync(client, ChaleValido, DataRelativaAHoje(0), 3);

    [Fact]
    public async Task GetVisaoDiaria_SemAuthorizationHeader_Retorna401()
    {
        using var client = factory.CreateClient();

        var resposta = await client.GetAsync("/api/admin/visao-diaria", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task GetVisaoDiaria_SemReservasCobrindoHoje_Retorna200ComOsDozeChalesDesocupados()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await ObterTokenValidoAsync(client));

        var resposta = await client.GetAsync("/api/admin/visao-diaria", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var visao = await resposta.Content.ReadFromJsonAsync<List<VisaoDiariaChaleDto>>(TestContext.Current.CancellationToken);
        Assert.NotNull(visao);
        Assert.Equal(12, visao.Count);
        Assert.All(visao, dto => Assert.Equal("Desocupado", dto.Estado));
    }

    [Fact]
    public async Task GetVisaoDiaria_ComReservaValidaComCheckinHoje_MostraOChaleComoCheckInHoje()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await ObterTokenValidoAsync(client));
        await CriarReservaComCheckinHojeAsync(client);

        var resposta = await client.GetAsync("/api/admin/visao-diaria", TestContext.Current.CancellationToken);

        var visao = await resposta.Content.ReadFromJsonAsync<List<VisaoDiariaChaleDto>>(TestContext.Current.CancellationToken);
        var chale = visao!.Single(dto => dto.ChaleId == ChaleValido);
        Assert.Equal("CheckInHoje", chale.Estado);
        Assert.Contains("Hóspede Teste", chale.Detalhe);
    }

    /// <summary>
    /// Único caso ambíguo da fórmula de ocupação — duas Reservas diferentes pro mesmo Chalé, uma
    /// saindo e outra entrando hoje. Prova real (não só no classificador, com Reservas falsas em
    /// memória) de que a query do repositório (`DataCheckin &lt;= data &amp;&amp; DataCheckout &gt;= data`) traz
    /// as duas Reservas corretamente e o endpoint classifica como "virada", não como dois estados
    /// isolados (achado de code review, 2026-07-20).
    ///
    /// `POST /api/reservas` real nunca aceita `DataCheckin` no passado (regra de negócio do
    /// domínio) — não dá pra criar uma Reserva "saindo hoje" só com checkin no futuro. Contorna
    /// criando-a com checkin hoje e retrocedendo `DataCheckin`/`DataCheckout` via SQL depois (mesma
    /// técnica já usada pra retroceder `CriadoEm` em CancelamentoAutomaticoJobTests) — simula uma
    /// Reserva que foi criada dias atrás e está saindo hoje.
    /// </summary>
    [Fact]
    public async Task GetVisaoDiaria_ComUmaReservaSaindoEOutraEntrandoHojeNoMesmoChale_MostraViradaMesmoDia()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await ObterTokenValidoAsync(client));

        var codigoQueSai = await CriarReservaAsync(client, ChaleVirada, DataRelativaAHoje(0), 3);
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ChaleOnlineDbContext>();
            var novoCheckin = DataRelativaAHoje(-3);
            var novoCheckout = DataRelativaAHoje(0);
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE Reserva SET DataCheckin = {novoCheckin}, DataCheckout = {novoCheckout} WHERE CodigoConsulta = {codigoQueSai}",
                TestContext.Current.CancellationToken);

            // Libera as noites (hoje, hoje+1, hoje+2) que a Reserva original ocupava antes de
            // retroceder as datas — sem isso, a segunda Reserva abaixo (mesmo Chalé, checkin hoje)
            // colidiria com o anti-overbooking por causa de ReservaNoite órfã, mesmo a query de
            // ocupação (BuscarRelevantesParaDataAsync) não dependendo de ReservaNoite pra nada aqui.
            var reservaId = await db.Reservas.Where(r => r.CodigoConsulta == codigoQueSai).Select(r => r.Id).SingleAsync(TestContext.Current.CancellationToken);
            await db.ReservaNoites.Where(n => n.ReservaId == reservaId).ExecuteDeleteAsync(TestContext.Current.CancellationToken);
        }

        await CriarReservaAsync(client, ChaleVirada, DataRelativaAHoje(0), 3); // checkin = hoje

        var resposta = await client.GetAsync("/api/admin/visao-diaria", TestContext.Current.CancellationToken);

        var visao = await resposta.Content.ReadFromJsonAsync<List<VisaoDiariaChaleDto>>(TestContext.Current.CancellationToken);
        var chale = visao!.Single(dto => dto.ChaleId == ChaleVirada);
        Assert.Equal("ViradaMesmoDia", chale.Estado);
    }

    /// <summary>AC #2, prova real (não só no classificador): uma Reserva cancelada nunca aparece como "ocupado".</summary>
    [Fact]
    public async Task GetVisaoDiaria_ComReservaCanceladaCujoCheckinEraHoje_MostraOChaleComoDesocupado()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await ObterTokenValidoAsync(client));
        var codigo = await CriarReservaComCheckinHojeAsync(client);

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ChaleOnlineDbContext>();
            var novoCriadoEm = DateTime.UtcNow - TimeSpan.FromHours(49);
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE Reserva SET CriadoEm = {novoCriadoEm} WHERE CodigoConsulta = {codigo}",
                TestContext.Current.CancellationToken);

            var job = scope.ServiceProvider.GetRequiredService<CancelarReservasExpiradasJob>();
            await job.ExecutarAsync(TestContext.Current.CancellationToken);
        }

        var resposta = await client.GetAsync("/api/admin/visao-diaria", TestContext.Current.CancellationToken);

        var visao = await resposta.Content.ReadFromJsonAsync<List<VisaoDiariaChaleDto>>(TestContext.Current.CancellationToken);
        var chale = visao!.Single(dto => dto.ChaleId == ChaleValido);
        Assert.Equal("Desocupado", chale.Estado);
    }
}
