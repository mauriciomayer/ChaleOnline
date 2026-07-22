"use client";

import { useState } from "react";
import { useTranslations } from "next-intl";
import type { ChaleMidia } from "@/lib/api-client/chales";
import styles from "./ChaleGaleria.module.css";

interface ChaleGaleriaProps {
  midias: ChaleMidia[];
  nomeChale: string;
}

/**
 * Composição (hero + tira de thumbnails) é uma referência ilustrativa, não uma
 * decisão de design formalmente travada — gap conhecido UX-DR12 (DESIGN.md não
 * trava a página de Detalhe do Chalé).
 */
export function ChaleGaleria({ midias, nomeChale }: ChaleGaleriaProps) {
  const t = useTranslations("ChaleGaleria");
  const [indiceAtivo, setIndiceAtivo] = useState(0);
  const midiaAtiva = midias[indiceAtivo];

  if (!midiaAtiva) {
    return (
      <div className={styles.galeria}>
        <div className={styles.hero} aria-label={t("semFotos", { nomeChale })} />
      </div>
    );
  }

  return (
    <div className={styles.galeria}>
      <div className={styles.hero}>
        {midiaAtiva.tipo === "Video" ? (
          <video
            key={midiaAtiva.url}
            src={midiaAtiva.url}
            controls
            className={styles.heroMedia}
            aria-label={t("video", { nomeChale })}
          />
        ) : (
          // eslint-disable-next-line @next/next/no-img-element
          <img
            src={midiaAtiva.url}
            alt={t("foto", { indice: indiceAtivo + 1, total: midias.length, nomeChale })}
            className={styles.heroMedia}
          />
        )}
      </div>

      {midias.length > 1 && (
        <ul className={styles.thumbnails}>
          {midias.map((midia, indice) => (
            <li key={`${midia.url}-${indice}`}>
              <button
                type="button"
                className={styles.thumbnailButton}
                data-active={indice === indiceAtivo}
                onClick={() => setIndiceAtivo(indice)}
                aria-label={
                  midia.tipo === "Video"
                    ? t("verVideo", { nomeChale })
                    : t("verFoto", { indice: indice + 1, nomeChale })
                }
              >
                {midia.tipo === "Video" ? (
                  <span className={styles.videoThumbnail} aria-hidden="true">
                    ▶
                  </span>
                ) : (
                  // eslint-disable-next-line @next/next/no-img-element
                  <img
                    src={midia.url}
                    alt={t("miniaturaFoto", { indice: indice + 1, nomeChale })}
                    className={styles.thumbnailMedia}
                  />
                )}
              </button>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
