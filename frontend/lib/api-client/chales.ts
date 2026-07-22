const API_BASE_URL = process.env.API_BASE_URL ?? "http://localhost:5122";

export type TipoChale = "A" | "B" | "C";

export interface ChaleResumo {
  id: number;
  nome: string;
  tipo: TipoChale;
  numeroQuartos: number;
  numeroBanheiros: number;
  preco: number;
  fotoUrl: string;
}

export interface ChaleMidia {
  url: string;
  tipo: "Foto" | "Video";
  ordem: number;
}

export interface Avaliacao {
  nota: number;
  comentario: string;
}

export interface ChaleDetalhe {
  id: number;
  nome: string;
  tipo: TipoChale;
  numeroQuartos: number;
  numeroBanheiros: number;
  preco: number;
  midias: ChaleMidia[];
  comodidades: string[];
  avaliacoes: Avaliacao[];
}

/**
 * Lista o catálogo completo de Chalés. Chamado sempre server-side (Server
 * Component) — o Next.js nunca acessa o MySQL diretamente (AD-1).
 */
export async function listarChales(): Promise<ChaleResumo[]> {
  const response = await fetch(`${API_BASE_URL}/api/chales`, {
    cache: "no-store",
    signal: AbortSignal.timeout(5000),
  });

  if (!response.ok) {
    throw new Error(
      `Falha ao buscar o catálogo de Chalés (status ${response.status}).`
    );
  }

  const data: unknown = await response.json();

  if (!Array.isArray(data)) {
    throw new Error("Resposta inesperada da API ao buscar o catálogo de Chalés.");
  }

  return data as ChaleResumo[];
}

/**
 * Busca o detalhe de um Chalé (galeria, comodidades, avaliações). Chamado sempre
 * server-side (Server Component da página de detalhe) — mesmo padrão de
 * listarChales(), sem passar pelo Route Handler proxy, já que não é uma busca
 * disparada por interação do usuário (AD-1). Retorna `null` se o Chalé não existir.
 */
export async function buscarChaleDetalhe(id: number): Promise<ChaleDetalhe | null> {
  const response = await fetch(`${API_BASE_URL}/api/chales/${id}`, {
    cache: "no-store",
    signal: AbortSignal.timeout(5000),
  });

  if (response.status === 404) {
    return null;
  }

  if (!response.ok) {
    throw new Error(
      `Falha ao buscar o detalhe do Chalé ${id} (status ${response.status}).`
    );
  }

  return (await response.json()) as ChaleDetalhe;
}

/**
 * Busca chalés disponíveis por data + estrutura. Chamado client-side (o "Buscar"
 * do hero-searchbar), por isso usa a rota same-origin `/api/chales` (proxy fino
 * do Next.js — ver app/api/chales/route.ts), nunca a API .NET diretamente (AD-1).
 */
export async function buscarChalesDisponiveis(
  checkin: string,
  checkout: string,
  tipos: TipoChale[]
): Promise<ChaleResumo[]> {
  const params = new URLSearchParams({ checkin, checkout });
  for (const tipo of tipos) {
    params.append("tipos", tipo);
  }

  const response = await fetch(`/api/chales?${params.toString()}`, {
    signal: AbortSignal.timeout(5000),
  });

  if (!response.ok) {
    throw new Error(
      `Falha ao buscar chalés disponíveis (status ${response.status}).`
    );
  }

  const data: unknown = await response.json();

  if (!Array.isArray(data)) {
    throw new Error("Resposta inesperada da API ao buscar chalés disponíveis.");
  }

  return data as ChaleResumo[];
}
