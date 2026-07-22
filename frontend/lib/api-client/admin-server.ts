import { cookies } from "next/headers";

const API_BASE_URL = process.env.API_BASE_URL ?? "http://localhost:5122";
const COOKIE_NAME = "chale_admin_token";

export type SessaoAdmin =
  | { status: "autenticado"; email: string }
  | { status: "sem-cookie" }
  | { status: "token-invalido" }
  | { status: "indisponivel" };

/**
 * Confirma se a sessão do admin ainda é válida, chamando a API .NET diretamente server-side
 * (AD-1) — mesmo padrão de `consultarReserva` (Story 1.6): Server Component, sem passar pelo
 * Route Handler proxy (`fetch` no servidor exige URL absoluta e não encaminha cookies
 * automaticamente pra uma rota relativa, então o hop pelo proxy não traria benefício aqui).
 *
 * Distingue "sem-cookie" (nunca logou), "token-invalido" (cookie presente mas o backend recusou
 * — expirado ou adulterado) e "indisponivel" (rede/erro do backend, não relacionado à sessão) —
 * a página chamadora só deve mostrar a mensagem de "sessão expirada" (AC #4) no segundo caso;
 * tratar qualquer falha como sessão expirada (achado de code review, 2026-07-20) confundiria uma
 * instabilidade transitória do backend com uma sessão de verdade expirada. Também nunca deixa uma
 * falha de rede propagar como exceção não tratada (mesmo achado) — sempre retorna um status.
 *
 * Em arquivo separado de admin.ts (não server-only) — admin.ts é importado por
 * AdminLoginForm.tsx (Client Component), e `next/headers` não pode entrar no bundle do cliente.
 */
export async function obterAdminAtual(): Promise<SessaoAdmin> {
  const cookieStore = await cookies();
  const token = cookieStore.get(COOKIE_NAME)?.value;

  if (!token) {
    return { status: "sem-cookie" };
  }

  let response: Response;
  try {
    response = await fetch(`${API_BASE_URL}/api/admin/me`, {
      headers: { Authorization: `Bearer ${token}` },
      cache: "no-store",
      signal: AbortSignal.timeout(5000),
    });
  } catch {
    return { status: "indisponivel" };
  }

  if (response.status === 401) {
    return { status: "token-invalido" };
  }

  if (!response.ok) {
    return { status: "indisponivel" };
  }

  const admin = (await response.json()) as { email: string };
  return { status: "autenticado", email: admin.email };
}
