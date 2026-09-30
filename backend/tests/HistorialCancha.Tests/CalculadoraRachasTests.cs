using HistorialCancha.Domain.Entidades;
using HistorialCancha.Domain.Estadisticas;
using HistorialCancha.Tests.Ayudas;
using Xunit;

namespace HistorialCancha.Tests;

/// <summary>
/// FR17 y FR18 — racha actual y racha histórica más larga.
///
/// Una racha es una secuencia TEMPORAL, así que lo que se prueba acá no es una cuenta
/// sino un recorrido: que el orden sea el correcto, que un partido que no cumple la
/// corte, y que se distinga una racha viva de una que terminó hace meses.
/// </summary>
public class CalculadoraRachasTests
{
    [Fact]
    public void SinPartidos_NoHayNingunaRacha()
    {
        // Arrange — el hincha recién registrado entra a estadísticas con el historial vacío.

        // Act
        var resumen = CalculadoraRachas.Calcular([]);

        // Assert
        Assert.Equal(Racha.Ninguna, resumen.Invicto.Actual);
        Assert.Equal(Racha.Ninguna, resumen.Invicto.MasLarga);
    }

    // ─────────────────────── qué cuenta como invicto ───────────────────────

    [Fact]
    public void LaRachaInvictaCuentaVictoriasYEmpates()
    {
        // Arrange — invicto no es "ganando": es "sin perder", así que el empate suma.
        var partidos = new[]
        {
            Partidos.Dia(1, 2, 1),   // victoria
            Partidos.Dia(2, 1, 1),   // empate
            Partidos.Dia(3, 3, 0)    // victoria
        };

        // Act
        var resumen = CalculadoraRachas.Calcular(partidos);

        // Assert
        Assert.Equal(3, resumen.Invicto.Actual.Longitud);
    }

    [Fact]
    public void UnaDerrotaCortaLaRachaInvicta()
    {
        // Arrange — la derrota va en el medio: parte el historial en dos rachas de 1.
        var partidos = new[]
        {
            Partidos.Dia(1, 2, 1),   // victoria
            Partidos.Dia(2, 0, 3),   // DERROTA: corta
            Partidos.Dia(3, 1, 0)    // victoria
        };

        // Act
        var resumen = CalculadoraRachas.Calcular(partidos);

        // Assert
        Assert.Equal(1, resumen.Invicto.Actual.Longitud);
        Assert.Equal(1, resumen.Invicto.MasLarga.Longitud);
    }

    [Fact]
    public void LaRachaSinGanarCuentaEmpatesYDerrotas()
    {
        // Arrange — la contracara: "sin ganar" incluye el empate igual que el invicto.
        var partidos = new[]
        {
            Partidos.Dia(1, 1, 1),   // empate
            Partidos.Dia(2, 0, 2)    // derrota
        };

        // Act
        var resumen = CalculadoraRachas.Calcular(partidos);

        // Assert
        Assert.Equal(2, resumen.SinGanar.Actual.Longitud);
    }

    [Fact]
    public void LaRachaSinRecibirGolesMiraSoloLosGolesEnContra()
    {
        // Arrange — una derrota 0-0 no existe, pero un empate 0-0 sí cuenta como
        // valla invicta: la regla mira el arco, no el resultado.
        var partidos = new[]
        {
            Partidos.Dia(1, 3, 0),   // gana sin recibir
            Partidos.Dia(2, 0, 0),   // empata sin recibir
            Partidos.Dia(3, 5, 1)    // gana PERO recibe: corta
        };

        // Act
        var resumen = CalculadoraRachas.Calcular(partidos);

        // Assert
        Assert.Equal(2, resumen.SinRecibirGoles.MasLarga.Longitud);
        Assert.Equal(0, resumen.SinRecibirGoles.Actual.Longitud);
    }

    // ─────────────────────── actual vs. más larga ───────────────────────

    [Fact]
    public void LaRachaActualSeCuentaDesdeElUltimoPartidoHaciaAtras()
    {
        // Arrange — dos invictos separados por una derrota. El primero es más largo,
        // pero el que está corriendo HOY es el segundo.
        var partidos = new[]
        {
            Partidos.Dia(1, 1, 0),   // ┐
            Partidos.Dia(2, 2, 0),   // │ invicto de 3, en el pasado
            Partidos.Dia(3, 1, 1),   // ┘
            Partidos.Dia(4, 0, 2),   // DERROTA
            Partidos.Dia(5, 3, 1),   // ┐ invicto de 2, corriendo
            Partidos.Dia(6, 1, 0)    // ┘
        };

        // Act
        var resumen = CalculadoraRachas.Calcular(partidos);

        // Assert
        Assert.Equal(2, resumen.Invicto.Actual.Longitud);
        Assert.Equal(3, resumen.Invicto.MasLarga.Longitud);
    }

    [Fact]
    public void SiElUltimoPartidoNoCumple_NoHayRachaActual()
    {
        // Arrange — el historial termina en derrota: la racha invicta está cortada.
        var partidos = new[]
        {
            Partidos.Dia(1, 2, 0),
            Partidos.Dia(2, 1, 0),
            Partidos.Dia(3, 0, 1)    // termina perdiendo
        };

        // Act
        var resumen = CalculadoraRachas.Calcular(partidos);

        // Assert — 0 y no "la última que hubo": lo que se pregunta es qué está pasando ahora.
        Assert.Equal(Racha.Ninguna, resumen.Invicto.Actual);
        Assert.Equal(2, resumen.Invicto.MasLarga.Longitud);
    }

    [Fact]
    public void UnaRachaQueTerminoNoFiguraEnCurso()
    {
        // Arrange — la más larga quedó en el pasado, cortada por la derrota final.
        var partidos = new[]
        {
            Partidos.Dia(1, 1, 0),
            Partidos.Dia(2, 1, 0),
            Partidos.Dia(3, 0, 4)
        };

        // Act
        var resumen = CalculadoraRachas.Calcular(partidos);

        // Assert — "en curso" es lo que separa un récord histórico de algo que puede
        // seguir creciendo, y es lo que la pantalla muestra distinto.
        Assert.False(resumen.Invicto.MasLarga.EnCurso);
    }

    [Fact]
    public void UnaRachaQueLlegaAlUltimoPartidoFiguraEnCurso()
    {
        // Arrange
        var partidos = new[] { Partidos.Dia(1, 0, 2), Partidos.Dia(2, 1, 0), Partidos.Dia(3, 2, 0) };

        // Act
        var resumen = CalculadoraRachas.Calcular(partidos);

        // Assert
        Assert.True(resumen.Invicto.MasLarga.EnCurso);
        Assert.Equal(2, resumen.Invicto.MasLarga.Longitud);
    }

    [Fact]
    public void LaRachaGuardaDesdeCuandoYHastaCuandoFue()
    {
        // Arrange
        var partidos = new[] { Partidos.Dia(4, 1, 0), Partidos.Dia(5, 2, 2), Partidos.Dia(6, 0, 3) };

        // Act
        var resumen = CalculadoraRachas.Calcular(partidos);

        // Assert — sin las fechas la pantalla no puede decir "de tal día a tal otro".
        Assert.Equal(new DateOnly(2026, 1, 4), resumen.Invicto.MasLarga.Desde);
        Assert.Equal(new DateOnly(2026, 1, 5), resumen.Invicto.MasLarga.Hasta);
    }

    // ─────────────────────── el orden no lo pone quien llama ───────────────────────

    [Fact]
    public void LosPartidosSeOrdenanPorFechaAunqueLleguenDesordenados()
    {
        // Arrange — el repositorio devuelve del más nuevo al más viejo (así lo muestra
        // la pantalla), pero una racha se recorre al revés. Si la calculadora confiara
        // en el orden de entrada, contaría cualquier cosa.
        var alReves = new[]
        {
            Partidos.Dia(3, 0, 2),   // derrota, la MÁS NUEVA
            Partidos.Dia(2, 1, 0),
            Partidos.Dia(1, 2, 0)    // la más vieja
        };

        // Act
        var resumen = CalculadoraRachas.Calcular(alReves);

        // Assert — si respetara el orden de entrada, vería la derrota primero y
        // reportaría una racha actual de 2. La correcta es 0: el último partido perdió.
        Assert.Equal(Racha.Ninguna, resumen.Invicto.Actual);
        Assert.Equal(2, resumen.Invicto.MasLarga.Longitud);
    }

    [Fact]
    public void ConUnSoloPartidoGanado_LaRachaEsDeUno()
    {
        // Arrange — el borde de abajo, que el propio código declara contemplado.

        // Act
        var resumen = CalculadoraRachas.Calcular([Partidos.Dia(1, 1, 0)]);

        // Assert
        Assert.Equal(1, resumen.Invicto.Actual.Longitud);
        Assert.True(resumen.Invicto.Actual.EnCurso);
    }
}
