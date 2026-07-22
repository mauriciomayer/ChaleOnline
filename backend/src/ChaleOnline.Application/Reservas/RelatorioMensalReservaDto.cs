namespace ChaleOnline.Application.Reservas;

// Sem EmailHospede — não usado por nenhum consumidor do relatório (nem a página, nem a AC), então
// não expõe esse PII a mais do que o necessário (achado de code review, 2026-07-20).
public record RelatorioMensalReservaDto(
    string CodigoConsulta,
    string ChaleNome,
    string NomeHospede,
    DateOnly DataCheckin,
    DateOnly DataCheckout,
    decimal ValorTotal,
    string Status);
