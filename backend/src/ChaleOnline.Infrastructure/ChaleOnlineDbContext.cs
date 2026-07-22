using ChaleOnline.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ChaleOnline.Infrastructure;

// IdentityDbContext<IdentityUser> (Story 3.1, AD-5) — traz as tabelas padrão do ASP.NET Core
// Identity (AspNetUsers, AspNetRoles, etc.) pro mesmo banco, sem afetar as entidades já
// existentes. "Administrador" é uma conta standalone do Identity, fora do grafo de domínio de
// reservas (ARCHITECTURE-SPINE.md, notas do ERD).
public class ChaleOnlineDbContext(DbContextOptions<ChaleOnlineDbContext> options) : IdentityDbContext<IdentityUser>(options)
{
    public DbSet<Chale> Chales => Set<Chale>();
    public DbSet<Reserva> Reservas => Set<Reserva>();
    public DbSet<ReservaNoite> ReservaNoites => Set<ReservaNoite>();
    public DbSet<Avaliacao> Avaliacoes => Set<Avaliacao>();
    public DbSet<ChaleMidia> ChaleMidias => Set<ChaleMidia>();
    public DbSet<ChaleComodidade> ChaleComodidades => Set<ChaleComodidade>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // base primeiro — configura as tabelas do Identity (AspNetUsers etc.) antes de qualquer
        // configuração própria que também toque IdentityUser (ex.: AdminUserConfiguration.HasData).
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ChaleOnlineDbContext).Assembly);
    }
}
