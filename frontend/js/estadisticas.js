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

/* El backend manda el mes de corte como número; el chip lo muestra con nombre. */
const NOMBRE_MES = [
  "", "enero", "febrero", "marzo", "abril", "mayo", "junio",
  "julio", "agosto", "septiembre", "octubre", "noviembre", "diciembre"
];

const SIN_DATO = "—";

/* Cuántos partidos entran en la tira de forma reciente. */
const MAX_FORMA = 20;

/* Las efectividades siempre se grafican sobre 100: escalarlas al máximo
   observado haría que la peor modalidad se vea llena. */
const ESCALA_EFECTIVIDAD = 100;

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

  document.getElementById("reparto-global").innerHTML = VIZ.reparto(r);
}

/**
 * Los últimos partidos. Se invierten para leerlos como se leen las rachas:
 * de izquierda a derecha, del más viejo al más nuevo.
 */
function pintarForma(partidos) {
  const aviso = document.getElementById("sin-forma");
  const ultimos = partidos.slice(0, MAX_FORMA).reverse();

  document.getElementById("forma-reciente").innerHTML = VIZ.forma(ultimos);
  aviso.hidden = ultimos.length > 0;
  if (ultimos.length === 0) aviso.textContent = "Todavía no cargaste ningún partido.";
}

/** Una fila por grupo, con la efectividad sobre una escala fija de 0 a 100. */
function filaDeEfectividad(etiqueta, record) {
  return {
    etiqueta,
    valor: record.efectividad,
    texto: efectividad(record),
    vacio: record.partidosJugados === 0,
    detalle: record.partidosJugados === 0
      ? `${etiqueta}: sin partidos`
      : `${etiqueta}: ${record.partidosJugados} PJ · ` +
        `${record.ganados}-${record.empatados}-${record.perdidos} · ${record.efectividad}% de efectividad`
  };
}

/* ---------- modalidad y veredicto ---------- */

function pintarModalidad(resumen) {
  document.getElementById("viz-modalidad").innerHTML = VIZ.barras(
    resumen.porModalidad.map((m) =>
      filaDeEfectividad(NOMBRE_MODALIDAD[m.modalidad] ?? m.modalidad, m.record)),
    ESCALA_EFECTIVIDAD
  );

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

  document.getElementById("viz-rivales").innerHTML = VIZ.barras(
    resumen.ranking.map((r) => filaDeEfectividad(r.rival, r.record)),
    ESCALA_EFECTIVIDAD
  );

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

/* ---------- desgloses ---------- */

/**
 * Un corte que no tiene ninguna fila no es un error: es un historial vacío.
 * Se esconde la tabla y se explica, igual que con los rivales.
 */
function pintarCorte(idTabla, idAviso, filas, mensajeVacio) {
  const tabla = document.getElementById(idTabla);
  const aviso = document.getElementById(idAviso);

  if (filas.length === 0) {
    tabla.hidden = true;
    aviso.hidden = false;
    aviso.textContent = mensajeVacio;
    return;
  }

  tabla.hidden = false;
  aviso.hidden = true;
  tabla.querySelector("tbody").innerHTML = filas.join("");
}

function pintarDesgloses(resumen) {
  texto("corte-temporada", `temporada desde ${NOMBRE_MES[resumen.mesDeCorteAplicado]}`);

  document.getElementById("viz-condicion").innerHTML = VIZ.barras(
    resumen.porCondicion.map((c) => filaDeEfectividad(c.condicion, c.record)),
    ESCALA_EFECTIVIDAD
  );

  document.getElementById("viz-torneo").innerHTML = VIZ.barras(
    resumen.porTorneo.map((t) => filaDeEfectividad(t.torneo, t.record)),
    ESCALA_EFECTIVIDAD
  );

  document.getElementById("viz-temporada").innerHTML = VIZ.barras(
    resumen.porTemporada.map((t) => filaDeEfectividad(t.temporada, t.record)),
    ESCALA_EFECTIVIDAD
  );

  // Local y Visitante vienen siempre, aunque estén en cero: no se esconde ninguno.
  document.querySelector("#tabla-condicion tbody").innerHTML = resumen.porCondicion
    .map((c) => `
      <tr class="${c.record.partidosJugados === 0 ? "apagada" : ""}">
        <td>${c.condicion}</td>
        ${celdasDeRecord(c.record)}
      </tr>`)
    .join("");

  pintarCorte(
    "tabla-torneo",
    "sin-torneos",
    resumen.porTorneo.map(
      (t) => `<tr><td class="clave">${escapar(t.torneo)}</td>${celdasDeRecord(t.record)}</tr>`
    ),
    "Todavía no hay partidos cargados, así que no hay torneos para desglosar."
  );

  pintarCorte(
    "tabla-temporada",
    "sin-temporadas",
    resumen.porTemporada.map(
      (t) => `<tr><td class="clave">${escapar(t.temporada)}</td>${celdasDeRecord(t.record)}</tr>`
    ),
    "Todavía no hay partidos cargados, así que no hay temporadas para desglosar."
  );
}

/* ---------- arranque ---------- */

async function cargarEstadisticas() {
  const aviso = document.getElementById("error-estadisticas");

  try {
    // Los partidos se piden para la tira de forma: se usa el resultado que ya
    // resolvió el backend, no se deduce de los goles acá.
    const [global, modalidad, rachas, rivales, desgloses, partidos] = await Promise.all([
      API.estadisticasGlobal(),
      API.estadisticasModalidad(),
      API.estadisticasRachas(),
      API.estadisticasRivales(),
      API.estadisticasDesgloses(),
      API.listarPartidos()
    ]);

    aviso.hidden = true;
    pintarGlobal(global);
    pintarForma(partidos);
    pintarModalidad(modalidad);
    pintarRachas(rachas);
    pintarRivales(rivales);
    pintarDesgloses(desgloses);
  } catch (error) {
    aviso.hidden = false;
    aviso.textContent = error.message;
  }
}

document.addEventListener("DOMContentLoaded", cargarEstadisticas);
