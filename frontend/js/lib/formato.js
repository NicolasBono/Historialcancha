/*
  Lógica de presentación pura: recibe valores y devuelve valores. No toca el DOM,
  no habla con la API y no lee configuración, así que se puede verificar con tests
  unitarios sin navegador.

  Antes estas piezas vivían duplicadas en estadisticas.js y partidos.js. Unificarlas
  destapó que las dos copias de formatearFecha no se comportaban igual: la de
  estadisticas.js manejaba el nulo y la de partidos.js explotaba. Quedó la primera,
  que es la que no rompe.
*/

/** Lo que se muestra cuando un dato no existe o no significa nada todavía. */
export const SIN_DATO = "—";

/** Nombre visible de cada modalidad. La clave es la del backend. */
export const NOMBRE_MODALIDAD = {
  EnCancha: "En cancha",
  TV: "TV",
  Streaming: "Streaming",
  Radio: "Radio",
  NoLoVi: "No lo vi"
};

/** Nombre visible de cada tipo de racha. La clave es la del backend. */
export const NOMBRE_RACHA = {
  invicto: "Invicto",
  sinGanar: "Sin ganar",
  sinRecibirGoles: "Sin recibir goles"
};

/** Letra con la que se muestra cada resultado en la tabla de partidos. */
export const RESULTADOS = { Victoria: "V", Empate: "E", Derrota: "D" };

/**
 * Pasa una fecha ISO (aaaa-mm-dd) al formato dd/mm/aaaa.
 * Sin fecha devuelve el guión: no hay nada que formatear.
 */
export function formatearFecha(iso) {
  if (!iso) return SIN_DATO;
  const [anio, mes, dia] = iso.split("-");
  return `${dia}/${mes}/${anio}`;
}

/**
 * Efectividad de un récord, lista para mostrar.
 * Con cero partidos jugados el porcentaje no significa nada: va el guión.
 */
export function efectividad(record) {
  return record.partidosJugados === 0 ? SIN_DATO : `${record.efectividad}%`;
}

/**
 * Describe una racha en palabras: cuántos partidos duró y si sigue viva.
 * Una racha de longitud cero no existe, así que no se describe.
 */
export function describirRacha(racha) {
  if (racha.longitud === 0) return SIN_DATO;
  const partidos = racha.longitud === 1 ? "1 partido" : `${racha.longitud} partidos`;
  return racha.enCurso ? `${partidos} (en curso)` : partidos;
}

/** Diferencia de gol con su signo: el + no lo pone el número solo. */
export function conSigno(numero) {
  return numero > 0 ? `+${numero}` : String(numero);
}

/** El contador del listado, en singular o plural según corresponda. */
export function contarPartidos(cantidad) {
  return cantidad === 1 ? "1 partido" : `${cantidad} partidos`;
}

/**
 * Clasifica un marcador para poder destacarlo en el listado: no es lo mismo
 * ganar 1 a 0 que meter una goleada, y hoy la tabla los muestra igual.
 * @param {number} aFavor
 * @param {number} enContra
 * @returns {string} la etiqueta que va en la celda del marcador
 */
export function clasificarMarcador(aFavor, enContra) {
  const diferencia = aFavor - enContra;

  if (diferencia >= 4) return 'goleada';
  if (diferencia > 0) return 'victoria';
  if (diferencia === 0 && aFavor === 0) return 'empate sin goles';
  if (diferencia === 0) return 'empate';
  if (diferencia <= -4) return 'paliza';
  return 'derrota';
}
