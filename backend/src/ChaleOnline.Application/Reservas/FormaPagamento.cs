namespace ChaleOnline.Application.Reservas;

/// <summary>
/// Formas de pagamento mock (nenhuma integra um gateway real). CartaoRecusadoTeste é a opção
/// dedicada e determinística pra simular recusa — decisão confirmada com Mauricio (2026-07-20) em
/// vez de uma recusa aleatória, pra manter os testes determinísticos e permitir demonstrar os dois
/// fluxos de propósito no portfólio.
/// </summary>
public enum FormaPagamento
{
    CartaoCredito,
    Pix,
    Boleto,
    CartaoRecusadoTeste,
}
