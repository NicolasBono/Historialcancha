using HistorialCancha.Domain.Entidades;

namespace HistorialCancha.Api.Dtos;

/// <summary>
/// Lo que llega por HTTP. Vive en la API: el dominio nunca ve un tipo
/// que exista por razones de transporte.
/// Los strings son nullable a propósito: si el cliente omite un campo,
/// lo rechaza el validador de dominio con un mensaje claro, no un NullReference.
/// </summary>
public record PartidoRequest(
    DateOnly Fecha,
    string? Rival,
    string? Torneo,
    Condicion Condicion,
    string? Estadio,
    int GolesAFavor,
    int GolesEnContra,
    Modalidad Modalidad,
    string? Sector,
    string? ConQuien,
    byte? Nota);

public record PartidoResponse(
    int Id,
    DateOnly Fecha,
    string Rival,
    string Torneo,
    Condicion Condicion,
    string? Estadio,
    int GolesAFavor,
    int GolesEnContra,
    Resultado Resultado,
    int DiferenciaDeGol,
    Modalidad Modalidad,
    string? Sector,
    string? ConQuien,
    byte? Nota);

public static class PartidoMapper
{
    public static PartidoResponse AResponse(Partido p) => new(
        p.Id,
        p.Fecha,
        p.Rival,
        p.Torneo,
        p.Condicion,
        p.Estadio,
        p.GolesAFavor,
        p.GolesEnContra,
        p.Resultado,
        p.DiferenciaDeGol,
        p.Vivencia?.Modalidad ?? Modalidad.NoLoVi,
        p.Vivencia?.Sector,
        p.Vivencia?.ConQuien,
        p.Vivencia?.Nota);

    /// <summary>Vuelca el request sobre la entidad, normalizando los textos.</summary>
    public static void Volcar(PartidoRequest request, Partido destino)
    {
        destino.Fecha = request.Fecha;
        destino.Rival = request.Rival?.Trim() ?? string.Empty;
        destino.Torneo = request.Torneo?.Trim() ?? string.Empty;
        destino.Condicion = request.Condicion;
        destino.Estadio = Limpiar(request.Estadio);
        destino.GolesAFavor = request.GolesAFavor;
        destino.GolesEnContra = request.GolesEnContra;

        destino.Vivencia ??= new Vivencia();
        destino.Vivencia.Modalidad = request.Modalidad;
        destino.Vivencia.Sector = Limpiar(request.Sector);
        destino.Vivencia.ConQuien = Limpiar(request.ConQuien);
        destino.Vivencia.Nota = request.Nota;
    }

    /// <summary>Un opcional vacío es null, no cadena vacía.</summary>
    private static string? Limpiar(string? valor)
        => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
