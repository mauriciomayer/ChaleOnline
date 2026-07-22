using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using ChaleOnline.Application.Admin;
using ChaleOnline.Infrastructure.Admin;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ChaleOnline.Api.IntegrationTests.Admin;

/// <summary>
/// Entra na collection "ReservaDbTests" mesmo não tocando Reserva/ReservaNoite — o nome é
/// Reserva-específico, mas a collection já é a coleção "roda tudo sequencialmente" usada por
/// qualquer classe que precise de exclusividade sobre estado compartilhado. Necessário desde que
/// VisaoDiariaEndpointTests/RelatorioMensalEndpointTests (Stories 3.2/3.3) passaram a logar como o
/// mesmo admin seedado: sem isso, xUnit roda classes de collections diferentes em paralelo, e um
/// login bem-sucedido concorrente de outra classe (`ResetAccessFailedCountAsync`) pode zerar o
/// contador de tentativas no meio do teste de rate-limit desta classe, causando falha intermitente
/// (achado real, reproduzido em execução completa da suite, 2026-07-20). Toca a única linha do
/// admin seedado (AspNetUsers); InitializeAsync reseta AccessFailedCount/LockoutEnd antes de cada
/// teste pra evitar que um teste de rate-limit vaze bloqueio pros seguintes desta mesma classe.
/// </summary>
[Collection("ReservaDbTests")]
public class AdminLoginEndpointTests(ChalesApiFactory factory) : IClassFixture<ChalesApiFactory>, IAsyncLifetime
{
    private const string AdminEmail = "admin@chaleonline.com";
    private const string SenhaCorreta = "ChaleOnline@2026";

    public async ValueTask InitializeAsync()
    {
        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var admin = await userManager.FindByEmailAsync(AdminEmail);
        if (admin is not null)
        {
            await userManager.ResetAccessFailedCountAsync(admin);
            await userManager.SetLockoutEndDateAsync(admin, null);
        }
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task PostLogin_ComCredencialValida_Retorna200ComTokenJwtValidoPorCercaDe2h()
    {
        using var client = factory.CreateClient();

        var resposta = await client.PostAsJsonAsync(
            "/api/admin/login",
            new AdminLoginRequest(AdminEmail, SenhaCorreta),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var dto = await resposta.Content.ReadFromJsonAsync<AdminLoginResultadoDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(dto);
        Assert.False(string.IsNullOrWhiteSpace(dto.Token));

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(dto.Token);
        Assert.True(jwt.ValidTo > DateTime.UtcNow.AddMinutes(115), "Token deveria expirar em ~2h, não antes.");
        Assert.True(jwt.ValidTo <= DateTime.UtcNow.AddMinutes(125), "Token deveria expirar em ~2h, não muito depois.");
    }

    [Fact]
    public async Task PostLogin_ComEmailInexistente_Retorna401ComMensagemGenerica()
    {
        using var client = factory.CreateClient();

        var resposta = await client.PostAsJsonAsync(
            "/api/admin/login",
            new AdminLoginRequest("naoexiste@chaleonline.com", SenhaCorreta),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
        var corpo = await resposta.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("CREDENCIAIS_INVALIDAS", corpo);
    }

    [Fact]
    public async Task PostLogin_ComSenhaErrada_Retorna401ComAMesmaMensagemGenericaDoEmailInexistente()
    {
        using var client = factory.CreateClient();

        var resposta = await client.PostAsJsonAsync(
            "/api/admin/login",
            new AdminLoginRequest(AdminEmail, "senhaErrada"),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
        var corpo = await resposta.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("CREDENCIAIS_INVALIDAS", corpo);
    }

    [Theory]
    [InlineData("")]
    [InlineData("{ isso não é json")]
    public async Task PostLogin_ComBodyVazioOuMalformado_Retorna400DentroDoEnvelope(string corpoBruto)
    {
        using var client = factory.CreateClient();
        using var conteudo = new StringContent(corpoBruto, Encoding.UTF8, "application/json");

        var resposta = await client.PostAsync("/api/admin/login", conteudo, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        var corpo = await resposta.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("VALIDATION_ERROR", corpo);
    }

    /// <summary>
    /// AC #3, mandatório: prova real (não mockada) de que o lockout do UserManager está de fato
    /// conectado — dispara MaxFailedAccessAttempts (5) tentativas erradas e confirma que a 6ª,
    /// mesmo com a senha certa, é bloqueada.
    /// </summary>
    [Fact]
    public async Task PostLogin_Com5TentativasErradasSeguidas_BloqueiaTemporariamenteMesmoComSenhaCertaDepois()
    {
        using var client = factory.CreateClient();

        for (var tentativa = 0; tentativa < 5; tentativa++)
        {
            var respostaErrada = await client.PostAsJsonAsync(
                "/api/admin/login",
                new AdminLoginRequest(AdminEmail, "senhaErrada"),
                TestContext.Current.CancellationToken);
            Assert.Equal(HttpStatusCode.Unauthorized, respostaErrada.StatusCode);
        }

        var respostaComSenhaCerta = await client.PostAsJsonAsync(
            "/api/admin/login",
            new AdminLoginRequest(AdminEmail, SenhaCorreta),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.TooManyRequests, respostaComSenhaCerta.StatusCode);
        var corpo = await respostaComSenhaCerta.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("CONTA_BLOQUEADA_TEMPORARIAMENTE", corpo);
    }

    [Fact]
    public async Task GetMe_SemAuthorizationHeader_Retorna401()
    {
        using var client = factory.CreateClient();

        var resposta = await client.GetAsync("/api/admin/me", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task GetMe_ComTokenValido_Retorna200ComEmailCerto()
    {
        using var client = factory.CreateClient();
        var respostaLogin = await client.PostAsJsonAsync(
            "/api/admin/login",
            new AdminLoginRequest(AdminEmail, SenhaCorreta),
            TestContext.Current.CancellationToken);
        var dto = await respostaLogin.Content.ReadFromJsonAsync<AdminLoginResultadoDto>(TestContext.Current.CancellationToken);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", dto!.Token);
        var resposta = await client.GetAsync("/api/admin/me", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var corpo = await resposta.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains(AdminEmail, corpo);
    }

    [Fact]
    public async Task GetMe_ComTokenDeliberadamenteExpirado_Retorna401()
    {
        using var scope = factory.Services.CreateScope();
        var jwtOptions = scope.ServiceProvider.GetRequiredService<IOptions<JwtOptions>>().Value;

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "id-qualquer"),
            new Claim(ClaimTypes.Email, AdminEmail),
        };
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var tokenExpirado = new JwtSecurityToken(
            issuer: jwtOptions.Issuer,
            audience: jwtOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(-1),
            signingCredentials: credentials);
        var tokenString = new JwtSecurityTokenHandler().WriteToken(tokenExpirado);

        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenString);
        var resposta = await client.GetAsync("/api/admin/me", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }
}
