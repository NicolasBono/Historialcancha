import { expect, test } from '@playwright/test'
import { USUARIO, fechaUnica, obtenerToken } from './sesion.js'

// Pruebas END-TO-END: un navegador de verdad usando el front desplegado, que le habla
// a su api y a su base. Nada de interceptar pedidos ni de inventar respuestas.

// El usuario de prueba tiene que existir antes de loguearse por pantalla.
test.beforeAll(async ({ request }) => {
  await obtenerToken(request)
})

// Cada test entra como lo hace una persona: por la pantalla de login.
test.beforeEach(async ({ page }) => {
  await page.goto('/login.html')
  // El login y el registro tienen campos con el mismo nombre: se busca dentro del
  // formulario de login. «Ingresar» también es el nombre de la pestaña.
  const login = page.locator('#form-login')
  await login.getByLabel('DNI').fill(USUARIO.dni)
  await login.getByLabel('Contraseña').fill(USUARIO.contrasena)
  await login.getByRole('button', { name: 'Ingresar' }).click()
  await expect(page.getByRole('heading', { name: 'Mi historial', exact: true })).toBeVisible()
  // El título está en el HTML y aparece antes de que cargue el JavaScript. Si el test
  // toca «Guardar» antes, el navegador manda el formulario solo, recarga la página y
  // no se guarda nada (pasó contra Render, que es más lento que el compose local).
  // El contador lo pinta el script cuando ya enganchó el formulario: se espera eso.
  await expect(page.locator('#contador')).toHaveText(/\d+ partidos?/)
  // El borrado pide confirmación con confirm(): se acepta como lo haría la persona.
  page.on('dialog', dialogo => dialogo.accept())
})

async function cargarPartido (page, rival, fecha) {
  await page.getByLabel('Fecha').fill(fecha)
  await page.getByLabel('Rival').fill(rival)
  await page.getByLabel('Torneo').fill('Pruebas automáticas')
  await page.getByRole('button', { name: 'Guardar' }).click()
}

const filaDe = (page, rival) => page.getByRole('row').filter({ hasText: rival })

test('cargar un partido lo muestra en el historial, y borrarlo lo saca', async ({ page }) => {
  const rival = `e2e ${Date.now()}`
  await cargarPartido(page, rival, fechaUnica(2))
  await expect(filaDe(page, rival)).toBeVisible()            // el dato que ESTE test creó

  await page.getByRole('button', { name: `Borrar partido contra ${rival}` }).click()
  await expect(filaDe(page, rival)).toHaveCount(0)           // y el borrado, comprobado
})

test('un partido con fecha futura muestra el error y no se carga', async ({ page }) => {
  const rival = `e2e ${Date.now()}`
  const manana = new Date(Date.now() + 86_400_000).toISOString().slice(0, 10)
  // El input de fecha no ofrece días futuros, pero se pueden escribir: la regla real
  // la aplica la api, y el usuario tiene que ver lo que contestó.
  await cargarPartido(page, rival, manana)

  await expect(page.getByRole('alert')).toContainText('no puede ser posterior a hoy')
  await expect(filaDe(page, rival)).toHaveCount(0)           // y no apareció nada
})

// Lo que el hincha hace todos los días: cargar el partido y volver a verlo después.
// Si al recargar no está, la app no guarda nada, aunque la pantalla lo haya mostrado.
test('un partido cargado sigue en el historial al volver a entrar', async ({ page }) => {
  const rival = `e2e ${Date.now()}`
  await cargarPartido(page, rival, fechaUnica(3))
  await expect(filaDe(page, rival)).toBeVisible()

  await page.reload()
  await expect(filaDe(page, rival)).toBeVisible()            // lo devolvió la base

  await page.getByRole('button', { name: `Borrar partido contra ${rival}` }).click()
  await expect(filaDe(page, rival)).toHaveCount(0)
})
