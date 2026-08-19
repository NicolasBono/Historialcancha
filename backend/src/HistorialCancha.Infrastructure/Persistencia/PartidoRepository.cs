using HistorialCancha.Domain;
using HistorialCancha.Domain.Entidades;
using Microsoft.EntityFrameworkCore;

namespace HistorialCancha.Infrastructure.Persistencia;

/// <summary>
/// Implementación del contrato que declara el dominio. Único lugar con EF Core.
/// Cada consulta filtra por <c>usuarioId</c>: el aislamiento entre hinchas vive acá.
/// </summary>
public class PartidoRepository : IPartidoRepository
{
    private readonly HistorialContext _contexto;

    public PartidoRepository(HistorialContext contexto) => _contexto = contexto;

    public async Task<IReadOnlyList<Partido>> ObtenerTodosAsync(int usuarioId, CancellationToken ct = default)
    {
        return await _contexto.Partidos
            .Where(p => p.UsuarioId == usuarioId)
            .Include(p => p.Vivencia)
            .AsNoTracking()
            .OrderByDescending(p => p.Fecha)
            .ToListAsync(ct);
    }

    public async Task<Partido?> ObtenerPorIdAsync(int id, int usuarioId, CancellationToken ct = default)
    {
        // Con seguimiento: este mismo objeto es el que se edita y se guarda.
        // El filtro por usuario es también la autorización: si el partido es de otro,
        // devuelve null y el controller responde 404 (no se filtra que existe).
        return await _contexto.Partidos
            .Include(p => p.Vivencia)
            .FirstOrDefaultAsync(p => p.Id == id && p.UsuarioId == usuarioId, ct);
    }

    public async Task<bool> ExisteEnFechaAsync(int usuarioId, DateOnly fecha, int? idAExcluir = null, CancellationToken ct = default)
    {
        return await _contexto.Partidos
            .AsNoTracking()
            .AnyAsync(p => p.UsuarioId == usuarioId
                && p.Fecha == fecha
                && (idAExcluir == null || p.Id != idAExcluir), ct);
    }

    public async Task AgregarAsync(Partido partido, CancellationToken ct = default)
    {
        _contexto.Partidos.Add(partido);
        await _contexto.SaveChangesAsync(ct);
    }

    public async Task GuardarCambiosAsync(CancellationToken ct = default)
    {
        await _contexto.SaveChangesAsync(ct);
    }

    public async Task<bool> EliminarAsync(int id, int usuarioId, CancellationToken ct = default)
    {
        var partido = await _contexto.Partidos
            .FirstOrDefaultAsync(p => p.Id == id && p.UsuarioId == usuarioId, ct);
        if (partido is null) return false;

        // La vivencia se va sola: la FK está configurada en cascada.
        _contexto.Partidos.Remove(partido);
        await _contexto.SaveChangesAsync(ct);
        return true;
    }
}
