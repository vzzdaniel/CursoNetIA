# TAREA-005 — Implementación

- **ID:** TAREA-005
- **Objetivo:** Contrato `Result` en Api y manejo global de excepciones no controladas.
- **Rol:** Implementador
- **Fecha:** 2026-10-15

## Archivos consultados

- `docs/adr/007-Result.md`
- `docs/informes/TAREA-005/02-arquitectura.md`
- `docs/informes/TAREA-005/03-pruebas.md`
- `docs/informes/TAREA-005/04-sintesis.md`
- `src/AulaPedidos.Api/Program.cs`
- `src/AulaPedidos.Api/Endpoints/ProductEndpoints.cs`
- `src/AulaPedidos.Api/Errors/ValidationExceptionHandler.cs`
- `src/AulaPedidos.Api/Errors/ProductValidationExceptionHandler.cs`
- `tests/AulaPedidos.Api.IntegrationTests/ProductEndpointTests.cs`
- `tests/AulaPedidos.Api.IntegrationTests/ValidationEndpointTests.cs`

## Cambios implementados

1. **Contrato `Result` en Api**
   - Nuevo archivo `src/AulaPedidos.Api/Contracts/Result.cs` con `IsSuccess`, `Message` y `Data`.
   - Se forzó serialización de `Message` y `Data` en `null` mediante `JsonIgnore(Condition = Never)`.

2. **Respuestas uniformes en endpoints de productos**
   - `src/AulaPedidos.Api/Endpoints/ProductEndpoints.cs` ahora envuelve respuestas exitosas y `404` en `Result`.
   - `DELETE` se implementó con **200 + `Result<object?>`** en éxito para mantener uniformidad de contrato (en lugar de `204`).

3. **Manejo global de excepciones no controladas**
   - Nuevo `src/AulaPedidos.Api/Errors/UnhandledExceptionHandler.cs`.
   - Responde `500` con `Result` seguro (`IsSuccess=false`, `Message` genérico, `Data=null`) cuando la respuesta no ha iniciado.
   - Si la respuesta ya inició, no intenta reescribirla.
   - Si la excepción es cancelación de cliente (`OperationCanceledException` con `RequestAborted`), no la convierte a `500`.

4. **Validaciones en contrato `Result` (400)**
   - `src/AulaPedidos.Api/Errors/ValidationExceptionHandler.cs` y `src/AulaPedidos.Api/Errors/ProductValidationExceptionHandler.cs` dejaron de devolver `ProblemDetails` y ahora devuelven `Result`.

5. **Configuración JSON en Api**
   - `src/AulaPedidos.Api/Program.cs` configura `ConfigureHttpJsonOptions` para no omitir `null`.
   - Registro del `UnhandledExceptionHandler` en el pipeline de excepción.

6. **Pruebas de integración actualizadas**
   - `tests/AulaPedidos.Api.IntegrationTests/ProductEndpointTests.cs`
   - `tests/AulaPedidos.Api.IntegrationTests/ValidationEndpointTests.cs`
   - Ajustes para validar contrato `Result` en `200/201/400/404/500`.
   - Se verifica que el JSON incluya explícitamente `message` y `data`.

## Comprobaciones ejecutadas

### Ejecutadas

1. `dotnet build CursoNETIA.slnx`
   - **Resultado:** correcto.

2. `dotnet test CursoNETIA.slnx`
   - **Resultado:** correcto.
   - **Resumen:** 70 pruebas, 0 fallidas, 0 omitidas.

3. `dotnet test CursoNETIA.slnx --collect:"XPlat Code Coverage" --results-directory TestResults`
   - **Resultado:** correcto.
   - **Artefactos locales generados:**
	 - `TestResults/22c35455-6e17-4d6d-b254-3008c97e6155/coverage.cobertura.xml`
	 - `TestResults/3e761885-a144-46ea-9966-3c1849884d0c/coverage.cobertura.xml`
	 - `TestResults/8226d8a5-0bd6-49b6-830c-913634127d73/coverage.cobertura.xml`
	 - `TestResults/5d458629-0fcc-4835-b36b-9435ac5f91ed/coverage.cobertura.xml`
   - **Cobertura combinada observada (suma de líneas de esos 4 reportes):** 933 / 1957 = **47.67 %**.

### No ejecutadas localmente (pendientes en PR/checks remotos)

- NuGet Audit / Dependency Review del commit remoto del PR.
- CodeQL y code scanning remoto.
- Secret scanning / push protection.
- GitHub Code Quality (si está habilitado).

## Referencias de issue y PR

- **Issue:** `#13` — https://github.com/vzzdaniel/CursoNetIA/issues/13
- **Rama remota de trabajo:** `codex/tarea-005-result-api-excepciones`
- **Draft PR:** `#14` — https://github.com/vzzdaniel/CursoNetIA/pull/14

## Estado de controles en esta entrega

| Control | Estado | Commit y evidencia |
|---|---|---|
| Compilación y analizadores | correcto | `5fe9a68` (código): `dotnet build CursoNETIA.slnx` local correcto |
| Pruebas | correcto | `5fe9a68` (código): `dotnet test CursoNETIA.slnx` local: 70/70 |
| Cobertura de líneas del código mantenido ≥ 80 % | fallido | `5fe9a68` (código): `XPlat Code Coverage` local 47.67 % combinado observado |
| NuGet Audit y Dependency Review | en curso | `ae8e824` (head actual): NuGet audit en ejecución (`https://github.com/vzzdaniel/CursoNetIA/actions/runs/38806940287/job/116670109328`); no check Dependency Review visible para este head |
| CodeQL y alertas de seguridad | pendiente | `ae8e824` (head actual): sin check CodeQL visible para este head |
| Secret scanning / push protection | no disponible | No accesible desde las herramientas MCP disponibles en esta sesión |
| Copilot code review | en curso | Solicitud enviada en PR #14 para `5fe9a68` y repetida para `ae8e824`; sin reseña publicada aún |
| GitHub Code Quality, si está habilitado | pendiente | Sin check visible de Code Quality en PR #14 |

## Pendientes

1. Esperar finalización y resultado de `Build, tests and coverage` y `NuGet audit` del head actual `ae8e824`.
2. Confirmar si para el head actual se ejecutan o no `Dependency review` y `CodeQL`.
3. Verificar publicación de revisión de Copilot sobre el commit actual y atender hallazgos dentro de alcance.
