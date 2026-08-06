using System.Globalization;
using HistorialCancha.Domain.Entidades;

namespace HistorialCancha.Domain;

/// <summary>
/// Reglas de carga (FR7 a FR11). Función pura: no consulta la base, no conoce HTTP.
/// Todo lo que necesita saber del mundo exterior —la fecha de hoy, si ya hay un partido
/// ese día— entra como parámetro.
/// </summary>
public static class ValidadorPartido
{
    public static void Validar(
        Partido partido,
        bool existeOtroEnEsaFecha,
        DateOnly hoy,
        OpcionesDominio opciones)
    {
        ArgumentNullException.ThrowIfNull(partido);
        ArgumentNullException.ThrowIfNull(opciones);

        if (string.IsNullOrWhiteSpace(partido.Rival))
            throw new ReglaDeNegocioException("rival-requerido",
                "El rival es obligatorio.");

        if (string.IsNullOrWhiteSpace(partido.Torneo))
            throw new ReglaDeNegocioException("torneo-requerido",
                "El torneo es obligatorio.");

        // FR7 — goles no negativos.
        if (partido.GolesAFavor < 0 || partido.GolesEnContra < 0)
            throw new ReglaDeNegocioException("goles-negativos",
                "Los goles no pueden ser negativos.");

        // FR8 — fecha no futura.
        if (partido.Fecha > hoy)
            throw new ReglaDeNegocioException("fecha-futura",
                "La fecha del partido no puede ser posterior a hoy.");

        // FR9 — el rival no puede ser el propio equipo.
        if (EsElPropioEquipo(partido.Rival, opciones.MiEquipo))
            throw new ReglaDeNegocioException("rival-es-mi-equipo",
                $"El rival no puede ser tu propio equipo ({opciones.MiEquipo.Trim()}).");

        // FR10 — un solo partido por día.
        if (existeOtroEnEsaFecha)
            throw new ReglaDeNegocioException("fecha-duplicada",
                $"Ya hay un partido cargado el {partido.Fecha.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}.");

        // FR11 — nota entre 1 y 10.
        if (partido.Vivencia?.Nota is byte nota && (nota < 1 || nota > 10))
            throw new ReglaDeNegocioException("nota-fuera-de-rango",
                "La nota tiene que estar entre 1 y 10.");
    }

    private static bool EsElPropioEquipo(string rival, string miEquipo)
    {
        if (string.IsNullOrWhiteSpace(miEquipo)) return false;

        return string.Equals(rival.Trim(), miEquipo.Trim(), StringComparison.OrdinalIgnoreCase);
    }
}
