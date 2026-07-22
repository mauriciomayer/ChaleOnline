import { getTranslations } from "next-intl/server";
import { Link } from "@/i18n/navigation";
import { HeaderNav } from "@/components/HeaderNav";
import { ConsultaForm } from "@/components/ConsultaForm";
import styles from "./page.module.css";

// Conteúdo estático — nenhuma busca acontece aqui, só navegação client-side pro
// resultado em /consulta/[codigo] (ver ConsultaForm).
export default async function ConsultaPage() {
  const t = await getTranslations("Consulta");
  const tCommon = await getTranslations("Common");

  return (
    <>
      <HeaderNav />
      <main className={styles.main}>
        <Link href="/" className={styles.backLink}>
          ← {tCommon("voltarHome")}
        </Link>

        <h1 className={styles.title}>{t("title")}</h1>
        <p className="body">{t("instrucao")}</p>
        <ConsultaForm />
      </main>
    </>
  );
}
