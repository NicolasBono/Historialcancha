namespace HistorialCancha.Domain.Entidades;

/// <summary>
/// El hecho objetivo: qué pasó en la cancha.
/// El resultado NO se persiste, se deriva de los goles.
/// </summary>
public class Partido
{
    public int Id { get; set; }
    public DateOnly Fecha { get; set; }
    public string Rival { get; set; } = string.Empty;
    public string Torneo { get; set; } = string.Empty;
    public Condicion Condicion { get; set; }
    public string? Estadio { get; set; }
    public int GolesAFavor { get; set; }
    public int GolesEnContra { get; set; }
    public DateTime CreadoEn { get; set; }

    public Vivencia? Vivencia { get; set; }

    public Resultado Resultado =>
        GolesAFavor > GolesEnContra ? Resultado.Victoria :
        GolesAFavor == GolesEnContra ? Resultado.Empate :
        Resultado.Derrota;

    public int DiferenciaDeGol => GolesAFavor - GolesEnContra;
}
