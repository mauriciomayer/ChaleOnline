import { cookies } from "next/headers";

const API_BASE_URL = process.env.API_BASE_URL ?? "http://localhost:5122";
const COOKIE_NAME = "chale_admin_token";

function respostaUpstreamIndisponivel() {
  return Response.json(
    {
      error: {
        code: "ADMIN_UPSTREAM_UNAVAILABLE",
        message: "Não foi possível contatar a API.",
      },
    },
    { status: 502 }
  );
}

/**
 * Route Handler fino — repassa o body pra API .NET (AD-1), mesmo padrão de
 * app/api/reservas/route.ts (Story 1.5), com a adição do `Set-Cookie` httpOnly no caminho de
 * sucesso: o token nunca chega a ficar acessível a JS no navegador (AD-5/Task 5 — decisão de
 * arquitetura, não escolha aberta, ver Dev Notes da Story 3.1).
 */
export async function POST(request: Request) {
  const bodyTexto = await request.text();
  const signal = AbortSignal.any([request.signal, AbortSignal.timeout(5000)]);

  let response: Response;
  let respostaTexto: string;
  try {
    response = await fetch(`${API_BASE_URL}/api/admin/login`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: bodyTexto,
      signal,
    });
    respostaTexto = await response.text();
  } catch {
    return respostaUpstreamIndisponivel();
  }

  if (response.ok) {
    let resultado: { token: string; expiraEmUtc: string };
    try {
      resultado = JSON.parse(respostaTexto) as { token: string; expiraEmUtc: string };
    } catch {
      return respostaUpstreamIndisponivel();
    }

    const cookieStore = await cookies();
    cookieStore.set(COOKIE_NAME, resultado.token, {
      httpOnly: true,
      secure: process.env.NODE_ENV === "production",
      sameSite: "lax",
      // Folga de 24h além da validade real do JWT (não o mesmo instante do `expiraEmUtc`) —
      // sem essa folga, o navegador para de enviar o cookie no exato momento em que o JWT
      // expira (RFC 6265 §5.4), e o `proxy.ts` (que só checa presença) trata isso como "nunca
      // logou" em vez de "sessão expirada" (AC #4). Com a folga, o cookie ainda chega no
      // `/admin/painel`, que valida o JWT de verdade contra o backend e SÓ ENTÃO decide entre
      // os dois textos distintos (achado de code review, 2026-07-20).
      expires: new Date(new Date(resultado.expiraEmUtc).getTime() + 24 * 60 * 60 * 1000),
      // path "/" (não "/admin", como o texto original da história sugeria) — /api/admin/me
      // também precisa receber este cookie do navegador, e path "/admin" não cobriria /api/admin.
      path: "/",
    });

    // Nunca devolve o token em texto puro no corpo da resposta ao navegador — o cookie httpOnly
    // já carrega o token; ecoar `resultado.token` aqui deixaria o valor acessível a qualquer JS
    // client-side (via `response.json()`), anulando a própria justificativa de mitigação contra
    // XSS do httpOnly (achado de code review, 2026-07-20).
    return Response.json({ expiraEmUtc: resultado.expiraEmUtc }, { status: response.status });
  }

  return new Response(respostaTexto, {
    status: response.status,
    headers: {
      "Content-Type": response.headers.get("Content-Type") ?? "application/json",
    },
  });
}
