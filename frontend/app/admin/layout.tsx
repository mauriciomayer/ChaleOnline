import type { Metadata } from "next";
import "../globals.css";

// Root layout próprio do admin — PT-only, sem next-intl (decisão já fechada da arquitetura,
// Epic 3). Consequência técnica de Story 2.1 ter apagado o root layout único compartilhado:
// cada grupo de rota de topo agora define o seu próprio <html>/<body>.
export const metadata: Metadata = {
  title: "Chalé Online — Admin",
  description: "Painel administrativo do Chalé Online.",
};

export default function AdminLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="pt">
      <body>{children}</body>
    </html>
  );
}
