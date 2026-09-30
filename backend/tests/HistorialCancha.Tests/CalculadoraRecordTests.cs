using HistorialCancha.Domain.Entidades;
using HistorialCancha.Domain.Estadisticas;
using Xunit;

// El Record del dominio y el Record de xUnit se llaman igual: el alias dice cuál es cuál.
using Record = HistorialCancha.Domain.Estadisticas.Record;

namespace HistorialCancha.Tests;

/// <summary>
/// FR14 — el récord de un conjunto de partidos y la efectividad sobre puntos en juego.
/// Es la cuenta de la que cuelgan todas las estadísticas de la app, así que si esto
/// miente, miente toda la pantalla.
/// </summary>
public class CalculadoraRecordTests
{
    private static Partido PartidoCon(int aFavor, int enContra) =>
        new() { Fecha = new DateOnly(2026, 5, 1), Rival = "River Plate", GolesAFavor = aFavor, GolesEnContra = enContra };

    /// <summary>Una victoria, un empate y una derrota: 4 puntos de 9 en juego.</summary>
    private static IEnumerable<Partido> TresPartidos() =>
        [PartidoCon(2, 1), PartidoCon(1, 1), PartidoCon(0, 3)];

    [Fact]
    public void SinPartidos_DevuelveElRecordVacio()
    {
        // Arrange — el caso borde que la app se come de verdad: un hincha recién
        // registrado entra a estadísticas con el historial vacío. Sin este camino,
        // la efectividad sería una división por cero.

        // Act
        var record = CalculadoraRecord.Calcular([]);

        // Assert — el récord vacío ya dice que todo está en cero; comprobarlo campo
        // por campo sería repetir lo que la propia constante define.
        Assert.Equal(Record.Vacio, record);
    }

    [Theory]
    [InlineData(2, 1, 1, 0, 0)]   // gana
    [InlineData(1, 1, 0, 1, 0)]   // empata
    [InlineData(0, 3, 0, 0, 1)]   // pierde
    public void ElResultadoSeDerivaDeLosGoles(int aFavor, int enContra,
        int ganados, int empatados, int perdidos)
    {
        // Arrange — el resultado no se guarda en la base: se deduce del marcador.
        var partidos = new[] { PartidoCon(aFavor, enContra) };

        // Act
        var record = CalculadoraRecord.Calcular(partidos);

        // Assert — un solo comportamiento (en qué casillero cae el partido), mirado
        // en los tres contadores porque son tres caras del mismo resultado.
        Assert.Equal(ganados, record.Ganados);
        Assert.Equal(empatados, record.Empatados);
        Assert.Equal(perdidos, record.Perdidos);
    }

    [Fact]
    public void Efectividad_EsPuntosObtenidosSobrePuntosEnJuego()
    {
        // Arrange — 4 puntos de 9 en juego = 44,4 %. Con el 3/1/0 mal puesto (por
        // ejemplo 2 puntos por victoria) el número cambia y este test lo agarra.

        // Act
        var record = CalculadoraRecord.Calcular(TresPartidos());

        // Assert
        Assert.Equal(44.4m, record.Efectividad);
    }

    [Fact]
    public void LosPuntosSonTresPorVictoriaYUnoPorEmpate()
    {
        // Act
        var record = CalculadoraRecord.Calcular(TresPartidos());

        // Assert
        Assert.Equal(4, record.Puntos);
    }

    [Fact]
    public void LosGolesSeAcumulanSobreTodosLosPartidos()
    {
        // Act
        var record = CalculadoraRecord.Calcular(TresPartidos());

        // Assert — 2+1+0 a favor, 1+1+3 en contra.
        Assert.Equal(3, record.GolesAFavor);
        Assert.Equal(5, record.GolesEnContra);
    }

    [Fact]
    public void LaDiferenciaDeGolEsLaResta()
    {
        // Act
        var record = CalculadoraRecord.Calcular(TresPartidos());

        // Assert
        Assert.Equal(-2, record.DiferenciaDeGol);
    }

    [Fact]
    public void LosPromediosDeGolSeRedondeanAUnDecimal()
    {
        // Act
        var record = CalculadoraRecord.Calcular(TresPartidos());

        // Assert — 3/3 da exacto; 5/3 es 1,666… y tiene que quedar en 1,7.
        Assert.Equal(1.0m, record.PromedioGolesAFavor);
        Assert.Equal(1.7m, record.PromedioGolesEnContra);
    }
}
