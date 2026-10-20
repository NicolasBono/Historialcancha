import { defineConfig } from '@playwright/test'

// Una sola instalación para las dos suites: e2e/api.spec.js (pedidos HTTP a la api,
// sin navegador) y e2e/partidos.spec.js (un navegador de verdad sobre el front).
// Cada job del pipeline corre su archivo por nombre.
export default defineConfig({
  testDir: './e2e',
  // Tope de cada test. Generoso por el free tier de Render: el smoke del job anterior
  // ya despertó QA, pero por internet un pedido suelto se puede demorar igual.
  timeout: 60_000,
  // Tope de cada aserción: no lo hereda del de arriba (el default es 5 s).
  expect: { timeout: 15_000 },
  use: {
    // El FRONT. La api tiene su propia dirección: API_BASE_URL, que leen los specs.
    baseURL: process.env.E2E_BASE_URL || 'http://localhost:3000',
    trace: 'on-first-retry',         // la traza se graba en el reintento de un fallo
    screenshot: 'only-on-failure',
  },
  // Un reintento absorbe una demora suelta; si un test pasa recién ahí, el reporte
  // lo marca «flaky» con la corrida en verde: por eso el reporte se mira igual.
  retries: 1,
  // Uno por vez: los tests comparten el usuario de prueba y la base de QA.
  workers: 1,
  reporter: [['html', { open: 'never' }], ['list']],
})
