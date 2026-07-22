import { getTranslations } from "next-intl/server";
import { Link } from "@/i18n/navigation";
import { HeaderNav } from "@/components/HeaderNav";
import styles from "./page.module.css";

// Mensagem dedicada no registro editorial + caminho de volta pra Home — nunca um
// 404 genérico do servidor (AC #4, UX-DR11).
export default async function ChaleNaoEncontrado() {
  const t = await getTranslations("ChaleNaoEncontrado");
  const tCommon = await getTranslations("Common");

  return (
    <>
      <HeaderNav />
      <main className={styles.main}>
        <div className={styles.intro}>
          <h1 className={styles.title}>{t("titulo")}</h1>
          <p className={styles.subtitle}>
            {t("mensagem")}{" "}
            <Link href="/" className={styles.backLink}>
              {tCommon("voltarHome")}
            </Link>
          </p>
        </div>
      </main>
    </>
  );
}
