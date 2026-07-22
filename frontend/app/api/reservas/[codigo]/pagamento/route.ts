const API_BASE_URL = process.env.API_BASE_URL ?? "http://localhost:5122";

function respostaUpstreamIndisponivel() {
  return Response.json(
    {
      error: {
        code: "RESERVA_UPSTREAM_UNAVAILABLE",
        message: "Não foi possível contatar a API.",
      },
    },
    { status: 502 }
  );
}

/**
 * Route Handler fino — só repassa o body pra API .NET (AD-1), mesmo padrão de
 * app/api/reservas/route.ts (Story 1.5). Existe pra que o PagamentoForm (client component)
 * tenha uma URL same-origin pra chamar a partir do navegador, sem CORS e sem expor
 * API_BASE_URL ao cliente.
 */
export async function POST(request: Request, { params }: { params: Promise<{ codigo: string }> }) {
  const { codigo } = await params;
  const bodyTexto = await request.text();
  const signal = AbortSignal.any([request.signal, AbortSignal.timeout(5000)]);

  let response: Response;
  let respostaTexto: string;
  try {
    response = await fetch(`${API_BASE_URL}/api/reservas/${codigo}/pagamento`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: bodyTexto,
      signal,
    });
    respostaTexto = await response.text();
  } catch {
    return respostaUpstreamIndisponivel();
  }

  return new Response(respostaTexto, {
    status: response.status,
    headers: {
      "Content-Type": response.headers.get("Content-Type") ?? "application/json",
    },
  });
}
