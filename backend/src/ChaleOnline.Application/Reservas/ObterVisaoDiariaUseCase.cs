using ChaleOnline.Application.Chales;
using ChaleOnline.Domain;

namespace ChaleOnline.Application.Reservas;

public class ObterVisaoDiariaUseCase(IChaleRepository chaleRepository, IReservaRepository reservaRepository)
{
    public async Task<IReadOnlyList<VisaoDiariaChaleDto>> ExecutarAsync(CancellationToken cancellationToken = default)
    {
        var diaCorrente = HorarioBrasil.DiaCorrente(DateTime.UtcNow);

        var chales = await chaleRepository.ListarTodosAsync(cancellationToken);
        var reservas = await reservaRepository.BuscarRelevantesParaDataAsync(diaCorrente, cancellationToken);
        var reservasPorChale = reservas
            .GroupBy(reserva => reserva.ChaleId)
            .ToDictionary(grupo => grupo.Key, grupo => (IReadOnlyList<Reserva>)grupo.ToList());

        return chales
            .Select(chale =>
            {
                var reservasDoChale = reservasPorChale.GetValueOrDefault(chale.Id, []);
                var estado = ClassificadorOcupacao.Classificar(diaCorrente, reservasDoChale);
                return new VisaoDiariaChaleDto(
                    chale.Id,
                    chale.Nome,
                    chale.Tipo.ToString(),
                    chale.NumeroQuartos,
                    chale.NumeroBanheiros,
                    estado.ToString(),
                    MontarDetalhe(estado, diaCorrente, reservasDoChale));
            })
            .ToList();
    }

    private static string? MontarDetalhe(EstadoOcupacao estado, DateOnly diaCorrente, IReadOnlyList<Reserva> reservasDoChale)
    {
        // Refiltra Status != Cancelada aqui também — hoje inofensivo (a lista já vem filtrada do
        // repositório), mas defesa em profundidade consistente com ClassificadorOcupacao (achado
        // de code review, 2026-07-20).
        var naoCanceladas = reservasDoChale.Where(r => r.Status != StatusReserva.Cancelada).ToList();
        var checkinHoje = naoCanceladas.FirstOrDefault(r => r.DataCheckin == diaCorrente);
        var checkoutHoje = naoCanceladas.FirstOrDefault(r => r.DataCheckout == diaCorrente);
        var emAndamento = naoCanceladas.FirstOrDefault(r => r.DataCheckin < diaCorrente && diaCorrente < r.DataCheckout);

        return estado switch
        {
            // dd/MM/yyyy, não só dd/MM — uma estadia que atravessa virada de ano ficaria ambígua
            // pro admin (achado de code review, 2026-07-20).
            EstadoOcupacao.Ocupado => emAndamento is null ? null : $"Hóspede: {emAndamento.NomeHospede} · estadia até {emAndamento.DataCheckout:dd/MM/yyyy}",
            EstadoOcupacao.CheckInHoje => checkinHoje is null ? null : $"Chegada: {checkinHoje.NomeHospede}",
            EstadoOcupacao.CheckOutHoje => checkoutHoje is null ? null : $"Saída: {checkoutHoje.NomeHospede}",
            EstadoOcupacao.ViradaMesmoDia => checkoutHoje is null || checkinHoje is null
                ? null
                : $"Saída: {checkoutHoje.NomeHospede} · Chegada: {checkinHoje.NomeHospede}",
            _ => null,
        };
    }
}
