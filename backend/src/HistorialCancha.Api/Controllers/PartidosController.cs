using HistorialCancha.Api.Dtos;
using HistorialCancha.Domain;
using HistorialCancha.Domain.Entidades;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HistorialCancha.Api.Controllers;

[ApiController]
[Route("api/partidos")]
public class PartidosController : ControladorAutenticado
{
    private readonly IPartidoRepository _repositorio;
    private readonly OpcionesDominio _opciones;

    public PartidosController(IPartidoRepository repositorio, IOptions<OpcionesDominio> opciones)
    {
        _repositorio = repositorio;
        _opciones = opciones.Value;
    }

    private static DateOnly Hoy => DateOnly.FromDateTime(DateTime.Today);

    /// <summary>FR4 — listado completo, del más reciente al más viejo.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PartidoResponse>>> Listar(CancellationToken ct)
    {
        var partidos = await _repositorio.ObtenerTodosAsync(UsuarioId, ct);
        return Ok(partidos.Select(PartidoMapper.AResponse));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PartidoResponse>> Obtener(int id, CancellationToken ct)
    {
        var partido = await _repositorio.ObtenerPorIdAsync(id, UsuarioId, ct);
        if (partido is null) return NoEncontrado(id);

        return Ok(PartidoMapper.AResponse(partido));
    }

    /// <summary>FR1, FR2, FR3 — alta del partido con su vivencia.</summary>
    [HttpPost]
    public async Task<ActionResult<PartidoResponse>> Crear(PartidoRequest request, CancellationToken ct)
    {
        var partido = new Partido { UsuarioId = UsuarioId, CreadoEn = DateTime.UtcNow };
        PartidoMapper.Volcar(request, partido);

        // El dominio no consulta la base: recibe el dato ya leído.
        var existeOtro = await _repositorio.ExisteEnFechaAsync(UsuarioId, partido.Fecha, null, ct);
        ValidadorPartido.Validar(partido, existeOtro, Hoy, _opciones);

        await _repositorio.AgregarAsync(partido, ct);

        return CreatedAtAction(nameof(Obtener), new { id = partido.Id },
            PartidoMapper.AResponse(partido));
    }

    /// <summary>FR5 — edición completa, revalidando todas las reglas.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, PartidoRequest request, CancellationToken ct)
    {
        var partido = await _repositorio.ObtenerPorIdAsync(id, UsuarioId, ct);
        if (partido is null) return NoEncontrado(id);

        PartidoMapper.Volcar(request, partido);

        // Se excluye a sí mismo: cambiar cualquier dato sin tocar la fecha no debe chocar.
        var existeOtro = await _repositorio.ExisteEnFechaAsync(UsuarioId, partido.Fecha, id, ct);
        ValidadorPartido.Validar(partido, existeOtro, Hoy, _opciones);

        await _repositorio.GuardarCambiosAsync(ct);
        return NoContent();
    }

    /// <summary>FR6 — baja del partido; la vivencia se borra en cascada.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id, CancellationToken ct)
    {
        var eliminado = await _repositorio.EliminarAsync(id, UsuarioId, ct);
        if (!eliminado) return NoEncontrado(id);

        return NoContent();
    }

    private ObjectResult NoEncontrado(int id) => StatusCode(StatusCodes.Status404NotFound,
        new { error = $"No existe un partido con id {id}.", regla = "no-encontrado" });
}
