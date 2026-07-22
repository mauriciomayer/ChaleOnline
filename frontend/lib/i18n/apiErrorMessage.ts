import type { useTranslations } from "next-intl";
import type { ReservaErroCode } from "@/lib/api-client/reservas";

type Translator = ReturnType<typeof useTranslations>;

// RESERVA_EXPIRADA não entra aqui — tem tratamento dedicado (redirect pra /reserva-expirada),
// nunca vira texto exibido inline (ver PagamentoForm.tsx).
//
// RESERVATION_CONFLICT só é alcançável hoje via POST /api/reservas (criarReserva) — ReservaForm.tsx
// intercepta esse code antes de chamar este helper (estado "conflito" próprio, com link "Ver outros
// chalés"), então a entrada abaixo nunca é exercitada por ele; fica mapeada mesmo assim porque
// POST /api/reservas/{codigo}/pagamento nunca retorna esse code hoje, mas o mapa existe pra cobrir
// qualquer chamador futuro do helper sem tratamento especial dedicado.
const CODE_TO_KEY: Partial<Record<ReservaErroCode, string>> = {
  RESERVATION_CONFLICT: "reservationConflict",
  CHALE_NOT_FOUND: "chaleNotFound",
  RESERVA_NOT_FOUND: "reservaNotFound",
  VALIDATION_ERROR: "validationError",
  RESERVA_UPSTREAM_UNAVAILABLE: "upstreamUnavailable",
};

/**
 * Traduz um `ReservaErroCode` (envelope AD-8) pra texto exibível. `code` desconhecido/não mapeado
 * (inclui "UNKNOWN") cai no fallback genérico de `Common` — nunca deixa a tela sem mensagem nem
 * mostra o código bruto (AC #2), e evita duplicar o texto genérico num namespace próprio.
 */
export function apiErrorMessage(code: ReservaErroCode, t: Translator, tCommon: Translator): string {
  const key = CODE_TO_KEY[code];
  return key ? t(key) : tCommon("erroGenerico");
}
