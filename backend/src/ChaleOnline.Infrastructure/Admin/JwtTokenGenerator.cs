using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ChaleOnline.Application.Admin;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ChaleOnline.Infrastructure.Admin;

/// <summary>
/// AD-5: validade fixa de 2h a partir da emissão, sem sliding window — decidida aqui, não
/// parametrizável pelo chamador.
/// </summary>
public class JwtTokenGenerator(IOptions<JwtOptions> jwtOptions) : IJwtTokenGenerator
{
    private static readonly TimeSpan Validade = TimeSpan.FromHours(2);

    public TokenGeradoDto Gerar(string adminId, string email)
    {
        var options = jwtOptions.Value;
        var expiraEmUtc = DateTime.UtcNow.Add(Validade);

        var claims = new[]
        {
            // JwtRegisteredClaimNames.Sub — a claim padrão que qualquer decodificador de JWT
            // reconhece; ClaimTypes.NameIdentifier mapeia pra uma URI XML-SOAP longa, não `sub`
            // (achado de code review, 2026-07-20). Mantido ao lado de ClaimTypes.NameIdentifier
            // porque é o que ClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier) já espera
            // em qualquer código futuro que venha a usá-lo.
            new Claim(JwtRegisteredClaimNames.Sub, adminId),
            new Claim(ClaimTypes.NameIdentifier, adminId),
            new Claim(ClaimTypes.Email, email),
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            claims: claims,
            expires: expiraEmUtc,
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return new TokenGeradoDto(tokenString, expiraEmUtc);
    }
}
