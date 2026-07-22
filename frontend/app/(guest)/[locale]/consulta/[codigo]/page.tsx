import { notFound } from "next/navigation";
import { getTranslations, getLocale } from "next-intl/server";
import { Link } from "@/i18n/navigation";
import { HeaderNav } from "@/components/HeaderNav";
import { consultarReserva } from "@/lib/api-client/reservas";
import styles from "./page.module.css";

// Texto próprio por valor de Status — nunca um badge de cor isolado (EXPERIENCE.md, State
// Patterns: "Consulta — status da reserva"). Fallback defensivo — StatusReserva só tem 3 membros
// hoje, mas evita vazar a string crua do enum do backend se isso mudar.
const STATUS_MESSAGE_KEY: Record<string, string> = {
  AguardandoPagamento: "statusAguardandoPagamento",
  Paga: "statusPaga",
  Cancelada: "statusCancelada",
};

const precoFormatado = new Intl.NumberFormat("pt-BR", {
  style: "currency",
  currency: "BRL",
});

// Datas viram locale-aware (parte da interface — decisão da Story 2.1); a moeda continua pt-BR/BRL
// (o imóvel é brasileiro, decisão deliberada, ver Dev Notes da história).
const DATE_LOCALE_TAG: Record<string, string> = { pt: "pt-BR", en: "en-US", es: "es-ES" };

// A AC #1 lista só os 3 valores de Status pro texto exibido — mostra "Aguardando pagamento" mesmo
// se Expirada===true (job de cancelamento ainda não rodou), mas o link "Ir para pagamento" só
// aparece se a Reserva NÃO estiver expirada por tempo, pra não oferecer um CTA que sempre dá em
// /reserva-expirada.
export default async function ConsultaResultadoPage({
  params,
}: {
  params: Promise<{ codigo: string }>;
}) {
  const { codigo } = await params;
  const reserva = await consultarReserva(codigo);

  if (!reserva) {
    notFound();
  }

  const t = await getTranslations("ConsultaResultado");
  const tCommon = await getTranslations("Common");
  const locale = await getLocale();
  // DataOnly do backend ("yyyy-MM-dd") não tem componente de hora — formata em UTC pra não sofrer
  // deslocamento de fuso horário local (mesma decisão já registrada em deferred-work.md sobre
  // datas "sem hora" sendo tratadas em UTC no app inteiro).
  const dataFormatada = new Intl.DateTimeFormat(DATE_LOCALE_TAG[locale] ?? "pt-BR", { timeZone: "UTC" });

  return (
    <>
      <HeaderNav />
      <main className={styles.main}>
        <Link href="/" className={styles.backLink}>
          ← {tCommon("voltarHome")}
        </Link>

        <h1 className={styles.title}>{t("title")}</h1>
        <div className={styles.resultado} role="status">
          <p className={styles.chale}>{reserva.nomeChale}</p>
          <p className={styles.status}>
            {STATUS_MESSAGE_KEY[reserva.status] ? t(STATUS_MESSAGE_KEY[reserva.status]) : t("statusDesconhecido")}
          </p>
          <p className={styles.datas}>
            {dataFormatada.format(new Date(reserva.dataCheckin))} — {dataFormatada.format(new Date(reserva.dataCheckout))}
          </p>
          <p className={styles.valor}>
            {t.rich("valorTotal", {
              valor: precoFormatado.format(reserva.valorTotal),
              strong: (chunks) => <strong>{chunks}</strong>,
            })}
          </p>
        </div>

        {reserva.status === "AguardandoPagamento" && !reserva.expirada && (
          <Link href={`/pagamento/${codigo}`} className={styles.linkPagamento}>
            {t("irParaPagamento")}
          </Link>
        )}
      </main>
    </>
  );
}
