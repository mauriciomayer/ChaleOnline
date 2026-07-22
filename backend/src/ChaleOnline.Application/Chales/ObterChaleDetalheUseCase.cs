namespace ChaleOnline.Application.Chales;

public class ObterChaleDetalheUseCase(IChaleRepository chaleRepository)
{
    public async Task<ChaleDetalheDto?> ExecutarAsync(int id, CancellationToken cancellationToken = default)
    {
        var chale = await chaleRepository.BuscarPorIdAsync(id, cancellationToken);

        if (chale is null)
        {
            return null;
        }

        var midias = await chaleRepository.ListarMidiasAsync(id, cancellationToken);
        var comodidades = await chaleRepository.ListarComodidadesAsync(id, cancellationToken);
        var avaliacoes = await chaleRepository.ListarAvaliacoesAsync(id, cancellationToken);

        return new ChaleDetalheDto(
            chale.Id,
            chale.Nome,
            chale.Tipo.ToString(),
            chale.NumeroQuartos,
            chale.NumeroBanheiros,
            chale.Preco,
            midias.Select(ChaleMidiaDto.DeEntidade).ToList(),
            comodidades.Select(comodidade => comodidade.Nome).ToList(),
            avaliacoes.Select(AvaliacaoDto.DeEntidade).ToList());
    }
}
