"use client";

import { useTranslations } from "next-intl";
import type { ChaleResumo } from "@/lib/api-client/chales";
import { Link } from "@/i18n/navigation";
import { AraucariaMotif } from "./AraucariaMotif";
import styles from "./ChaletCard.module.css";

// Moeda permanece pt-BR/BRL independente do idioma da interface — o imóvel é brasileiro
// e o preço é sempre cobrado em reais, decisão deliberada da Story 2.1 (não é "parte da interface").
const precoFormatado = new Intl.NumberFormat("pt-BR", {
  style: "currency",
  currency: "BRL",
});

interface ChaletCardProps {
  chale: ChaleResumo;
}

export function ChaletCard({ chale }: ChaletCardProps) {
  const t = useTranslations("ChaletCard");

  return (
    <Link href={`/chales/${chale.id}`} className={styles.card}>
      <div className={styles.textureStrip} aria-hidden="true" />
      <div className={styles.photoWrapper}>
        {/* Placeholder ilustrativo (SVG) — foto real de Chalé fica para conteúdo seed futuro; SVG usa <img> simples, não next/image (otimização de SVG exige config extra de segurança fora do escopo desta história). */}
        {/* eslint-disable-next-line @next/next/no-img-element */}
        <img
          src={chale.fotoUrl}
          alt={t("fotoAlt", { nome: chale.nome })}
          className={styles.photo}
        />
        <AraucariaMotif width={20} height={30} strokeWidth={1.6} className={styles.photoMotif} />
        <span className={styles.badgeRooms}>
          {chale.numeroQuartos}Q · {chale.numeroBanheiros}B
        </span>
      </div>

      <div className={styles.body}>
        <span className={styles.tipo}>{t("tipo", { tipo: chale.tipo })}</span>
        <h3 className={styles.name}>{chale.nome}</h3>
        <div className={styles.priceRow}>
          <span className={styles.price}>{precoFormatado.format(chale.preco)}</span>
          <span className={styles.priceCaption}>{t("porNoite")}</span>
        </div>
      </div>
    </Link>
  );
}
