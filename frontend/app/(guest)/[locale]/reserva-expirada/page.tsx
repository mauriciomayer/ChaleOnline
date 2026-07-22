import { getTranslations } from "next-intl/server";
import { Link } from "@/i18n/navigation";
import { HeaderNav } from "@/components/HeaderNav";
import styles from "./page.module.css";

// Server Component estático, sem parâmetro/fetch — mensagem genérica per EXPERIENCE.md ("Esta
// reserva expirou e o Chalé foi liberado."), sem precisar saber qual Chalé/Reserva especificamente
// (rota do Structural Seed é reserva-expirada sem segmento dinâmico).
export default async function ReservaExpiradaPage() {
  const t = await getTranslations("ReservaExpirada");

  return (
    <>
      <HeaderNav />
      <main className={styles.main}>
        <h1 className={styles.title}>{t("titulo")}</h1>
        <p className="body">{t("mensagem")}</p>
        <p className="body">
          {t("novaReserva")}{" "}
          <Link href="/" className={styles.backLink}>
            {t("verChales")}
          </Link>
        </p>
      </main>
    </>
  );
}
