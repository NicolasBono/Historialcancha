using HistorialCancha.Domain;
using HistorialCancha.Domain.Entidades;
using Microsoft.EntityFrameworkCore;

namespace HistorialCancha.Infrastructure.Persistencia;

public class EquipoRepository : IEquipoRepository
{
    private readonly HistorialContext _contexto;

    public EquipoRepository(HistorialContext contexto) => _contexto = contexto;

    public async Task<IReadOnlyList<Equipo>> ObtenerActivosAsync(CancellationToken ct = default)
    {
        return await _contexto.Equipos
            .AsNoTracking()
            .Where(e => e.Activo)
            .OrderBy(e => e.Nombre)
            .ToListAsync(ct);
    }
}
