namespace ChaleOnline.Api.IntegrationTests;

// Classes de teste que seedam/escrevem Reserva/ReservaNoite diretamente em chaleonline_test devem
// entrar nesta collection — força o xUnit a rodá-las sequencialmente (por padrão, classes de teste
// diferentes rodam em paralelo), evitando colisão entre truncamentos/escritas concorrentes nas
// mesmas tabelas. Ver BuscarChalesDisponiveisEndpointTests e CriarReservaEndpointTests.
[CollectionDefinition("ReservaDbTests")]
public class ReservaDbTestsCollection;
