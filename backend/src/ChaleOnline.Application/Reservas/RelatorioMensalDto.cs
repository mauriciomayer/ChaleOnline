namespace ChaleOnline.Application.Reservas;

public record RelatorioMensalDto(IReadOnlyList<RelatorioMensalReservaDto> Reservas, RelatorioMensalResumoDto Resumo);
