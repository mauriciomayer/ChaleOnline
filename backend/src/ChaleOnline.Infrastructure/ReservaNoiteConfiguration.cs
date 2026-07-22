using ChaleOnline.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChaleOnline.Infrastructure;

public class ReservaNoiteConfiguration : IEntityTypeConfiguration<ReservaNoite>
{
    public void Configure(EntityTypeBuilder<ReservaNoite> builder)
    {
        builder.ToTable("ReservaNoite");

        // Chave composta (ChaleId, Data) — é a própria constraint UNIQUE de anti-overbooking (AD-3),
        // não uma restrição adicional: não pode existir duas linhas para o mesmo Chalé na mesma noite.
        builder.HasKey(noite => new { noite.ChaleId, noite.Data });

        builder.Property(noite => noite.ReservaId)
            .IsRequired();

        // A PK composta (ChaleId, Data) não serve a busca por intervalo em Data sozinha
        // (Data é a coluna final da PK) — índice dedicado pra BuscarDisponiveisAsync.
        builder.HasIndex(noite => noite.Data);

        builder.HasOne<Chale>()
            .WithMany()
            .HasForeignKey(noite => noite.ChaleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Reserva>()
            .WithMany()
            .HasForeignKey(noite => noite.ReservaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
