using ChaleOnline.Domain;

namespace ChaleOnline.Application.Chales;

public record ChaleResumoDto(
    int Id,
    string Nome,
    string Tipo,
    int NumeroQuartos,
    int NumeroBanheiros,
    decimal Preco,
    string FotoUrl
)
{
    public static ChaleResumoDto DeEntidade(Chale chale) => new(
        chale.Id,
        chale.Nome,
        chale.Tipo.ToString(),
        chale.NumeroQuartos,
        chale.NumeroBanheiros,
        chale.Preco,
        chale.FotoUrl);
}
