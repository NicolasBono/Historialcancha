using System.Diagnostics.CodeAnalysis;

namespace HistorialCancha.Api.Dtos;

/// <summary>
/// Alta de un usuario. Los campos son nullable a propósito: si el cliente omite
/// uno, lo rechaza el validador de dominio con un mensaje claro, no un NullReference.
/// </summary>
[ExcludeFromCodeCoverage]   // clase de datos: sólo propiedades, ninguna regla que verificar
public record RegistroRequest(
    string? Nombre,
    string? Apellido,
    string? Dni,
    string? Contrasena);

/// <summary>Login por DNI + contraseña.</summary>
[ExcludeFromCodeCoverage]   // clase de datos: sólo propiedades, ninguna regla que verificar
public record LoginRequest(
    string? Dni,
    string? Contrasena);

/// <summary>
/// Lo que se devuelve al registrarse o loguearse: el token para las siguientes
/// requests y los datos de identidad para el header del frontend.
/// </summary>
[ExcludeFromCodeCoverage]   // clase de datos: sólo propiedades, ninguna regla que verificar
public record AuthResponse(
    string Token,
    DateTime ExpiraEn,
    string Nombre,
    string Apellido);
