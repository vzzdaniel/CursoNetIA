---
applyTo: "tests/**/*.cs"
---

Usa Arrange/Act/Assert y nombres que expresen comportamiento. Cada test debe tener un riesgo y resultado observable. Prueba límites monetarios, transiciones, idempotencia, autorización, errores, persistencia y cancelación según el cambio. Usa dobles en los límites externos y base relacional para consultas; no mocks de IQueryable/DbSet. SQLite no valida semántica SQL Server. Un test no debe depender del orden ni de esperas arbitrarias. No falsees cobertura ni agregues asserts triviales.

El rol Pruebas define matriz y resultados esperados a partir de reglas del expediente antes de mirar la solución cuando sea posible; es lector y no ejecuta. Sólo el implementador designado escribe tests después de la decisión humana. El humano ejecuta comprobaciones independientes y aporta salidas; el implementador también puede correr comandos autorizados. Pruebas y Revisor evalúan en sesiones separadas sobre el mismo diff. Registrar quién ejecutó, estado, resultado y brechas; consenso o cambio de rol no son independencia. Sigue docs/multiagente-copilot.md.
