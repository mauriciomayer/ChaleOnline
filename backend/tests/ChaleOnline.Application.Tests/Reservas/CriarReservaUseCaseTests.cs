using ChaleOnline.Application.Chales;
using ChaleOnline.Application.Reservas;
using ChaleOnline.Domain;

namespace ChaleOnline.Application.Tests.Reservas;

public class CriarReservaUseCaseTests
{
    private sealed class ChaleRepositoryFalso(Chale? chale) : IChaleRepository
    {
        public Task<IReadOnlyList<Chale>> ListarTodosAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Chale>>([]);

        public Task<IReadOnlyList<Chale>> BuscarDisponiveisAsync(
            DateOnly dataCheckin,
            DateOnly dataCheckout,
            IReadOnlyList<TipoChale> tipos,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Chale>>([]);

        public Task<Chale?> BuscarPorIdAsync(int id, CancellationToken cancellationToken = default)
            => Task.FromResult(chale);

        public Task<IReadOnlyList<ChaleMidia>> ListarMidiasAsync(int chaleId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<ChaleMidia>>([]);

        public Task<IReadOnlyList<ChaleComodidade>> ListarComodidadesAsync(int chaleId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<ChaleComodidade>>([]);

        public Task<IReadOnlyList<Avaliacao>> ListarAvaliacoesAsync(int chaleId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Avaliacao>>([]);
    }

    private sealed class ReservaRepositoryFalso(bool existeConflito) : IReservaRepository
    {
        public bool CriarAsyncChamado { get; private set; }

        public Task<bool> ExisteConflitoAsync(int chaleId, DateOnly dataCheckin, DateOnly dataCheckout, CancellationToken cancellationToken = default)
            => Task.FromResult(existeConflito);

        public Task CriarAsync(Reserva reserva, CancellationToken cancellationToken = default)
        {
            CriarAsyncChamado = true;
            return Task.CompletedTask;
        }

        public Task<Reserva?> BuscarPorCodigoConsultaAsync(Guid codigoConsulta, CancellationToken cancellationToken = default)
            => throw new NotSupportedException("Não usado pelos testes de CriarReservaUseCase.");

        public Task<bool> ConfirmarPagamentoAsync(int reservaId, CancellationToken cancellationToken = default)
            => throw new NotSupportedException("Não usado pelos testes de CriarReservaUseCase.");

        public Task<IReadOnlyList<Reserva>> BuscarExpiradasAsync(DateTime limiteUtc, CancellationToken cancellationToken = default)
            => throw new NotSupportedException("Não usado pelos testes de CriarReservaUseCase.");

        public Task<bool> CancelarPorExpiracaoAsync(int reservaId, CancellationToken cancellationToken = default)
            => throw new NotSupportedException("Não usado pelos testes de CriarReservaUseCase.");

        public Task<IReadOnlyList<Reserva>> BuscarRelevantesParaDataAsync(DateOnly data, CancellationToken cancellationToken = default)
            => throw new NotSupportedException("Não usado pelos testes de CriarReservaUseCase.");

        public Task<IReadOnlyList<Reserva>> BuscarPorMesCheckinAsync(DateOnly primeiroDiaDoMes, DateOnly primeiroDiaDoProximoMes, CancellationToken cancellationToken = default)
            => throw new NotSupportedException("Não usado pelos testes de CriarReservaUseCase.");
    }

    private static readonly DateOnly Checkin = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(5);

    [Fact]
    public async Task ExecutarAsync_ComDadosValidos_RetornaDtoComValorTotalCorreto()
    {
        var chale = new Chale(10, "Pinheiro Bravo", TipoChale.A, 2, 1, 420m, "/media/pinheiro-bravo.jpg");
        var useCase = new CriarReservaUseCase(new ReservaRepositoryFalso(existeConflito: false), new ChaleRepositoryFalso(chale));

        var resultado = await useCase.ExecutarAsync(10, "João Silva", "joao@example.com", Checkin, 2, TestContext.Current.CancellationToken);

        Assert.Equal(840m, resultado.ValorTotal);
        Assert.Equal(Checkin, resultado.DataCheckin);
        Assert.Equal(Checkin.AddDays(2), resultado.DataCheckout);
        Assert.Equal("AguardandoPagamento", resultado.Status);
    }

    [Fact]
    public async Task ExecutarAsync_ComChaleInexistente_LancaChaleNaoEncontradoException()
    {
        var useCase = new CriarReservaUseCase(new ReservaRepositoryFalso(existeConflito: false), new ChaleRepositoryFalso(null));

        await Assert.ThrowsAsync<ChaleNaoEncontradoException>(() =>
            useCase.ExecutarAsync(99999, "João Silva", "joao@example.com", Checkin, 2, TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(31)]
    public async Task ExecutarAsync_ComQuantidadeDiariasInvalida_LancaArgumentOutOfRangeException(int quantidadeInvalida)
    {
        var chale = new Chale(10, "Pinheiro Bravo", TipoChale.A, 2, 1, 420m, "/media/pinheiro-bravo.jpg");
        var useCase = new CriarReservaUseCase(new ReservaRepositoryFalso(existeConflito: false), new ChaleRepositoryFalso(chale));

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            useCase.ExecutarAsync(10, "João Silva", "joao@example.com", Checkin, quantidadeInvalida, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ExecutarAsync_ComConflitoDetectadoNoPreCheck_LancaReservaConflitanteExceptionSemChamarCriarAsync()
    {
        var chale = new Chale(10, "Pinheiro Bravo", TipoChale.A, 2, 1, 420m, "/media/pinheiro-bravo.jpg");
        var reservaRepositorio = new ReservaRepositoryFalso(existeConflito: true);
        var useCase = new CriarReservaUseCase(reservaRepositorio, new ChaleRepositoryFalso(chale));

        await Assert.ThrowsAsync<ReservaConflitanteException>(() =>
            useCase.ExecutarAsync(10, "João Silva", "joao@example.com", Checkin, 2, TestContext.Current.CancellationToken));

        Assert.False(reservaRepositorio.CriarAsyncChamado);
    }
}
