import Link from "next/link";
import { redirect } from "next/navigation";
import { obterAdminAtual } from "@/lib/api-client/admin-server";
import { obterVisaoDiaria } from "@/lib/api-client/painel-server";
import { EstadoOcupacaoChip } from "@/components/EstadoOcupacaoChip";
import styles from "./page.module.css";

// Server Component: valida o JWT de verdade (não só a presença do cookie, que o proxy.ts já
// garantiu) — só o status "token-invalido" (401 do backend) usa a mensagem de sessão expirada;
// "indisponivel" (rede/erro do backend) manda pro login sem essa mensagem, pra não confundir uma
// instabilidade transitória com sessão expirada de verdade (achado de code review, 2026-07-20).
export default async function AdminPainelPage() {
  const sessao = await obterAdminAtual();

  if (sessao.status === "token-invalido") {
    redirect("/admin/login?motivo=sessao-expirada");
  }

  if (sessao.status !== "autenticado") {
    redirect("/admin/login");
  }

  const visao = await obterVisaoDiaria();

  return (
    <main className={styles.main}>
      <nav className={styles.navPainel}>
        <Link href="/admin/painel/relatorio-mensal">Relatório Mensal</Link>
      </nav>
      <h1 className={styles.title}>Visão Diária de Ocupação</h1>

      {visao === null && (
        <p className={styles.erro} role="alert">
          Não foi possível carregar a ocupação agora. Tente novamente em instantes.
        </p>
      )}

      {visao !== null && (
        <div className={styles.tabelaWrapper}>
          <table className={styles.tabela}>
            <thead>
              <tr>
                <th>Chalé</th>
                <th>Estrutura</th>
                <th>Estado</th>
                <th>Detalhe</th>
              </tr>
            </thead>
            <tbody>
              {visao.map((chale) => (
                <tr key={chale.chaleId}>
                  <td data-label="Chalé">{chale.nome}</td>
                  <td data-label="Estrutura">{`Tipo ${chale.tipo} · ${chale.numeroQuartos}Q/${chale.numeroBanheiros}B`}</td>
                  <td data-label="Estado">
                    <EstadoOcupacaoChip estado={chale.estado} />
                  </td>
                  <td data-label="Detalhe">{chale.detalhe ?? "—"}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </main>
  );
}
