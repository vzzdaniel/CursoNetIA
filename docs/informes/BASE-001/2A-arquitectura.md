# 2A — Arquitectura (BASE-001)

## Objetivo
Revisar BASE-001 (`docs/expedientes/BASE-001.md`), contrastar el estado actual del repositorio y proponer ajustes mínimos para alinear la solución con el alcance solicitado.

## Rol
Arquitecto (paso 2A).

## Archivos realmente consultados
- `.github/agents/arquitecto.agent.md`
- `docs/multiagente-copilot.md`
- `docs/expedientes/BASE-001.md`
- `CursoNETIA.slnx`
- `src/AulaPedidos.Api/AulaPedidos.Api.csproj`
- `src/AulaPedidos.Api/Program.cs`
- `src/AulaPedidos.Application/AulaPedidos.Application.csproj`
- `src/AulaPedidos.Domain/AulaPedidos.Domain.csproj`
- `src/AulaPedidos.Infrastructure/AulaPedidos.Infrastructure.csproj`

## Trazabilidad de la fuente BASE-001
- El contenido de alcance BASE-001 usado para este análisis proviene de `docs/expedientes/BASE-001.md`.
- `.github/agents/arquitecto.agent.md` se consultó como guía operativa del rol Arquitecto (flujo e instrucciones), no como expediente funcional de BASE-001.

## Limitaciones observadas durante el análisis
- No se localizó `docs/paquetes/BASE-001-paquete-comun.md` en la ruta esperada.
- No se encontraron (o no pudieron resolverse por ruta) archivos de extensiones de DI (`AddApplication`, `AddInfrastructure`), contrato `IRepository<T>` ni proyectos de pruebas.

## Estructura actual
- Solución `CursoNETIA.slnx` con 4 proyectos en `src/`:
  - `AulaPedidos.Api`
  - `AulaPedidos.Application`
  - `AulaPedidos.Domain`
  - `AulaPedidos.Infrastructure`
- Todos los proyectos targetean `net10.0`.
- Dependencias entre capas (correctas para 4 capas):
  - `Api -> Application, Infrastructure`
  - `Infrastructure -> Application, Domain`
  - `Application -> Domain`
  - `Domain` sin referencias de salida
- API actual:
  - Usa Minimal APIs.
  - OpenAPI se mapea solo en Development.
  - Endpoint de salud actual: `/health/live` (no coincide con el criterio de `/health`).

## Brecha frente a BASE-001
1. Falta composición por extensiones (`AddApplication`, `AddInfrastructure`).
2. Falta contrato `IRepository<T>`.
3. Falta biblioteca Mediator de martinothamar y su registro.
4. Falta estandarizar health checks en `/health`.
5. No hay proyectos de pruebas en la solución.

## Dependencias propuestas (mínimas)
- **Application**: agregar paquete `Mediator` (martinothamar) para registro de handlers desde la capa de aplicación.
- **Api**: mantener `Microsoft.AspNetCore.OpenApi`; usar health checks del framework ASP.NET Core (sin infraestructura ficticia).
- **Sin MediatR** y sin mediador propio.

## Archivos por cambiar (propuesta)
1. `src/AulaPedidos.Application/AulaPedidos.Application.csproj`
   - Agregar referencia a `Mediator`.
2. `src/AulaPedidos.Application/...`
   - Crear extensión `AddApplication(IServiceCollection)` para registrar Mediator.
3. `src/AulaPedidos.Application/.../IRepository.cs` (o ruta equivalente en Application)
   - Definir contrato genérico `IRepository<T>` sin implementación.
4. `src/AulaPedidos.Infrastructure/...`
   - Crear extensión `AddInfrastructure(IServiceCollection)` con registro base (sin repositorios fake ni almacenamiento).
5. `src/AulaPedidos.Api/Program.cs`
   - Componer servicios vía `AddApplication` y `AddInfrastructure`.
   - Configurar health checks en `/health`.
   - Mantener OpenAPI solo en Development.
6. `CursoNETIA.slnx` (+ carpeta `tests/`)
   - Agregar proyectos de pruebas mínimos para habilitar evolución.

## Decisiones de diseño sugeridas
- Ubicar `IRepository<T>` en **Application** (contrato de caso de uso), evitando acoplar Domain a persistencia cuando aún no existen entidades.
- Mantener Domain vacío en esta base para respetar la exclusión explícita de lógica y entidades.
- Registrar Mediator en Application y llamarlo desde Api mediante extensión de servicios.

## Alternativa considerada
- Poner `IRepository<T>` en Domain.
  - **Pros**: contrato muy cercano al núcleo.
  - **Contras**: sin entidades/casos de uso definidos en BASE-001, puede introducir una abstracción prematura en Domain.
  - **Decisión**: priorizar Application para minimizar diseño anticipado.

## Justificación
La propuesta mantiene cambios pequeños, respeta las cuatro capas y cumple el alcance vigente de BASE-001: preparación de infraestructura de composición, contratos de repositorio sin almacenamiento ficticio y uso de Mediator (martinothamar) sin introducir negocio, DB ni autenticación.

## Aceptación sugerida
Aceptar cuando se verifique:
1. `CursoNETIA.slnx` conserva los 4 proyectos base y añade proyectos de pruebas.
2. Existen `AddApplication` y `AddInfrastructure` consumidos desde `Program.cs`.
3. Existe `IRepository<T>` como contrato sin implementación de almacenamiento.
4. Mediator de martinothamar está referenciado y registrado; no se usa MediatR.
5. API mantiene Minimal APIs, OpenAPI solo en Development y health checks en `/health`.
6. No se agregan entidades, lógica de negocio, proveedor DB ni autenticación.

## Comprobaciones realizadas o pendientes
- **Realizadas (análisis estático):** revisión de solución, proyectos y `Program.cs`.
- **Pendientes (implementación/verificación):** compilación y pruebas de solución tras aplicar cambios.

## Siguiente paso
Pasar al Implementador para materializar los cambios mínimos indicados y luego validar con build/test.
