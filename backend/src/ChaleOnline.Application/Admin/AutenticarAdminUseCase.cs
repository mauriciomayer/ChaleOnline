namespace ChaleOnline.Application.Admin;

public class AutenticarAdminUseCase(IAdminAuthenticator adminAuthenticator, IJwtTokenGenerator jwtTokenGenerator)
{
    public async Task<AdminLoginResultadoDto> ExecutarAsync(string email, string senha, CancellationToken cancellationToken = default)
    {
        var resultado = await adminAuthenticator.AutenticarAsync(email, senha, cancellationToken);

        switch (resultado.Status)
        {
            case StatusAutenticacao.CredenciaisInvalidas:
                throw new CredenciaisInvalidasException();
            case StatusAutenticacao.ContaBloqueada:
                throw new ContaBloqueadaException();
            case StatusAutenticacao.Sucesso:
                break;
            default:
                // Guarda contra um valor futuro de StatusAutenticacao sem case correspondente
                // cair silenciosamente no caminho de sucesso (achado de code review, 2026-07-20).
                throw new InvalidOperationException($"StatusAutenticacao não tratado: {resultado.Status}.");
        }

        var token = jwtTokenGenerator.Gerar(resultado.AdminId!, resultado.Email!);
        return new AdminLoginResultadoDto(token.Token, token.ExpiraEmUtc);
    }
}
