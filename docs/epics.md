# Épicas e historias — Mi historial de hincha (Fase 1)

Cuatro épicas. La primera es el esqueleto de los tres servicios: hasta que no esté
completa y andando de punta a punta, no se arranca con features.

---

## Épica 1 — Esqueleto de los tres servicios

**Objetivo:** frontend, backend y base de datos levantados en local, hablando entre sí,
con health check, conexión real a PostgreSQL y la pantalla mostrando versión y entorno.

### HU 1.1 — Backend con health check

*Como desarrollador, quiero un Web API .NET Core con un endpoint de health, para confirmar
que el servicio arranca y responde.*

- **Dado** el repositorio recién clonado, **cuando** ejecuto `dotnet run` en
  `backend/src/HistorialCancha.Api`, **entonces** el servicio queda escuchando en
  `http://localhost:5080` sin errores de arranque.
- **Dado** el backend corriendo, **cuando** hago `GET /api/health`, **entonces** recibo 200
  con `status`, `version` y `entorno`, en menos de 1 segundo.
- **Dado** el proyecto de API, **cuando** reviso su estructura, **entonces** existen los tres
  proyectos (`Domain`, `Infrastructure`, `Api`) y `Domain` no tiene ningún `PackageReference`.
- **Dado** el proyecto de API, **cuando** reviso su contenido, **entonces** no existe carpeta
  `wwwroot` ni se sirve ningún archivo estático.

### HU 1.2 — Base de datos PostgreSQL conectada

*Como desarrollador, quiero el esquema creado en PostgreSQL y el backend conectado, para
tener persistencia real desde el primer día.*

- **Dado** el contenedor de PostgreSQL levantado (base y usuario creados por la imagen a
  partir de `POSTGRES_DB` / `POSTGRES_USER` / `POSTGRES_PASSWORD`), **cuando** arranco el
  backend, **entonces** la app aplica las migraciones al inicio (`Database.Migrate()`) y crea
  las tablas `Partidos` y `Vivencias`, sus restricciones y el índice único por fecha.
- **Dado** el backend corriendo con la connection string configurada, **cuando** hago
  `GET /api/health`, **entonces** el campo `baseDeDatos` devuelve `ok`.
- **Dado** el contenedor de PostgreSQL detenido **después** de que el backend arrancó,
  **cuando** hago `GET /api/health`, **entonces** el endpoint responde igual (no se cae) e
  informa `baseDeDatos: "error"`.
- **Dado** que la base no está disponible **al arrancar**, **cuando** levanto el backend,
  **entonces** falla el arranque a propósito: sin esquema no hay app que valga.
- **Dado** el repositorio, **cuando** busco la connection string, **entonces** no aparece en
  ningún archivo versionado: sólo en `appsettings.Development.example.json` con valores de ejemplo.

### HU 1.3 — Frontend independiente con versión y entorno

*Como hincha, quiero abrir la app y ver de inmediato qué versión estoy usando y si el
backend está vivo.*

- **Dado** el frontend servido en `http://localhost:5500`, **cuando** abro la página,
  **entonces** el header muestra de forma visible y permanente la versión y el entorno,
  leídos de `js/config.js`.
- **Dado** el frontend abierto, **cuando** la página consulta `GET /api/health` del backend
  en otro origen, **entonces** el navegador no bloquea la respuesta por CORS y se muestra el
  indicador "backend disponible".
- **Dado** el backend apagado, **cuando** abro el frontend, **entonces** se muestra
  "backend no disponible" y la página sigue navegable, sin errores en consola sin manejar.
- **Dado** el frontend, **cuando** reviso sus archivos, **entonces** no hay `package.json`,
  ni bundler, ni framework, ni paso de build: se abre sirviendo la carpeta tal cual.
- **Dado** el código del frontend, **cuando** busco la URL del backend, **entonces** aparece
  únicamente en `js/config.js`, que no está versionado y tiene su `config.example.js`.

### HU 1.4 — README para levantar todo de cero

*Como desarrollador que vuelve al proyecto en dos meses, quiero instrucciones que funcionen
sin recordar nada.*

- **Dado** el README, **cuando** lo sigo desde cero, **entonces** puedo levantar el contenedor
  de PostgreSQL, arrancar el backend (que crea el esquema solo) y servir el frontend, en ese
  orden y sin pasos implícitos.
- **Dado** el README, **cuando** llego a la sección de configuración, **entonces** encuentro
  cada variable, qué hace y su valor de ejemplo para local.

---

## Épica 2 — Registro de partidos

**Objetivo:** cargar, ver, editar y borrar partidos con su vivencia, con las validaciones
aplicadas desde el dominio.

### HU 2.1 — Cargar un partido con su vivencia

*Como hincha, quiero registrar un partido y cómo lo viví, para empezar a construir mi historial.*

- **Dado** el formulario de alta, **cuando** completo fecha, rival, torneo, condición, goles
  a favor, goles en contra y modalidad, **entonces** el partido se guarda en PostgreSQL y aparece
  en el listado sin recargar la página.
- **Dado** el formulario, **cuando** además cargo sector, con quién fui y una nota del 1 al 10,
  **entonces** esos datos quedan persistidos en la vivencia.
- **Dado** el formulario, **cuando** dejo vacíos los campos opcionales, **entonces** el alta
  se completa igual.
- **Dado** un alta exitosa, **cuando** reinicio backend y navegador, **entonces** el partido
  sigue estando.

### HU 2.2 — Validaciones de carga

*Como hincha, quiero que la app no me deje cargar datos imposibles, para que mis estadísticas
no mientan.*

- **Dado** un partido con goles negativos, **cuando** intento guardarlo, **entonces** recibo
  400 con un mensaje que indica que los goles no pueden ser negativos, y no se guarda nada.
- **Dado** un partido con fecha posterior a hoy, **cuando** intento guardarlo, **entonces**
  se rechaza indicando que la fecha no puede ser futura.
- **Dado** un rival igual al equipo propio configurado (aun con otras mayúsculas o espacios
  sobrantes), **cuando** intento guardarlo, **entonces** se rechaza indicando que el rival no
  puede ser el propio equipo.
- **Dado** una fecha que ya tiene un partido cargado, **cuando** intento cargar otro,
  **entonces** se rechaza indicando que ya existe un partido ese día.
- **Dado** una nota fuera del rango 1–10, **cuando** intento guardarla, **entonces** se rechaza
  indicando el rango válido.
- **Dado** cualquiera de estos rechazos, **cuando** ocurre, **entonces** el frontend muestra el
  mensaje junto al formulario y conserva lo que ya había cargado.

### HU 2.3 — Listar, editar y eliminar

*Como hincha, quiero corregir o borrar lo que cargué mal.*

- **Dado** partidos cargados, **cuando** abro el listado, **entonces** los veo ordenados por
  fecha descendente, con rival, marcador, resultado derivado (V/E/D) y modalidad.
- **Dado** un partido del listado, **cuando** lo edito y guardo, **entonces** los cambios
  quedan persistidos y se revalidan todas las reglas de HU 2.2.
- **Dado** que edito un partido sin cambiarle la fecha, **cuando** guardo, **entonces** la regla
  de "un partido por día" no lo rechaza contra sí mismo.
- **Dado** un partido del listado, **cuando** lo elimino y confirmo, **entonces** desaparece
  del listado y su vivencia se borra en cascada.

---

## Épica 3 — Estadísticas del historial

**Objetivo:** el récord del equipo según el historial cargado, con toda la lógica en el dominio.

### HU 3.1 — Récord global

*Como hincha, quiero ver mi récord completo, para saber cómo le fue al equipo en mi historial.*

- **Dado** un historial con partidos, **cuando** abro estadísticas, **entonces** veo PJ, G, E, P,
  GF, GC, diferencia de gol y efectividad porcentual.
- **Dado** un historial de 1 victoria y 1 derrota, **cuando** se calcula la efectividad,
  **entonces** da 50% (3 de 6 puntos en juego).
- **Dado** un historial vacío, **cuando** abro estadísticas, **entonces** todo aparece en cero,
  la efectividad se muestra como `—` y no hay error ni división por cero.
- **Dado** cualquier partido, **cuando** se determina su resultado, **entonces** sale de comparar
  los goles y nunca es un dato que el usuario haya cargado.
- **Dado** un historial con partidos, **cuando** veo el récord, **entonces** también veo el
  promedio de goles a favor y en contra por partido.

### HU 3.2 — Récord por modalidad y veredicto cábala/yeta

*Como hincha, quiero comparar cómo le va al equipo cuando voy a la cancha contra cuando lo
miro por otro medio.* **(corazón de la app)**

- **Dado** un historial con partidos en varias modalidades, **cuando** abro estadísticas,
  **entonces** veo el récord completo desglosado por cada una de las cinco modalidades.
- **Dado** al menos 5 partidos en cancha y 5 por otro medio, **cuando** la efectividad en cancha
  supera a la otra por 10 puntos o más, **entonces** el veredicto es **Cábala**.
- **Dado** las mismas condiciones, **cuando** la efectividad en cancha es 10 puntos o más
  inferior, **entonces** el veredicto es **Yeta**.
- **Dado** que falta volumen en alguno de los dos grupos o la diferencia es menor a 10 puntos,
  **cuando** se calcula, **entonces** el veredicto es **Indefinido** y se explica por qué.
- **Dado** partidos con modalidad "no lo vi", **cuando** se calcula el veredicto, **entonces**
  quedan excluidos de la comparación pero siguen contando en el récord global.

### HU 3.3 — Rachas

*Como hincha, quiero ver mis rachas, porque es lo primero que se discute.*

- **Dado** un historial ordenado por fecha, **cuando** abro rachas, **entonces** veo la racha
  actual de invicto, sin ganar y sin recibir goles.
- **Dado** el mismo historial, **cuando** abro rachas, **entonces** veo la más larga histórica de
  cada tipo, con su fecha de inicio y de fin.
- **Dado** un historial vacío, **cuando** abro rachas, **entonces** todas valen 0 y no hay error.
- **Dado** un historial con un solo partido, **cuando** abro rachas, **entonces** las rachas
  valen 1 o 0 según el resultado, de forma consistente.
- **Dado** que la racha actual sigue abierta, **cuando** se muestra, **entonces** se indica como
  en curso y su fecha de fin es la del último partido.

### HU 3.4 — Rival talismán y rival maldición

*Como hincha, quiero saber contra quién me va bien y contra quién me va mal.*

- **Dado** rivales con al menos el mínimo de partidos configurado, **cuando** abro rivales,
  **entonces** veo el talismán (mejor efectividad) y la maldición (peor efectividad).
- **Dado** un rival con menos partidos que el umbral, **cuando** se arma el ranking,
  **entonces** queda excluido de ambos puestos.
- **Dado** dos rivales empatados en efectividad, **cuando** se resuelve el orden, **entonces**
  desempata la mayor diferencia de gol; si persiste, más partidos jugados; si persiste, orden alfabético.
- **Dado** que ningún rival alcanza el umbral, **cuando** abro rivales, **entonces** se informa
  que todavía no hay suficientes partidos, sin error.

---

## Épica 4 — Desgloses del historial

**Objetivo:** cortar el récord por condición, torneo y temporada.

### HU 4.1 — Local y visitante, y por torneo

*Como hincha, quiero ver dónde y en qué competencia me fue mejor.*

- **Dado** un historial con partidos de local y de visitante, **cuando** abro desgloses,
  **entonces** veo el récord completo de cada condición por separado.
- **Dado** un historial con varios torneos, **cuando** abro desgloses, **entonces** veo una fila
  por torneo con su récord y efectividad.
- **Dado** un torneo sin partidos, **cuando** se arma el desglose, **entonces** simplemente no aparece.

### HU 4.2 — Por temporada

*Como hincha, quiero ver mi historial agrupado por temporada, no por año calendario.*

- **Dado** el mes de corte configurado en julio, **cuando** se agrupa un partido del 15 de agosto
  de 2024, **entonces** cae en la temporada `2024/25`.
- **Dado** el mismo corte, **cuando** se agrupa un partido del 10 de marzo de 2025, **entonces**
  también cae en `2024/25`.
- **Dado** que cambio el mes de corte por configuración, **cuando** recargo las estadísticas,
  **entonces** la agrupación cambia sin tocar el código.
- **Dado** el desglose por temporada, **cuando** lo veo, **entonces** las temporadas aparecen
  de la más reciente a la más antigua, cada una con su récord y efectividad.

---

## Épica 5 — Cuentas de hincha y datos por usuario

**Objetivo:** que cada hincha tenga su cuenta y vea sólo su propio historial. El equipo que
sigue la app (`App:MiEquipo`) es el mismo para todos; los partidos son de cada uno.

### HU 5.1 — Registro de un hincha

*Como hincha, quiero crear mi cuenta, para tener mi propio historial separado del de otros.*

- **Dado** el formulario de registro, **cuando** ingreso nombre, apellido, DNI y una
  contraseña de al menos 8 caracteres, **entonces** se crea mi cuenta y entro directo a mi
  historial (auto-login con token).
- **Dado** un DNI que no es un número de 7 u 8 dígitos, **cuando** intento registrarme,
  **entonces** se rechaza indicando el formato válido.
- **Dado** un DNI que ya tiene cuenta, **cuando** intento registrarme, **entonces** se
  rechaza indicando que ya existe un usuario con ese DNI.
- **Dado** un registro exitoso, **cuando** miro la base, **entonces** la contraseña está
  hasheada, no en texto plano.

### HU 5.2 — Login y sesión

*Como hincha, quiero ingresar con mi DNI y contraseña, para acceder a mis datos.*

- **Dado** un DNI y contraseña correctos, **cuando** ingreso, **entonces** obtengo un token
  y entro a mi historial.
- **Dado** un DNI inexistente o una contraseña incorrecta, **cuando** intento ingresar,
  **entonces** recibo 401 con un mensaje genérico que no revela cuál de los dos falló.
- **Dado** que estoy logueado, **cuando** cierro sesión, **entonces** vuelvo al login y mi
  token deja de usarse.
- **Dado** el historial o las estadísticas, **cuando** entro sin sesión o con el token
  vencido, **entonces** el frontend me manda al login.

### HU 5.3 — Aislamiento de datos entre usuarios

*Como hincha, quiero que nadie más vea ni toque mis partidos.*

- **Dado** dos usuarios con partidos, **cuando** cada uno abre su historial o sus
  estadísticas, **entonces** ve únicamente los suyos.
- **Dado** el id de un partido de otro usuario, **cuando** lo pido, edito o borro con mi
  token, **entonces** recibo 404, como si no existiera.
- **Dado** dos usuarios distintos, **cuando** cada uno carga un partido en la misma fecha,
  **entonces** ambos se guardan: la regla "un partido por día" es por usuario.
- **Dado** cualquier endpoint de partidos o estadísticas, **cuando** lo llamo sin token,
  **entonces** recibo 401.
