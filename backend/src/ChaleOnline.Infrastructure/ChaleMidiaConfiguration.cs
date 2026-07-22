using ChaleOnline.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChaleOnline.Infrastructure;

public class ChaleMidiaConfiguration : IEntityTypeConfiguration<ChaleMidia>
{
    public void Configure(EntityTypeBuilder<ChaleMidia> builder)
    {
        builder.ToTable("ChaleMidia");

        builder.HasKey(midia => midia.Id);

        builder.Property(midia => midia.ChaleId)
            .IsRequired();
        builder.HasIndex(midia => midia.ChaleId);

        builder.Property(midia => midia.Url)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(midia => midia.Tipo)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(10);

        builder.Property(midia => midia.Ordem)
            .IsRequired();

        builder.HasOne<Chale>()
            .WithMany()
            .HasForeignKey(midia => midia.ChaleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(SeedData.ChaleMidias);
    }
}
