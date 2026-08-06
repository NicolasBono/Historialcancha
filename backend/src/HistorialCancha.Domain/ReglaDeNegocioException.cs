namespace HistorialCancha.Domain;

/// <summary>
/// Regla de negocio incumplida. La API la traduce a HTTP 400;
/// el dominio no sabe que existe HTTP.
/// </summary>
public class ReglaDeNegocioException : Exception
{
    /// <summary>Identificador estable de la regla, para que el cliente pueda reaccionar.</summary>
    public string Regla { get; }

    public ReglaDeNegocioException(string regla, string mensaje) : base(mensaje)
    {
        Regla = regla;
    }
}
