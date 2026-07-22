namespace ChaleOnline.Domain;

/// <summary>
/// Linha por diária ocupada (AD-3, modelo de anti-overbooking) — (ChaleId, Data) é a chave única
/// que impede duas Reservas conflitantes para o mesmo Chalé/data. Esta história (1.2) só lê; a
/// Story 1.5 introduz a transação que insere essas linhas ao confirmar uma Reserva.
/// </summary>
public class ReservaNoite
{
    public int ChaleId { get; private set; }
    public DateOnly Data { get; private set; }
    public int ReservaId { get; private set; }

    private ReservaNoite()
    {
    }

    internal ReservaNoite(int chaleId, DateOnly data, int reservaId)
    {
        ChaleId = chaleId;
        Data = data;
        ReservaId = reservaId;
    }
}
