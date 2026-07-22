namespace ChaleOnline.Application.Reservas;

/// <summary>
/// QuantidadeTotal conta todas as Reservas do mês, incluindo Canceladas (reflete o tamanho real da
/// lista). TotalValores soma só ValorTotal das Reservas com Status != Cancelada (AC #2).
/// </summary>
public record RelatorioMensalResumoDto(int QuantidadeTotal, int QuantidadeCanceladas, decimal TotalValores);
