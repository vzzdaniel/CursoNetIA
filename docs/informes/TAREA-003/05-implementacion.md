# TAREA-003 — Implementación del CRUD de Product

- Rol: Implementador. Fecha: 2026-10-13.
- Fuente vigente: [issue #2](https://github.com/vzzdaniel/CursoNetIA/issues/2).
- Contexto leído: instrucciones del repositorio y personales, convención e índice de informes, guía GitHub, flujo multiagente, informes 02/03, código y proyectos existentes. No existe `04-sintesis.md` ni el paquete BASE-001 indicado por instrucciones históricas.
- Rama remota creada mediante MCP: `codex/tarea-003-product-crud`, desde `master` (`1a5a6f3302f036d9ba2be20c6a5032aed7ffe935`).
- Se preservan todos los cambios locales ajenos, incluidos configuración de agentes/CI e Infrastructure/DependencyInjection.cs; no se publican con esta tarea.
- Draft PR: [#3](https://github.com/vzzdaniel/CursoNetIA/pull/3). Primer commit publicado: `2463da2916cd20b4a1c3a530403ec03474486400`.
- Consulta MCP: sin reviews ni check runs en ese commit. Solicitud real de Copilot code review realizada mediante MCP (respuesta sin contenido; finalización pendiente de verificar).

## Primer incremento verificable (histórico; pendientes actualizados debajo)

- Añadidos `Domain/Products/Product.cs`, `ProductValidationException.cs` y `Application.Tests/ProductTests.cs`.
- Id Guid, precio no negativo, descripciones normalizadas, límite 200 y actualización atómica respecto a validación.
- Ejecutado `dotnet build CursoNETIA.slnx --nologo -warnaserror`: correcto, sin advertencias.
- Ejecutado `dotnet test tests/AulaPedidos.Application.Tests/AulaPedidos.Application.Tests.csproj --no-build --nologo`: 10/10 correctos.
- Pendiente: handlers, persistencia/esquema, endpoints, pruebas completas, cobertura, controles remotos y URL del draft PR.

| Control | Estado | Commit y evidencia |
|---|---|---|
| Compilación y analizadores | correcto | Primer incremento local; comando anterior |
| Pruebas | correcto | Primer incremento local; 10 pruebas de Application/Domain |
| Cobertura combinada ≥80 %, reporte sin filtro | pendiente | Sin medición aún |
| NuGet Audit / Dependency Review | pendiente | Sin ejecución acreditada |
| CodeQL / alertas | pendiente | Sin ejecución ni acceso a alertas acreditados |
| Secret scanning / push protection | pendiente | Disponibilidad remota por verificar |
| Copilot code review | pendiente | Se solicitará al abrir el draft |
| GitHub Code Quality | pendiente | Licencia/configuración por verificar |