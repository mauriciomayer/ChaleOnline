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
/// Mesma collection "ReservaDbTests" que VisaoDiariaEndpointTests/CriarReservaEndpointTests —
/// toca Reserva/ReservaNoite de verdade (precisa rodar sequencialmente com as outras classes que
/// tocam essas tabelas).
/// </summary>
[Collection("ReservaDbTests")]
public class RelatorioMensalEndpointTests(ChalesApiFactory factory) : IClassFixture<ChalesApiFactory>, IAsyncLifetime
{
    private const string AdminEmail = "admin@chaleonline.com";
    private const string SenhaCorreta = "ChaleOnline@2026";

    // Chalé 5 = Clareira Dourada (seed da Story 1.1).
    private const int ChaleValido = 5;

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

    // Mesma armadilha de fuso horário já corrigida em code review na Story 3.2: "mês corrente"
    // precisa vir de HorarioBrasil.DiaCorrente, nunca de DateTime.UtcNow.Month cru (entre
    // 00:00-02:59 UTC o mês em UTC já pode ser diferente do mês em São Paulo perto da virada).
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

    private async Task<Guid> CriarReservaComCheckinHojeAsync(HttpClient client) =>
        (await CriarReservaAsync(client)).CodigoConsulta;

    private async Task<ReservaCriadaDto> CriarReservaAsync(HttpClient client)
    {
        var resposta = await client.PostAsJsonAsync(
            "/api/reservas",
            new CriarReservaRequest(ChaleValido, "Hóspede Teste", "hospede@example.com", DataRelativaAHoje(0), 3),
            TestContext.Current.CancellationToken);
        var dto = await resposta.Content.ReadFromJsonAsync<ReservaCriadaDto>(TestContext.Current.CancellationToken);
        return dto!;
    }

    private async Task<RelatorioMensalDto> ConsultarRelatorioAsync(HttpClient client, int ano, int mes)
    {
        var resposta = await client.GetAsync($"/api/admin/relatorio-mensal?ano={ano}&mes={mes}", TestContext.Current.CancellationToken);
        return (await resposta.Content.ReadFromJsonAsync<RelatorioMensalDto>(TestContext.Current.CancellationToken))!;
    }

    [Fact]
    public async Task GetRelatorioMensal_SemAuthorizationHeader_Retorna401()
    {
        using var client = factory.CreateClient();

        var resposta = await client.GetAsync("/api/admin/relatorio-mensal?ano=2026&mes=7", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task GetRelatorioMensal_SemAnoOuMes_Retorna400ValidationError()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await ObterTokenValidoAsync(client));

        var resposta = await client.GetAsync("/api/admin/relatorio-mensal", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        var corpo = await resposta.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("VALIDATION_ERROR", corpo);
    }

    [Fact]
    public async Task GetRelatorioMensal_ComMesInvalido_Retorna400ValidationError()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await ObterTokenValidoAsync(client));

        var resposta = await client.GetAsync("/api/admin/relatorio-mensal?ano=2026&mes=13", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        var corpo = await resposta.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("VALIDATION_ERROR", corpo);
    }

    [Fact]
    public async Task GetRelatorioMensal_ComAnoInvalido_Retorna400ValidationError()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await ObterTokenValidoAsync(client));

        var resposta = await client.GetAsync("/api/admin/relatorio-mensal?ano=0&mes=7", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        var corpo = await resposta.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("VALIDATION_ERROR", corpo);
    }

    /// <summary>ano=9999 com mes=12 faria o AddMonths(1) do use case ultrapassar o ano máximo de DateOnly.</summary>
    [Fact]
    public async Task GetRelatorioMensal_ComAnoNoLimiteSuperiorEMesDezembro_Retorna400ValidationError()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await ObterTokenValidoAsync(client));

        var resposta = await client.GetAsync("/api/admin/relatorio-mensal?ano=9999&mes=12", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        var corpo = await resposta.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("VALIDATION_ERROR", corpo);
    }

    [Fact]
    public async Task GetRelatorioMensal_SoComMesSemAno_Retorna400ValidationError()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await ObterTokenValidoAsync(client));

        var resposta = await client.GetAsync("/api/admin/relatorio-mensal?mes=7", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        var corpo = await resposta.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("VALIDATION_ERROR", corpo);
    }

    [Fact]
    public async Task GetRelatorioMensal_SemNenhumaReservaNoMes_Retorna200ComListaVaziaEResumoZerado()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await ObterTokenValidoAsync(client));
        var hoje = DataRelativaAHoje(0);

        var relatorio = await ConsultarRelatorioAsync(client, hoje.Year, hoje.Month);

        Assert.Empty(relatorio.Reservas);
        Assert.Equal(0, relatorio.Resumo.QuantidadeTotal);
        Assert.Equal(0m, relatorio.Resumo.TotalValores);
    }

    [Fact]
    public async Task GetRelatorioMensal_ComReservaNoMesCorrente_MostraNaListaENoTotal()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await ObterTokenValidoAsync(client));
        var criada = await CriarReservaAsync(client);
        var hoje = DataRelativaAHoje(0);

        var relatorio = await ConsultarRelatorioAsync(client, hoje.Year, hoje.Month);

        var linha = Assert.Single(relatorio.Reservas);
        Assert.Equal(criada.CodigoConsulta.ToString(), linha.CodigoConsulta);
        Assert.Equal("Clareira Dourada", linha.ChaleNome);
        Assert.Equal("Hóspede Teste", linha.NomeHospede);
        Assert.Equal(criada.ValorTotal, linha.ValorTotal);
        Assert.Equal(1, relatorio.Resumo.QuantidadeTotal);
        Assert.Equal(criada.ValorTotal, relatorio.Resumo.TotalValores);
    }

    /// <summary>
    /// AC #1: atribuição pelo mês de DataCheckin, mesmo pra um mês passado. `POST /api/reservas`
    /// nunca aceita `DataCheckin` no passado — cria com checkin hoje e retroage via SQL depois
    /// (mesma técnica de `VisaoDiariaEndpointTests`, Story 3.2).
    /// </summary>
    [Fact]
    public async Task GetRelatorioMensal_ComReservaAtribuidaAUmMesPassado_ApareceSoNoMesDoCheckin()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await ObterTokenValidoAsync(client));
        var criada = await CriarReservaAsync(client);
        var hoje = DataRelativaAHoje(0);
        var mesPassado = new DateOnly(hoje.Year, hoje.Month, 1).AddMonths(-2);

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ChaleOnlineDbContext>();
            var novoCheckin = mesPassado;
            var novoCheckout = mesPassado.AddDays(3);
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE Reserva SET DataCheckin = {novoCheckin}, DataCheckout = {novoCheckout} WHERE CodigoConsulta = {criada.CodigoConsulta}",
                TestContext.Current.CancellationToken);
        }

        var relatorioMesPassado = await ConsultarRelatorioAsync(client, mesPassado.Year, mesPassado.Month);
        var relatorioMesCorrente = await ConsultarRelatorioAsync(client, hoje.Year, hoje.Month);

        Assert.Single(relatorioMesPassado.Reservas);
        Assert.Empty(relatorioMesCorrente.Reservas);
    }

    /// <summary>AC #2, prova real (não só no use case): Reserva cancelada aparece na lista mas não soma no total.</summary>
    [Fact]
    public async Task GetRelatorioMensal_ComReservaCanceladaNoMes_ApareceNaListaMasNaoSomaNoTotal()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await ObterTokenValidoAsync(client));
        var codigo = await CriarReservaComCheckinHojeAsync(client);
        var hoje = DataRelativaAHoje(0);

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

        var relatorio = await ConsultarRelatorioAsync(client, hoje.Year, hoje.Month);

        var linha = Assert.Single(relatorio.Reservas);
        Assert.Equal("Cancelada", linha.Status);
        Assert.Equal(1, relatorio.Resumo.QuantidadeTotal);
        Assert.Equal(1, relatorio.Resumo.QuantidadeCanceladas);
        Assert.Equal(0m, relatorio.Resumo.TotalValores);
    }
}
