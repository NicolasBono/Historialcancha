using System.Globalization;
using HistorialCancha.Domain.Entidades;

namespace HistorialCancha.Domain.Estadisticas;

public enum Veredicto { Cabala, Yeta, Indefinido }

public record RecordPorModalidad(Modalidad Modalidad, Record Record);

public record ResumenModalidad(
    IReadOnlyList<RecordPorModalidad> PorModalidad,
    Record EnCancha,
    Record PorOtroMedio,
    Veredicto Veredicto,
    string Explicacion);

/// <summary>
/// FR15 y FR16 — el corazón de la app: el récord por modalidad y el veredicto
/// cábala / yeta comparando la cancha contra el resto de los medios.
/// </summary>
public static class CalculadoraModalidad
{
    /// <summary>
    /// "No lo vi" queda afuera de la comparación: no hubo experiencia que comparar.
    /// Sigue contando para el récord global.
    /// </summary>
    private static readonly Modalidad[] OtrosMedios =
        [Modalidad.TV, Modalidad.Streaming, Modalidad.Radio];

    public static ResumenModalidad Calcular(IEnumerable<Partido> partidos, OpcionesDominio opciones)
    {
        ArgumentNullException.ThrowIfNull(partidos);
        ArgumentNullException.ThrowIfNull(opciones);

        var lista = partidos as IReadOnlyList<Partido> ?? partidos.ToList();

        var porModalidad = Enum.GetValues<Modalidad>()
            .Select(m => new RecordPorModalidad(m, CalculadoraRecord.Calcular(De(lista, m))))
            .ToList();

        var enCancha = CalculadoraRecord.Calcular(De(lista, Modalidad.EnCancha));
        var porOtroMedio = CalculadoraRecord.Calcular(
            lista.Where(p => p.Vivencia is not null && OtrosMedios.Contains(p.Vivencia.Modalidad)));

        var (veredicto, explicacion) = Dictaminar(enCancha, porOtroMedio, opciones);

        return new ResumenModalidad(porModalidad, enCancha, porOtroMedio, veredicto, explicacion);
    }

    private static IEnumerable<Partido> De(IEnumerable<Partido> partidos, Modalidad modalidad)
        => partidos.Where(p => p.Vivencia is not null && p.Vivencia.Modalidad == modalidad);

    private static (Veredicto, string) Dictaminar(Record enCancha, Record otroMedio, OpcionesDominio opciones)
    {
        var minimo = Math.Max(1, opciones.MinPartidosVeredicto);

        if (enCancha.PartidosJugados < minimo || otroMedio.PartidosJugados < minimo)
            return (Veredicto.Indefinido,
                $"Hacen falta al menos {minimo} partidos en cancha y {minimo} por otro medio para " +
                $"que el veredicto signifique algo. Llevás {enCancha.PartidosJugados} y {otroMedio.PartidosJugados}.");

        var diferencia = enCancha.Efectividad - otroMedio.Efectividad;
        var umbral = opciones.UmbralVeredictoPuntos;

        if (diferencia >= umbral)
            return (Veredicto.Cabala,
                $"Cuando vas a la cancha el equipo rinde {Redondear(diferencia)} puntos de efectividad " +
                $"más que cuando lo seguís por otro medio.");

        if (diferencia <= -umbral)
            return (Veredicto.Yeta,
                $"Cuando vas a la cancha el equipo rinde {Redondear(-diferencia)} puntos de efectividad " +
                $"menos que cuando lo seguís por otro medio.");

        return (Veredicto.Indefinido,
            $"La diferencia es de apenas {Redondear(Math.Abs(diferencia))} puntos de efectividad, " +
            $"y hacen falta {umbral} para cantar victoria.");
    }

    /// <summary>
    /// Formato invariante a propósito: los números que van en el JSON se serializan con
    /// punto decimal, y estos textos tienen que coincidir con ellos. Además, así el
    /// resultado no cambia según la cultura de la máquina donde corre.
    /// </summary>
    private static string Redondear(decimal valor)
        => Math.Round(valor, 1, MidpointRounding.AwayFromZero).ToString("0.#", CultureInfo.InvariantCulture);
}
