import { getTranslations } from "next-intl/server";
import { Link } from "@/i18n/navigation";
import { HeaderNav } from "@/components/HeaderNav";
import styles from "./page.module.css";

// Chaves fixas q1..q7 — mesmo conjunto de perguntas antes bundlado via lib/content/faq.json,
// agora nos catálogos next-intl (lib/i18n/messages/*.json) pra sair traduzido nos 3 idiomas.
const FAQ_KEYS = ["q1", "q2", "q3", "q4", "q5", "q6", "q7"] as const;

// Conteúdo 100% estático, bundlado no build — sem fetch, sem loading/error state (AC #1).
export default async function FaqPage() {
  const t = await getTranslations("Faq");
  const tCommon = await getTranslations("Common");

  return (
    <>
      <HeaderNav />
      <main className={styles.main}>
        <Link href="/" className={styles.backLink}>
          ← {tCommon("voltarHome")}
        </Link>

        <h1 className={styles.title}>{t("title")}</h1>
        <dl className={styles.lista}>
          {FAQ_KEYS.map((key) => (
            <div key={key} className={styles.item}>
              {/* Tipografia da pergunta é referência ilustrativa (mesmo aviso já usado na galeria da Story 1.3) — nenhum mockup renderizado cobre a tela de FAQ. */}
              <dt className={styles.pergunta}>{t(`${key}.pergunta`)}</dt>
              {/* `body` é o utilitário global (não um CSS Module) — reuso deliberado do token criado na Story 1.3, não um esquecimento. */}
              <dd className="body">{t(`${key}.resposta`)}</dd>
            </div>
          ))}
        </dl>
      </main>
    </>
  );
}
