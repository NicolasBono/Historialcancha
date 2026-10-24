// Sentry en el front: errores del navegador, tiempos de carga y visitas, vistos desde
// producción. La librería llega por el CDN de Sentry (script anterior en cada página, con
// versión fijada e integridad): el front no tiene empaquetador (NFR1), así que no se instala
// por npm. Script clásico y no módulo, para que arranque antes que el código de la página.
//
// El DSN va escrito acá a propósito: el de un front no es un secreto. Sólo sirve para ENVIAR
// datos a Sentry, no para leerlos, y viaja igual en el JavaScript que baja el navegador.
(function () {
  if (!window.Sentry) return   // si el CDN no cargó (bloqueado, sin red), la app anda igual

  window.Sentry.init({
    dsn: 'https://2f5648b26a4472cf40a1153b34cf2b65@o4512313534578688.ingest.us.sentry.io/4512313568198656',
    environment: window.location.hostname,   // de qué sitio vino: QA, PROD o localhost
    integrations: [window.Sentry.browserTracingIntegration()],
    tracesSampleRate: 1.0,
  })
})()
