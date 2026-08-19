using HistorialCancha.Domain;
using HistorialCancha.Domain.Entidades;
using Microsoft.EntityFrameworkCore;

namespace HistorialCancha.Infrastructure.Persistencia;

/// <summary>
/// Implementación del contrato que declara el dominio. Único lugar con EF Core.
/// </summary>
public class UsuarioRepository : IUsuarioRepository
{
    private readonly HistorialContext _contexto;

    public UsuarioRepository(HistorialContext contexto) => _contexto = contexto;

    public async Task<Usuario?> ObtenerPorDniAsync(string dni, CancellationToken ct = default)
    {
        return await _contexto.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Dni == dni, ct);
    }

    public async Task<bool> ExisteDniAsync(string dni, CancellationToken ct = default)
    {
        return await _contexto.Usuarios
            .AsNoTracking()
            .AnyAsync(u => u.Dni == dni, ct);
    }

    public async Task AgregarAsync(Usuario usuario, CancellationToken ct = default)
    {
        _contexto.Usuarios.Add(usuario);
        await _contexto.SaveChangesAsync(ct);
    }
}
