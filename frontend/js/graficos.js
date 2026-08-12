/*
  Dibujo de gráficos, en HTML y CSS: sin librerías ni build step (NFR1).

  Acá tampoco se calcula nada. Todo lo que se grafica —efectividades, récords,
  resultados— ya vino resuelto del backend; este módulo sólo traduce números a
  anchos de barra.
*/
const VIZ = (() => {
  const LETRA_RESULTADO = { Victoria: "V", Empate: "E", Derrota: "D" };
  const NOMBRE_RESULTADO = { Victoria: "Victoria", Empate: "Empate", Derrota: "Derrota" };

  function escapar(valor) {
    const div = document.createElement("div");
    div.textContent = valor ?? "";
    return div.innerHTML;
  }

  /**
   * Barras horizontales para comparar magnitudes.
   *
   * Una sola serie y un solo color a propósito: las modalidades, los rivales y los
   * torneos no tienen un orden natural, así que darle a cada uno su color pintaría
   * de nuevo lo que el largo de la barra ya dice.
   *
   * `tope` fija la escala. Para porcentajes es 100 siempre: escalarlos al máximo
   * observado haría que un 40% se vea lleno y mienta.
   */
  function barras(items, tope) {
    const maximo = tope ?? Math.max(1, ...items.map((i) => i.valor));

    return items
      .map((i) => {
        // Cero se dibuja como nada. Un valor chico pero real se lleva un mínimo
        // visible, para que no desaparezca y se confunda con el cero.
        const crudo = (i.valor / maximo) * 100;
        const ancho = i.vacio || i.valor <= 0 ? 0 : Math.max(1.2, Math.min(100, crudo));

        return `
          <div class="barra-fila ${i.vacio ? "apagada" : ""}" title="${escapar(i.detalle ?? "")}">
            <span class="barra-etiqueta">${escapar(i.etiqueta)}</span>
            <div class="barra-pista">
              <div class="barra-fill" style="width:${ancho}%"></div>
            </div>
            <span class="barra-valor">${escapar(i.texto)}</span>
          </div>`;
      })
      .join("");
  }

  /**
   * Reparto de ganados / empatados / perdidos como una sola barra.
   * Siempre sale con leyenda: el color no puede ser el único canal que distinga
   * una victoria de una derrota.
   */
  function reparto(record) {
    if (record.partidosJugados === 0) return "";

    const total = record.partidosJugados;
    const partes = [
      ["Victoria", "Ganados", record.ganados],
      ["Empate", "Empatados", record.empatados],
      ["Derrota", "Perdidos", record.perdidos]
    ];

    const segmentos = partes
      .filter(([, , cantidad]) => cantidad > 0)
      .map(([tipo, rotulo, cantidad]) => `
        <div class="seg" data-resultado="${tipo}" style="width:${(cantidad / total) * 100}%"
             title="${rotulo}: ${cantidad} de ${total}"></div>`)
      .join("");

    const leyenda = partes
      .map(([tipo, rotulo, cantidad]) =>
        `<span><i data-resultado="${tipo}"></i>${rotulo} ${cantidad}</span>`)
      .join("");

    return `<div class="apilada">${segmentos}</div><div class="leyenda">${leyenda}</div>`;
  }

  /**
   * Los últimos partidos, del más viejo al más nuevo. Cada casilla lleva su letra
   * además del color, que es lo que la hace leíble con daltonismo rojo-verde.
   */
  function forma(partidos) {
    if (partidos.length === 0) return "";

    const casillas = partidos
      .map((p) => `
        <span class="forma-casilla resultado" data-resultado="${p.resultado}"
              title="${escapar(p.fecha)} · ${NOMBRE_RESULTADO[p.resultado]} ${p.golesAFavor}-${p.golesEnContra} vs ${escapar(p.rival)}">
          ${LETRA_RESULTADO[p.resultado] ?? "?"}
        </span>`)
      .join("");

    return `<div class="forma">${casillas}</div>`;
  }

  return { barras, reparto, forma };
})();
