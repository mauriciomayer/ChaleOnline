using ChaleOnline.Application.Reservas;

namespace ChaleOnline.Api.Admin;

/// <summary>
/// Dados do painel (Visão Diária — Story 3.2, Relatório Mensal — Story 3.3) — separado de
/// AdminEndpoints.cs, que fica focado em login/sessão (Story 3.1).
/// </summary>
public static class PainelEndpoints
{
    public static IEndpointRouteBuilder MapPainelEndpoints(this IEndpointRouteBuilder app)
    {
        // Sem body/params de entrada — único modo de falha é 401, já tratado automaticamente pelo
        // middleware JWT, mesmo padrão simples de GET /api/admin/me.
        app.MapGet("/api/admin/visao-diaria", async (ObterVisaoDiariaUseCase useCase, CancellationToken cancellationToken) =>
                Results.Ok(await useCase.ExecutarAsync(cancellationToken)))
            .RequireAuthorization()
            .WithName("VisaoDiaria")
            .Produces<IReadOnlyList<VisaoDiariaChaleDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        // int? (não int) — permite validar manualmente e devolver o envelope AD-8 (VALIDATION_ERROR,
        // 400) em vez do 400 automático sem envelope do model binding do ASP.NET Core, mesma
        // disciplina já usada em POST /api/admin/login. Primeiro endpoint do painel com parâmetros
        // de entrada (GET /api/admin/me e /visao-diaria não têm nenhum).
        app.MapGet("/api/admin/relatorio-mensal", async (
                int? ano,
                int? mes,
                ObterRelatorioMensalUseCase useCase,
                CancellationToken cancellationToken) =>
            {
                if (ano is null || mes is null)
                {
                    return EnvelopeErro("VALIDATION_ERROR", "Parâmetros 'ano' e 'mes' são obrigatórios.", StatusCodes.Status400BadRequest);
                }

                if (mes < 1 || mes > 12)
                {
                    return EnvelopeErro("VALIDATION_ERROR", "Mês inválido.", StatusCodes.Status400BadRequest);
                }

                // 1-9998 (não 9999) — deixa margem pro AddMonths(1) do use case nunca ultrapassar o
                // ano máximo de DateOnly (9999); sem essa validação, ano=9999 com mes=12 lançaria
                // ArgumentOutOfRangeException não tratada (500 genérico em vez do envelope AD-8),
                // mesmo problema de qualquer ano fora do intervalo válido de DateOnly (achado de code
                // review, 2026-07-20).
                if (ano < 1 || ano > 9998)
                {
                    return EnvelopeErro("VALIDATION_ERROR", "Ano inválido.", StatusCodes.Status400BadRequest);
                }

                var relatorio = await useCase.ExecutarAsync(ano.Value, mes.Value, cancellationToken);
                return Results.Ok(relatorio);
            })
            .RequireAuthorization()
            .WithName("RelatorioMensal")
            .Produces<RelatorioMensalDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        return app;
    }

    // Envelope estável { error: { code, message } } (AD-8) — mesmo padrão de AdminEndpoints.cs.
    private static IResult EnvelopeErro(string code, string message, int statusCode)
        => Results.Json(new { error = new { code, message } }, statusCode: statusCode);
}
