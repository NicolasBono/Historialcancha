namespace HistorialCancha.Domain.Entidades;

/// <summary>
/// Cómo vivió el hincha ese partido. Relación 1:1 con <see cref="Partido"/>.
/// </summary>
public class Vivencia
{
    public int PartidoId { get; set; }
    public Modalidad Modalidad { get; set; }
    public string? Sector { get; set; }
    public string? ConQuien { get; set; }
    public byte? Nota { get; set; }

    public Partido? Partido { get; set; }
}
