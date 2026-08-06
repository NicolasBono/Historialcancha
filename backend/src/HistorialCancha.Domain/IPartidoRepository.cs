using HistorialCancha.Domain.Entidades;

namespace HistorialCancha.Domain;

/// <summary>
/// El dominio declara qué necesita de la persistencia; la infraestructura decide cómo.
/// La flecha de dependencia apunta hacia acá, nunca al revés.
/// </summary>
public interface IPartidoRepository
{
    Task<IReadOnlyList<Partido>> ObtenerTodosAsync(CancellationToken ct = default);

    Task<Partido?> ObtenerPorIdAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Si ya hay un partido en esa fecha. <paramref name="idAExcluir"/> permite editar
    /// un partido sin que la regla lo rechace contra sí mismo.
    /// </summary>
    Task<bool> ExisteEnFechaAsync(DateOnly fecha, int? idAExcluir = null, CancellationToken ct = default);

    Task AgregarAsync(Partido partido, CancellationToken ct = default);

    Task GuardarCambiosAsync(CancellationToken ct = default);

    Task<bool> EliminarAsync(int id, CancellationToken ct = default);
}
