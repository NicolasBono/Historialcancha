namespace HistorialCancha.Domain.Entidades;

/// <summary>
/// Un club de Primera División. Es dato de referencia, no dato del hincha: existe para
/// poblar el selector de rival y que dos partidos contra el mismo club nunca queden
/// escritos distinto.
/// </summary>
/// <remarks>
/// <see cref="Partido.Rival"/> sigue siendo texto y no una FK a esta tabla. Es a propósito:
/// un historial viejo puede tener rivales que hoy no están en Primera, y perderlos porque
/// el club descendió sería absurdo. La tabla ordena lo que se carga de acá en adelante;
/// no gobierna lo que ya pasó.
/// </remarks>
public class Equipo
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Un club que deja la categoría se desactiva, no se borra: los partidos que ya
    /// jugaste contra él siguen siendo válidos.
    /// </summary>
    public bool Activo { get; set; } = true;
}
