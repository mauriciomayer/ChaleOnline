using ChaleOnline.Domain;

namespace ChaleOnline.Application.Chales;

public record ChaleDetalheDto(
    int Id,
    string Nome,
    string Tipo,
    int NumeroQuartos,
    int NumeroBanheiros,
    decimal Preco,
    IReadOnlyList<ChaleMidiaDto> Midias,
    IReadOnlyList<string> Comodidades,
    IReadOnlyList<AvaliacaoDto> Avaliacoes
);

public record ChaleMidiaDto(string Url, string Tipo, int Ordem)
{
    public static ChaleMidiaDto DeEntidade(ChaleMidia midia) => new(midia.Url, midia.Tipo.ToString(), midia.Ordem);
}

public record AvaliacaoDto(int Nota, string Comentario)
{
    public static AvaliacaoDto DeEntidade(Avaliacao avaliacao) => new(avaliacao.Nota, avaliacao.Comentario);
}
