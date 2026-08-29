[![CI](https://github.com/NicolasBono/Historialcancha/actions/workflows/ci.yml/badge.svg)](https://github.com/NicolasBono/Historialcancha/actions/workflows/ci.yml)
# Mi historial de hincha
Una app donde cada hincha se registra con su cuenta y anota los partidos de su equipo y
**cómo los vivió**, para responder la pregunta de fondo: *¿soy cábala o soy yeta?* Cada
usuario ve sólo su propio historial; el equipo que sigue la app es el mismo para todos.

Tres servicios independientes corriendo en local:

```
[ Frontend estático ]  --HTTP/JSON-->  [ Backend Web API ]  --SQL-->  [ PostgreSQL 16 ]
  http://localhost:5500                 http://localhost:5080         contenedor Docker
  HTML + CSS + JS puro                  .NET 9                         historialcancha
```

El frontend **no** vive dentro del backend: son dos orígenes distintos y por eso la API
tiene CORS configurado.

**Estado actual: Épicas 1, 2, 3 y 5 completas** — esqueleto con health check y esquema
aplicado, registro de partidos con sus validaciones, las estadísticas (récord global,
desglose por modalidad con el veredicto cábala / yeta, rachas y ranking de rivales) y las
cuentas de hincha con login (cada usuario ve sólo su historial).
Falta la Épica 4 (desgloses por condición, torneo y temporada).
El detalle está en [docs/](docs/): [brief](docs/brief.md), [PRD](docs/prd.md),
[arquitectura](docs/architecture.md) y [épicas](docs/epics.md).

---

## Requisitos

| Herramienta | Versión usada | Cómo verificar |
|---|---|---|
| .NET SDK | 9.0 | `dotnet --version` |
| Docker | con Compose v2 | `docker compose version` |
| PostgreSQL | 16 (vía contenedor) | — |
| `dotnet-ef` | 9 o superior — **sólo para crear migraciones nuevas** | `dotnet ef --version` |
| Python | 3.x — sólo para servir el frontend | `python --version` |

`dotnet-ef` ya **no** hace falta para poner en marcha la app: las migraciones se aplican
solas al arrancar el backend. Sólo lo necesitás si vas a **crear** una migración nueva:

```powershell
dotnet tool install --global dotnet-ef
```

> El frontend es HTML/CSS/JS puro: no necesita Node ni npm. Python se usa únicamente como
> servidor de archivos estáticos; cualquier otro sirve igual (Live Server de VS Code, `dotnet serve`).

---

## Puesta en marcha

### 1. Base de datos

PostgreSQL corre en un contenedor. La imagen oficial crea la base y el usuario sola, a
partir de variables de entorno —no hace falta ningún script de setup previo—:

```powershell
docker run -d --name historialcancha-db `
  -e POSTGRES_DB=historialcancha `
  -e POSTGRES_USER=historial `
  -e POSTGRES_PASSWORD=tu_password `
  -p 5432:5432 postgres:16
```

`POSTGRES_DB` crea la base, `POSTGRES_USER`/`POSTGRES_PASSWORD` crean el usuario dueño de
esa base. **No crea tablas**: de eso se encargan las migraciones, que el backend aplica
solo al arrancar (ver paso 3).

### 2. Configuración del backend

```powershell
cd backend/src/HistorialCancha.Api
copy appsettings.Development.example.json appsettings.Development.json
```

Editá `appsettings.Development.json` y completá:

- la contraseña real en la cadena de conexión (formato Npgsql:
  `Host=localhost;Database=historialcancha;Username=historial;Password=...`);
- `App:MiEquipo` con el nombre de tu equipo;
- `Jwt:Key` con un secreto largo y aleatorio (mínimo 32 caracteres): es lo que firma los
  tokens de sesión. La app no arranca si falta o es demasiado corto.

Ese archivo está en `.gitignore` y no se versiona nunca. En un contenedor, en vez de este
archivo se pasan los secretos por variables de entorno
(`ConnectionStrings__HistorialCancha`, `Jwt__Key`).

### 3. Esquema de la base

No hay paso manual: **el backend aplica las migraciones pendientes al arrancar**
(`Database.Migrate()`). La primera vez crea `Partidos`, `Vivencias`, sus restricciones y el
índice único por fecha. Si la base no está disponible, el backend **no arranca** a
propósito —una app sin esquema no sirve—.

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

Abrí `http://localhost:5500`. La primera pantalla es el **login**: creá tu cuenta
(nombre, apellido, DNI y contraseña) desde "Crear cuenta" y entrás directo a tu historial.
Cada usuario ve sólo sus propios partidos. Arriba a la derecha vas a ver tu nombre, la
versión, el entorno, el estado del backend y el botón de salir.

---

## Levantarlo con Docker (todo el sistema, un comando)

La alternativa a los cinco pasos de arriba: no hace falta tener .NET, ni PostgreSQL, ni
Python instalados — sólo Docker.

```bash
cp .env.example .env        # y editalo: poné una contraseña y un Jwt:Key de 32+ caracteres
docker compose up -d --build
```

- Frontend: **http://localhost:3000** (nginx sirve los estáticos y proxea `/api` al backend)
- Backend directo, para `curl`/Postman: **http://localhost:8080/api/health**

La base **no** publica puerto: sólo la alcanza el backend por la red interna de compose. El
esquema lo aplican las migraciones al arrancar, así que la primera vez tarda unos segundos
más.

```bash
docker compose ps       # esperá a ver db "healthy"
docker compose logs -f backend
docker compose down     # apaga y conserva los datos
docker compose down -v  # apaga y BORRA el volumen: la base vuelve a nacer vacía
```

> El `.env` no se versiona. Si clonás el repo y no lo creás, el compose corta en el acto
> avisando qué variable falta.

### Correr las imágenes publicadas, sin compilar

```bash
docker compose -f docker-compose.registry.yml up -d
```

Baja las imágenes de `ghcr.io` en vez de construirlas. Sirve para levantar el sistema en
una máquina que no tiene el código.

---

## Configuración

Nada está hardcodeado: todo sale de un archivo de configuración o de una variable de entorno.

### Backend — `appsettings.Development.json`

| Clave | Qué hace | Ejemplo |
|---|---|---|
| `ConnectionStrings:HistorialCancha` | Cadena de conexión a PostgreSQL (formato Npgsql) | `Host=localhost;Database=historialcancha;Username=historial;Password=...` |
| `Jwt:Key` | Secreto que firma los tokens de sesión (mín. 32 caracteres). Nunca se versiona | `una-cadena-larga-y-aleatoria-de-32+` |
| `Jwt:ExpiraMinutos` | Vida del token de sesión, en minutos | `120` |
| `App:Version` | Versión que informa el health check | `1.0.0` |
| `App:MiEquipo` | Equipo propio; un rival nunca puede ser este valor | `Mi Equipo` |
| `App:MinPartidosRanking` | Partidos mínimos para entrar al ranking de rivales | `3` |
| `App:MesInicioTemporada` | Mes de corte de temporada | `7` (julio) |
| `App:MinPartidosVeredicto` | Partidos mínimos por grupo para el veredicto cábala/yeta | `5` |
| `App:UmbralVeredictoPuntos` | Diferencia de efectividad mínima para el veredicto | `10` |
| `Salud:TimeoutChequeoMs` | Presupuesto del chequeo de base en el health check | `600` |
| `Cors:OrigenesPermitidos` | Orígenes autorizados. Nunca `*` | `["http://localhost:5500"]` |

Cualquier clave se puede pisar por variable de entorno con doble guión bajo, sin tocar el
archivo ni recompilar:

```powershell
$env:ConnectionStrings__HistorialCancha = "Host=otro;Database=historialcancha;Username=historial;Password=..."
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
├── frontend/                   # servicio 1 — sitio estático, sin build step
│   ├── login.html              # registro + login (única pantalla sin sesión)
│   ├── index.html
│   ├── css/estilos.css
│   └── js/                     # config.js (local) · auth.js · api.js · login.js · app.js
└── backend/                    # servicio 2 — solución .NET
    └── src/
        ├── HistorialCancha.Domain/          # lógica pura — CERO dependencias externas
        ├── HistorialCancha.Infrastructure/  # EF Core + PostgreSQL (Npgsql) + migraciones
        └── HistorialCancha.Api/             # controllers, CORS, DI, health check
```

La regla que sostiene el diseño: **`HistorialCancha.Domain` no tiene ni un solo
`PackageReference`**. Si alguna vez necesita uno, algo se rompió.

---

## Problemas frecuentes

| Síntoma | Causa probable |
|---|---|
| El backend no arranca y dice que falta la cadena de conexión | No copiaste `appsettings.Development.example.json` a `appsettings.Development.json` (o no definiste `ConnectionStrings__HistorialCancha`) |
| El backend no arranca y dice que falta `Jwt:Key` o es corta | Definí `Jwt:Key` (o `Jwt__Key`) con un secreto de 32+ caracteres |
| El frontend te manda siempre al login | No hay sesión o el token expiró: volvé a ingresar. Cada request usa el token guardado |
| "DNI o contraseña incorrectos" al ingresar | El DNI no está registrado o la clave no coincide (el mensaje es genérico a propósito) |
| "Ya existe un usuario con ese DNI" al registrarte | Ese DNI ya tiene cuenta: ingresá en vez de registrarte |
| El backend no arranca al aplicar migraciones | El contenedor de Postgres no está levantado o la cadena de conexión apunta mal: el arranque falla a propósito si no puede migrar |
| `baseDeDatos: "error"` en el health check | El contenedor de Postgres está detenido o la contraseña no coincide |
| El frontend dice "backend no disponible" | El backend no está levantado, o `apiBaseUrl` en `js/config.js` apunta a otro puerto |
| El navegador bloquea las llamadas por CORS | El origen desde el que abrís el frontend no está en `Cors:OrigenesPermitidos` |
| `password authentication failed for user "historial"` | La contraseña de la cadena de conexión no coincide con `POSTGRES_PASSWORD` del contenedor |
| Abriste `index.html` con doble clic y no anda | Hay que servirlo por HTTP: desde `file://` el navegador bloquea las llamadas |
