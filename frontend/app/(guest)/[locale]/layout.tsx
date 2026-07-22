import type { Metadata } from "next";
import { hasLocale, NextIntlClientProvider } from "next-intl";
import { getTranslations, setRequestLocale } from "next-intl/server";
import { notFound } from "next/navigation";
import { routing } from "@/i18n/routing";
import "../../globals.css";

export async function generateMetadata({
  params,
}: {
  params: Promise<{ locale: string }>;
}): Promise<Metadata> {
  const { locale } = await params;
  const t = await getTranslations({ locale, namespace: "Metadata" });
  return { title: t("title"), description: t("description") };
}

export function generateStaticParams() {
  return routing.locales.map((locale) => ({ locale }));
}

// Root layout do grupo (guest) — Next.js exige exatamente um layout definindo <html>/<body> por
// árvore renderizada; sem um root layout compartilhado (Story 2.1 apagou o antigo app/layout.tsx),
// cada grupo de rota de topo define o seu ((guest)/[locale] aqui, admin/ em app/admin/layout.tsx),
// o que separa naturalmente o app i18n-routed do admin PT-only.
export default async function LocaleLayout({
  children,
  params,
}: {
  children: React.ReactNode;
  params: Promise<{ locale: string }>;
}) {
  const { locale } = await params;

  if (!hasLocale(routing.locales, locale)) {
    notFound();
  }

  // Habilita o padrão de acesso a locale em not-found.tsx aninhados (chales/[id], consulta/[codigo],
  // pagamento/[codigo]) — sem isso, params não chega nesses arquivos especiais.
  setRequestLocale(locale);

  return (
    <html lang={locale}>
      <body>
        <NextIntlClientProvider>{children}</NextIntlClientProvider>
      </body>
    </html>
  );
}
