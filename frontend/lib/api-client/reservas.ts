const API_BASE_URL = process.env.API_BASE_URL ?? "http://localhost:5122";

export interface CriarReservaPayload {
  chaleId: number;
  nomeHospede: string;
  emailHospede: string;
  dataCheckin: string;
  quantidadeDiarias: number;
}

export interface ReservaCriada {
  codigoConsulta: string;
  dataCheckin: string;
  dataCheckout: string;
  valorTotal: number;
  status: string;
}

export interface ReservaConsulta {
  codigoConsulta: string;
  status: string;
  expirada: boolean;
  nomeChale: string;
  dataCheckin: string;
  dataCheckout: string;
  valorTotal: number;
}

export interface ConfirmarPagamentoResultado {
  aprovado: boolean;
  status: string;
  mensagemRecusa: string | null;
}

export type ReservaErroCode =
  | "RESERVATION_CONFLICT"
  | "CHALE_NOT_FOUND"
  | "RESERVA_NOT_FOUND"
  | "RESERVA_EXPIRADA"
  | "VALIDATION_ERROR"
  | "RESERVA_UPSTREAM_UNAVAILABLE"
  | "UNKNOWN";

export class ReservaError extends Error {
  constructor(
    public readonly code: ReservaErroCode,
    message: string
  ) {
    super(message);
    this.name = "ReservaError";
  }
}

/** Extrai { code, message } do envelope AD-8 ({ error: { code, message } }) de uma resposta de erro. */
async function envelopeDoErro(response: Response, mensagemPadrao: string): Promise<ReservaError> {
  const data: unknown = await response.json().catch(() => null);
  const code =
    data && typeof data === "object" && "error" in data && data.error && typeof data.error === "object" && "code" in data.error
      ? (data.error.code as ReservaErroCode)
      : "UNKNOWN";
  const message =
    data && typeof data === "object" && "error" in data && data.error && typeof data.error === "object" && "message" in data.error
      ? String(data.error.message)
      : mensagemPadrao;

  return new ReservaError(code, message);
}

/**
 * Cria uma Reserva. Chamado client-side (disparado pela submissão do formulário), por isso
 * usa a rota same-origin `/api/reservas` (proxy Next.js — ver app/api/reservas/route.ts),
 * nunca a API .NET diretamente (AD-1). Lança `ReservaError` com o `code` do envelope de erro
 * (AD-8) pra `ReservaForm` distinguir conflito de outros tipos de falha.
 */
export async function criarReserva(payload: CriarReservaPayload): Promise<ReservaCriada> {
  let response: Response;
  try {
    response = await fetch("/api/reservas", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload),
      signal: AbortSignal.timeout(5000),
    });
  } catch {
    throw new ReservaError("RESERVA_UPSTREAM_UNAVAILABLE", "Não foi possível contatar a API.");
  }

  if (!response.ok) {
    throw await envelopeDoErro(response, `Falha ao criar reserva (status ${response.status}).`);
  }

  return (await response.json()) as ReservaCriada;
}

/**
 * Consulta uma Reserva pelo CodigoConsulta. Chamado sempre server-side (Server Component da
 * página de pagamento) — mesmo padrão de buscarChaleDetalhe (Story 1.3), sem passar pelo Route
 * Handler proxy, já que não é uma busca disparada por interação do usuário (AD-1). Retorna
 * `null` se o código não existir.
 */
export async function consultarReserva(codigo: string): Promise<ReservaConsulta | null> {
  const response = await fetch(`${API_BASE_URL}/api/reservas/${codigo}`, {
    cache: "no-store",
    signal: AbortSignal.timeout(5000),
  });

  if (response.status === 404) {
    return null;
  }

  if (!response.ok) {
    throw new Error(`Falha ao consultar a Reserva ${codigo} (status ${response.status}).`);
  }

  return (await response.json()) as ReservaConsulta;
}

/**
 * Confirma (ou simula recusa de) o pagamento de uma Reserva. Chamado client-side (disparado pela
 * submissão do PagamentoForm), por isso usa a rota same-origin
 * `/api/reservas/[codigo]/pagamento` (proxy Next.js), nunca a API .NET diretamente (AD-1).
 * Aprovado=false NÃO lança — é um resultado de negócio válido (recusa simulada), só falhas
 * HTTP reais (404/410/400/rede) lançam `ReservaError`.
 */
export async function confirmarPagamento(codigo: string, formaPagamento: string): Promise<ConfirmarPagamentoResultado> {
  let response: Response;
  try {
    response = await fetch(`/api/reservas/${codigo}/pagamento`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ formaPagamento }),
      signal: AbortSignal.timeout(5000),
    });
  } catch {
    throw new ReservaError("RESERVA_UPSTREAM_UNAVAILABLE", "Não foi possível contatar a API.");
  }

  if (!response.ok) {
    throw await envelopeDoErro(response, `Falha ao confirmar pagamento (status ${response.status}).`);
  }

  return (await response.json()) as ConfirmarPagamentoResultado;
}
