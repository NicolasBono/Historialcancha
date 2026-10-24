using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;

namespace HistorialCancha.Api.Controllers;

/// <summary>
/// TEMPORAL (TP9): un endpoint que falla a propósito, para ver un error llegar a Sentry
/// desde producción. Sin login, para poder llamarlo desde el navegador. Se saca al cerrar el TP.
/// </summary>
[ApiController]
[Route("api/falla")]
[ExcludeFromCodeCoverage]
public class FallaController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() =>
        throw new InvalidOperationException("Falla de prueba del TP9");
}
