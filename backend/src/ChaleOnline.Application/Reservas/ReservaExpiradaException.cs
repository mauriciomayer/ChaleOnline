namespace ChaleOnline.Application.Reservas;

/// <summary>
/// A Reserva já passou das 48h desde CriadoEm sem pagamento (Status=Cancelada, ou calculado por
/// tempo mesmo que o job de cancelamento ainda não tenha rodado — AD-4) — não há mais o que pagar.
/// </summary>
public class ReservaExpiradaException : Exception;
