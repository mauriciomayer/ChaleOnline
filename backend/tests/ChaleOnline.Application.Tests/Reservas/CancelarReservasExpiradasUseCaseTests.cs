using ChaleOnline.Application.Email;
using ChaleOnline.Application.Reservas;
using ChaleOnline.Domain;
using Microsoft.Extensions.Logging.Abstractions;

namespace ChaleOnline.Application.Tests.Reservas;

public class CancelarReservasExpiradasUseCaseTests
{
    private sealed class ReservaRepositoryFalso(IEnumerable<Reserva> reservas) : IReservaRepository
    {
        private readonly Dictionary<int, Reserva> _reservas = reservas.ToDictionary(reserva => reserva.Id);

        public List<int> Canceladas { get; } = [];

        /// <summary>Hook usado só pelo teste de corrida — simula outra chamada (pagamento) vencendo entre o scan e a tentativa de cancelar.</summary>
        public Action<int>? AoIniciarCancelamento { get; set; }

        public Task<bool> ExisteConflitoAsync(int chaleId, DateOnly dataCheckin, DateOnly dataCheckout, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task CriarAsync(Reserva reserva, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Reserva?> BuscarPorCodigoConsultaAsync(Guid codigoConsulta, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<bool> ConfirmarPagamentoAsync(int reservaId, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<IReadOnlyList<Reserva>> BuscarExpiradasAsync(DateTime limiteUtc, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Reserva>>(
                _reservas.Values.Where(reserva => reserva.Status == StatusReserva.AguardandoPagamento && reserva.CriadoEm <= limiteUtc).ToList());

        public Task<bool> CancelarPorExpiracaoAsync(int reservaId, CancellationToken cancellationToken = default)
        {
            AoIniciarCancelamento?.Invoke(reservaId);

            var atual = _reservas[reservaId];
            if (atual.Status != StatusReserva.AguardandoPagamento)
            {
                return Task.FromResult(false);
            }

            Canceladas.Add(reservaId);
            _reservas[reservaId] = ComStatus(atual, StatusReserva.Cancelada);
            return Task.FromResult(true);
        }

        public Task<IReadOnlyList<Reserva>> BuscarRelevantesParaDataAsync(DateOnly data, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<IReadOnlyList<Reserva>> BuscarPorMesCheckinAsync(DateOnly primeiroDiaDoMes, DateOnly primeiroDiaDoProximoMes, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public void MarcarComoPagaExternamente(int reservaId)
            => _reservas[reservaId] = ComStatus(_reservas[reservaId], StatusReserva.Paga);

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
        id, Guid.NewGuid(), chaleId: 1, $"Hóspede {id}", $"hospede{id}@example.com",
        DateOnly.FromDateTime(DateTime.UtcNow).AddDays(5), DateOnly.FromDateTime(DateTime.UtcNow).AddDays(7),
        valorTotal: 800m, status, criadoEm);

    [Fact]
    public async Task ExecutarAsync_ComReservasExpiradas_CancelaECadaUmaEnviaEmail()
    {
        var expirada1 = NovaReserva(1, StatusReserva.AguardandoPagamento, DateTime.UtcNow.AddHours(-49));
        var expirada2 = NovaReserva(2, StatusReserva.AguardandoPagamento, DateTime.UtcNow.AddHours(-72));
        var repositorio = new ReservaRepositoryFalso([expirada1, expirada2]);
        var emailSender = new EmailSenderFalso();
        var useCase = new CancelarReservasExpiradasUseCase(repositorio, emailSender, NullLogger<CancelarReservasExpiradasUseCase>.Instance);

        await useCase.ExecutarAsync(TestContext.Current.CancellationToken);

        Assert.Equal([1, 2], repositorio.Canceladas.Order());
        Assert.Equal(2, emailSender.DestinatariosNotificados.Count);
        Assert.Contains("hospede1@example.com", emailSender.DestinatariosNotificados);
        Assert.Contains("hospede2@example.com", emailSender.DestinatariosNotificados);
    }

    [Fact]
    public async Task ExecutarAsync_ComReservaDentroDaJanelaDe48h_NaoCancelaNemEnviaEmail()
    {
        var dentroDaJanela = NovaReserva(1, StatusReserva.AguardandoPagamento, DateTime.UtcNow.AddHours(-1));
        var repositorio = new ReservaRepositoryFalso([dentroDaJanela]);
        var emailSender = new EmailSenderFalso();
        var useCase = new CancelarReservasExpiradasUseCase(repositorio, emailSender, NullLogger<CancelarReservasExpiradasUseCase>.Instance);

        await useCase.ExecutarAsync(TestContext.Current.CancellationToken);

        Assert.Empty(repositorio.Canceladas);
        Assert.Empty(emailSender.DestinatariosNotificados);
    }

    [Fact]
    public async Task ExecutarAsync_ComReservasJaPagaOuCancelada_NaoEntramNaVarreduraNemSaoTocadas()
    {
        var paga = NovaReserva(1, StatusReserva.Paga, DateTime.UtcNow.AddHours(-100));
        var cancelada = NovaReserva(2, StatusReserva.Cancelada, DateTime.UtcNow.AddHours(-100));
        var repositorio = new ReservaRepositoryFalso([paga, cancelada]);
        var emailSender = new EmailSenderFalso();
        var useCase = new CancelarReservasExpiradasUseCase(repositorio, emailSender, NullLogger<CancelarReservasExpiradasUseCase>.Instance);

        await useCase.ExecutarAsync(TestContext.Current.CancellationToken);

        Assert.Empty(repositorio.Canceladas);
        Assert.Empty(emailSender.DestinatariosNotificados);
    }

    [Fact]
    public async Task ExecutarAsync_QuandoPagamentoVenceACorridaAntesDoCancelamento_NaoEnviaEmailDeCancelamento()
    {
        var reserva = NovaReserva(1, StatusReserva.AguardandoPagamento, DateTime.UtcNow.AddHours(-49));
        var repositorio = new ReservaRepositoryFalso([reserva]);
        repositorio.AoIniciarCancelamento = id => repositorio.MarcarComoPagaExternamente(id);
        var emailSender = new EmailSenderFalso();
        var useCase = new CancelarReservasExpiradasUseCase(repositorio, emailSender, NullLogger<CancelarReservasExpiradasUseCase>.Instance);

        await useCase.ExecutarAsync(TestContext.Current.CancellationToken);

        Assert.Empty(repositorio.Canceladas);
        Assert.Empty(emailSender.DestinatariosNotificados);
    }
}
