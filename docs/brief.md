# Brief — Mi historial de hincha

## Problema

El hincha acumula años de partidos vividos de formas muy distintas —en la cancha, por TV,
por radio, o directamente sin verlos— pero esa experiencia queda como anécdota suelta.
No tiene forma de responder la pregunta que siempre aparece en la sobremesa:
**¿soy cábala o soy yeta?** Las apps de fútbol existentes registran lo que hizo el equipo,
no lo que vivió la persona.

## Usuario

Un hincha único, dueño de su propio historial, que sigue a **un solo equipo**.
Carga los partidos a mano (los que le importan, no necesariamente todos) y quiere ver
la estadística de su experiencia personal. No hay cuentas, ni login, ni multiusuario.

## Propuesta de valor

Registrar cada partido junto con **cómo lo viví**, y que la app devuelva el veredicto:
comparar el récord del equipo cuando el hincha estuvo en la cancha contra el récord
cuando lo siguió por otro medio. Alrededor de ese núcleo, las estadísticas clásicas
(efectividad, rachas, rival talismán, promedios) pero siempre atravesadas por la
modalidad en que se vivió el partido.

## Alcance de la Fase 1

La aplicación completa corriendo en la notebook del desarrollador, con tres piezas
levantables en local:

- **Frontend** — servicio independiente en HTML, CSS y JavaScript puro. Sin frameworks,
  sin build step, sin npm. Se sirve por separado del backend.
- **Backend** — servicio independiente, Web API en C# / .NET Core, con CORS configurado
  y endpoint de health check. La lógica de negocio vive en una capa de dominio pura.
- **Base de datos** — SQL Server (MSSQL) local, con esquema versionado. Persistencia real:
  lo que se carga sobrevive al reinicio.

Funcionalmente, la Fase 1 entrega: alta / edición / baja / listado de partidos con su
registro personal, las validaciones de carga, y el set completo de estadísticas
(global, por modalidad, rachas, rivales, desgloses) más el veredicto cábala/yeta.

Configuración —connection string, URL del backend, nombre del equipo propio, entorno,
versión— resuelta por variables de entorno o archivo de configuración. Nada hardcodeado.
README con las instrucciones para levantar los tres servicios de cero.

## Qué queda FUERA de la Fase 1

**Explícitamente fuera de alcance, no se documenta ni se implementa ahora:**

- Tests de cualquier tipo (unitarios, integración, e2e), proyectos de testing, coverage.
- Análisis estático, linters obligatorios, quality gates.
- Pipelines de CI/CD, GitHub Actions, Azure DevOps, YAML de build o release.
- Entornos de QA o Producción, aprobaciones manuales, estrategias de deploy.
- Infraestructura cloud, contenedores, orquestación.

**Fuera por decisión de producto (mantener la app chica):**

- Autenticación, usuarios múltiples, roles.
- Más de un equipo propio por instalación.
- Importación automática de fixtures o resultados desde APIs externas.
- Carga de imágenes, entradas escaneadas, adjuntos.
- Notificaciones, calendario de próximos partidos, recordatorios.
- Exportación a PDF/Excel, reportes imprimibles.
- Diseño responsive avanzado, modo oscuro, animaciones, i18n.
- Paginación, búsqueda full-text, filtros combinados complejos.

> La Fase 2 (testing + pipeline de CI/CD para el TP Integrador de DevOps) existe y es
> el destino del proyecto, pero **no se diseña ni se documenta en esta fase**. La única
> concesión que se le hace ahora es estructural: la capa de dominio queda aislada.

## Supuestos

Decisiones tomadas por el camino ante puntos ambiguos, resueltas siempre por la opción
más simple:

1. **Un solo equipo propio**, definido por configuración (`App:MiEquipo`). Es el valor
   contra el que se valida que "el rival no puede ser el propio equipo".
2. **Un solo hincha por instalación**. Sin tabla de usuarios ni sesión.
3. **Comparación cábala/yeta**: se compara la modalidad `EnCancha` contra el conjunto de
   `TV + Streaming + Radio`. Los partidos con modalidad `NoLoVi` cuentan para el récord
   global pero se excluyen de la comparación, porque no hubo experiencia que comparar.
4. **Veredicto**: se emite sólo con al menos 5 partidos en cada grupo y una diferencia de
   efectividad ≥ 10 puntos porcentuales. Si no, el resultado es "Indefinido".
5. **Umbral de ranking de rivales**: mínimo 3 partidos jugados contra ese rival,
   configurable (`App:MinPartidosRanking`).
6. **Corte de temporada**: 1 de julio por defecto, configurable por mes
   (`App:MesInicioTemporada`). Etiqueta con formato `2024/25`.
7. **Efectividad con cero partidos**: se devuelve `0` con `PJ = 0`; el frontend muestra `—`.
8. **Esquema de base**: se versiona con migraciones de EF Core, no con scripts SQL sueltos.
   La cadena de conexión apunta a `.\SQLEXPRESS`.
9. **Sin borrado lógico**: eliminar un partido lo borra de verdad, junto con su vivencia.
10. **Fechas sin hora**: el partido se identifica por día. La hora no se registra.
11. **Empates sin definición por penales**: un 1-1 es empate aunque haya habido penales;
    no se modela la instancia de eliminación.
12. **Idioma único**: español, sin i18n.
