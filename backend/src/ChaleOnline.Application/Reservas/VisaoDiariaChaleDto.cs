namespace ChaleOnline.Application.Reservas;

/// <summary>
/// Detalhe é preenchido só com dados reais (nome do hóspede, datas) — nunca inventa horário de
/// check-in/check-out, já que Reserva não guarda hora, só DateOnly.
/// </summary>
public record VisaoDiariaChaleDto(
    int ChaleId,
    string Nome,
    string Tipo,
    int NumeroQuartos,
    int NumeroBanheiros,
    string Estado,
    string? Detalhe);
