using ChaleOnline.Application.Chales;
using ChaleOnline.Domain;
using Microsoft.EntityFrameworkCore;

namespace ChaleOnline.Infrastructure;

public class ChaleRepository(ChaleOnlineDbContext dbContext) : IChaleRepository
{
    public async Task<IReadOnlyList<Chale>> ListarTodosAsync(CancellationToken cancellationToken = default)
        => await dbContext.Chales
            .AsNoTracking()
            .OrderBy(chale => chale.Tipo)
            .ThenBy(chale => chale.Nome)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Chale>> BuscarDisponiveisAsync(
        DateOnly dataCheckin,
        DateOnly dataCheckout,
        IReadOnlyList<TipoChale> tipos,
        CancellationToken cancellationToken = default)
    {
        var chaleIdsIndisponiveis = dbContext.ReservaNoites
            .Where(noite => noite.Data >= dataCheckin && noite.Data < dataCheckout)
            .Join(
                dbContext.Reservas,
                noite => noite.ReservaId,
                reserva => reserva.Id,
                (noite, reserva) => new { noite.ChaleId, reserva.Status })
            .Where(x => x.Status != StatusReserva.Cancelada)
            .Select(x => x.ChaleId);

        var query = dbContext.Chales
            .AsNoTracking()
            .Where(chale => !chaleIdsIndisponiveis.Contains(chale.Id));

        if (tipos.Count > 0)
        {
            query = query.Where(chale => tipos.Contains(chale.Tipo));
        }

        return await query
            .OrderBy(chale => chale.Tipo)
            .ThenBy(chale => chale.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<Chale?> BuscarPorIdAsync(int id, CancellationToken cancellationToken = default)
        => await dbContext.Chales
            .AsNoTracking()
            .FirstOrDefaultAsync(chale => chale.Id == id, cancellationToken);

    public async Task<IReadOnlyList<ChaleMidia>> ListarMidiasAsync(int chaleId, CancellationToken cancellationToken = default)
        => await dbContext.ChaleMidias
            .AsNoTracking()
            .Where(midia => midia.ChaleId == chaleId)
            .OrderBy(midia => midia.Ordem)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ChaleComodidade>> ListarComodidadesAsync(int chaleId, CancellationToken cancellationToken = default)
        => await dbContext.ChaleComodidades
            .AsNoTracking()
            .Where(comodidade => comodidade.ChaleId == chaleId)
            .OrderBy(comodidade => comodidade.Id)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Avaliacao>> ListarAvaliacoesAsync(int chaleId, CancellationToken cancellationToken = default)
        => await dbContext.Avaliacoes
            .AsNoTracking()
            .Where(avaliacao => avaliacao.ChaleId == chaleId)
            .OrderBy(avaliacao => avaliacao.Id)
            .ToListAsync(cancellationToken);
}
