import { notFound } from "next/navigation";
import { getTranslations, getLocale } from "next-intl/server";
import { redirect, Link } from "@/i18n/navigation";
import { HeaderNav } from "@/components/HeaderNav";
import { PagamentoForm } from "@/components/PagamentoForm";
import { consultarReserva } from "@/lib/api-client/reservas";
import styles from "./page.module.css";

// AC #5 — link de pagamento após expirado mostra a tela dedicada "Reserva Expirada" (rota própria
// do Structural Seed), nunca o formulário. Expirada é calculada por tempo no backend (não só por
// Status=Cancelada), então cobre o caso do job de cancelamento ainda não ter rodado.
export default async function PagamentoPage({
  params,
}: {
  params: Promise<{ codigo: string }>;
}) {
  const { codigo } = await params;
  const reserva = await consultarReserva(codigo);

  if (!reserva) {
    notFound();
  }

  if (reserva.expirada) {
    redirect({ href: "/reserva-expirada", locale: await getLocale() });
  }

  const t = await getTranslations("Pagamento");
  const tCommon = await getTranslations("Common");

  return (
    <>
      <HeaderNav />
      <main className={styles.main}>
        <Link href={`/consulta/${codigo}`} className={styles.backLink}>
          ← {tCommon("voltar")}
        </Link>

        <h1 className={styles.title}>{t("title")}</h1>
        {reserva.status === "Paga" ? (
          <p className={styles.jaPago} role="status">
            {t("jaPago")}
          </p>
        ) : (
          <PagamentoForm codigo={codigo} reserva={reserva} />
        )}
      </main>
    </>
  );
}
