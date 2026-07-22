"use client";

import { useTranslations } from "next-intl";
import type { ChaleResumo } from "@/lib/api-client/chales";
import { ChaletCard } from "./ChaletCard";
import styles from "./ChaletGrid.module.css";

interface ChaletGridProps {
  chales: ChaleResumo[];
}

export function ChaletGrid({ chales }: ChaletGridProps) {
  const t = useTranslations("ChaletGrid");

  if (chales.length === 0) {
    return <p className={styles.empty}>{t("vazio")}</p>;
  }

  return (
    <ul className={styles.grid}>
      {chales.map((chale) => (
        <li key={chale.id}>
          <ChaletCard chale={chale} />
        </li>
      ))}
    </ul>
  );
}
