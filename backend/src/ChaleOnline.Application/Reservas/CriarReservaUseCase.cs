using ChaleOnline.Application.Chales;
using ChaleOnline.Domain;

namespace ChaleOnline.Application.Reservas;

public class CriarReservaUseCase(IReservaRepository reservaRepository, IChaleRepository chaleRepository)
{
    // Limite superior arbitrário mas razoável (1 mês) — evita que uma requisição insira uma
    // quantidade desproporcional de linhas ReservaNoite numa única transação.
    private const int MaxDiarias = 30;

    public async Task<ReservaCriadaDto> ExecutarAsync(
        int chaleId,
        string nomeHospede,
        string emailHospede,
        DateOnly dataCheckin,
        int quantidadeDiarias,
        CancellationToken cancellationToken = default)
    {
        if (quantidadeDiarias <= 0 || quantidadeDiarias > MaxDiarias)
        {
            throw new ArgumentOutOfRangeException(
                nameof(quantidadeDiarias),
                $"Quantidade de diárias deve estar entre 1 e {MaxDiarias}.");
        }

        var chale = await chaleRepository.BuscarPorIdAsync(chaleId, cancellationToken)
            ?? throw new ChaleNaoEncontradoException();

        var dataCheckout = dataCheckin.AddDays(quantidadeDiarias);

        // Pré-check: evita trabalho óbvio, mas NÃO é a garantia de concorrência — essa vem da
        // constraint UNIQUE(ChaleId, Data) exercida dentro de ReservaRepository.CriarAsync.
        if (await reservaRepository.ExisteConflitoAsync(chaleId, dataCheckin, dataCheckout, cancellationToken))
        {
            throw new ReservaConflitanteException();
        }

        var valorTotal = chale.Preco * quantidadeDiarias;
        var reserva = new Reserva(
            Guid.NewGuid(),
            chaleId,
            nomeHospede,
            emailHospede,
            dataCheckin,
            dataCheckout,
            valorTotal,
            DateTime.UtcNow);

        await reservaRepository.CriarAsync(reserva, cancellationToken);

        return ReservaCriadaDto.DeEntidade(reserva);
    }
}
