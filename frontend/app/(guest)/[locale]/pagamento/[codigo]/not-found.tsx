import { getTranslations } from "next-intl/server";
import { Link } from "@/i18n/navigation";
import { HeaderNav } from "@/components/HeaderNav";
import styles from "./page.module.css";

// (guest)/ não tem not-found.tsx próprio (só error.tsx cascata pra cá), e pagamento/[codigo] é um
// segmento de topo novo (não aninhado sob chales/[id]) — sem herança disponível, precisa do seu
// próprio not-found.tsx. Mensagem neutra: não revela se o código já existiu (mesmo princípio da
// Story 1.7).
export default async function ReservaNaoEncontrada() {
  const t = await getTranslations("PagamentoNaoEncontrada");
  const tCommon = await getTranslations("Common");

  return (
    <>
      <HeaderNav />
      <main className={styles.main}>
        <h1 className={styles.title}>{t("titulo")}</h1>
        <p className="body">
          {t("mensagem")}{" "}
          <Link href="/" className={styles.backLink}>
            {tCommon("voltarHome")}
          </Link>
        </p>
      </main>
    </>
  );
}
