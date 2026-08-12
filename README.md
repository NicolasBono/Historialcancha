# Mi historial de hincha

Una app donde un hincha registra los partidos de su equipo y **cómo los vivió**, para
responder la pregunta de fondo: *¿soy cábala o soy yeta?*

Tres servicios independientes corriendo en local:

```
[ Frontend estático ]  --HTTP/JSON-->  [ Backend Web API ]  --TDS-->  [ SQL Server ]
  http://localhost:5500                 http://localhost:5080          .\SQLEXPRESS
  HTML + CSS + JS puro                  .NET 9                         HistorialCancha
```

El frontend **no** vive dentro del backend: son dos orígenes distintos y por eso la API
tiene CORS configurado.

**Estado actual: Fase 1 completa** — las cuatro épicas: esqueleto con health check y
esquema aplicado, registro de partidos con sus validaciones, y las estadísticas: récord
global, desglose por modalidad con el veredicto cábala / yeta, rachas, ranking de rivales
y desgloses por condición, torneo y temporada.

El alta se abre en un modal y el rival **se elige de una lista**: los 30 clubes de Primera
viven en la tabla `Equipos`, que se carga sola con las migraciones. Las estadísticas se
grafican con barras hechas en HTML y CSS, sin ninguna librería.
El detalle está en [docs/](docs/): [brief](docs/brief.md), [PRD](docs/prd.md),
[arquitectura](docs/architecture.md) y [épicas](docs/epics.md).

---

## Requisitos

| Herramienta | Versión usada | Cómo verificar |
|---|---|---|
| .NET SDK | 9.0 | `dotnet --version` |
| SQL Server Express | 2025 | `Get-Service MSSQL*` |
| `dotnet-ef` | 9 o superior | `dotnet ef --version` |
| Python | 3.x — sólo para servir el frontend | `python --version` |

Si falta `dotnet-ef`:

```powershell
dotnet tool install --global dotnet-ef
```

> El frontend es HTML/CSS/JS puro: no necesita Node ni npm. Python se usa únicamente como
> servidor de archivos estáticos; cualquier otro sirve igual (Live Server de VS Code, `dotnet serve`).

---

## Puesta en marcha

### 1. Base de datos

La aplicación se conecta con un login de SQL Server que **no** tiene permiso para crear
bases —y no debería tenerlo—. Por eso hay un script de setup que se corre **una sola vez**
con autenticación de Windows:

```powershell
sqlcmd -S ".\SQLEXPRESS" -E -C -i db/setup.sql -v LoginApp="admin" LoginPassword="tu_password"
```

Ese script crea la base `HistorialCancha`, crea el login si no existe y lo hace `db_owner`
de esa base únicamente. **No crea tablas**: de eso se encargan las migraciones.

> Requiere que SQL Server tenga habilitada la autenticación mixta.
> Para verificarlo: `sqlcmd -S ".\SQLEXPRESS" -E -C -Q "SELECT SERVERPROPERTY('IsIntegratedSecurityOnly')"` — tiene que devolver `0`.

### 2. Configuración del backend

```powershell
cd backend/src/HistorialCancha.Api
copy appsettings.Development.example.json appsettings.Development.json
```

Editá `appsettings.Development.json` y completá:

- la contraseña real en la cadena de conexión;
- `App:MiEquipo` con el nombre de tu equipo.

Ese archivo está en `.gitignore` y no se versiona nunca.

### 3. Esquema de la base

Desde `backend/`:

```powershell
dotnet ef database update -p src/HistorialCancha.Infrastructure -s src/HistorialCancha.Api
```

Crea `Partidos`, `Vivencias` y `Equipos`, sus restricciones y el índice único por fecha.
`Equipos` queda cargada con los 30 clubes de Primera: el seed va dentro de la migración,
así que no hay ningún script extra que correr.

> **Los equipos son los de la temporada 2025.** Cuando cambie la categoría, la forma de
> actualizarla es otra migración (`dotnet ef migrations add`), nunca un `UPDATE` a mano:
> la lista vive en `EquipoConfiguration.PrimeraDivision`. Un club que desciende se marca
> `Activo = 0` en vez de borrarse, para que los partidos que ya jugaste contra él sigan
> siendo válidos.

### 4. Backend

Desde `backend/src/HistorialCancha.Api`:

```powershell
dotnet run
```

Queda escuchando en `http://localhost:5080`. Verificalo:

```powershell
curl http://localhost:5080/api/health
```

```json
{"status":"ok","version":"1.0.0","entorno":"Development","baseDeDatos":"ok","miEquipo":"..."}
```

### 5. Frontend

Copiá la configuración de ejemplo y serví la carpeta, en **otra terminal**:

```powershell
cd frontend
copy js\config.example.js js\config.js
python -m http.server 5500
```

Abrí `http://localhost:5500`. Arriba a la derecha vas a ver la versión, el entorno y el
estado del backend.

---

## Configuración

Nada está hardcodeado: todo sale de un archivo de configuración o de una variable de entorno.

### Backend — `appsettings.Development.json`

| Clave | Qué hace | Ejemplo |
|---|---|---|
| `ConnectionStrings:HistorialCancha` | Cadena de conexión a MSSQL | `Server=.\SQLEXPRESS;Database=HistorialCancha;User Id=admin;Password=...;TrustServerCertificate=True` |
| `App:Version` | Versión que informa el health check | `1.0.0` |
| `App:MiEquipo` | Equipo propio; un rival nunca puede ser este valor, y queda fuera del selector | `Racing Club` |
| `App:MinPartidosRanking` | Partidos mínimos para entrar al ranking de rivales | `3` |
| `App:MesInicioTemporada` | Mes de corte de temporada | `7` (julio) |
| `App:MinPartidosVeredicto` | Partidos mínimos por grupo para el veredicto cábala/yeta | `5` |
| `App:UmbralVeredictoPuntos` | Diferencia de efectividad mínima para el veredicto | `10` |
| `Salud:TimeoutChequeoMs` | Presupuesto del chequeo de base en el health check | `600` |
| `Cors:OrigenesPermitidos` | Orígenes autorizados. Nunca `*` | `["http://localhost:5500"]` |

Cualquier clave se puede pisar por variable de entorno con doble guión bajo, sin tocar el
archivo ni recompilar:

```powershell
$env:ConnectionStrings__HistorialCancha = "Server=otro;..."
$env:App__MiEquipo = "Otro equipo"
```

El entorno sale de `ASPNETCORE_ENVIRONMENT`.

### Frontend — `js/config.js`

| Clave | Qué hace |
|---|---|
| `apiBaseUrl` | URL base del backend. Es el **único** lugar del frontend donde aparece |
| `version` | Versión mostrada en la barra superior |
| `entorno` | Entorno mostrado en la barra superior |

---

## Estructura

```
├── docs/                       # brief, PRD, arquitectura y épicas
├── db/setup.sql                # creación de base y usuario (una sola vez, como sysadmin)
├── frontend/                   # servicio 1 — sitio estático, sin build step
│   ├── index.html              # historial + modal de alta/edición
│   ├── estadisticas.html
│   ├── css/estilos.css
│   └── js/                     # config.js (local) · api.js · app.js · graficos.js
└── backend/                    # servicio 2 — solución .NET
    └── src/
        ├── HistorialCancha.Domain/          # lógica pura — CERO dependencias externas
        ├── HistorialCancha.Infrastructure/  # EF Core + SQL Server + migraciones
        └── HistorialCancha.Api/             # controllers, CORS, DI, health check
```

La regla que sostiene el diseño: **`HistorialCancha.Domain` no tiene ni un solo
`PackageReference`**. Si alguna vez necesita uno, algo se rompió.

---

## Problemas frecuentes

| Síntoma | Causa probable |
|---|---|
| El backend no arranca y dice que falta la cadena de conexión | No copiaste `appsettings.Development.example.json` a `appsettings.Development.json` |
| `baseDeDatos: "error"` en el health check | SQL Server detenido, contraseña incorrecta, o no corriste `db/setup.sql` |
| El frontend dice "backend no disponible" | El backend no está levantado, o `apiBaseUrl` en `js/config.js` apunta a otro puerto |
| El navegador bloquea las llamadas por CORS | El origen desde el que abrís el frontend no está en `Cors:OrigenesPermitidos` |
| `Login failed for user 'admin'` | El login no existe o la contraseña no coincide: volvé a correr `db/setup.sql` |
| Abriste `index.html` con doble clic y no anda | Hay que servirlo por HTTP: desde `file://` el navegador bloquea las llamadas |
