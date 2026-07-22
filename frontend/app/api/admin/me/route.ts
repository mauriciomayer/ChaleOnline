import { cookies } from "next/headers";

const API_BASE_URL = process.env.API_BASE_URL ?? "http://localhost:5122";
const COOKIE_NAME = "chale_admin_token";

/**
 * Route Handler fino — lê o cookie httpOnly `chale_admin_token` da requisição recebida e
 * repassa como `Authorization: Bearer` pra API .NET (AD-1), pra uma futura chamada client-side
 * poder checar a sessão sem nunca ter acesso direto ao token. O placeholder `/admin/painel`
 * desta história não usa esta rota (ver Completion Notes) — chama a API .NET diretamente
 * server-side, mesmo padrão de `consultarReserva` (Story 1.6), já que `fetch` no servidor exige
 * URL absoluta e não encaminha cookies automaticamente pra uma rota relativa.
 */
export async function GET(request: Request) {
  const cookieStore = await cookies();
  const token = cookieStore.get(COOKIE_NAME)?.value;

  if (!token) {
    return new Response(null, { status: 401 });
  }

  let response: Response;
  try {
    response = await fetch(`${API_BASE_URL}/api/admin/me`, {
      headers: { Authorization: `Bearer ${token}` },
      signal: AbortSignal.any([request.signal, AbortSignal.timeout(5000)]),
    });
  } catch {
    return new Response(null, { status: 502 });
  }

  const respostaTexto = await response.text();
  return new Response(respostaTexto, {
    status: response.status,
    headers: {
      "Content-Type": response.headers.get("Content-Type") ?? "application/json",
    },
  });
}
