using HistorialCancha.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HistorialCancha.Infrastructure.Persistencia.Configuraciones;

/// <summary>
/// Todo el mapeo vive acá: la entidad de dominio no lleva atributos de EF.
/// </summary>
public class PartidoConfiguration : IEntityTypeConfiguration<Partido>
{
    public void Configure(EntityTypeBuilder<Partido> builder)
    {
        builder.ToTable("Partidos", t =>
        {
            t.HasCheckConstraint("CK_Partidos_GolesAFavor", "\"GolesAFavor\" >= 0");
            t.HasCheckConstraint("CK_Partidos_GolesEnContra", "\"GolesEnContra\" >= 0");
            t.HasCheckConstraint("CK_Partidos_Condicion", "\"Condicion\" IN (0, 1)");
        });

        builder.HasKey(p => p.Id);

        builder.Property(p => p.UsuarioId).IsRequired();

        // Cada partido pertenece a un usuario; si se borra el usuario, se van sus partidos.
        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(p => p.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(p => p.Fecha)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(p => p.Rival)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(p => p.Torneo)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(p => p.Condicion)
            .HasConversion<byte>()
            .HasColumnType("smallint")
            .IsRequired();

        builder.Property(p => p.Estadio)
            .HasMaxLength(120);

        builder.Property(p => p.GolesAFavor).IsRequired();
        builder.Property(p => p.GolesEnContra).IsRequired();

        builder.Property(p => p.CreadoEn)
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("now()")
            .IsRequired();

        // Un partido por día POR USUARIO: garantiza en la base la regla FR10.
        // El índice es compuesto: dos hinchas distintos sí pueden tener un
        // partido el mismo día; el mismo hincha, no.
        builder.HasIndex(p => new { p.UsuarioId, p.Fecha })
            .IsUnique()
            .HasDatabaseName("UX_Partidos_Usuario_Fecha");

        builder.HasIndex(p => p.Rival)
            .HasDatabaseName("IX_Partidos_Rival");

        // Derivados: se calculan en el dominio, no se persisten.
        builder.Ignore(p => p.Resultado);
        builder.Ignore(p => p.DiferenciaDeGol);
    }
}
