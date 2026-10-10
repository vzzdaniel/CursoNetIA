# BASE-002 — Informe 3 (Implementación)

## Objetivo
Implementar Repository Pattern + Unit of Work sobre BASE-001 con contratos en Application e implementación EF Core + SQLite en Infrastructure, siguiendo `docs/informes/BASE-002/3-sintesis.md`.

## Alcance implementado

### 1) Application
- `src/AulaPedidos.Application/Contracts/Persistence/IRepository.cs`
  - Se amplió el contrato para consultas por predicado con `Expression<Func<TEntity, bool>>`:
	- `FirstOrDefaultAsync`
	- `ListAsync`
	- `AnyAsync`
	- `CountAsync`
  - Se mantuvieron operaciones de escritura:
	- `AddAsync`
	- `RemoveAsync`
- `src/AulaPedidos.Application/Contracts/Persistence/IUnitOfWork.cs`
  - Nuevo contrato con `Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);`.

### 2) Infrastructure
- `src/AulaPedidos.Infrastructure/Persistence/AulaPedidosDbContext.cs`
  - Nuevo `DbContext` base para persistencia.
- `src/AulaPedidos.Infrastructure/Persistence/EfRepository.cs`
  - Implementación genérica de `IRepository<TEntity>` con EF Core.
  - Validaciones de argumentos nulos y ejecución async con token.
- `src/AulaPedidos.Infrastructure/Persistence/EfUnitOfWork.cs`
  - Implementación de `IUnitOfWork` sobre `DbContext.SaveChangesAsync`.
- `src/AulaPedidos.Infrastructure/DependencyInjection.cs`
  - Registro DI:
	- `AddDbContext<AulaPedidosDbContext>(UseSqlite(...))`
	- `IRepository<> -> EfRepository<>`
	- `IUnitOfWork -> EfUnitOfWork`
  - Lectura de `ConnectionStrings:AulaPedidos` con fallback a `Data Source=aulapedidos.db`.
- `src/AulaPedidos.Infrastructure/AulaPedidos.Infrastructure.csproj`
  - Paquetes añadidos:
	- `Microsoft.EntityFrameworkCore` 10.0.12
	- `Microsoft.EntityFrameworkCore.Sqlite` 10.0.12
  - Ajuste de versiones para evitar downgrades:
	- `Microsoft.Extensions.Configuration.Abstractions` 10.0.12
	- `Microsoft.Extensions.DependencyInjection.Abstractions` 10.0.12

### 3) Api (configuración)
- `src/AulaPedidos.Api/appsettings.json`
  - Añadida cadena de conexión `ConnectionStrings:AulaPedidos`.
- `src/AulaPedidos.Api/appsettings.Development.json`
  - Añadida cadena de conexión para Development.

### 4) Pruebas
- `tests/AulaPedidos.ArchitectureTests/UnitTest1.cs`
  - Verifica métodos esperados de `IRepository<>`.
  - Verifica `SaveChangesAsync` en `IUnitOfWork`.
  - Verifica que Application no referencia `Microsoft.EntityFrameworkCore`.
- `tests/AulaPedidos.Infrastructure.Tests/AulaPedidos.Infrastructure.Tests.csproj`
  - Paquetes añadidos/ajustados para integración con EF + SQLite.
- `tests/AulaPedidos.Infrastructure.Tests/UnitTest1.cs`
  - Pruebas de integración SQLite para:
	- DI de Infrastructure (registro y resolución scoped)
	- consultas por predicado
	- commit con UoW
	- eliminación + persistencia
	- cancelación en repositorio/UoW
	- guard clauses de predicados nulos (`R005`)
	- composición de filtros AND/OR (`Q001`, `Q002`)
	- sin commit no persiste (`U002`)
	- excepción por restricción única (`U003`)
	- uso de UoW tras dispose (`U006`)

## Comandos ejecutados y resultados reales

1. `dotnet build CursoNETIA.slnx`
   - **Primer intento**: falló por `NU1605` (downgrade de paquetes de `Microsoft.Extensions.*`).
   - **Tras ajuste de versiones**: **OK**.

2. `dotnet test tests/AulaPedidos.Infrastructure.Tests/AulaPedidos.Infrastructure.Tests.csproj --logger "console;verbosity=minimal"`
   - **Intento intermedio**: falló por resolución scoped desde root y modelo EF de entidad de prueba.
   - **Resultado final inicial**: **OK** (7/7).

3. `dotnet test tests/AulaPedidos.Infrastructure.Tests/AulaPedidos.Infrastructure.Tests.csproj --logger "console;verbosity=minimal"`
   - **Tras ampliar cobertura R/Q/U solicitada en 2B**: **OK** (16/16).

4. `dotnet build CursoNETIA.slnx`
   - **Resultado final**: **OK**.

5. `dotnet test CursoNETIA.slnx --logger "console;verbosity=minimal"`
   - **Resultado final**: **OK**.
   - Resumen: **27 passed, 0 failed**.

## Decisiones tomadas
- Se implementó UoW mínimo (`SaveChangesAsync`) sin exponer rollback/transacciones avanzadas.
- No se expuso `IQueryable` en contrato público; se priorizó API por predicados para mantener el alcance acotado.

## Pendientes
- Si el alcance funcional futuro lo requiere, decidir explícitamente:
  1) exposición (o no) de `IQueryable` en contratos,
  2) capacidades transaccionales adicionales (begin/commit/rollback explícitos).
