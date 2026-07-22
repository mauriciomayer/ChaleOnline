namespace ChaleOnline.Domain;

public class ChaleMidia
{
    public int Id { get; private set; }
    public int ChaleId { get; private set; }
    public string Url { get; private set; }
    public TipoMidia Tipo { get; private set; }
    public int Ordem { get; private set; }

    private ChaleMidia()
    {
        Url = string.Empty;
    }

    /// <summary>Usado exclusivamente por ChaleOnline.Infrastructure para seed determinístico (EF Core HasData exige Id explícito) — sem endpoint de criação no V1.</summary>
    internal ChaleMidia(int id, int chaleId, string url, TipoMidia tipo, int ordem)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("URL da mídia do Chalé não pode ser vazia.", nameof(url));
        }

        if (url.Length > 255)
        {
            throw new ArgumentException("URL da mídia do Chalé não pode ter mais de 255 caracteres.", nameof(url));
        }

        Id = id;
        ChaleId = chaleId;
        Url = url;
        Tipo = tipo;
        Ordem = ordem;
    }
}
