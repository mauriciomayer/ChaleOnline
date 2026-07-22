import { getTranslations } from "next-intl/server";
import { Link } from "@/i18n/navigation";
import { HeaderNav } from "@/components/HeaderNav";
import styles from "./page.module.css";

// (guest)/ não tem not-found.tsx próprio (só error.tsx cascata pra cá), e consulta/[codigo] é um
// segmento de topo novo — sem herança disponível, precisa do seu próprio. Mensagem neutra (AC
// #2): não revela se o código já existiu pra outra reserva.
export default async function ReservaNaoEncontrada() {
  const t = await getTranslations("ConsultaNaoEncontrada");

  return (
    <>
      <HeaderNav />
      <main className={styles.main}>
        <h1 className={styles.title}>{t("titulo")}</h1>
        <p className="body">
          {t("mensagem")}{" "}
          <Link href="/consulta" className={styles.backLink}>
            {t("tentarNovamente")}
          </Link>
        </p>
      </main>
    </>
  );
}
