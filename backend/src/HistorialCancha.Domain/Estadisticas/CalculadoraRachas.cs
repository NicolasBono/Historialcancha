using HistorialCancha.Domain.Entidades;

namespace HistorialCancha.Domain.Estadisticas;

public record Racha(int Longitud, DateOnly? Desde, DateOnly? Hasta, bool EnCurso)
{
    public static readonly Racha Ninguna = new(0, null, null, false);
}

public record RachasDeTipo(Racha Actual, Racha MasLarga);

public record ResumenRachas(
    RachasDeTipo Invicto,
    RachasDeTipo SinGanar,
    RachasDeTipo SinRecibirGoles);

/// <summary>
/// FR17 y FR18 — racha actual y racha histórica más larga.
/// Casos borde contemplados: historial vacío, un solo partido, racha todavía abierta.
/// </summary>
public static class CalculadoraRachas
{
    public static ResumenRachas Calcular(IEnumerable<Partido> partidos)
    {
        ArgumentNullException.ThrowIfNull(partidos);

        // Del más viejo al más nuevo: una racha es una secuencia temporal.
        var ordenados = partidos.OrderBy(p => p.Fecha).ToList();

        return new ResumenRachas(
            Resolver(ordenados, p => p.Resultado != Resultado.Derrota),
            Resolver(ordenados, p => p.Resultado != Resultado.Victoria),
            Resolver(ordenados, p => p.GolesEnContra == 0));
    }

    private static RachasDeTipo Resolver(IReadOnlyList<Partido> ordenados, Func<Partido, bool> cumple)
    {
        if (ordenados.Count == 0) return new RachasDeTipo(Racha.Ninguna, Racha.Ninguna);

        var masLarga = Racha.Ninguna;
        var corriendo = 0;
        var inicio = default(DateOnly);

        for (var i = 0; i < ordenados.Count; i++)
        {
            if (!cumple(ordenados[i]))
            {
                corriendo = 0;
                continue;
            }

            if (corriendo == 0) inicio = ordenados[i].Fecha;
            corriendo++;

            if (corriendo > masLarga.Longitud)
            {
                var llegaAlFinal = i == ordenados.Count - 1;
                masLarga = new Racha(corriendo, inicio, ordenados[i].Fecha, llegaAlFinal);
            }
        }

        // La actual se cuenta desde el último partido hacia atrás.
        var actual = 0;
        for (var i = ordenados.Count - 1; i >= 0 && cumple(ordenados[i]); i--) actual++;

        var rachaActual = actual == 0
            ? Racha.Ninguna
            : new Racha(actual, ordenados[^actual].Fecha, ordenados[^1].Fecha, true);

        return new RachasDeTipo(rachaActual, masLarga);
    }
}
