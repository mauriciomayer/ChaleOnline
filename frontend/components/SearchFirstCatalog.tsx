"use client";

import { useState } from "react";
import { useTranslations } from "next-intl";
import type { ChaleResumo, TipoChale } from "@/lib/api-client/chales";
import { buscarChalesDisponiveis } from "@/lib/api-client/chales";
import { ChaletGrid } from "./ChaletGrid";
import { FilterChip } from "./FilterChip";
import styles from "./SearchFirstCatalog.module.css";

type Estado = "prompt" | "carregando" | "resultados" | "erro";

const TIPOS_DISPONIVEIS: TipoChale[] = ["A", "B", "C"];

interface SearchFirstCatalogProps {
  todosChales: ChaleResumo[];
}

/**
 * "Buscar" chama /api/chales (proxy Next.js — ver app/api/chales/route.ts) com
 * checkin/checkout + tipos selecionados; a busca real acontece server-side, na API .NET.
 *
 * Decisão 2026-07-21 (substitui o antigo padrão "search-first" puro): o catálogo completo
 * (`todosChales`, carregado server-side em page.tsx via listarChales()) fica sempre visível
 * abaixo, com ou sem busca — quando a busca acontece, uma seção de "disponíveis" aparece ACIMA
 * dela, nunca a substitui. Isso preserva o fluxo de busca/reserva 100% intacto (mesmo estado,
 * mesma chamada, mesmo componente ChaletGrid) e só resolve a Home parecer vazia antes da busca.
 * Os cards da vitrine completa nunca afirmam disponibilidade (nenhum selo "livre"); um aviso
 * textual (padrão Booking.com) deixa isso explícito em vez de depender só da ausência de selo.
 */
export function SearchFirstCatalog({ todosChales }: SearchFirstCatalogProps) {
  const t = useTranslations("Home");
  const tCommon = useTranslations("Common");
  const [estado, setEstado] = useState<Estado>("prompt");
  const [checkin, setCheckin] = useState("");
  const [checkout, setCheckout] = useState("");
  const [tiposSelecionados, setTiposSelecionados] = useState<TipoChale[]>([]);
  const [chales, setChales] = useState<ChaleResumo[]>([]);

  const hoje = new Date().toISOString().slice(0, 10);
  const intervaloValido = checkin !== "" && checkout !== "" && checkout > checkin;

  function alternarTipo(tipo: TipoChale) {
    setTiposSelecionados((atual) =>
      atual.includes(tipo) ? atual.filter((t) => t !== tipo) : [...atual, tipo]
    );
  }

  async function buscar() {
    // Guarda contra busca duplicada: ignora cliques/submits enquanto uma busca já está em andamento.
    if (!intervaloValido || estado === "carregando") {
      return;
    }

    setEstado("carregando");

    try {
      const resultado = await buscarChalesDisponiveis(checkin, checkout, tiposSelecionados);
      setChales(resultado);
      setEstado("resultados");
    } catch (erro) {
      console.error(erro);
      setEstado("erro");
    }
  }

  return (
    <div>
      <div className={styles.hero}>
        <h1 className={styles.title}>{t("title")}</h1>
        <p className={styles.subtitle}>{t("subtitle")}</p>

        <form
          className={styles.form}
          onSubmit={(event) => {
            event.preventDefault();
            void buscar();
          }}
        >
          <div className={styles.datesRow}>
            <label className={styles.field}>
              <span className={styles.fieldLabel}>{t("checkin")}</span>
              <input
                type="date"
                value={checkin}
                min={hoje}
                onChange={(event) => setCheckin(event.target.value)}
                className={styles.dateInput}
              />
            </label>
            <label className={styles.field}>
              <span className={styles.fieldLabel}>{t("checkout")}</span>
              <input
                type="date"
                value={checkout}
                min={checkin || hoje}
                onChange={(event) => setCheckout(event.target.value)}
                className={styles.dateInput}
              />
            </label>
          </div>

          <fieldset className={styles.chipsFieldset}>
            <legend className={styles.fieldLabel}>{t("estrutura")}</legend>
            <div className={styles.chipsRow}>
              {TIPOS_DISPONIVEIS.map((tipo) => (
                <FilterChip
                  key={tipo}
                  label={t("tipo", { tipo })}
                  selected={tiposSelecionados.includes(tipo)}
                  onToggle={() => alternarTipo(tipo)}
                />
              ))}
            </div>
          </fieldset>

          <button
            type="submit"
            className={styles.searchButton}
            disabled={!intervaloValido || estado === "carregando"}
          >
            {t("buscar")}
          </button>
        </form>
      </div>

      {estado === "carregando" && (
        <div role="status" aria-label={t("buscando")}>
          <span className={styles.srOnly}>{t("buscandoSrOnly")}</span>
          <ul className={styles.skeletonGrid} aria-hidden="true">
            {[0, 1, 2].map((i) => (
              <li key={i} className={styles.skeletonCard} />
            ))}
          </ul>
        </div>
      )}

      {estado === "erro" && (
        <p className={styles.statusMessage} role="alert">
          {tCommon("erroGenerico")}
        </p>
      )}

      {estado === "resultados" && chales.length === 0 && (
        <p className={styles.statusMessage}>{t("semResultados")}</p>
      )}

      {estado === "resultados" && chales.length > 0 && (
        <div>
          <div className={styles.resultsIntro}>
            <h2 className={styles.sectionLabel} aria-live="polite">
              {t("resultados", { count: chales.length })}
            </h2>
          </div>
          <ChaletGrid chales={chales} />
        </div>
      )}

      <div className={styles.allSection}>
        <div className={styles.resultsIntro}>
          <h2 className={styles.sectionLabel}>
            {t("todosChalesTitulo", { count: todosChales.length })}
          </h2>
          <p className={styles.availabilityNotice}>{t("todosChalesAviso")}</p>
        </div>
        <ChaletGrid chales={todosChales} />
      </div>
    </div>
  );
}
