using ChaleOnline.Application.Chales;
using ChaleOnline.Application.Reservas;
using ChaleOnline.Domain;

namespace ChaleOnline.Application.Tests.Reservas;

public class ObterVisaoDiariaUseCaseTests
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
            => Task.FromResult(reservas);

        public Task<IReadOnlyList<Reserva>> BuscarPorMesCheckinAsync(DateOnly primeiroDiaDoMes, DateOnly primeiroDiaDoProximoMes, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private static readonly IReadOnlyList<Chale> TresChales =
    [
        new(1, "Chalé Desocupado", TipoChale.A, 2, 1, 420m, "/media/a.jpg"),
        new(2, "Chalé Ocupado", TipoChale.B, 3, 1, 520m, "/media/b.jpg"),
        new(3, "Chalé Check-in", TipoChale.C, 4, 2, 620m, "/media/c.jpg"),
    ];

    private static Reserva NovaReserva(int chaleId, DateOnly checkin, DateOnly checkout) => new(
        1, Guid.NewGuid(), chaleId, "Fernanda Lima", "fernanda@example.com",
        checkin, checkout, valorTotal: 840m, StatusReserva.Paga, DateTime.UtcNow);

    [Fact]
    public async Task ExecutarAsync_ComTresChalesEmEstadosDiferentes_ClassificaCadaUmCorretamente()
    {
        var hoje = HorarioBrasil.DiaCorrente(DateTime.UtcNow);
        var reservas = new List<Reserva>
        {
            NovaReserva(chaleId: 2, hoje.AddDays(-2), hoje.AddDays(3)), // ocupado
            NovaReserva(chaleId: 3, hoje, hoje.AddDays(3)), // check-in hoje
        };
        var useCase = new ObterVisaoDiariaUseCase(new ChaleRepositoryFalso(TresChales), new ReservaRepositoryFalso(reservas));

        var visao = await useCase.ExecutarAsync(TestContext.Current.CancellationToken);

        Assert.Equal(3, visao.Count);
        Assert.Equal("Desocupado", visao.Single(v => v.ChaleId == 1).Estado);
        Assert.Equal("Ocupado", visao.Single(v => v.ChaleId == 2).Estado);
        Assert.Equal("CheckInHoje", visao.Single(v => v.ChaleId == 3).Estado);
        Assert.Null(visao.Single(v => v.ChaleId == 1).Detalhe);
        Assert.Contains("Fernanda Lima", visao.Single(v => v.ChaleId == 2).Detalhe);
    }

    [Fact]
    public async Task ExecutarAsync_SemNenhumaReserva_TodosOsChalesFicamDesocupados()
    {
        var useCase = new ObterVisaoDiariaUseCase(new ChaleRepositoryFalso(TresChales), new ReservaRepositoryFalso([]));

        var visao = await useCase.ExecutarAsync(TestContext.Current.CancellationToken);

        Assert.All(visao, dto => Assert.Equal("Desocupado", dto.Estado));
    }
}
