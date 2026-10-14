# TAREA-003 — Arquitectura: CRUD de Product

- ID: TAREA-003 (alias Producto-003). Rol: Arquitecto. Issue: vzzdaniel/CursoNetIA#2.
- Fuente de requisitos: issue #2 (Description/AlternateDescription sustituyen Sku/Name del expediente histórico).
- Archivos leídos: Program.cs, Application/DependencyInjection.cs, Contracts/Persistence/IRepository.cs e IUnitOfWork.cs, Infrastructure/DependencyInjection.cs, Persistence/EfRepository.cs, AulaPedidosDbContext.cs, EfUnitOfWork.cs, expediente, docs/github-pr.md, índice y README de informes.
- No leídos: csproj, tests existentes, docs/paquetes/BASE-001.

## Estructura actual
- Application: IRepository<T> (FirstOrDefault/List/Any/Count con predicado, Add, Remove; sin Update ni orden) e IUnitOfWork; Mediator sólo registrado.
- Infrastructure: EfRepository<T> (consultas con seguimiento, sin AsNoTracking), EfUnitOfWork, AulaPedidosDbContext vacío (sin DbSets ni mapeos), SQLite con `aulapedidos.db`.
- Api: Program.cs sin endpoints de productos; sin migraciones conocidas ni EnsureCreated.
- Los nombres del expediente (AppDbContext, Repository, UnitOfWork) están desactualizados.

## Propuesta
1. Domain: `Product` (Id Guid, Price, Description, AlternateDescription) con fábrica `Create` y `Update` que normalizan (trim, vacío→null en alternativa) y validan (Description obligatoria ≤200, Alternate ≤200, Price ≥ 0). Excepción de dominio propia.
2. Application: Create/Update/Delete commands, GetProductById/GetProducts queries, `ProductDto`, handlers Mediator. Not-found como resultado explícito (null/resultado simple), sin introducir Result genérico.
3. Infrastructure: `ProductConfiguration` (HasMaxLength 200, HasPrecision(18,2), Description requerida), DbSet<Product>, ApplyConfigurationsFromAssembly. Actualización vía entidad rastreada (FirstOrDefaultAsync con seguimiento) + SaveChanges. Listado: ordenar en memoria o, preferible, añadir `IProductRepository` con orden estable sólo si se demuestra necesidad (IRepository no ordena). Esquema: migración EF aditiva; no recrear ni borrar la base.
4. Api: `MapGroup("/products")` en una clase de extensión; delega en ISender con HttpContext.RequestAborted; traduce validación a 400 ProblemDetails y no encontrado a 404; POST 201+Location.
5. Dependencias: Domain←Application←Infrastructure; Api→Application/Infrastructure. Sin paquetes nuevos (salvo EF Design si se usan migraciones).

## Archivos por cambiar
Nuevos: Domain/Products/Product.cs; Application/Products/{Commands,Queries,ProductDto}; Infrastructure/Persistence/ProductConfiguration.cs (+ migración); Api/Endpoints/ProductEndpoints.cs; pruebas en proyectos existentes.
Modificados: AulaPedidosDbContext.cs, Program.cs; Application.csproj/Infrastructure.csproj sólo si faltan referencias.

## Justificación y riesgos
- Reutilizar abstracciones evita infraestructura duplicada; sin Update en IRepository, la mutación de entidad rastreada basta.
- Riesgo: estrategia de esquema no verificada (sin migraciones/EnsureCreated visibles). Riesgo: precisión decimal en SQLite (almacena como TEXT; el orden/comparación por precio no es fiable). Riesgo: el expediente histórico contradice el issue.
- Las lecturas con seguimiento no cumplen la "AsNoTracking" del expediente; irrelevante para update, pero afecta consultas de sólo lectura.

## Aceptación sugerida
Criterios del issue #2 más: pruebas de dominio (límites 200/201, trim, cero, negativo), handlers con repositorio/UoW, repositorio con SQLite aislado, integración HTTP con base aislada (201/Location, 200 [], 400, 404, GUID mal formado), pruebas de arquitectura vigentes, `dotnet build/test CursoNETIA.slnx`.

## Pendientes
Confirmar estrategia de esquema, orden del listado y forma de errores; leer tests y csproj antes de implementar.

## Publicación en GitHub
comentario id 6157690326, https://github.com/vzzdaniel/CursoNetIA/issues/2#issuecomment-6157690326; fecha 2026-10-13; estado: publicado (sin comentarios previos en el issue).
