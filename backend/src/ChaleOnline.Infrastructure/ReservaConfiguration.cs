using ChaleOnline.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ChaleOnline.Infrastructure;

public class ReservaConfiguration : IEntityTypeConfiguration<Reserva>
{
    // MySQL/Pomelo não guarda timezone — sem isso, toda leitura volta com Kind=Unspecified e
    // fica sujeita a interpretação errada. CriadoEm é a âncora do auto-cancelamento de 48h
    // (Story 1.6): precisa ser UTC de forma garantida, não por convenção do chamador.
    private static readonly ValueConverter<DateTime, DateTime> UtcConverter = new(
        v => v.Kind == DateTimeKind.Utc ? v : v.ToUniversalTime(),
        v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

    public void Configure(EntityTypeBuilder<Reserva> builder)
    {
        builder.ToTable("Reserva");

        builder.HasKey(reserva => reserva.Id);

        builder.Property(reserva => reserva.CodigoConsulta)
            .IsRequired();
        builder.HasIndex(reserva => reserva.CodigoConsulta)
            .IsUnique();

        builder.Property(reserva => reserva.ChaleId)
            .IsRequired();

        builder.Property(reserva => reserva.NomeHospede)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(reserva => reserva.EmailHospede)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(reserva => reserva.DataCheckin)
            .IsRequired();

        builder.Property(reserva => reserva.DataCheckout)
            .IsRequired();

        builder.Property(reserva => reserva.ValorTotal)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(reserva => reserva.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(reserva => reserva.CriadoEm)
            .IsRequired()
            .HasConversion(UtcConverter);

        builder.HasOne<Chale>()
            .WithMany()
            .HasForeignKey(reserva => reserva.ChaleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
