# BASE-001 — Plan de implementación (2B-arquitectura)

## Objetivo
Definir un plan ejecutable para que el Implementador alinee la solución actual con un resultado equivalente al ejemplo de referencia, manteniendo BASE-001, .NET 10, Minimal APIs y arquitectura en cuatro capas.

## Rol
Arquitectura (plan para implementación).

## Archivos realmente consultados
### Solución actual
- `CursoNETIA.slnx`
- `src/AulaPedidos.Api/Program.cs`
- `src/AulaPedidos.Api/AulaPedidos.Api.csproj`
- `src/AulaPedidos.Api/AulaPedidos.Api.http`
- `src/AulaPedidos.Application/AulaPedidos.Application.csproj`
- `src/AulaPedidos.Application/DependencyInjection.cs`
- `src/AulaPedidos.Application/Contracts/Persistence/IRepository.cs`
- `src/AulaPedidos.Infrastructure/AulaPedidos.Infrastructure.csproj`
- `src/AulaPedidos.Infrastructure/DependencyInjection.cs`
- `src/AulaPedidos.Domain/AulaPedidos.Domain.csproj`
- `tests/AulaPedidos.Application.Tests/AulaPedidos.Application.Tests.csproj`
- `tests/AulaPedidos.Application.Tests/UnitTest1.cs`
- `tests/AulaPedidos.Infrastructure.Tests/AulaPedidos.Infrastructure.Tests.csproj`
- `tests/AulaPedidos.Infrastructure.Tests/UnitTest1.cs`

### Referencia comparada
- `../../Downloads/ArqSwConIaTraining/CursoNetIA_-master/CursoNETIA.slnx`
- `../../Downloads/ArqSwConIaTraining/CursoNetIA_-master/src/AulaPedidos.Api/Program.cs`
- `../../Downloads/ArqSwConIaTraining/CursoNetIA_-master/src/AulaPedidos.Api/AulaPedidos.Api.csproj`
- `../../Downloads/ArqSwConIaTraining/CursoNetIA_-master/src/AulaPedidos.Api/AulaPedidos.Api.http`
- `../../Downloads/ArqSwConIaTraining/CursoNetIA_-master/src/AulaPedidos.Application/AulaPedidos.Application.csproj`
- `../../Downloads/ArqSwConIaTraining/CursoNetIA_-master/src/AulaPedidos.Application/DependencyInjection.cs`
- `../../Downloads/ArqSwConIaTraining/CursoNetIA_-master/src/AulaPedidos.Application/Abstractions/Persistence/IRepository.cs`
- `../../Downloads/ArqSwConIaTraining/CursoNetIA_-master/src/AulaPedidos.Infrastructure/AulaPedidos.Infrastructure.csproj`
- `../../Downloads/ArqSwConIaTraining/CursoNetIA_-master/src/AulaPedidos.Infrastructure/DependencyInjection.cs`
- `../../Downloads/ArqSwConIaTraining/CursoNetIA_-master/tests/AulaPedidos.Api.IntegrationTests/AulaPedidos.Api.IntegrationTests.csproj`
- `../../Downloads/ArqSwConIaTraining/CursoNetIA_-master/tests/AulaPedidos.Api.IntegrationTests/ApiTests.cs`
- `../../Downloads/ArqSwConIaTraining/CursoNetIA_-master/tests/AulaPedidos.ArchitectureTests/AulaPedidos.ArchitectureTests.csproj`
- `../../Downloads/ArqSwConIaTraining/CursoNetIA_-master/tests/AulaPedidos.ArchitectureTests/ArchitectureTests.cs`

## Estructura actual y brechas detectadas
1. **Capas y .NET 10**: correcto.
2. **Composición DI**: existe `AddApplication` y `AddInfrastructure`, pero falta alinear registro de Mediator con el patrón de la referencia.
3. **Contrato Repository**: `IRepository<T>` existe, pero está vacío.
4. **Pruebas**: hay xUnit unitarias básicas de DI; faltan pruebas de arquitectura e integración API equivalentes.
5. **Artifacts API**: `AulaPedidos.Api.http` todavía usa `/weatherforecast` y no refleja `/health` + OpenAPI.

## Alcance de implementación propuesto (mínimo, por fases)

### Fase 1 — Contratos y composición (Application/Infrastructure/Api)
1. Actualizar `IRepository<T>` para incluir únicamente:
   - `Task AddAsync(T entity, CancellationToken cancellationToken = default);`
   - `Task RemoveAsync(T entity, CancellationToken cancellationToken = default);`
2. Ajustar `AulaPedidos.Application/DependencyInjection.cs` para registrar:
   - `ISender` y `IPublisher` como scoped resolviendo `IMediator`.
3. Ajustar `AulaPedidos.Infrastructure/DependencyInjection.cs` para aceptar:
   - `IConfiguration configuration` en `AddInfrastructure(...)`.
4. Ajustar `src/AulaPedidos.Api/Program.cs` para:
   - llamar `AddInfrastructure(builder.Configuration)`;
   - registrar `AddMediator(options => options.Assemblies = [typeof(AulaPedidos.Application.DependencyInjection)] ...)`;
   - mantener OpenAPI en Development y `/health`.
   - declarar `public partial class Program;`.

### Fase 2 — Dependencias de paquetes y referencias
1. `src/AulaPedidos.Application/AulaPedidos.Application.csproj`
   - mantener `Mediator.Abstractions` y `Microsoft.Extensions.DependencyInjection.Abstractions`.
   - retirar `Mediator.SourceGenerator` de Application.
2. `src/AulaPedidos.Api/AulaPedidos.Api.csproj`
   - agregar `Mediator.SourceGenerator` con `PrivateAssets="all"`.
3. `src/AulaPedidos.Infrastructure/AulaPedidos.Infrastructure.csproj`
   - agregar `Microsoft.Extensions.Configuration.Abstractions`.
   - dejar referencia a `Application`; mantener referencia a `Domain` solo si el estado actual del código la requiere explícitamente.

### Fase 3 — Pruebas faltantes (xUnit)
1. Crear `tests/AulaPedidos.ArchitectureTests` (xUnit) y agregarlo a `CursoNETIA.slnx`.
2. Crear `tests/AulaPedidos.Api.IntegrationTests` (xUnit + `Microsoft.AspNetCore.Mvc.Testing`) y agregarlo a `CursoNETIA.slnx`.
3. Cobertura mínima esperada:
   - **Arquitectura**: fronteras de capas, paquetes permitidos/prohibidos, contrato `IRepository<T>`, ausencia de implementaciones de repositorio.
   - **Integración API**: `/health` en Development/Production/Staging, OpenAPI solo Development, rutas plantilla fuera de uso.

### Fase 4 — Artefactos de soporte
1. Actualizar `src/AulaPedidos.Api/AulaPedidos.Api.http` a:
   - `GET /health`
   - `GET /openapi/v1.json` (nota: solo Development).

## Archivos por cambiar (plan de trabajo)
- `src/AulaPedidos.Application/Contracts/Persistence/IRepository.cs` (o mover a `Abstractions/Persistence` si se adopta exactamente la referencia)
- `src/AulaPedidos.Application/DependencyInjection.cs`
- `src/AulaPedidos.Application/AulaPedidos.Application.csproj`
- `src/AulaPedidos.Infrastructure/DependencyInjection.cs`
- `src/AulaPedidos.Infrastructure/AulaPedidos.Infrastructure.csproj`
- `src/AulaPedidos.Api/Program.cs`
- `src/AulaPedidos.Api/AulaPedidos.Api.csproj`
- `src/AulaPedidos.Api/AulaPedidos.Api.http`
- `CursoNETIA.slnx`
- `tests/AulaPedidos.ArchitectureTests/*`
- `tests/AulaPedidos.Api.IntegrationTests/*`

## Criterios de aceptación sugeridos
1. Solución compila en `net10.0` con cuatro capas intactas.
2. `IRepository<T>` no está vacío y mantiene independencia de almacenamiento.
3. Mediator de martinothamar queda registrado sin MediatR ni mediador propio.
4. API mantiene `/health` y OpenAPI solo en Development.
5. Existen proyectos de pruebas xUnit de arquitectura e integración API en la solución.
6. No se introducen entidades, lógica de negocio ni persistencia concreta.

## Supuestos
- Se busca equivalencia técnica con la referencia, no copia literal de estructura/nombres.
- La implementación debe respetar el alcance BASE-001 (sin extender a dominio funcional).

## Comprobaciones realizadas y pendientes
### Realizadas
- Comparación estática de estructura, contratos, DI, paquetes y pruebas entre solución actual y referencia.

### Pendientes (para Implementador)
- `dotnet build CursoNETIA.slnx`
- `dotnet test CursoNETIA.slnx`
- Verificación HTTP de `/health` y `/openapi/v1.json` por entorno.

## Siguiente paso
Implementador: ejecutar fases 1→4 en cambios pequeños, mantener trazabilidad por archivo y entregar evidencia de build/test + validación HTTP.
