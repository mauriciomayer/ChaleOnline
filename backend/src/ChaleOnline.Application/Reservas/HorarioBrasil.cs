namespace ChaleOnline.Application.Reservas;

/// <summary>
/// Primeira conversão de fuso horário do projeto — datas são persistidas em UTC, mas o "dia
/// corrente" da Visão Diária de Ocupação (FR-10) é calculado em America/Sao_Paulo no momento da
/// leitura (conversão de apresentação, não de armazenamento). Brasil não observa horário de verão
/// desde 2019, então o offset é fixo (UTC-03:00); mesmo assim usamos o `TimeZoneInfo` com o id IANA
/// em vez de somar -3h à mão, pra ficar correto automaticamente se isso um dia mudar.
/// </summary>
public static class HorarioBrasil
{
    private static readonly TimeZoneInfo FusoSaoPaulo = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    /// <summary>
    /// "Hoje" em America/Sao_Paulo, a partir de um instante UTC explícito (sempre passado pelo
    /// chamador — mesmo padrão de <see cref="ReservaExpiracao.EstaExpirada"/>, sem abstração de
    /// clock/tempo neste projeto).
    /// </summary>
    public static DateOnly DiaCorrente(DateTime agoraUtc)
        => DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(agoraUtc, FusoSaoPaulo));
}
