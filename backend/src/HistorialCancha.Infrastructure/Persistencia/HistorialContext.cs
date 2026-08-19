using HistorialCancha.Domain.Entidades;
using Microsoft.EntityFrameworkCore;

namespace HistorialCancha.Infrastructure.Persistencia;

public class HistorialContext : DbContext
{
    public HistorialContext(DbContextOptions<HistorialContext> options) : base(options) { }

    public DbSet<Partido> Partidos => Set<Partido>();
    public DbSet<Vivencia> Vivencias => Set<Vivencia>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HistorialContext).Assembly);
    }
}
