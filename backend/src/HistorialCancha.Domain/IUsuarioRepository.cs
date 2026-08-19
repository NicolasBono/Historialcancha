using HistorialCancha.Domain.Entidades;

namespace HistorialCancha.Domain;

/// <summary>
/// El dominio declara qué necesita de la persistencia de usuarios;
/// la infraestructura decide cómo.
/// </summary>
public interface IUsuarioRepository
{
    /// <summary>Para el login: trae el usuario por su DNI, o null si no existe.</summary>
    Task<Usuario?> ObtenerPorDniAsync(string dni, CancellationToken ct = default);

    /// <summary>Para el registro: si ya hay alguien con ese DNI.</summary>
    Task<bool> ExisteDniAsync(string dni, CancellationToken ct = default);

    Task AgregarAsync(Usuario usuario, CancellationToken ct = default);
}
