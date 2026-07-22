# Chalé Online — Web

Frontend Next.js 16 (App Router) do Chalé Online — projeto de portfólio de uma pousada fictícia de 12 chalés em Campos do Jordão. Este app nunca acessa o MySQL diretamente; ele só fala com a API .NET (`../src/ChaleOnline.Api`) via HTTP (ver AD-1 em `ARCHITECTURE-SPINE.md`).

## Rodando localmente

Pré-requisitos: Node.js 20+, a API .NET rodando (`dotnet run` em `src/ChaleOnline.Api`), e um MySQL 8.4 acessível (ver abaixo).

```bash
cp .env.example .env.local   # ajuste API_BASE_URL se a API não estiver em localhost:5122
npm install
npm run dev
```

Abra [http://localhost:3000](http://localhost:3000).

### Banco de dados (MySQL 8.4)

A API espera um MySQL 8.4 com um banco `chaleonline` migrado (`dotnet ef database update` a partir de `src/ChaleOnline.Api`) e a connection string configurada via `dotnet user-secrets` no projeto `ChaleOnline.Api` (chave `ConnectionStrings:ChaleOnlineDb`) — nunca commitada no repositório.

Se você não tem uma instância MySQL 8.4 rodando, qualquer uma serve (Windows service, Docker, instalação nativa) — não é necessário nenhum setup específico além de criar o banco e o usuário e apontar a connection string para ele.

## Estrutura

- `app/(guest)/` — rotas do hóspede (Home, e as que vierem nas próximas histórias)
- `app/admin/` — painel administrativo (Epic 3; autenticação implementada na Story 3.1, Visão Diária/Relatório Mensal chegam nas Stories 3.2/3.3)
- `lib/api-client/` — cliente HTTP tipado para a API .NET
- `components/` — componentes de UI compartilhados
- `public/media/` — mídia estática (fotos/placeholders dos chalés)

Identidade visual "Amanhecer Alpino" / "Refúgio Editorial" definida em `app/globals.css` como CSS custom properties.

## Painel administrativo (`/admin`)

Login em [http://localhost:3000/admin/login](http://localhost:3000/admin/login) com a credencial de demonstração (fixa e documentada de propósito — projeto de portfólio, não sistema com dados sensíveis):

- **E-mail:** `admin@chaleonline.com`
- **Senha:** `ChaleOnline@2026`

O JWT emitido tem validade fixa de 2h; após expirar, a próxima ação no painel redireciona pro login com aviso de sessão expirada.
