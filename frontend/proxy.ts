import createMiddleware from "next-intl/middleware";
import { NextResponse } from "next/server";
import type { NextRequest } from "next/server";
import { routing } from "./i18n/routing";

const COOKIE_NAME = "chale_admin_token";
const intlMiddleware = createMiddleware(routing);

/**
 * Nomeado `proxy.ts`, não `middleware.ts` — a partir do Next.js 16 Middleware foi renomeado pra
 * Proxy (mesma funcionalidade, ver node_modules/next/dist/docs/01-app/01-getting-started/16-proxy.md).
 * Next.js só permite um proxy.ts por projeto — compõe a checagem de auth do admin (Story 3.1) com
 * o roteamento de locale do next-intl (Story 2.1) num único arquivo/função: `/admin/**` nunca
 * passa pelo `next-intl` (admin é PT-only, decisão já fechada da arquitetura), qualquer outra
 * rota passa pelo `intlMiddleware`.
 */
export function proxy(request: NextRequest) {
  if (request.nextUrl.pathname.startsWith("/admin")) {
    if (request.nextUrl.pathname.startsWith("/admin/painel") && !request.cookies.has(COOKIE_NAME)) {
      // Checa só a PRESENÇA do cookie (AC #2 da Story 3.1, caso "nunca logou") — nunca decodifica
      // o JWT aqui; a validação de assinatura/expiração de verdade acontece no backend (AD-5). O
      // caso "token expirou no meio da sessão" é responsabilidade da página do painel.
      return NextResponse.redirect(new URL("/admin/login", request.url));
    }

    return NextResponse.next();
  }

  return intlMiddleware(request);
}

export const config = {
  // O padrão geral exclui qualquer segmento com ponto (arquivos estáticos) — isso deixaria
  // qualquer rota futura sob /admin/painel/** com extensão (ex.: exportação .csv) pular o proxy
  // inteiro, sem checagem de cookie nenhuma. Mantém /admin/painel/:path* como entrada própria,
  // sempre coberta independente de conter ponto.
  matcher: ["/admin/painel/:path*", "/((?!api|_next|.*\\..*).*)"],
};
