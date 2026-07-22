"use client";

import { useLocale, useTranslations } from "next-intl";
import { Link, usePathname } from "@/i18n/navigation";
import { routing } from "@/i18n/routing";
import styles from "./LanguageSelector.module.css";

export function LanguageSelector() {
  const locale = useLocale();
  const pathname = usePathname();
  const t = useTranslations("LanguageSelector");

  return (
    <nav className={styles.selector} aria-label={t("ariaLabel")}>
      {routing.locales.map((loc, index) => (
        <span key={loc} className={styles.item}>
          {index > 0 && (
            <span className={styles.divider} aria-hidden="true">
              ·
            </span>
          )}
          <Link
            href={pathname}
            locale={loc}
            className={loc === locale ? `${styles.link} ${styles.active}` : styles.link}
            aria-current={loc === locale ? "true" : undefined}
          >
            {loc.toUpperCase()}
          </Link>
        </span>
      ))}
    </nav>
  );
}
