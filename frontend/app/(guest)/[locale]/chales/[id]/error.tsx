"use client";

import { useEffect } from "react";
import { useTranslations } from "next-intl";
import { HeaderNav } from "@/components/HeaderNav";
import styles from "./page.module.css";

// Estado de erro dedicado (rede/servidor), distinto do "não encontrado" (AC #5) —
// mesmo padrão do error.tsx da Home (Story 1.1).
export default function ChaleDetalheError({
  error,
  reset,
}: {
  error: Error;
  reset: () => void;
}) {
  const t = useTranslations("ChaleDetalheError");
  const tCommon = useTranslations("Common");

  useEffect(() => {
    console.error(error);
  }, [error]);

  return (
    <>
      <HeaderNav />
      <main className={styles.main}>
        <div className={styles.intro}>
          <h1 className={styles.title}>{t("titulo")}</h1>
          <p className={styles.subtitle}>
            {t("subtitulo")}{" "}
            <button type="button" onClick={reset} className={styles.retryButton}>
              {tCommon("tentarDeNovo")}
            </button>
          </p>
        </div>
      </main>
    </>
  );
}
