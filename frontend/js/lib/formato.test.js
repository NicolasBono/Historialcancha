import { describe, expect, it } from 'vitest'
import {
  SIN_DATO,
  clasificarMarcador,
  conSigno,
  contarPartidos,
  describirRacha,
  efectividad,
  formatearFecha
} from './formato.js'

describe('formatearFecha', () => {
  it.each([
    ['un día de un dígito', '2026-03-08', '08/03/2026'],
    ['fin de año', '2025-12-31', '31/12/2025'],
    ['primero de enero', '2026-01-01', '01/01/2026']
  ])('pasa %s de ISO a dd/mm/aaaa', (_caso, iso, esperado) => {
    expect(formatearFecha(iso)).toBe(esperado)
  })

  it.each([
    ['nulo', null],
    ['indefinido', undefined],
    ['cadena vacía', '']
  ])('devuelve el guión si la fecha es %s', (_caso, entrada) => {
    // Este es el caso que antes rompía: partidos.js tenía su propia copia de
    // formatearFecha sin esta guarda, y con un nulo explotaba en pantalla.
    expect(formatearFecha(entrada)).toBe(SIN_DATO)
  })
})

describe('efectividad', () => {
  it('muestra el porcentaje con su signo de porciento', () => {
    expect(efectividad({ partidosJugados: 10, efectividad: 70 })).toBe('70%')
  })

  it('con cero partidos jugados muestra el guión, no 0%', () => {
    // No es lo mismo "no ganaste nunca" que "todavía no jugaste": un 0% le diría
    // al hincha recién registrado que es un desastre.
    expect(efectividad({ partidosJugados: 0, efectividad: 0 })).toBe(SIN_DATO)
  })
})

describe('describirRacha', () => {
  it('una racha de longitud cero no existe: va el guión', () => {
    expect(describirRacha({ longitud: 0, enCurso: false })).toBe(SIN_DATO)
  })

  it.each([
    [1, false, '1 partido'],
    [2, false, '2 partidos'],
    [7, false, '7 partidos']
  ])('con longitud %i dice "%s"', (longitud, enCurso, esperado) => {
    // El singular en 1: "1 partidos" se lee mal y es la clase de detalle que
    // nadie mira hasta que está en producción.
    expect(describirRacha({ longitud, enCurso })).toBe(esperado)
  })

  it('aclara cuando la racha sigue viva', () => {
    expect(describirRacha({ longitud: 4, enCurso: true })).toBe('4 partidos (en curso)')
  })
})

describe('conSigno', () => {
  it.each([
    [3, '+3'],
    [-2, '-2'],
    [0, '0']
  ])('a %i le corresponde "%s"', (numero, esperado) => {
    // El + lo tiene que poner la función: una diferencia de gol positiva sin signo
    // no se distingue de una neutra de un vistazo.
    expect(conSigno(numero)).toBe(esperado)
  })
})

describe('contarPartidos', () => {
  it.each([
    [0, '0 partidos'],
    [1, '1 partido'],
    [12, '12 partidos']
  ])('con %i dice "%s"', (cantidad, esperado) => {
    expect(contarPartidos(cantidad)).toBe(esperado)
  })
})

describe('clasificarMarcador', () => {
  it.each([
    ['goleada', 5, 0],            // diferencia 5
    ['victoria', 2, 1],           // diferencia 1
    ['empate sin goles', 0, 0],   // el 0 a 0, que es su propio caso
    ['empate', 2, 2],             // empate con goles
    ['derrota', 1, 3],            // diferencia -2
    ['paliza', 0, 4]              // diferencia -4
  ])('un %s sale de %i a %i', (esperado, aFavor, enContra) => {
    expect(clasificarMarcador(aFavor, enContra)).toBe(esperado)
  })

  it.each([
    [3, 0, 'victoria'],   // tres de diferencia todavía NO es goleada
    [4, 0, 'goleada'],    // cuatro sí
    [0, 3, 'derrota'],    // tres en contra todavía NO es paliza
    [0, 4, 'paliza']      // cuatro sí
  ])('en el borde, %i a %i es "%s"', (aFavor, enContra, esperado) => {
    // Los cuatro bordes de las dos reglas con umbral. Si alguien cambia un >= por
    // un >, estos son los únicos tests que se ponen rojos.
    expect(clasificarMarcador(aFavor, enContra)).toBe(esperado)
  })

  it('el 0 a 0 no se confunde con cualquier otro empate', () => {
    // Las dos ramas devuelven cosas distintas y la condición que las separa mira
    // los goles, no la diferencia: sin este par, el orden de los if daría igual.
    expect(clasificarMarcador(0, 0)).not.toBe(clasificarMarcador(1, 1))
  })
})
