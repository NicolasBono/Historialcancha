import { describe, expect, it, vi } from 'vitest'
import { buscarPartidoPorId } from './partidos-datos.js'

/*
  El doble reemplaza al lector del historial. En la app real entra
  API.listarPartidos, que sale a la red; acá entra una función impostora, así que
  estos tests corren en milisegundos y no fallan los días que el backend está caído.

  En los primeros tests el doble actúa de STUB —sólo contesta lo que se le pidió
  que conteste— y en el último de MOCK: ahí el assert no mira lo devuelto, mira
  cómo se usó la dependencia. Es el mismo objeto; lo que cambia es el assert.
*/
const HISTORIAL = [
  { id: 1, rival: 'River Plate', golesAFavor: 2, golesEnContra: 1 },
  { id: 7, rival: 'Racing', golesAFavor: 0, golesEnContra: 0 },
  { id: 12, rival: 'Independiente', golesAFavor: 1, golesEnContra: 3 }
]

describe('buscarPartidoPorId', () => {
  it('devuelve el partido que se le pidió', async () => {
    // Arrange
    const listar = vi.fn().mockResolvedValue(HISTORIAL)

    // Act
    const partido = await buscarPartidoPorId(7, listar)

    // Assert
    expect(partido).toMatchObject({ rival: 'Racing' })
  })

  it('encuentra el partido aunque el id llegue como texto', async () => {
    // Arrange — el id viene de un data-attribute del DOM, y de ahí siempre sale
    // string. Si la comparación dejara de convertirlo, el botón de editar no
    // encontraría nada.
    const listar = vi.fn().mockResolvedValue(HISTORIAL)

    // Act
    const partido = await buscarPartidoPorId('12', listar)

    // Assert
    expect(partido).toMatchObject({ rival: 'Independiente' })
  })

  it('devuelve null si ese id no está en el historial', async () => {
    // Arrange
    const listar = vi.fn().mockResolvedValue(HISTORIAL)

    // Act
    const partido = await buscarPartidoPorId(999, listar)

    // Assert — null y no undefined es una decisión: quien llama pregunta por
    // "no está", no por "no me contestaron".
    expect(partido).toBeNull()
  })

  it('devuelve null con el historial vacío, sin romperse', async () => {
    // Arrange
    const listar = vi.fn().mockResolvedValue([])

    // Act
    const partido = await buscarPartidoPorId(1, listar)

    // Assert
    expect(partido).toBeNull()
  })

  it('le pide el historial al lector exactamente una vez', async () => {
    // Arrange
    const listar = vi.fn().mockResolvedValue(HISTORIAL)

    // Act
    await buscarPartidoPorId(7, listar)

    // Assert — acá el doble actúa de MOCK. Si mañana alguien pide el historial dos
    // veces por descuido, la pantalla hace dos viajes a la red y esto se pone rojo.
    expect(listar).toHaveBeenCalledTimes(1)
  })
})
