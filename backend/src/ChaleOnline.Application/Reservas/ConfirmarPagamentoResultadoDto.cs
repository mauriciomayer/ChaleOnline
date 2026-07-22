namespace ChaleOnline.Application.Reservas;

public record ConfirmarPagamentoResultadoDto(bool Aprovado, string Status, string? MensagemRecusa);
