"use client";

import { useState } from "react";
import { useTranslations } from "next-intl";
import { useRouter } from "@/i18n/navigation";
import styles from "./ConsultaForm.module.css";

// Aceita tanto o código puro quanto um link colado inteiro (ex.: .../pagamento/<guid> ou
// .../consulta/<guid>) — extrai o primeiro GUID encontrado no texto.
const GUID_REGEX = /[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/i;

/**
 * Formulário "Campo único de código/link" — Client Component. Nenhuma chamada de API aqui: ao
 * enviar, navega (client-side) pra /consulta/[codigo], que faz a busca de verdade como Server
 * Component (mesmo padrão de /pagamento/[codigo], Story 1.6). Se nenhum GUID for encontrado no
 * texto, navega com o texto bruto mesmo assim — /consulta/[codigo] trata qualquer código
 * inválido com a mesma mensagem neutra de "não encontrado" (AC #2).
 */
export function ConsultaForm() {
  const t = useTranslations("ConsultaForm");
  const router = useRouter();
  const [texto, setTexto] = useState("");

  function enviar() {
    const textoLimpo = texto.trim();
    if (textoLimpo === "") {
      return;
    }

    const codigoExtraido = GUID_REGEX.exec(textoLimpo)?.[0] ?? textoLimpo;
    router.push(`/consulta/${encodeURIComponent(codigoExtraido)}`);
  }

  return (
    <form
      className={styles.form}
      onSubmit={(event) => {
        event.preventDefault();
        enviar();
      }}
    >
      <label className={styles.field}>
        <span className={styles.fieldLabel}>{t("label")}</span>
        <input
          type="text"
          value={texto}
          onChange={(event) => setTexto(event.target.value)}
          className={styles.textInput}
          placeholder={t("placeholder")}
          required
        />
      </label>

      <button type="submit" className={styles.submitButton} disabled={texto.trim() === ""}>
        {t("consultar")}
      </button>
    </form>
  );
}
