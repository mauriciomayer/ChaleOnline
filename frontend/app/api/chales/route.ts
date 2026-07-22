const API_BASE_URL = process.env.API_BASE_URL ?? "http://localhost:5122";

function respostaUpstreamIndisponivel() {
  return Response.json(
    {
      error: {
        code: "SEARCH_UPSTREAM_UNAVAILABLE",
        message: "Não foi possível contatar a API.",
      },
    },
    { status: 502 }
  );
}

/**
 * Route Handler fino — só repassa a query string pra API .NET (AD-1: o Next.js
 * nunca acessa o MySQL nem reimplementa lógica de domínio). Existe pra que o
 * `SearchFirstCatalog` (client component) tenha uma URL same-origin pra chamar
 * a partir do navegador, sem CORS e sem expor API_BASE_URL ao cliente.
 */
export async function GET(request: Request) {
  const { search } = new URL(request.url);

  // Cancela a chamada upstream se o navegador cancelar a busca (ex.: nova busca disparada
  // antes da anterior terminar), além do timeout de 5s independente do proxy.
  const signal = AbortSignal.any([request.signal, AbortSignal.timeout(5000)]);

  let response: Response;
  let body: string;
  try {
    response = await fetch(`${API_BASE_URL}/api/chales${search}`, {
      cache: "no-store",
      signal,
    });
    body = await response.text();
  } catch {
    return respostaUpstreamIndisponivel();
  }

  return new Response(body, {
    status: response.status,
    headers: {
      "Content-Type": response.headers.get("Content-Type") ?? "application/json",
    },
  });
}
