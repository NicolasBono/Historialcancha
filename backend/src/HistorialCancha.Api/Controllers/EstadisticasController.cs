using HistorialCancha.Domain;
using HistorialCancha.Domain.Estadisticas;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HistorialCancha.Api.Controllers;

/// <summary>
/// Todos los cálculos ocurren en el dominio. Este controller sólo trae los partidos
/// de la base y se los pasa: no hay una sola cuenta acá adentro, ni en SQL.
/// </summary>
[ApiController]
[Route("api/estadisticas")]
public class EstadisticasController : ControladorAutenticado
{
    private readonly IPartidoRepository _repositorio;
    private readonly OpcionesDominio _opciones;

    public EstadisticasController(IPartidoRepository repositorio, IOptions<OpcionesDominio> opciones)
    {
        _repositorio = repositorio;
        _opciones = opciones.Value;
    }

    /// <summary>FR14, FR20 — récord global y promedios de gol.</summary>
    [HttpGet("global")]
    public async Task<IActionResult> Global(CancellationToken ct)
    {
        var partidos = await _repositorio.ObtenerTodosAsync(UsuarioId, ct);
        return Ok(CalculadoraRecord.Calcular(partidos));
    }

    /// <summary>FR15, FR16 — récord por modalidad y veredicto cábala / yeta.</summary>
    [HttpGet("modalidad")]
    public async Task<IActionResult> PorModalidad(CancellationToken ct)
    {
        var partidos = await _repositorio.ObtenerTodosAsync(UsuarioId, ct);
        return Ok(CalculadoraModalidad.Calcular(partidos, _opciones));
    }

    /// <summary>FR17, FR18 — rachas actuales e históricas.</summary>
    [HttpGet("rachas")]
    public async Task<IActionResult> Rachas(CancellationToken ct)
    {
        var partidos = await _repositorio.ObtenerTodosAsync(UsuarioId, ct);
        return Ok(CalculadoraRachas.Calcular(partidos));
    }

    /// <summary>FR19 — rival talismán y rival maldición.</summary>
    [HttpGet("rivales")]
    public async Task<IActionResult> Rivales(CancellationToken ct)
    {
        var partidos = await _repositorio.ObtenerTodosAsync(UsuarioId, ct);
        return Ok(RankingRivales.Resolver(partidos, _opciones));
    }
}
