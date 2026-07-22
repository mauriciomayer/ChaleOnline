import { getTranslations } from "next-intl/server";
import { HeaderNav } from "@/components/HeaderNav";
import styles from "./page.module.css";

export default async function ConsultaResultadoLoading() {
  const t = await getTranslations("ConsultaResultadoLoading");

  return (
    <>
      <HeaderNav />
      <main className={styles.main} aria-busy="true">
        <span className={styles.srOnly}>{t("carregando")}</span>
        <div className={styles.skeletonLine} style={{ width: "40%" }} />
        <div className={styles.skeletonBlock} />
      </main>
    </>
  );
}
