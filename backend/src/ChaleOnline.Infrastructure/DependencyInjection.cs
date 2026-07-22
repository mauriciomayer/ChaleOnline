using ChaleOnline.Application.Admin;
using ChaleOnline.Application.Chales;
using ChaleOnline.Application.Reservas;
using ChaleOnline.Infrastructure.Admin;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChaleOnline.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ChaleOnlineDb")
            ?? throw new InvalidOperationException("Connection string 'ChaleOnlineDb' não configurada.");

        // Versão fixa (não AutoDetect) — evita round-trip extra ao banco na primeira resolução
        // e é determinística, consistente com o MySQL 8.4 pinado na arquitetura.
        var serverVersion = new MySqlServerVersion(new Version(8, 4, 9));

        services.AddDbContext<ChaleOnlineDbContext>(options =>
            options.UseMySql(connectionString, serverVersion));

        services.AddScoped<IChaleRepository, ChaleRepository>();
        services.AddScoped<IReservaRepository, ReservaRepository>();

        // AD-5 — valores de rate-limit (5 tentativas / 15 min) não estão quantificados em nenhum
        // lugar do PRD/arquitetura; escolha razoável desta história, documentada nos Dev Notes da
        // Story 3.1. Sem AddDefaultTokenProviders() — não há fluxo de reset de senha nesta história.
        services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.AllowedForNewUsers = true;
            })
            .AddEntityFrameworkStores<ChaleOnlineDbContext>();

        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.AddScoped<IAdminAuthenticator, IdentityAdminAuthenticator>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        return services;
    }
}
