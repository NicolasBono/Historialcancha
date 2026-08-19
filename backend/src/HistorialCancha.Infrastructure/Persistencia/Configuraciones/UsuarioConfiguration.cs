using HistorialCancha.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HistorialCancha.Infrastructure.Persistencia.Configuraciones;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Nombre)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(u => u.Apellido)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(u => u.Dni)
            .HasMaxLength(8)
            .IsRequired();

        // El DNI es la credencial de entrada: único e indexado para el login.
        builder.HasIndex(u => u.Dni)
            .IsUnique()
            .HasDatabaseName("UX_Usuarios_Dni");

        // Guarda el hash, nunca la contraseña. El largo cubre cualquier formato del hasher.
        builder.Property(u => u.HashContrasena)
            .HasMaxLength(256)
            .IsRequired();
    }
}
