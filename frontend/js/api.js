/*
  Único módulo que conoce la URL del backend. El resto del frontend
  habla con la API a través de acá.
*/
class ErrorDeApi extends Error {
  constructor(mensaje, regla) {
    super(mensaje);
    this.name = "ErrorDeApi";
    this.regla = regla ?? "desconocida";
  }
}

const API = (() => {
  const config = window.APP_CONFIG;

  if (!config || !config.apiBaseUrl) {
    throw new Error(
      "Falta la configuración del frontend. Copiá js/config.example.js a js/config.js."
    );
  }

  async function pedir(ruta, opciones = {}) {
    let respuesta;
    try {
      respuesta = await fetch(config.apiBaseUrl + ruta, {
        headers: { "Content-Type": "application/json" },
        ...opciones
      });
    } catch {
      // Backend apagado, CORS bloqueado o red caída: un solo error para el llamador.
      throw new ErrorDeApi("No se pudo contactar al backend.", "sin-conexion");
    }

    const cuerpo = respuesta.status === 204 ? null : await respuesta.json().catch(() => null);

    if (!respuesta.ok) {
      throw new ErrorDeApi(
        cuerpo?.error ?? `El backend respondió ${respuesta.status}.`,
        cuerpo?.regla
      );
    }

    return cuerpo;
  }

  return {
    health: () => pedir("/health"),

    listarPartidos: () => pedir("/partidos"),

    listarEquipos: () => pedir("/equipos"),

    crearPartido: (partido) => pedir("/partidos", {
      method: "POST",
      body: JSON.stringify(partido)
    }),

    actualizarPartido: (id, partido) => pedir(`/partidos/${id}`, {
      method: "PUT",
      body: JSON.stringify(partido)
    }),

    eliminarPartido: (id) => pedir(`/partidos/${id}`, { method: "DELETE" }),

    estadisticasGlobal: () => pedir("/estadisticas/global"),
    estadisticasModalidad: () => pedir("/estadisticas/modalidad"),
    estadisticasRachas: () => pedir("/estadisticas/rachas"),
    estadisticasRivales: () => pedir("/estadisticas/rivales"),
    estadisticasDesgloses: () => pedir("/estadisticas/desgloses")
  };
})();
