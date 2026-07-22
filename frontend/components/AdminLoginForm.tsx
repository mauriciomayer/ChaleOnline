"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { login, AdminError } from "@/lib/api-client/admin";
import styles from "./AdminLoginForm.module.css";

type Estado = "formulario" | "enviando" | "erro";

interface AdminLoginFormProps {
  sessaoExpirada: boolean;
}

/**
 * Client Component — `<label>` associado nos campos e-mail/senha (UX-DR9). As mensagens de
 * "sessão expirada" (recebida via prop, setada pela página a partir do query param
 * `motivo=sessao-expirada`) e "credenciais inválidas"/"conta bloqueada" precisam ser visivelmente
 * diferentes na tela (EXPERIENCE.md, State Patterns) — nunca a mesma string.
 */
export function AdminLoginForm({ sessaoExpirada }: AdminLoginFormProps) {
  const router = useRouter();
  const [estado, setEstado] = useState<Estado>("formulario");
  const [email, setEmail] = useState("");
  const [senha, setSenha] = useState("");
  const [mensagemErro, setMensagemErro] = useState("");

  async function enviar() {
    if (estado === "enviando") {
      return;
    }

    setEstado("enviando");

    try {
      await login(email, senha);
      router.push("/admin/painel");
    } catch (erro) {
      if (erro instanceof AdminError && erro.code === "CREDENCIAIS_INVALIDAS") {
        setMensagemErro("E-mail ou senha inválidos.");
      } else if (erro instanceof AdminError && erro.code === "CONTA_BLOQUEADA_TEMPORARIAMENTE") {
        // Nunca expõe a contagem exata de tentativas restantes nem o tempo exato de desbloqueio
        // (EXPERIENCE.md, State Patterns — "Login malsucedido").
        setMensagemErro("Muitas tentativas de login. Tente novamente em alguns minutos.");
      } else {
        setMensagemErro("Não conseguimos concluir agora. Tente novamente em instantes.");
      }
      setEstado("erro");
    }
  }

  return (
    <form
      className={styles.form}
      onSubmit={(event) => {
        event.preventDefault();
        void enviar();
      }}
    >
      {sessaoExpirada && (
        <p className={styles.avisoSessaoExpirada} role="alert">
          Sua sessão expirou. Faça login novamente.
        </p>
      )}

      <label className={styles.field}>
        <span className={styles.fieldLabel}>E-mail</span>
        <input
          type="email"
          value={email}
          onChange={(event) => setEmail(event.target.value)}
          className={styles.textInput}
          autoComplete="username"
          required
        />
      </label>

      <label className={styles.field}>
        <span className={styles.fieldLabel}>Senha</span>
        <input
          type="password"
          value={senha}
          onChange={(event) => setSenha(event.target.value)}
          className={styles.textInput}
          autoComplete="current-password"
          required
        />
      </label>

      {estado === "erro" && (
        <p className={styles.erro} role="alert">
          {mensagemErro}
        </p>
      )}

      <button type="submit" className={styles.submitButton} disabled={estado === "enviando"}>
        {estado === "enviando" ? "Entrando..." : "Entrar"}
      </button>
    </form>
  );
}
