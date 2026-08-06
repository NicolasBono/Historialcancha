namespace HistorialCancha.Infrastructure.Salud;

/// <summary>
/// Presupuesto de tiempo del chequeo de salud. Viene de configuración
/// para no atarlo al timeout de conexión de la cadena.
/// </summary>
public class OpcionesSalud
{
    public int TimeoutChequeoMs { get; set; } = 600;
}
