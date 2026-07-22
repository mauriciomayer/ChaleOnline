namespace ChaleOnline.Domain;

public class Chale
{
    public int Id { get; private set; }
    public string Nome { get; private set; }
    public TipoChale Tipo { get; private set; }
    public int NumeroQuartos { get; private set; }
    public int NumeroBanheiros { get; private set; }
    public decimal Preco { get; private set; }
    public string FotoUrl { get; private set; }

    private Chale()
    {
        Nome = string.Empty;
        FotoUrl = string.Empty;
    }

    public Chale(string nome, TipoChale tipo, int numeroQuartos, int numeroBanheiros, decimal preco, string fotoUrl)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new ArgumentException("Nome do Chalé não pode ser vazio.", nameof(nome));
        }

        if (nome.Length > 120)
        {
            throw new ArgumentException("Nome do Chalé não pode ter mais de 120 caracteres.", nameof(nome));
        }

        if (numeroQuartos <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(numeroQuartos), "Número de quartos deve ser maior que zero.");
        }

        if (numeroBanheiros <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(numeroBanheiros), "Número de banheiros deve ser maior que zero.");
        }

        if (preco <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(preco), "Preço deve ser maior que zero.");
        }

        if (string.IsNullOrWhiteSpace(fotoUrl))
        {
            throw new ArgumentException("URL da foto do Chalé não pode ser vazia.", nameof(fotoUrl));
        }

        Nome = nome;
        Tipo = tipo;
        NumeroQuartos = numeroQuartos;
        NumeroBanheiros = numeroBanheiros;
        Preco = preco;
        FotoUrl = fotoUrl;
    }

    /// <summary>Usado exclusivamente por ChaleOnline.Infrastructure para seed determinístico (EF Core HasData exige Id explícito).</summary>
    internal Chale(int id, string nome, TipoChale tipo, int numeroQuartos, int numeroBanheiros, decimal preco, string fotoUrl)
        : this(nome, tipo, numeroQuartos, numeroBanheiros, preco, fotoUrl)
    {
        Id = id;
    }
}
