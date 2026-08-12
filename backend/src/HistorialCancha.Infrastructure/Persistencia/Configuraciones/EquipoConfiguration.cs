using HistorialCancha.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HistorialCancha.Infrastructure.Persistencia.Configuraciones;

public class EquipoConfiguration : IEntityTypeConfiguration<Equipo>
{
    /// <summary>
    /// Los clubes de Primera División. Van como <c>HasData</c> para que queden dentro de
    /// una migración versionada: la base se reconstruye de cero con los equipos adentro,
    /// sin un script suelto que alguien tenga que acordarse de correr (NFR5).
    /// Cuando cambie la categoría, se agrega otra migración; nunca un UPDATE a mano.
    /// </summary>
    private static readonly string[] PrimeraDivision =
    [
        "Aldosivi",
        "Argentinos Juniors",
        "Atlético Tucumán",
        "Banfield",
        "Barracas Central",
        "Belgrano",
        "Boca Juniors",
        "Central Córdoba (SdE)",
        "Defensa y Justicia",
        "Deportivo Riestra",
        "Estudiantes (LP)",
        "Gimnasia y Esgrima (LP)",
        "Godoy Cruz",
        "Huracán",
        "Independiente",
        "Independiente Rivadavia",
        "Instituto",
        "Lanús",
        "Newell's Old Boys",
        "Platense",
        "Racing Club",
        "River Plate",
        "Rosario Central",
        "San Lorenzo",
        "San Martín (SJ)",
        "Sarmiento",
        "Talleres",
        "Tigre",
        "Unión",
        "Vélez Sarsfield"
    ];

    public void Configure(EntityTypeBuilder<Equipo> builder)
    {
        builder.ToTable("Equipos");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Nombre)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(e => e.Activo)
            .HasDefaultValue(true)
            .IsRequired();

        // Dos veces el mismo club es exactamente lo que esta tabla existe para evitar.
        builder.HasIndex(e => e.Nombre)
            .IsUnique()
            .HasDatabaseName("UX_Equipos_Nombre");

        builder.HasData(PrimeraDivision.Select((nombre, i) => new Equipo
        {
            Id = i + 1,
            Nombre = nombre,
            Activo = true
        }));
    }
}
