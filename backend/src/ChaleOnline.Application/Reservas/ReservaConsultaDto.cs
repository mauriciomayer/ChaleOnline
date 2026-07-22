namespace ChaleOnline.Application.Reservas;

/// <summary>
/// Escopo deliberadamente mínimo pra Story 1.6 — só o suficiente pra tela de pagamento decidir o
/// que renderizar (formulário / recusada / expirada / já paga). A Story 1.7 (Consulta de Reserva)
/// deve reaproveitar/estender este mesmo DTO e o endpoint GET /api/reservas/{codigo}, não duplicar.
/// </summary>
public record ReservaConsultaDto(
    Guid CodigoConsulta,
    string Status,
    bool Expirada,
    string NomeChale,
    DateOnly DataCheckin,
    DateOnly DataCheckout,
    decimal ValorTotal
);
