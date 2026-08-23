# Evidencias

---

## TP2 — Contenedores y Compose

### Estado

Los archivos están escritos y **el compose valida** (`docker compose config --quiet` pasa
sin errores, y no necesita el motor de Docker prendido).

Lo que falta se completa en una máquina con Docker funcionando. En la PC de escritorio
donde se escribió esto, Docker Desktop no arranca: la placa (ASUS PRIME B550M-A AC, Ryzen
7 5700X) tiene la virtualización deshabilitada en el firmware —`SVM Mode` en `Disabled`—,
y el síntoma derivado es que la distro de WSL quedó en versión 1 en vez de 2, que es lo
que Docker Desktop necesita.

```
HyperVRequirementVirtualizationFirmwareEnabled : False
HyperVRequirementVMMonitorModeExtensions       : True
HyperVRequirementSecondLevelAddressTranslation : True

  NAME      STATE      VERSION
* Ubuntu    Stopped    1
```

### Archivos entregados

| Archivo | Qué resuelve |
|---|---|
| `backend/Dockerfile` | Multi-stage: `sdk:9.0` compila, `aspnet:9.0` ejecuta. Corre como `USER app` |
| `backend/.dockerignore` | Excluye `**/bin/`, `**/obj/` y los `appsettings.Development.json` con credenciales |
| `frontend/Dockerfile` | Multi-stage: prepara el sitio y genera `js/config.js`; `nginx:alpine` lo sirve |
| `frontend/.dockerignore` | Excluye el `js/config.js` de la máquina local |
| `frontend/nginx.conf` | Sirve los estáticos y proxea `/api/` a `backend:8080` |
| `docker-compose.yml` | Los tres servicios, red interna, volumen nombrado y healthcheck |
| `docker-compose.registry.yml` | El mismo sistema con `image:` en vez de `build:` |
| `.env.example` | Plantilla versionada; el `.env` real está en `.gitignore` |

### Validación del compose (ya hecha)

```
$ docker compose config --quiet
$ echo $?
0
```

### Pendiente de ejecutar

Los comandos, en orden. La salida de cada uno se pega abajo.

```bash
cp .env.example .env          # y editarlo con valores reales
docker compose up -d --build
docker compose ps             # db "healthy", backend y frontend "running"
```

**1. Tamaños de imagen — la prueba de que el multi-stage sirve**

```bash
docker images mcr.microsoft.com/dotnet/sdk:9.0       # la que COMPILA
docker images mcr.microsoft.com/dotnet/aspnet:9.0    # la que sólo EJECUTA
docker images historialcancha-backend
docker images historialcancha-frontend
```

> _(pegar la salida)_

**2. Sistema funcionando end-to-end**

```bash
curl -s localhost:8080/api/health          # backend directo
curl -s localhost:3000/api/health          # a través del proxy de nginx: misma respuesta
```

El segundo es el que prueba que el proxy funciona: la misma respuesta por el puerto del
frontend significa que nginx resolvió `backend:8080` por la red interna.

> _(pegar la salida)_

**3. Persistencia — la prueba del volumen**

```bash
# crear una cuenta y un partido desde http://localhost:3000, y después:
docker compose down && docker compose up -d
# esperar a que /api/health conteste y verificar que el partido SIGUE

docker compose down -v && docker compose up -d
# ahora la base nace vacía: -v borró también el volumen
```

> _(pegar la salida de las tres corridas)_

**4. Publicación en el registry**

```bash
echo $CR_PAT | docker login ghcr.io -u nicolasbono --password-stdin
docker tag historialcancha-backend  ghcr.io/nicolasbono/historialcancha-backend:v0.1.0
docker tag historialcancha-frontend ghcr.io/nicolasbono/historialcancha-frontend:v0.1.0
docker push ghcr.io/nicolasbono/historialcancha-backend:v0.1.0
docker push ghcr.io/nicolasbono/historialcancha-frontend:v0.1.0
```

Después hay que **hacer públicos los dos packages** desde GitHub (Perfil → Packages →
package → Package settings → Change visibility → Public): nacen privados y mientras lo
estén, nadie puede hacer `pull`.

> _(pegar los digest y las URLs de los packages)_

**5. El sistema desde el registry, sin código fuente**

```bash
docker compose down -v
docker compose -f docker-compose.registry.yml up -d
curl -s localhost:3000/api/health
```

> _(pegar la salida)_
