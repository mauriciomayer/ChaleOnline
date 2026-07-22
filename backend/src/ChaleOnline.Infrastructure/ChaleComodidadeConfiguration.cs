using ChaleOnline.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChaleOnline.Infrastructure;

public class ChaleComodidadeConfiguration : IEntityTypeConfiguration<ChaleComodidade>
{
    public void Configure(EntityTypeBuilder<ChaleComodidade> builder)
    {
        builder.ToTable("ChaleComodidade");

        builder.HasKey(comodidade => comodidade.Id);

        builder.Property(comodidade => comodidade.ChaleId)
            .IsRequired();
        builder.HasIndex(comodidade => comodidade.ChaleId);

        builder.Property(comodidade => comodidade.Nome)
            .IsRequired()
            .HasMaxLength(80);

        builder.HasOne<Chale>()
            .WithMany()
            .HasForeignKey(comodidade => comodidade.ChaleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(SeedData.ChaleComodidades);
    }
}
