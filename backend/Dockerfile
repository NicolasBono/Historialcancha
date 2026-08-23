# syntax=docker/dockerfile:1

# ---- Etapa 1: build (SDK completo: compilador y herramientas) ----
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Los archivos de proyecto PRIMERO, y el restore antes de copiar el código.
# El restore es el paso caro (baja NuGet) y sólo depende de los .csproj: separarlo
# hace que tocar una línea de código reuse esta capa del cache en vez de rehacerla.
COPY HistorialCancha.sln .
COPY src/HistorialCancha.Domain/HistorialCancha.Domain.csproj src/HistorialCancha.Domain/
COPY src/HistorialCancha.Infrastructure/HistorialCancha.Infrastructure.csproj src/HistorialCancha.Infrastructure/
COPY src/HistorialCancha.Api/HistorialCancha.Api.csproj src/HistorialCancha.Api/
RUN dotnet restore HistorialCancha.sln

# Ahora sí el código. Esta capa se invalida en cada commit, y está bien: es barata.
COPY . .
RUN dotnet publish src/HistorialCancha.Api/HistorialCancha.Api.csproj \
    -c Release -o /app/publish --no-restore

# ---- Etapa 2: runtime (sin SDK, sin compilador, sin código fuente) ----
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Usuario sin privilegios que ya trae la imagen de .NET. Si alguien logra ejecutar
# algo acá adentro, no lo hace como root.
USER app

# aspnet:9.0 escucha en 8080 por defecto (en .NET 6/7 era el 80).
# EXPOSE documenta; quien publica el puerto es el compose.
EXPOSE 8080

# El contenedor ES la API: ENTRYPOINT, no CMD.
ENTRYPOINT ["dotnet", "HistorialCancha.Api.dll"]
