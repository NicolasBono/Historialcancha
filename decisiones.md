# Decisiones

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


