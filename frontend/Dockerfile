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

RUN cp js/config.example.js js/config.js && rm js/config.example.js  && rm -f js/lib/*.test.js
# El rm de los .test.js: viven al lado del código que prueban (es la convención de
# JavaScript) pero no son parte del sitio. Sin esta línea nginx los sirve igual.

# ---- Etapa 2: tests ----
# Acá NO se puede hacer `FROM build AS test` como en el backend: la etapa build es
# alpine pelado, que no trae Node. Parte de node:22-alpine, que es lo mínimo para
# correr vitest. La imagen final no hereda nada de esto.
FROM node:22-alpine AS test
WORKDIR /app
# El lockfile PRIMERO y el npm ci antes del código: es el paso caro y sólo depende
# de esos dos archivos, así que tocar una línea de test reusa esta capa del cache.
# Va npm ci (no install) y SIN --omit=dev: vitest es una dependencia de desarrollo,
# y saltearlas dejaría la etapa de tests sin la herramienta que tiene que correr.
COPY package.json package-lock.json ./
RUN npm ci
COPY vitest.config.js ./
COPY js/ js/
ENTRYPOINT ["npm", "run", "test:ci"]

# ---- Etapa 3: nginx sirve los estáticos (ya la tenías, queda ÚLTIMA) ----
FROM nginx:alpine
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /sitio /usr/share/nginx/html
EXPOSE 80
