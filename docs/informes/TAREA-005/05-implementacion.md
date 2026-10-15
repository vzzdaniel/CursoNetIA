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
- **Draft PR:** pendiente de creación/actualización en este informe.

## Estado de controles en esta entrega

| Control | Estado | Commit y evidencia |
|---|---|---|
| Compilación y analizadores | correcto | `dotnet build CursoNETIA.slnx` local correcto |
| Pruebas | correcto | `dotnet test CursoNETIA.slnx` local: 70/70 |
| Cobertura de líneas del código mantenido ≥ 80 % | fallido | Ejecución local con `XPlat Code Coverage`: 47.67 % combinado observado |
| NuGet Audit y Dependency Review | pendiente | Sin evidencia local del commit remoto |
| CodeQL y alertas de seguridad | pendiente | Sin evidencia local del commit remoto |
| Secret scanning / push protection | no disponible | No accesible desde las herramientas MCP disponibles en esta sesión |
| Copilot code review | pendiente | Se solicitará tras abrir el draft PR |
| GitHub Code Quality, si está habilitado | pendiente | Pendiente de verificar en checks del PR |

## Pendientes

1. Publicar estos cambios en la rama remota y abrir/actualizar draft PR.
2. Solicitar revisión automática con `request_copilot_review` en el PR real.
3. Revisar checks y alertas del commit actual del PR para actualizar estado de controles remotos.
