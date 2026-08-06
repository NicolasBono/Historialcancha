using HistorialCancha.Domain;
using HistorialCancha.Infrastructure.Salud;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HistorialCancha.Api.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    private readonly IChequeoBaseDeDatos _chequeo;
    private readonly IHostEnvironment _entorno;
    private readonly IConfiguration _configuracion;
    private readonly OpcionesDominio _opciones;

    public HealthController(
        IChequeoBaseDeDatos chequeo,
        IHostEnvironment entorno,
        IConfiguration configuracion,
        IOptions<OpcionesDominio> opciones)
    {
        _chequeo = chequeo;
        _entorno = entorno;
        _configuracion = configuracion;
        _opciones = opciones.Value;
    }

    /// <summary>
    /// Estado del servicio. Responde 200 aunque la base esté caída:
    /// el campo baseDeDatos es el que informa el problema.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var baseOk = await _chequeo.RespondeAsync(ct);

        return Ok(new
        {
            status = "ok",
            version = _configuracion["App:Version"] ?? "desconocida",
            entorno = _entorno.EnvironmentName,
            baseDeDatos = baseOk ? "ok" : "error",
            miEquipo = _opciones.MiEquipo
        });
    }
}
