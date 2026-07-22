using ChaleOnline.Domain;

namespace ChaleOnline.Application.Tests.Domain;

public class AvaliacaoTests
{
    [Fact]
    public void Construtor_ComDadosValidos_CriaAvaliacao()
    {
        var avaliacao = new Avaliacao(1, 10, 5, "Chalé maravilhoso, recomendo muito!");

        Assert.Equal(1, avaliacao.Id);
        Assert.Equal(10, avaliacao.ChaleId);
        Assert.Equal(5, avaliacao.Nota);
        Assert.Equal("Chalé maravilhoso, recomendo muito!", avaliacao.Comentario);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public void Construtor_ComNotaForaDoRange_LancaExcecao(int notaInvalida)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Avaliacao(1, 10, notaInvalida, "Comentário válido."));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_ComComentarioVazio_LancaExcecao(string comentarioInvalido)
    {
        Assert.Throws<ArgumentException>(() =>
            new Avaliacao(1, 10, 5, comentarioInvalido));
    }

    [Fact]
    public void Construtor_ComComentarioMaiorQue500Caracteres_LancaExcecao()
    {
        var comentarioMuitoLongo = new string('A', 501);

        Assert.Throws<ArgumentException>(() =>
            new Avaliacao(1, 10, 5, comentarioMuitoLongo));
    }
}
