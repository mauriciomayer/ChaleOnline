using ChaleOnline.Application.Email;
using ChaleOnline.Application.Reservas;
using ChaleOnline.Domain;
using Microsoft.Extensions.Logging.Abstractions;

namespace ChaleOnline.Application.Tests.Reservas;

public class ConfirmarPagamentoUseCaseTests
{
    private sealed class ReservaRepositoryFalso(params Reserva[] reservas) : IReservaRepository
    {
        private readonly Dictionary<int, Reserva> _reservas = reservas.ToDictionary(reserva => reserva.Id);

        public int ConfirmarPagamentoChamadas { get; private set; }

        public Task<bool> ExisteConflitoAsync(int chaleId, DateOnly dataCheckin, DateOnly dataCheckout, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task CriarAsync(Reserva reserva, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Reserva?> BuscarPorCodigoConsultaAsync(Guid codigoConsulta, CancellationToken cancellationToken = default)
            => Task.FromResult(_reservas.Values.SingleOrDefault(reserva => reserva.CodigoConsulta == codigoConsulta));

        public Task<bool> ConfirmarPagamentoAsync(int reservaId, CancellationToken cancellationToken = default)
        {
            ConfirmarPagamentoChamadas++;
            var reserva = _reservas[reservaId];
            if (reserva.Status != StatusReserva.AguardandoPagamento)
            {
                return Task.FromResult(false);
            }

            _reservas[reservaId] = ComStatus(reserva, StatusReserva.Paga);
            return Task.FromResult(true);
        }

        public Task<IReadOnlyList<Reserva>> BuscarExpiradasAsync(DateTime limiteUtc, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<bool> CancelarPorExpiracaoAsync(int reservaId, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<IReadOnlyList<Reserva>> BuscarRelevantesParaDataAsync(DateOnly data, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<IReadOnlyList<Reserva>> BuscarPorMesCheckinAsync(DateOnly primeiroDiaDoMes, DateOnly primeiroDiaDoProximoMes, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        private static Reserva ComStatus(Reserva original, StatusReserva novoStatus) => new(
            original.Id, original.CodigoConsulta, original.ChaleId, original.NomeHospede, original.EmailHospede,
            original.DataCheckin, original.DataCheckout, original.ValorTotal, novoStatus, original.CriadoEm);
    }

    private sealed class EmailSenderFalso : IEmailSender
    {
        public List<string> DestinatariosNotificados { get; } = [];

        public Task EnviarAsync(string destinatario, string assunto, string corpo, CancellationToken cancellationToken = default)
        {
            DestinatariosNotificados.Add(destinatario);
            return Task.CompletedTask;
        }
    }

    private static Reserva NovaReserva(int id, StatusReserva status, DateTime criadoEm) => new(
        id, Guid.NewGuid(), chaleId: 1, "Hóspede Teste", "hospede@example.com",
        DateOnly.FromDateTime(DateTime.UtcNow).AddDays(5), DateOnly.FromDateTime(DateTime.UtcNow).AddDays(7),
        valorTotal: 800m, status, criadoEm);

    [Fact]
    public async Task ExecutarAsync_ComFormaPagamentoValida_AprovaEEnviaEmail()
    {
        var reserva = NovaReserva(1, StatusReserva.AguardandoPagamento, DateTime.UtcNow.AddHours(-1));
        var repositorio = new ReservaRepositoryFalso(reserva);
        var emailSender = new EmailSenderFalso();
        var useCase = new ConfirmarPagamentoUseCase(repositorio, emailSender, NullLogger<ConfirmarPagamentoUseCase>.Instance);

        var resultado = await useCase.ExecutarAsync(reserva.CodigoConsulta, "CartaoCredito", TestContext.Current.CancellationToken);

        Assert.True(resultado.Aprovado);
        Assert.Equal("Paga", resultado.Status);
        Assert.Null(resultado.MensagemRecusa);
        Assert.Single(emailSender.DestinatariosNotificados, "hospede@example.com");
    }

    [Fact]
    public async Task ExecutarAsync_ComFormaCartaoRecusadoTeste_RecusaSemAlterarStatusNemEnviarEmail()
    {
        var reserva = NovaReserva(1, StatusReserva.AguardandoPagamento, DateTime.UtcNow.AddHours(-1));
        var repositorio = new ReservaRepositoryFalso(reserva);
        var emailSender = new EmailSenderFalso();
        var useCase = new ConfirmarPagamentoUseCase(repositorio, emailSender, NullLogger<ConfirmarPagamentoUseCase>.Instance);

        var resultado = await useCase.ExecutarAsync(reserva.CodigoConsulta, "CartaoRecusadoTeste", TestContext.Current.CancellationToken);

        Assert.False(resultado.Aprovado);
        Assert.Equal("AguardandoPagamento", resultado.Status);
        Assert.NotNull(resultado.MensagemRecusa);
        Assert.Empty(emailSender.DestinatariosNotificados);
        Assert.Equal(0, repositorio.ConfirmarPagamentoChamadas);
    }

    [Fact]
    public async Task ExecutarAsync_ComCodigoInexistente_LancaReservaNaoEncontradaException()
    {
        var repositorio = new ReservaRepositoryFalso();
        var useCase = new ConfirmarPagamentoUseCase(repositorio, new EmailSenderFalso(), NullLogger<ConfirmarPagamentoUseCase>.Instance);

        await Assert.ThrowsAsync<ReservaNaoEncontradaException>(() =>
            useCase.ExecutarAsync(Guid.NewGuid(), "CartaoCredito", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ExecutarAsync_ComReservaExpiradaPorTempoMesmoAindaAguardandoPagamento_LancaReservaExpiradaException()
    {
        var reserva = NovaReserva(1, StatusReserva.AguardandoPagamento, DateTime.UtcNow.AddHours(-49));
        var repositorio = new ReservaRepositoryFalso(reserva);
        var useCase = new ConfirmarPagamentoUseCase(repositorio, new EmailSenderFalso(), NullLogger<ConfirmarPagamentoUseCase>.Instance);

        await Assert.ThrowsAsync<ReservaExpiradaException>(() =>
            useCase.ExecutarAsync(reserva.CodigoConsulta, "CartaoCredito", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ExecutarAsync_ComReservaJaCancelada_LancaReservaExpiradaException()
    {
        var reserva = NovaReserva(1, StatusReserva.Cancelada, DateTime.UtcNow.AddHours(-1));
        var repositorio = new ReservaRepositoryFalso(reserva);
        var useCase = new ConfirmarPagamentoUseCase(repositorio, new EmailSenderFalso(), NullLogger<ConfirmarPagamentoUseCase>.Instance);

        await Assert.ThrowsAsync<ReservaExpiradaException>(() =>
            useCase.ExecutarAsync(reserva.CodigoConsulta, "CartaoCredito", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ExecutarAsync_ComReservaJaPaga_RetornaSucessoIdempotenteSemChamarConfirmarPagamentoDeNovo()
    {
        var reserva = NovaReserva(1, StatusReserva.Paga, DateTime.UtcNow.AddHours(-1));
        var repositorio = new ReservaRepositoryFalso(reserva);
        var emailSender = new EmailSenderFalso();
        var useCase = new ConfirmarPagamentoUseCase(repositorio, emailSender, NullLogger<ConfirmarPagamentoUseCase>.Instance);

        var resultado = await useCase.ExecutarAsync(reserva.CodigoConsulta, "CartaoCredito", TestContext.Current.CancellationToken);

        Assert.True(resultado.Aprovado);
        Assert.Equal("Paga", resultado.Status);
        Assert.Equal(0, repositorio.ConfirmarPagamentoChamadas);
        Assert.Empty(emailSender.DestinatariosNotificados);
    }

    [Fact]
    public async Task ExecutarAsync_ComFormaPagamentoInvalida_LancaArgumentException()
    {
        var reserva = NovaReserva(1, StatusReserva.AguardandoPagamento, DateTime.UtcNow.AddHours(-1));
        var repositorio = new ReservaRepositoryFalso(reserva);
        var useCase = new ConfirmarPagamentoUseCase(repositorio, new EmailSenderFalso(), NullLogger<ConfirmarPagamentoUseCase>.Instance);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            useCase.ExecutarAsync(reserva.CodigoConsulta, "BitcoinMagico", TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("0")]
    [InlineData("1")]
    [InlineData("2")]
    [InlineData("3")]
    public async Task ExecutarAsync_ComFormaPagamentoNumerica_LancaArgumentException(string formaPagamentoNumerica)
    {
        // Enum.TryParse sozinho aceitaria essas strings (valor subjacente do enum) — só nomes
        // deveriam ser aceitos como forma de pagamento válida.
        var reserva = NovaReserva(1, StatusReserva.AguardandoPagamento, DateTime.UtcNow.AddHours(-1));
        var repositorio = new ReservaRepositoryFalso(reserva);
        var useCase = new ConfirmarPagamentoUseCase(repositorio, new EmailSenderFalso(), NullLogger<ConfirmarPagamentoUseCase>.Instance);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            useCase.ExecutarAsync(reserva.CodigoConsulta, formaPagamentoNumerica, TestContext.Current.CancellationToken));
    }
}
