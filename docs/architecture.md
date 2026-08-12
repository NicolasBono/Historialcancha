# Arquitectura — Mi historial de hincha (Fase 1)

Tres servicios independientes corriendo en la máquina local:

```
[ Frontend estático ]  --HTTP/JSON-->  [ Backend Web API ]  --TDS-->  [ SQL Server ]
  http://localhost:5500                 http://localhost:5080          .\SQLEXPRESS
  HTML + CSS + JS puro                  .NET Core                      HistorialCancha
```

Orígenes distintos por diseño: el frontend **no** vive dentro del backend, no hay `wwwroot`
y la API no sirve archivos estáticos. Por eso el backend necesita CORS.

## Estructura del repositorio

```
Historialcancha/
├── docs/
├── frontend/                       # servicio 1 — sitio estático
│   ├── index.html                  # listado de partidos + alta/edición
│   ├── estadisticas.html           # récord, modalidades, rachas, rivales, desgloses
│   ├── css/estilos.css
│   ├── js/
│   │   ├── config.js               # generado desde config.example.js — NO se versiona
│   │   ├── api.js                  # único punto que conoce la URL del backend
│   │   ├── partidos.js             # listado + modal de alta/edición
│   │   ├── graficos.js             # barras, reparto G-E-P y forma reciente
│   │   ├── estadisticas.js
│   │   └── app.js                  # badge de versión/entorno + estado del backend
│   └── config.example.js
├── backend/                        # servicio 2 — solución .NET
│   ├── HistorialCancha.sln
│   ├── src/
│   │   ├── HistorialCancha.Domain/           # lógica pura, cero dependencias externas
│   │   ├── HistorialCancha.Infrastructure/   # EF Core + SQL Server + migraciones
│   │   └── HistorialCancha.Api/              # controllers, DTOs, CORS, DI, health
│   └── appsettings.Development.example.json
└── README.md
```

## Backend — capas y dependencias

Tres proyectos, con las flechas de dependencia apuntando siempre hacia adentro:

```
HistorialCancha.Api  ──►  HistorialCancha.Infrastructure  ──►  HistorialCancha.Domain
        └──────────────────────────────────────────────────────────►┘
```

| Proyecto | Contiene | Referencias NuGet |
|---|---|---|
| **Domain** | Entidades (`Partido`, `Vivencia`), enums (`Modalidad`, `Condicion`, `Resultado`), servicios de dominio (`ValidadorPartido`, y en `Estadisticas/`: `CalculadoraRecord`, `CalculadoraModalidad`, `CalculadoraRachas`, `RankingRivales`, `CalculadoraDesgloses`, `CalculadoraTemporada`), interfaces de repositorio (`IPartidoRepository`), excepción `ReglaDeNegocioException`, opciones de negocio (`OpcionesDominio`) | **ninguna** |
| **Infrastructure** | `HistorialContext` (DbContext), configuraciones de EF, `PartidoRepository`, migraciones | `Microsoft.EntityFrameworkCore.SqlServer`, `.Design` |
| **Api** | Controllers, DTOs de request/response, mapeo DTO↔entidad, política CORS, registro de DI, `/api/health`, middleware de errores | `Microsoft.AspNetCore.*`, `EntityFrameworkCore.Design` (sólo para `dotnet ef`) |

`Domain` no compila contra EF, ni contra ASP.NET, ni contra ningún DTO. Es un
class library sin `PackageReference`: esa ausencia es la garantía verificable.

## Modelo de datos (MSSQL)

Base `HistorialCancha`. Dos tablas en relación 1:1 — el partido es el hecho objetivo,
la vivencia es cómo lo vivió el hincha.

### `Partidos`

| Columna | Tipo | Notas |
|---|---|---|
| `Id` | `INT IDENTITY` | PK |
| `Fecha` | `DATE NOT NULL` | índice **único** → una fecha, un partido (FR10) |
| `Rival` | `NVARCHAR(80) NOT NULL` | índice no único, para el ranking de rivales |
| `Torneo` | `NVARCHAR(80) NOT NULL` | |
| `Condicion` | `TINYINT NOT NULL` | 0 = Local, 1 = Visitante. `CHECK (Condicion IN (0,1))` |
| `Estadio` | `NVARCHAR(120) NULL` | |
| `GolesAFavor` | `INT NOT NULL` | `CHECK (GolesAFavor >= 0)` |
| `GolesEnContra` | `INT NOT NULL` | `CHECK (GolesEnContra >= 0)` |
| `CreadoEn` | `DATETIME2 NOT NULL` | `DEFAULT SYSUTCDATETIME()` |

El resultado (V/E/D) **no se persiste**: se deriva en el dominio a partir de los goles (FR13).

### `Vivencias`

| Columna | Tipo | Notas |
|---|---|---|
| `PartidoId` | `INT` | PK y FK a `Partidos(Id)` `ON DELETE CASCADE` |
| `Modalidad` | `TINYINT NOT NULL` | 0 EnCancha, 1 TV, 2 Streaming, 3 Radio, 4 NoLoVi. `CHECK (Modalidad BETWEEN 0 AND 4)` |
| `Sector` | `NVARCHAR(80) NULL` | |
| `ConQuien` | `NVARCHAR(120) NULL` | |
| `Nota` | `TINYINT NULL` | `CHECK (Nota BETWEEN 1 AND 10)` |

Los `CHECK` duplican reglas que ya viven en el dominio. Es intencional: el dominio las
aplica y explica, la base las garantiza.

### `Equipos`

| Columna | Tipo | Notas |
|---|---|---|
| `Id` | `INT IDENTITY` | PK |
| `Nombre` | `NVARCHAR(80) NOT NULL` | índice **único** `UX_Equipos_Nombre` |
| `Activo` | `BIT NOT NULL` | `DEFAULT 1`; un club que baja se desactiva, no se borra |

Dato de referencia, no dato del hincha: puebla el selector de rival para que dos partidos
contra el mismo club no queden escritos distinto. Los 30 clubes de Primera se cargan con
`HasData` **dentro de la migración**, así la base se reconstruye de cero con los equipos
adentro y no queda un script suelto que alguien tenga que acordarse de correr (NFR5).

`Partidos.Rival` sigue siendo texto y **no** una FK a esta tabla. Es deliberado: un
historial viejo puede tener rivales que hoy no están en Primera, y perderlos porque el
club descendió sería absurdo. La tabla ordena lo que se carga de acá en adelante; no
gobierna lo que ya pasó. El frontend, al editar un partido cuyo rival no está en la
lista, le agrega su propia opción para no perderlo.

**Esquema versionado con migraciones de EF Core** (`dotnet ef migrations add` /
`dotnet ef database update`). Ningún cambio manual sobre la base.

**Creación de la base y del usuario: `db/setup.sql`.** El login de la aplicación no tiene
permiso para crear bases, así que hay un paso previo de una sola vez, ejecutado con
autenticación de Windows, que crea `HistorialCancha`, crea el login y lo hace `db_owner`
de esa base y nada más. Ese script **no crea tablas**: el límite entre los dos es claro
—`setup.sql` es permisos, las migraciones son esquema—. La contraseña se pasa como
parámetro de `sqlcmd`, no está en el archivo.

## API — endpoints

Base: `http://localhost:5080/api`. JSON en `camelCase`, fechas `YYYY-MM-DD`.

| Método | Ruta | Descripción |
|---|---|---|
| `GET` | `/health` | Estado del servicio: `status`, `version`, `entorno`, `baseDeDatos` (`ok`/`error`), `miEquipo` |
| `GET` | `/partidos` | Listado completo, fecha descendente, con resultado y modalidad resueltos |
| `GET` | `/partidos/{id}` | Detalle de un partido con su vivencia |
| `POST` | `/partidos` | Alta. 201 + `Location`, o 400 con el detalle de la regla incumplida |
| `PUT` | `/partidos/{id}` | Edición completa (partido + vivencia). 204 / 400 / 404 |
| `DELETE` | `/partidos/{id}` | Baja en cascada. 204 / 404 |
| `GET` | `/estadisticas/global` | PJ, G-E-P, GF, GC, DG, efectividad, promedios de gol |
| `GET` | `/estadisticas/modalidad` | El mismo récord por cada modalidad + veredicto cábala/yeta |
| `GET` | `/estadisticas/rachas` | Racha actual e histórica: invicto, sin ganar, sin recibir goles |
| `GET` | `/estadisticas/rivales` | Talismán, maldición y tabla de rivales sobre el umbral |
| `GET` | `/estadisticas/desgloses` | Récord por condición, por torneo y por temporada |
| `GET` | `/equipos` | Clubes de Primera en actividad, alfabético, sin el equipo propio |

Errores: `ReglaDeNegocioException` → 400 con `{ "error": "...", "regla": "..." }`,
resuelto por un único middleware. Los 500 devuelven un mensaje genérico y loguean el detalle.

## CORS

Política nombrada `FrontendLocal`, registrada en `Program.cs` y aplicada globalmente:

```json
"Cors": { "OrigenesPermitidos": [ "http://localhost:5500" ] }
```

- Orígenes leídos de configuración, nunca hardcodeados y nunca `*` (NFR3).
- Métodos permitidos: `GET, POST, PUT, DELETE, OPTIONS`. Headers: `Content-Type`.
- Sin credenciales: no hay cookies ni sesión.
- `UseCors` se ubica antes de `MapControllers` para que el preflight `OPTIONS` responda.

## Configuración

Nada hardcodeado, ni siquiera para local (NFR6). Un archivo de ejemplo versionado y
un archivo real ignorado por Git en cada servicio.

### Backend — `appsettings.Development.json` (ignorado) + variables de entorno

```json
{
  "ConnectionStrings": {
    "HistorialCancha": "Server=.\\SQLEXPRESS;Database=HistorialCancha;User Id=admin;Password=admin123;TrustServerCertificate=True"
  },
  "App": {
    "Version": "1.0.0",
    "MiEquipo": "Mi Equipo",
    "MinPartidosRanking": 3,
    "MesInicioTemporada": 7,
    "MinPartidosVeredicto": 5,
    "UmbralVeredictoPuntos": 10
  },
  "Salud": { "TimeoutChequeoMs": 600 },
  "Cors": { "OrigenesPermitidos": [ "http://localhost:5500" ] }
}
```

`Salud:TimeoutChequeoMs` es el presupuesto de tiempo del chequeo de base dentro del health
check. Existe porque el timeout de conexión de la cadena es demasiado largo para un endpoint
que tiene que responder rápido: si la base no contesta dentro del presupuesto, el health
check corta y la reporta caída en vez de esperarla. Además, la API abre la primera conexión
al arrancar, para que ese costo no lo pague la primera request.

El entorno sale de `ASPNETCORE_ENVIRONMENT`. Cualquier clave se pisa por variable de
entorno con la convención de doble guión bajo
(`ConnectionStrings__HistorialCancha`, `App__MiEquipo`), sin tocar el archivo.
Los valores de `App` se inyectan al dominio como `OpcionesDominio` vía `IOptions`.

### Frontend — `js/config.js` (ignorado, se copia de `config.example.js`)

```js
window.APP_CONFIG = {
  apiBaseUrl: "http://localhost:5080/api",
  version: "1.0.0",
  entorno: "Development"
};
```

`api.js` es el único módulo que lee `apiBaseUrl`; ningún otro archivo conoce la URL del
backend. `app.js` pinta `version` y `entorno` en el badge del header (FR25) y consulta
`/health` para el indicador de disponibilidad (FR26).

## Separación entre lógica de dominio y acceso a datos

Esta es la decisión estructural que el proyecto no puede perder.

**Regla:** el dominio recibe datos y devuelve resultados. No sabe de dónde vinieron
ni a dónde van.

1. **El dominio declara la interfaz, la infraestructura la implementa.**
   `IPartidoRepository` vive en `Domain`; `PartidoRepository` (EF Core) vive en
   `Infrastructure`. La flecha de dependencia apunta hacia el dominio, nunca al revés.

2. **Los cálculos son funciones puras sobre colecciones en memoria.**
   `CalculadoraRecord.Calcular(IEnumerable<Partido>)`, `CalculadoraRachas.Calcular(...)`,
   `RankingRivales.Resolver(...)` reciben partidos ya materializados y devuelven
   objetos de resultado. No hay `IQueryable`, ni `DbContext`, ni `async` en el dominio:
   ninguna estadística se calcula en SQL.

3. **Las validaciones son del dominio, no del controller ni de atributos.**
   `ValidadorPartido` concentra FR7 a FR11 y lanza `ReglaDeNegocioException`. El
   controller no valida reglas de negocio; sólo traduce la excepción a HTTP 400.
   La única regla que necesita consultar el estado existente —"no dos partidos el mismo
   día" (FR10)— recibe como parámetro el dato ya leído (`bool existeEnEsaFecha`),
   no un repositorio.

4. **Las entidades de dominio son las que se persisten, pero no están decoradas.**
   `Partido` y `Vivencia` no llevan `[Table]`, `[Key]` ni atributos de EF: el mapeo se
   define con Fluent API en `Infrastructure`. Así el dominio ignora que EF existe.

5. **Los DTOs no cruzan hacia adentro.**
   Los DTOs de request/response viven en `Api` y se mapean a entidades en el borde.
   El dominio nunca ve un tipo que exista por razones de transporte HTTP.

   *Excepción explícita para las estadísticas:* los resultados de las calculadoras
   (`Record`, `ResumenModalidad`, `ResumenRachas`, `ResumenRivales`, `ResumenDesgloses`) se serializan
   directamente. Son `record` inmutables de sólo lectura, sin dependencias de EF ni de
   ASP.NET, y duplicarlos en DTOs idénticos sería ceremonia pura en una app que quiere
   mantenerse chica. La regla que importa —que el dominio no dependa del transporte—
   se sigue cumpliendo: la flecha nunca se invierte.

6. **Flujo de una request de estadísticas:**
   `Controller` → `IPartidoRepository.ObtenerTodosAsync()` (Infrastructure, único punto
   async con la base) → `List<Partido>` → calculadora de dominio (síncrona y pura) →
   DTO de respuesta → JSON.

Verificación simple de que la separación se sostiene: si `HistorialCancha.Domain.csproj`
alguna vez necesita un `PackageReference`, la regla se rompió.

## Preparado para la Fase 2

Decisiones de este diseño que permiten sumar testing y pipeline después sin reescribir:

- **Dominio sin dependencias ni I/O**: las calculadoras y el validador son funciones puras
  sobre listas; se pueden ejercitar directamente, sin base ni servidor.
- **Repositorio detrás de una interfaz declarada en el dominio**: se puede sustituir la
  implementación sin tocar la lógica.
- **Configuración externalizada y sobreescribible por variables de entorno**: apuntar la
  app a otra base o a otro origen es cambiar una variable, no recompilar.
- **Esquema en migraciones versionadas**: la base se reconstruye de cero desde el repositorio
  con un comando, de forma repetible.
- **Health check con estado de la base**: hay un punto único ya listo para verificar
  automáticamente que el servicio arrancó y está sano.
- **Servicios desacoplados y sin build step en el frontend**: cada uno se empaqueta y arranca
  por separado, y el estático no arrastra tooling.
- **Versión y entorno como valores de configuración visibles**: ya existe el lugar donde
  inyectar el número de build y el nombre del entorno.

*(Enumeración de habilitadores, no diseño de la Fase 2: nada de eso se implementa ahora.)*
