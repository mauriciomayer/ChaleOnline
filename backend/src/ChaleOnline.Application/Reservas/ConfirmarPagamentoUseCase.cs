using ChaleOnline.Application.Email;
using ChaleOnline.Domain;
using Microsoft.Extensions.Logging;

namespace ChaleOnline.Application.Reservas;

public class ConfirmarPagamentoUseCase(IReservaRepository reservaRepository, IEmailSender emailSender, ILogger<ConfirmarPagamentoUseCase> logger)
{
    public async Task<ConfirmarPagamentoResultadoDto> ExecutarAsync(
        Guid codigoConsulta,
        string formaPagamentoRaw,
        CancellationToken cancellationToken = default)
    {
        // Enum.TryParse sozinho aceita strings puramente numéricas ("0"-"3") representando o valor
        // subjacente — não é a intenção (só os nomes deveriam ser aceitos), então confirma que o
        // texto bate com um dos nomes de verdade antes de aceitar o parse.
        var nomeReconhecido = Enum.GetNames<FormaPagamento>().Contains(formaPagamentoRaw, StringComparer.OrdinalIgnoreCase);
        if (!nomeReconhecido || !Enum.TryParse<FormaPagamento>(formaPagamentoRaw, ignoreCase: true, out var forma))
        {
            throw new ArgumentException("Forma de pagamento inválida.", nameof(formaPagamentoRaw));
        }

        var reserva = await reservaRepository.BuscarPorCodigoConsultaAsync(codigoConsulta, cancellationToken)
            ?? throw new ReservaNaoEncontradaException();

        if (ReservaExpiracao.EstaExpirada(reserva, DateTime.UtcNow))
        {
            throw new ReservaExpiradaException();
        }

        // Idempotente: reenvio/double-click num pagamento já confirmado não deve virar erro.
        if (reserva.Status == StatusReserva.Paga)
        {
            return new ConfirmarPagamentoResultadoDto(true, StatusReserva.Paga.ToString(), null);
        }

        if (forma == FormaPagamento.CartaoRecusadoTeste)
        {
            // Nenhuma escrita no banco — Status continua AguardandoPagamento, permitindo nova
            // tentativa imediata dentro da janela de 48h (AC #6).
            return new ConfirmarPagamentoResultadoDto(
                false,
                StatusReserva.AguardandoPagamento.ToString(),
                "Pagamento recusado. Tente novamente com outra forma de pagamento.");
        }

        var confirmado = await reservaRepository.ConfirmarPagamentoAsync(reserva.Id, cancellationToken);
        if (!confirmado)
        {
            // Perdeu a corrida pro job de cancelamento entre o BuscarPorCodigoConsultaAsync acima e
            // agora (AC #3 — atualização condicional garante que só um vence).
            throw new ReservaExpiradaException();
        }

        // O pagamento já foi confirmado no banco no momento em que chegamos aqui (linha acima) — uma
        // falha ao notificar o hóspede não pode derrubar uma operação que já teve sucesso, nem pode
        // ser tentada de novo no reenvio idempotente (o branch Status==Paga acima nunca chega aqui).
        try
        {
            await emailSender.EnviarAsync(
                reserva.EmailHospede,
                "Pagamento confirmado",
                $"Seu pagamento da Reserva {reserva.CodigoConsulta} foi confirmado.",
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha ao enviar e-mail de confirmação de pagamento pra Reserva {CodigoConsulta}.", reserva.CodigoConsulta);
        }

        return new ConfirmarPagamentoResultadoDto(true, StatusReserva.Paga.ToString(), null);
    }
}
