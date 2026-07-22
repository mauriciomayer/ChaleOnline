using ChaleOnline.Domain;

namespace ChaleOnline.Application.Reservas;

public enum EstadoOcupacao
{
    Desocupado,
    Ocupado,
    CheckInHoje,
    CheckOutHoje,
    ViradaMesmoDia,
}

/// <summary>
/// Fórmula de derivação de ocupação (FR-10, ARCHITECTURE-SPINE.md) — os 5 estados são mutuamente
/// exclusivos por Chalé por dia, avaliados só sobre Reservas com Status != Cancelada. O repositório
/// (`BuscarRelevantesParaDataAsync`) já filtra isso na query, mas o classificador também filtra
/// aqui — defesa em profundidade, AC #2 (uma Reserva cancelada nunca aparece como "ocupado") nunca
/// depende só do chamador se lembrar de filtrar corretamente. Ordem de prioridade abaixo é a que
/// resolve o único caso ambíguo (virada) antes dos casos mais simples.
/// </summary>
public static class ClassificadorOcupacao
{
    public static EstadoOcupacao Classificar(DateOnly diaCorrente, IReadOnlyList<Reserva> reservasDoChale)
    {
        Reserva? checkoutHoje = null;
        Reserva? checkinHoje = null;
        var ocupado = false;

        foreach (var reserva in reservasDoChale)
        {
            if (reserva.Status == StatusReserva.Cancelada)
            {
                continue;
            }

            if (reserva.DataCheckout == diaCorrente)
            {
                checkoutHoje = reserva;
            }

            if (reserva.DataCheckin == diaCorrente)
            {
                checkinHoje = reserva;
            }

            if (reserva.DataCheckin < diaCorrente && diaCorrente < reserva.DataCheckout)
            {
                ocupado = true;
            }
        }

        // Virada: a Reserva que sai e a que entra são necessariamente diferentes (uma Reserva não
        // pode ter DataCheckin == DataCheckout == diaCorrente — o construtor exige checkout > checkin).
        if (checkoutHoje is not null && checkinHoje is not null)
        {
            return EstadoOcupacao.ViradaMesmoDia;
        }

        if (checkinHoje is not null)
        {
            return EstadoOcupacao.CheckInHoje;
        }

        if (checkoutHoje is not null)
        {
            return EstadoOcupacao.CheckOutHoje;
        }

        return ocupado ? EstadoOcupacao.Ocupado : EstadoOcupacao.Desocupado;
    }
}
