"use client";

import { useState } from "react";
import { useTranslations } from "next-intl";
import { useRouter, Link } from "@/i18n/navigation";
import { confirmarPagamento, ReservaError, type ReservaConsulta } from "@/lib/api-client/reservas";
import { apiErrorMessage } from "@/lib/i18n/apiErrorMessage";
import styles from "./PagamentoForm.module.css";

type Estado = "formulario" | "enviando" | "aprovado" | "recusado" | "erro";

interface PagamentoFormProps {
  codigo: string;
  reserva: ReservaConsulta;
}

const precoFormatado = new Intl.NumberFormat("pt-BR", {
  style: "currency",
  currency: "BRL",
});

// Valores enviados à API (não mudam com idioma) — rótulos exibidos vêm de t() dentro do componente.
// CartaoRecusadoTeste é a opção dedicada e explícita pra simular recusa (decisão confirmada
// 2026-07-20), rotulada sem esconder que é uma simulação.
const FORMAS_PAGAMENTO_VALORES = [
  { valor: "CartaoCredito", chave: "cartaoCredito" },
  { valor: "Pix", chave: "pix" },
  { valor: "Boleto", chave: "boleto" },
  { valor: "CartaoRecusadoTeste", chave: "cartaoTeste" },
] as const;

/**
 * Formulário de Pagamento (simulado) — Client Component. `aprovado` é um estado terminal (troca
 * o formulário pela confirmação); `recusado` e `erro` mostram a mensagem acima do formulário, que
 * continua visível pra permitir nova tentativa imediata dentro da janela de 48h (AC #6) sem exigir
 * um botão dedicado de "tentar novamente".
 */
export function PagamentoForm({ codigo, reserva }: PagamentoFormProps) {
  const t = useTranslations("PagamentoForm");
  const tCommon = useTranslations("Common");
  const tApiErrors = useTranslations("ApiErrors");
  const router = useRouter();
  const [estado, setEstado] = useState<Estado>("formulario");
  const [formaPagamento, setFormaPagamento] = useState<string>(FORMAS_PAGAMENTO_VALORES[0].valor);
  const [mensagem, setMensagem] = useState("");

  async function enviar() {
    if (estado === "enviando") {
      return;
    }

    setEstado("enviando");

    try {
      const resultado = await confirmarPagamento(codigo, formaPagamento);
      if (resultado.aprovado) {
        setEstado("aprovado");
      } else {
        setMensagem(resultado.mensagemRecusa ?? t("pagamentoRecusadoPadrao"));
        setEstado("recusado");
      }
    } catch (erro) {
      // Reserva expirou entre o carregamento da página e este envio (AC #5 exige a tela dedicada,
      // não um erro genérico) — manda pra lá em vez de mostrar a mensagem inline.
      if (erro instanceof ReservaError && erro.code === "RESERVA_EXPIRADA") {
        router.replace("/reserva-expirada");
        return;
      }

      if (erro instanceof ReservaError) {
        setMensagem(apiErrorMessage(erro.code, tApiErrors, tCommon));
      } else {
        console.error(erro);
        setMensagem(tCommon("erroGenerico"));
      }
      setEstado("erro");
    }
  }

  if (estado === "aprovado") {
    return (
      <div className={styles.confirmacao} role="status">
        <p className={styles.confirmacaoTitulo}>{t("pagamentoConfirmado")}</p>
        <p className="body">{t("emailConfirmacao")}</p>
        <Link href={`/consulta/${codigo}`} className={styles.linkConsulta}>
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
      <p className={styles.resumo}>
        {reserva.nomeChale} — <strong>{precoFormatado.format(reserva.valorTotal)}</strong>
      </p>

      <label className={styles.field}>
        <span className={styles.fieldLabel}>{t("formaPagamento")}</span>
        <select
          value={formaPagamento}
          onChange={(event) => setFormaPagamento(event.target.value)}
          className={styles.select}
        >
          {FORMAS_PAGAMENTO_VALORES.map((forma) => (
            <option key={forma.valor} value={forma.valor}>
              {t(forma.chave)}
            </option>
          ))}
        </select>
      </label>

      <p className={styles.aviso}>{t("aviso")}</p>

      {estado === "recusado" && (
        <p className={styles.recusado} role="alert">
          {mensagem} {t("tentarNovamenteJanela")}
        </p>
      )}

      {estado === "erro" && (
        <p className={styles.statusMessage} role="alert">
          {mensagem}
        </p>
      )}

      <button type="submit" className={styles.submitButton} disabled={estado === "enviando"}>
        {estado === "enviando" ? t("confirmando") : t("confirmar")}
      </button>
    </form>
  );
}
