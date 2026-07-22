using ChaleOnline.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChaleOnline.Infrastructure;

public class AvaliacaoConfiguration : IEntityTypeConfiguration<Avaliacao>
{
    public void Configure(EntityTypeBuilder<Avaliacao> builder)
    {
        builder.ToTable("Avaliacao");

        builder.HasKey(avaliacao => avaliacao.Id);

        builder.Property(avaliacao => avaliacao.ChaleId)
            .IsRequired();
        builder.HasIndex(avaliacao => avaliacao.ChaleId);

        builder.Property(avaliacao => avaliacao.Nota)
            .IsRequired();

        builder.Property(avaliacao => avaliacao.Comentario)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasOne<Chale>()
            .WithMany()
            .HasForeignKey(avaliacao => avaliacao.ChaleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(SeedData.Avaliacoes);
    }
}
