using System.Net;
using System.Net.Http.Json;
using ChaleOnline.Application.Reservas;
using ChaleOnline.Domain;
using ChaleOnline.Infrastructure;
using ChaleOnline.Infrastructure.Jobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChaleOnline.Api.IntegrationTests.Reservas;

/// <summary>
/// Mesma collection "ReservaDbTests" — toca Reserva/ReservaNoite (ver CriarReservaEndpointTests).
/// O job é invocado diretamente via DI (sem esperar o agendamento real do Hangfire) — é só um
/// método comum, testável sem envolver o scheduler.
/// </summary>
[Collection("ReservaDbTests")]
public class CancelamentoAutomaticoJobTests(ChalesApiFactory factory) : IClassFixture<ChalesApiFactory>, IAsyncLifetime
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

    private async Task<Guid> CriarReservaAsync(HttpClient client, int diasAPartirDeHoje, int quantidadeDiarias = 2)
    {
        var resposta = await client.PostAsJsonAsync(
            "/api/reservas",
            new CriarReservaRequest(ChaleValido, "Hóspede Teste", "hospede@example.com", DataFutura(diasAPartirDeHoje), quantidadeDiarias),
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

    private async Task<Reserva> BuscarReservaAsync(Guid codigo)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ChaleOnlineDbContext>();
        return await db.Reservas.SingleAsync(r => r.CodigoConsulta == codigo, TestContext.Current.CancellationToken);
    }

    private async Task<int> ContarReservaNoitesAsync(Guid codigo)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ChaleOnlineDbContext>();
        var reservaId = await db.Reservas.Where(r => r.CodigoConsulta == codigo).Select(r => r.Id).SingleAsync(TestContext.Current.CancellationToken);
        return await db.ReservaNoites.CountAsync(n => n.ReservaId == reservaId, TestContext.Current.CancellationToken);
    }

    private async Task ExecutarJobAsync()
    {
        using var scope = factory.Services.CreateScope();
        var job = scope.ServiceProvider.GetRequiredService<CancelarReservasExpiradasJob>();
        await job.ExecutarAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ExecutarJob_ComReservaExpirada_CancelaRemoveReservaNoiteELiberaDataPraNovaReserva()
    {
        using var client = factory.CreateClient();
        var checkin = DataFutura(30);
        var codigo = await CriarReservaAsync(client, 30);
        await BackdatearCriadoEmAsync(codigo, TimeSpan.FromHours(49));
        Assert.Equal(2, await ContarReservaNoitesAsync(codigo));

        await ExecutarJobAsync();

        var reserva = await BuscarReservaAsync(codigo);
        Assert.Equal(StatusReserva.Cancelada, reserva.Status);
        Assert.Equal(0, await ContarReservaNoitesAsync(codigo));

        // Prova de que o gap documentado desde a Story 1.2 foi resolvido: uma nova Reserva pra
        // mesma data/Chalé agora funciona em vez de colidir com ReservaNoite órfãs.
        var respostaNovaReserva = await client.PostAsJsonAsync(
            "/api/reservas",
            new CriarReservaRequest(ChaleValido, "Outro Hóspede", "outro@example.com", checkin, 2),
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Created, respostaNovaReserva.StatusCode);
    }

    [Fact]
    public async Task ExecutarJob_ComReservaDentroDaJanelaDe48h_NaoCancela()
    {
        using var client = factory.CreateClient();
        var codigo = await CriarReservaAsync(client, 31);

        await ExecutarJobAsync();

        var reserva = await BuscarReservaAsync(codigo);
        Assert.Equal(StatusReserva.AguardandoPagamento, reserva.Status);
        Assert.Equal(2, await ContarReservaNoitesAsync(codigo));
    }

    /// <summary>
    /// Complementa (não substitui) o teste abaixo: prova que, do ponto de vista do hóspede, uma
    /// Reserva já expirada por tempo nunca é paga com sucesso mesmo que o pagamento e a varredura
    /// de cancelamento sejam disparados ao mesmo tempo — o guard de aplicação (`EstaExpirada`) em
    /// `ConfirmarPagamentoUseCase` já rejeita antes de chegar no `UPDATE` condicional. **Não** é o
    /// teste que prova a corrida no banco (ver `ConfirmarPagamentoAsync_ECancelarPorExpiracaoAsync...`
    /// logo abaixo, que exercita isso de verdade) — Review Finding do code review confirmou que uma
    /// Reserva retrocedida >48h faz `EstaExpirada` barrar o pagamento por tempo antes do
    /// `ExecuteUpdateAsync` condicional ser sequer alcançado, então o branch `Status == Paga` deste
    /// teste é inatingível por construção (mantido documentado, não removido, pra deixar isso claro).
    /// </summary>
    [Fact]
    public async Task PagamentoECancelamento_DisparadosSimultaneamenteParaMesmaReservaExpirada_PagamentoSempreRejeitadoPorTempo()
    {
        using var client = factory.CreateClient();
        var codigo = await CriarReservaAsync(client, 32);
        await BackdatearCriadoEmAsync(codigo, TimeSpan.FromHours(49));

        var tarefaPagamento = client.PostAsJsonAsync(
            $"/api/reservas/{codigo}/pagamento",
            new ConfirmarPagamentoRequest("CartaoCredito"),
            TestContext.Current.CancellationToken);
        var tarefaCancelamento = ExecutarJobAsync();

        await Task.WhenAll(tarefaPagamento, tarefaCancelamento);
        var respostaPagamento = await tarefaPagamento;

        // O guard de tempo em ConfirmarPagamentoUseCase.EstaExpirada roda antes do UPDATE condicional
        // — pra uma Reserva já expirada por tempo, o pagamento é sempre rejeitado (410), nunca chega
        // a disputar a corrida no banco. Ver o teste seguinte pra a prova real da corrida no banco.
        Assert.Equal(HttpStatusCode.Gone, respostaPagamento.StatusCode);
        var reservaFinal = await BuscarReservaAsync(codigo);
        Assert.Equal(StatusReserva.Cancelada, reservaFinal.Status);
        Assert.Equal(0, await ContarReservaNoitesAsync(codigo));
    }

    /// <summary>
    /// AC #3, mandatório: corrida real (não mock) no próprio banco entre a atualização condicional
    /// de pagamento e a de cancelamento por expiração, pra uma Reserva que NÃO está expirada por
    /// tempo (contorna deliberadamente os guards de aplicação de ConfirmarPagamentoUseCase — chama
    /// IReservaRepository diretamente, cada chamada com seu próprio scope/DbContext, simulando duas
    /// requisições HTTP concorrentes de verdade) — prova que o `WHERE Status=@esperado` do
    /// `ExecuteUpdateAsync` (AD-4) é o que realmente decide "só um vence", não os guards de tempo.
    /// </summary>
    [Fact]
    public async Task ConfirmarPagamentoAsync_ECancelarPorExpiracaoAsync_DisparadosSimultaneamenteParaMesmaReservaNaoExpirada_ExatamenteUmVence()
    {
        using var client = factory.CreateClient();
        var codigo = await CriarReservaAsync(client, 33);
        var reservaId = (await BuscarReservaAsync(codigo)).Id;

        // Cada tarefa usa seu próprio scope (logo, seu próprio DbContext) — DbContext não é
        // thread-safe pra operações concorrentes na mesma instância; dois scopes é o que de fato
        // simula duas requisições HTTP independentes disputando a mesma Reserva.
        using var scopePagamento = factory.Services.CreateScope();
        using var scopeCancelamento = factory.Services.CreateScope();
        var repoPagamento = scopePagamento.ServiceProvider.GetRequiredService<IReservaRepository>();
        var repoCancelamento = scopeCancelamento.ServiceProvider.GetRequiredService<IReservaRepository>();

        var tarefaPagamento = repoPagamento.ConfirmarPagamentoAsync(reservaId, TestContext.Current.CancellationToken);
        var tarefaCancelamento = repoCancelamento.CancelarPorExpiracaoAsync(reservaId, TestContext.Current.CancellationToken);

        await Task.WhenAll(tarefaPagamento, tarefaCancelamento);
        var pagamentoVenceu = await tarefaPagamento;
        var cancelamentoVenceu = await tarefaCancelamento;

        Assert.True(pagamentoVenceu ^ cancelamentoVenceu, "Exatamente uma das duas atualizações condicionais deveria ter afetado a linha.");

        var reservaFinal = await BuscarReservaAsync(codigo);
        var noitesRestantes = await ContarReservaNoitesAsync(codigo);

        if (pagamentoVenceu)
        {
            Assert.Equal(StatusReserva.Paga, reservaFinal.Status);
            Assert.Equal(2, noitesRestantes);
        }
        else
        {
            Assert.Equal(StatusReserva.Cancelada, reservaFinal.Status);
            Assert.Equal(0, noitesRestantes);
        }
    }
}
