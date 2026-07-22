using ChaleOnline.Application.Admin;

namespace ChaleOnline.Application.Tests.Admin;

public class AutenticarAdminUseCaseTests
{
    private sealed class AdminAuthenticatorFalso(ResultadoAutenticacao resultado) : IAdminAuthenticator
    {
        public Task<ResultadoAutenticacao> AutenticarAsync(string email, string senha, CancellationToken cancellationToken = default)
            => Task.FromResult(resultado);
    }

    private sealed class JwtTokenGeneratorFalso : IJwtTokenGenerator
    {
        public string? AdminIdRecebido { get; private set; }
        public string? EmailRecebido { get; private set; }

        public TokenGeradoDto Gerar(string adminId, string email)
        {
            AdminIdRecebido = adminId;
            EmailRecebido = email;
            return new TokenGeradoDto("token-falso", DateTime.UtcNow.AddHours(2));
        }
    }

    [Fact]
    public async Task ExecutarAsync_ComCredenciaisValidas_RetornaTokenComAdminIdEEmailCertos()
    {
        var authenticator = new AdminAuthenticatorFalso(new ResultadoAutenticacao(StatusAutenticacao.Sucesso, "admin-id-1", "admin@chaleonline.com"));
        var tokenGenerator = new JwtTokenGeneratorFalso();
        var useCase = new AutenticarAdminUseCase(authenticator, tokenGenerator);

        var resultado = await useCase.ExecutarAsync("admin@chaleonline.com", "ChaleOnline@2026", TestContext.Current.CancellationToken);

        Assert.Equal("token-falso", resultado.Token);
        Assert.Equal("admin-id-1", tokenGenerator.AdminIdRecebido);
        Assert.Equal("admin@chaleonline.com", tokenGenerator.EmailRecebido);
    }

    [Fact]
    public async Task ExecutarAsync_ComCredenciaisInvalidas_LancaCredenciaisInvalidasException()
    {
        var authenticator = new AdminAuthenticatorFalso(new ResultadoAutenticacao(StatusAutenticacao.CredenciaisInvalidas, null, null));
        var useCase = new AutenticarAdminUseCase(authenticator, new JwtTokenGeneratorFalso());

        await Assert.ThrowsAsync<CredenciaisInvalidasException>(() =>
            useCase.ExecutarAsync("admin@chaleonline.com", "senhaErrada", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ExecutarAsync_ComContaBloqueada_LancaContaBloqueadaException()
    {
        var authenticator = new AdminAuthenticatorFalso(new ResultadoAutenticacao(StatusAutenticacao.ContaBloqueada, null, null));
        var useCase = new AutenticarAdminUseCase(authenticator, new JwtTokenGeneratorFalso());

        await Assert.ThrowsAsync<ContaBloqueadaException>(() =>
            useCase.ExecutarAsync("admin@chaleonline.com", "qualquer", TestContext.Current.CancellationToken));
    }
}
