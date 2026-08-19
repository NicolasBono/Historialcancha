/*
  Pantalla de ingreso: alterna entre login y registro, y ante éxito guarda la
  sesión y entra al historial. Si ya hay sesión, no tiene sentido mostrar el login.
*/
document.addEventListener("DOMContentLoaded", () => {
  if (Auth.haySesion()) {
    location.href = "index.html";
    return;
  }

  const formLogin = document.getElementById("form-login");
  const formRegistro = document.getElementById("form-registro");
  const tabLogin = document.getElementById("tab-login");
  const tabRegistro = document.getElementById("tab-registro");
  const error = document.getElementById("error-auth");

  function mostrarError(mensaje) {
    error.textContent = mensaje;
    error.hidden = false;
  }

  function limpiarError() {
    error.hidden = true;
  }

  function activar(cual) {
    const esLogin = cual === "login";
    formLogin.hidden = !esLogin;
    formRegistro.hidden = esLogin;
    tabLogin.className = esLogin ? "primario" : "secundario";
    tabRegistro.className = esLogin ? "secundario" : "primario";
    limpiarError();
  }

  tabLogin.addEventListener("click", () => activar("login"));
  tabRegistro.addEventListener("click", () => activar("registro"));

  async function entrar(promesa) {
    limpiarError();
    try {
      const sesion = await promesa;
      Auth.guardarSesion(sesion);
      location.href = "index.html";
    } catch (e) {
      mostrarError(e.message);
    }
  }

  formLogin.addEventListener("submit", (ev) => {
    ev.preventDefault();
    entrar(API.login({
      dni: document.getElementById("login-dni").value,
      contrasena: document.getElementById("login-contrasena").value
    }));
  });

  formRegistro.addEventListener("submit", (ev) => {
    ev.preventDefault();
    entrar(API.registro({
      nombre: document.getElementById("reg-nombre").value,
      apellido: document.getElementById("reg-apellido").value,
      dni: document.getElementById("reg-dni").value,
      contrasena: document.getElementById("reg-contrasena").value
    }));
  });
});
