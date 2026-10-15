# TAREA-005 — Arquitectura

- **ID:** TAREA-005
- **Objetivo:** Contrato `Result` en Api y manejo global de excepciones no controladas.
- **Rol:** Arquitecto
- **Fecha:** 2026-10-15

## Fuentes consultadas

- `docs/informes/TAREA-005/01-coordinacion.md`
- `docs/adr/007-Result.md`
- `CursoNETIA.slnx`
- `src/AulaPedidos.Api/Program.cs`
- `src/AulaPedidos.Api/Endpoints/ProductEndpoints.cs`
- `src/AulaPedidos.Api/Errors/ValidationExceptionHandler.cs`
- `src/AulaPedidos.Api/Errors/ProductValidationExceptionHandler.cs`
- `src/AulaPedidos.Api/AulaPedidos.Api.csproj`
- `src/AulaPedidos.Application/Products/*.cs`
- `src/AulaPedidos.Application/Validation/ValidationBehavior.cs`
- `src/AulaPedidos.Application/AulaPedidos.Application.csproj`
- `src/AulaPedidos.Infrastructure/DependencyInjection.cs`
- `src/AulaPedidos.Infrastructure/AulaPedidos.Infrastructure.csproj`
- `src/AulaPedidos.Domain/Products/Product.cs`
- `src/AulaPedidos.Domain/Products/ProductValidationException.cs`
- `src/AulaPedidos.Domain/AulaPedidos.Domain.csproj`

## Estructura actual (4 capas)

1. **Domain**
   - `Product` protege reglas de negocio y lanza `ProductValidationException`.
   - Sin dependencias de otras capas (correcto para la arquitectura objetivo).

2. **Application**
   - Casos de uso Mediator (`CreateProduct`, `GetProducts`, `GetProductById`, `UpdateProduct`, `DeleteProduct`).
   - `ValidationBehavior` (FluentValidation) lanza `ValidationException`.
   - No usa contrato `Result` (alineado con el alcance de mantener `Result` solo en Api).

3. **Infrastructure**
   - EF Core Sqlite (`AulaPedidosDbContext`, `EfRepository`, `EfUnitOfWork`) registrado en `AddInfrastructure`.
   - Depende de `Application` y `Domain`.

4. **Api**
   - Minimal APIs en `ProductEndpoints`.
   - Manejo de excepciones mediante `IExceptionHandler` + `UseExceptionHandler()`.
   - Actualmente devuelve respuestas heterogéneas:
	 - Éxitos: `Created`, `Ok`, `NoContent`.
	 - Errores esperados: `NotFound`, `ValidationProblemDetails`, `ProblemDetails`.
   - No existe contrato de respuesta uniforme con `IsSuccess`, `Message`, `Data`.

## Dependencias relevantes actuales

- **Api**: `Microsoft.AspNetCore.OpenApi`, `Mediator.SourceGenerator`, referencia a `Application` e `Infrastructure`.
- **Application**: `Mediator.Abstractions`, `FluentValidation`.
- **Infrastructure**: `Microsoft.EntityFrameworkCore` y `Microsoft.EntityFrameworkCore.Sqlite`.
- **Domain**: sin paquetes externos.

No se requieren paquetes nuevos para esta tarea.

## Brecha frente al objetivo de TAREA-005

- El middleware/handler global actual no serializa el contrato `Result`.
- Los handlers de excepción de validación devuelven `ProblemDetails`.
- Los `404` y `204` no siguen un cuerpo uniforme.
- No hay decisión técnica cerrada para cancelaciones (`OperationCanceledException`) en el flujo global.

## Propuesta técnica (mínimo cambio coherente)

### 1) Contrato `Result` en Api

Crear un contrato en Api (por ejemplo `Api/Contracts/Result.cs`) con:

- `bool IsSuccess`
- `string? Message`
- `TData? Data`

Mantenerlo exclusivamente en Api (sin moverlo a Application/Domain).

### 2) Mapeo uniforme en endpoints

Actualizar `ProductEndpoints` para devolver `Result` en éxitos y errores HTTP esperados:

- `201/200`: `IsSuccess = true`, `Message = null`, `Data = payload`.
- `404`: `IsSuccess = false`, `Message` controlado (p. ej. "Product not found"), `Data = null`.
- `400` por validación: también en formato `Result`.

Para `DELETE` hay dos opciones:

- **Recomendada para uniformidad estricta**: cambiar `204` a `200` con `Result<object?>` y `Data = null`.
- **Alternativa conservadora**: mantener `204` como excepción explícita al contrato global.

### 3) Manejo global de excepciones inesperadas

Agregar un `IExceptionHandler` de fallback (después de los handlers específicos) que:

- Registre de forma segura (sin secretos ni detalles internos en la respuesta).
- Si la respuesta no ha comenzado, responda `500` con `Result`:
  - `IsSuccess = false`
  - `Message =` mensaje genérico seguro o `null`
  - `Data = null`
- Si la respuesta ya inició, no intentar reescribir cuerpo/cabeceras.

Para cancelaciones:

- No tratarlas como error interno 500.
- Si `RequestAborted` está cancelado, evitar convertir automáticamente a `500` (dejar manejo específico y sin exposición de detalle interno).

### 4) Validación en el mismo contrato

Adaptar `ValidationExceptionHandler` y `ProductValidationExceptionHandler` para devolver `Result` en lugar de `ProblemDetails`, preservando `400` y sin romper lógica de validación existente.

### 5) Serialización de `null`

Verificar/forzar en Api que `Message` y `Data` se serialicen incluso en `null` (siempre presentes), sin mover esta política fuera de Api.

## Archivos a cambiar (propuesta)

- `src/AulaPedidos.Api/Program.cs`
  - Registro del handler global de fallback y configuración JSON para nulos si aplica.
- `src/AulaPedidos.Api/Endpoints/ProductEndpoints.cs`
  - Respuestas envueltas en `Result` para éxito y errores esperados.
- `src/AulaPedidos.Api/Errors/ValidationExceptionHandler.cs`
  - Salida `400` en contrato `Result`.
- `src/AulaPedidos.Api/Errors/ProductValidationExceptionHandler.cs`
  - Salida `400` en contrato `Result`.
- **Nuevo:** `src/AulaPedidos.Api/Errors/UnhandledExceptionHandler.cs` (o nombre equivalente).
- **Nuevo:** `src/AulaPedidos.Api/Contracts/Result.cs` (o ubicación equivalente en Api).

Archivos potenciales de pruebas (pendiente de localización exacta):

- `tests/AulaPedidos.Api.IntegrationTests/*` para validar formato uniforme en 200/201/400/404/500.

## Alternativas comparadas (solo para decidir)

1. **Result en Mediator/Application**
   - Descartada: rompe el alcance explícito de dejar `Result` únicamente en Api.

2. **Mantener ProblemDetails y usar Result solo en 500**
   - Descartada: no cumple el objetivo de uniformidad transversal.

3. **Captura por endpoint sin handler global**
   - Descartada: duplica código y crea divergencias.

## Riesgos y límites

- **Ruptura de contrato** para clientes actuales al cambiar cuerpos y, si se decide, `204 -> 200`.
- **Cancelaciones**: no siempre es posible responder cuerpo si la conexión se cerró.
- **Streaming/descargas**: fuera del alcance inmediato; requieren excepción explícita si se agregan endpoints de ese tipo.
- **401/403**: no hay autenticación/autorización en lo consultado; si se incorporan, deben mapearse al mismo contrato.

## Aceptación técnica sugerida

1. Toda respuesta JSON de endpoints de productos incluye `IsSuccess`, `Message`, `Data`.
2. En error inesperado antes de iniciar respuesta: HTTP `500` + `Result` seguro sin detalles internos.
3. Errores de validación (`FluentValidation` y dominio) devuelven `400` con `Result`.
4. `404` devuelve `Result` con `Data = null`.
5. `Message` y `Data` aparecen serializados aunque sean `null`.
6. `Application` y `Domain` no introducen dependencia a `Result` ni tipos HTTP.
7. Cancelación de cliente no se transforma automáticamente en `500`.

## Comprobaciones realizadas

- Revisión estática de estructura y código fuente disponible.
- **No** se ejecutaron comandos, build ni tests (fuera del alcance de este rol).

## Limitaciones observadas

- No se pudo abrir `docs/informes/TAREA-005/02-arquitectura.md` previo (ruta no resuelta de forma unívoca por herramienta), por lo que se genera esta versión como base actual.
- No se localizaron rutas concretas de archivos de tests de integración mediante las herramientas disponibles; se mantiene propuesta de cobertura a nivel de carpeta/proyecto.

## Publicación en GitHub

- **Repositorio:** `vzzdaniel/CursoNetIA`
- **Issue:** `#13` — https://github.com/vzzdaniel/CursoNetIA/issues/13
- **Fase:** `arquitectura`
- **Marcador:** `<!-- aulapedidos:TAREA-005:arquitectura -->`
- **Estado:** publicado.
- **Comentario (id/url):** `6201752210` — https://github.com/vzzdaniel/CursoNetIA/issues/13#issuecomment-6201752210.
