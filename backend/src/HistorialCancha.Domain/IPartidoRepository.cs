using HistorialCancha.Domain.Entidades;

namespace HistorialCancha.Domain;

/// <summary>
/// El dominio declara qué necesita de la persistencia; la infraestructura decide cómo.
/// La flecha de dependencia apunta hacia acá, nunca al revés.
///
/// Todo está acotado por <c>usuarioId</c>: cada hincha sólo ve y toca sus partidos.
/// El aislamiento entre usuarios se garantiza acá, no en el controller.
/// </summary>
public interface IPartidoRepository
{
    Task<IReadOnlyList<Partido>> ObtenerTodosAsync(int usuarioId, CancellationToken ct = default);

    Task<Partido?> ObtenerPorIdAsync(int id, int usuarioId, CancellationToken ct = default);

    /// <summary>
    /// Si ese usuario ya tiene un partido en esa fecha. <paramref name="idAExcluir"/>
    /// permite editar un partido sin que la regla lo rechace contra sí mismo.
    /// </summary>
    Task<bool> ExisteEnFechaAsync(int usuarioId, DateOnly fecha, int? idAExcluir = null, CancellationToken ct = default);

    Task AgregarAsync(Partido partido, CancellationToken ct = default);

    Task GuardarCambiosAsync(CancellationToken ct = default);

    Task<bool> EliminarAsync(int id, int usuarioId, CancellationToken ct = default);
}
