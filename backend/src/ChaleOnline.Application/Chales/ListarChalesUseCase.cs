namespace ChaleOnline.Application.Chales;

public class ListarChalesUseCase(IChaleRepository chaleRepository)
{
    public async Task<IReadOnlyList<ChaleResumoDto>> ExecutarAsync(CancellationToken cancellationToken = default)
    {
        var chales = await chaleRepository.ListarTodosAsync(cancellationToken);

        return chales.Select(ChaleResumoDto.DeEntidade).ToList();
    }
}
