using System.Text;
using System.Text.Json.Serialization;
using HistorialCancha.Api.Middleware;
using HistorialCancha.Domain;
using HistorialCancha.Infrastructure;
using HistorialCancha.Infrastructure.Autenticacion;
using HistorialCancha.Infrastructure.Salud;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// --- Configuración: nada hardcodeado, todo de appsettings o variables de entorno ---
var connectionString = builder.Configuration.GetConnectionString("HistorialCancha")
    ?? throw new InvalidOperationException(
        "Falta la cadena de conexión 'ConnectionStrings:HistorialCancha'. " +
        "Copiá appsettings.Development.example.json a appsettings.Development.json o definí " +
        "la variable de entorno ConnectionStrings__HistorialCancha.");

var origenesPermitidos = builder.Configuration
    .GetSection("Cors:OrigenesPermitidos").Get<string[]>() ?? [];

if (origenesPermitidos.Length == 0)
    throw new InvalidOperationException(
        "Falta 'Cors:OrigenesPermitidos'. El frontend corre en otro origen y sin esto el navegador lo bloquea.");

builder.Services.Configure<OpcionesDominio>(builder.Configuration.GetSection("App"));

if (string.IsNullOrWhiteSpace(builder.Configuration["App:MiEquipo"]))
    throw new InvalidOperationException(
        "Falta 'App:MiEquipo'. Sin ese valor no se puede validar que el rival no sea tu propio equipo.");

var opcionesSalud = builder.Configuration.GetSection("Salud").Get<OpcionesSalud>() ?? new OpcionesSalud();

// El secreto de firma del token nunca se hardcodea: sale de config/entorno.
var opcionesJwt = builder.Configuration.GetSection("Jwt").Get<OpcionesJwt>() ?? new OpcionesJwt();

if (string.IsNullOrWhiteSpace(opcionesJwt.Key) || opcionesJwt.Key.Length < 32)
    throw new InvalidOperationException(
        "Falta 'Jwt:Key' o es demasiado corta (mínimo 32 caracteres para HMAC-SHA256). " +
        "Definí la variable de entorno Jwt__Key con un secreto largo y aleatorio.");

// --- Servicios ---
builder.Services.AgregarInfraestructura(connectionString, opcionesSalud, opcionesJwt);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opciones =>
    {
        opciones.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = opcionesJwt.Issuer,
            ValidateAudience = true,
            ValidAudience = opcionesJwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(opcionesJwt.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)   // sin la tolerancia default de 5 min
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        // Enums como texto: el frontend lee "EnCancha", no 0.
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Los errores de formato del cuerpo salen con la misma forma { error, regla }
// que los de negocio: el frontend tiene un solo caso que manejar.
builder.Services.Configure<ApiBehaviorOptions>(o =>
{
    o.InvalidModelStateResponseFactory = contexto =>
    {
        // System.Text.Json deja la ruta del campo como "$.golesAFavor";
        // el nombre del parámetro del action no le sirve a nadie.
        var campo = contexto.ModelState
            .Where(e => e.Value?.Errors.Count > 0)
            .Select(e => e.Key)
            .FirstOrDefault(k => k.StartsWith("$.", StringComparison.Ordinal))?[2..];

        var mensaje = string.IsNullOrEmpty(campo)
            ? "El cuerpo de la petición no tiene el formato esperado."
            : $"El campo '{campo}' tiene un valor inválido.";

        return new BadRequestObjectResult(new { error = mensaje, regla = "formato-invalido" });
    };
});

const string PoliticaCors = "FrontendLocal";
builder.Services.AddCors(opciones =>
{
    opciones.AddPolicy(PoliticaCors, politica => politica
        .WithOrigins(origenesPermitidos)   // nunca AllowAnyOrigin: la lista sale de configuración
        .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
        .WithHeaders("Content-Type", "Authorization"));
});

var app = builder.Build();

// --- Pipeline ---
app.UseMiddleware<ManejadorDeErrores>();
app.UseCors(PoliticaCors);   // antes de MapControllers, para que el preflight OPTIONS responda
app.UseAuthentication();     // valida el token y arma el User…
app.UseAuthorization();      // …y recién ahí [Authorize] puede decidir
app.MapControllers();

app.Logger.LogInformation("Entorno: {Entorno} | Orígenes CORS permitidos: {Origenes}",
    app.Environment.EnvironmentName, string.Join(", ", origenesPermitidos));

// La base nace vacía en el contenedor: la app aplica el esquema al arrancar.
// Si esto falla, la app no arranca — a propósito.
await app.Services.MigrarBaseDeDatosAsync(app.Logger);

await app.Services.CalentarInfraestructuraAsync(app.Logger);

app.Run();
