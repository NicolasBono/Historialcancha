/*
  Lo poco del frontend que necesita el DOM y no es pintar una pantalla.
  Vive aparte de lib/formato.js justo por eso: acá no llegan los tests unitarios,
  porque hace falta un navegador. Estaba duplicado en estadisticas.js y partidos.js.
*/

/**
 * Convierte un valor en texto seguro para interpolar dentro de HTML.
 * Se apoya en el navegador —textContent escapa, innerHTML devuelve el escapado—
 * en lugar de reemplazar caracteres a mano, que es donde se cometen los errores.
 */
export function escapar(valor) {
  const div = document.createElement("div");
  div.textContent = valor ?? "";
  return div.innerHTML;
}
