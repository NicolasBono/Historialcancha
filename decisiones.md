# Decisiones

## TP1- Git colaborativo
1.¿porque git no pudo resolver el conflicto solo y que habria tenido que pasar para que nunca apareciera?
porque git no entiende el codigo ni el texto si no que lo que hace es comparar desde donde se separaron la rama,una version en una rama y la otra version en la otra rama,ve en cada rama que lineas se tocaron y si las 2 ramas salieron de la misma version y tocan la misma linea va a haber un problema. primero cuando quiero subir la rama b git me frena el push porque estoy atrasado,ya que la rama a se subio antes,entonces hago un pull y ahi git si trae los cambios y corre un merge por detras,pero en ese merge ve que las dos ramas cambiaron la misma linea y no puede decidir cual de las dos vale,asi que frena a la mitad y me pregunta a mi. ese es el verdadero motivo por el que no lo resuelve solo,no es que le falle la verificacion de version ni que no pueda pullear,es que tiene dos cambios validos sobre la misma linea y no sabe cual quiero. para evitarlo tendria que o primero haberse subido la rama a y desde esa version hacer la rama b y subirla,asi te queda la respuesta de la rama b,o si no que no toquen las mismas lineas 

## TP2 — Contenedores y Compose

### Por qué esta app

**Mi historial de hincha**: backend .NET 9 (Web API), frontend estático HTML/CSS/JS y
PostgreSQL. Cumple los tres requisitos del TP (backend con API, frontend, base de datos)
y es de desarrollo propio, así que la entiendo entera y puedo modificarla en vivo.
Tamaño acotado: CRUD de partidos, login y pantalla de estadísticas.

### El frontend no tiene build step, y el Dockerfile igual es multi-stage

El frontend es HTML/CSS/JS puro. No es una limitación: es el NFR1 del proyecto —sin
frameworks, sin transpiladores, sin bundlers, sin `package.json`—. Así que acá no existe
el `npm ci && npm run build` del ejemplo de la guía.

Quedó multi-stage de todos modos, pero por un motivo real y no para cumplir la consigna:
**`frontend/js/config.js` no se versiona** (está en `.gitignore`, porque es la config de
cada máquina), así que no viaja en el contexto de build. La primera etapa lo genera desde
`config.example.js`, que sí está versionado. Sin ese paso la imagen quedaría sin
configuración y la app moriría en el primer script con *"Falta la configuración del
frontend"*.

La segunda etapa es `nginx:alpine` y sólo recibe los archivos del sitio: no entran ni el
`Dockerfile` ni el `nginx.conf` ni el `config.example.js`.

### Ruta relativa + proxy en nginx (opción (a) de la guía), no URL absoluta + CORS

`apiBaseUrl` pasó de `http://localhost:5080/api` a `/api`. El browser le pide al mismo
origen del que cargó la página y nginx reenvía `/api/` a `backend:8080` por la red interna
de compose.

Dos ventajas concretas sobre la opción (b):

1. **La misma imagen sirve en cualquier entorno.** La URL del backend no queda escrita en
   el frontend, así que cambiar de entorno no obliga a reconstruir la imagen — que es
   justo lo que va a necesitar el TP7.
2. **Desaparece el CORS.** Para el browser todo es same-origin. El backend igual conserva
   su política CORS configurable, porque en desarrollo el front se sirve aparte con
   `python -m http.server 5500` y ahí sí son dos orígenes.

El nombre del backend va en una **variable** de nginx (`set $backend_api`) y no escrito
dentro del `proxy_pass`. Con el nombre directo, nginx lo resuelve al arrancar y se niega a
levantar si el backend todavía no existe (`host not found in upstream`); con variable
resuelve recién cuando llega un pedido, así el contenedor del frontend puede correr solo.

Y el `proxy_pass` va **sin barra final**: con barra, nginx reescribe el prefijo y
`/api/health` llegaría al backend como `/health` — 404 en todas las llamadas.

### `try_files ... =404`, no fallback a `index.html`

El ejemplo de la guía asume una SPA con router del lado del cliente, donde cualquier ruta
tiene que devolver el `index.html`. Este frontend es **multipágina** (`index.html`,
`estadisticas.html`, `login.html`): una ruta que no existe tiene que dar 404 de verdad y
no devolver el index disfrazado, que sería más difícil de diagnosticar.

### La base no publica puerto

El servicio `db` no tiene `ports:`. Sólo lo alcanza el backend por la red interna, que es
todo lo que hace falta. Dos motivos: menos superficie expuesta, y en esta máquina el 5432
ya lo ocupa un PostgreSQL instalado nativamente — publicarlo daría `port is already
allocated`.

### Volumen nombrado para los datos

`db_data:/var/lib/postgresql/data`, no un bind mount. Los datos de PostgreSQL los
administra Docker: en Windows un bind mount del directorio de datos pasa por la VM de
Docker Desktop, es notablemente más lento y suele dar problemas de permisos.

Es lo único del sistema que no es descartable: los contenedores se recrean sin costo, el
estado no.

### El esquema lo aplica la app, no un script

El backend corre `Database.Migrate()` al arrancar (`MigrarBaseDeDatosAsync`). El PostgreSQL
del compose nace con la base vacía, así que sin esto no habría tablas. A diferencia del
warm-up, este paso **sí tira** si falla: una app sin esquema no sirve, y arrancar
"operativa" sin tablas sería mentir.

Por eso tampoco hace falta `dotnet ef` para levantar el sistema — sólo para *crear* una
migración nueva.

### `USER app` en la imagen del backend

La etapa final corre como el usuario sin privilegios que ya trae la imagen de .NET, no
como root. Si alguien lograra ejecutar algo dentro del contenedor, no lo haría como
administrador. Cuesta una línea.

### Los secretos

`DB_PASSWORD` y `JWT_KEY` viven en un `.env` que **no se versiona**, con un `.env.example`
que sí. El compose usa la forma `${VAR:?mensaje}`: si la variable falta, falla en el acto
con una explicación, en vez de arrancar con la variable vacía y romper más adelante con un
error que no menciona el problema real.

`appsettings.Development.json` está excluido en el `.dockerignore` del backend: tiene
credenciales y no puede viajar dentro de la imagen. La configuración entra por variables de
entorno.

### Orden de las instrucciones en el Dockerfile del backend

Los `.csproj` y el `dotnet restore` van **antes** de copiar el código. El restore es el
paso caro y sólo depende de los archivos de proyecto: separarlo hace que cambiar una línea
de código reuse esa capa del cache en lugar de volver a bajar NuGet.

El `.dockerignore` usa `**/bin/` y `**/obj/` con doble asterisco, no `bin/`: la solución
tiene un `bin` y un `obj` dentro de **cada** proyecto, y sin el `**` el `COPY . .` metería
el `obj/project.assets.json` generado en mi máquina pisando el del contenedor.

---

## TP1 — Repositorio y protecciones

*(Ver historial del repo: protecciones de rama y flujo por PR.)*
