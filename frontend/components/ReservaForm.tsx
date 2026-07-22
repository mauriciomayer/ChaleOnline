"use client";

import { useState } from "react";
import { useTranslations } from "next-intl";
import { Link } from "@/i18n/navigation";
import { criarReserva, ReservaError, type ReservaCriada } from "@/lib/api-client/reservas";
import { apiErrorMessage } from "@/lib/i18n/apiErrorMessage";
import styles from "./ReservaForm.module.css";

type Estado = "formulario" | "enviando" | "confirmada" | "conflito" | "erro";

interface ReservaFormProps {
  chaleId: number;
  nomeChale: string;
  precoNoite: number;
}

const precoFormatado = new Intl.NumberFormat("pt-BR", {
  style: "currency",
  currency: "BRL",
});

/**
 * Formulário + confirmação da Reserva num único Client Component com estado local — decisão
 * confirmada 2026-07-20: uma rota /confirmacao separada exigiria buscar a Reserva de novo por
 * CodigoConsulta, e esse GET-por-código é escopo da Story 1.7 (Consulta de Reserva). A tela de
 * confirmação usa os dados já retornados pelo próprio POST.
 */
export function ReservaForm({ chaleId, nomeChale, precoNoite }: ReservaFormProps) {
  const t = useTranslations("ReservaForm");
  const tCommon = useTranslations("Common");
  const tApiErrors = useTranslations("ApiErrors");
  const [estado, setEstado] = useState<Estado>("formulario");
  const [nomeHospede, setNomeHospede] = useState("");
  const [emailHospede, setEmailHospede] = useState("");
  const [dataCheckin, setDataCheckin] = useState("");
  const [quantidadeDiarias, setQuantidadeDiarias] = useState(1);
  const [mensagemErro, setMensagemErro] = useState("");
  const [reservaCriada, setReservaCriada] = useState<ReservaCriada | null>(null);

  const hoje = new Date().toISOString().slice(0, 10);
  const formularioValido = nomeHospede.trim() !== "" && emailHospede.trim() !== "" && dataCheckin !== "" && quantidadeDiarias >= 1;
  const valorTotalPreview = precoNoite * quantidadeDiarias;

  async function enviar() {
    if (!formularioValido || estado === "enviando") {
      return;
    }

    setEstado("enviando");

    try {
      const resultado = await criarReserva({
        chaleId,
        nomeHospede,
        emailHospede,
        dataCheckin,
        quantidadeDiarias,
      });
      setReservaCriada(resultado);
      setEstado("confirmada");
    } catch (erro) {
      // Campos do formulário NUNCA são limpos aqui, independente do tipo de erro (AC #3).
      if (erro instanceof ReservaError && erro.code === "RESERVATION_CONFLICT") {
        setMensagemErro(tApiErrors("reservationConflict"));
        setEstado("conflito");
      } else if (erro instanceof ReservaError) {
        setMensagemErro(apiErrorMessage(erro.code, tApiErrors, tCommon));
        setEstado("erro");
      } else {
        console.error(erro);
        setMensagemErro(tCommon("erroGenerico"));
        setEstado("erro");
      }
    }
  }

  if (estado === "confirmada" && reservaCriada) {
    return (
      <div className={styles.confirmacao} role="status">
        <p className={styles.confirmacaoTitulo}>{t("confirmacaoTitulo")}</p>
        <p className={styles.codigo}>{reservaCriada.codigoConsulta}</p>
        <p className="body">{t("horas48")}</p>
        <p className="body">
          {t.rich("valorTotal", {
            valor: precoFormatado.format(reservaCriada.valorTotal),
            strong: (chunks) => <strong>{chunks}</strong>,
          })}
        </p>
        <Link href={`/pagamento/${reservaCriada.codigoConsulta}`} className={styles.linkPagamento}>
          {t("irParaPagamento")}
        </Link>
        <Link href={`/consulta/${reservaCriada.codigoConsulta}`} className={styles.linkSecundario}>
          {t("consultarDepois")}
        </Link>
      </div>
    );
  }

  return (
    <form
      className={styles.form}
      onSubmit={(event) => {
        event.preventDefault();
        void enviar();
      }}
    >
      <label className={styles.field}>
        <span className={styles.fieldLabel}>{t("nomeCompleto")}</span>
        <input
          type="text"
          value={nomeHospede}
          onChange={(event) => setNomeHospede(event.target.value)}
          className={styles.textInput}
          required
        />
      </label>

      <label className={styles.field}>
        <span className={styles.fieldLabel}>{t("email")}</span>
        <input
          type="email"
          value={emailHospede}
          onChange={(event) => setEmailHospede(event.target.value)}
          className={styles.textInput}
          required
        />
      </label>

      <div className={styles.datesRow}>
        <label className={styles.field}>
          <span className={styles.fieldLabel}>{t("dataInicio")}</span>
          <input
            type="date"
            value={dataCheckin}
            min={hoje}
            onChange={(event) => setDataCheckin(event.target.value)}
            className={styles.dateInput}
            required
          />
        </label>

        <label className={styles.field}>
          <span className={styles.fieldLabel}>{t("quantidadeDiarias")}</span>
          <input
            type="number"
            min={1}
            value={quantidadeDiarias}
            onChange={(event) => {
              const valor = Number(event.target.value);
              setQuantidadeDiarias(Number.isNaN(valor) ? 1 : Math.max(1, valor));
            }}
            className={styles.numberInput}
            required
          />
        </label>
      </div>

      <p className={styles.previewValor}>
        {t("diarias", { count: quantidadeDiarias })} × {precoFormatado.format(precoNoite)} ={" "}
        <strong>{precoFormatado.format(valorTotalPreview)}</strong>
      </p>

      {(estado === "conflito" || estado === "erro") && (
        <p className={styles.statusMessage} role="alert">
          {mensagemErro}
          {estado === "conflito" && (
            <>
              {" "}
              <Link href="/" className={styles.linkOutrasOpcoes}>
                {t("verOutrosChales")}
              </Link>
            </>
          )}
        </p>
      )}

      <button type="submit" className={styles.submitButton} disabled={!formularioValido || estado === "enviando"}>
        {estado === "enviando" ? t("confirmando") : t("reservar", { nomeChale })}
      </button>
    </form>
  );
}
