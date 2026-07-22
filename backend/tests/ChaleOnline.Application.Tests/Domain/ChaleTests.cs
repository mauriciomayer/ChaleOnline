using ChaleOnline.Domain;

namespace ChaleOnline.Application.Tests.Domain;

public class ChaleTests
{
    [Fact]
    public void Construtor_ComDadosValidos_CriaChale()
    {
        var chale = new Chale("Pinheiro Bravo", TipoChale.A, 2, 1, 450m, "/media/pinheiro-bravo.jpg");

        Assert.Equal("Pinheiro Bravo", chale.Nome);
        Assert.Equal(TipoChale.A, chale.Tipo);
        Assert.Equal(2, chale.NumeroQuartos);
        Assert.Equal(1, chale.NumeroBanheiros);
        Assert.Equal(450m, chale.Preco);
        Assert.Equal("/media/pinheiro-bravo.jpg", chale.FotoUrl);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_ComNomeVazio_LancaExcecao(string nomeInvalido)
    {
        Assert.Throws<ArgumentException>(() =>
            new Chale(nomeInvalido, TipoChale.A, 2, 1, 450m, "/media/foto.jpg"));
    }

    [Fact]
    public void Construtor_ComNomeMaiorQue120Caracteres_LancaExcecao()
    {
        var nomeMuitoLongo = new string('A', 121);

        Assert.Throws<ArgumentException>(() =>
            new Chale(nomeMuitoLongo, TipoChale.A, 2, 1, 450m, "/media/foto.jpg"));
    }

    [Fact]
    public void Construtor_ComPrecoZeroOuNegativo_LancaExcecao()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Chale("Araucária", TipoChale.B, 3, 1, 0m, "/media/foto.jpg"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Construtor_ComNumeroQuartosInvalido_LancaExcecao(int numeroQuartosInvalido)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Chale("Araucária", TipoChale.B, numeroQuartosInvalido, 1, 450m, "/media/foto.jpg"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Construtor_ComNumeroBanheirosInvalido_LancaExcecao(int numeroBanheirosInvalido)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Chale("Araucária", TipoChale.B, 3, numeroBanheirosInvalido, 450m, "/media/foto.jpg"));
    }

    [Fact]
    public void Construtor_ComFotoUrlVazia_LancaExcecao()
    {
        Assert.Throws<ArgumentException>(() =>
            new Chale("Araucária", TipoChale.B, 3, 1, 450m, ""));
    }
}
