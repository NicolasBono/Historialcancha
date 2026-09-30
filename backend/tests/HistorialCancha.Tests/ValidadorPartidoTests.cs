using HistorialCancha.Domain;
using HistorialCancha.Domain.Entidades;
using Xunit;

namespace HistorialCancha.Tests;

/// <summary>
/// Reglas de carga de un partido (FR7 a FR11).
///
/// La fecha de "hoy" está fija a propósito: el validador la recibe por parámetro,
/// así que estos tests no dependen del reloj de la máquina y no se vuelven flaky
/// el día que cambie el mes.
///
/// Sobre el Act de estos tests: el validador NO devuelve un resultado, lanza una
/// excepción. Con excepciones el Act va envuelto en Assert.Throws —es la única
/// forma de atraparla en xUnit— y el Assert propiamente dicho inspecciona lo que
/// esa llamada devolvió.
/// </summary>
public class ValidadorPartidoTests
{
    private static readonly DateOnly Hoy = new(2026, 9, 30);
    private const string MiEquipo = "Boca Juniors";

    /// <summary>Un partido que pasa todas las reglas. Cada test rompe UNA sola cosa.</summary>
    private static Partido PartidoValido(Action<Partido>? romperAlgo = null)
    {
        var partido = new Partido
        {
            UsuarioId = 1,
            Fecha = Hoy.AddDays(-7),
            Rival = "River Plate",
            Torneo = "Liga Profesional",
            Condicion = Condicion.Local,
            GolesAFavor = 2,
            GolesEnContra = 1,
            Vivencia = new Vivencia { Modalidad = Modalidad.EnCancha, Nota = 8 }
        };

        romperAlgo?.Invoke(partido);
        return partido;
    }

    private static void Validar(Partido partido, bool existeOtroEnEsaFecha = false) =>
        ValidadorPartido.Validar(partido, existeOtroEnEsaFecha, Hoy,
            new OpcionesDominio { MiEquipo = MiEquipo });

    // ─────────────────────── FR7: goles no negativos ───────────────────────

    [Theory]
    [InlineData(-1, 0)]    // negativo el de a favor
    [InlineData(0, -1)]    // negativo el de en contra
    [InlineData(-3, -2)]   // los dos
    public void GolesNegativos_SonRechazados(int aFavor, int enContra)
    {
        // Arrange
        var partido = PartidoValido(p => { p.GolesAFavor = aFavor; p.GolesEnContra = enContra; });

        // Act
        var error = Assert.Throws<ReglaDeNegocioException>(() => Validar(partido));

        // Assert
        Assert.Equal("goles-negativos", error.Regla);
    }

    [Fact]
    public void PartidoSinGoles_EsValido()
    {
        // Arrange — el cero es un marcador legítimo, no un dato faltante: 0 a 0 existe.
        var partido = PartidoValido(p => { p.GolesAFavor = 0; p.GolesEnContra = 0; });

        // Act + Assert — que no lance ES el comportamiento esperado.
        Validar(partido);
    }

    // ─────────────────────── FR8: fecha no futura ───────────────────────

    [Fact]
    public void FechaFutura_EsRechazada()
    {
        // Arrange
        var partido = PartidoValido(p => p.Fecha = Hoy.AddDays(1));

        // Act
        var error = Assert.Throws<ReglaDeNegocioException>(() => Validar(partido));

        // Assert
        Assert.Equal("fecha-futura", error.Regla);
    }

    [Fact]
    public void PartidoDeHoy_EsAceptado()
    {
        // Arrange — el borde exacto de la regla: hoy NO es futuro. Si alguien cambia
        // el ">" por un ">=", este test es el único que se pone rojo.
        var partido = PartidoValido(p => p.Fecha = Hoy);

        // Act + Assert
        Validar(partido);
    }

    // ─────────────────────── FR9: el rival no soy yo ───────────────────────

    [Theory]
    [InlineData("Boca Juniors")]      // igual
    [InlineData("boca juniors")]      // distinta capitalización
    [InlineData("  BOCA JUNIORS  ")]  // con espacios alrededor
    public void RivalIgualAMiEquipo_EsRechazado(string rival)
    {
        // Arrange
        var partido = PartidoValido(p => p.Rival = rival);

        // Act
        var error = Assert.Throws<ReglaDeNegocioException>(() => Validar(partido));

        // Assert
        Assert.Equal("rival-es-mi-equipo", error.Regla);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void RivalSinContenido_EsRechazado(string rival)
    {
        // Arrange
        var partido = PartidoValido(p => p.Rival = rival);

        // Act
        var error = Assert.Throws<ReglaDeNegocioException>(() => Validar(partido));

        // Assert
        Assert.Equal("rival-requerido", error.Regla);
    }

    // ─────────────────────── FR10: un partido por día ───────────────────────

    [Fact]
    public void PartidoDuplicadoEnLaFecha_EsRechazado()
    {
        // Arrange
        var partido = PartidoValido(p => p.Fecha = new DateOnly(2026, 3, 8));

        // Act
        var error = Assert.Throws<ReglaDeNegocioException>(
            () => Validar(partido, existeOtroEnEsaFecha: true));

        // Assert
        Assert.Equal("fecha-duplicada", error.Regla);
    }

    [Fact]
    public void PartidoDuplicado_ElMensajeDiceCualEsLaFecha()
    {
        // Arrange — el rechazo tiene que decir POR QUÉ: un "fecha duplicada" pelado
        // obliga al hincha a adivinar cuál de sus partidos le está estorbando.
        var partido = PartidoValido(p => p.Fecha = new DateOnly(2026, 3, 8));

        // Act
        var error = Assert.Throws<ReglaDeNegocioException>(
            () => Validar(partido, existeOtroEnEsaFecha: true));

        // Assert
        Assert.Contains("08/03/2026", error.Message);
    }

    // ─────────────────────── FR11: nota entre 1 y 10 ───────────────────────

    [Theory]
    [InlineData((byte)0)]
    [InlineData((byte)11)]
    public void NotaFueraDeRango_EsRechazada(byte nota)
    {
        // Arrange
        var partido = PartidoValido(p => p.Vivencia!.Nota = nota);

        // Act
        var error = Assert.Throws<ReglaDeNegocioException>(() => Validar(partido));

        // Assert
        Assert.Equal("nota-fuera-de-rango", error.Regla);
    }

    [Theory]
    [InlineData((byte)1)]    // borde de abajo
    [InlineData((byte)10)]   // borde de arriba
    public void NotaEnElBorde_EsAceptada(byte nota)
    {
        // Arrange
        var partido = PartidoValido(p => p.Vivencia!.Nota = nota);

        // Act + Assert
        Validar(partido);
    }

    [Fact]
    public void PartidoSinNota_EsValido()
    {
        // Arrange — la nota es opcional: no ponerla no es lo mismo que poner una nota mala.
        var partido = PartidoValido(p => p.Vivencia!.Nota = null);

        // Act + Assert
        Validar(partido);
    }
}
