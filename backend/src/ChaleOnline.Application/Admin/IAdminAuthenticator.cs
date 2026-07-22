namespace ChaleOnline.Application.Admin;

/// <summary>
/// AD-5: abstração pura de autenticação do admin — a Application não sabe nada sobre ASP.NET Core
/// Identity (mesmo princípio já aplicado a IEmailSender, Story 1.6). A Infrastructure implementa
/// via UserManager&lt;IdentityUser&gt;.
/// </summary>
public interface IAdminAuthenticator
{
    Task<ResultadoAutenticacao> AutenticarAsync(string email, string senha, CancellationToken cancellationToken = default);
}
