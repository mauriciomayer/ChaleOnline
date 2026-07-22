using ChaleOnline.Application.Reservas;
using ChaleOnline.Domain;

namespace ChaleOnline.Application.Tests.Reservas;

public class ClassificadorOcupacaoTests
{
    private static readonly DateOnly DiaCorrente = new(2026, 7, 20);

    private static Reserva NovaReserva(DateOnly checkin, DateOnly checkout, StatusReserva status = StatusReserva.Paga) => new(
        1, Guid.NewGuid(), chaleId: 10, "Hóspede Teste", "hospede@example.com",
        checkin, checkout, valorTotal: 840m, status, DateTime.UtcNow);

    [Fact]
    public void Classificar_SemReservas_RetornaDesocupado()
    {
        var estado = ClassificadorOcupacao.Classificar(DiaCorrente, []);

        Assert.Equal(EstadoOcupacao.Desocupado, estado);
    }

    [Fact]
    public void Classificar_ComEstadiaEmAndamentoSemEventoHoje_RetornaOcupado()
    {
        var reserva = NovaReserva(DiaCorrente.AddDays(-2), DiaCorrente.AddDays(3));

        var estado = ClassificadorOcupacao.Classificar(DiaCorrente, [reserva]);

        Assert.Equal(EstadoOcupacao.Ocupado, estado);
    }

    [Fact]
    public void Classificar_ComCheckinHoje_RetornaCheckInHoje()
    {
        var reserva = NovaReserva(DiaCorrente, DiaCorrente.AddDays(3));

        var estado = ClassificadorOcupacao.Classificar(DiaCorrente, [reserva]);

        Assert.Equal(EstadoOcupacao.CheckInHoje, estado);
    }

    [Fact]
    public void Classificar_ComCheckoutHoje_RetornaCheckOutHoje()
    {
        var reserva = NovaReserva(DiaCorrente.AddDays(-3), DiaCorrente);

        var estado = ClassificadorOcupacao.Classificar(DiaCorrente, [reserva]);

        Assert.Equal(EstadoOcupacao.CheckOutHoje, estado);
    }

    [Fact]
    public void Classificar_ComCheckoutDeUmaReservaECheckinDeOutraNoMesmoDia_RetornaViradaMesmoDia()
    {
        var reservaQueSai = NovaReserva(DiaCorrente.AddDays(-3), DiaCorrente);
        var reservaQueEntra = NovaReserva(DiaCorrente, DiaCorrente.AddDays(3));

        var estado = ClassificadorOcupacao.Classificar(DiaCorrente, [reservaQueSai, reservaQueEntra]);

        Assert.Equal(EstadoOcupacao.ViradaMesmoDia, estado);
    }

    /// <summary>AC #2: uma Reserva cancelada nunca aparece como "ocupado" (nem nenhum outro estado que não desocupado).</summary>
    [Fact]
    public void Classificar_ComReservaCanceladaComCheckinHoje_RetornaDesocupado()
    {
        var reserva = NovaReserva(DiaCorrente, DiaCorrente.AddDays(3), StatusReserva.Cancelada);

        var estado = ClassificadorOcupacao.Classificar(DiaCorrente, [reserva]);

        Assert.Equal(EstadoOcupacao.Desocupado, estado);
    }

    [Fact]
    public void Classificar_ComReservaCanceladaQueSairiaHojeEOutraValidaQueEntraHoje_RetornaCheckInHojeNaoVirada()
    {
        var reservaCanceladaQueSairia = NovaReserva(DiaCorrente.AddDays(-3), DiaCorrente, StatusReserva.Cancelada);
        var reservaQueEntra = NovaReserva(DiaCorrente, DiaCorrente.AddDays(3));

        var estado = ClassificadorOcupacao.Classificar(DiaCorrente, [reservaCanceladaQueSairia, reservaQueEntra]);

        Assert.Equal(EstadoOcupacao.CheckInHoje, estado);
    }
}
