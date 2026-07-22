"use client";

import { useTranslations } from "next-intl";
import { Link } from "@/i18n/navigation";
import styles from "./HeaderNav.module.css";
import { AraucariaMotif } from "./AraucariaMotif";
import { LanguageSelector } from "./LanguageSelector";

export function HeaderNav() {
  const t = useTranslations("HeaderNav");

  return (
    <header className={styles.header}>
      <div className={styles.brand}>
        <Link href="/" className={styles.wordmarkLink} aria-label={t("wordmarkAriaLabel")}>
          <AraucariaMotif className={styles.wordmarkGlyph} />
          <span className={styles.wordmark}>Chalé Online</span>
        </Link>
        <span className={styles.locationTag}>
          <AraucariaMotif width={10} height={15} strokeWidth={2} className={styles.locationGlyph} />
          Campos do Jordão
        </span>
      </div>

      <nav className={styles.nav} aria-label={t("navegacaoPrincipal")}>
        <Link href="/" className={styles.navLink}>
          {t("inicio")}
        </Link>
        <Link href="/faq" className={styles.navLink}>
          {t("faq")}
        </Link>
        <Link href="/consulta" className={styles.navLink}>
          {t("minhaReserva")}
        </Link>
      </nav>

      <LanguageSelector />
    </header>
  );
}
