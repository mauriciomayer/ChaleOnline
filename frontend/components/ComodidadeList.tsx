import type { ReactNode } from "react";
import styles from "./ComodidadeList.module.css";

interface ComodidadeListProps {
  comodidades: string[];
}

const ICON_PROPS = {
  width: 11,
  height: 11,
  viewBox: "0 0 24 24",
  fill: "none",
  stroke: "var(--color-secondary)",
  strokeWidth: 1.8,
  strokeLinecap: "round" as const,
  strokeLinejoin: "round" as const,
  "aria-hidden": true,
  focusable: false,
};

// Ícone (line-art) por nome de comodidade conhecido; nomes não mapeados caem no
// ícone genérico — nunca quebra a renderização (UX-DR9: nunca só ícone, sempre
// rótulo textual junto, garantido pelo <span> ao lado em ComodidadeList).
function iconePorNome(nome: string): ReactNode {
  switch (nome) {
    case "Lareira":
      return (
        <svg {...ICON_PROPS}>
          <path d="M6 3h12v18H6z" />
          <path d="M9 21c0-3 3-3 3-6s-3-3-3-6" />
        </svg>
      );
    case "Deck com hidromassagem":
      return (
        <svg {...ICON_PROPS}>
          <circle cx="12" cy="12" r="8" />
          <path d="M8 12c1-2 2-2 4-2s3 0 4 2" />
        </svg>
      );
    case "Vista para o bosque":
      return (
        <svg {...ICON_PROPS}>
          <path d="M12 3l5 9H7z" />
          <path d="M12 9l4 8H8z" />
        </svg>
      );
    case "Wi-Fi":
      return (
        <svg {...ICON_PROPS}>
          <path d="M2 8.5a15 15 0 0 1 20 0" />
          <path d="M5.5 12a10 10 0 0 1 13 0" />
          <path d="M9 15.5a5 5 0 0 1 6 0" />
        </svg>
      );
    case "Estacionamento privativo":
      return (
        <svg {...ICON_PROPS}>
          <rect x="4" y="4" width="16" height="16" rx="2" />
          <path d="M9 8h3.5a2.5 2.5 0 0 1 0 5H9V8Zm0 5v4" />
        </svg>
      );
    case "Churrasqueira":
      return (
        <svg {...ICON_PROPS}>
          <circle cx="12" cy="14" r="6" />
          <path d="M8 10l-2-4M16 10l2-4M12 8V4" />
        </svg>
      );
    default:
      return (
        <svg {...ICON_PROPS}>
          <polyline points="4 12 9 17 20 6" />
        </svg>
      );
  }
}

export function ComodidadeList({ comodidades }: ComodidadeListProps) {
  if (comodidades.length === 0) {
    return null;
  }

  return (
    <ul className={styles.lista}>
      {comodidades.map((nome, indice) => (
        <li key={`${nome}-${indice}`} className={styles.item}>
          {iconePorNome(nome)}
          <span>{nome}</span>
        </li>
      ))}
    </ul>
  );
}
