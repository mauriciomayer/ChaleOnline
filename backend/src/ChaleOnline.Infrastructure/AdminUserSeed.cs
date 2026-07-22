using Microsoft.AspNetCore.Identity;

namespace ChaleOnline.Infrastructure;

/// <summary>
/// Conta única de administração (Antônio) — Story 3.1, FR9 ("login único, sem múltiplos perfis").
/// Decisão confirmada com Mauricio (2026-07-20): credencial fixa e documentada (não secreta em
/// configuração local) — é um projeto de portfólio, e quem estiver vendo o site deve conseguir
/// entrar no painel administrativo de verdade.
///
/// Credencial de demonstração:
///   E-mail: admin@chaleonline.com
///   Senha:  ChaleOnline@2026
///
/// A senha foi hasheada uma única vez via PasswordHasher&lt;IdentityUser&gt; standalone (não
/// precisa de UserManager/banco pra isso — é isso que permite semear via HasData de migration,
/// mesmo padrão já usado pra Chale/Avaliacao/etc. em SeedData.cs) e o resultado congelado como
/// constante abaixo — **nunca chamar HashPassword de novo aqui**: `PasswordHasher.HashPassword`
/// usa um salt aleatório a cada chamada, e como este é um `static readonly` reavaliado em todo
/// processo novo (inclusive `dotnet ef migrations add` de qualquer migration futura sem relação
/// com auth), um hash recalculado a cada vez faria o EF comparar contra o snapshot congelado e
/// emitir um `UpdateData` espúrio pro admin (achado de code review, 2026-07-20). NormalizedUserName/
/// NormalizedEmail são preenchidos manualmente em maiúsculas porque o seed não passa pelo
/// UserManager (que faria isso automaticamente); SecurityStamp/ConcurrencyStamp precisam de um
/// valor fixo não-nulo — o Identity exige isso mesmo pra uma linha semeada estaticamente.
/// </summary>
internal static class AdminUserSeed
{
    private const string Id = "8f2b1e2a-6f3a-4b8e-9a1c-2b6d7e9f0a11";
    private const string Email = "admin@chaleonline.com";

    // Hash de "ChaleOnline@2026" (a senha de demonstração documentada acima), gerado uma única
    // vez via PasswordHasher<IdentityUser>().HashPassword — mesmo valor já aplicado na migration
    // AddAdminIdentity e nas duas bases (chaleonline/chaleonline_test); não regenerar.
    private const string PasswordHashCongelado = "AQAAAAIAAYagAAAAEC5rPBHLiKsnG+JIV8jUibtQFCBBtT8uB5kt9SkrIiOGwX12lDodaGf4NObffB2T+g==";

    public static readonly IdentityUser Admin = new()
    {
        Id = Id,
        UserName = Email,
        NormalizedUserName = Email.ToUpperInvariant(),
        Email = Email,
        NormalizedEmail = Email.ToUpperInvariant(),
        EmailConfirmed = true,
        SecurityStamp = "7A1D9C3E-2F5B-4A6D-8E1C-3B9F0D2A4C61",
        ConcurrencyStamp = "1C4E7B2A-9D3F-4C6E-A1B8-5F2D9E0C7A33",
        LockoutEnabled = true,
        PasswordHash = PasswordHashCongelado,
    };
}
