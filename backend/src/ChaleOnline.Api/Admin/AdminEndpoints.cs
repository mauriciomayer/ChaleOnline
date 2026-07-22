using System.Security.Claims;
using System.Text.Json;
using ChaleOnline.Application.Admin;

namespace ChaleOnline.Api.Admin;

public static class AdminEndpoints
{
    public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/admin/login", async (
                HttpContext httpContext,
                AutenticarAdminUseCase useCase,
                CancellationToken cancellationToken) =>
            {
                // Mesmo padrão de leitura manual do body dentro de try/catch já usado em POST
                // /api/reservas (Story 1.5) — mantém corpo malformado dentro do envelope AD-8.
                AdminLoginRequest? request;
                try
                {
                    request = await httpContext.Request.ReadFromJsonAsync<AdminLoginRequest>(cancellationToken);
                }
                catch (JsonException)
                {
                    return EnvelopeErro("VALIDATION_ERROR", "Corpo da requisição inválido.", StatusCodes.Status400BadRequest);
                }

                if (request is null)
                {
                    return EnvelopeErro("VALIDATION_ERROR", "Corpo da requisição ausente.", StatusCodes.Status400BadRequest);
                }

                // { "email": null, "senha": "x" } desserializa num AdminLoginRequest não-nulo com
                // Email nulo — sem esta checagem, o null chega até o UserManager e lança uma
                // exceção não tratada (500) em vez do 400 esperado (achado de code review, 2026-07-20).
                if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Senha))
                {
                    return EnvelopeErro("VALIDATION_ERROR", "E-mail e senha são obrigatórios.", StatusCodes.Status400BadRequest);
                }

                try
                {
                    var resultado = await useCase.ExecutarAsync(request.Email, request.Senha, cancellationToken);
                    return Results.Ok(resultado);
                }
                catch (CredenciaisInvalidasException)
                {
                    // Mensagem genérica — nunca distingue e-mail inexistente de senha errada.
                    return EnvelopeErro("CREDENCIAIS_INVALIDAS", "E-mail ou senha inválidos.", StatusCodes.Status401Unauthorized);
                }
                catch (ContaBloqueadaException)
                {
                    // Não expõe a contagem exata de tentativas restantes nem o tempo exato de
                    // desbloqueio (EXPERIENCE.md, State Patterns — "Login malsucedido").
                    return EnvelopeErro(
                        "CONTA_BLOQUEADA_TEMPORARIAMENTE",
                        "Muitas tentativas de login. Tente novamente em alguns minutos.",
                        StatusCodes.Status429TooManyRequests);
                }
            })
            .WithName("AdminLogin")
            .Produces<AdminLoginResultadoDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status429TooManyRequests);

        // Existe só pra esta história provar que a autenticação funciona de ponta a ponta
        // (AC #2/#4) — não expõe nada do painel de verdade (Stories 3.2/3.3). Sem token/token
        // expirado: 401 automático do middleware JWT, nenhum código customizado necessário.
        app.MapGet("/api/admin/me", (ClaimsPrincipal user) =>
            {
                var email = user.FindFirstValue(ClaimTypes.Email);
                return Results.Ok(new { email });
            })
            .RequireAuthorization()
            .WithName("AdminMe")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        return app;
    }

    // Envelope estável { error: { code, message } } (AD-8) — mesmo padrão de ReservasEndpoints.cs.
    private static IResult EnvelopeErro(string code, string message, int statusCode)
        => Results.Json(new { error = new { code, message } }, statusCode: statusCode);
}
