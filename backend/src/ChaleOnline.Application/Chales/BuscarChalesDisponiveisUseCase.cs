using ChaleOnline.Domain;

namespace ChaleOnline.Application.Chales;

public class BuscarChalesDisponiveisUseCase(IChaleRepository chaleRepository)
{
    public async Task<IReadOnlyList<ChaleResumoDto>> ExecutarAsync(
        DateOnly dataCheckin,
        DateOnly dataCheckout,
        IReadOnlyList<TipoChale> tipos,
        CancellationToken cancellationToken = default)
    {
        if (dataCheckout <= dataCheckin)
        {
            throw new ArgumentException(
                "Data de checkout deve ser posterior à data de checkin.",
                nameof(dataCheckout));
        }

        var chales = await chaleRepository.BuscarDisponiveisAsync(dataCheckin, dataCheckout, tipos, cancellationToken);

        return chales.Select(ChaleResumoDto.DeEntidade).ToList();
    }
}
