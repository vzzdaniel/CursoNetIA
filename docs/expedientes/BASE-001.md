# BASE-001 — Alcance inicial

## Objetivo
Establecer la base técnica del proyecto **AulaPedidos** desde el estado actual, conservando **CursoNETIA.slnx**, **.NET 10** y la arquitectura en **cuatro capas** (**Api, Application, Domain, Infrastructure**), con configuración mínima para evolución posterior.

## Rol
Coordinador.

## Fuentes consultadas
- `CursoNETIA.slnx`
- `src/AulaPedidos.Api/AulaPedidos.Api.csproj`
- `src/AulaPedidos.Api/Program.cs`
- `src/AulaPedidos.Application/AulaPedidos.Application.csproj`
- `src/AulaPedidos.Domain/AulaPedidos.Domain.csproj`
- `src/AulaPedidos.Infrastructure/AulaPedidos.Infrastructure.csproj`
- `.github/copilot-instructions.md`
- `docs/multiagente-copilot.md`

## Estado base observado
- La solución ya contiene los 4 proyectos en `src/`:
  - `AulaPedidos.Api`
  - `AulaPedidos.Application`
  - `AulaPedidos.Domain`
  - `AulaPedidos.Infrastructure`
- Todos targetean `net10.0`.
- La API usa Minimal APIs y `Microsoft.AspNetCore.OpenApi`.
- Existe endpoint de salud en `/health/live`.
- No se observan proyectos de pruebas en la solución.

## Alcance funcional y técnico de BASE-001
1. **Estructura y referencias**
   - Mantener `CursoNETIA.slnx` como solución principal.
   - Mantener arquitectura de 4 capas:
	 - Domain: núcleo sin dependencias de infraestructura/transporte.
	 - Application: contratos y orquestación de casos de uso.
	 - Infrastructure: implementaciones técnicas.
	 - Api: composición con Minimal APIs.

2. **Composición por extensiones**
   - Definir y usar:
	 - `AddApplication(...)`
	 - `AddInfrastructure(...)`
   - Registro desde Api, manteniendo separación por capa.

3. **API base**
   - Mantener enfoque **Minimal APIs**.
   - Exponer **OpenAPI solo en Development**.
   - Exponer **Health Checks en `/health`**.

4. **Repository Pattern**
   - Incorporar contrato genérico `IRepository<T>` en la capa adecuada (Application/Domain según diseño final).
   - Preparar contratos para persistencia futura.
   - **No** crear repositorios ficticios, in-memory ni mocks de persistencia productiva en esta base.

5. **Mediator de martinothamar**
   - Incluir la biblioteca Mediator de **martinothamar**.
   - Registrar Mediator desde Api (a través de la configuración de servicios de capa).
   - **No usar MediatR** ni un mediador propio.

6. **Pruebas**
   - Incluir proyectos de pruebas en la solución para habilitar evolución con cobertura (unitarias y/o integración según convención del repo).

## Exclusiones explícitas (fuera de alcance)
- Entidades de dominio.
- Lógica de negocio.
- Base de datos o proveedor de persistencia concreto.
- Autenticación/autorización.

## Criterios de aceptación de alcance
- Se conserva `CursoNETIA.slnx` con .NET 10 y cuatro capas.
- Quedan definidos `AddApplication` y `AddInfrastructure` para composición.
- La API mantiene Minimal APIs, OpenAPI en Development y health endpoint en `/health`.
- Existe contrato `IRepository<T>` sin implementación ficticia de almacenamiento.
- Mediator de martinothamar queda contemplado y sin uso de MediatR.
- La solución contempla proyectos de pruebas.
- No se agregan entidades, lógica de negocio, DB ni autenticación.

## Supuestos
- BASE-001 prioriza infraestructura de arranque y contratos, no casos de uso de negocio.
- La estructura de carpetas/clases concreta se definirá en implementación, respetando convenciones existentes.

## Pendientes para implementación
- Crear/ajustar extensiones `AddApplication` y `AddInfrastructure`.
- Ajustar `Program.cs` para health checks en `/health`.
- Definir ubicación final de `IRepository<T>`.
- Agregar referencia y registro de Mediator (martinothamar).
- Crear e incluir proyectos de pruebas en la solución.

## Siguiente paso recomendado
Pasar al **Implementador** para materializar este alcance en código y pruebas, en cambios pequeños y coherentes con la solución actual.