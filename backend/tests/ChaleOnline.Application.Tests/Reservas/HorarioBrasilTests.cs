using ChaleOnline.Application.Reservas;

namespace ChaleOnline.Application.Tests.Reservas;

/// <summary>
/// Prova a conversão de fuso horário em si (UTC-03:00, Brasil não observa horário de verão desde
/// 2019) — não só o resultado indireto de outros testes que chamam DiaCorrente pra montar fixtures.
/// </summary>
public class HorarioBrasilTests
{
    [Fact]
    public void DiaCorrente_ComInstanteLogoAntesDaMeiaNoiteEmSaoPaulo_AindaRetornaODiaAnterior()
    {
        // 2026-07-21T02:59:00Z = 2026-07-20T23:59:00 em São Paulo (UTC-03:00).
        var agoraUtc = new DateTime(2026, 7, 21, 2, 59, 0, DateTimeKind.Utc);

        var dia = HorarioBrasil.DiaCorrente(agoraUtc);

        Assert.Equal(new DateOnly(2026, 7, 20), dia);
    }

    [Fact]
    public void DiaCorrente_ComInstanteNoMomentoDaMeiaNoiteEmSaoPaulo_JaRetornaODiaSeguinte()
    {
        // 2026-07-21T03:00:00Z = 2026-07-21T00:00:00 em São Paulo (UTC-03:00).
        var agoraUtc = new DateTime(2026, 7, 21, 3, 0, 0, DateTimeKind.Utc);

        var dia = HorarioBrasil.DiaCorrente(agoraUtc);

        Assert.Equal(new DateOnly(2026, 7, 21), dia);
    }
}
