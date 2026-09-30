using HistorialCancha.Domain;
using HistorialCancha.Domain.Entidades;
using HistorialCancha.Domain.Estadisticas;
using HistorialCancha.Tests.Ayudas;
using Xunit;

namespace HistorialCancha.Tests;

/// <summary>
/// FR15 y FR16 — el récord por modalidad y el veredicto cábala / yeta.
///
/// Es la regla que le da nombre a la app, y la que más fácil se rompe sin que nadie
/// se dé cuenta: un veredicto equivocado no tira ningún error, sale en pantalla como
/// si fuera verdad. Los umbrales salen de configuración, así que cada test declara
/// los suyos en vez de depender de los valores por defecto.
/// </summary>
public class CalculadoraModalidadTests
{
    private static OpcionesDominio Opciones(int minPartidos = 5, int umbralPuntos = 10) =>
        new() { MiEquipo = "Boca Juniors", MinPartidosVeredicto = minPartidos, UmbralVeredictoPuntos = umbralPuntos };

    // ─────────────────────── el desglose por modalidad ───────────────────────

    [Fact]
    public void HayUnRecordPorCadaModalidadAunqueNoTengaPartidos()
    {
        // Arrange — un solo partido, en cancha.
        var partidos = new[] { Partidos.Dia(1, 2, 1, Modalidad.EnCancha) };

        // Act
        var resumen = CalculadoraModalidad.Calcular(partidos, Opciones());

        // Assert — la tabla de la pantalla muestra las cinco filas siempre, en gris
        // las que no tienen partidos. Si la calculadora devolviera sólo las modalidades
        // usadas, la tabla cambiaría de tamaño según el historial.
        Assert.Equal(Enum.GetValues<Modalidad>().Length, resumen.PorModalidad.Count);
    }

    [Fact]
    public void CadaModalidadCuentaSoloSusPartidos()
    {
        // Arrange
        var partidos = new[]
        {
            Partidos.Dia(1, 2, 0, Modalidad.EnCancha),
            Partidos.Dia(2, 1, 0, Modalidad.EnCancha),
            Partidos.Dia(3, 0, 3, Modalidad.TV)
        };

        // Act
        var resumen = CalculadoraModalidad.Calcular(partidos, Opciones());

        // Assert
        Assert.Equal(2, resumen.EnCancha.PartidosJugados);
        Assert.Equal(2, resumen.EnCancha.Ganados);
    }

    [Fact]
    public void UnPartidoSinVivenciaNoEntraEnNingunaModalidad()
    {
        // Arrange — la vivencia es opcional en el modelo: puede haber un partido
        // cargado sin decir cómo se lo vivió.
        var partidos = new[]
        {
            Partidos.Dia(1, 2, 0, Modalidad.EnCancha),
            Partidos.SinVivencia(2, 5, 0)
        };

        // Act
        var resumen = CalculadoraModalidad.Calcular(partidos, Opciones());

        // Assert — si se colara en "en cancha", inflaría el récord del grupo con un
        // partido que nadie dijo haber ido a ver.
        Assert.Equal(1, resumen.EnCancha.PartidosJugados);
    }

    // ─────────────────────── qué cuenta como "otro medio" ───────────────────────

    [Fact]
    public void TvStreamingYRadioCuentanComoOtroMedio()
    {
        // Arrange
        var partidos = new[]
        {
            Partidos.Dia(1, 1, 0, Modalidad.TV),
            Partidos.Dia(2, 1, 0, Modalidad.Streaming),
            Partidos.Dia(3, 1, 0, Modalidad.Radio)
        };

        // Act
        var resumen = CalculadoraModalidad.Calcular(partidos, Opciones());

        // Assert
        Assert.Equal(3, resumen.PorOtroMedio.PartidosJugados);
    }

    [Fact]
    public void NoLoViQuedaAfueraDeLaComparacion()
    {
        // Arrange — la regla más sutil de toda la app: "no lo vi" NO es otro medio,
        // porque no hubo experiencia que comparar. Sigue contando para el récord
        // global, pero no puede entrar a decidir si sos cábala.
        var partidos = Partidos.Varios(5, 1, 0, Modalidad.NoLoVi).ToList();

        // Act
        var resumen = CalculadoraModalidad.Calcular(partidos, Opciones());

        // Assert — cero, no cinco. Si contara, cinco partidos que el hincha ni miró
        // estarían votando en el veredicto.
        Assert.Equal(0, resumen.PorOtroMedio.PartidosJugados);
    }

    [Fact]
    public void NoLoViSiFiguraEnElDesglosePorModalidad()
    {
        // Arrange — la otra mitad de la regla: queda afuera de la COMPARACIÓN, no
        // del historial. Tiene que seguir apareciendo en la tabla.
        var partidos = Partidos.Varios(3, 1, 0, Modalidad.NoLoVi).ToList();

        // Act
        var resumen = CalculadoraModalidad.Calcular(partidos, Opciones());

        // Assert
        var fila = resumen.PorModalidad.Single(m => m.Modalidad == Modalidad.NoLoVi);
        Assert.Equal(3, fila.Record.PartidosJugados);
    }

    // ─────────────────────── el veredicto ───────────────────────

    [Fact]
    public void SiRindeMuchoMejorEnCancha_EsCabala()
    {
        // Arrange — 5 victorias en cancha (100 %) contra 5 derrotas por TV (0 %).
        var partidos = Partidos.Varios(5, 2, 0, Modalidad.EnCancha)
            .Concat(Partidos.Varios(5, 0, 2, Modalidad.TV, desdeElDia: 6))
            .ToList();

        // Act
        var resumen = CalculadoraModalidad.Calcular(partidos, Opciones());

        // Assert
        Assert.Equal(Veredicto.Cabala, resumen.Veredicto);
    }

    [Fact]
    public void SiRindeMuchoPeorEnCancha_EsYeta()
    {
        // Arrange — el espejo exacto del anterior: la cancha pierde, la TV gana.
        var partidos = Partidos.Varios(5, 0, 2, Modalidad.EnCancha)
            .Concat(Partidos.Varios(5, 2, 0, Modalidad.TV, desdeElDia: 6))
            .ToList();

        // Act
        var resumen = CalculadoraModalidad.Calcular(partidos, Opciones());

        // Assert — este par de tests es el que atrapa un signo invertido en la resta,
        // que es el error más probable de toda la función.
        Assert.Equal(Veredicto.Yeta, resumen.Veredicto);
    }

    [Fact]
    public void SiLaDiferenciaNoLlegaAlUmbral_EsIndefinido()
    {
        // Arrange — en cancha 60 % (3 victorias y 2 derrotas) contra 53,3 % por TV
        // (2 victorias, 2 empates y 1 derrota): 6,7 puntos, menos que los 10 del umbral.
        var partidos = Partidos.Varios(3, 2, 0, Modalidad.EnCancha)
            .Concat(Partidos.Varios(2, 0, 2, Modalidad.EnCancha, desdeElDia: 4))
            .Concat(Partidos.Varios(2, 2, 0, Modalidad.TV, desdeElDia: 6))
            .Concat(Partidos.Varios(2, 1, 1, Modalidad.TV, desdeElDia: 8))
            .Concat(Partidos.Varios(1, 0, 2, Modalidad.TV, desdeElDia: 10))
            .ToList();

        // Act
        var resumen = CalculadoraModalidad.Calcular(partidos, Opciones());

        // Assert — sin esta rama, cualquier diferencia mínima cantaría cábala.
        Assert.Equal(Veredicto.Indefinido, resumen.Veredicto);
    }

    [Fact]
    public void ConPocosPartidosEsIndefinido_YLaExplicacionDiceCuantosLleva()
    {
        // Arrange — dos y dos, contra un mínimo de cinco.
        var partidos = Partidos.Varios(2, 2, 0, Modalidad.EnCancha)
            .Concat(Partidos.Varios(2, 0, 2, Modalidad.TV, desdeElDia: 3))
            .ToList();

        // Act
        var resumen = CalculadoraModalidad.Calcular(partidos, Opciones(minPartidos: 5));

        // Assert — con 2 victorias contra 2 derrotas la diferencia es del 100 %, así
        // que sin esta guarda diría "sos cábala" con cuatro partidos cargados. El
        // mensaje además tiene que decirle al hincha cuánto le falta.
        Assert.Equal(Veredicto.Indefinido, resumen.Veredicto);
        Assert.Contains("5", resumen.Explicacion);
        Assert.Contains("2", resumen.Explicacion);
    }

    [Fact]
    public void ConElMinimoJustoElVeredictoYaSePronuncia()
    {
        // Arrange — el borde: exactamente el mínimo de cada lado. Con un ">" en lugar
        // de un "<" mal puesto, acá seguiría diciendo indefinido.
        var partidos = Partidos.Varios(3, 2, 0, Modalidad.EnCancha)
            .Concat(Partidos.Varios(3, 0, 2, Modalidad.TV, desdeElDia: 4))
            .ToList();

        // Act
        var resumen = CalculadoraModalidad.Calcular(partidos, Opciones(minPartidos: 3));

        // Assert
        Assert.Equal(Veredicto.Cabala, resumen.Veredicto);
    }

    // ─────────────────────── los umbrales vienen de configuración ───────────────────────

    [Fact]
    public void ElUmbralDePuntosSaleDeLaConfiguracionYNoEstaHardcodeado()
    {
        // Arrange — la misma diferencia de 100 puntos, con el umbral subido a 150.
        var partidos = Partidos.Varios(5, 2, 0, Modalidad.EnCancha)
            .Concat(Partidos.Varios(5, 0, 2, Modalidad.TV, desdeElDia: 6))
            .ToList();

        // Act
        var resumen = CalculadoraModalidad.Calcular(partidos, Opciones(umbralPuntos: 150));

        // Assert — si el 10 estuviera escrito dentro de la función, esto seguiría
        // diciendo cábala y el parámetro de configuración sería decorativo.
        Assert.Equal(Veredicto.Indefinido, resumen.Veredicto);
    }

    [Fact]
    public void ConElMinimoEnCeroUnPartidoDeCadaLadoYaAlcanza()
    {
        // Arrange — el mínimo configurado en cero se trata como uno, así que un
        // partido de cada lado llega justo.
        var partidos = Partidos.Varios(1, 2, 0, Modalidad.EnCancha)
            .Concat(Partidos.Varios(1, 0, 2, Modalidad.TV, desdeElDia: 2))
            .ToList();

        // Act
        var resumen = CalculadoraModalidad.Calcular(partidos, Opciones(minPartidos: 0));

        // Assert
        Assert.Equal(Veredicto.Cabala, resumen.Veredicto);
    }

    [Fact]
    public void UnMinimoDeCeroNoDejaEmitirVeredictoConUnGrupoVacio()
    {
        // Arrange — cinco victorias por TV y NINGÚN partido en cancha, con el mínimo
        // configurado en cero. Es el caso que obliga al piso de 1: sin él, el grupo
        // vacío cumpliría el mínimo porque 0 >= 0.
        var partidos = Partidos.Varios(5, 2, 0, Modalidad.TV).ToList();

        // Act
        var resumen = CalculadoraModalidad.Calcular(partidos, Opciones(minPartidos: 0));

        // Assert — sin el Math.Max la app te declararía YETA por cero partidos en
        // cancha contra cinco victorias por televisión, que es un veredicto sacado
        // de la nada. Con el piso, dice que no hay datos suficientes.
        Assert.Equal(Veredicto.Indefinido, resumen.Veredicto);
    }

    [Fact]
    public void SinNingunPartidoElVeredictoEsIndefinido()
    {
        // Act
        var resumen = CalculadoraModalidad.Calcular([], Opciones());

        // Assert
        Assert.Equal(Veredicto.Indefinido, resumen.Veredicto);
    }

    // ─────────────────────── el texto de la explicación ───────────────────────

    [Fact]
    public void LaExplicacionUsaPuntoDecimalYNoComa()
    {
        // Arrange — la misma diferencia de 6,7 puntos del test de más arriba.
        var partidos = Partidos.Varios(3, 2, 0, Modalidad.EnCancha)
            .Concat(Partidos.Varios(2, 0, 2, Modalidad.EnCancha, desdeElDia: 4))
            .Concat(Partidos.Varios(2, 2, 0, Modalidad.TV, desdeElDia: 6))
            .Concat(Partidos.Varios(2, 1, 1, Modalidad.TV, desdeElDia: 8))
            .Concat(Partidos.Varios(1, 0, 2, Modalidad.TV, desdeElDia: 10))
            .ToList();

        // Act
        var resumen = CalculadoraModalidad.Calcular(partidos, Opciones());

        // Assert — el código formatea con cultura invariante a propósito, para que
        // el texto coincida con los números del JSON y no cambie según la máquina
        // donde corre. En una máquina con cultura es-AR, sin eso saldría "6,7".
        Assert.Contains("6.7", resumen.Explicacion);
    }
}
