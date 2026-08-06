# PRD — Mi historial de hincha (Fase 1)

Alcance: aplicación corriendo en local (frontend + backend + MSSQL).
Todo requerimiento de testing, CI/CD, infraestructura o entornos no locales
queda fuera de este documento por definición del alcance.

## Requerimientos funcionales

### Registro de partidos

- **FR1** — El usuario puede registrar un partido con: fecha, rival, torneo, condición
  (Local / Visitante), estadio, goles a favor y goles en contra. Todos obligatorios salvo estadio.
- **FR2** — Al registrar un partido, el usuario informa la modalidad en que lo vivió:
  `EnCancha`, `TV`, `Streaming`, `Radio` o `NoLoVi`. Es obligatoria.
- **FR3** — El usuario puede completar opcionalmente sector de la tribuna, con quién fue,
  y una nota del 1 al 10 sobre cómo vivió el partido.
- **FR4** — El usuario puede listar todos sus partidos ordenados por fecha descendente,
  viendo resultado, marcador y modalidad de cada uno.
- **FR5** — El usuario puede editar cualquier dato de un partido ya cargado, incluidos los
  de su registro personal.
- **FR6** — El usuario puede eliminar un partido; se elimina también su registro personal asociado.

### Validaciones de carga

- **FR7** — El sistema rechaza un partido con goles a favor o goles en contra negativos.
- **FR8** — El sistema rechaza un partido con fecha posterior al día actual.
- **FR9** — El sistema rechaza un partido cuyo rival coincida con el equipo propio configurado
  (comparación sin distinguir mayúsculas ni espacios sobrantes).
- **FR10** — El sistema rechaza el alta de un segundo partido en una fecha que ya tiene uno cargado.
- **FR11** — El sistema rechaza una nota fuera del rango 1 a 10.
- **FR12** — Ante cualquier validación fallida, el backend responde HTTP 400 con un mensaje
  en español que identifica la regla incumplida, y el frontend lo muestra sin recargar la página.

### Estadísticas

- **FR13** — El sistema deriva el resultado de cada partido (Victoria / Empate / Derrota)
  a partir de los goles, sin que el usuario lo cargue.
- **FR14** — El sistema calcula el récord global: PJ, G, E, P, GF, GC, diferencia de gol
  y efectividad porcentual (3 puntos por victoria, 1 por empate, 0 por derrota, sobre puntos en juego).
- **FR15** — El sistema calcula el mismo récord desglosado por cada una de las cinco modalidades.
- **FR16** — El sistema emite un veredicto "Cábala / Yeta / Indefinido" comparando la efectividad
  en modalidad `EnCancha` contra la efectividad agregada de `TV + Streaming + Radio`.
- **FR17** — El sistema calcula la racha actual de cada tipo: invicto, sin ganar y sin recibir goles.
- **FR18** — El sistema calcula la racha histórica más larga de cada uno de esos tres tipos,
  informando fecha de inicio y de fin.
- **FR19** — El sistema identifica el rival talismán (mejor efectividad) y el rival maldición
  (peor efectividad) entre los rivales que superan el umbral mínimo de partidos configurado.
- **FR20** — El sistema calcula el promedio de goles a favor y en contra por partido.
- **FR21** — El sistema desglosa el récord por condición: Local contra Visitante.
- **FR22** — El sistema desglosa el récord por torneo.
- **FR23** — El sistema desglosa el récord por temporada, agrupando según el mes de corte configurado.

### Operación local

- **FR24** — El backend expone un endpoint de health check que informa su estado, la versión,
  el entorno y si la conexión a la base de datos responde.
- **FR25** — El frontend muestra de forma visible y permanente la versión de la aplicación
  y el entorno en que está corriendo.
- **FR26** — El frontend indica visualmente si el backend está disponible, consultando el health check.

## Requerimientos no funcionales

- **NFR1** — El frontend se construye sólo con HTML, CSS y JavaScript nativo: sin frameworks,
  sin transpiladores, sin bundlers y sin `package.json`.
- **NFR2** — El frontend se sirve como sitio estático desde un origen distinto al del backend;
  el backend no sirve archivos del frontend ni tiene contenido en `wwwroot`.
- **NFR3** — El backend habilita CORS mediante una política nombrada, con la lista de orígenes
  permitidos tomada de configuración; no se usa el comodín `*`.
- **NFR4** — La persistencia es MSSQL real: no se admiten mocks, archivos JSON, ni estado en memoria.
  Los datos sobreviven al reinicio de los servicios.
- **NFR5** — El esquema de la base se crea y evoluciona mediante migraciones versionadas
  en el repositorio; no se aplica ningún cambio manual sobre la base.
- **NFR6** — Toda la configuración (connection string, URL del backend, equipo propio, versión,
  entorno, umbrales de negocio) se resuelve por variables de entorno o archivo de configuración.
  Ningún valor está escrito en el código, ni siquiera para local.
- **NFR7** — Ningún secreto ni cadena de conexión con credenciales se versiona: los archivos de
  configuración local quedan ignorados por Git y se acompañan de un archivo de ejemplo.
- **NFR8** — La capa de dominio no referencia Entity Framework, el proveedor de base de datos,
  ASP.NET Core ni ningún tipo de los controllers. Se compila de forma aislada.
- **NFR9** — El acceso a datos se consume desde interfaces declaradas en el dominio;
  las implementaciones viven en la capa de infraestructura.
- **NFR10** — La API es REST sobre JSON, con nombres de campo en `camelCase` y fechas en formato
  `YYYY-MM-DD`.
- **NFR11** — Los tres servicios se levantan en local siguiendo el README, sin conocimiento previo
  del proyecto y sin pasos manuales fuera de los documentados.
- **NFR12** — El health check responde en menos de 1 segundo y no requiere autenticación.
- **NFR13** — Las consultas de estadísticas responden en menos de 500 ms con un historial
  de hasta 500 partidos.
- **NFR14** — El backend registra en consola cada request recibida y cada error no controlado,
  con nivel de log configurable.
- **NFR15** — La solución de backend se mantiene en tres proyectos (dominio, infraestructura, API);
  no se agregan capas ni proyectos adicionales en esta fase.
- **NFR16** — La interfaz está íntegramente en español y usa codificación UTF-8.
