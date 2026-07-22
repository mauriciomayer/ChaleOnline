using ChaleOnline.Application.Chales;
using ChaleOnline.Domain;

namespace ChaleOnline.Application.Tests.Chales;

public class ObterChaleDetalheUseCaseTests
{
    private sealed class ChaleRepositoryFalso(
        Chale? chale,
        IReadOnlyList<ChaleMidia> midias,
        IReadOnlyList<ChaleComodidade> comodidades,
        IReadOnlyList<Avaliacao> avaliacoes) : IChaleRepository
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
            => Task.FromResult(midias);

        public Task<IReadOnlyList<ChaleComodidade>> ListarComodidadesAsync(int chaleId, CancellationToken cancellationToken = default)
            => Task.FromResult(comodidades);

        public Task<IReadOnlyList<Avaliacao>> ListarAvaliacoesAsync(int chaleId, CancellationToken cancellationToken = default)
            => Task.FromResult(avaliacoes);
    }

    [Fact]
    public async Task ExecutarAsync_ComIdEncontrado_RetornaDtoCompleto()
    {
        var chale = new Chale(10, "Pinheiro Bravo", TipoChale.A, 2, 1, 450m, "/media/pinheiro-bravo.jpg");
        var midias = new List<ChaleMidia> { new(1, 10, "/media/foto-1.jpg", TipoMidia.Foto, 0) };
        var comodidades = new List<ChaleComodidade> { new(1, 10, "Lareira") };
        var avaliacoes = new List<Avaliacao> { new(1, 10, 5, "Excelente estadia!") };
        var useCase = new ObterChaleDetalheUseCase(new ChaleRepositoryFalso(chale, midias, comodidades, avaliacoes));

        var resultado = await useCase.ExecutarAsync(10, TestContext.Current.CancellationToken);

        Assert.NotNull(resultado);
        Assert.Equal(10, resultado.Id);
        Assert.Equal("Pinheiro Bravo", resultado.Nome);
        Assert.Equal("A", resultado.Tipo);
        Assert.Single(resultado.Midias);
        Assert.Equal("/media/foto-1.jpg", resultado.Midias[0].Url);
        Assert.Single(resultado.Comodidades);
        Assert.Equal("Lareira", resultado.Comodidades[0]);
        Assert.Single(resultado.Avaliacoes);
        Assert.Equal(5, resultado.Avaliacoes[0].Nota);
    }

    [Fact]
    public async Task ExecutarAsync_ComIdNaoEncontrado_RetornaNull()
    {
        var useCase = new ObterChaleDetalheUseCase(new ChaleRepositoryFalso(null, [], [], []));

        var resultado = await useCase.ExecutarAsync(99999, TestContext.Current.CancellationToken);

        Assert.Null(resultado);
    }
}
