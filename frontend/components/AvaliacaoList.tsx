import { getTranslations } from "next-intl/server";
import type { Avaliacao } from "@/lib/api-client/chales";
import styles from "./AvaliacaoList.module.css";

interface AvaliacaoListProps {
  avaliacoes: Avaliacao[];
}

/**
 * Somente leitura — sem formulário de nova avaliação em lugar nenhum da página
 * (AC #2, FR6 é seed-only no V1). Sem nome de avaliador: a arquitetura não define
 * esse campo, então não inventamos um.
 */
export async function AvaliacaoList({ avaliacoes }: AvaliacaoListProps) {
  const t = await getTranslations("AvaliacaoList");

  if (avaliacoes.length === 0) {
    return <p className={styles.vazio}>{t("vazio")}</p>;
  }

  return (
    <ul className={styles.lista}>
      {avaliacoes.map((avaliacao, indice) => (
        <li key={indice} className={styles.item}>
          <span className={styles.nota} aria-label={t("notaAriaLabel", { nota: avaliacao.nota })}>
            {"★".repeat(avaliacao.nota)}
            {"☆".repeat(5 - avaliacao.nota)}
          </span>
          <p className={`${styles.comentario} body`}>{avaliacao.comentario}</p>
        </li>
      ))}
    </ul>
  );
}
