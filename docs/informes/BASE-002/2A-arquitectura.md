# 2A — Arquitectura (BASE-002)

## Objetivo
Preparar el diseño técnico de **Repository Pattern + Unit of Work** sobre el estado actual de BASE-001, sin implementar código, definiendo contratos en Application, expectativas de Infrastructure con EF Core + SQLite, registro en Api, riesgos y decisiones abiertas.

## Rol
Arquitecto (paso 2A).

## Archivos realmente consultados
- `.github/copilot-instructions.md`
- `docs/multiagente-copilot.md`
- `docs/expedientes/BASE-001.md`
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
- `tests/AulaPedidos.Infrastructure.Tests/AulaPedidos.Infrastructure.Tests.csproj`
- `tests/AulaPedidos.ArchitectureTests/AulaPedidos.ArchitectureTests.csproj`
- `tests/AulaPedidos.ArchitectureTests/UnitTest1.cs`
- `tests/AulaPedidos.Api.IntegrationTests/AulaPedidos.Api.IntegrationTests.csproj`
- `tests/AulaPedidos.Api.IntegrationTests/UnitTest1.cs`

## Estructura actual relevante
- Solución con 4 capas (`Api`, `Application`, `Domain`, `Infrastructure`) en `.NET 10`.
- `IRepository<T>` ya existe en Application, con `AddAsync` y `RemoveAsync`.
- No existe contrato de Unit of Work.
- Infrastructure aún no tiene implementación de persistencia concreta.
- Api registra `AddApplication`, `AddInfrastructure(builder.Configuration)` y Mediator.

## Conclusiones
1. La base actual está lista para evolucionar a persistencia real sin romper cuatro capas.
2. El contrato de repositorio necesita ampliarse para consultas reutilizables (filtros por `Expression<Func<TEntity, bool>>`) y composición opcional con `IQueryable`.
3. Falta formalizar una frontera transaccional (`IUnitOfWork`) en Application.
4. Infrastructure debe incorporar EF Core + SQLite con implementaciones genéricas, manteniendo Domain sin dependencias de EF.
5. Api solo debe componer dependencias; no debe contener lógica de persistencia.

## Propuesta de diseño (sin implementación)

### 1) Contratos en Application para repositorio reutilizable
Mantener `IRepository<TEntity>` en Application (carpeta de contratos/abstracciones) y ampliar con operaciones de lectura orientadas a filtros:

- `Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);`
- `Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);`
- `Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default);`
- `Task<List<TEntity>> ListAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default);`

Para composición avanzada sin acoplar Application a EF Core:
- Opción preferida: exponer `IQueryable<TEntity> Query();` en contrato separado (`IReadRepository<TEntity>`) y mantener comandos en `IRepository<TEntity>`.
- Si se desea menor exposición de `IQueryable`, alternativa: especificaciones (`ISpecification<TEntity>`). No recomendable introducirla en BASE-002 si aún no hay necesidad concreta.

Notas de diseño:
- Priorizar `Expression<Func<TEntity, bool>>` en métodos `Get`.
- Mantener async + `CancellationToken`.
- No incluir métodos de paginación/ordenamiento hasta definir casos de uso concretos.

### 2) Contrato Unit of Work y responsabilidades
Crear `IUnitOfWork` en Application con responsabilidad transaccional mínima:

- `Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);`

Responsabilidades:
- Confirmar cambios acumulados por repositorios en un mismo scope.
- Ser la única frontera de commit en casos de uso de escritura.

No responsabilidades:
- No orquestar lógica de negocio.
- No exponer APIs de base de datos específicas en Application.

Opcional futuro (no obligatorio en BASE-002):
- `BeginTransactionAsync/CommitAsync/RollbackAsync` si aparecen casos multi-step que lo requieran.

### 3) Implementaciones esperadas en Infrastructure (EF Core + SQLite)
Estructura propuesta:
- `Persistence/AppDbContext` (EF Core).
- `Persistence/Repositories/EfRepository<TEntity>` implementando `IRepository<TEntity>`.
- `Persistence/UnitOfWork/EfUnitOfWork` implementando `IUnitOfWork` (sobre `DbContext`).

Configuración esperada:
- Paquetes en Infrastructure: `Microsoft.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore.Sqlite`.
- `DbContext` scoped.
- Repositorio genérico scoped.
- Unit of Work scoped.

Reglas de capa:
- Domain sigue sin referencia a EF.
- Application define contratos, Infrastructure implementa.
- Sin repositorios in-memory/fake productivos.

### 4) Registro de dependencias desde Api
Mantener patrón actual:
- Api llama `AddInfrastructure(builder.Configuration)`.
- Infrastructure registra `DbContext` + repositorios + unit of work.

Configuración esperada en Api:
- Connection string SQLite en `appsettings` (ej. `Data Source=aulapedidos.db`).
- Sin lógica de persistencia en `Program.cs` más allá de composición DI.

### 5) Riesgos y decisiones abiertas
Riesgos:
1. **Exponer `IQueryable`** puede filtrar detalles de persistencia a Application si se usa sin disciplina.
2. **Sobrediseño temprano** (Specification, transacciones avanzadas) sin casos de uso reales.
3. **Acoplamiento accidental a EF** si interfaces de Application incluyen tipos de EF Core.
4. **Scope inconsistente** entre repositorios y Unit of Work puede romper consistencia transaccional.

Decisiones abiertas:
1. Ubicación/nombre final del contrato (`Contracts/Persistence` vs `Abstractions/Persistence`) manteniendo consistencia del repo.
2. Si `IQueryable` vive en el contrato principal o en contrato de lectura separado.
3. Si BASE-002 incluye ya migraciones iniciales de EF Core o solo diseño + wiring DI.
4. Convención para dividir repositorios genéricos y específicos por agregado cuando aparezca dominio real.

## Supuestos
- BASE-002 se apoya en BASE-001 ya estabilizado.
- No se introducen entidades de negocio nuevas en este paso de análisis.
- La persistencia objetivo para la siguiente implementación es EF Core con proveedor SQLite.
- El análisis es estático; no se ejecutaron build/test en este paso.

## Comprobaciones pendientes
Para el Implementador en el siguiente paso:
1. Validar compilación con nuevos contratos y registros DI (`dotnet build CursoNETIA.slnx`).
2. Añadir/actualizar pruebas xUnit de arquitectura para contratos (`IRepository`, `IUnitOfWork`) y límites de capas.
3. Añadir pruebas de integración para resolución DI de repositorio/UoW y arranque API.
4. Definir criterio de uso de `IQueryable` y documentar ejemplos permitidos/no permitidos.

## Siguiente paso
Pasar al Implementador con este diseño para crear una implementación mínima de BASE-002 en fases: (1) contratos Application, (2) Infrastructure EF Core + SQLite, (3) registro DI desde Api, (4) pruebas de arquitectura/integración y evidencia de build/test.
