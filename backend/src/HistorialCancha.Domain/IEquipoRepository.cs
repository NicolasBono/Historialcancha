using HistorialCancha.Domain.Entidades;

namespace HistorialCancha.Domain;

public interface IEquipoRepository
{
    /// <summary>Los clubes en actividad, en orden alfabético, para poblar el selector.</summary>
    Task<IReadOnlyList<Equipo>> ObtenerActivosAsync(CancellationToken ct = default);
}
