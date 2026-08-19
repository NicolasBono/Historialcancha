using HistorialCancha.Api.Dtos;
using HistorialCancha.Domain;
using HistorialCancha.Domain.Entidades;
using HistorialCancha.Infrastructure.Autenticacion;
using Microsoft.AspNetCore.Mvc;

namespace HistorialCancha.Api.Controllers;

/// <summary>
/// Registro y login. Único controller anónimo junto al health check:
/// todo lo demás exige token.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IHasheadorContrasenas _hasheador;
    private readonly IGeneradorTokens _tokens;

    public AuthController(
        IUsuarioRepository usuarios,
        IHasheadorContrasenas hasheador,
        IGeneradorTokens tokens)
    {
        _usuarios = usuarios;
        _hasheador = hasheador;
        _tokens = tokens;
    }

    /// <summary>Alta de un hincha y auto-login: devuelve el token ya listo.</summary>
    [HttpPost("registro")]
    public async Task<ActionResult<AuthResponse>> Registro(RegistroRequest request, CancellationToken ct)
    {
        var usuario = new Usuario
        {
            Nombre = request.Nombre?.Trim() ?? string.Empty,
            Apellido = request.Apellido?.Trim() ?? string.Empty,
            Dni = request.Dni?.Trim() ?? string.Empty
        };

        // El dominio no consulta la base: recibe el dato ya leído.
        var dniYaRegistrado = await _usuarios.ExisteDniAsync(usuario.Dni, ct);
        ValidadorUsuario.Validar(usuario, request.Contrasena, dniYaRegistrado);

        usuario.HashContrasena = _hasheador.Hashear(usuario, request.Contrasena!);
        await _usuarios.AgregarAsync(usuario, ct);

        // 201: la cuenta quedó creada. No hay endpoint GET del usuario al que apuntar,
        // así que se devuelve el token directo en el cuerpo, sin header Location.
        return StatusCode(StatusCodes.Status201Created, Emitir(usuario));
    }

    /// <summary>Login. Mensaje genérico ante DNI inexistente o clave incorrecta:
    /// no se revela cuál de los dos falló.</summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        var dni = request.Dni?.Trim() ?? string.Empty;
        var usuario = await _usuarios.ObtenerPorDniAsync(dni, ct);

        if (usuario is null
            || string.IsNullOrEmpty(request.Contrasena)
            || !_hasheador.Verificar(usuario, usuario.HashContrasena, request.Contrasena))
        {
            return Unauthorized(new { error = "DNI o contraseña incorrectos.", regla = "credenciales-invalidas" });
        }

        return Ok(Emitir(usuario));
    }

    private AuthResponse Emitir(Usuario usuario)
    {
        var token = _tokens.Generar(usuario);
        return new AuthResponse(token.Token, token.ExpiraEn, usuario.Nombre, usuario.Apellido);
    }
}
