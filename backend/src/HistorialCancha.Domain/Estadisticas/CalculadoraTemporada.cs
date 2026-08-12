using System.Globalization;

namespace HistorialCancha.Domain.Estadisticas;

/// <summary>
/// FR23 — a qué temporada pertenece una fecha. La temporada no es el año calendario:
/// arranca en el mes de corte configurado y termina el mes anterior del año siguiente.
/// </summary>
public static class CalculadoraTemporada
{
    /// <summary>
    /// El año en que arranca la temporada de esa fecha. Es el valor por el que se agrupa
    /// y por el que se ordena: el orden no depende nunca del texto de la etiqueta.
    /// </summary>
    public static int AnioDeInicio(DateOnly fecha, int mesInicio)
        => fecha.Month >= MesDeCorte(mesInicio) ? fecha.Year : fecha.Year - 1;

    /// <summary>
    /// La etiqueta que se muestra: <c>2024/25</c> con corte a mitad de año.
    /// Con corte en enero la temporada coincide con el año calendario y se nombra
    /// con un solo número: <c>2024</c>, porque "2024/25" sería mentira.
    /// </summary>
    public static string Etiquetar(DateOnly fecha, int mesInicio)
    {
        var anioInicio = AnioDeInicio(fecha, mesInicio);

        if (MesDeCorte(mesInicio) == 1)
            return anioInicio.ToString(CultureInfo.InvariantCulture);

        // Formato invariante: la etiqueta viaja en el JSON y no puede cambiar
        // según la cultura de la máquina donde corre.
        var anioFin = (anioInicio + 1) % 100;
        return anioInicio.ToString(CultureInfo.InvariantCulture)
            + "/"
            + anioFin.ToString("D2", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// El mes de corte que realmente se aplica. Un valor fuera de 1..12 no es configuración
    /// válida: en vez de reventar en tiempo de ejecución por un número mal tipeado, cae en
    /// enero. La API informa este valor —no el configurado— para que un error se vea.
    /// </summary>
    public static int MesDeCorte(int mesInicio) => mesInicio is >= 1 and <= 12 ? mesInicio : 1;
}
