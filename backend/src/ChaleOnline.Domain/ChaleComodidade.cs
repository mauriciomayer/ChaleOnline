namespace ChaleOnline.Domain;

public class ChaleComodidade
{
    public int Id { get; private set; }
    public int ChaleId { get; private set; }
    public string Nome { get; private set; }

    private ChaleComodidade()
    {
        Nome = string.Empty;
    }

    /// <summary>Usado exclusivamente por ChaleOnline.Infrastructure para seed determinístico (EF Core HasData exige Id explícito) — sem endpoint de criação no V1.</summary>
    internal ChaleComodidade(int id, int chaleId, string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new ArgumentException("Nome da comodidade não pode ser vazio.", nameof(nome));
        }

        if (nome.Length > 80)
        {
            throw new ArgumentException("Nome da comodidade não pode ter mais de 80 caracteres.", nameof(nome));
        }

        Id = id;
        ChaleId = chaleId;
        Nome = nome;
    }
}
