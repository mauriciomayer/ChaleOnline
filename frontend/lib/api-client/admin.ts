// Nunca inclui o token — o Route Handler de login (app/api/admin/login/route.ts) já grava o
// token no cookie httpOnly e nunca o ecoa de volta no corpo da resposta (achado de code review,
// 2026-07-20: devolver o token aqui anularia a mitigação anti-XSS do httpOnly).
export interface AdminLoginResultado {
  expiraEmUtc: string;
}

export type AdminErroCode = "CREDENCIAIS_INVALIDAS" | "CONTA_BLOQUEADA_TEMPORARIAMENTE" | "VALIDATION_ERROR" | "ADMIN_UPSTREAM_UNAVAILABLE" | "UNKNOWN";

export class AdminError extends Error {
  constructor(
    public readonly code: AdminErroCode,
    message: string
  ) {
    super(message);
    this.name = "AdminError";
  }
}

/** Extrai { code, message } do envelope AD-8 ({ error: { code, message } }) de uma resposta de erro. */
async function envelopeDoErro(response: Response, mensagemPadrao: string): Promise<AdminError> {
  const data: unknown = await response.json().catch(() => null);
  const code =
    data && typeof data === "object" && "error" in data && data.error && typeof data.error === "object" && "code" in data.error
      ? (data.error.code as AdminErroCode)
      : "UNKNOWN";
  const message =
    data && typeof data === "object" && "error" in data && data.error && typeof data.error === "object" && "message" in data.error
      ? String(data.error.message)
      : mensagemPadrao;

  return new AdminError(code, message);
}

/**
 * Autentica o admin. Chamado client-side (disparado pela submissão do AdminLoginForm), por isso
 * usa a rota same-origin `/api/admin/login` (proxy Next.js — ver app/api/admin/login/route.ts,
 * que também grava o cookie httpOnly em caso de sucesso), nunca a API .NET diretamente (AD-1).
 */
export async function login(email: string, senha: string): Promise<AdminLoginResultado> {
  let response: Response;
  try {
    response = await fetch("/api/admin/login", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ email, senha }),
      signal: AbortSignal.timeout(5000),
    });
  } catch {
    throw new AdminError("ADMIN_UPSTREAM_UNAVAILABLE", "Não foi possível contatar a API.");
  }

  if (!response.ok) {
    throw await envelopeDoErro(response, `Falha ao autenticar (status ${response.status}).`);
  }

  return (await response.json()) as AdminLoginResultado;
}
