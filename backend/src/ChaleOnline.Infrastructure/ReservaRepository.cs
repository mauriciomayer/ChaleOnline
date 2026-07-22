using ChaleOnline.Application.Reservas;
using ChaleOnline.Domain;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace ChaleOnline.Infrastructure;

public class ReservaRepository(ChaleOnlineDbContext dbContext) : IReservaRepository
{
    private const int MySqlErroChaveDuplicada = 1062;

    // Reservas com datas parcialmente sobrepostas (não idênticas) inserindo ReservaNoite em ordem
    // ascendente de data podem colidir em ordens de lock diferentes o suficiente pro InnoDB detectar
    // um deadlock genuíno (em vez de uma violação limpa de chave duplicada) — do ponto de vista do
    // chamador, ainda é "essa reserva perdeu a corrida por essas datas", então também mapeia pra
    // ReservaConflitanteException.
    private const int MySqlErroDeadlock = 1213;

    public async Task<bool> ExisteConflitoAsync(
        int chaleId,
        DateOnly dataCheckin,
        DateOnly dataCheckout,
        CancellationToken cancellationToken = default)
        => await dbContext.ReservaNoites
            .Where(noite => noite.ChaleId == chaleId && noite.Data >= dataCheckin && noite.Data < dataCheckout)
            .Join(
                dbContext.Reservas,
                noite => noite.ReservaId,
                reserva => reserva.Id,
                (noite, reserva) => reserva.Status)
            .AnyAsync(status => status != StatusReserva.Cancelada, cancellationToken);

    public async Task CriarAsync(Reserva reserva, CancellationToken cancellationToken = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        // Falhas aqui (ex.: violação do índice único de CodigoConsulta, timeout, conexão caída) não
        // são conflito de reserva — propagam sem serem mascaradas; o `using` acima já garante rollback
        // implícito no Dispose se a transação nunca for commitada.
        dbContext.Reservas.Add(reserva);
        await dbContext.SaveChangesAsync(cancellationToken);

        for (var data = reserva.DataCheckin; data < reserva.DataCheckout; data = data.AddDays(1))
        {
            dbContext.ReservaNoites.Add(new ReservaNoite(reserva.ChaleId, data, reserva.Id));
        }

        try
        {
            // A constraint UNIQUE(ChaleId, Data) de ReservaNoiteConfiguration (Story 1.2) é a garantia
            // real de anti-overbooking sob concorrência (AD-3) — se outra transação já ocupou uma das
            // noites, este SaveChangesAsync lança DbUpdateException com violação de chave duplicada
            // (1062) ou, no caso de sobreposição parcial com ordens de lock diferentes, um deadlock
            // genuíno (1213). Qualquer outra falha de banco não é um conflito de reserva — propaga
            // sem ser mascarada, em vez de virar um falso RESERVATION_CONFLICT.
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is MySqlException { Number: MySqlErroChaveDuplicada or MySqlErroDeadlock })
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new ReservaConflitanteException();
        }

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<Reserva?> BuscarPorCodigoConsultaAsync(Guid codigoConsulta, CancellationToken cancellationToken = default)
        => await dbContext.Reservas.SingleOrDefaultAsync(reserva => reserva.CodigoConsulta == codigoConsulta, cancellationToken);

    public async Task<bool> ConfirmarPagamentoAsync(int reservaId, CancellationToken cancellationToken = default)
    {
        // Atualização condicional pura (AD-4): UPDATE ... WHERE Id=@id AND Status=AguardandoPagamento.
        // ExecuteUpdateAsync opera direto no banco, sem carregar a entidade rastreada — evita a mesma
        // janela de corrida check-then-act que um load-mutate-SaveChangesAsync reintroduziria.
        var linhasAfetadas = await dbContext.Reservas
            .Where(reserva => reserva.Id == reservaId && reserva.Status == StatusReserva.AguardandoPagamento)
            .ExecuteUpdateAsync(setters => setters.SetProperty(reserva => reserva.Status, StatusReserva.Paga), cancellationToken);

        return linhasAfetadas == 1;
    }

    public async Task<IReadOnlyList<Reserva>> BuscarExpiradasAsync(DateTime limiteUtc, CancellationToken cancellationToken = default)
        => await dbContext.Reservas
            .Where(reserva => reserva.Status == StatusReserva.AguardandoPagamento && reserva.CriadoEm <= limiteUtc)
            .ToListAsync(cancellationToken);

    public async Task<bool> CancelarPorExpiracaoAsync(int reservaId, CancellationToken cancellationToken = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var linhasAfetadas = await dbContext.Reservas
            .Where(reserva => reserva.Id == reservaId && reserva.Status == StatusReserva.AguardandoPagamento)
            .ExecuteUpdateAsync(setters => setters.SetProperty(reserva => reserva.Status, StatusReserva.Cancelada), cancellationToken);

        if (linhasAfetadas != 1)
        {
            // A Reserva já não estava mais AguardandoPagamento (paga antes, ou já cancelada por outra
            // execução do job) — nada a fazer, não mexe nas ReservaNoite de um caminho que não venceu.
            await transaction.RollbackAsync(cancellationToken);
            return false;
        }

        // Libera as noites ocupadas (AD-3) — resolve o gap documentado desde a Story 1.2: sem isso, a
        // constraint UNIQUE(ChaleId, Data) (cega a Status) bloquearia indevidamente novas Reservas pra
        // essa mesma data mesmo com a Reserva original já cancelada.
        await dbContext.ReservaNoites
            .Where(noite => noite.ReservaId == reservaId)
            .ExecuteDeleteAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    // Sem paginação/limite — deliberado na escala atual (12 Chalés, Visão Diária de um único dia).
    // Não copiar pra um contexto com mais volume sem reavaliar (achado de code review, 2026-07-20).
    public async Task<IReadOnlyList<Reserva>> BuscarRelevantesParaDataAsync(DateOnly data, CancellationToken cancellationToken = default)
        => await dbContext.Reservas
            .AsNoTracking()
            .Where(reserva => reserva.Status != StatusReserva.Cancelada && reserva.DataCheckin <= data && reserva.DataCheckout >= data)
            .ToListAsync(cancellationToken);

    // Sem paginação/limite — deliberado na escala atual do projeto (portfólio, poucas Reservas por
    // mês). Inclui Cancelada de propósito (AC #2 do Relatório Mensal exige que ela apareça na
    // lista); a exclusão do total de valores acontece no use case, não aqui.
    public async Task<IReadOnlyList<Reserva>> BuscarPorMesCheckinAsync(DateOnly primeiroDiaDoMes, DateOnly primeiroDiaDoProximoMes, CancellationToken cancellationToken = default)
        => await dbContext.Reservas
            .AsNoTracking()
            .Where(reserva => reserva.DataCheckin >= primeiroDiaDoMes && reserva.DataCheckin < primeiroDiaDoProximoMes)
            .ToListAsync(cancellationToken);
}
