using ChaleOnline.Application.Chales;
using ChaleOnline.Application.Reservas;
using ChaleOnline.Domain;

namespace ChaleOnline.Application.Tests.Reservas;

public class ConsultarReservaUseCaseTests
{
    private sealed class ChaleRepositoryFalso(Chale chale) : IChaleRepository
    {
        public Task<IReadOnlyList<Chale>> ListarTodosAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Chale>>([]);

        public Task<IReadOnlyList<Chale>> BuscarDisponiveisAsync(
            DateOnly dataCheckin, DateOnly dataCheckout, IReadOnlyList<TipoChale> tipos, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Chale>>([]);

        public Task<Chale?> BuscarPorIdAsync(int id, CancellationToken cancellationToken = default)
            => Task.FromResult<Chale?>(chale);

        public Task<IReadOnlyList<ChaleMidia>> ListarMidiasAsync(int chaleId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<ChaleMidia>>([]);

        public Task<IReadOnlyList<ChaleComodidade>> ListarComodidadesAsync(int chaleId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<ChaleComodidade>>([]);

        public Task<IReadOnlyList<Avaliacao>> ListarAvaliacoesAsync(int chaleId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Avaliacao>>([]);
    }

    private sealed class ReservaRepositoryFalso(Reserva? reserva) : IReservaRepository
    {
        public Task<bool> ExisteConflitoAsync(int chaleId, DateOnly dataCheckin, DateOnly dataCheckout, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task CriarAsync(Reserva reserva, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Reserva?> BuscarPorCodigoConsultaAsync(Guid codigoConsulta, CancellationToken cancellationToken = default)
            => Task.FromResult(reserva is not null && reserva.CodigoConsulta == codigoConsulta ? reserva : null);

        public Task<bool> ConfirmarPagamentoAsync(int reservaId, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<IReadOnlyList<Reserva>> BuscarExpiradasAsync(DateTime limiteUtc, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<bool> CancelarPorExpiracaoAsync(int reservaId, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<IReadOnlyList<Reserva>> BuscarRelevantesParaDataAsync(DateOnly data, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<IReadOnlyList<Reserva>> BuscarPorMesCheckinAsync(DateOnly primeiroDiaDoMes, DateOnly primeiroDiaDoProximoMes, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private static readonly Chale ChaleTeste = new(10, "Pinheiro Bravo", TipoChale.A, 2, 1, 420m, "/media/pinheiro-bravo.jpg");

    private static Reserva NovaReserva(StatusReserva status, DateTime criadoEm) => new(
        1, Guid.NewGuid(), chaleId: 10, "Hóspede Teste", "hospede@example.com",
        DateOnly.FromDateTime(DateTime.UtcNow).AddDays(5), DateOnly.FromDateTime(DateTime.UtcNow).AddDays(7),
        valorTotal: 840m, status, criadoEm);

    [Fact]
    public async Task ExecutarAsync_ComReservaRecemCriada_RetornaExpiradaFalse()
    {
        var reserva = NovaReserva(StatusReserva.AguardandoPagamento, DateTime.UtcNow.AddHours(-1));
        var useCase = new ConsultarReservaUseCase(new ReservaRepositoryFalso(reserva), new ChaleRepositoryFalso(ChaleTeste));

        var dto = await useCase.ExecutarAsync(reserva.CodigoConsulta, TestContext.Current.CancellationToken);

        Assert.NotNull(dto);
        Assert.False(dto.Expirada);
        Assert.Equal("AguardandoPagamento", dto.Status);
        Assert.Equal("Pinheiro Bravo", dto.NomeChale);
        Assert.Equal(840m, dto.ValorTotal);
    }

    [Fact]
    public async Task ExecutarAsync_ComTempoPassadoAindaAguardandoPagamento_RetornaExpiradaTrue()
    {
        var reserva = NovaReserva(StatusReserva.AguardandoPagamento, DateTime.UtcNow.AddHours(-49));
        var useCase = new ConsultarReservaUseCase(new ReservaRepositoryFalso(reserva), new ChaleRepositoryFalso(ChaleTeste));

        var dto = await useCase.ExecutarAsync(reserva.CodigoConsulta, TestContext.Current.CancellationToken);

        Assert.NotNull(dto);
        Assert.True(dto.Expirada);
    }

    [Fact]
    public async Task ExecutarAsync_ComReservaCancelada_RetornaExpiradaTrue()
    {
        var reserva = NovaReserva(StatusReserva.Cancelada, DateTime.UtcNow.AddHours(-1));
        var useCase = new ConsultarReservaUseCase(new ReservaRepositoryFalso(reserva), new ChaleRepositoryFalso(ChaleTeste));

        var dto = await useCase.ExecutarAsync(reserva.CodigoConsulta, TestContext.Current.CancellationToken);

        Assert.NotNull(dto);
        Assert.True(dto.Expirada);
    }

    [Fact]
    public async Task ExecutarAsync_ComReservaPagaMesmoComCriadoEmAntigo_RetornaExpiradaFalse()
    {
        var reserva = NovaReserva(StatusReserva.Paga, DateTime.UtcNow.AddHours(-1000));
        var useCase = new ConsultarReservaUseCase(new ReservaRepositoryFalso(reserva), new ChaleRepositoryFalso(ChaleTeste));

        var dto = await useCase.ExecutarAsync(reserva.CodigoConsulta, TestContext.Current.CancellationToken);

        Assert.NotNull(dto);
        Assert.False(dto.Expirada);
    }

    [Fact]
    public async Task ExecutarAsync_ComCodigoInexistente_RetornaNull()
    {
        var useCase = new ConsultarReservaUseCase(new ReservaRepositoryFalso(null), new ChaleRepositoryFalso(ChaleTeste));

        var dto = await useCase.ExecutarAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        Assert.Null(dto);
    }
}
