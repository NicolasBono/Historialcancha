using HistorialCancha.Domain.Entidades;
using Microsoft.AspNetCore.Identity;

namespace HistorialCancha.Infrastructure.Autenticacion;

/// <summary>
/// Hashea y verifica contraseñas. Se apoya en <see cref="PasswordHasher{TUser}"/>
/// del framework (PBKDF2 con salt por contraseña), para no inventar criptografía.
/// El dominio no lo conoce: la contraseña se hashea en el borde.
/// </summary>
public interface IHasheadorContrasenas
{
    string Hashear(Usuario usuario, string contrasena);

    /// <summary>True si la contraseña coincide con el hash guardado.</summary>
    bool Verificar(Usuario usuario, string hashGuardado, string contrasena);
}

public class HasheadorContrasenas : IHasheadorContrasenas
{
    private readonly PasswordHasher<Usuario> _hasher = new();

    public string Hashear(Usuario usuario, string contrasena)
        => _hasher.HashPassword(usuario, contrasena);

    public bool Verificar(Usuario usuario, string hashGuardado, string contrasena)
    {
        var resultado = _hasher.VerifyHashedPassword(usuario, hashGuardado, contrasena);
        return resultado is PasswordVerificationResult.Success
            or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
