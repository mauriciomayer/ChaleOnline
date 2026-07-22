using ChaleOnline.Application.Chales;
using ChaleOnline.Domain;

namespace ChaleOnline.Application.Tests.Chales;

public class BuscarChalesDisponiveisUseCaseTests
{
    private sealed class ChaleRepositoryFalso(IReadOnlyList<Chale> chalesDisponiveis) : IChaleRepository
    {
        public DateOnly? CheckinRecebido { get; private set; }
        public DateOnly? CheckoutRecebido { get; private set; }
        public IReadOnlyList<TipoChale>? TiposRecebidos { get; private set; }

        public Task<IReadOnlyList<Chale>> ListarTodosAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Chale>>([]);

        public Task<IReadOnlyList<Chale>> BuscarDisponiveisAsync(
            DateOnly dataCheckin,
            DateOnly dataCheckout,
            IReadOnlyList<TipoChale> tipos,
            CancellationToken cancellationToken = default)
        {
            CheckinRecebido = dataCheckin;
            CheckoutRecebido = dataCheckout;
            TiposRecebidos = tipos;
            return Task.FromResult(chalesDisponiveis);
        }

        public Task<Chale?> BuscarPorIdAsync(int id, CancellationToken cancellationToken = default)
            => Task.FromResult<Chale?>(null);

        public Task<IReadOnlyList<ChaleMidia>> ListarMidiasAsync(int chaleId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<ChaleMidia>>([]);

        public Task<IReadOnlyList<ChaleComodidade>> ListarComodidadesAsync(int chaleId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<ChaleComodidade>>([]);

        public Task<IReadOnlyList<Avaliacao>> ListarAvaliacoesAsync(int chaleId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Avaliacao>>([]);
    }

    [Fact]
    public async Task ExecutarAsync_ComCheckoutAposCheckin_RetornaChalesMapeados()
    {
        var chales = new List<Chale>
        {
            new("Pinheiro Bravo", TipoChale.A, 2, 1, 420m, "/media/pinheiro-bravo.jpg"),
        };
        var repositorio = new ChaleRepositoryFalso(chales);
        var useCase = new BuscarChalesDisponiveisUseCase(repositorio);
        var checkin = new DateOnly(2026, 8, 10);
        var checkout = new DateOnly(2026, 8, 12);

        var resultado = await useCase.ExecutarAsync(checkin, checkout, [], TestContext.Current.CancellationToken);

        Assert.Single(resultado);
        Assert.Equal("Pinheiro Bravo", resultado[0].Nome);
        Assert.Equal(checkin, repositorio.CheckinRecebido);
        Assert.Equal(checkout, repositorio.CheckoutRecebido);
    }

    [Fact]
    public async Task ExecutarAsync_ComTiposInformados_RepassaOsTiposAoRepositorio()
    {
        var repositorio = new ChaleRepositoryFalso([]);
        var useCase = new BuscarChalesDisponiveisUseCase(repositorio);
        var tipos = new List<TipoChale> { TipoChale.A, TipoChale.B };

        await useCase.ExecutarAsync(
            new DateOnly(2026, 8, 10),
            new DateOnly(2026, 8, 12),
            tipos,
            TestContext.Current.CancellationToken);

        Assert.Equal(tipos, repositorio.TiposRecebidos);
    }

    [Fact]
    public async Task ExecutarAsync_ComCheckoutIgualCheckin_LancaExcecao()
    {
        var useCase = new BuscarChalesDisponiveisUseCase(new ChaleRepositoryFalso([]));
        var data = new DateOnly(2026, 8, 10);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            useCase.ExecutarAsync(data, data, [], TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ExecutarAsync_ComCheckoutAntesDoCheckin_LancaExcecao()
    {
        var useCase = new BuscarChalesDisponiveisUseCase(new ChaleRepositoryFalso([]));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            useCase.ExecutarAsync(
                new DateOnly(2026, 8, 12),
                new DateOnly(2026, 8, 10),
                [],
                TestContext.Current.CancellationToken));
    }
}
