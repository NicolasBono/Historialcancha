using HistorialCancha.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HistorialCancha.Infrastructure.Persistencia.Configuraciones;

public class VivenciaConfiguration : IEntityTypeConfiguration<Vivencia>
{
    public void Configure(EntityTypeBuilder<Vivencia> builder)
    {
        builder.ToTable("Vivencias", t =>
        {
            t.HasCheckConstraint("CK_Vivencias_Modalidad", "\"Modalidad\" BETWEEN 0 AND 4");
            t.HasCheckConstraint("CK_Vivencias_Nota", "\"Nota\" IS NULL OR \"Nota\" BETWEEN 1 AND 10");
        });

        // La PK es también la FK: relación 1:1 estricta.
        builder.HasKey(v => v.PartidoId);

        builder.Property(v => v.PartidoId).ValueGeneratedNever();

        builder.Property(v => v.Modalidad)
            .HasConversion<byte>()
            .HasColumnType("smallint")
            .IsRequired();

        builder.Property(v => v.Sector).HasMaxLength(80);
        builder.Property(v => v.ConQuien).HasMaxLength(120);
        builder.Property(v => v.Nota).HasColumnType("smallint");

        builder.HasOne(v => v.Partido)
            .WithOne(p => p.Vivencia)
            .HasForeignKey<Vivencia>(v => v.PartidoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
