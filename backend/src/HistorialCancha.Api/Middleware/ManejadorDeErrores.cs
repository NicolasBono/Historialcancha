using System.Text.Json;
using HistorialCancha.Domain;

namespace HistorialCancha.Api.Middleware;

/// <summary>
/// Único traductor de excepciones a HTTP. Los controllers no atrapan errores de negocio:
/// el dominio lanza <see cref="ReglaDeNegocioException"/> y acá se convierte en 400.
/// </summary>
public class ManejadorDeErrores
{
    private readonly RequestDelegate _siguiente;
    private readonly ILogger<ManejadorDeErrores> _log;

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public ManejadorDeErrores(RequestDelegate siguiente, ILogger<ManejadorDeErrores> log)
    {
        _siguiente = siguiente;
        _log = log;
    }

    public async Task InvokeAsync(HttpContext contexto)
    {
        try
        {
            await _siguiente(contexto);
        }
        catch (ReglaDeNegocioException ex)
        {
            _log.LogInformation("Regla de negocio rechazada: {Regla} — {Mensaje}", ex.Regla, ex.Message);
            await EscribirAsync(contexto, StatusCodes.Status400BadRequest,
                new { error = ex.Message, regla = ex.Regla });
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Error no controlado en {Metodo} {Ruta}",
                contexto.Request.Method, contexto.Request.Path);
            await EscribirAsync(contexto, StatusCodes.Status500InternalServerError,
                new { error = "Ocurrió un error inesperado.", regla = "interno" });
        }
    }

    private static async Task EscribirAsync(HttpContext contexto, int codigo, object cuerpo)
    {
        if (contexto.Response.HasStarted) return;

        contexto.Response.Clear();
        contexto.Response.StatusCode = codigo;
        contexto.Response.ContentType = "application/json; charset=utf-8";
        await contexto.Response.WriteAsync(JsonSerializer.Serialize(cuerpo, Json));
    }
}
