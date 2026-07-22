using ChaleOnline.Application.Chales;
using ChaleOnline.Domain;

namespace ChaleOnline.Application.Reservas;

public class ObterRelatorioMensalUseCase(IChaleRepository chaleRepository, IReservaRepository reservaRepository)
{
    public async Task<RelatorioMensalDto> ExecutarAsync(int ano, int mes, CancellationToken cancellationToken = default)
    {
        var primeiroDiaDoMes = new DateOnly(ano, mes, 1);
        var primeiroDiaDoProximoMes = primeiroDiaDoMes.AddMonths(1);

        var reservas = await reservaRepository.BuscarPorMesCheckinAsync(primeiroDiaDoMes, primeiroDiaDoProximoMes, cancellationToken);
        var chales = await chaleRepository.ListarTodosAsync(cancellationToken);
        var nomePorChaleId = chales.ToDictionary(chale => chale.Id, chale => chale.Nome);

        // ThenBy(CodigoConsulta) — desempate determinístico pra Reservas com o mesmo DataCheckin,
        // senão a ordem entre elas não é garantida entre requisições idênticas (achado de code
        // review, 2026-07-20).
        var reservasOrdenadas = reservas
            .OrderBy(reserva => reserva.DataCheckin)
            .ThenBy(reserva => reserva.CodigoConsulta)
            .ToList();

        var reservasDto = reservasOrdenadas
            .Select(reserva => new RelatorioMensalReservaDto(
                reserva.CodigoConsulta.ToString(),
                // "Chalé removido" (não string vazia) — uma célula em branco esconderia um
                // problema real de integridade de dados atrás de uma célula muda (achado de code
                // review, 2026-07-20).
                nomePorChaleId.GetValueOrDefault(reserva.ChaleId, "Chalé removido"),
                reserva.NomeHospede,
                reserva.DataCheckin,
                reserva.DataCheckout,
                reserva.ValorTotal,
                reserva.Status.ToString()))
            .ToList();

        var canceladas = reservasOrdenadas.Where(reserva => reserva.Status == StatusReserva.Cancelada).ToList();
        var resumo = new RelatorioMensalResumoDto(
            QuantidadeTotal: reservasOrdenadas.Count,
            QuantidadeCanceladas: canceladas.Count,
            TotalValores: reservasOrdenadas.Where(reserva => reserva.Status != StatusReserva.Cancelada).Sum(reserva => reserva.ValorTotal));

        return new RelatorioMensalDto(reservasDto, resumo);
    }
}
