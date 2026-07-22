using ChaleOnline.Application.Chales;
using ChaleOnline.Domain;

namespace ChaleOnline.Application.Tests.Chales;

public class ListarChalesUseCaseTests
{
    private sealed class ChaleRepositoryFalso(IReadOnlyList<Chale> chales) : IChaleRepository
    {
        public Task<IReadOnlyList<Chale>> ListarTodosAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(chales);

        public Task<IReadOnlyList<Chale>> BuscarDisponiveisAsync(
            DateOnly dataCheckin,
            DateOnly dataCheckout,
            IReadOnlyList<TipoChale> tipos,
            CancellationToken cancellationToken = default)
            => Task.FromResult(chales);

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
    public async Task ExecutarAsync_RetornaTodosOsChalesMapeadosParaDto()
    {
        var chales = new List<Chale>
        {
            new("Pinheiro Bravo", TipoChale.A, 2, 1, 450m, "/media/pinheiro-bravo.jpg"),
            new("Araucária Alta", TipoChale.B, 3, 1, 620m, "/media/araucaria-alta.jpg"),
            new("Vista da Serra", TipoChale.C, 4, 2, 890m, "/media/vista-da-serra.jpg"),
        };
        var useCase = new ListarChalesUseCase(new ChaleRepositoryFalso(chales));

        var resultado = await useCase.ExecutarAsync(TestContext.Current.CancellationToken);

        Assert.Equal(3, resultado.Count);
        Assert.Contains(resultado, dto => dto.Nome == "Pinheiro Bravo" && dto.Tipo == "A" && dto.NumeroQuartos == 2 && dto.NumeroBanheiros == 1);
        Assert.Contains(resultado, dto => dto.Nome == "Vista da Serra" && dto.Tipo == "C" && dto.Preco == 890m);
    }

    [Fact]
    public async Task ExecutarAsync_SemChalesCadastrados_RetornaListaVazia()
    {
        var useCase = new ListarChalesUseCase(new ChaleRepositoryFalso([]));

        var resultado = await useCase.ExecutarAsync(TestContext.Current.CancellationToken);

        Assert.Empty(resultado);
    }
}
