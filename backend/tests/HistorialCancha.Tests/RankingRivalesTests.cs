using HistorialCancha.Domain;
using HistorialCancha.Domain.Entidades;
using HistorialCancha.Tests.Ayudas;
using Xunit;

namespace HistorialCancha.Tests;

/// <summary>
/// FR19 — rival talismán y rival maldición.
///
/// Lo que se prueba acá es sobre todo el CRITERIO: a quién se deja entrar al ranking,
/// cómo se agrupa un mismo rival escrito de dos formas, y cómo se desempata. Un
/// ranking mal ordenado no tira ningún error: muestra al rival equivocado como
/// talismán y nadie se entera.
/// </summary>
public class RankingRivalesTests
{
    private static OpcionesDominio Opciones(int minPartidos = 2) =>
        new() { MiEquipo = "Boca Juniors", MinPartidosRanking = minPartidos };

    // ─────────────────────── quién entra al ranking ───────────────────────

    [Fact]
    public void LosRivalesConMenosPartidosQueElUmbralQuedanAfuera()
    {
        // Arrange — dos partidos contra Racing y uno solo contra Lanús.
        var partidos = new[]
        {
            Partidos.Dia(1, 2, 0, rival: "Racing"),
            Partidos.Dia(2, 1, 0, rival: "Racing"),
            Partidos.Dia(3, 3, 0, rival: "Lanús")
        };

        // Act
        var resumen = MandarAlRanking(partidos, Opciones(minPartidos: 2));

        // Assert — un solo partido ganado no hace a nadie talismán: con una muestra
        // de uno, el 100 % de efectividad no significa nada.
        Assert.Single(resumen.Ranking);
        Assert.Equal("Racing", resumen.Ranking[0].Rival);
    }

    [Fact]
    public void SinRivalesQueSuperenElUmbralNoHayTalismanNiMaldicion()
    {
        // Arrange — un partido contra cada uno, con un umbral de 3.
        var partidos = new[]
        {
            Partidos.Dia(1, 2, 0, rival: "Racing"),
            Partidos.Dia(2, 0, 1, rival: "River Plate")
        };

        // Act
        var resumen = MandarAlRanking(partidos, Opciones(minPartidos: 3));

        // Assert — null y no un rival cualquiera: la pantalla muestra el aviso de
        // "todavía no hay ningún rival con al menos N partidos".
        Assert.Null(resumen.Talisman);
        Assert.Null(resumen.Maldicion);
        Assert.Empty(resumen.Ranking);
    }

    [Fact]
    public void UnUmbralDeCeroSeTrataComoUno()
    {
        // Arrange — un umbral de cero dejaría entrar rivales con cero partidos.
        // El código lo sube a 1 con un Math.Max.
        var partidos = new[] { Partidos.Dia(1, 2, 0, rival: "Racing") };

        // Act
        var resumen = MandarAlRanking(partidos, Opciones(minPartidos: 0));

        // Assert
        Assert.Equal(1, resumen.UmbralAplicado);
        Assert.Single(resumen.Ranking);
    }

    [Fact]
    public void SeInformaElUmbralQueSeAplico()
    {
        // Arrange — la pantalla necesita el número para poder escribir "mínimo N PJ".
        var partidos = Partidos.Varios(3, 1, 0, rival: "Racing").ToList();

        // Act
        var resumen = MandarAlRanking(partidos, Opciones(minPartidos: 3));

        // Assert
        Assert.Equal(3, resumen.UmbralAplicado);
    }

    // ─────────────────────── un mismo rival es un mismo rival ───────────────────────

    [Fact]
    public void ElMismoRivalEscritoDistintoSeAgrupaIgual()
    {
        // Arrange — el rival lo tipea el hincha a mano, así que el mismo equipo
        // termina cargado de tres formas distintas.
        var partidos = new[]
        {
            Partidos.Dia(1, 2, 0, rival: "River Plate"),
            Partidos.Dia(2, 1, 0, rival: "river plate"),
            Partidos.Dia(3, 3, 0, rival: "  RIVER PLATE  ")
        };

        // Act
        var resumen = MandarAlRanking(partidos, Opciones(minPartidos: 3));

        // Assert — si se agruparan por separado, quedarían tres rivales de un partido
        // cada uno, ninguno llegaría al umbral, y el ranking saldría vacío.
        Assert.Single(resumen.Ranking);
        Assert.Equal(3, resumen.Ranking[0].Record.PartidosJugados);
    }

    [Fact]
    public void ElNombreDelRivalSeGuardaSinEspaciosAlrededor()
    {
        // Arrange
        var partidos = Partidos.Varios(2, 1, 0, rival: "  Racing  ").ToList();

        // Act
        var resumen = MandarAlRanking(partidos, Opciones());

        // Assert — lo que se guarda va directo a la pantalla; con los espacios
        // adentro, el nombre se ve corrido en la tarjeta del talismán.
        Assert.Equal("Racing", resumen.Ranking[0].Rival);
    }

    // ─────────────────────── el orden ───────────────────────

    [Fact]
    public void ElTalismanEsElRivalContraElQueMejorAnda()
    {
        // Act
        var resumen = MandarAlRanking(TresRivales(), Opciones());

        // Assert — Racing: 2 victorias, 100 % de efectividad.
        Assert.Equal("Racing", resumen.Talisman!.Rival);
    }

    [Fact]
    public void LaMaldicionEsElRivalContraElQuePeorAnda()
    {
        // Act
        var resumen = MandarAlRanking(TresRivales(), Opciones());

        // Assert — Independiente: 2 derrotas, 0 %. Es el ÚLTIMO del mismo orden, no
        // un segundo ranking al revés: por eso talismán y maldición nunca se
        // contradicen entre sí.
        Assert.Equal("Independiente", resumen.Maldicion!.Rival);
    }

    [Fact]
    public void ConUnSoloRivalEnElRankingNoHayMaldicion()
    {
        // Arrange — el borde que el propio código comenta: con un solo rival no hay
        // contraste.
        var partidos = Partidos.Varios(2, 2, 0, rival: "Racing").ToList();

        // Act
        var resumen = MandarAlRanking(partidos, Opciones());

        // Assert — sin la guarda, el mismo y único rival saldría en pantalla como
        // talismán Y como maldición a la vez.
        Assert.Equal("Racing", resumen.Talisman!.Rival);
        Assert.Null(resumen.Maldicion);
    }

    [Fact]
    public void AEfectividadIgualDesempataLaDiferenciaDeGol()
    {
        // Arrange — los dos rivales tienen una victoria y una derrota, o sea el mismo
        // 50 % de efectividad. Lo único que los separa es el gol.
        // Los nombres están elegidos para que el alfabético CONTRADIGA al gol: si el
        // desempate por diferencia de gol no estuviera, el siguiente criterio es el
        // nombre y Aldosivi quedaría primero.
        var partidos = new[]
        {
            Partidos.Dia(1, 1, 0, rival: "Aldosivi"),        // Aldosivi: +1 y -3 => DG -2
            Partidos.Dia(2, 0, 3, rival: "Aldosivi"),
            Partidos.Dia(3, 3, 0, rival: "Vélez"),           // Vélez:    +3 y -1 => DG +2
            Partidos.Dia(4, 0, 1, rival: "Vélez")
        };

        // Act
        var resumen = MandarAlRanking(partidos, Opciones());

        // Assert — Vélez primero aunque alfabéticamente vaya último, porque le hizo
        // más goles. Sin el desempate el orden quedaría a merced del nombre, y el
        // talismán sería el que tuviera mejor suerte en el abecedario.
        Assert.Equal("Vélez", resumen.Ranking[0].Rival);
        Assert.Equal("Aldosivi", resumen.Ranking[1].Rival);
    }

    [Fact]
    public void ElRankingQuedaOrdenadoDeMejorAPeor()
    {
        // Act
        var resumen = MandarAlRanking(TresRivales(), Opciones());

        // Assert
        Assert.Equal(["Racing", "River Plate", "Independiente"],
            resumen.Ranking.Select(r => r.Rival));
    }

    // ─────────────────────── ayudas ───────────────────────

    /// <summary>Racing gana siempre, River empata siempre, Independiente pierde siempre.</summary>
    private static Partido[] TresRivales() =>
    [
        Partidos.Dia(1, 2, 0, rival: "Racing"),
        Partidos.Dia(2, 1, 0, rival: "Racing"),
        Partidos.Dia(3, 1, 1, rival: "River Plate"),
        Partidos.Dia(4, 2, 2, rival: "River Plate"),
        Partidos.Dia(5, 0, 2, rival: "Independiente"),
        Partidos.Dia(6, 0, 1, rival: "Independiente")
    ];

    private static Domain.Estadisticas.ResumenRivales MandarAlRanking(
        IEnumerable<Partido> partidos, OpcionesDominio opciones) =>
        Domain.Estadisticas.RankingRivales.Resolver(partidos, opciones);
}
