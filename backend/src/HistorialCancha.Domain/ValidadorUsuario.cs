using HistorialCancha.Domain.Entidades;

namespace HistorialCancha.Domain;

/// <summary>
/// Reglas de alta de un usuario. Función pura: no consulta la base, no conoce HTTP.
/// Si el DNI ya existe lo decide la infraestructura (necesita mirar la base) y entra
/// como parámetro, igual que "un partido por día".
/// </summary>
public static class ValidadorUsuario
{
    public const int LargoMinimoContrasena = 8;

    public static void Validar(Usuario usuario, string? contrasena, bool dniYaRegistrado)
    {
        ArgumentNullException.ThrowIfNull(usuario);

        if (string.IsNullOrWhiteSpace(usuario.Nombre))
            throw new ReglaDeNegocioException("nombre-requerido",
                "El nombre es obligatorio.");

        if (string.IsNullOrWhiteSpace(usuario.Apellido))
            throw new ReglaDeNegocioException("apellido-requerido",
                "El apellido es obligatorio.");

        if (string.IsNullOrWhiteSpace(usuario.Dni))
            throw new ReglaDeNegocioException("dni-requerido",
                "El DNI es obligatorio.");

        if (!EsDniValido(usuario.Dni))
            throw new ReglaDeNegocioException("dni-invalido",
                "El DNI tiene que ser un número de 7 u 8 dígitos.");

        if (string.IsNullOrWhiteSpace(contrasena) || contrasena.Length < LargoMinimoContrasena)
            throw new ReglaDeNegocioException("contrasena-debil",
                $"La contraseña tiene que tener al menos {LargoMinimoContrasena} caracteres.");

        if (dniYaRegistrado)
            throw new ReglaDeNegocioException("dni-duplicado",
                "Ya existe un usuario registrado con ese DNI.");
    }

    private static bool EsDniValido(string dni)
    {
        var limpio = dni.Trim();
        return limpio.Length is >= 7 and <= 8 && limpio.All(char.IsDigit);
    }
}
