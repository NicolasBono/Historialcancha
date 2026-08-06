/*
  Pantalla de estadísticas. Todo lo que se muestra acá lo calculó el backend:
  este archivo no hace una sola cuenta, sólo pinta.
*/
const NOMBRE_MODALIDAD = {
  EnCancha: "En cancha",
  TV: "TV",
  Streaming: "Streaming",
  Radio: "Radio",
  NoLoVi: "No lo vi"
};

const NOMBRE_RACHA = {
  invicto: "Invicto",
  sinGanar: "Sin ganar",
  sinRecibirGoles: "Sin recibir goles"
};

const SIN_DATO = "—";

function texto(id, valor) {
  document.getElementById(id).textContent = valor;
}

function escapar(valor) {
  const div = document.createElement("div");
  div.textContent = valor ?? "";
  return div.innerHTML;
}

function formatearFecha(iso) {
  if (!iso) return SIN_DATO;
  const [anio, mes, dia] = iso.split("-");
  return `${dia}/${mes}/${anio}`;
}

/** Con cero partidos la efectividad no significa nada: se muestra un guión. */
function efectividad(record) {
  return record.partidosJugados === 0 ? SIN_DATO : `${record.efectividad}%`;
}

function celdasDeRecord(r) {
  return `
    <td>${r.partidosJugados}</td>
    <td>${r.ganados}</td>
    <td>${r.empatados}</td>
    <td>${r.perdidos}</td>
    <td>${r.golesAFavor}</td>
    <td>${r.golesEnContra}</td>
    <td>${r.diferenciaDeGol > 0 ? "+" : ""}${r.diferenciaDeGol}</td>
    <td class="efec">${efectividad(r)}</td>`;
}

/* ---------- récord global ---------- */

function pintarGlobal(r) {
  const tarjetas = [
    ["Partidos", r.partidosJugados],
    ["G - E - P", `${r.ganados} - ${r.empatados} - ${r.perdidos}`],
    ["Goles", `${r.golesAFavor} : ${r.golesEnContra}`],
    ["Diferencia", `${r.diferenciaDeGol > 0 ? "+" : ""}${r.diferenciaDeGol}`],
    ["Efectividad", efectividad(r)],
    ["Promedio GF", r.partidosJugados === 0 ? SIN_DATO : r.promedioGolesAFavor],
    ["Promedio GC", r.partidosJugados === 0 ? SIN_DATO : r.promedioGolesEnContra]
  ];

  document.getElementById("global").innerHTML = tarjetas
    .map(([rotulo, valor]) => `
      <div class="tarjetita">
        <span class="rotulo">${rotulo}</span>
        <span class="valor">${valor}</span>
      </div>`)
    .join("");
}

/* ---------- modalidad y veredicto ---------- */

function pintarModalidad(resumen) {
  document.querySelector("#tabla-modalidad tbody").innerHTML = resumen.porModalidad
    .map((m) => `
      <tr class="${m.record.partidosJugados === 0 ? "apagada" : ""}">
        <td>${NOMBRE_MODALIDAD[m.modalidad] ?? m.modalidad}</td>
        ${celdasDeRecord(m.record)}
      </tr>`)
    .join("");

  const tarjeta = document.getElementById("tarjeta-veredicto");
  tarjeta.dataset.veredicto = resumen.veredicto.toLowerCase();

  const titulos = { Cabala: "Sos cábala", Yeta: "Sos yeta", Indefinido: "Todavía no se sabe" };
  texto("dictamen", titulos[resumen.veredicto] ?? resumen.veredicto);
  texto("explicacion", resumen.explicacion);

  const cancha = resumen.enCancha;
  const otro = resumen.porOtroMedio;

  document.getElementById("comparacion").hidden = false;
  texto("efec-cancha", efectividad(cancha));
  texto("efec-otro", efectividad(otro));
  texto("detalle-cancha", `${cancha.partidosJugados} PJ · ${cancha.ganados}-${cancha.empatados}-${cancha.perdidos}`);
  texto("detalle-otro", `${otro.partidosJugados} PJ · ${otro.ganados}-${otro.empatados}-${otro.perdidos}`);
}

/* ---------- rachas ---------- */

function describirRacha(racha) {
  if (racha.longitud === 0) return SIN_DATO;
  const partidos = racha.longitud === 1 ? "1 partido" : `${racha.longitud} partidos`;
  return racha.enCurso ? `${partidos} (en curso)` : partidos;
}

function pintarRachas(resumen) {
  const filas = Object.entries(NOMBRE_RACHA).map(([clave, nombre]) => {
    const { actual, masLarga } = resumen[clave];
    const cuando = masLarga.longitud === 0
      ? SIN_DATO
      : `${formatearFecha(masLarga.desde)} → ${formatearFecha(masLarga.hasta)}`;

    return `
      <tr>
        <td>${nombre}</td>
        <td class="${actual.longitud > 0 ? "resaltado" : ""}">${describirRacha(actual)}</td>
        <td>${describirRacha(masLarga)}</td>
        <td class="tenue">${cuando}</td>
      </tr>`;
  });

  document.querySelector("#tabla-rachas tbody").innerHTML = filas.join("");
}

/* ---------- rivales ---------- */

function tarjetaDeRival(titulo, clase, rival) {
  if (!rival) return "";

  return `
    <div class="destacado ${clase}">
      <span class="rotulo">${titulo}</span>
      <span class="nombre">${escapar(rival.rival)}</span>
      <span class="detalle-chico">
        ${rival.record.partidosJugados} PJ ·
        ${rival.record.ganados}-${rival.record.empatados}-${rival.record.perdidos} ·
        ${efectividad(rival.record)}
      </span>
    </div>`;
}

function pintarRivales(resumen) {
  texto("umbral", `mínimo ${resumen.umbralAplicado} PJ`);

  document.getElementById("destacados").innerHTML =
    tarjetaDeRival("Talismán", "talisman", resumen.talisman) +
    tarjetaDeRival("Maldición", "maldicion", resumen.maldicion);

  const tabla = document.getElementById("tabla-rivales");
  const aviso = document.getElementById("sin-rivales");

  if (resumen.ranking.length === 0) {
    tabla.hidden = true;
    aviso.hidden = false;
    aviso.textContent =
      `Todavía no hay ningún rival con al menos ${resumen.umbralAplicado} partidos jugados.`;
    return;
  }

  tabla.hidden = false;
  aviso.hidden = true;
  document.querySelector("#tabla-rivales tbody").innerHTML = resumen.ranking
    .map((r) => `<tr><td class="rival">${escapar(r.rival)}</td>${celdasDeRecord(r.record)}</tr>`)
    .join("");
}

/* ---------- arranque ---------- */

async function cargarEstadisticas() {
  const aviso = document.getElementById("error-estadisticas");

  try {
    const [global, modalidad, rachas, rivales] = await Promise.all([
      API.estadisticasGlobal(),
      API.estadisticasModalidad(),
      API.estadisticasRachas(),
      API.estadisticasRivales()
    ]);

    aviso.hidden = true;
    pintarGlobal(global);
    pintarModalidad(modalidad);
    pintarRachas(rachas);
    pintarRivales(rivales);
  } catch (error) {
    aviso.hidden = false;
    aviso.textContent = error.message;
  }
}

document.addEventListener("DOMContentLoaded", cargarEstadisticas);
