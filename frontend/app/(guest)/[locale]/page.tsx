import { HeaderNav } from "@/components/HeaderNav";
import { SearchFirstCatalog } from "@/components/SearchFirstCatalog";
import { listarChales } from "@/lib/api-client/chales";

// Vitrine completa (todos os chalés) carregada server-side e sempre visível — decisão de
// 2026-07-21: a busca por data continua funcionando exatamente como antes (client-side, via
// buscarChalesDisponiveis), mas deixa de ser a única forma de ver o catálogo. Falha aqui sobe
// pro error.tsx da Home (mesmo padrão de buscarChaleDetalhe na página de detalhe).
export default async function HomePage() {
  const todosChales = await listarChales();

  return (
    <>
      <HeaderNav />
      <main>
        <SearchFirstCatalog todosChales={todosChales} />
      </main>
    </>
  );
}
