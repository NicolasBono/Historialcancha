using HistorialCancha.Domain;
using HistorialCancha.Infrastructure.Persistencia;
using HistorialCancha.Infrastructure.Salud;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HistorialCancha.Infrastructure;

/// <summary>
/// Único punto donde la API sabe que existe EF Core y SQL Server.
/// </summary>
public static class InyeccionDeDependencias
{
    public static IServiceCollection AgregarInfraestructura(
        this IServiceCollection servicios,
        string connectionString,
        OpcionesSalud opcionesSalud)
    {
        servicios.AddSingleton(opcionesSalud);

        servicios.AddDbContext<HistorialContext>(opciones =>
            opciones.UseSqlServer(connectionString, sql =>
            {
                sql.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(2), errorNumbersToAdd: null);
                sql.CommandTimeout(15);
            }));

        servicios.AddScoped<IChequeoBaseDeDatos, ChequeoBaseDeDatos>();
        servicios.AddScoped<IPartidoRepository, PartidoRepository>();
        servicios.AddScoped<IEquipoRepository, EquipoRepository>();

        return servicios;
    }

    /// <summary>
    /// Paga al arrancar el costo de construir el modelo de EF y abrir la primera
    /// conexión, para que no lo pague la primera request al health check.
    /// Nunca tira: si la base no está, la API igual queda operativa.
    /// </summary>
    public static async Task CalentarInfraestructuraAsync(this IServiceProvider servicios, ILogger logger)
    {
        using var alcance = servicios.CreateScope();
        var contexto = alcance.ServiceProvider.GetRequiredService<HistorialContext>();

        try
        {
            var responde = await contexto.Database.CanConnectAsync();
            logger.LogInformation("Chequeo inicial de la base de datos: {Estado}",
                responde ? "ok" : "sin conexión");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "La base de datos no respondió al arrancar. La API queda operativa igual.");
        }
    }
}
