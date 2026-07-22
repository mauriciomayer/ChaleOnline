interface AraucariaMotifProps {
  width?: number;
  height?: number;
  strokeWidth?: number;
  color?: string;
  className?: string;
}

/**
 * Glifo reutilizável (UX-DR6) inspirado na araucária de Campos do Jordão.
 * Usado no header-nav (ao lado do wordmark) e no canto da foto do chalet-card.
 */
export function AraucariaMotif({
  width = 15,
  height = 22,
  strokeWidth = 2.4,
  color = "currentColor",
  className,
}: AraucariaMotifProps) {
  return (
    <svg
      width={width}
      height={height}
      viewBox="0 0 15 22"
      fill="none"
      stroke={color}
      strokeWidth={strokeWidth}
      strokeLinecap="round"
      strokeLinejoin="round"
      className={className}
      aria-hidden="true"
      focusable="false"
    >
      <line x1="7.5" y1="2" x2="7.5" y2="20" />
      <path d="M7.5 4 L2 8 M7.5 4 L13 8" />
      <path d="M7.5 9 L1.5 14 M7.5 9 L13.5 14" />
      <path d="M7.5 14 L1 20 M7.5 14 L14 20" />
    </svg>
  );
}
