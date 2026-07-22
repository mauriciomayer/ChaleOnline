import { getTranslations } from "next-intl/server";
import { HeaderNav } from "@/components/HeaderNav";
import styles from "./page.module.css";

export default async function ReservaLoading() {
  const t = await getTranslations("ReservaLoading");

  return (
    <>
      <HeaderNav />
      <main className={styles.main} aria-busy="true">
        <span className={styles.srOnly}>{t("carregando")}</span>
        <div className={styles.skeletonLine} style={{ width: "30%" }} />
        <div className={styles.skeletonLine} style={{ width: "60%" }} />
        <div className={styles.skeletonBlock} />
      </main>
    </>
  );
}
