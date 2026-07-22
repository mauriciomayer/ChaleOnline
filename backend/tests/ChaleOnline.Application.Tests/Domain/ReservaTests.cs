using ChaleOnline.Domain;

namespace ChaleOnline.Application.Tests.Domain;

public class ReservaTests
{
    private static readonly DateTime CriadoEm = new(2026, 8, 1, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateOnly Checkin = new(2026, 8, 10);
    private static readonly DateOnly Checkout = new(2026, 8, 12);

    [Fact]
    public void Construtor_ComDadosValidos_CriaReservaAguardandoPagamento()
    {
        var codigoConsulta = Guid.NewGuid();

        var reserva = new Reserva(codigoConsulta, 1, "João Silva", "joao@example.com", Checkin, Checkout, 840m, CriadoEm);

        Assert.Equal(codigoConsulta, reserva.CodigoConsulta);
        Assert.Equal(1, reserva.ChaleId);
        Assert.Equal("João Silva", reserva.NomeHospede);
        Assert.Equal("joao@example.com", reserva.EmailHospede);
        Assert.Equal(Checkin, reserva.DataCheckin);
        Assert.Equal(Checkout, reserva.DataCheckout);
        Assert.Equal(840m, reserva.ValorTotal);
        Assert.Equal(StatusReserva.AguardandoPagamento, reserva.Status);
        Assert.Equal(CriadoEm, reserva.CriadoEm);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_ComNomeVazio_LancaExcecao(string nomeInvalido)
    {
        Assert.Throws<ArgumentException>(() =>
            new Reserva(Guid.NewGuid(), 1, nomeInvalido, "joao@example.com", Checkin, Checkout, 840m, CriadoEm));
    }

    [Fact]
    public void Construtor_ComNomeMaiorQue150Caracteres_LancaExcecao()
    {
        var nomeMuitoLongo = new string('A', 151);

        Assert.Throws<ArgumentException>(() =>
            new Reserva(Guid.NewGuid(), 1, nomeMuitoLongo, "joao@example.com", Checkin, Checkout, 840m, CriadoEm));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("joao-sem-arroba.com")]
    [InlineData("@")]
    [InlineData("a@")]
    [InlineData("@example.com")]
    public void Construtor_ComEmailInvalido_LancaExcecao(string emailInvalido)
    {
        Assert.Throws<ArgumentException>(() =>
            new Reserva(Guid.NewGuid(), 1, "João Silva", emailInvalido, Checkin, Checkout, 840m, CriadoEm));
    }

    [Fact]
    public void Construtor_ComEmailMaiorQue200Caracteres_LancaExcecao()
    {
        var emailMuitoLongo = new string('a', 195) + "@x.com";

        Assert.Throws<ArgumentException>(() =>
            new Reserva(Guid.NewGuid(), 1, "João Silva", emailMuitoLongo, Checkin, Checkout, 840m, CriadoEm));
    }

    [Fact]
    public void Construtor_ComCheckoutIgualCheckin_LancaExcecao()
    {
        Assert.Throws<ArgumentException>(() =>
            new Reserva(Guid.NewGuid(), 1, "João Silva", "joao@example.com", Checkin, Checkin, 840m, CriadoEm));
    }

    [Fact]
    public void Construtor_ComCheckoutAntesDoCheckin_LancaExcecao()
    {
        Assert.Throws<ArgumentException>(() =>
            new Reserva(Guid.NewGuid(), 1, "João Silva", "joao@example.com", Checkout, Checkin, 840m, CriadoEm));
    }

    [Fact]
    public void Construtor_ComCheckinNoPassado_LancaExcecao()
    {
        var checkinPassado = DateOnly.FromDateTime(CriadoEm).AddDays(-1);
        var checkoutPassado = checkinPassado.AddDays(2);

        Assert.Throws<ArgumentException>(() =>
            new Reserva(Guid.NewGuid(), 1, "João Silva", "joao@example.com", checkinPassado, checkoutPassado, 840m, CriadoEm));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void Construtor_ComValorTotalInvalido_LancaExcecao(decimal valorInvalido)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Reserva(Guid.NewGuid(), 1, "João Silva", "joao@example.com", Checkin, Checkout, valorInvalido, CriadoEm));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Construtor_ComChaleIdInvalido_LancaExcecao(int chaleIdInvalido)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Reserva(Guid.NewGuid(), chaleIdInvalido, "João Silva", "joao@example.com", Checkin, Checkout, 840m, CriadoEm));
    }

    [Fact]
    public void Construtor_ComCriadoEmForaDeUtc_LancaExcecao()
    {
        var criadoEmLocal = DateTime.SpecifyKind(CriadoEm, DateTimeKind.Local);

        Assert.Throws<ArgumentException>(() =>
            new Reserva(Guid.NewGuid(), 1, "João Silva", "joao@example.com", Checkin, Checkout, 840m, criadoEmLocal));
    }
}
