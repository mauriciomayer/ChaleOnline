using ChaleOnline.Domain;

namespace ChaleOnline.Application.Chales;

public interface IChaleRepository
{
    Task<IReadOnlyList<Chale>> ListarTodosAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retorna os Chalés sem ReservaNoite ocupada em nenhuma data do intervalo [dataCheckin, dataCheckout),
    /// excluindo Reservas com Status = Cancelada. <paramref name="tipos"/> vazio significa "todos os tipos".
    /// </summary>
    Task<IReadOnlyList<Chale>> BuscarDisponiveisAsync(
        DateOnly dataCheckin,
        DateOnly dataCheckout,
        IReadOnlyList<TipoChale> tipos,
        CancellationToken cancellationToken = default);

    /// <summary>Retorna o Chalé com o id informado, ou <c>null</c> se não existir.</summary>
    Task<Chale?> BuscarPorIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ChaleMidia>> ListarMidiasAsync(int chaleId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ChaleComodidade>> ListarComodidadesAsync(int chaleId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Avaliacao>> ListarAvaliacoesAsync(int chaleId, CancellationToken cancellationToken = default);
}
