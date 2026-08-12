using HistorialCancha.Domain.Entidades;

namespace HistorialCancha.Domain.Estadisticas;

public record RecordPorCondicion(Condicion Condicion, Record Record);

public record RecordPorTorneo(string Torneo, Record Record);

public record RecordPorTemporada(string Temporada, Record Record);

public record ResumenDesgloses(
    IReadOnlyList<RecordPorCondicion> PorCondicion,
    IReadOnlyList<RecordPorTorneo> PorTorneo,
    IReadOnlyList<RecordPorTemporada> PorTemporada,
    int MesDeCorteAplicado);

/// <summary>
/// FR21, FR22 y FR23 — el mismo récord cortado por condición, por torneo y por temporada.
/// Ningún corte recalcula nada: todos delegan en <see cref="CalculadoraRecord"/>.
/// </summary>
public static class CalculadoraDesgloses
{
    public static ResumenDesgloses Calcular(IEnumerable<Partido> partidos, OpcionesDominio opciones)
    {
        ArgumentNullException.ThrowIfNull(partidos);
        ArgumentNullException.ThrowIfNull(opciones);

        var lista = partidos as IReadOnlyList<Partido> ?? partidos.ToList();
        var mesDeCorte = CalculadoraTemporada.MesDeCorte(opciones.MesInicioTemporada);

        return new ResumenDesgloses(
            PorCondicion(lista),
            PorTorneo(lista),
            PorTemporada(lista, mesDeCorte),
            mesDeCorte);
    }

    /// <summary>
    /// Local y Visitante aparecen siempre, aunque alguna esté en cero: la gracia del
    /// desglose es el contraste, y una fila ausente no se contrasta con nada.
    /// </summary>
    private static List<RecordPorCondicion> PorCondicion(IReadOnlyList<Partido> partidos)
        => Enum.GetValues<Condicion>()
            .Select(c => new RecordPorCondicion(
                c,
                CalculadoraRecord.Calcular(partidos.Where(p => p.Condicion == c))))
            .ToList();

    /// <summary>
    /// Un torneo sin partidos no existe como grupo, así que nunca aparece vacío.
    /// Un mismo torneo escrito con distintas mayúsculas es el mismo torneo, igual que
    /// los rivales en <see cref="RankingRivales"/>.
    /// </summary>
    private static List<RecordPorTorneo> PorTorneo(IReadOnlyList<Partido> partidos)
        => partidos
            .GroupBy(p => p.Torneo.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(grupo => new RecordPorTorneo(
                grupo.First().Torneo.Trim(),
                CalculadoraRecord.Calcular(grupo)))
            .OrderByDescending(t => t.Record.Efectividad)
            .ThenByDescending(t => t.Record.DiferenciaDeGol)
            .ThenByDescending(t => t.Record.PartidosJugados)
            .ThenBy(t => t.Torneo, StringComparer.OrdinalIgnoreCase)
            .ToList();

    /// <summary>
    /// De la más reciente a la más antigua. Se agrupa por el año de inicio y no por la
    /// etiqueta: ordenar "2024/25" como texto funciona de casualidad y deja de funcionar
    /// en cuanto cambia el formato.
    /// </summary>
    private static List<RecordPorTemporada> PorTemporada(IReadOnlyList<Partido> partidos, int mesInicio)
        => partidos
            .GroupBy(p => CalculadoraTemporada.AnioDeInicio(p.Fecha, mesInicio))
            .OrderByDescending(grupo => grupo.Key)
            .Select(grupo => new RecordPorTemporada(
                CalculadoraTemporada.Etiquetar(grupo.First().Fecha, mesInicio),
                CalculadoraRecord.Calcular(grupo)))
            .ToList();
}
