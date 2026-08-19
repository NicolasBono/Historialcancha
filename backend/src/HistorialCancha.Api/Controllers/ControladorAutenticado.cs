using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HistorialCancha.Api.Controllers;

/// <summary>
/// Base de los controllers que operan sobre datos de un usuario. Exige token
/// (<c>[Authorize]</c>) y expone el id del dueño de la request, sacado del claim.
/// Ningún controller lee el UsuarioId de la URL ni del body: sale del token firmado.
/// </summary>
[Authorize]
public abstract class ControladorAutenticado : ControllerBase
{
    protected int UsuarioId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("El token no trae el id del usuario."));
}
