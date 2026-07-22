using ChaleOnline.Domain;

namespace ChaleOnline.Application.Reservas;

public interface IReservaRepository
{
    /// <summary>
    /// Pré-check de disponibilidade — exclui Reservas com Status = Cancelada, mesma lógica de
    /// IChaleRepository.BuscarDisponiveisAsync. NÃO é a garantia de concorrência (ver CriarAsync):
    /// tem uma janela de corrida clássica (check-then-act), só evita trabalho óbvio.
    /// </summary>
    Task<bool> ExisteConflitoAsync(int chaleId, DateOnly dataCheckin, DateOnly dataCheckout, CancellationToken cancellationToken = default);

    /// <summary>
    /// Insere a Reserva e uma ReservaNoite por diária dentro de uma única transação. A garantia real
    /// de anti-overbooking sob concorrência (AD-3) vem da constraint UNIQUE(ChaleId, Data) de
    /// ReservaNoite — se outra transação venceu a corrida, lança ReservaConflitanteException.
    /// </summary>
    Task CriarAsync(Reserva reserva, CancellationToken cancellationToken = default);

    /// <summary>Busca uma Reserva pelo código de consulta (GUID) recebido no link de pagamento/confirmação.</summary>
    Task<Reserva?> BuscarPorCodigoConsultaAsync(Guid codigoConsulta, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualização condicional AguardandoPagamento → Paga (AD-4): `UPDATE ... WHERE Status =
    /// AguardandoPagamento`. Retorna true só se exatamente 1 linha foi afetada — false significa que a
    /// Reserva já não estava mais AguardandoPagamento (perdeu a corrida pro cancelamento automático, ou
    /// já tinha sido paga antes).
    /// </summary>
    Task<bool> ConfirmarPagamentoAsync(int reservaId, CancellationToken cancellationToken = default);

    /// <summary>Todas as Reservas ainda AguardandoPagamento cujo CriadoEm é anterior a limiteUtc (candidatas ao cancelamento por expiração).</summary>
    Task<IReadOnlyList<Reserva>> BuscarExpiradasAsync(DateTime limiteUtc, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualização condicional AguardandoPagamento → Cancelada (AD-4). Se e só se a atualização afetou
    /// exatamente 1 linha, remove também as ReservaNoite da Reserva na mesma transação — resolve o gap
    /// documentado desde a Story 1.2 (ReservaNoite de Reserva cancelada não eram removidas, bloqueando
    /// indevidamente novas Reservas pra mesma data). Retorna true só se a transição realmente ocorreu.
    /// </summary>
    Task<bool> CancelarPorExpiracaoAsync(int reservaId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reservas não-canceladas que cobrem <paramref name="data"/> — usado pela Visão Diária de
    /// Ocupação (FR-10) pra classificar cada Chalé. Inclui o dia de checkout (por isso `>=`, não a
    /// mesma janela `[checkin, checkout)` usada por ExisteConflitoAsync/ReservaNoite): o dia de
    /// checkout não tem linha em ReservaNoite, mas ainda é um estado relevante aqui.
    /// </summary>
    Task<IReadOnlyList<Reserva>> BuscarRelevantesParaDataAsync(DateOnly data, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reservas cujo DataCheckin cai no intervalo semi-aberto [primeiroDiaDoMes,
    /// primeiroDiaDoProximoMes) — usado pelo Relatório Mensal (FR-11), que atribui cada Reserva ao
    /// mês do seu DataCheckin. Inclui Reservas Canceladas (elas aparecem na lista do relatório; só
    /// são excluídas do total de valores, calculado pelo use case, não filtrado aqui).
    /// </summary>
    Task<IReadOnlyList<Reserva>> BuscarPorMesCheckinAsync(DateOnly primeiroDiaDoMes, DateOnly primeiroDiaDoProximoMes, CancellationToken cancellationToken = default);
}
