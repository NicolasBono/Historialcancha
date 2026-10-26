# Decisiones

## Enlaces del TP7

paquetes publicos del registry (se bajan sin credenciales). la version que esta en prod (v7.0.0):
- backend: https://github.com/users/NicolasBono/packages/container/package/historialcancha-backend
  `docker pull ghcr.io/nicolasbono/historialcancha-backend:sha-575a5a166c4444d5e4fe55c02d5b6a8a62c3579a`
- frontend: https://github.com/users/NicolasBono/packages/container/package/historialcancha-frontend
  `docker pull ghcr.io/nicolasbono/historialcancha-frontend:sha-575a5a166c4444d5e4fe55c02d5b6a8a62c3579a`

la corrida donde la integracion quedo VERDE y la e2e ROJA (el rojo provocado):
- el commit que rompio la app (el front manda nombreRival en vez de rival):
  https://github.com/NicolasBono/Historialcancha/commit/d8960ab (PR #39)
- la corrida: https://github.com/NicolasBono/Historialcancha/actions/runs/39509620502
- reporte de integracion, en verde: https://github.com/NicolasBono/Historialcancha/actions/runs/39509620502/artifacts/12305003376
- reporte e2e, en rojo: https://github.com/NicolasBono/Historialcancha/actions/runs/39509620502/artifacts/12305033696
  (los reportes vencen a los 7 dias en github; tengo una copia local para mostrar en la defensa)

la corrida completa en verde posterior, hasta prod:
- https://github.com/NicolasBono/Historialcancha/actions/runs/39512864301
- release: https://github.com/NicolasBono/Historialcancha/releases/tag/v7.0.0

los entornos (vivos hasta la defensa; el primer pedido tarda ~1 min si estaban dormidos):
- QA: https://historialcancha-front-qa.onrender.com · api: https://historialcancha-api-qa.onrender.com/api/health
- PROD: https://historialcancha-front-prod.onrender.com · api: https://historialcancha-api-prod.onrender.com/api/health

## Enlaces de este TP (TP6)

los paquetes y las urls viven afuera del repo, por eso van aca.

paquetes publicos del registry (se bajan sin credenciales):
- backend: https://github.com/users/NicolasBono/packages/container/package/historialcancha-backend
- frontend: https://github.com/users/NicolasBono/packages/container/package/historialcancha-frontend
- los dos etiquetados con el commit del merge que esta en prod (v6.0.0):
  `docker pull ghcr.io/nicolasbono/historialcancha-backend:sha-234eac32425af16b8457fa2ae3c39e89dfed5dfc`
  `docker pull ghcr.io/nicolasbono/historialcancha-frontend:sha-234eac32425af16b8457fa2ae3c39e89dfed5dfc`

la cadena de la tarea 1:
- job de un PR con los tests en verde donde "Entrar al registry" sale salteado (solo main publica):
  https://github.com/NicolasBono/Historialcancha/actions/runs/37652974547/job/112900806568
- lista de pasos de una corrida de main, donde "Construir y publicar la imagen" es el ultimo paso,
  despues de los tests: https://github.com/NicolasBono/Historialcancha/actions/runs/39068689730/job/117501586191

los entornos (vivos hasta la defensa; el primer pedido tarda ~1 min si estaban dormidos):
- QA: https://historialcancha-front-qa.onrender.com · api: https://historialcancha-api-qa.onrender.com/api/health
- PROD: https://historialcancha-front-prod.onrender.com · api: https://historialcancha-api-prod.onrender.com/api/health

corridas que conviene mirar:
- rechazo con motivo: https://github.com/NicolasBono/Historialcancha/actions/runs/39068216767
- flujo aprobado (QA → waiting for review → aprobado → PROD), y despues re-corrido para el rollback:
  https://github.com/NicolasBono/Historialcancha/actions/runs/39068689730
- release: https://github.com/NicolasBono/Historialcancha/releases/tag/v6.0.0

## TP1- Git colaborativo
1.¿porque git no pudo resolver el conflicto solo y que habria tenido que pasar para que nunca apareciera?
porque git no entiende el codigo ni el texto si no que lo que hace es comparar desde donde se separaron la rama,una version en una rama y la otra version en la otra rama,ve en cada rama que lineas se tocaron y si las 2 ramas salieron de la misma version y tocan la misma linea va a haber un conflicto. primero cuando quiero subir la rama b git me frena el push porque estoy atrasado,ya que la rama a se subio antes,entonces hago un pull y ahi git si trae los cambios y corre un merge por detras,pero en ese merge ve que las dos ramas cambiaron la misma linea y no puede decidir cual de las dos vale,asi que frena a la mitad y me pregunta a mi. ese es el verdadero motivo por el que no lo resuelve solo,no es que le falle la verificacion de version ni que no pueda pullear,es que tiene dos cambios validos sobre la misma linea y no sabe cual quiero. para evitarlo tendria que o primero haberse subido la rama a y desde esa version hacer la rama b y subirla,asi te queda la respuesta de la rama b,o si no que no toquen las mismas lineas 
2.Problemas que tuve fue que este trabajo lo hice en un repositorio aparte en las primeras clases pero me di cuenta que lo tenia que hacer en este asi que por eso subi primero el tp2 que el tp1 como lo resolvi lo volvi a hacer rapido total es corto 
3.uso de inteligencia artificial en este tp mas que alguna duda que le pregunte de como cargar imagenes y me ayudo a subir los archivos al repo una vez lo termine 
## TP2 — Contenedores y Compose
1) Porque esta app criterios
¿buildea y corre localmente? si los Requisitos serian 
.NET SDK 9 (dotnet --version)
Python (solo para servir el frontend)
Una base PostgreSQL
el paso a paso seria correr el backend en una terminal apartes que donde vas a correr el frontend en la primera el backend que tenes que hacer un 
donet run(asumiendo que tenes configurado el appsettings). el backend al arrancar se conecta a la base y crea las tablas solo con las migraciones, queda escuchando en localhost:5080 y lo pruebo con un curl a /api/health.
y luego en otra terminal el frontend con un python -m http.server 5500, que sirve los archivos html/css/js. aclaro que python es solo el servidorcito para abrir el front, la app es toda en C#. despues lo abro en el navegador en localhost:5500 y me registro para entrar.
y si no quiero prender cada cosa a mano, lo levanto todo junto con docker compose up -d --build, que arranca las 3 cosas de una.
que es lo que se implemento en el tp2 

aca alguno se puede preguntar porque uso python en local si en el docker del front no puse python. es porque adentro del contenedor el que sirve los archivos es nginx, asi que ahi no me hace falta python. cuando lo corro sin docker no hay ningun nginx prendido y el navegador no me deja abrir los html sueltos con doble clic, entonces necesito algun servidor que me los sirva por http y uso python que es una sola linea. tambien se podria usar nginx en local, pero es mucho mas lio de instalar y configurar, y ademas el nginx.conf que tengo apunta a backend:8080 que solo existe dentro de docker, asi que en local no me serviria igual. la otra diferencia es que nginx ademas proxea el /api y va todo por el mismo origen sin cors, en cambio con python el front le pega directo al backend en localhost:5080 y por eso el backend tiene cors habilitado.

¿tiene tests? No no tiene test porque la desarrolle pensando en el tp y la idea era agregarselo cuando hagamos la guia de test pero si se pueden hacer test ya que hay una api para consumir por ejemplo 

¿entiendo el codigo para poder modificarlo? si bien el codigo fue desarrollado por ia con una metodologia bmad entiendo la estructura de codigo y si bien c# no es mi fuerte me defiendo para cambios pequeños mientras que no implique mucha logica 

¿el tamaño? es el justo, un crud de partidos con login y estadisticas, 2 o 3 pantallas, no mas grande de lo necesario.

2) Por qué dos etapas (multi-stage)
en los dos dockerfiles separe el build del runtime, o sea la parte que fabrica la imagen de la parte que despues la corre. la primera etapa tiene las herramientas pesadas para construir: en el backend es el sdk de .net que trae el compilador, y en el front es alpine que me genera el config.js desde el config.example. la segunda etapa se queda solo con lo minimo para ejecutar, que en el backend es el runtime aspnet y en el front es nginx. asi la imagen final no se lleva el compilador ni el codigo fuente, entonces queda mucho mas chica y con menos superficie de ataque porque adentro no hay nada de mas.

3) Cómo se encuentran los servicios
los servicios se hablan entre ellos por el nombre y no por ip, porque compose arma una red interna con su propio dns. entonces el backend le pega a la base poniendo Host=db, que es el nombre del servicio de la base en el compose, y nginx le pega al backend con backend:8080. asi no importa en que ip le toca a cada contenedor cuando levanto todo, siempre se encuentran por el nombre. eso me evita tener que hardcodear ips que ademas cambian cada vez que reinicio.

4) Healthcheck vs depends_on
el depends_on por si solo solo espera a que el contenedor de la base arranque, pero que un contenedor arranque no quiere decir que el servicio de adentro ya este listo para recibir conexiones. por eso ademas del depends_on se le pone condition service_healthy, que se apoya en un healthcheck que hace pg_isready contra la base. asi el backend no arranca hasta que postgres realmente esta aceptando conexiones, y me evito que el backend se caiga al intentar conectarse o aplicar las migraciones cuando la base todavia no estaba lista.

5) Dónde viven los secretos
los secretos como la contraseña de la base y el jwt key no estan en el codigo ni adentro de la imagen, viven en un archivo .env que no se sube nunca porque lo tengo en el gitignore. lo que si subo es un .env.example que tiene los nombres de las variables pero sin los valores reales, asi otro que clona el repo sabe que tiene que completar. despues el compose agarra esas variables del .env y se las pasa al contenedor por variables de entorno, entonces los secretos entran recien cuando levanto el sistema y nunca quedan escritos dentro de la imagen. ademas las variables obligatorias las puse con una forma que corta el arranque si faltan, asi si alguien no creo el .env el compose avisa en el momento en vez de romperse mas adelante.


Problemas 
1. Docker no levantaba por la BIOS (WSL2)
Docker Desktop no te arrancaba porque el SVM Mode (virtualización) estaba desactivado en la BIOS, y eso dejaba a WSL en versión 1.Esto paso porque lo quise instalar en la pc para hacer el trabajo desde ahi y tenia que instalar docker la solucion buscar la notebook y hacer el tp desde ahi que ya lo tenia instalado 

2. 
El puerto 5432 ocupado
En tu máquina ya corría un PostgreSQL nativo que ocupaba el 5432. Por eso decidiste que la base del compose no publique puerto (solo la ve el backend por la red interna), y así evitaste el error port is already allocated.Ya que justo antes use ese puerto para hacer el tp con la guia del profe 

Uso de ia en este tp ya si me ayudo la ia a crear el contenido del dockerfile y dockercompose y explicarme por que lo hacia, a solucionar errores de mi compu y a escribir el evidencias.



## TP3 - planificacion-devops

1) le puse 1 semana al sprint ya que como nos vemos todos los miercoles la idea es que hagamos un tp por sprint y hacerlo en una semana seria lo ideal para el trabajo 

2) le puse un limite de wip de 3 en la columna en progreso. la idea es que una tarea que esta en progreso no siempre esta activa al 100%: una la puedo estar codeando, otra puede estar esperando o bloqueada por algo y otra en prueba, entonces con 3 tengo margen para no quedarme frenado cuando una se traba, pero lo corto ahi para no llenar el tablero de cosas empezadas sin terminar. igual el limite es algo que se ajusta segun como fluye el trabajo, asi que si veo que se me empiezan a acumular tareas a medio hacer, bajarlo a 2 seria una buena opcion porque me daria mas foco pero manteniendo algo de margen para los bloqueos. al final la idea del wip es parar de empezar y empezar a terminar, y como trabajo solo lo importante es que el numero sea bajo.
3) esta historia esta mal escrita porque en realidad es una tarea tecnica y no una historia: dice "crear la tabla usuarios", que es el como se hace por dentro, en vez del valor que le da a un usuario real, ademas el rol es "desarrollador" y no tiene criterios de aceptacion para saber cuando esta hecha.basicamente no cumple con el criterio como quiero para. yo la reescribiria pensando en el usuario, algo como "como usuario quiero registrarme para tener mi propio historial de partidos" con sus criterios de aceptacion, y dejaria "crear la tabla usuarios" como una tarea de esa historia.

Problemas encontrados
el problema que tuve fue que quise trabajar los PRs desde la terminal con gh pero no lo tenia instalado, me tiraba que el comando no existia. lo resolvi de dos formas: primero segui abriendo los PR desde la web con el link que te da github al pushear, y despues instale gh con winget y me loguee, asi ya lo pude usar desde la terminal.

uso de ia: en este tp la ia me ayudo a redactar los problemas, a subir el tp con su tag y release. Y verificar que este todo loq ue pide el entregable como lo hizo(fui probando cada paso a medida que lo hacia (que los issues quedaran con su label, que la jerarquia se viera en el board, y que el PR cerrara la tarea) y revise que el resultado coincidiera con lo del video antes de darlo por bueno.)

## TP4 - Pipelines as code
1) estructura del pipeline: hice dos jobs, build-backend y build-frontend, uno para cada imagen. los puse en paralelo (que es como corren por defecto, cada uno en su propia maquina limpia) porque el back y el front no dependen uno del otro para construirse, asi que no tiene sentido que uno espere al otro y de paso tardan menos. cada job construye con el Dockerfile de su parte. si mi app tuviera un solo Dockerfile seria un solo job, pero como tengo dos imagenes separadas del tp2, van dos jobs.

2) que cachea: lo que se cachea son las capas de la imagen de docker. si una capa no cambio (por ejemplo la que instala las dependencias) se reutiliza en vez de rehacerla, y eso se ve en el log cuando aparece CACHED. cada job guarda sus capas en un scope distinto (scope=backend y scope=frontend) para que no se pisen entre ellos, porque si comparten el mismo scope el ultimo job que termina le borra el cache al otro. lo importante es que el cache es solo una optimizacion para ir mas rapido: puede desaparecer en cualquier momento porque github lo desaloja cuando quiere o por limite de tamaño, asi que el pipeline tiene que funcionar igual sin el, solo que mas lento. si fallara sin cache no seria un cache, seria una dependencia escondida, y eso es un bug.

3) por que construye con mi Dockerfile: el pipeline no compila por su cuenta con dotnet o npm, usa el mismo Dockerfile que ya tenia del tp2.Usando el Dockerfile hay una sola fuente de verdad, y ademas el mismo workflow le sirve a cualquier stack porque el workflow no sabe que hay adentro, eso lo sabe el Dockerfile.

Problemas encontrados
no tuve problemas mas que algun comando mal escrito de git para subir las cosas

uso de ia: en este tp la ia me ayudo a entender el archivo del pipeline (ci.yml) con los dos jobs y el cache viendo para que sirve cada parte (el scope del cache, el buildx, y por que conviene construir con el Dockerfile en vez de compilar aparte) y a resolver errores de git. a verificar lo que hicimos corriendo el workflow y mirando en la pestaña actions que los dos jobs pasaran en verde, que apareciera CACHED en la segunda corrida, y que el PR con el build roto a proposito quedara bloqueado hasta que lo arregle tambien me ayudo a hacer fallar la compilacion de mi imagen 

## TP5 - Testing en el pipeline

1) que logica elegi testear y por que esa

elegi la logica de HistorialCancha.Domain: los dos validadores y las cuatro calculadoras de
estadisticas. es donde duele un bug en mi app. si el validador deja pasar una fecha futura o dos
partidos el mismo dia, el historial queda mal y ninguna estadistica sirve, porque todas se calculan
sobre esos partidos. y si una calculadora miente, miente toda la pantalla de estadisticas de una.

quedaron 30 metodos (44 casos, porque los Theory corren uno por dato) sobre 9 reglas distintas en el
back, y 14 bloques (35 casos) en el front. el minimo era 8 sobre 4 reglas, y 4 en el front.

2) mi umbral de coverage

| | mide hoy | umbral |
|---|---|---|
| backend | 64,7% linea / 65,2% rama | 60 |
| frontend | 88% linea / 93,33% rama | 80 |

estos numeros salen de esta corrida, donde se ve el resumen de los dos lados y el reporte queda
descargable: https://github.com/NicolasBono/Historialcancha/actions/runs/37395326164

puse esos umbrales porque quiero que me frenen si bajan, no que sean inalcanzables. los dos aplican
sobre linea Y rama.

cuando tenia solo los tests obligatorios la cobertura me daba un 20%, y con eso no me
parecia que el codigo estuviera testeado: quedaban afuera casi todas las reglas. por eso segui
escribiendo tests hasta pasar el umbral, en vez de bajar el umbral para que entrara lo que tenia.

son distintos a proposito porque miden cosas distintas: el del front cubre solo la logica pura de
js/lib, y el del back incluye los controllers, que son adaptadores http. forzarlos a ser iguales
seria copiar un numero sin pensarlo.

en el backend el umbral va sobre el TOTAL y no por ensamblado. por defecto coverlet lo evalua por
ensamblado, y como mi capa Api esta en 28% cualquier numero arriba de eso fallaba siempre aunque el
total diera 64%.

para subir el back a 75 tendria que testear los controllers, que hoy no tienen tests: eso pide
tests de integracion con la api levantada, que es el tp7.

3) que deje afuera de la cuenta de cobertura

backend:
- toda la capa Infrastructure. las migraciones las genero entity framework, no yo: testearlas seria
  testear al generador. y el dbcontext, las configuraciones y los repositorios no tienen reglas de
  negocio, traducen entre el dominio y sql, y verificar eso pide una base de verdad.
- Program, que es el arranque. no hay reglas ahi y si esta mal la app no levanta.
- los dtos y los record de resultados, que solo tienen propiedades.
- NO excluí la entidad Partido aunque sea una entidad, porque tiene el Resultado y la
  DiferenciaDeGol derivados de los goles, y eso es logica de verdad. esta al 100%.

frontend: no excluí nada. js/lib/dom.js queda adentro en 0% aunque no tenga tests, porque escapar()
tiene comportamiento real (escapa html) y sacarlo seria esconder lo que no testee. esta en 0 porque
depende del dom y testearlo pide jsdom, que el tp declara opcional; se verifica de punta a punta en
el tp7.

lo hice con el atributo [ExcludeFromCodeCoverage] y no con filtros en el pipeline, porque el
atributo lo respetan tanto coverlet (el que aplica el umbral) como reportgenerator (el que arma el
reporte). asi el numero que frena y el que se muestra salen del mismo lugar y no se pueden
desincronizar.

4) por que un coverage alto no garantiza calidad (mi ejemplo)

un test que ejecuta el codigo sin comprobar nada suma cobertura y no verifica nada. para ver si los
mios comprueban de verdad rompi 16 reglas a proposito, de a una, y mire si algun test se ponia
rojo. las 16 las agarro algun test, y casi todas las agarro un test de BORDE, no uno del caso
feliz. ejemplo: el unico test que se pone rojo cuando cambias el > por >= en la fecha es
PartidoDeHoy_EsAceptado, que solo carga un partido con la fecha de hoy y espera que lo acepte. sin
ese test la mutacion pasaba desapercibida con todo lo demas en verde.

y dos de los mios NO agarraron su mutante la primera vez, aunque estaban en verde: los datos que
les habia puesto no distinguian. uno comparaba dos rivales donde el orden alfabetico daba el mismo
resultado que el desempate por gol, asi que el test pasaba igual con el desempate sacado. los
arregle cambiando los datos. eso es exactamente lo que la cobertura no te dice.

5) el pull request bloqueado

agregue una funcion al front, clasificarMarcador, que devuelve la etiqueta de cada marcador
(goleada, victoria, empate sin goles, empate, derrota, paliza) y la enganche en la tabla de
partidos. compilaba y los 24 tests pasaban, pero la funcion entro sin un solo test y la cobertura
cayo de 83,33% a 60% de linea y 53,33% de rama.

el check que se puso rojo fue build-frontend, por las DOS metricas, contra un umbral de 80. el log
lo dice con todas las letras: "ERROR: Coverage for lines (60%) does not meet global threshold
(80%)". esa es la corrida roja:
https://github.com/NicolasBono/Historialcancha/actions/runs/37394967938

el backend quedo en verde, y con uno solo de los dos en rojo el merge ya queda bloqueado.

lo arregle escribiendo 11 casos: uno por cada camino que la funcion declara (son seis), los cuatro
bordes de las dos reglas con umbral (3 de diferencia todavia no es goleada, 4 si, y lo mismo del
otro lado con la paliza), y uno que comprueba que el 0 a 0 no devuelve lo mismo que cualquier otro
empate. la cantidad no la invente: salio de leer los if.

con eso subio a 88% de linea y 93,33% de rama, el check paso a verde y lo mergee. la secuencia
entera —rojo, los tests que faltaban, verde y el merge— se ve en la conversacion de este pull
request: https://github.com/NicolasBono/Historialcancha/pull/28

y deje un SEGUNDO pull request abierto y en rojo a proposito, con otra funcion sin tests
(etiquetaDeEfectividad), para que se vea que el freno sigue vivo. ese no lo mergeo hasta despues de
la defensa: https://github.com/NicolasBono/Historialcancha/pull/29

lo que mas me gusto de esto es que no habia ningun error. nada roto, todo compilaba, todos los tests
pasaban, y el merge estaba bloqueado igual por un numero que elegi yo.

6) si refactorice para poder mockear

en el backend NO tuve que refactorizar. el dominio ya declaraba IPartidoRepository y el
PartidosController ya la recibia por constructor desde el tp2, asi que moq me fabrico el doble sin
tocar una coma de produccion. el test con mock verifica una colaboracion, no una cuenta: que al
crear un partido el controller le pregunte a la base si ya habia uno esa fecha, y que recien
despues mande a guardar. el que mas me gusta verifica una interaccion que NO ocurrio: con fecha
futura, AgregarAsync tiene que llamarse Times.Never. eso prueba que la validacion corre ANTES de
tocar la base; un controller que guardara primero dejaria basura y ningun test de valor devuelto
se daria cuenta.

en el frontend SI refactorice. tenia una busqueda de partido por id que llamaba sola a
API.listarPartidos() adentro, asi que para probarla necesitaba la api levantada. la saque a
js/lib/partidos-datos.js y le pase el lector por parametro: buscarPartidoPorId(id, listar). en
produccion le entra el cliente real, en el test un vi.fn().

ademas del refactor para el mock, tuve que hacer uno mas grande para que el front se pudiera testear en absoluto:

mi frontend era js puro con scripts clasicos, sin un solo export, y los archivos se hablaban por
variables globales. vitest solo importa modulos es, asi que el problema no era instalar vitest: era
que mi codigo no tenia por donde ser importado. tuve que migrar js/ a modulos es antes de escribir
un solo test, y de paso separe la logica pura (js/lib/formato.js) de lo que depende del dom
(js/lib/dom.js).

7) herramientas que use (mi front no es el stack de la catedra)

| lo que se pedia | backend | frontend |
|---|---|---|
| parametrizado | [Theory] + [InlineData] | it.each |
| el doble (mock) | Moq | vi.fn() |
| medir cobertura | coverlet (--collect) | @vitest/coverage-v8 |
| umbral que rompe | coverlet.msbuild (-p:Threshold) | coverage.thresholds de vitest |
| que entra en la cuenta | [ExcludeFromCodeCoverage] | include de vitest.config.js |
| reporte legible | ReportGenerator | reporter html + lcov |

el back es el stack de la catedra (.net + xunit). el front no: es js puro sin bundler, asi que
vitest lo sume solo como herramienta de test.

8) el ejercicio del camino sin cubrir

la encontre en ValidadorPartido.cs linea 26, el if (string.IsNullOrWhiteSpace(partido.Torneo)). en
el reporte sale en naranja: se ejecuto 38 veces pero siempre por el mismo camino, el if siempre dio
falso, y las dos lineas del throw estan en rojo.

me gusto porque muestra la diferencia entre las dos metricas: esa linea cuenta como linea CUBIERTA
(se ejecuto 38 veces) y como rama DESCUBIERTA (solo uno de los dos caminos). por eso la de rama es
la mas honesta.

la entrada que la recorreria es un partido con el torneo vacio: Torneo = "" o "   ". y es
alcanzable de verdad, porque el dto tiene Torneo nullable y el mapper lo convierte en string vacio.

decidi NO agregar el test por ahora, y lo dejo anotado como deuda. el motivo honesto es que ya tengo
el test hermano para el rival (RivalSinContenido_EsRechazado), asi que la regla ya esta probada en
su forma, y el umbral ya se cumple. lo dejo registrado tal cual lo encontre en vez de taparlo.

9) problemas encontrados y como los resolvi

- un bug que destapo el tp. al juntar la logica duplicada del front me encontre con que
  formatearFecha estaba escrita dos veces, en estadisticas.js y en partidos.js, y las dos versiones
  no se comportaban igual: una manejaba el nulo y la otra explotaba con "cannot read properties of
  null". nadie lo habia notado porque no habia un solo test que la ejercitara. me quede con la que
  no rompe. tambien estaban duplicadas escapar() y la tabla de modalidades.

- el dockerfile. al agregar el proyecto de tests a la solucion, el restore adentro del contenedor
  no lo encontraba, porque mi dockerfile lista los csproj uno por uno. lo peor es que lo que se
  rompia no eran los tests sino el build que ya tenia verde desde el tp4. se arreglo agregando el
  COPY del csproj de tests antes del restore.

- vulnerabilidades en vitest. instale la version 3 siguiendo la guia y npm audit me marco 2
  vulnerabilidades moderadas en @vitest/mocker. pase a la 5, que da 0. el efecto secundario fue que
  la 5 usa rolldown, que tiene un binario nativo por plataforma, y en windows me fallo con "cannot
  find module @rolldown/binding-wasm32-wasi". se arregla borrando node_modules y el lock y
  reinstalando limpio, y me paso dos veces: cada npm install incremental se lo vuelve a comer.

- el package-lock. lo habia puesto en el .dockerignore del front y npm ci lo necesita si o si. lo
  saque. node_modules si queda afuera.

- los .test.js se estaban copiando a la imagen de nginx. lo vi entrando al contenedor: adentro de
  /usr/share/nginx/html/js/lib estaban formato.test.js y partidos-datos.test.js. no es un agujero
  de seguridad pero es codigo de test en produccion, y nadie lo habia notado porque la imagen
  construia bien. ahora la etapa de build los borra.

- el NFR1, que lo tuve que cambiar. tenia escrito en el prd que el frontend va sin bundlers y sin package.json. este tp me obliga a
  agregar uno para correr vitest. de todo lo que decia el NFR1 lo unico que se rompe es el
  package.json: los modulos es son javascript nativo, no un bundler. y la imagen no cambia en nada:
  lo verifique entrando al contenedor, adentro hay nada mas que css/, js/ y los html. vitest quedo
  como devDependency y nunca entra.
  
  o sea que lo que el NFR1 queria proteger, no meter complejidad en produccion, se sigue cumpliendo.
  lo que cambio es que aparecio un requisito nuevo que cuando lo escribi no estaba contemplado. la
  proxima escribiria los NFR diciendo en que entorno aplican: "sin package.json" prohibia una
  herramienta, y lo que yo queria proteger era el runtime.

10) uso de ia en este tp

lo que hizo la ia: Escribió los tests. Arrancamos con los obligatorios(viendo en todo momento la logica y porque estabamos haciendo), y como cubrían poco también escribió los que están de más ya que con los obligatorios vi que cubria un 20% y como explique en el punto 2 que si tenia menos de ese porcentaje no me parecia que el codigo este testeado.
Midió la cobertura con distintos criterios de exclusión, para comparar cuánto cambiaba el número.
Cableó la etapa de tests en los dos Dockerfiles y los pasos del ci.yml.
Encontró cosas que yo no sabía que tenía. Una fue el bug de formatearFecha, que estaba duplicada y se comportaba distinto en cada lugar. La otra, que los .test.js se estaban copiando a la imagen de nginx.

Me ayudo a la hora de escribir el contenido de los pr y a subir las cosas al repo

## TP6 - CD: environments, aprobaciones y deployment patterns

los enlaces de este tp (paquetes, corridas y urls) estan arriba de todo, en "Enlaces de este TP".

1) por que el artefacto se publica solo con la verificacion en verde

el pipeline ahora deja algo: las dos imagenes en ghcr.io, etiquetadas con el commit que las produjo
(sha-<commit>). que en el registry haya solo cosas verificadas no lo garantiza un control, lo
garantizan tres eslabones encadenados:
1. nada entra a main sin el pipeline en verde (el gate del tp4 y el umbral de cobertura del tp5).
2. solo main publica: el push de la imagen tiene la condicion de evento push Y rama main. en un PR
   construye y verifica pero no sube nada.
3. publicar es el ULTIMO paso del mismo job que corre los tests. si un test falla el job muere ahi
   y nunca llega a publicar. no hay ningun if de "si los tests pasaron": la condicion es el orden.

si se publicara igual con los tests en rojo, estar en el registry dejaria de significar "esto paso
la verificacion" y pasaria a ser un deposito de cosas que alguien construyo alguna vez.

lo que NO garantiza: es "por accidente". yo puedo hacer un docker push a mano desde mi maquina y
nadie me lo impide; la cadena garantiza lo que publica el pipeline, no lo que fisicamente puede
entrar. y la etiqueta sha-... se puede pisar; lo que identifica una imagen de forma inmutable es su
digest (sha256:...).

el permiso de escribir paquetes lo declare adentro de cada job y no arriba de todo, porque arriba
reemplaza el default de todos los jobs: asi solo tiene permiso el que publica (minimo privilegio).

2) continuous delivery, no continuous deployment

implemente continuous delivery: todo cambio verificado llega solo a QA y queda listo para prod,
pero a prod lo sube una persona. no hice continuous deployment (sin humano) porque mi red de
seguridad no alcanza: el smoke solo prueba que la app contesta, no tengo monitoreo ni metricas de
errores, y los tests de integracion con la api levantada recien llegan en el tp7. automatizar el
ultimo paso con eso seria automatizar la propagacion de errores.

3) la cadena: needs, if y environments

build-backend y build-frontend (en paralelo) → deploy-qa → deploy-prod

- deploy-qa tiene needs de los dos builds: sin ci verde no hay deploy. y tiene el if de rama main:
  los PRs verifican pero no despliegan.
- deploy-prod tiene needs: deploy-qa. no repite el if de la rama y no es un olvido: como depende de
  deploy-qa, que si lo tiene, en un PR no corre ninguno de los dos (se ve en los checks de los PRs:
  los dos salen salteados). y si el smoke de QA falla, deploy-prod ni arranca.
- environment: qa y environment: production conectan cada job con su environment: hereda sus
  secrets, aplica sus reglas y queda en el historial de Deployments.
- deploy-prod tiene concurrency (group deploy-prod) para que no haya dos deploys a prod pisandose.
  no ordena la cola de aprobaciones: si quedan dos corridas esperando, la vieja la rechazo a mano,
  porque aprobarla despues de la nueva haria retroceder prod.

el &ref=$GITHUB_SHA en el hook es lo mas importante del job: un hook pelado despliega la punta de
la rama. con dos merges seguidos, la corrida del primero desplegaria el segundo, que todavia no
paso por nada; y en prod, entre que la corrida espera y yo apruebo, main se puede mover, y
aprobaria una cosa y subiria otra. con el &ref se despliega el commit que se verifico y se aprobo.
va con & y no con ? porque el hook ya trae su ?key=. lo comprobe en render → deploys: cada deploy
dice el commit del merge que lo disparo.

4) el alcance de cada secret

los cuatro deploy hooks de render son secrets (con la url cualquiera despliega mi app), y cada uno
vive en su environment, no en el repo:
- qa: RENDER_HOOK_API_QA y RENDER_HOOK_FRONT_QA
- production: RENDER_HOOK_API_PROD y RENDER_HOOK_FRONT_PROD

los de prod solo los lee un job que apunta a production, y ese job no arranca hasta que yo apruebo:
sin aprobacion no los lee nadie. si estuvieran en el repo, cualquier job de cualquier workflow los
podria usar y desplegar a prod salteandose el gate. los cargue sin --body / desde la web, para que
la url no quede en el historial de la terminal.

el token del registry no es un secret mio: es el GITHUB_TOKEN que github le da a cada corrida y
vive lo que dura el job.

que quedo por variable y que quedo adentro de la imagen:

el front: el nginx.conf paso a ser una plantilla (default.conf.template en /etc/nginx/templates/).
la imagen oficial de nginx reemplaza ${BACKEND_URL} y ${DNS_RESOLVER} al arrancar. en compose valen
los defaults del dockerfile (http://backend:8080 y el dns de docker), y en render cada front tiene
la url de la api de SU entorno y un dns publico. lo probe local: la misma imagen sin variables
apunta a backend:8080 y con variables apunta a la api de qa. misma imagen, dos configuraciones: es
lo que hace posible el tp7 (desplegar la misma imagen en qa y prod).

el back, por variable de entorno en cada servicio de render: la cadena de conexion de SU base,
Jwt__Key (distinta en qa y en prod), App__MiEquipo, Cors__OrigenesPermitidos__0 (el front de su
entorno), ASPNETCORE_ENVIRONMENT y Salud__TimeoutChequeoMs.

adentro de la imagen queda solo lo que es igual en todos lados: el codigo, el sitio estatico y los
defaults de compose. la guia pide solo la connection string, pero mi app no arranca sin esas cinco
variables (lo valida al arrancar y dice cual falta).

5) que mira mi aprobador antes de aprobar

1. que build y QA esten en verde.
2. CUANTO tardo el smoke de QA. si dio verde en menos de lo que tarda render en construir (minutos),
   no probo nada: probo la version anterior.
3. que QA este corriendo ESTE commit: el /api/health devuelve el commit, y tiene que ser el de la
   corrida que estoy aprobando.
4. que cambia este deploy (el PR) y si toca algo delicado: la base, la cadena de conexion, una
   migracion.
5. que no haya otra corrida mas nueva esperando: si la hay, rechazo la vieja.

lo que mi aprobador NO puede ver hoy: si la app anda bien con usuarios reales (no hay metricas ni
logs centralizados), si una migracion es reversible, y la version del front (nginx no informa su
commit, solo la api).

el rechazo: lo hice dos veces. el que defiendo es el segundo
(https://github.com/NicolasBono/Historialcancha/actions/runs/39068216767): el smoke de QA dio verde
2 segundos despues de disparar los hooks (20:48:52 → 20:48:54). en 2 segundos render no puede
construir, asi que el smoke valido la version anterior y no el commit eebe0b0. no subi a prod algo
que esa corrida no habia verificado. el job quedo en failure y prod no se toco.

y ese rechazo lo arregle: el /api/health ahora devuelve el commit que corre (render lo inyecta en
RENDER_GIT_COMMIT) y el smoke de qa y de prod solo da verde cuando coincide con el commit que mando
a desplegar. en la corrida siguiente el smoke espero 65 segundos, hasta que render termino. ese
fue el deploy que aprobe.

6) la letra chica del free tier

- render duerme cada servicio a los 15 min sin trafico, y el primer pedido lo despierta con un
  cold start de hasta ~1 minuto. por eso el smoke reintenta 30 veces cada 20 s, con --max-time 10
  en cada curl (un servicio despertando acepta la conexion y no contesta; sin tope el curl se
  cuelga).
- 750 horas por mes por WORKSPACE, no por servicio. con los 4 durmiendo cuando no se usan alcanza;
  lo que no hago es dejar algo pingueando para que no duerman.
- 500 minutos de build por mes. cada deploy son 2 builds por entorno (api y front): una promocion
  completa son 4 builds de ~1-2 min. si se acaban, render deja de construir y el hook responde
  igual, por eso importa que el smoke compare el commit.
- neon suspende la base a los ~5 min sin uso y se despierta sola (mas rapido). 0.5 GB.
- un problema propio de la region: la api esta en virginia y la base en sao paulo, y mi chequeo
  de salud le daba 600 ms a la base: no alcanzaba y el health decia baseDeDatos: error con la base
  andando. lo subi a 3000 con la variable Salud__TimeoutChequeoMs en render, sin tocar el codigo
  (en compose la base esta al lado y los 600 ms estan bien).

7) que garantia pierdo porque render reconstruye

mi pipeline publica la imagen que verifico, pero render no la usa: vuelve a construir desde el repo
con el commit que le paso. o sea que despliego el commit verificado, no la imagen verificada. son
dos builds del mismo codigo y pueden salir distintos: una imagen base (nginx:alpine, aspnet:9.0)
que se actualizo entre un build y otro, o una dependencia que cambio. si pasa, lo que corre en prod
no es lo que pasaron mis tests. se arregla en el tp7, cuando render deje de construir y ejecute la
imagen del registry.

8) que prueba mi smoke y que no

prueba: que la api contesta, que la base responde, que el front se sirve, que el front llega a SU
api a traves de la plantilla de nginx, y que la api corre el commit que se mando a desplegar.

mi smoke es distinto al del video: mi /api/health devuelve 200 aunque la base este caida (lo
informa en el campo baseDeDatos), y todos los endpoints de datos piden login, asi que no tengo un
/api/tareas para pegarle. por eso la prueba de la base es leer "baseDeDatos":"ok" y no el codigo
http. y le agregue pegarle a front/api/health, que el del video no tiene.

no prueba: que la app funcione bien (no hace login ni carga un partido), la version del front, ni
que no haya errores para los usuarios.

9) deployment pattern para una produccion real, y rollback

para esta app con usuarios reales usaria blue-green. es una app chica con una sola instancia, asi
que rolling no aplica (no hay "tandas"), y canary pide metricas para decidir si el canario anda
bien, que hoy no tengo. blue-green me da lo que mas me importa: rollback instantaneo (volver el
router al entorno anterior) y cero downtime. el costo es tener la infraestructura doble, que para
una app chica es poco. el cuidado esta en la base: las dos versiones usan la misma, asi que las
migraciones tienen que ser compatibles hacia atras (agregar columnas, no borrarlas en el mismo
deploy).

me falta para hacerlo en serio: un router o balanceador delante, y observabilidad (errores y
latencia por version) para decidir cuando volver atras, que es lo del tp9.

feature flags los usaria para desacoplar deploy de release: por ejemplo subir apagada una
estadistica nueva y prenderla primero para mi usuario.

mi rollback hoy, paso a paso (lo hice de verdad):
1. veo en Deployments → production cual fue el ultimo deploy bueno (o el tag de la release
   anterior: v6.0.0 = 234eac3).
2. vuelvo a correr SOLO el job deploy-prod de la corrida de ese commit (no re-run all jobs). pide
   aprobacion igual y despliega su propio commit con el &ref. otra forma, sin github: llamar a los
   dos hooks de prod con &ref=<ese commit>.
3. el smoke de prod espera a que la api informe ese commit.

lo probe: prod estaba en 1.1.0 (5450077) y lo volvi a v6.0.0. los hooks se dispararon a las
18:17:15 y la api de prod contesto con 234eac3 a las 18:18:09. en render → deploys: api-prod
50 s y front-prod 26,7 s. el rollback tarda alrededor de un minuto desde que se dispara (mas lo
que tarde yo en aprobar). es la metrica dora de tiempo de recuperacion.

lo que el rollback de codigo NO deshace: los datos. si un deploy hubiera corrido una migracion que
borra una columna, volver el codigo no la trae de vuelta. para eso haria falta un backup de neon
antes de cada migracion, o migraciones que nunca borren en el mismo deploy.

render tiene un boton de rollback propio, pero no lo uso: vuelve atras sin pasar por mi pipeline ni
por la aprobacion.

10) que use

- registry: ghcr.io, con el GITHUB_TOKEN de cada corrida (ningun secret nuevo).
- entornos: dos environments de github, qa (sin reglas) y production (con required reviewer: yo).
- donde corre: render, 4 web services con docker (api y front, en qa y en prod). region virginia,
  plan free.
- base de datos: neon, un proyecto con dos bases separadas, app_qa y app_prod.
- quien dispara el deploy: mi pipeline, con el deploy hook de render + &ref=<commit>. el auto-deploy
  esta en off en los 4.
- las tablas: las crea la app sola al arrancar (MigrateAsync de entity framework).

no use el postgres de render porque el gratis vence a los 30 dias, y neon es gratis permanente.

los cinco puntos del contrato: dos entornos con su url (qa y prod), una base por entorno (app_qa y
app_prod), front y back como contenedores a partir de mis dockerfiles (runtime docker en render), el
deploy lo dispara el pipeline con el commit verificado (auto-deploy off), y prod detras de la
aprobacion del environment production.


11) problemas encontrados

- la primera publicacion fallo con permission_denied: write_package. los paquetes
  historialcancha-backend y -frontend ya existian porque los habia subido a mano en el tp2, con mi
  usuario, y no estaban conectados al repo. el pipeline no entra con mi usuario, entra con el
  GITHUB_TOKEN, que solo puede escribir en paquetes conectados a este repo. lo resolvi en cada
  paquete (package settings → manage actions access → el repo con rol write) y volvi a correr solo
  los jobs que fallaron. no quedo nada a medias: el paso que fallo era el ultimo.
- el nombre de la imagen: github.repository trae mayusculas (NicolasBono/Historialcancha) y ghcr
  las rechaza. lo escribi a mano en minusculas.
- el health decia baseDeDatos: error con la base andando (los 600 ms, punto 6).
- el smoke daba verde contra la version vieja (punto 5). lo arregle con el commit en el health.
- el rechazo lo tuve que hacer dos veces. la primera segui el video y rechace por el mismo motivo que
  el, "sabado no se trabaja" (https://github.com/NicolasBono/Historialcancha/actions/runs/39067827154).
  despues me di cuenta que ese motivo no salia de mirar la corrida: no tenia nada que ver con lo que
  se estaba por subir. con ayuda de la ia revise la corrida siguiente y encontramos un motivo real:
  el smoke de qa habia dado verde 2 segundos despues de disparar los hooks, y en ese tiempo render no
  puede construir, asi que habia probado la version anterior. ese es el rechazo que defiendo (punto 5).
- un secret cargado como variable: el hook del front de qa lo puse en "environment variables" (se
  ve en texto claro y el job no lo encuentra). lo borre y lo cargue como secret.
- git bash cambia las rutas que empiezan con / (docker exec ... /etc/nginx/...) y les pega
  C:/Program Files/Git adelante. se evita con MSYS_NO_PATHCONV=1.

12) uso de ia en este tp
lo que hizo la ia: escribio los cambios del ci.yml (publicar la imagen, los jobs de qa y de prod y
el smoke) y la plantilla de nginx. reviso mi app contra la guia y encontro que el health respondia
200 con la base caida y que me faltaban variables en render. me ayudo a diagnosticar el
permission_denied del registry y el timeout del health

lo que hice yo: cree las cuentas y los servicios de render y neon, los dos environments con la
aprobacion en production y cargue los secrets. hice los merges, aprobe y rechace los deploys y
cronometre el rollback.

como lo verifique:  mire en render → deploys que cada deploy tenia el commit del merge, el
/api/health de qa y prod devolviendo el commit, y el rechazo: vi que el smoke dio verde en 2
segundos y por eso no lo aprobe

## TP7 - Contenedores en el pipeline + integracion y e2e

los enlaces de este tp estan arriba de todo, en "Enlaces del TP7".

1) build once, deploy many: que problema del tp6 resuelve

en el tp6 render reconstruia mi app desde el repo en cada deploy. lo que corria en qa y en prod no
era la imagen que pasaron mis tests: eran dos builds mas del mismo commit, a otra hora, que podian
salir distintos (una imagen base como nginx:alpine o aspnet:9.0 que se actualizo, una dependencia
resuelta de nuevo). ahora el pipeline construye la imagen UNA vez, la publica en ghcr.io etiquetada
con el commit, y los cuatro servicios de render la ejecutan tal cual (fuente "Existing Image").
qa y prod corren el mismo binario. promover es decirle a prod "ejecuta sha-<commit>", la misma que
qa ya verifico; y volver atras es ejecutar una imagen anterior, que sigue en el registry.

2) mi estrategia de etiquetas

- sha-<commit completo> en el registry: dice de que commit salio cada imagen. es la que manda cada
  deploy en el imgURL del hook, a qa y a prod. son los 40 caracteres: el registry no completa como
  git (con el corto el hook contesta 400 "unable to fetch image").
- v7.0.0 en git: es el nombre de la version que se que anduvo en prod. el tag no esta en el
  registry: apunta a un commit, y ese commit me dice que imagen es.
- no publico latest porque es un puntero que se mueve: no dice que contenido hay detras, y un
  deploy que lo nombrara ejecutaria "lo ultimo que se publico", haya pasado las pruebas o no.
- lo que no garantiza la etiqueta: se puede volver a publicar (re-correr la corrida de un commit
  publica otra vez sha-<ese commit>). lo inmutable es el digest (sha256:...).
- en render configure los cuatro servicios con la imagen de un merge viejo (sha-5450077...). esa
  NO es la que corre: render pide una imagen para cambiar la fuente, pero la que corre la elige
  cada deploy del pipeline con su imgURL (render solo exige que coincidan el registry y el nombre;
  la etiqueta puede cambiar). por eso nunca uso "manual deploy": desplegaria la de la
  configuracion, la vieja, y prod volveria para atras sin avisar.
- no cree servicios nuevos: les cambie la fuente a los cuatro, asi conserve las urls, las
  variables, los hooks y el historial.

3) de la release a la imagen, en un paso

`git rev-list -n1 v7.0.0` da 575a5a1..., y en mis dos paquetes esta la imagen sha-575a5a1...
(backend y frontend, la misma etiqueta). el tag lo puse sobre el commit que dice Deployments de
github para production, no sobre el ultimo de main.

4) como se comprueba desde afuera que un entorno ejecuta mi imagen

- en render, en Events de cada servicio, el deploy dice "Triggered via Deploy Hook" y nombra la
  imagen sha-<commit> de esa corrida (eso lo muestro en vivo: los events no son publicos).
- y lo hice automatico para la api: el commit va horneado adentro de la imagen (un ARG COMMIT en
  el dockerfile que el pipeline le pasa con build-args, y queda en App__Commit) y /api/health lo
  devuelve. el smoke espera hasta que el commit del health sea el de la corrida. esto lo necesitaba
  porque en el tp6 el health leia el commit de RENDER_GIT_COMMIT, y con una imagen render ya no
  construye desde git y esa variable deja de existir.
- lo que no prueba el smoke: la version del FRONT. nginx no informa su commit. y eso me paso de
  verdad (punto 10).

5) que pruebas elegi en cada suite

integracion (frontend/e2e/api.spec.js, playwright con el fixture request: sin navegador, contra la
api de qa con su base de verdad):
- alta + get que lo encuentra + borrado + get que ya no lo encuentra.
- un partido sin rival → 400 con la regla rival-requerido, y no se creo nada.
- la tercera: dos partidos el mismo dia → el segundo da 400 fecha-duplicada y queda uno solo. la
  elegi porque es una regla que depende de lo que YA HAY en la base: el dominio recibe "ya existe
  otro ese dia" leido de postgres. un unitario con un doble contesta lo que yo le diga.

e2e (frontend/e2e/partidos.spec.js, un chromium de verdad contra el front de qa):
- cargar un partido → aparece en el historial → borrarlo → ya no esta.
- un partido con fecha futura → el usuario ve el error de la api → no se cargo nada.
- la tercera: el partido cargado sigue ahi al recargar la pagina. es lo que el hincha hace todos
  los dias (cargar el partido y volver a verlo), y si se rompe es lo primero que me escriben.

todas entran como una persona (login por la pantalla), buscan por label y rol (getByLabel,
getByRole), afirman sobre su propio dato (un rival con la hora adentro, que no se repite) y borran
lo que crean. para poder borrar le agregue al front un aria-label "Borrar partido contra X" en cada
fila: sin eso no habia forma accesible de encontrar el boton.

el usuario de prueba es FIJO por entorno (no uno nuevo por corrida): mi app no tiene forma de
borrar usuarios, asi que crear uno por corrida llenaria qa de cuentas. si no existe, la primera
corrida lo registra. sus credenciales van en secrets del repo (E2E_DNI y E2E_CONTRASENA) y no en el
environment qa: los jobs de prueba no despliegan, y con environment: qa cada corrida sumaria
"deploys a qa" falsos al historial de Deployments.

que NO puse: las reglas una por una (goles negativos, nota fuera de rango, rival = mi equipo)
estan en los unitarios del tp5, que corren en milisegundos. arriba van pocas y caras: lo que solo
se ve con la base de verdad (integracion) o con la pantalla de verdad (e2e).

6) que pruebo en integracion y que en e2e: el par verde/rojo

la integracion le habla a la api directo, asi que prueba que la api y la base se entienden. la e2e
prueba que el front use bien esa api. por eso juntas dan un diagnostico:

- integracion verde y e2e roja: se rompio el front.
- integracion roja (y la e2e no corre): se rompio la api o la base.
- las dos verdes: no se rompio nada de lo que estas dos ven.

mi rotura: cambie en el front el nombre del campo que manda el alta (rival → nombreRival), sin
tocar e2e/. compilaba, los unitarios pasaban (no prueban ese armado) y el smoke dio verde. en la
corrida, integracion VERDE (3/3) y e2e ROJA (3/3), y deploy-prod ni pidio aprobacion. es el primer caso:
la api esta sana (le hable con el nombre correcto y guardo y borro), el que no la usa bien es el
front. lo confirme con el reporte, no con el codigo: en la traza, pestaña network, el POST manda
nombreRival y la api contesta 400 "El rival es obligatorio". si hubiera roto la api, la integracion
se ponia roja y la e2e no llegaba a correr (su needs no se cumple).

un cambio que solo atrapa la integracion: mi dominio no valida el largo del rival, pero la columna
tiene un maximo de 80 caracteres. un rival de 100 pasa el unitario (el doble acepta cualquier cosa)
y la base de verdad lo rechaza: la api contesta 500. no lo agregue como prueba y lo dejo anotado como
deuda. el motivo es que una prueba de integracion que espere un 500 estaria fijando un error como si
fuera lo correcto: lo que esta mal es que el dominio no valide el largo. el arreglo de verdad es
agregar esa regla al validador (maximo 80, igual que la columna) para que la api conteste 400 con un
mensaje claro, con su unitario; y recien ahi una prueba de integracion que confirme que la base y el
dominio dicen lo mismo. no lo hice en este tp porque cambia una regla de negocio y queria que este tp
fuera solo sobre el pipeline y las pruebas.

7) por que mi integracion es la amplia y no la estrecha

la amplia le pega a la api ya desplegada en qa, con su base de verdad. la estrecha arma la api
adentro del pipeline (WebApplicationFactory) contra un postgres descartable del job.
- gana: un decimo de configuracion (ya tengo qa corriendo la imagen exacta de la corrida) y ademas
  prueba que el deploy quedo bien.
- pierde: llega mas tarde (necesita el deploy), depende de que qa este despierto, y comparte la base
  de qa con todas las corridas (por eso cada prueba borra lo suyo).

8) cold start y tests flaky

render duerme los servicios. el smoke de deploy-qa ya despierta qa antes de las pruebas, y la config
de playwright tiene topes largos (60 s por test, 15 s por asercion), un reintento y nada de sleep:
playwright espera solo a que aparezca lo que busca. un test flaky es el que a veces pasa y a veces
no: es peor que no tenerlo, porque acostumbra a ignorar el rojo. con el reintento, un test que pasa
recien en el segundo intento sale marcado "flaky" con la corrida verde, por eso el reporte se mira
igual.

y tuve uno de verdad (punto 10): el test tocaba "Guardar" antes de que la pagina terminara de cargar
su javascript. lo arregle esperando un estado de la app (el contador de partidos, que el script
pinta cuando ya engancho el formulario), no con un sleep.

9) la misma imagen del front en qa y en prod

la direccion del backend no esta adentro de la imagen: nginx la lee del entorno al arrancar (la
plantilla del tp6). en render cada front tiene su BACKEND_URL (la api de SU entorno) y
DNS_RESOLVER. adentro de la imagen queda el sitio y los defaults de compose. por eso la misma
imagen sha-<commit> del front corre en qa y en prod y cada una habla con su api.

10) problemas encontrados

- el primer deploy a mano con el hook dio 400 "docker environment does not support image URLs":
  todavia no le habia cambiado la fuente a ese servicio.
- les cargue a los cuatro servicios la imagen -backend. el front de prod rechazo el deploy con 400
  (render no deja cambiar el nombre de la imagen por el hook, solo la etiqueta), y la api de prod ya
  se habia desplegado: prod quedo un rato con la api nueva y el front viejo. lo corregi poniendo
  -frontend en los fronts y re-corriendo solo el job deploy-prod (pidio aprobacion de nuevo).
- `npm i -D @playwright/test` en windows borro del package-lock los binarios de rolldown (el mismo
  bug del tp5) y vitest no arrancaba. regenere el lock de cero; de paso se fue una vulnerabilidad
  de vite.
- la corrida del arreglo (https://github.com/NicolasBono/Historialcancha/actions/runs/39511147445)
  tambien quedo roja, y no era mi codigo. el reporte mostraba "El rival es obligatorio", el error de
  la version ROTA: el front de qa todavia no habia terminado de desplegarse. el smoke solo espera el
  commit de la api; el front no informa el suyo, asi que la e2e arranco contra un front viejo. lo
  detecte con el reporte y con los events, y re-corri solo el job e2e (valido porque qa ya tenia la
  imagen de ESA corrida). al re-correr aparecio el test flaky del punto 8 y lo arregle en otro PR.
  esa corrida la deje en rojo a la vista a proposito. como lo cerraria: hornear el commit tambien
  en el front (un version.txt) y que el smoke espere los dos.
- limite conocido: qa es uno solo. con dos merges seguidos, la corrida B puede redesplegar qa
  mientras las pruebas de A lo usan. lo reconoceria mirando en events que imagen corre qa y a que
  hora corrio el deploy-qa de B; la corrida A se rechaza y vale la B. por eso hago un merge por vez.

11) uso de ia en este tp

lo que hizo la ia: escribio los cambios del ci.yml para desplegar la imagen con imgURL y los jobs
de integracion y e2e, el ARG COMMIT para hornear el commit en la imagen, y las dos suites de
playwright. me ayudo a diagnosticar las dos cosas raras que pasaron: que la e2e de la corrida del
arreglo corrio contra el front viejo, y el test flaky que tocaba "guardar" antes de tiempo. y armo
el borrador de este decisiones.

lo que hice yo: les cambie la fuente a los 4 servicios de render (y arregle el que tenia la imagen
equivocada), cargue los secrets del usuario de prueba, elegi la tercera prueba de cada suite guiado por la ia,
rompi la app a proposito, hice los merges y las aprobaciones

como lo verifique: abri el reporte de la corrida roja y en la traza (network) vi que el POST
mandaba nombreRival y la api contestaba 400; mire en los events de render que cada servicio corriera el sha de la corrida

## TP8 - Infraestructura como codigo

declare un entorno nuevo, preprod (base + api + front), en infra/. no existe hasta que corro
terraform apply y desaparece con terraform destroy. qa y prod no se tocaron: siguen siendo los que
clickee en el tp6. pr #42 (infra/ en main) y release v8.0.0.

1) riel y versiones

riel canonico: terraform (v1.16.6, instalado con winget) contra neon y render, los mismos
proveedores del tp6. providers fijados con version exacta y el .terraform.lock.hcl commiteado:
- kislerdm/neon 0.18.0: proyecto propio historialcancha-preprod (para poder destruirlo entero sin
  rozar el de qa y prod), rol y base app_preprod, en sao paulo como mi base de siempre.
- render-oss/render 1.9.1: historialcancha-api-preprod y historialcancha-front-preprod, plan free,
  virginia, con la imagen de v7.0.0 (sha-575a5a1..., la que corre en prod).
- hashicorp/random 3.7.2: genera la clave jwt (mi api no arranca sin una de 32+ caracteres).
el estado queda en infra/terraform.tfstate, en mi maquina, ignorado por git. no esta en el repo
porque mi repo es publico y adentro estan la contrasena del rol y la cadena de conexion en texto
plano. las llaves (NEON_API_KEY, RENDER_API_KEY, RENDER_OWNER_ID) las leen los providers del
entorno, no estan en ningun .tf.
lo que se deriva y no se copia: la cadena de conexion se arma con el host, la base, el rol y la
contrasena que devuelve neon; la BACKEND_URL del front sale de la url de su propia api.

lo que sigue siendo artesanal en mi cuenta: qa y prod enteros (los 4 servicios de render y las dos
bases de neon, que clickee en el tp6), los environments y secrets de github con los deploy hooks,
la proteccion de main, las llaves de api, el monitor de uptimerobot y sentry. si perdiera qa o
prod, rehacerlos seria repetir clicks de memoria; preprod lo recreo con un apply.

2) problemas y como los resolvi

- neon respondio "org_id is required" con mi personal api key. el porque: mi cuenta vive dentro de
  una organizacion, y en ese caso los proyectos no son del usuario sino de la organizacion. como una
  misma llave personal puede tener acceso a varias organizaciones, neon necesita que le digas en
  cual crear el proyecto. agregue org_id al neon_project, como variable (la guia no lo menciona
  porque arma la cuenta sin organizacion).
- mi api pide mas variables que la app de la guia: ConnectionStrings__HistorialCancha (no
  __Default), Jwt__Key, App__MiEquipo y un origen cors. el porque: mi api valida esas variables al
  arrancar y si falta una no levanta (lo hice asi desde el tp6, para que un entorno mal configurado
  falle al toque y no a la mitad). la clave jwt la genera terraform: nadie la escribe ni la copia.
  cambia en cada recreacion, y en preprod no importa porque lo unico que se pierde son las sesiones
  abiertas, y preprod se tira igual.
- el origen cors no podia salir de la url del front. el porque: terraform crea primero lo que
  otro necesita. el front necesita la url de la api (BACKEND_URL), asi que la api se crea antes; si
  ademas la api necesitara la url del front, cada uno esperaria al otro y terraform rechaza ese
  ciclo. lo arme con el nombre del front
  (https://historialcancha-front-preprod.onrender.com). no pasa nada si no coincide: el navegador
  llama a /api por el nginx del front, desde el mismo origen, y no usa cors.
- la guia prueba /health y /api/tareas; en mi app es /api/health (mirando "baseDeDatos":"ok" y el
  commit) y los datos piden login. el porque: la guia esta escrita para otra app, sin usuarios; en
  la mia cada partido es de un usuario, asi que sin token la api contesta 401. probe registro,
  login y /api/partidos pasando por el front, asi queda probado tambien que el front habla con SU
  api.
- lo no medido por la catedra, medido aca: al recrear, render devolvio las mismas urls (sin
  sufijo), porque la url sale del nombre del servicio y el nombre estaba libre otra vez; neon creo
  un proyecto nuevo con otro host, porque el host es propio de cada proyecto, y no importo porque
  la cadena se deriva. la base nace vacia y mi api corre las migraciones al arrancar: me pude
  registrar de nuevo con el mismo dni.
- el plan del destroy dice "6 to destroy" y no 5 como la guia (y el del apply "6 to add"). los 5 de
  la guia son el proyecto, el rol y la base de neon, y la api y el front de render. el sexto es la
  clave jwt (random_password). el porque: la app de la guia no tiene usuarios, asi que no necesita
  clave; la mia no arranca sin una Jwt__Key de 32 caracteres o mas. en vez de inventarla yo y
  guardarla en una variable, la genera terraform, asi nadie la escribe ni la copia.
  lo que me llamo la atencion es que random_password cuenta como recurso aunque no exista en ningun
  proveedor: no crea nada ni en neon ni en render, vive solo en el estado de terraform. pero
  terraform lo maneja igual que a los otros: lo crea en el apply, guarda el valor en el estado y lo
  destruye en el destroy. destruirlo es olvidar esa clave, por eso en el proximo apply sale una
  nueva y las sesiones viejas dejan de valer.

3) uso de ia en este tp
lo que hizo la ia: escribio el hcl de infra/ (main.tf, variables.tf y outputs.tf) adaptando la
guia a mi app: las variables que pide mi api, la cadena de conexion derivada, la clave jwt y el
cors. corrio init, plan, apply y destroy, y verifico por api que qa y prod seguian intactos
despues de cada cosa. diagnostico el "org_id is required" de neon y el ciclo entre el front y la
api. armo el commit y el pr.

lo que hice yo: cree las llaves de neon y render, provoque el drift borrando app_preprod en la
consola de neon (y cree y borre la base_intrusa para ver que el plan no la ve), entre al front de
preprod a registrarme y cargar un partido, hice el merge y aprobe prod.

como lo verifique: lei los planes antes de aplicar (que crea, que destruye y por que son 6 y no
5), y despues de recrear preprod me pude registrar de nuevo con el mismo dni.

## TP9 - DevSecOps + observabilidad

lo que quedo: dependabot en los 3 ecosistemas (pr #43), un pr de dependabot mergeado (#48), codeql
y push protection activos, un monitor de uptimerobot sobre el health de prod (pausado), sentry en
el back y en el front (pr #52, el endpoint de prueba se saco en el #54), una tarjeta en el tablero
nacida de un dato de sentry, y la release v9.0.0 sobre 45663a0 (el commit que corre en prod).

1) el pr de dependabot que mergee y por que me anime

el #48: Microsoft.AspNetCore.OpenApi de 9.0.15 a 9.0.21. es un patch (semver): mismo major, sigue
en .net 9 como el resto de mi app. toca solo el .csproj de la api y las notas de la version son de
mantenimiento (versiones, arreglos de dependencias de build), sin cambios que rompan. me anime
porque mi cadena lo prueba entero: el ci del pr compila y corre los tests con el umbral, y al
mergear paso por qa, integracion (4 pruebas) y e2e (3 pruebas) antes de que yo aprobara prod.
ese verde es lo que me deja mergear en minutos.
dependabot abrio 8 prs apenas llego el dependabot.yml a main. no mergee los otros: Identity.Core 10
es de .net 10 (mi app es .net 9), coverlet y Test.Sdk suben de major y pueden mover la cobertura del
gate, y las actions v6 a v7 tambien son major. quedan abiertos: no tocan main ni prod.

2) cuanto tardo el monitor, y mi operacion mas usada y la mas lenta

- antes de crear el monitor medi el health con HEAD (que es lo que usa uptimerobot gratis): prod
  contestaba 405. le agregue [HttpHead] al health y una prueba de integracion que lo cuida, y lo
  lleve a prod antes de crear el monitor. si no, el monitor marcaba caida con la api viva.
- caida provocada: suspend de la api de prod a las 19:05:53 → mail de DOWN a las 19:08, o sea ~2
  minutos (el maximo es 5, el intervalo del monitor). reanude a las 19:10:45 → mail de UP a las
  19:14. el historial del monitor dice "1 incident, 5m 5s down". despues lo pause: despierto todo
  el mes se come las 750 horas que comparten mis servicios gratis.
- lo que el monitor NO mira: mi health da 200 aunque la base este caida (el estado de la base va
  en el campo baseDeDatos). el monitor por palabra clave, que lo veria, es pago en uptimerobot.
  eso lo ve sentry (los errores) o un usuario.
- en sentry, despues de usar prod unos minutos: la mas lenta es la autenticacion (POST
  /api/auth/...), p50 1,11 s y p95 2,15 s, y fallo la mitad de las veces. la mas usada es GET
  /api/partidos (se pide en cada carga de pagina), p95 1,11 s. con eso abri la tarjeta "Perf: la
  autenticacion tarda 2,15 s en P95" en el tablero, con dos hipotesis: la base de neon dormida en
  el primer pedido, o el costo del hash de la contrasena.

3) problemas y como los resolvi

- el health contestaba 405 a HEAD (lo de arriba).
- sentry no veia mis errores: mi api tiene un ManejadorDeErrores que atrapa toda excepcion para
  devolver un 500 prolijo, asi que el middleware de sentry nunca se enteraba. le agregue
  SentrySdk.CaptureException en ese catch. los 400 de reglas de negocio no se mandan: son
  respuestas esperadas, no fallas. con eso aparecio "Falla de prueba del TP9" en issues.
- mi front no tiene empaquetador (nfr1), asi que no podia instalar sentry por npm como la guia.
  lo cargo desde el cdn de sentry con version fijada (11.9.0) e integrity (si el archivo cambia,
  el navegador no lo ejecuta), y un js/observabilidad.js hace el init en las 3 paginas. contra:
  dependabot no vigila esa version.
  el front tiene trazas igual que el back: uso el bundle que las trae (bundle.tracing.min.js, no
  el bundle.min.js que solo manda errores), y el init las prende con browserTracingIntegration()
  y tracesSampleRate 1.0. como no hay @sentry/react en el package.json, buscarlo ahi no lo
  encuentra: esta en el script de cada html (index.html, login.html y estadisticas.html) y en
  js/observabilidad.js. lo comprobe en sentry: el dashboard Frontend Overview del proyecto del
  front muestra las cargas de pagina con sus tiempos y el Performance Score de los Web Vitals,
  que solo los puede mandar el sdk del navegador.
- el dsn del back va como variable SENTRY_DSN solo en la api de prod. en render la guarde con
  "save only": en settings los servicios de prod tienen una imagen vieja, y "save and deploy" la
  habria desplegado. verifique por la api de render que no se disparo ningun deploy. el dsn del
  front va escrito en el codigo porque solo sirve para enviar datos, no para leerlos.
- codeql encontro 2 cosas que deje analizadas y sin arreglar: (a) el log de errores escribe la
  ruta que manda el usuario (log injection, owasp a09): alguien podria meter saltos de linea y
  falsear lineas del log; se arregla saneando la ruta. (b) 4 jobs del ci.yml no declaran
  permisos (minimo privilegio, owasp a01); se arregla con permissions: contents: read.
- el token de gh no tenia permiso sobre projects: la tarjeta la cree a mano.

4) uso de ia en este tp
lo que hizo la ia: adapto la guia a mi app. midio el health con HEAD antes de crear el monitor y
encontro el 405, y escribio el arreglo con su prueba de integracion. escribio el dependabot.yml y
me ayudo a analizar los 8 prs que abrio dependabot para elegir cual mergear. escribio el codigo de
sentry en el back (con el CaptureException en el manejador de errores) y en el front (desde el cdn
con integrity). antes de cada aprobacion de prod revisaba la corrida

lo que hice yo: prendi dependabot, codeql y la proteccion de secretos en settings. cree el monitor
de uptimerobot, provoque la caida (suspend y resume de la api de prod en render) y lo pause.
cree la cuenta y los proyectos de sentry y cargue el SENTRY_DSN en render con "save only". use la
app de prod para generar datos, cree la tarjeta en el tablero e hice los merges y las
aprobaciones.

como lo verifique: me llego el mail de DOWN y el de UP y lo vi en el historial del monitor; vi
aparecer "Falla de prueba del TP9" en issues de sentry; y mire en sentry las demoras de cada
operacion para sacar la mas lenta y la mas usada.
