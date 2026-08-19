/*
  Sesión del hincha en el navegador. Guarda el token JWT y los datos de identidad,
  y expone el estado de la sesión al resto del frontend. Se carga ANTES que api.js,
  porque api.js le pide el token para cada request.
*/
const Auth = (() => {
  const CLAVE_TOKEN = "hc_token";
  const CLAVE_USUARIO = "hc_usuario";

  function guardarSesion(sesion) {
    localStorage.setItem(CLAVE_TOKEN, sesion.token);
    localStorage.setItem(CLAVE_USUARIO, JSON.stringify({
      nombre: sesion.nombre,
      apellido: sesion.apellido
    }));
  }

  function token() {
    return localStorage.getItem(CLAVE_TOKEN);
  }

  function usuario() {
    try {
      return JSON.parse(localStorage.getItem(CLAVE_USUARIO));
    } catch {
      return null;
    }
  }

  function haySesion() {
    return !!token();
  }

  function cerrarSesion() {
    localStorage.removeItem(CLAVE_TOKEN);
    localStorage.removeItem(CLAVE_USUARIO);
    location.href = "login.html";
  }

  /// Guarda de página: si no hay sesión, no se ve nada, se va al login.
  function exigirSesion() {
    if (!haySesion()) location.href = "login.html";
  }

  return { guardarSesion, token, usuario, haySesion, cerrarSesion, exigirSesion };
})();
