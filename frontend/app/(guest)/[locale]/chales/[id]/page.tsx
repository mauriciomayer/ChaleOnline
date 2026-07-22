import { notFound } from "next/navigation";
import { getTranslations } from "next-intl/server";
import { Link } from "@/i18n/navigation";
import { HeaderNav } from "@/components/HeaderNav";
import { ChaleGaleria } from "@/components/ChaleGaleria";
import { ComodidadeList } from "@/components/ComodidadeList";
import { AvaliacaoList } from "@/components/AvaliacaoList";
import { buscarChaleDetalhe } from "@/lib/api-client/chales";
import styles from "./page.module.css";

const precoFormatado = new Intl.NumberFormat("pt-BR", {
  style: "currency",
  currency: "BRL",
});

const notaFormatador = new Intl.NumberFormat("pt-BR", {
  minimumFractionDigits: 1,
  maximumFractionDigits: 1,
});

export default async function ChaleDetalhePage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;
  const chaleId = Number(id);
  const t = await getTranslations("ChaleDetalhe");
  const tCommon = await getTranslations("Common");

  if (!Number.isInteger(chaleId) || chaleId <= 0) {
    notFound();
  }

  const chale = await buscarChaleDetalhe(chaleId);

  if (!chale) {
    notFound();
  }

  const notaMedia =
    chale.avaliacoes.length > 0
      ? chale.avaliacoes.reduce((soma, avaliacao) => soma + avaliacao.nota, 0) /
        chale.avaliacoes.length
      : null;

  return (
    <>
      <HeaderNav />
      <main className={styles.main}>
        <Link href="/" className={styles.backLink}>
          ← {tCommon("voltarHome")}
        </Link>

        <ChaleGaleria midias={chale.midias} nomeChale={chale.nome} />

        <div className={styles.content}>
          <div className={styles.headerRow}>
            <span className={styles.tipo}>{t("tipo", { tipo: chale.tipo })}</span>
            <h1 className={styles.name}>{chale.nome}</h1>

            <div className={styles.priceRow}>
              <span className={styles.price}>{precoFormatado.format(chale.preco)}</span>
              <span className={styles.priceCaption}>{t("porNoite")}</span>
            </div>

            {notaMedia !== null && (
              <a href="#avaliacoes" className={styles.ratingLink} aria-label={t("verAvaliacoes")}>
                <span className={styles.rating}>
                  <span aria-hidden="true">★</span> {notaFormatador.format(notaMedia)}
                </span>
                <span className={styles.ratingCount}>({chale.avaliacoes.length})</span>
              </a>
            )}

            <p className={styles.estrutura}>
              {t("quartos", { count: chale.numeroQuartos })} ·{" "}
              {t("banheiros", { count: chale.numeroBanheiros })}
            </p>

            <Link href={`/chales/${chale.id}/reserva`} className={styles.reservarCta}>
              {t("reservar")}
            </Link>
          </div>

          {chale.comodidades.length > 0 && (
            <section aria-labelledby="comodidades-heading" className={styles.section}>
              <h2 id="comodidades-heading" className={styles.sectionTitle}>
                {t("comodidades")}
              </h2>
              <ComodidadeList comodidades={chale.comodidades} />
            </section>
          )}

          <section id="avaliacoes" aria-labelledby="avaliacoes-heading" className={styles.section}>
            <h2 id="avaliacoes-heading" className={styles.sectionTitle}>
              {t("avaliacoes")}
            </h2>
            <AvaliacaoList avaliacoes={chale.avaliacoes} />
          </section>
        </div>
      </main>
    </>
  );
}
