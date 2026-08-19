namespace HistorialCancha.Domain.Entidades;

/// <summary>
/// El hincha dueño de un historial. Se identifica por su DNI para entrar.
/// La contraseña nunca se guarda en claro: sólo su hash, calculado en la
/// infraestructura. El dominio ignora cómo se hashea.
/// </summary>
public class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Dni { get; set; } = string.Empty;
    public string HashContrasena { get; set; } = string.Empty;
}
