using ChaleOnline.Application.Chales;
using ChaleOnline.Application.Reservas;
using ChaleOnline.Domain;

namespace ChaleOnline.Application.Tests.Reservas;

public class ObterRelatorioMensalUseCaseTests
{
    private sealed class ChaleRepositoryFalso(IReadOnlyList<Chale> chales) : IChaleRepository
    {
        public Task<IReadOnlyList<Chale>> ListarTodosAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(chales);

        public Task<IReadOnlyList<Chale>> BuscarDisponiveisAsync(
            DateOnly dataCheckin, DateOnly dataCheckout, IReadOnlyList<TipoChale> tipos, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Chale?> BuscarPorIdAsync(int id, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<IReadOnlyList<ChaleMidia>> ListarMidiasAsync(int chaleId, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<IReadOnlyList<ChaleComodidade>> ListarComodidadesAsync(int chaleId, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<IReadOnlyList<Avaliacao>> ListarAvaliacoesAsync(int chaleId, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private sealed class ReservaRepositoryFalso(IReadOnlyList<Reserva> reservas) : IReservaRepository
    {
        public Task<bool> ExisteConflitoAsync(int chaleId, DateOnly dataCheckin, DateOnly dataCheckout, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task CriarAsync(Reserva reserva, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Reserva?> BuscarPorCodigoConsultaAsync(Guid codigoConsulta, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<bool> ConfirmarPagamentoAsync(int reservaId, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<IReadOnlyList<Reserva>> BuscarExpiradasAsync(DateTime limiteUtc, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<bool> CancelarPorExpiracaoAsync(int reservaId, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<IReadOnlyList<Reserva>> BuscarRelevantesParaDataAsync(DateOnly data, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<IReadOnlyList<Reserva>> BuscarPorMesCheckinAsync(DateOnly primeiroDiaDoMes, DateOnly primeiroDiaDoProximoMes, CancellationToken cancellationToken = default)
            => Task.FromResult(reservas);
    }

    private static readonly IReadOnlyList<Chale> UmChale =
    [
        new(1, "Chalé Teste", TipoChale.A, 2, 1, 420m, "/media/a.jpg"),
    ];

    private static Reserva NovaReserva(DateOnly checkin, decimal valorTotal, StatusReserva status) => new(
        1, Guid.NewGuid(), chaleId: 1, "Fernanda Lima", "fernanda@example.com",
        checkin, checkin.AddDays(3), valorTotal, status, DateTime.UtcNow);

    [Fact]
    public async Task ExecutarAsync_ComReservasDeStatusVariados_SomaSoAsNaoCanceladasNoTotal()
    {
        var reservas = new List<Reserva>
        {
            NovaReserva(new DateOnly(2026, 7, 5), 500m, StatusReserva.Paga),
            NovaReserva(new DateOnly(2026, 7, 10), 300m, StatusReserva.AguardandoPagamento),
            NovaReserva(new DateOnly(2026, 7, 15), 900m, StatusReserva.Cancelada),
        };
        var useCase = new ObterRelatorioMensalUseCase(new ChaleRepositoryFalso(UmChale), new ReservaRepositoryFalso(reservas));

        var relatorio = await useCase.ExecutarAsync(2026, 7, TestContext.Current.CancellationToken);

        Assert.Equal(3, relatorio.Reservas.Count);
        Assert.Contains(relatorio.Reservas, r => r.Status == "Cancelada");
        Assert.Equal(3, relatorio.Resumo.QuantidadeTotal);
        Assert.Equal(1, relatorio.Resumo.QuantidadeCanceladas);
        Assert.Equal(800m, relatorio.Resumo.TotalValores); // 500 + 300, sem a cancelada
    }

    [Fact]
    public async Task ExecutarAsync_ComChaleNoResultado_PreencheNomeDoChale()
    {
        var reservas = new List<Reserva> { NovaReserva(new DateOnly(2026, 7, 5), 500m, StatusReserva.Paga) };
        var useCase = new ObterRelatorioMensalUseCase(new ChaleRepositoryFalso(UmChale), new ReservaRepositoryFalso(reservas));

        var relatorio = await useCase.ExecutarAsync(2026, 7, TestContext.Current.CancellationToken);

        Assert.Equal("Chalé Teste", relatorio.Reservas.Single().ChaleNome);
    }

    [Fact]
    public async Task ExecutarAsync_SemNenhumaReservaNoMes_RetornaListaVaziaEResumoZerado()
    {
        var useCase = new ObterRelatorioMensalUseCase(new ChaleRepositoryFalso(UmChale), new ReservaRepositoryFalso([]));

        var relatorio = await useCase.ExecutarAsync(2026, 7, TestContext.Current.CancellationToken);

        Assert.Empty(relatorio.Reservas);
        Assert.Equal(0, relatorio.Resumo.QuantidadeTotal);
        Assert.Equal(0, relatorio.Resumo.QuantidadeCanceladas);
        Assert.Equal(0m, relatorio.Resumo.TotalValores);
    }

    /// <summary>Achado de code review: ChaleId sem correspondência na lista de Chalés não deve virar uma célula muda.</summary>
    [Fact]
    public async Task ExecutarAsync_ComReservaDeChaleInexistenteNaListaAtual_UsaRotuloVisivel()
    {
        var reservas = new List<Reserva> { NovaReserva(new DateOnly(2026, 7, 5), 500m, StatusReserva.Paga) };
        var useCase = new ObterRelatorioMensalUseCase(new ChaleRepositoryFalso([]), new ReservaRepositoryFalso(reservas));

        var relatorio = await useCase.ExecutarAsync(2026, 7, TestContext.Current.CancellationToken);

        Assert.Equal("Chalé removido", relatorio.Reservas.Single().ChaleNome);
    }

    [Fact]
    public async Task ExecutarAsync_ComDuasReservasNoMesmoDia_OrdenaDeFormaDeterministicaPorCodigoConsulta()
    {
        var reservaA = NovaReserva(new DateOnly(2026, 7, 5), 500m, StatusReserva.Paga);
        var reservaB = NovaReserva(new DateOnly(2026, 7, 5), 600m, StatusReserva.Paga);
        var esperadas = new[] { reservaA, reservaB }.OrderBy(r => r.CodigoConsulta).Select(r => r.CodigoConsulta.ToString()).ToList();
        var useCase = new ObterRelatorioMensalUseCase(new ChaleRepositoryFalso(UmChale), new ReservaRepositoryFalso([reservaA, reservaB]));

        var relatorio = await useCase.ExecutarAsync(2026, 7, TestContext.Current.CancellationToken);

        Assert.Equal(esperadas, relatorio.Reservas.Select(r => r.CodigoConsulta));
    }
}
