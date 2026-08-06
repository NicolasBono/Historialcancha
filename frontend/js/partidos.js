/*
  Alta, edición, listado y baja de partidos.
*/
const MODALIDADES = {
  EnCancha: "En cancha",
  TV: "TV",
  Streaming: "Streaming",
  Radio: "Radio",
  NoLoVi: "No lo vi"
};

const RESULTADOS = { Victoria: "V", Empate: "E", Derrota: "D" };

let enEdicion = null;   // id del partido que se está editando, o null si es un alta

/* ---------- utilidades ---------- */

function escapar(texto) {
  const div = document.createElement("div");
  div.textContent = texto ?? "";
  return div.innerHTML;
}

function formatearFecha(iso) {
  const [anio, mes, dia] = iso.split("-");
  return `${dia}/${mes}/${anio}`;
}

function mostrarError(mensaje) {
  const caja = document.getElementById("error-form");
  caja.textContent = mensaje;
  caja.hidden = false;
}

function limpiarError() {
  const caja = document.getElementById("error-form");
  caja.hidden = true;
  caja.textContent = "";
}

/* ---------- listado ---------- */

function filaDePartido(p) {
  const marcador = `${p.golesAFavor} - ${p.golesEnContra}`;
  const condicion = p.condicion === "Local" ? "L" : "V";

  return `
    <tr>
      <td>${formatearFecha(p.fecha)}</td>
      <td class="rival">${escapar(p.rival)} <span class="condicion">${condicion}</span></td>
      <td>${escapar(p.torneo)}</td>
      <td class="marcador">${marcador}</td>
      <td><span class="resultado" data-resultado="${p.resultado}">${RESULTADOS[p.resultado]}</span></td>
      <td>${MODALIDADES[p.modalidad] ?? p.modalidad}</td>
      <td class="acciones">
        <button type="button" class="enlace" data-editar="${p.id}">editar</button>
        <button type="button" class="enlace peligro" data-eliminar="${p.id}">borrar</button>
      </td>
    </tr>`;
}

async function cargarPartidos() {
  const cuerpo = document.getElementById("cuerpo-tabla");
  const vacio = document.getElementById("sin-partidos");
  const tabla = document.getElementById("tabla-partidos");

  try {
    const partidos = await API.listarPartidos();

    document.getElementById("contador").textContent =
      partidos.length === 1 ? "1 partido" : `${partidos.length} partidos`;

    if (partidos.length === 0) {
      tabla.hidden = true;
      vacio.hidden = false;
      return;
    }

    tabla.hidden = false;
    vacio.hidden = true;
    cuerpo.innerHTML = partidos.map(filaDePartido).join("");
  } catch (error) {
    tabla.hidden = true;
    vacio.hidden = false;
    vacio.textContent = error.message;
  }
}

/* ---------- formulario ---------- */

function leerFormulario() {
  const valor = (id) => document.getElementById(id).value.trim();
  const nota = valor("nota");

  return {
    fecha: valor("fecha"),
    rival: valor("rival"),
    torneo: valor("torneo"),
    condicion: valor("condicion"),
    estadio: valor("estadio") || null,
    golesAFavor: Number(valor("golesAFavor")),
    golesEnContra: Number(valor("golesEnContra")),
    modalidad: valor("modalidad"),
    sector: valor("sector") || null,
    conQuien: valor("conQuien") || null,
    nota: nota === "" ? null : Number(nota)
  };
}

function limpiarFormulario() {
  document.getElementById("form-partido").reset();
  enEdicion = null;
  document.getElementById("titulo-form").textContent = "Registrar partido";
  document.getElementById("btn-guardar").textContent = "Guardar";
  document.getElementById("btn-cancelar").hidden = true;
  limpiarError();
}

function cargarEnFormulario(p) {
  const set = (id, v) => { document.getElementById(id).value = v ?? ""; };

  set("fecha", p.fecha);
  set("rival", p.rival);
  set("torneo", p.torneo);
  set("condicion", p.condicion);
  set("estadio", p.estadio);
  set("golesAFavor", p.golesAFavor);
  set("golesEnContra", p.golesEnContra);
  set("modalidad", p.modalidad);
  set("sector", p.sector);
  set("conQuien", p.conQuien);
  set("nota", p.nota);

  enEdicion = p.id;
  document.getElementById("titulo-form").textContent = `Editando el partido del ${formatearFecha(p.fecha)}`;
  document.getElementById("btn-guardar").textContent = "Guardar cambios";
  document.getElementById("btn-cancelar").hidden = false;
  limpiarError();
  document.getElementById("form-partido").scrollIntoView({ behavior: "smooth", block: "start" });
}

async function guardar(evento) {
  evento.preventDefault();
  limpiarError();

  const boton = document.getElementById("btn-guardar");
  boton.disabled = true;

  try {
    const partido = leerFormulario();

    if (enEdicion === null) {
      await API.crearPartido(partido);
    } else {
      await API.actualizarPartido(enEdicion, partido);
    }

    limpiarFormulario();
    await cargarPartidos();
  } catch (error) {
    // El backend ya mandó el motivo: se muestra tal cual, sin recargar la página
    // y sin perder lo que el usuario había cargado.
    mostrarError(error.message);
  } finally {
    boton.disabled = false;
  }
}

/* ---------- acciones de la tabla ---------- */

async function manejarClickTabla(evento) {
  const editar = evento.target.closest("[data-editar]");
  const eliminar = evento.target.closest("[data-eliminar]");

  if (editar) {
    const partidos = await API.listarPartidos();
    const partido = partidos.find((p) => p.id === Number(editar.dataset.editar));
    if (partido) cargarEnFormulario(partido);
    return;
  }

  if (eliminar) {
    const id = Number(eliminar.dataset.eliminar);
    if (!confirm("¿Borrar este partido? También se borra cómo lo viviste.")) return;

    try {
      await API.eliminarPartido(id);
      if (enEdicion === id) limpiarFormulario();
      await cargarPartidos();
    } catch (error) {
      mostrarError(error.message);
    }
  }
}

/* ---------- arranque ---------- */

document.addEventListener("DOMContentLoaded", () => {
  // La fecha no puede ser futura: el input tampoco la ofrece.
  document.getElementById("fecha").max = new Date().toISOString().slice(0, 10);

  document.getElementById("form-partido").addEventListener("submit", guardar);
  document.getElementById("btn-cancelar").addEventListener("click", limpiarFormulario);
  document.getElementById("tabla-partidos").addEventListener("click", manejarClickTabla);

  cargarPartidos();
});
