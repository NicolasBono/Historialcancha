# syntax=docker/dockerfile:1

# ---- Etapa 1: preparar el sitio ----
# Este frontend es HTML/CSS/JS puro (NFR1: sin bundler, sin transpilador, sin
# package.json), así que acá no se "compila" nada. La etapa existe por una razón
# concreta: js/config.js NO se versiona (está en .gitignore), así que no viene en el
# contexto de build. Se genera desde la plantilla versionada, que ya trae apiBaseUrl
# relativo ("/api") — justo lo que necesita el front detrás de nginx.
# Sin este paso la imagen arrancaría sin configuración y la app moriría en el primer
# script con "Falta la configuración del frontend".
FROM alpine:3.20 AS build
WORKDIR /sitio

# Se copia sólo lo que se sirve: nada de Dockerfile ni nginx.conf adentro del sitio.
COPY css/ css/
COPY js/ js/
COPY *.html ./

RUN cp js/config.example.js js/config.js && rm js/config.example.js

# ---- Etapa 2: nginx sirve los estáticos ----
FROM nginx:alpine
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /sitio /usr/share/nginx/html
EXPOSE 80
