using ChaleOnline.Application.Chales;

namespace ChaleOnline.Application.Reservas;

public class ConsultarReservaUseCase(IReservaRepository reservaRepository, IChaleRepository chaleRepository)
{
    public async Task<ReservaConsultaDto?> ExecutarAsync(Guid codigoConsulta, CancellationToken cancellationToken = default)
    {
        var reserva = await reservaRepository.BuscarPorCodigoConsultaAsync(codigoConsulta, cancellationToken);
        if (reserva is null)
        {
            return null;
        }

        var chale = await chaleRepository.BuscarPorIdAsync(reserva.ChaleId, cancellationToken);

        return new ReservaConsultaDto(
            reserva.CodigoConsulta,
            reserva.Status.ToString(),
            ReservaExpiracao.EstaExpirada(reserva, DateTime.UtcNow),
            chale?.Nome ?? string.Empty,
            reserva.DataCheckin,
            reserva.DataCheckout,
            reserva.ValorTotal);
    }
}
