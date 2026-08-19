using HistorialCancha.Domain;
using HistorialCancha.Infrastructure.Autenticacion;
using HistorialCancha.Infrastructure.Persistencia;
using HistorialCancha.Infrastructure.Salud;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HistorialCancha.Infrastructure;

/// <summary>
/// Único punto donde la API sabe que existe EF Core y PostgreSQL.
/// </summary>
public static class InyeccionDeDependencias
{
    public static IServiceCollection AgregarInfraestructura(
        this IServiceCollection servicios,
        string connectionString,
        OpcionesSalud opcionesSalud,
        OpcionesJwt opcionesJwt)
    {
        servicios.AddSingleton(opcionesSalud);
        servicios.AddSingleton(opcionesJwt);

        servicios.AddDbContext<HistorialContext>(opciones =>
            opciones.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(2), errorCodesToAdd: null);
                npgsql.CommandTimeout(15);
            }));

        servicios.AddScoped<IChequeoBaseDeDatos, ChequeoBaseDeDatos>();
        servicios.AddScoped<IPartidoRepository, PartidoRepository>();
        servicios.AddScoped<IUsuarioRepository, UsuarioRepository>();

        // Auth: sin estado, se pueden compartir.
        servicios.AddSingleton<IHasheadorContrasenas, HasheadorContrasenas>();
        servicios.AddSingleton<IGeneradorTokens, GeneradorTokens>();

        return servicios;
    }

    /// <summary>
    /// Aplica las migraciones pendientes al arrancar. En el contenedor la base
    /// nace vacía y nadie corre <c>dotnet ef</c> a mano, así que el esquema lo
    /// crea la propia app. A diferencia del warm-up, esto SÍ tira: una app sin
    /// esquema no sirve, y arrancar "operativa" sin tablas sería mentir. El retry
    /// configurado en <see cref="AgregarInfraestructura"/> cubre el arranque en el
    /// que Postgres todavía no terminó de levantar.
    /// </summary>
    public static async Task MigrarBaseDeDatosAsync(this IServiceProvider servicios, ILogger logger)
    {
        using var alcance = servicios.CreateScope();
        var contexto = alcance.ServiceProvider.GetRequiredService<HistorialContext>();

        logger.LogInformation("Aplicando migraciones pendientes...");
        await contexto.Database.MigrateAsync();
        logger.LogInformation("Migraciones aplicadas: la base está al día.");
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
