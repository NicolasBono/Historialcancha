import { expect, test } from '@playwright/test'
import { API, fechaUnica, obtenerToken, partidoValido } from './sesion.js'

// Pruebas de INTEGRACIÓN: le hablan a la api desplegada con su base de verdad, sin
// navegador (el fixture `request` es un cliente HTTP) y sin ningún doble en el medio.
// Un unitario con un doble contesta lo que uno le dijo; acá contesta Postgres.

let token
test.beforeAll(async ({ request }) => {
  token = await obtenerToken(request)
})

const conToken = () => ({ Authorization: `Bearer ${token}` })

// Pide la lista de partidos del usuario de prueba y la devuelve ya convertida.
async function listar (request) {
  const r = await request.get(`${API}/api/partidos`, { headers: conToken() })
  expect(r.status()).toBe(200)
  return await r.json()
}

test('el alta guarda en la base de verdad, y el borrado lo saca', async ({ request }) => {
  const rival = `api ${Date.now()}`
  const alta = await request.post(`${API}/api/partidos`, {
    headers: conToken(), data: partidoValido(rival, fechaUnica()),
  })
  expect(alta.status(), await alta.text()).toBe(201)       // la api dice «creado»
  const creado = await alta.json()

  const lista = await listar(request)                      // y la BASE lo devuelve
  expect(lista.some(p => p.rival === rival)).toBe(true)

  const borrado = await request.delete(`${API}/api/partidos/${creado.id}`, { headers: conToken() })
  expect(borrado.status()).toBe(204)

  const despues = await listar(request)                    // y el borrado, comprobado
  expect(despues.some(p => p.rival === rival)).toBe(false)
})

test('un partido sin rival lo rechaza la api, y no crea nada', async ({ request }) => {
  const antes = await listar(request)

  const alta = await request.post(`${API}/api/partidos`, {
    headers: conToken(), data: partidoValido('', fechaUnica()),
  })
  expect(alta.status()).toBe(400)                          // el contrato del error
  expect((await alta.json()).regla).toBe('rival-requerido')

  const despues = await listar(request)
  expect(despues.length).toBe(antes.length)                // y de verdad no se creó nada
})

// La regla de «un partido por día» no la puede ver un unitario con doble: depende de
// lo que YA HAY en la base. Acá la base tiene el primer partido de verdad.
test('un segundo partido el mismo día lo rechaza la api, y queda uno solo', async ({ request }) => {
  const fecha = fechaUnica(1)
  const rival = `api ${Date.now()}`
  const primero = await request.post(`${API}/api/partidos`, {
    headers: conToken(), data: partidoValido(rival, fecha),
  })
  expect(primero.status(), await primero.text()).toBe(201)
  const creado = await primero.json()

  try {
    const segundo = await request.post(`${API}/api/partidos`, {
      headers: conToken(), data: partidoValido(`${rival} bis`, fecha),
    })
    expect(segundo.status()).toBe(400)
    expect((await segundo.json()).regla).toBe('fecha-duplicada')

    const lista = await listar(request)
    expect(lista.filter(p => p.fecha === fecha)).toHaveLength(1)
  } finally {
    // Se borra aunque una aserción haya fallado: QA lo comparten todas las corridas.
    const borrado = await request.delete(`${API}/api/partidos/${creado.id}`, { headers: conToken() })
    expect(borrado.status()).toBe(204)
  }
})

test('el health contesta HEAD, que es lo que usa el monitor externo', async ({ request }) => {
  const r = await request.head(`${API}/api/health`)
  expect(r.status()).toBe(200)                             // con 405 el monitor marca caída
})
