using ChaleOnline.Infrastructure.Email;
using Microsoft.Extensions.Logging;

namespace ChaleOnline.Api.IntegrationTests.Email;

/// <summary>
/// A garantia de e-mail em confirmação/cancelamento (AC #4) era verificada só contra um
/// IEmailSender falso nos testes de use case — este teste cobre a implementação real (dev-only,
/// log em vez de envio).
/// </summary>
public class LogEmailSenderTests
{
    private sealed class LoggerFalso : ILogger<LogEmailSender>
    {
        public List<(LogLevel Nivel, string Mensagem)> Entradas { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
            => Entradas.Add((logLevel, formatter(state, exception)));
    }

    [Fact]
    public async Task EnviarAsync_Sempre_LogaDestinatarioAssuntoECorpoSemLancar()
    {
        var logger = new LoggerFalso();
        var sender = new LogEmailSender(logger);

        await sender.EnviarAsync("hospede@example.com", "Pagamento confirmado", "Corpo de teste.", TestContext.Current.CancellationToken);

        var entrada = Assert.Single(logger.Entradas);
        Assert.Equal(LogLevel.Information, entrada.Nivel);
        Assert.Contains("hospede@example.com", entrada.Mensagem);
        Assert.Contains("Pagamento confirmado", entrada.Mensagem);
        Assert.Contains("Corpo de teste.", entrada.Mensagem);
    }
}
