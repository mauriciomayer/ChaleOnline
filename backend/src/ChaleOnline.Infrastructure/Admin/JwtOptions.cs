namespace ChaleOnline.Infrastructure.Admin;

/// <summary>Bindável via IConfiguration.GetSection("Jwt") — valores vêm de user-secrets, nunca de appsettings.json.</summary>
public class JwtOptions
{
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
}
