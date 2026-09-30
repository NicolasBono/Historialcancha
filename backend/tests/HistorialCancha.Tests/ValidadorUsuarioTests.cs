using HistorialCancha.Domain;
using HistorialCancha.Domain.Entidades;
using Xunit;

namespace HistorialCancha.Tests;

/// <summary>
/// Reglas de alta de un usuario. Igual que el validador de partidos, todo lo que
/// exige mirar la base —si el DNI ya existe— entra como parámetro, y el Act va
/// envuelto en Assert.Throws porque el validador lanza en vez de devolver.
/// </summary>
public class ValidadorUsuarioTests
{
    private const string ContrasenaValida = "unaClaveLarga";

    /// <summary>Un usuario que pasa todas las reglas. Cada test rompe UNA sola cosa.</summary>
    private static Usuario UsuarioValido(Action<Usuario>? romperAlgo = null)
    {
        var usuario = new Usuario
        {
            Nombre = "Nicolás",
            Apellido = "Bono",
            Dni = "39123456"
        };

        romperAlgo?.Invoke(usuario);
        return usuario;
    }

    private static void Validar(Usuario usuario, string? contrasena = ContrasenaValida,
        bool dniYaRegistrado = false) =>
        ValidadorUsuario.Validar(usuario, contrasena, dniYaRegistrado);

    // ─────────────────────── el DNI tiene forma ───────────────────────

    [Theory]
    [InlineData("123456")]      // seis dígitos: uno menos que el mínimo
    [InlineData("123456789")]   // nueve: uno más que el máximo
    [InlineData("1234567a")]    // ocho caracteres, pero uno no es dígito
    [InlineData("39.123.456")]  // con puntos, como lo escribe la gente
    public void DniConFormaInvalida_EsRechazado(string dni)
    {
        // Arrange
        var usuario = UsuarioValido(u => u.Dni = dni);

        // Act
        var error = Assert.Throws<ReglaDeNegocioException>(() => Validar(usuario));

        // Assert
        Assert.Equal("dni-invalido", error.Regla);
    }

    [Theory]
    [InlineData("1234567")]    // borde de abajo: siete dígitos
    [InlineData("12345678")]   // borde de arriba: ocho
    public void DniEnLosBordesDeLargo_EsAceptado(string dni)
    {
        // Arrange
        var usuario = UsuarioValido(u => u.Dni = dni);

        // Act + Assert — que no lance ES el comportamiento esperado.
        Validar(usuario);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void DniSinContenido_EsRechazadoComoFaltante(string dni)
    {
        // Arrange — dos rechazos distintos a propósito: "no lo cargaste" y "lo
        // cargaste mal" no son el mismo problema, y el frontend puede reaccionar
        // distinto a cada uno.
        var usuario = UsuarioValido(u => u.Dni = dni);

        // Act
        var error = Assert.Throws<ReglaDeNegocioException>(() => Validar(usuario));

        // Assert
        Assert.Equal("dni-requerido", error.Regla);
    }

    // ─────────────────────── la contraseña tiene largo mínimo ───────────────────────

    [Fact]
    public void ContrasenaMasCortaQueElMinimo_EsRechazada()
    {
        // Arrange
        var sieteCaracteres = new string('a', ValidadorUsuario.LargoMinimoContrasena - 1);

        // Act
        var error = Assert.Throws<ReglaDeNegocioException>(
            () => Validar(UsuarioValido(), contrasena: sieteCaracteres));

        // Assert
        Assert.Equal("contrasena-debil", error.Regla);
    }

    [Fact]
    public void ContrasenaCorta_ElMensajeDiceCualEsElMinimo()
    {
        // Arrange — un "contraseña débil" pelado obliga al usuario a probar largos
        // hasta acertar: el mensaje también es comportamiento.
        var sieteCaracteres = new string('a', ValidadorUsuario.LargoMinimoContrasena - 1);

        // Act
        var error = Assert.Throws<ReglaDeNegocioException>(
            () => Validar(UsuarioValido(), contrasena: sieteCaracteres));

        // Assert
        Assert.Contains(ValidadorUsuario.LargoMinimoContrasena.ToString(), error.Message);
    }

    [Fact]
    public void ContrasenaExactamenteEnElMinimo_EsAceptada()
    {
        // Arrange — el borde: ocho caracteres alcanza. Con un ">" en lugar de un
        // ">=" en el validador, este test es el que avisa.
        var ochoCaracteres = new string('a', ValidadorUsuario.LargoMinimoContrasena);

        // Act + Assert
        Validar(UsuarioValido(), contrasena: ochoCaracteres);
    }

    // ─────────────────────── el DNI es único ───────────────────────

    [Fact]
    public void DniYaRegistrado_EsRechazado()
    {
        // Arrange
        var usuario = UsuarioValido();

        // Act
        var error = Assert.Throws<ReglaDeNegocioException>(
            () => Validar(usuario, dniYaRegistrado: true));

        // Assert
        Assert.Equal("dni-duplicado", error.Regla);
    }

    [Fact]
    public void NombreSinContenido_EsRechazado()
    {
        // Arrange
        var usuario = UsuarioValido(u => u.Nombre = "  ");

        // Act
        var error = Assert.Throws<ReglaDeNegocioException>(() => Validar(usuario));

        // Assert
        Assert.Equal("nombre-requerido", error.Regla);
    }
}
