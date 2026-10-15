# TAREA-005 — Pruebas

- **ID:** TAREA-005
- **Objetivo:** Contrato `Result` en Api y manejo global de excepciones no controladas.
- **Rol:** Pruebas (fase `pruebas-inicial`)
- **Fecha:** 2026-10-15

## Fuentes consultadas

- `docs/informes/TAREA-005/01-coordinacion.md`
- `docs/adr/007-Result.md`
- `docs/informes/TAREA-005/02-arquitectura.md`
- `src/AulaPedidos.Api/Program.cs`
- `src/AulaPedidos.Api/Endpoints/ProductEndpoints.cs`
- `src/AulaPedidos.Api/Errors/ValidationExceptionHandler.cs`
- `src/AulaPedidos.Api/Errors/ProductValidationExceptionHandler.cs`
- `tests/AulaPedidos.Api.IntegrationTests/ProductEndpointTests.cs`
- `tests/AulaPedidos.Api.IntegrationTests/ValidationEndpointTests.cs`
- `tests/AulaPedidos.Api.IntegrationTests/ProductApiFactory.cs`
- `tests/AulaPedidos.Api.IntegrationTests/UnitTest1.cs`

## Análisis del estado actual (sin ejecutar pruebas)

- El estado actual **no** implementa contrato uniforme `Result` en Api: endpoints y handlers devuelven `ProductDto`, `NoContent`, `NotFound` y `ProblemDetails`/`ValidationProblemDetails`.
- Ya existe manejo global de excepciones con `UseExceptionHandler()` y handlers específicos, pero el contrato de error sigue en `problem+json`.
- Las pruebas de integración actuales validan explícitamente `ProblemDetails`, por lo que fallarán cuando se migre a `Result` si no se actualizan.

## Casos prioritarios propuestos

> Nota: esta tabla define diseño de pruebas. No equivale a ejecuciones reales.

| Requisito | Caso | Entrada | Resultado esperado | Comprobación |
|---|---|---|---|---|
| Uniformidad en éxito con `IsSuccess/Message/Data` | Crear producto válido | `POST /products` con `{ price: 12.34, description: "Valid" }` | `201` con JSON `{"isSuccess":true,"message":null,"data":{...}}`; `data.id` no vacío; `Location` correcto | **Pendiente de ejecución** (actualmente el código devuelve `ProductDto` directo) |
| Uniformidad en éxito de consulta | Listar productos | `GET /products` | `200` con `isSuccess:true`, `message:null`, `data:[...]` (array, puede vacío) | **Pendiente de ejecución** |
| Uniformidad en no encontrado | Obtener por id inexistente | `GET /products/{guid-no-existe}` | `404` con `isSuccess:false`, `message` controlado, `data:null` | **Pendiente de ejecución** (actualmente `NotFound` sin cuerpo) |
| Uniformidad en actualización | Actualizar existente válido | `PUT /products/{id}` con payload válido | `200` con `isSuccess:true`, `message:null`, `data` actualizado | **Pendiente de ejecución** |
| Decisión sobre borrado (`DELETE`) | Borrar existente | `DELETE /products/{id}` | **Opción A (uniformidad estricta):** `200` + `Result` con `data:null`. **Opción B (excepción explícita):** `204` sin cuerpo documentado como excepción. | **Pendiente de decisión y ejecución** |
| Validación FluentValidation en contrato Result | Crear inválido por `price`/`description` | `POST /products` con `price:-1` o `description` vacío | `400` con `isSuccess:false`, `data:null`; sin `ProblemDetails`; sin filtrar mensajes internos sensibles | **Pendiente de ejecución** (hoy valida `ValidationProblemDetails`) |
| Validación de dominio en contrato Result | Excepción `ProductValidationException` | Forzar fallo de dominio (ej. `UnitOfWork` lanzando excepción de dominio) | `400` con `Result` homogéneo (`isSuccess:false`, `data:null`) | **Pendiente de ejecución** (hoy devuelve `ProblemDetails`) |
| Excepción inesperada sin fuga de detalles | Error técnico no controlado | Forzar `InvalidOperationException("TechnicalFailureMarker")` | `500` con `isSuccess:false`, `data:null`, mensaje genérico o `null`; no exponer `TechnicalFailureMarker` ni stacktrace | **Pendiente de ejecución** (hoy `ProblemDetails` genérico) |
| Nulabilidad serializada siempre presente | Campos `message` y `data` en `null` | Casos `404`, `500`, éxito sin payload | El JSON incluye siempre las claves `message` y `data` aunque valgan `null` | **Pendiente de ejecución** |
| Cancelaciones no convertidas a 500 | Cancelación de cliente/request abortado | Petición cancelada durante procesamiento | No devolver `500` artificial por cancelación; comportamiento consistente y sin fuga de detalle | **Pendiente de diseño técnico y ejecución** |
| Respuesta iniciada no se reescribe | Error después de empezar body | Endpoint de prueba que escriba parte de respuesta y luego falle | No intento de segunda respuesta/cabeceras; registro de error; sin corrupción de protocolo | **Pendiente de prueba específica** |
| Result solo en Api (sin contaminar capas) | Restricción de arquitectura | Revisión de referencias `Result` en `Application` y `Domain` | Ninguna dependencia nueva a contrato Api en capas internas | **Análisis de código actual:** no existe `Result` aún fuera de Api; **pendiente** de validación posterior al cambio |

## Defectos que estos casos detectarían

- Regresión de contrato HTTP/JSON (códigos o forma de body inconsistente por endpoint).
- Persistencia involuntaria en flujos inválidos (fallo de atomicidad en validaciones).
- Fuga de información sensible en errores 500.
- Omisión de `message`/`data` cuando son `null`.
- Introducción accidental de dependencias de Api en `Application`/`Domain`.

## Evidencia disponible vs pendientes

### Evidencia realmente disponible

- **Análisis de código**: el estado actual sigue usando `ProblemDetails` y respuestas heterogéneas (`NoContent`, `NotFound`, DTO directo).
- **Diseño de tests existente**: pruebas de integración actuales (`ProductEndpointTests`, `ValidationEndpointTests`) validan el contrato anterior y sirven como base para migrar aserciones a `Result`.

### Evidencia ejecutada por otros

- No se aportaron en esta tarea salidas de CI, logs de ejecución ni reportes de pruebas para TAREA-005.

### Verificaciones pendientes

- Ejecución real de pruebas de integración tras implementar `Result`.
- Confirmación explícita de la decisión `DELETE` (200 con `Result` o 204 como excepción documentada).
- Cobertura del caso de respuesta iniciada y cancelación en entorno de integración.

## Pasos concretos para Implementador o alumno

1. Implementar contrato `Result` en Api y adaptar endpoints/handlers (`ProductEndpoints`, `ValidationExceptionHandler`, `ProductValidationExceptionHandler`, fallback global de excepciones).
2. Actualizar/añadir pruebas de integración en `tests/AulaPedidos.Api.IntegrationTests` para cada caso de la tabla.
3. Priorizar primero: `201/200`, `400`, `404`, `500` y nulabilidad serializada.
4. Añadir prueba explícita para `DELETE` según la decisión final de contrato.
5. Añadir prueba de no fuga de detalle interno (`TechnicalFailureMarker` no visible).
6. Documentar en el PR qué casos quedaron pendientes y por qué.

## Criterios de aceptación comprobables

- Todos los endpoints de productos devuelven el contrato acordado o excepciones explícitamente documentadas.
- Los errores de validación y dominio usan `400` con `Result`, sin `ProblemDetails`.
- Las excepciones inesperadas previas al inicio de respuesta devuelven `500` con `Result` seguro.
- `message` y `data` existen en el JSON incluso cuando son `null`.
- No hay uso de `Result` en `Application` ni `Domain`.

## Publicación en GitHub

- **Repositorio:** `vzzdaniel/CursoNetIA`
- **Issue:** `#13`
- **URL:** https://github.com/vzzdaniel/CursoNetIA/issues/13
- **Fase/marcador:** `pruebas-inicial` / `<!-- aulapedidos:TAREA-005:pruebas-inicial -->`
- **Fecha:** 2026-10-15
- **Estado:** publicado
- **Comentario (id/url):** `6201800571` / https://github.com/vzzdaniel/CursoNetIA/issues/13#issuecomment-6201800571
