import Link from "next/link";
import { redirect } from "next/navigation";
import { obterAdminAtual } from "@/lib/api-client/admin-server";
import { obterRelatorioMensal } from "@/lib/api-client/painel-server";
import styles from "./page.module.css";

const precoFormatado = new Intl.NumberFormat("pt-BR", { style: "currency", currency: "BRL" });
const dataFormatada = new Intl.DateTimeFormat("pt-BR", { timeZone: "UTC" });

// "Mês corrente" em America/Sao_Paulo, calculado no frontend via Intl (não existe — nem deve
// existir — um endpoint só pra isso; equivalente ao que HorarioBrasil.DiaCorrente faz no
// backend). formatToParts em vez de confiar num formato de locale específico (ex.: en-CA
// "YYYY-MM-DD") pra extrair ano/mês de forma robusta.
function mesCorrenteEmSaoPaulo(): { ano: number; mes: number } {
  const partes = new Intl.DateTimeFormat("en-US", {
    timeZone: "America/Sao_Paulo",
    year: "numeric",
    month: "2-digit",
  }).formatToParts(new Date());

  return {
    ano: Number(partes.find((parte) => parte.type === "year")!.value),
    mes: Number(partes.find((parte) => parte.type === "month")!.value),
  };
}

// Server Component: mesmo gate de auth de app/admin/painel/page.tsx, sem alteração (Story 3.1 já
// cobre AC #2/#4 de autenticação — não reabrir aqui).
export default async function RelatorioMensalPage({
  searchParams,
}: {
  searchParams: Promise<{ mes?: string }>;
}) {
  const sessao = await obterAdminAtual();

  if (sessao.status === "token-invalido") {
    redirect("/admin/login?motivo=sessao-expirada");
  }

  if (sessao.status !== "autenticado") {
    redirect("/admin/login");
  }

  const { mes: mesParam } = await searchParams;
  const { ano: anoDefault, mes: mesDefault } = mesCorrenteEmSaoPaulo();
  // Regex só garante a forma (\d{4}-\d{2}) — "2026-13" passaria — então valida o mês
  // semanticamente também, senão um "mes" inválido na URL chegaria ao backend, tomaria 400 e
  // apareceria pro admin como o erro genérico de rede em vez de simplesmente cair no mês corrente
  // (achado de code review, 2026-07-20).
  const partesMes = mesParam?.match(/^(\d{4})-(\d{2})$/);
  const mesNumerico = partesMes ? Number(partesMes[2]) : null;
  const [ano, mes] =
    partesMes && mesNumerico !== null && mesNumerico >= 1 && mesNumerico <= 12
      ? [Number(partesMes[1]), mesNumerico]
      : [anoDefault, mesDefault];
  const mesInputValue = `${ano}-${String(mes).padStart(2, "0")}`;

  const relatorio = await obterRelatorioMensal(ano, mes);

  return (
    <main className={styles.main}>
      <nav className={styles.navPainel}>
        <Link href="/admin/painel">Visão Diária</Link>
      </nav>
      <h1 className={styles.title}>Relatório Mensal</h1>

      <form method="get" className={styles.filtro}>
        <label className={styles.filtroLabel}>
          <span>Mês</span>
          <input type="month" name="mes" defaultValue={mesInputValue} />
        </label>
        <button type="submit" className={styles.filtroBotao}>
          Consultar
        </button>
      </form>

      {relatorio === null && (
        <p className={styles.erro} role="alert">
          Não foi possível carregar o relatório agora. Tente novamente em instantes.
        </p>
      )}

      {relatorio !== null && relatorio.reservas.length === 0 && <p className={styles.vazio}>Nenhuma reserva neste mês.</p>}

      {relatorio !== null && relatorio.reservas.length > 0 && (
        <>
          <div className={styles.resumo}>
            <span>
              <strong>{relatorio.resumo.quantidadeTotal}</strong> Reservas
            </span>
            <span>
              <strong>{relatorio.resumo.quantidadeCanceladas}</strong> canceladas
            </span>
            <span>
              Total: <strong>{precoFormatado.format(relatorio.resumo.totalValores)}</strong>
            </span>
          </div>

          <div className={styles.tabelaWrapper}>
            <table className={styles.tabela}>
              <thead>
                <tr>
                  <th>Hóspede</th>
                  <th>Chalé</th>
                  <th>Check-in</th>
                  <th>Check-out</th>
                  <th>Valor</th>
                  <th>Status</th>
                </tr>
              </thead>
              <tbody>
                {relatorio.reservas.map((reserva) => (
                  <tr key={reserva.codigoConsulta} data-cancelada={reserva.status === "Cancelada"} className={styles.linha}>
                    <td data-label="Hóspede">{reserva.nomeHospede}</td>
                    <td data-label="Chalé">{reserva.chaleNome}</td>
                    <td data-label="Check-in">{dataFormatada.format(new Date(reserva.dataCheckin))}</td>
                    <td data-label="Check-out">{dataFormatada.format(new Date(reserva.dataCheckout))}</td>
                    <td data-label="Valor">
                      {precoFormatado.format(reserva.valorTotal)}
                      {reserva.status === "Cancelada" && <span className={styles.rotuloCancelada}> (cancelada, fora do total)</span>}
                    </td>
                    <td data-label="Status">{reserva.status}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </>
      )}
    </main>
  );
}
