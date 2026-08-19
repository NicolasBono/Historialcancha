namespace HistorialCancha.Infrastructure.Autenticacion;

/// <summary>
/// Parámetros del token JWT, tomados de configuración. La clave (<see cref="Key"/>)
/// es un secreto: sale de variable de entorno, nunca del repo.
/// </summary>
public class OpcionesJwt
{
    /// <summary>Secreto de firma. Mínimo 32 caracteres (256 bits) para HMAC-SHA256.</summary>
    public string Key { get; set; } = string.Empty;

    public string Issuer { get; set; } = "HistorialCancha";
    public string Audience { get; set; } = "HistorialCancha";

    /// <summary>Vida del token, en minutos.</summary>
    public int ExpiraMinutos { get; set; } = 120;
}
