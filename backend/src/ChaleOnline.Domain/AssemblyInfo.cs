using System.Runtime.CompilerServices;

// Construtores internos (seed determinístico, Ids explícitos) ficam acessíveis só para
// Infrastructure (produção) e os dois projetos de teste (seed de dados de teste).
[assembly: InternalsVisibleTo("ChaleOnline.Infrastructure")]
[assembly: InternalsVisibleTo("ChaleOnline.Application.Tests")]
[assembly: InternalsVisibleTo("ChaleOnline.Api.IntegrationTests")]
