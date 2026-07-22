namespace ChaleOnline.Domain;

public class Reserva
{
    public int Id { get; private set; }
    public Guid CodigoConsulta { get; private set; }
    public int ChaleId { get; private set; }
    public string NomeHospede { get; private set; }
    public string EmailHospede { get; private set; }
    public DateOnly DataCheckin { get; private set; }
    public DateOnly DataCheckout { get; private set; }
    public decimal ValorTotal { get; private set; }
    public StatusReserva Status { get; private set; }
    public DateTime CriadoEm { get; private set; }

    private Reserva()
    {
        NomeHospede = string.Empty;
        EmailHospede = string.Empty;
    }

    /// <summary>
    /// Cria uma Reserva nova, sempre com Status = AguardandoPagamento (transições de status não são
    /// responsabilidade deste construtor). ValorTotal é sempre calculado pelo chamador a partir de
    /// Chale.Preco — nunca aceito de input externo sem validação de negócio.
    /// </summary>
    public Reserva(
        Guid codigoConsulta,
        int chaleId,
        string nomeHospede,
        string emailHospede,
        DateOnly dataCheckin,
        DateOnly dataCheckout,
        decimal valorTotal,
        DateTime criadoEm)
    {
        if (string.IsNullOrWhiteSpace(nomeHospede))
        {
            throw new ArgumentException("Nome do hóspede não pode ser vazio.", nameof(nomeHospede));
        }

        if (nomeHospede.Length > 150)
        {
            throw new ArgumentException("Nome do hóspede não pode ter mais de 150 caracteres.", nameof(nomeHospede));
        }

        // Checagem leve (não regex RFC completa) — mas exige pelo menos um caractere antes e depois
        // do "@", rejeitando casos triviais como "@" ou "a@" que a checagem anterior deixava passar.
        var indiceArroba = emailHospede.IndexOf('@', StringComparison.Ordinal);
        if (string.IsNullOrWhiteSpace(emailHospede) || indiceArroba <= 0 || indiceArroba == emailHospede.Length - 1)
        {
            throw new ArgumentException("E-mail do hóspede inválido.", nameof(emailHospede));
        }

        if (emailHospede.Length > 200)
        {
            throw new ArgumentException("E-mail do hóspede não pode ter mais de 200 caracteres.", nameof(emailHospede));
        }

        if (chaleId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(chaleId), "ChaleId deve ser maior que zero.");
        }

        if (dataCheckout <= dataCheckin)
        {
            throw new ArgumentException("Data de checkout deve ser posterior à data de checkin.", nameof(dataCheckout));
        }

        if (criadoEm.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("CriadoEm deve estar em UTC (AD-4).", nameof(criadoEm));
        }

        if (dataCheckin < DateOnly.FromDateTime(criadoEm))
        {
            throw new ArgumentException("Data de checkin não pode ser anterior a hoje.", nameof(dataCheckin));
        }

        if (valorTotal <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(valorTotal), "Valor total deve ser maior que zero.");
        }

        CodigoConsulta = codigoConsulta;
        ChaleId = chaleId;
        NomeHospede = nomeHospede;
        EmailHospede = emailHospede;
        DataCheckin = dataCheckin;
        DataCheckout = dataCheckout;
        ValorTotal = valorTotal;
        Status = StatusReserva.AguardandoPagamento;
        CriadoEm = criadoEm;
    }

    /// <summary>
    /// Usado exclusivamente por ChaleOnline.Infrastructure e pelos projetos de teste (seed/fixtures) —
    /// sem validação de negócio, aceita qualquer Status explícito (inclusive Paga/Cancelada para fixtures
    /// de teste). O construtor público acima é o único caminho de criação real de uma Reserva (Story 1.5).
    /// </summary>
    internal Reserva(
        int id,
        Guid codigoConsulta,
        int chaleId,
        string nomeHospede,
        string emailHospede,
        DateOnly dataCheckin,
        DateOnly dataCheckout,
        decimal valorTotal,
        StatusReserva status,
        DateTime criadoEm)
    {
        Id = id;
        CodigoConsulta = codigoConsulta;
        ChaleId = chaleId;
        NomeHospede = nomeHospede;
        EmailHospede = emailHospede;
        DataCheckin = dataCheckin;
        DataCheckout = dataCheckout;
        ValorTotal = valorTotal;
        Status = status;
        CriadoEm = criadoEm;
    }
}
