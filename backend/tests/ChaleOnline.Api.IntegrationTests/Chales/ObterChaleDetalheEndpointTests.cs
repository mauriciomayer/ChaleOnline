using System.Net;
using System.Net.Http.Json;
using ChaleOnline.Application.Chales;

namespace ChaleOnline.Api.IntegrationTests.Chales;

/// <summary>
/// Avaliacao/ChaleMidia/ChaleComodidade são seedadas via HasData na migration (dados fixos e
/// permanentes) — diferente de BuscarChalesDisponiveisEndpointTests, esta classe NÃO trunca
/// nenhuma tabela em InitializeAsync/DisposeAsync, só lê os dados de seed já existentes.
/// </summary>
public class ObterChaleDetalheEndpointTests(ChalesApiFactory factory) : IClassFixture<ChalesApiFactory>
{
    // Chalé 1 = Pinheiro Bravo (Tipo A), seedado pela Story 1.1; ganhou 5 ChaleMidia Foto + 1 Video
    // (fotos/vídeo reais de uma pousada, 2026-07-20), 3 ChaleComodidade e 2 Avaliacao.
    private const int ChaleExistente = 1;
    private const int ChaleInexistente = 99999;

    [Fact]
    public async Task GetChaleDetalhe_ComIdValido_Retorna200ComGaleriaComodidadesEAvaliacoes()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync($"/api/chales/{ChaleExistente}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var detalhe = await response.Content.ReadFromJsonAsync<ChaleDetalheDto>(TestContext.Current.CancellationToken);

        Assert.NotNull(detalhe);
        Assert.Equal(ChaleExistente, detalhe.Id);
        Assert.False(string.IsNullOrWhiteSpace(detalhe.Nome));
        Assert.NotEmpty(detalhe.Midias);
        Assert.Equal(5, detalhe.Midias.Count(midia => midia.Tipo == "Foto"));
        Assert.Single(detalhe.Midias, midia => midia.Tipo == "Video");
        Assert.NotEmpty(detalhe.Comodidades);
        Assert.NotEmpty(detalhe.Avaliacoes);
        Assert.All(detalhe.Avaliacoes, avaliacao => Assert.InRange(avaliacao.Nota, 1, 5));
    }

    [Fact]
    public async Task GetChaleDetalhe_ComIdInexistente_Retorna404()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync($"/api/chales/{ChaleInexistente}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
