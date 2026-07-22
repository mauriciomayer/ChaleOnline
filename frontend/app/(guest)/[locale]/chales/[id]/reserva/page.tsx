import { notFound } from "next/navigation";
import { getTranslations } from "next-intl/server";
import { Link } from "@/i18n/navigation";
import { HeaderNav } from "@/components/HeaderNav";
import { ReservaForm } from "@/components/ReservaForm";
import { buscarChaleDetalhe } from "@/lib/api-client/chales";
import styles from "./page.module.css";

// Reaproveita buscarChaleDetalhe (Story 1.3) — só usa id/nome/preco, sem endpoint novo só pra isso.
export default async function ReservaPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;
  const chaleId = Number(id);
  const t = await getTranslations("Reserva");
  const tCommon = await getTranslations("Common");

  if (!Number.isInteger(chaleId) || chaleId <= 0) {
    notFound();
  }

  const chale = await buscarChaleDetalhe(chaleId);

  if (!chale) {
    notFound();
  }

  return (
    <>
      <HeaderNav />
      <main className={styles.main}>
        <Link href={`/chales/${chale.id}`} className={styles.backLink}>
          ← {tCommon("voltar")}
        </Link>

        <p className={styles.tipo}>{t("tipo", { tipo: chale.tipo })}</p>
        <h1 className={styles.title}>{t("title", { nomeChale: chale.nome })}</h1>
        <ReservaForm chaleId={chale.id} nomeChale={chale.nome} precoNoite={chale.preco} />
      </main>
    </>
  );
}
