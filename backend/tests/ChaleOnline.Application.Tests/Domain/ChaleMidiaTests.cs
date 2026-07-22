using ChaleOnline.Domain;

namespace ChaleOnline.Application.Tests.Domain;

public class ChaleMidiaTests
{
    [Fact]
    public void Construtor_ComDadosValidos_CriaChaleMidia()
    {
        var midia = new ChaleMidia(1, 10, "/media/chale-10-foto-1.jpg", TipoMidia.Foto, 0);

        Assert.Equal(1, midia.Id);
        Assert.Equal(10, midia.ChaleId);
        Assert.Equal("/media/chale-10-foto-1.jpg", midia.Url);
        Assert.Equal(TipoMidia.Foto, midia.Tipo);
        Assert.Equal(0, midia.Ordem);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_ComUrlVazia_LancaExcecao(string urlInvalida)
    {
        Assert.Throws<ArgumentException>(() =>
            new ChaleMidia(1, 10, urlInvalida, TipoMidia.Foto, 0));
    }

    [Fact]
    public void Construtor_ComUrlMaiorQue255Caracteres_LancaExcecao()
    {
        var urlMuitoLonga = "/media/" + new string('a', 250) + ".jpg";

        Assert.Throws<ArgumentException>(() =>
            new ChaleMidia(1, 10, urlMuitoLonga, TipoMidia.Foto, 0));
    }
}
