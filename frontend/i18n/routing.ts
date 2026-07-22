import { defineRouting } from "next-intl/routing";

// "as-needed" (não "always") — PT (padrão) continua em "/", "/faq", "/consulta/[codigo]" etc.
// sem prefixo, preservando os links já compartilhados nos fluxos de confirmação de reserva; só
// "/en/..."/"/es/..." levam prefixo. Também resolve o "idioma de fallback" (PRD §10.6, em
// aberto): sem prefixo reconhecido, cai no defaultLocale (pt) automaticamente.
export const routing = defineRouting({
  locales: ["pt", "en", "es"],
  defaultLocale: "pt",
  localePrefix: "as-needed",
  // Desliga o redirect automático por Accept-Language do navegador — com "as-needed", um link de
  // confirmação de reserva sem prefixo (ex.: /consulta/<codigo>) precisa abrir sempre igual pra
  // quem recebeu o link, independente do idioma do navegador de quem clica. Persistência de idioma
  // continua via cookie NEXT_LOCALE (comportamento padrão do next-intl), só a detecção automática
  // no primeiro acesso é que fica desligada.
  localeDetection: false,
});
