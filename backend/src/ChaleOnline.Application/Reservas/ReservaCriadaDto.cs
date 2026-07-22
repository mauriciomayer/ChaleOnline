using ChaleOnline.Domain;

namespace ChaleOnline.Application.Reservas;

public record ReservaCriadaDto(
    Guid CodigoConsulta,
    DateOnly DataCheckin,
    DateOnly DataCheckout,
    decimal ValorTotal,
    string Status
)
{
    public static ReservaCriadaDto DeEntidade(Reserva reserva) => new(
        reserva.CodigoConsulta,
        reserva.DataCheckin,
        reserva.DataCheckout,
        reserva.ValorTotal,
        reserva.Status.ToString());
}
