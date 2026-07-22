import styles from "./EstadoOcupacaoChip.module.css";

const ROTULO_POR_ESTADO: Record<string, string> = {
  Desocupado: "Desocupado",
  Ocupado: "Ocupado",
  CheckInHoje: "Check-in hoje",
  CheckOutHoje: "Check-out hoje",
  ViradaMesmoDia: "Virada no mesmo dia",
};

const CLASSE_POR_ESTADO: Record<string, string> = {
  Desocupado: styles.desocupado,
  Ocupado: styles.ocupado,
  CheckInHoje: styles.checkin,
  CheckOutHoje: styles.checkout,
  ViradaMesmoDia: styles.virada,
};

/**
 * Chip de estado de ocupação — sempre cor + texto, nunca só cor (UX-DR8, EXPERIENCE.md
 * Accessibility Floor). "Virada no mesmo dia" é um estado visualmente distinto dos outros
 * quatro, não uma combinação implícita de check-in+check-out (EXPERIENCE.md Component Patterns).
 */
export function EstadoOcupacaoChip({ estado }: { estado: string }) {
  // Fallback consistente entre os dois mapas — um `estado` fora dos 5 documentados (drift futuro
  // entre backend/frontend) precisa continuar visivelmente marcado como "desconhecido", nunca
  // parecer um "Desocupado" válido com um nome de enum bruto como texto (achado de code review,
  // 2026-07-20).
  const desconhecido = !(estado in CLASSE_POR_ESTADO);
  const classe = desconhecido ? styles.desocupado : CLASSE_POR_ESTADO[estado];
  const rotulo = desconhecido ? "Desconhecido" : ROTULO_POR_ESTADO[estado];

  return (
    <span className={`${styles.chip} ${classe}`}>
      <span className={styles.dot} />
      {rotulo}
    </span>
  );
}
