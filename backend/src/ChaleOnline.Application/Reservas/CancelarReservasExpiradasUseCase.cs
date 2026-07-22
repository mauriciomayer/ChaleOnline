using ChaleOnline.Application.Email;
using Microsoft.Extensions.Logging;

namespace ChaleOnline.Application.Reservas;

/// <summary>
/// Invocado pela varredura recorrente do Hangfire (Infrastructure/Api) — decisão confirmada com
/// Mauricio (2026-07-20) de usar uma varredura periódica em vez de um job agendado por Reserva.
/// </summary>
public class CancelarReservasExpiradasUseCase(IReservaRepository reservaRepository, IEmailSender emailSender, ILogger<CancelarReservasExpiradasUseCase> logger)
{
    public async Task ExecutarAsync(CancellationToken cancellationToken = default)
    {
        var expiradas = await reservaRepository.BuscarExpiradasAsync(DateTime.UtcNow.AddHours(-48), cancellationToken);

        foreach (var reserva in expiradas)
        {
            // A atualização condicional protege contra a mesma Reserva ter sido paga entre a busca
            // acima e esta tentativa (AC #3) — só envia e-mail de cancelamento se a transição
            // realmente ocorreu.
            var cancelada = await reservaRepository.CancelarPorExpiracaoAsync(reserva.Id, cancellationToken);
            if (cancelada)
            {
                // Cada Reserva é isolada num try/catch próprio — uma falha de e-mail numa não pode
                // abortar o cancelamento (já commitado) das demais Reservas expiradas desta mesma
                // varredura.
                try
                {
                    await emailSender.EnviarAsync(
                        reserva.EmailHospede,
                        "Reserva cancelada",
                        $"Sua Reserva {reserva.CodigoConsulta} foi cancelada automaticamente por falta de pagamento dentro de 48h.",
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Falha ao enviar e-mail de cancelamento pra Reserva {CodigoConsulta}.", reserva.CodigoConsulta);
                }
            }
        }
    }
}
