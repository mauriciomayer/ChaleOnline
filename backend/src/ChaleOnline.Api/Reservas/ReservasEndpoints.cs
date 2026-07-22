using System.Text.Json;
using ChaleOnline.Application.Reservas;

namespace ChaleOnline.Api.Reservas;

public static class ReservasEndpoints
{
    public static IEndpointRouteBuilder MapReservasEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/reservas", async (
                HttpContext httpContext,
                CriarReservaUseCase useCase,
                CancellationToken cancellationToken) =>
            {
                // Lido e desserializado manualmente (em vez de bindado como parâmetro de rota) pra
                // que corpo nulo/vazio/JSON malformado caia no mesmo envelope de erro AD-8 — bind
                // automático do minimal API faria isso fora deste try/catch, escapando o envelope.
                CriarReservaRequest? request;
                try
                {
                    request = await httpContext.Request.ReadFromJsonAsync<CriarReservaRequest>(cancellationToken);
                }
                catch (JsonException)
                {
                    return EnvelopeErro("VALIDATION_ERROR", "Corpo da requisição inválido.", StatusCodes.Status400BadRequest);
                }

                if (request is null)
                {
                    return EnvelopeErro("VALIDATION_ERROR", "Corpo da requisição ausente.", StatusCodes.Status400BadRequest);
                }

                try
                {
                    var resultado = await useCase.ExecutarAsync(
                        request.ChaleId,
                        request.NomeHospede,
                        request.EmailHospede,
                        request.DataCheckin,
                        request.QuantidadeDiarias,
                        cancellationToken);

                    return Results.Created($"/api/reservas/{resultado.CodigoConsulta}", resultado);
                }
                catch (ChaleNaoEncontradoException)
                {
                    return EnvelopeErro("CHALE_NOT_FOUND", "Chalé não encontrado.", StatusCodes.Status404NotFound);
                }
                catch (ReservaConflitanteException)
                {
                    return EnvelopeErro(
                        "RESERVATION_CONFLICT",
                        "Esse Chalé acabou de ser reservado para essas datas por outro hóspede.",
                        StatusCodes.Status409Conflict);
                }
                catch (ArgumentException ex)
                {
                    return EnvelopeErro("VALIDATION_ERROR", MensagemSemDetalheInterno(ex), StatusCodes.Status400BadRequest);
                }
            })
            .WithName("CriarReserva")
            .Produces<ReservaCriadaDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);

        app.MapGet("/api/reservas/{codigo:guid}", async (
                Guid codigo,
                ConsultarReservaUseCase useCase,
                CancellationToken cancellationToken) =>
            {
                var resultado = await useCase.ExecutarAsync(codigo, cancellationToken);
                return resultado is null
                    ? EnvelopeErro("RESERVA_NOT_FOUND", "Reserva não encontrada.", StatusCodes.Status404NotFound)
                    : Results.Ok(resultado);
            })
            .WithName("ConsultarReserva")
            .Produces<ReservaConsultaDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        app.MapPost("/api/reservas/{codigo:guid}/pagamento", async (
                Guid codigo,
                HttpContext httpContext,
                ConfirmarPagamentoUseCase useCase,
                CancellationToken cancellationToken) =>
            {
                // Mesmo padrão de leitura manual do body dentro de try/catch já usado em POST
                // /api/reservas (Story 1.5) — mantém corpo malformado/nulo/vazio dentro do envelope AD-8.
                ConfirmarPagamentoRequest? request;
                try
                {
                    request = await httpContext.Request.ReadFromJsonAsync<ConfirmarPagamentoRequest>(cancellationToken);
                }
                catch (JsonException)
                {
                    return EnvelopeErro("VALIDATION_ERROR", "Corpo da requisição inválido.", StatusCodes.Status400BadRequest);
                }

                if (request is null)
                {
                    return EnvelopeErro("VALIDATION_ERROR", "Corpo da requisição ausente.", StatusCodes.Status400BadRequest);
                }

                try
                {
                    var resultado = await useCase.ExecutarAsync(codigo, request.FormaPagamento, cancellationToken);
                    return Results.Ok(resultado);
                }
                catch (ReservaNaoEncontradaException)
                {
                    return EnvelopeErro("RESERVA_NOT_FOUND", "Reserva não encontrada.", StatusCodes.Status404NotFound);
                }
                catch (ReservaExpiradaException)
                {
                    // 410 Gone, não 404/409 — o recurso existiu e "morreu" (janela de 48h vencida),
                    // semântica distinta de "nunca existiu" ou "conflito a resolver".
                    return EnvelopeErro("RESERVA_EXPIRADA", "Esta reserva expirou e o Chalé foi liberado.", StatusCodes.Status410Gone);
                }
                catch (ArgumentException ex)
                {
                    return EnvelopeErro("VALIDATION_ERROR", MensagemSemDetalheInterno(ex), StatusCodes.Status400BadRequest);
                }
            })
            .WithName("ConfirmarPagamento")
            .Produces<ConfirmarPagamentoResultadoDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status410Gone);

        return app;
    }

    // Envelope estável { error: { code, message } } (AD-8) — distinto do Results.Problem (RFC7807)
    // usado em ChalesEndpoints.cs. Esta é a primeira história em que o frontend precisa distinguir
    // um código de erro específico (RESERVATION_CONFLICT), então o envelope real passa a importar de
    // verdade aqui; retrofitar os endpoints antigos fica fora de escopo (ver deferred-work.md).
    private static IResult EnvelopeErro(string code, string message, int statusCode)
        => Results.Json(new { error = new { code, message } }, statusCode: statusCode);

    // ArgumentException/ArgumentOutOfRangeException.Message inclui o sufixo "(Parameter 'x')" do
    // .NET — útil em log, mas é detalhe interno que não deveria vazar pro cliente da API.
    private static string MensagemSemDetalheInterno(ArgumentException ex)
    {
        var indice = ex.Message.IndexOf(" (Parameter", StringComparison.Ordinal);
        return indice < 0 ? ex.Message : ex.Message[..indice];
    }
}
