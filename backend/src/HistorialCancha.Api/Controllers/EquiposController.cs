using HistorialCancha.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HistorialCancha.Api.Controllers;

[ApiController]
[Route("api/equipos")]
public class EquiposController : ControllerBase
{
    private readonly IEquipoRepository _repositorio;
    private readonly OpcionesDominio _opciones;

    public EquiposController(IEquipoRepository repositorio, IOptions<OpcionesDominio> opciones)
    {
        _repositorio = repositorio;
        _opciones = opciones.Value;
    }

    /// <summary>
    /// Los rivales posibles. El equipo propio queda afuera de la lista: FR9 lo prohíbe
    /// como rival, y la forma más sana de aplicar una regla es que la opción inválida
    /// ni siquiera se pueda elegir. El validador la sigue aplicando igual, por si el
    /// alta no vino del selector.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var equipos = await _repositorio.ObtenerActivosAsync(ct);

        var rivales = equipos
            .Where(e => !string.Equals(e.Nombre.Trim(), _opciones.MiEquipo.Trim(),
                StringComparison.OrdinalIgnoreCase))
            .Select(e => e.Nombre)
            .ToList();

        return Ok(rivales);
    }
}
