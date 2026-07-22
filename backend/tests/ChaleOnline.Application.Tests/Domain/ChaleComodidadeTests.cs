using ChaleOnline.Domain;

namespace ChaleOnline.Application.Tests.Domain;

public class ChaleComodidadeTests
{
    [Fact]
    public void Construtor_ComDadosValidos_CriaChaleComodidade()
    {
        var comodidade = new ChaleComodidade(1, 10, "Lareira");

        Assert.Equal(1, comodidade.Id);
        Assert.Equal(10, comodidade.ChaleId);
        Assert.Equal("Lareira", comodidade.Nome);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_ComNomeVazio_LancaExcecao(string nomeInvalido)
    {
        Assert.Throws<ArgumentException>(() =>
            new ChaleComodidade(1, 10, nomeInvalido));
    }

    [Fact]
    public void Construtor_ComNomeMaiorQue80Caracteres_LancaExcecao()
    {
        var nomeMuitoLongo = new string('A', 81);

        Assert.Throws<ArgumentException>(() =>
            new ChaleComodidade(1, 10, nomeMuitoLongo));
    }
}
