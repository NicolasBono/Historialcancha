using HistorialCancha.Domain.Entidades;

namespace HistorialCancha.Domain.Estadisticas;

public record RecordRival(string Rival, Record Record);

public record ResumenRivales(
    RecordRival? Talisman,
    RecordRival? Maldicion,
    IReadOnlyList<RecordRival> Ranking,
    int UmbralAplicado);

/// <summary>
/// FR19 — rival talismán y rival maldición.
/// Sólo entran los rivales que superan el umbral mínimo de partidos configurado.
/// </summary>
public static class RankingRivales
{
    public static ResumenRivales Resolver(IEnumerable<Partido> partidos, OpcionesDominio opciones)
    {
        ArgumentNullException.ThrowIfNull(partidos);
        ArgumentNullException.ThrowIfNull(opciones);

        var umbral = Math.Max(1, opciones.MinPartidosRanking);

        // Un mismo rival escrito con distintas mayúsculas es el mismo rival.
        var ranking = partidos
            .GroupBy(p => p.Rival.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(grupo => new RecordRival(grupo.First().Rival.Trim(), CalculadoraRecord.Calcular(grupo)))
            .Where(r => r.Record.PartidosJugados >= umbral)
            // Un único orden: el talismán es el primero, la maldición el último.
            .OrderByDescending(r => r.Record.Efectividad)
            .ThenByDescending(r => r.Record.DiferenciaDeGol)
            .ThenByDescending(r => r.Record.PartidosJugados)
            .ThenBy(r => r.Rival, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new ResumenRivales(
            ranking.Count > 0 ? ranking[0] : null,
            // Con un solo rival en el ranking no hay contraste: no hay maldición.
            ranking.Count > 1 ? ranking[^1] : null,
            ranking,
            umbral);
    }
}
