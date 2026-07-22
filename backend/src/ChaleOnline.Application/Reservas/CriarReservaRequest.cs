namespace ChaleOnline.Application.Reservas;

// ValorTotal nunca aparece aqui — é sempre recalculado no servidor a partir de Chale.Preco,
// nunca aceito do cliente (evita um hóspede manipular o valor da própria reserva).
public record CriarReservaRequest(
    int ChaleId,
    string NomeHospede,
    string EmailHospede,
    DateOnly DataCheckin,
    int QuantidadeDiarias
);
