using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace ChaleOnline.Api.IntegrationTests;

/// <summary>
/// Aponta a API para o banco chaleonline_test em vez do banco de dev, via user-secrets
/// deste projeto de teste (dotnet user-secrets set "ConnectionStrings:ChaleOnlineDb" "..." aqui).
/// </summary>
public class ChalesApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddUserSecrets<ChalesApiFactory>();

            // Program.cs usa isso pra pular o registro real do Hangfire (storage/server/dashboard/
            // recurring job). Cada classe de teste com IClassFixture<ChalesApiFactory> cria sua própria
            // instância de WebApplicationFactory<Program>, e xUnit roda classes diferentes em paralelo
            // por padrão — múltiplas instâncias tentando instalar o schema do Hangfire.MySqlStorage ao
            // mesmo tempo colidem ("Table 'job' already exists"). Não usa IWebHostEnvironment/"Testing"
            // pra não alterar o branch IsDevelopment() já existente (MapOpenApi/UseHttpsRedirection).
            // Os testes que exercitam CancelarReservasExpiradasJob resolvem-no via DI e o invocam
            // diretamente, sem depender do scheduler/storage real do Hangfire.
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?> { ["Hangfire:Desabilitado"] = "true" });
        });
    }
}
