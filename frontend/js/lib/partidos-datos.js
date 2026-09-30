/*
  Consultas sobre el historial de partidos, separadas de quién las pide y de
  quién las muestra.

  Ninguna función de acá sale a la red por su cuenta: reciben por parámetro el
  lector que las trae. En la app real entra API.listarPartidos; en los tests entra
  un doble. Es lo que permite verificar esta lógica sin backend levantado.
*/

/**
 * Busca un partido por su id en el historial del usuario.
 * @param {number} id - el partido buscado
 * @param {() => Promise<Array>} listar - trae el historial completo
 * @returns {Promise<object|null>} el partido, o null si ese id no está
 */
export async function buscarPartidoPorId(id, listar) {
  const partidos = await listar();
  return partidos.find((p) => p.id === Number(id)) ?? null;
}
