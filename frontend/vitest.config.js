import { defineConfig } from 'vitest/config'

export default defineConfig({
  test: {
    coverage: {
      provider: 'v8',

      // `json-summary` no es decorativo: deja un coverage-summary.json con los
      // totales, y es el archivo que el pipeline lee para armar la tabla del
      // resumen de la corrida. `lcov` y `html` son el reporte navegable.
      reporter: ['text', 'html', 'lcov', 'json-summary'],

      // Dónde cae el reporte. Sale de una variable de entorno en vez de una
      // expansión de shell en el script de npm: así el mismo comando sirve en
      // Windows y adentro del contenedor Linux, que es donde el pipeline lo
      // necesita apuntado a la carpeta montada.
      reportsDirectory: process.env.COVERAGE_DIR || 'coverage',

      // Qué SÍ entra en la cuenta. Sin esta línea, desde vitest 4 se mide sólo lo
      // que los tests importan: un archivo nuevo sin tests no aparecería y el
      // número SUBIRÍA, dejando el umbral ciego justo para el código sin cubrir.
      // Con include, todo lo de js/lib entra aunque nadie lo pruebe — y por eso
      // dom.js figura en 0% en vez de desaparecer.
      include: ['js/lib/**'],

      // El umbral. Si la cobertura cae por debajo, vitest sale con error aunque
      // todos los tests estén en verde, el docker run devuelve ese error, el job
      // falla y el required check bloquea el merge.
      thresholds: { lines: 80, branches: 80 }
    }
  }
})
