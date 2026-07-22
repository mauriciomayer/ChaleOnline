using ChaleOnline.Application.Email;
using Microsoft.Extensions.Logging;

namespace ChaleOnline.Infrastructure.Email;

/// <summary>
/// Implementação dev do AD-6 — nenhum envio real, só log. Provedor de produção (SendGrid/Resend/SES)
/// é Deferred (decisão já explícita do AD-6, não desta história).
/// </summary>
public class LogEmailSender(ILogger<LogEmailSender> logger) : IEmailSender
{
    public Task EnviarAsync(string destinatario, string assunto, string corpo, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "[E-mail simulado] Para: {Destinatario} | Assunto: {Assunto} | Corpo: {Corpo}",
            destinatario,
            assunto,
            corpo);

        return Task.CompletedTask;
    }
}
