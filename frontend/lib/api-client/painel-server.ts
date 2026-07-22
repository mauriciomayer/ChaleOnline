import { cookies } from "next/headers";

const API_BASE_URL = process.env.API_BASE_URL ?? "http://localhost:5122";
const COOKIE_NAME = "chale_admin_token";

export interface VisaoDiariaChale {
  chaleId: number;
  nome: string;
  tipo: string;
  numeroQuartos: number;
  numeroBanheiros: number;
  estado: string;
  detalhe: string | null;
}

/**
 * Busca a Visão Diária de Ocupação server-side (AD-1) — mesmo padrão de `obterAdminAtual`
 * (`admin-server.ts`, Story 3.1): lê o cookie diretamente, chama a API .NET diretamente
 * (sem passar pelo proxy `/api/admin/...`), nunca lança — retorna `null` em qualquer falha
 * (sem cookie, sessão inválida, rede) pra página tratar. A checagem de sessão em si (distinguir
 * "não autenticado" de "sessão expirada") continua sendo responsabilidade exclusiva de
 * `obterAdminAtual()` — não duplicada aqui.
 */
export async function obterVisaoDiaria(): Promise<VisaoDiariaChale[] | null> {
  const cookieStore = await cookies();
  const token = cookieStore.get(COOKIE_NAME)?.value;

  if (!token) {
    return null;
  }

  let response: Response;
  try {
    response = await fetch(`${API_BASE_URL}/api/admin/visao-diaria`, {
      headers: { Authorization: `Bearer ${token}` },
      cache: "no-store",
      signal: AbortSignal.timeout(5000),
    });
  } catch {
    return null;
  }

  if (!response.ok) {
    return null;
  }

  return (await response.json()) as VisaoDiariaChale[];
}

export interface RelatorioMensalReserva {
  codigoConsulta: string;
  chaleNome: string;
  nomeHospede: string;
  dataCheckin: string;
  dataCheckout: string;
  valorTotal: number;
  status: string;
}

export interface RelatorioMensalResumo {
  quantidadeTotal: number;
  quantidadeCanceladas: number;
  totalValores: number;
}

export interface RelatorioMensal {
  reservas: RelatorioMensalReserva[];
  resumo: RelatorioMensalResumo;
}

/**
 * Busca o Relatório Mensal server-side (AD-1) — mesmo padrão exato de `obterVisaoDiaria` (lê o
 * cookie diretamente, chama a API .NET diretamente, nunca lança, retorna `null` em qualquer
 * falha de sessão/rede pra página tratar).
 */
export async function obterRelatorioMensal(ano: number, mes: number): Promise<RelatorioMensal | null> {
  const cookieStore = await cookies();
  const token = cookieStore.get(COOKIE_NAME)?.value;

  if (!token) {
    return null;
  }

  let response: Response;
  try {
    response = await fetch(`${API_BASE_URL}/api/admin/relatorio-mensal?ano=${ano}&mes=${mes}`, {
      headers: { Authorization: `Bearer ${token}` },
      cache: "no-store",
      signal: AbortSignal.timeout(5000),
    });
  } catch {
    return null;
  }

  if (!response.ok) {
    return null;
  }

  return (await response.json()) as RelatorioMensal;
}
