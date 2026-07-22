using ChaleOnline.Application.Admin;
using Microsoft.AspNetCore.Identity;

namespace ChaleOnline.Infrastructure.Admin;

/// <summary>
/// Implementa IAdminAuthenticator via UserManager&lt;IdentityUser&gt; — deliberadamente NÃO usa
/// SignInManager (orientado a cookies, não se aplica ao fluxo JWT desta história). Orquestra
/// manualmente o mesmo comportamento de lockout que SignInManager faria internamente.
/// </summary>
public class IdentityAdminAuthenticator(UserManager<IdentityUser> userManager) : IAdminAuthenticator
{
    public async Task<ResultadoAutenticacao> AutenticarAsync(string email, string senha, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            // Mensagem genérica — nunca revela se o e-mail existe (mesma disciplina de
            // neutralidade já aplicada em RESERVA_NOT_FOUND, Stories 1.6/1.7).
            return new ResultadoAutenticacao(StatusAutenticacao.CredenciaisInvalidas, null, null);
        }

        if (await userManager.IsLockedOutAsync(user))
        {
            return new ResultadoAutenticacao(StatusAutenticacao.ContaBloqueada, null, null);
        }

        var senhaOk = await userManager.CheckPasswordAsync(user, senha);
        if (!senhaOk)
        {
            await userManager.AccessFailedAsync(user);
            return new ResultadoAutenticacao(StatusAutenticacao.CredenciaisInvalidas, null, null);
        }

        await userManager.ResetAccessFailedCountAsync(user);
        return new ResultadoAutenticacao(StatusAutenticacao.Sucesso, user.Id, user.Email);
    }
}
