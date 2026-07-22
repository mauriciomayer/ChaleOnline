using ChaleOnline.Application.Reservas;

namespace ChaleOnline.Infrastructure.Jobs;

/// <summary>
/// Wrapper fino em cima de CancelarReservasExpiradasUseCase — existe como classe própria só pra
/// manter o pacote Hangfire fora de ChaleOnline.Application (AD-1/AD-2: Application só depende de
/// Domain). O Hangfire (Api/Infrastructure) agenda/invoca este método; a lógica de negócio real
/// vive inteiramente no use case.
/// </summary>
public class CancelarReservasExpiradasJob(CancelarReservasExpiradasUseCase useCase)
{
    public Task ExecutarAsync(CancellationToken cancellationToken = default)
        => useCase.ExecutarAsync(cancellationToken);
}
