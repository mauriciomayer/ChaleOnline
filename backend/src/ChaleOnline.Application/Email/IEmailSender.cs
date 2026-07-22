namespace ChaleOnline.Application.Email;

/// <summary>
/// AD-6: abstração pura de envio de e-mail — a Application não sabe nada sobre o provedor
/// concreto (SMTP/SendGrid/etc.). A Infrastructure dev ship só loga (LogEmailSender); o provedor
/// de produção é Deferred.
/// </summary>
public interface IEmailSender
{
    Task EnviarAsync(string destinatario, string assunto, string corpo, CancellationToken cancellationToken = default);
}
