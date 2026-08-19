/*
  Arranque de la app: pinta versión y entorno, y vigila el estado del backend.
*/
const INTERVALO_CHEQUEO_MS = 30000;

function pintarIdentidad() {
  document.getElementById("version").textContent = "v" + window.APP_CONFIG.version;

  const entorno = document.getElementById("entorno");
  entorno.textContent = window.APP_CONFIG.entorno;
  entorno.dataset.entorno = window.APP_CONFIG.entorno.toLowerCase();
}

function pintarUsuario() {
  const usuario = Auth.usuario();
  const chip = document.getElementById("usuario");
  if (chip && usuario) chip.textContent = usuario.nombre + " " + usuario.apellido;

  const salir = document.getElementById("btn-salir");
  if (salir) salir.addEventListener("click", () => Auth.cerrarSesion());
}

function pintarEstado(estado, texto) {
  const indicador = document.getElementById("estado-backend");
  indicador.dataset.estado = estado;
  indicador.querySelector(".etiqueta").textContent = texto;
}

function pintarDetalle(salud) {
  const detalle = document.getElementById("detalle-salud");
  if (!detalle) return;   // la pantalla de partidos no muestra el detalle, sólo el badge

  if (!salud) {
    detalle.innerHTML = "<p class='vacio'>Sin datos del backend.</p>";
    return;
  }

  const filas = [
    ["Estado", salud.status],
    ["Versión del backend", salud.version],
    ["Entorno del backend", salud.entorno],
    ["Base de datos", salud.baseDeDatos],
    ["Equipo configurado", salud.miEquipo || "(sin configurar)"]
  ];

  detalle.innerHTML =
    "<dl>" +
    filas.map(([clave, valor]) => `<dt>${clave}</dt><dd>${valor}</dd>`).join("") +
    "</dl>";
}

async function chequearBackend() {
  try {
    const salud = await API.health();
    const baseOk = salud.baseDeDatos === "ok";

    pintarEstado(baseOk ? "ok" : "parcial",
      baseOk ? "backend disponible" : "backend sin base de datos");
    pintarDetalle(salud);
  } catch (error) {
    // Se atrapa siempre: la página tiene que seguir navegable con el backend caído.
    pintarEstado("error", "backend no disponible");
    pintarDetalle(null);
    console.info("Chequeo de salud fallido:", error.message);
  }
}

document.addEventListener("DOMContentLoaded", () => {
  // Guarda de página: sin sesión no se muestra nada, se va al login.
  Auth.exigirSesion();
  if (!Auth.haySesion()) return;

  pintarIdentidad();
  pintarUsuario();
  chequearBackend();
  setInterval(chequearBackend, INTERVALO_CHEQUEO_MS);
});
