/*
  Copiá este archivo como js/config.js y ajustá los valores.
  js/config.js NO se versiona: es la configuración de tu máquina.

  apiBaseUrl es el ÚNICO lugar del frontend donde se define a quién le pega la API.
  - En el contenedor (nginx sirviendo el front): dejalo RELATIVO, "/api". El browser pide
    /api/... al mismo origen desde el que cargó la página y nginx lo reenvía al backend.
    Así el front no sabe dónde vive el backend y no hay CORS.
  - Para correr el front suelto en local (p. ej. python -m http.server) contra el backend
    en otro puerto, poné la URL ABSOLUTA: "http://localhost:5080/api".
*/
window.APP_CONFIG = {
  apiBaseUrl: "/api",
  version: "1.0.0",
  entorno: "Development"
};
