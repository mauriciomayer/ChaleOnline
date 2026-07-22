using ChaleOnline.Domain;

namespace ChaleOnline.Application.Reservas;

/// <summary>
/// Cálculo compartilhado de expiração (AD-4) entre ConfirmarPagamentoUseCase e
/// ConsultarReservaUseCase — expiração é calculada por tempo (CriadoEm + 48h), não só por
/// Status == Cancelada, porque o job de cancelamento (varredura recorrente) roda a cada poucos
/// minutos e pode não ter processado a Reserva ainda mesmo já passadas as 48h.
/// </summary>
public static class ReservaExpiracao
{
    public static bool EstaExpirada(Reserva reserva, DateTime agoraUtc)
        => reserva.Status == StatusReserva.Cancelada
            || (reserva.Status == StatusReserva.AguardandoPagamento && reserva.CriadoEm.AddHours(48) <= agoraUtc);
}
