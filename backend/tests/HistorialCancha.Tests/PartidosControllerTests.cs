using System.Security.Claims;
using HistorialCancha.Api.Controllers;
using HistorialCancha.Api.Dtos;
using HistorialCancha.Domain;
using HistorialCancha.Domain.Entidades;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace HistorialCancha.Tests;

/// <summary>
/// El alta y el listado de partidos, con la persistencia reemplazada por un doble.
///
/// Acá el sujeto de prueba no es una cuenta sino una COLABORACIÓN: que el controller
/// le pregunte a la base lo que el dominio necesita saber, y que recién después
/// —si la validación pasó— mande a guardar. Eso no se puede verificar mirando el
/// valor devuelto: hay que mirar cómo se usó la dependencia. Por eso el doble es un
/// mock y no un stub, y por eso los Assert son Verify.
///
/// No hizo falta refactorizar nada para poder mockear: PartidosController ya recibe
/// IPartidoRepository por constructor desde el TP2, y el dominio declara la interfaz.
/// </summary>
public class PartidosControllerTests
{
    private const int UsuarioId = 7;
    private static DateOnly Hoy => DateOnly.FromDateTime(DateTime.Today);

    private static PartidosController ConstruirController(IPartidoRepository repositorio)
    {
        var controller = new PartidosController(repositorio,
            Options.Create(new OpcionesDominio { MiEquipo = "Boca Juniors" }));

        // El UsuarioId sale del token firmado, nunca de la URL ni del body. En una
        // request real lo pone el middleware de autenticación; acá lo pone el test.
        var identidad = new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, UsuarioId.ToString())], "test");

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identidad) }
        };

        return controller;
    }

    /// <summary>El doble con la fecha declarada libre: el alta tiene que poder avanzar.</summary>
    private static Mock<IPartidoRepository> RepositorioConLaFechaLibre(DateOnly fecha)
    {
        var repositorio = new Mock<IPartidoRepository>();

        // Acá el doble actúa de STUB: sólo provee una respuesta, no se verifica nada sobre él.
        repositorio
            .Setup(r => r.ExisteEnFechaAsync(UsuarioId, fecha, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        return repositorio;
    }

    private static PartidoRequest Pedido(DateOnly fecha) => new(
        Fecha: fecha,
        Rival: "River Plate",
        Torneo: "Liga Profesional",
        Condicion: Condicion.Local,
        Estadio: "La Bombonera",
        GolesAFavor: 2,
        GolesEnContra: 1,
        Modalidad: Modalidad.EnCancha,
        Sector: "Platea",
        ConQuien: "Mi viejo",
        Nota: 9);

    [Fact]
    public async Task Crear_LePreguntaAlRepositorioSiYaHabiaPartidoEsaFecha()
    {
        // Arrange — el dominio no consulta la base: necesita el dato ya leído, y
        // conseguirlo es responsabilidad del controller.
        var fecha = Hoy.AddDays(-3);
        var repositorio = RepositorioConLaFechaLibre(fecha);

        // Act
        await ConstruirController(repositorio.Object).Crear(Pedido(fecha), CancellationToken.None);

        // Assert — acá el doble actúa de MOCK: se verifica la INTERACCIÓN. La consulta
        // va acotada al usuario del token, no a cualquiera, y se hace una sola vez.
        repositorio.Verify(
            r => r.ExisteEnFechaAsync(UsuarioId, fecha, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Crear_GuardaElPartidoUnaSolaVezYAtadoAlUsuarioDelToken()
    {
        // Arrange
        var fecha = Hoy.AddDays(-3);
        var repositorio = RepositorioConLaFechaLibre(fecha);

        // Act
        await ConstruirController(repositorio.Object).Crear(Pedido(fecha), CancellationToken.None);

        // Assert — si mañana alguien duplica el AgregarAsync, el hincha ve el partido
        // dos veces. Ningún test de valor devuelto lo notaría; éste sí.
        repositorio.Verify(
            r => r.AgregarAsync(
                It.Is<Partido>(p => p.UsuarioId == UsuarioId && p.Rival == "River Plate"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Crear_ConUnaFechaFutura_NoLlegaAGuardar()
    {
        // Arrange — la regla de negocio ya está probada aparte. Lo que se verifica acá
        // es el ORDEN: que la validación corra ANTES de tocar la persistencia. Un
        // controller que guarda primero y valida después dejaría basura en la base.
        var manana = Hoy.AddDays(1);
        var repositorio = RepositorioConLaFechaLibre(manana);

        // Act
        await Assert.ThrowsAsync<ReglaDeNegocioException>(
            () => ConstruirController(repositorio.Object).Crear(Pedido(manana), CancellationToken.None));

        // Assert — la interacción que importa es la que NO ocurrió.
        repositorio.Verify(
            r => r.AgregarAsync(It.IsAny<Partido>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Listar_PideSoloLosPartidosDelUsuarioDelToken()
    {
        // Arrange — el aislamiento entre hinchas es la regla de seguridad de la app.
        var repositorio = new Mock<IPartidoRepository>();
        repositorio
            .Setup(r => r.ObtenerTodosAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        await ConstruirController(repositorio.Object).Listar(CancellationToken.None);

        // Assert — con qué id se consultó no se ve en la respuesta; el doble es lo
        // único que deja comprobarlo.
        repositorio.Verify(
            r => r.ObtenerTodosAsync(UsuarioId, It.IsAny<CancellationToken>()), Times.Once);
        repositorio.Verify(
            r => r.ObtenerTodosAsync(It.Is<int>(id => id != UsuarioId), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
