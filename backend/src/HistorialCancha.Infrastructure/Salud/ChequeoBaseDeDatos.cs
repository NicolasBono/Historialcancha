using HistorialCancha.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HistorialCancha.Infrastructure.Salud;

/// <summary>
/// Contrato de salud de la base. Vive en Infraestructura, no en Dominio:
/// que la base responda no es una regla de negocio.
/// </summary>
public interface IChequeoBaseDeDatos
{
    Task<bool> RespondeAsync(CancellationToken ct = default);
}

public class ChequeoBaseDeDatos : IChequeoBaseDeDatos
{
    private readonly HistorialContext _contexto;
    private readonly ILogger<ChequeoBaseDeDatos> _log;
    private readonly TimeSpan _presupuesto;

    public ChequeoBaseDeDatos(
        HistorialContext contexto,
        ILogger<ChequeoBaseDeDatos> log,
        OpcionesSalud opciones)
    {
        _contexto = contexto;
        _log = log;
        _presupuesto = TimeSpan.FromMilliseconds(opciones.TimeoutChequeoMs);
    }

    public async Task<bool> RespondeAsync(CancellationToken ct = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(_presupuesto);

        try
        {
            var chequeo = _contexto.Database.CanConnectAsync(cts.Token);

            // Cota dura: si el driver no corta a tiempo (timeout de conexión largo,
            // reintentos), el health check contesta igual dentro de su presupuesto.
            var terminado = await Task.WhenAny(chequeo, Task.Delay(_presupuesto, ct));

            if (terminado != chequeo)
            {
                _log.LogWarning("La base no respondió en {Ms} ms; se reporta como caída.",
                    _presupuesto.TotalMilliseconds);
                return false;
            }

            return await chequeo;
        }
        catch (Exception ex)
        {
            // El health check nunca se cae: informa el problema y sigue respondiendo 200.
            _log.LogWarning(ex, "La base de datos no respondió al chequeo de salud.");
            return false;
        }
    }
}
