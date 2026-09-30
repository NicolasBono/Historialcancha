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
# El proyecto de tests entra al restore porque la solución lo nombra: sin este COPY,
# el "dotnet restore HistorialCancha.sln" falla acá adentro aunque en tu máquina ande.
COPY tests/HistorialCancha.Tests/HistorialCancha.Tests.csproj tests/HistorialCancha.Tests/
RUN dotnet restore HistorialCancha.sln

# Ahora sí el código. Esta capa se invalida en cada commit, y está bien: es barata.
COPY . .
RUN dotnet publish src/HistorialCancha.Api/HistorialCancha.Api.csproj \
    -c Release -o /app/publish --no-restore

# ---- Etapa 2: tests (parte del build: ya tiene el SDK y el código fuente) ----
# Parte de `build` y no de una imagen nueva: así testea exactamente el mismo código
# fuente y las mismas dependencias que se compilaron, y Docker reutiliza esas capas
# sin costo. No lleva WORKDIR propio — hereda /src de build, que es donde está el
# código. No se compila nada acá: el ENTRYPOINT corre recién en el `docker run` del
# pipeline, y por eso los resultados salen por el volumen montado.
FROM build AS test
ENTRYPOINT ["dotnet", "test", "tests/HistorialCancha.Tests/HistorialCancha.Tests.csproj",             "--collect:XPlat Code Coverage",             "-p:CollectCoverage=true",             "-p:Threshold=60",             "-p:ThresholdType=line%2cbranch",             "-p:ThresholdStat=total",             "--logger", "trx;LogFileName=tests.trx",             "--results-directory", "/out"]
# Los dos mecanismos de cobertura conviven y hacen cosas distintas: el --collect deja
# el XML que el pipeline convierte en reporte, y los -p: son los que FRENAN. Sin los
# -p: la cobertura se mide y no exige nada.
#
# Detalles que cuestan una tarde si se tocan:
#  · La coma de ThresholdType va escrita %2c. Con una coma de verdad, MSBuild muere
#    con "MSB1006: Property is not valid" antes de correr un solo test.
#  · ThresholdStat=total no es opcional acá: por defecto coverlet evalúa el umbral
#    POR ENSAMBLADO, y con la Api en 28% cualquier número arriba de eso fallaría
#    siempre, aunque el total dé 64%.
#  · Va -p: y no /p:. Con /p: sobre la solución, MSBuild devuelve "MSB1008: Only one
#    project can be specified".
#  · Qué queda afuera de la cuenta NO se decide acá: sale de los atributos
#    [ExcludeFromCodeCoverage] en el código, que respetan tanto coverlet como
#    ReportGenerator. Así el número que frena y el número que se muestra no pueden
#    desincronizarse.

# ---- Etapa 3: runtime (sin SDK, sin compilador, sin código fuente) ----
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
