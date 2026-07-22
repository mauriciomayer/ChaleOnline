import { getTranslations } from "next-intl/server";
import { HeaderNav } from "@/components/HeaderNav";
import styles from "./page.module.css";

// Placeholder de página completo (fotos, comodidades, avaliações) enquanto os
// dados do Chalé carregam — sem conteúdo parcial "piscando" (AC #3, UX-DR11).
export default async function ChaleDetalheLoading() {
  const t = await getTranslations("ChaleDetalheLoading");

  return (
    <>
      <HeaderNav />
      <main className={styles.main} aria-busy="true">
        <span className={styles.srOnly}>{t("carregando")}</span>
        <div className={styles.skeletonHero} aria-hidden="true" />
        <div className={styles.skeletonContent} aria-hidden="true">
          <div className={styles.skeletonLine} style={{ width: "40%" }} />
          <div className={styles.skeletonLine} style={{ width: "70%" }} />
          <div className={styles.skeletonLine} style={{ width: "30%" }} />
        </div>
      </main>
    </>
  );
}
