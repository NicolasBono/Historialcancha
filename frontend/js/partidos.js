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
let equipos = [];       // catálogo de rivales; lo sirve la API, no vive en el frontend

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

/**
 * Los errores del formulario viven dentro del modal. Los de la tabla —borrar, por
 * ejemplo— ocurren con el modal cerrado, así que necesitan su propio lugar visible.
 */
function mostrarErrorListado(mensaje) {
  const caja = document.getElementById("error-listado");
  caja.textContent = mensaje;
  caja.hidden = false;
}

function limpiarErrorListado() {
  const caja = document.getElementById("error-listado");
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

/* ---------- catálogo de rivales ---------- */

function poblarSelectDeRivales() {
  document.getElementById("rival").innerHTML =
    ['<option value="">Elegí un equipo…</option>']
      .concat(equipos.map((e) => `<option value="${escapar(e)}">${escapar(e)}</option>`))
      .join("");
}

/**
 * Un partido viejo puede tener un rival que hoy no está en la lista: un club que
 * descendió, o una carga anterior a que existiera el catálogo. Editarlo no puede
 * perderle el rival, así que se le agrega su propia opción.
 */
function asegurarOpcionDeRival(rival) {
  if (!rival) return;

  const select = document.getElementById("rival");
  const yaEsta = [...select.options].some((o) => o.value === rival);
  if (yaEsta) return;

  select.insertAdjacentHTML(
    "beforeend",
    `<option value="${escapar(rival)}">${escapar(rival)} (fuera de la lista)</option>`
  );
}

async function cargarEquipos() {
  try {
    equipos = await API.listarEquipos();
    poblarSelectDeRivales();
  } catch (error) {
    // Sin catálogo no se puede elegir rival, así que el alta no tendría sentido.
    // El listado y las estadísticas siguen funcionando.
    document.getElementById("btn-agregar").disabled = true;
    mostrarErrorListado(`No se pudo cargar la lista de equipos (${error.message}) — el alta queda deshabilitada.`);
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

/**
 * Deja el formulario en modo alta. Se llama al ABRIR, nunca al cerrar: hacerlo en el
 * evento `close` deja un reset encolado que puede pisar la carga de una edición
 * inmediata posterior —y peor, borrar `enEdicion`, con lo que guardar daría de alta.
 */
function resetearFormulario() {
  document.getElementById("form-partido").reset();
  // Repoblar borra cualquier opción "(fuera de la lista)" que dejó una edición anterior.
  poblarSelectDeRivales();
  enEdicion = null;
  document.getElementById("titulo-form").textContent = "Registrar partido";
  document.getElementById("btn-guardar").textContent = "Guardar";
  limpiarError();
}

/* ---------- modal ---------- */

function modal() {
  return document.getElementById("modal-partido");
}

function abrirAlta() {
  resetearFormulario();
  modal().showModal();
  document.getElementById("fecha").focus();
}

function abrirEdicion(p) {
  const set = (id, v) => { document.getElementById(id).value = v ?? ""; };

  resetearFormulario();
  asegurarOpcionDeRival(p.rival);

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
  modal().showModal();
  document.getElementById("rival").focus();
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

    // Sólo se cierra si el backend aceptó: un rechazo tiene que quedar a la vista.
    modal().close();
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
    limpiarErrorListado();
    const partidos = await API.listarPartidos();
    const partido = partidos.find((p) => p.id === Number(editar.dataset.editar));
    if (partido) abrirEdicion(partido);
    return;
  }

  if (eliminar) {
    const id = Number(eliminar.dataset.eliminar);
    if (!confirm("¿Borrar este partido? También se borra cómo lo viviste.")) return;

    limpiarErrorListado();

    try {
      await API.eliminarPartido(id);
      await cargarPartidos();
    } catch (error) {
      mostrarErrorListado(error.message);
    }
  }
}

/* ---------- arranque ---------- */

document.addEventListener("DOMContentLoaded", () => {
  // La fecha no puede ser futura: el input tampoco la ofrece.
  document.getElementById("fecha").max = new Date().toISOString().slice(0, 10);

  document.getElementById("form-partido").addEventListener("submit", guardar);
  document.getElementById("btn-agregar").addEventListener("click", abrirAlta);
  document.getElementById("tabla-partidos").addEventListener("click", manejarClickTabla);

  document.getElementById("btn-cancelar").addEventListener("click", () => modal().close());
  document.getElementById("btn-cerrar").addEventListener("click", () => modal().close());

  // Un click sobre el fondo oscuro tiene como target al propio <dialog>:
  // si el click hubiera caído dentro del panel, el target sería algo de adentro.
  modal().addEventListener("click", (evento) => {
    if (evento.target === modal()) modal().close();
  });

  cargarEquipos();
  cargarPartidos();
});
