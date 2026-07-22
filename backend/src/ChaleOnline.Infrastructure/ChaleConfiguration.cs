using ChaleOnline.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChaleOnline.Infrastructure;

public class ChaleConfiguration : IEntityTypeConfiguration<Chale>
{
    public void Configure(EntityTypeBuilder<Chale> builder)
    {
        builder.ToTable("Chale");

        builder.HasKey(chale => chale.Id);

        builder.Property(chale => chale.Nome)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(chale => chale.Tipo)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(1);

        builder.Property(chale => chale.NumeroQuartos)
            .IsRequired();

        builder.Property(chale => chale.NumeroBanheiros)
            .IsRequired();

        builder.Property(chale => chale.Preco)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(chale => chale.FotoUrl)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasData(SeedData.Chales);
    }
}
