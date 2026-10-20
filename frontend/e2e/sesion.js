import { expect } from '@playwright/test'

// La api, no el front: son dos servicios con dos direcciones. Sin barra al final.
export const API = process.env.API_BASE_URL || 'http://localhost:8080'

// Un usuario de prueba FIJO por entorno, no uno nuevo por corrida: la app no tiene
// forma de borrar usuarios, así que crear uno en cada corrida llenaría QA de cuentas
// que nadie usa. Los tests sólo crean partidos de este usuario, y los borran.
// En el pipeline las credenciales llegan por secrets; los defaults son para el compose.
export const USUARIO = {
  dni: process.env.E2E_DNI || '90000001',
  contrasena: process.env.E2E_CONTRASENA || 'pruebas-locales-1',
  nombre: 'Pruebas',
  apellido: 'Automaticas',
}

// Devuelve un token del usuario de prueba. Si todavía no existe en este entorno
// (la primera corrida contra una base nueva), lo registra: el alta ya devuelve token.
export async function obtenerToken (request) {
  const login = await request.post(`${API}/api/auth/login`, {
    data: { dni: USUARIO.dni, contrasena: USUARIO.contrasena },
  })
  if (login.status() === 200) return (await login.json()).token

  expect(login.status(), 'el login falló por algo que no es «no existe»').toBe(401)
  const alta = await request.post(`${API}/api/auth/registro`, { data: USUARIO })
  expect(alta.status(), await alta.text()).toBe(201)
  return (await alta.json()).token
}

// Una fecha pasada que no choca con otra corrida: la app permite un solo partido por
// día, así que cada test usa un día distinto, sacado de la hora. `desplazamiento`
// separa los días de dos partidos creados en el mismo test.
export function fechaUnica (desplazamiento = 0) {
  const diasAtras = 365 + (Math.floor(Date.now() / 1000) % 20000) + desplazamiento
  const fecha = new Date(Date.now() - diasAtras * 86_400_000)
  return fecha.toISOString().slice(0, 10)
}

// Un partido válido con un rival que no se repite: el nombre lleva la hora adentro,
// así ningún otro test ni otra corrida usa el mismo.
export function partidoValido (rival, fecha) {
  return {
    fecha,
    rival,
    torneo: 'Pruebas automáticas',
    condicion: 'Local',
    golesAFavor: 2,
    golesEnContra: 1,
    modalidad: 'TV',
  }
}
