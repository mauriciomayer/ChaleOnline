import styles from "./FilterChip.module.css";

interface FilterChipProps {
  label: string;
  selected: boolean;
  onToggle: () => void;
}

export function FilterChip({ label, selected, onToggle }: FilterChipProps) {
  return (
    <button
      type="button"
      className={styles.chip}
      data-selected={selected}
      aria-pressed={selected}
      onClick={onToggle}
    >
      {label}
    </button>
  );
}
