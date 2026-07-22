using ChaleOnline.Application.Chales;
using ChaleOnline.Domain;

namespace ChaleOnline.Api.Chales;

public static class ChalesEndpoints
{
    public static IEndpointRouteBuilder MapChalesEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/chales", async (
                ListarChalesUseCase listarUseCase,
                BuscarChalesDisponiveisUseCase buscarUseCase,
                DateOnly? checkin,
                DateOnly? checkout,
                string[]? tipos,
                CancellationToken cancellationToken) =>
            {
                // tipos é validado uma única vez, antes de decidir qual ramo seguir — um tipo
                // inválido deve dar 400 independentemente de checkin/checkout terem sido informados.
                var tiposParseados = new List<TipoChale>();
                if (tipos is not null)
                {
                    foreach (var tipo in tipos)
                    {
                        if (!Enum.TryParse<TipoChale>(tipo, ignoreCase: true, out var tipoChale))
                        {
                            return Results.Problem(
                                detail: $"Tipo de chalé inválido: '{tipo}'.",
                                statusCode: StatusCodes.Status400BadRequest);
                        }

                        tiposParseados.Add(tipoChale);
                    }
                }

                // Sem checkin/checkout: lista tudo (comportamento da Story 1.1), filtrando por estrutura se informado.
                if (checkin is null && checkout is null)
                {
                    var todos = await listarUseCase.ExecutarAsync(cancellationToken);

                    if (tiposParseados.Count > 0)
                    {
                        var tiposFiltro = new HashSet<string>(
                            tiposParseados.Select(t => t.ToString()),
                            StringComparer.OrdinalIgnoreCase);
                        todos = todos.Where(chale => tiposFiltro.Contains(chale.Tipo)).ToList();
                    }

                    return Results.Ok(todos);
                }

                if (checkin is null || checkout is null)
                {
                    return Results.Problem(
                        detail: "Informe checkin e checkout juntos, ou nenhum dos dois.",
                        statusCode: StatusCodes.Status400BadRequest);
                }

                try
                {
                    var disponiveis = await buscarUseCase.ExecutarAsync(
                        checkin.Value,
                        checkout.Value,
                        tiposParseados,
                        cancellationToken);

                    return Results.Ok(disponiveis);
                }
                catch (ArgumentException ex)
                {
                    return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
                }
            })
            .WithName("ListarOuBuscarChales")
            .Produces<IReadOnlyList<ChaleResumoDto>>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        app.MapGet("/api/chales/{id:int}", async (
                int id,
                ObterChaleDetalheUseCase obterDetalheUseCase,
                CancellationToken cancellationToken) =>
            {
                var detalhe = await obterDetalheUseCase.ExecutarAsync(id, cancellationToken);

                return detalhe is null ? Results.NotFound() : Results.Ok(detalhe);
            })
            .WithName("ObterChaleDetalhe")
            .Produces<ChaleDetalheDto>()
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}
