import { AdminLoginForm } from "@/components/AdminLoginForm";
import styles from "./page.module.css";

// Server Component estático — só lê o query param `motivo` (setado pelo redirect de
// app/admin/painel/page.tsx quando o JWT expira, AC #4) e repassa como prop pro Client Component.
// URL dedicada, não linkada na navegação do hóspede (EXPERIENCE.md, Information Architecture) —
// nenhum HeaderNav aqui, "duas superfícies isoladas".
export default async function AdminLoginPage({
  searchParams,
}: {
  searchParams: Promise<{ motivo?: string }>;
}) {
  const { motivo } = await searchParams;

  return (
    <main className={styles.main}>
      <h1 className={styles.title}>Login Administrativo</h1>
      <AdminLoginForm sessaoExpirada={motivo === "sessao-expirada"} />
    </main>
  );
}
