using HistorialCancha.Domain.Entidades;

namespace HistorialCancha.Domain.Estadisticas;

/// <summary>
/// El récord de un conjunto de partidos. Sirve para el global, para una modalidad,
/// para un rival o para cualquier otro recorte.
/// </summary>
public record Record(
    int PartidosJugados,
    int Ganados,
    int Empatados,
    int Perdidos,
    int GolesAFavor,
    int GolesEnContra,
    int DiferenciaDeGol,
    int Puntos,
    decimal Efectividad,
    decimal PromedioGolesAFavor,
    decimal PromedioGolesEnContra)
{
    /// <summary>Sin partidos: todo en cero. La efectividad no se divide por cero.</summary>
    public static readonly Record Vacio = new(0, 0, 0, 0, 0, 0, 0, 0, 0m, 0m, 0m);
}

public static class CalculadoraRecord
{
    /// <summary>
    /// FR14 — efectividad como puntos obtenidos sobre puntos en juego (3/1/0).
    /// Función pura: entra una colección ya materializada, sale un resultado.
    /// </summary>
    public static Record Calcular(IEnumerable<Partido> partidos)
    {
        ArgumentNullException.ThrowIfNull(partidos);

        var ganados = 0;
        var empatados = 0;
        var perdidos = 0;
        var golesAFavor = 0;
        var golesEnContra = 0;
        var jugados = 0;

        foreach (var partido in partidos)
        {
            jugados++;

            switch (partido.Resultado)
            {
                case Resultado.Victoria: ganados++; break;
                case Resultado.Empate: empatados++; break;
                default: perdidos++; break;
            }

            golesAFavor += partido.GolesAFavor;
            golesEnContra += partido.GolesEnContra;
        }

        if (jugados == 0) return Record.Vacio;

        var puntos = ganados * 3 + empatados;
        var puntosEnJuego = jugados * 3;

        return new Record(
            jugados,
            ganados,
            empatados,
            perdidos,
            golesAFavor,
            golesEnContra,
            golesAFavor - golesEnContra,
            puntos,
            Redondear(puntos * 100m / puntosEnJuego),
            Redondear((decimal)golesAFavor / jugados),
            Redondear((decimal)golesEnContra / jugados));
    }

    private static decimal Redondear(decimal valor) => Math.Round(valor, 1, MidpointRounding.AwayFromZero);
}
