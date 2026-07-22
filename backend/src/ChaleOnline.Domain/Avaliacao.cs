namespace ChaleOnline.Domain;

public class Avaliacao
{
    public int Id { get; private set; }
    public int ChaleId { get; private set; }
    public int Nota { get; private set; }
    public string Comentario { get; private set; }

    private Avaliacao()
    {
        Comentario = string.Empty;
    }

    /// <summary>Usado exclusivamente por ChaleOnline.Infrastructure para seed determinístico (EF Core HasData exige Id explícito) — sem endpoint de criação no V1.</summary>
    internal Avaliacao(int id, int chaleId, int nota, string comentario)
    {
        if (nota < 1 || nota > 5)
        {
            throw new ArgumentOutOfRangeException(nameof(nota), "Nota deve estar entre 1 e 5.");
        }

        if (string.IsNullOrWhiteSpace(comentario))
        {
            throw new ArgumentException("Comentário da Avaliação não pode ser vazio.", nameof(comentario));
        }

        if (comentario.Length > 500)
        {
            throw new ArgumentException("Comentário da Avaliação não pode ter mais de 500 caracteres.", nameof(comentario));
        }

        Id = id;
        ChaleId = chaleId;
        Nota = nota;
        Comentario = comentario;
    }
}
