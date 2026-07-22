using System.Net;
using System.Net.Http.Json;
using ChaleOnline.Application.Chales;

namespace ChaleOnline.Api.IntegrationTests.Chales;

public class GetChalesEndpointTests(ChalesApiFactory factory) : IClassFixture<ChalesApiFactory>
{
    [Fact]
    public async Task GetChales_RetornaOsDozeChalesSeedados()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/chales", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var chales = await response.Content.ReadFromJsonAsync<List<ChaleResumoDto>>(TestContext.Current.CancellationToken);

        Assert.NotNull(chales);
        Assert.Equal(12, chales.Count);
        Assert.Equal(6, chales.Count(c => c.Tipo == "A"));
        Assert.Equal(4, chales.Count(c => c.Tipo == "B"));
        Assert.Equal(2, chales.Count(c => c.Tipo == "C"));
        Assert.All(chales, chale =>
        {
            Assert.False(string.IsNullOrWhiteSpace(chale.Nome));
            Assert.False(string.IsNullOrWhiteSpace(chale.FotoUrl));
            Assert.True(chale.Preco > 0);
        });
    }
}
