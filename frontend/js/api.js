/*
  Único módulo que conoce la URL del backend. El resto del frontend
  habla con la API a través de acá. Adjunta el token de sesión en cada
  request y, si el backend responde 401, cierra la sesión y manda al login.
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
    const cabeceras = { "Content-Type": "application/json", ...(opciones.headers || {}) };

    // El token viaja en cada request; el backend saca de ahí de quién son los datos.
    const token = Auth.token();
    if (token) cabeceras["Authorization"] = "Bearer " + token;

    let respuesta;
    try {
      respuesta = await fetch(config.apiBaseUrl + ruta, { ...opciones, headers: cabeceras });
    } catch {
      // Backend apagado, CORS bloqueado o red caída: un solo error para el llamador.
      throw new ErrorDeApi("No se pudo contactar al backend.", "sin-conexion");
    }

    // 401 en una ruta protegida = token vencido o inválido: a login.
    // En /auth/* un 401 es "credenciales incorrectas" y lo maneja quien llamó.
    if (respuesta.status === 401 && !ruta.startsWith("/auth/")) {
      Auth.cerrarSesion();
      throw new ErrorDeApi("Tu sesión expiró. Ingresá de nuevo.", "sesion-expirada");
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

    registro: (datos) => pedir("/auth/registro", {
      method: "POST",
      body: JSON.stringify(datos)
    }),

    login: (datos) => pedir("/auth/login", {
      method: "POST",
      body: JSON.stringify(datos)
    }),

    listarPartidos: () => pedir("/partidos"),

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
    estadisticasRivales: () => pedir("/estadisticas/rivales")
  };
})();
