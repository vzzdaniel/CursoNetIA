# BASE-002 — Informe 2B (Pruebas)

## Objetivo
Diseñar la estrategia de pruebas para incorporar **Repository Pattern + Unit of Work** sobre BASE-001 con **EF Core + SQLite**, sin implementar cambios de código en este paso.

## Rol
Pruebas (paso 2B).

## Archivos realmente consultados
- `.github/copilot-instructions.md`
- `docs/multiagente-copilot.md`
- `CursoNETIA.slnx`
- `src/AulaPedidos.Api/Program.cs`
- `src/AulaPedidos.Application/DependencyInjection.cs`
- `src/AulaPedidos.Application/Contracts/Persistence/IRepository.cs`
- `src/AulaPedidos.Infrastructure/DependencyInjection.cs`
- `src/AulaPedidos.Infrastructure/AulaPedidos.Infrastructure.csproj`
- `tests/AulaPedidos.Api.IntegrationTests/AulaPedidos.Api.IntegrationTests.csproj`
- `tests/AulaPedidos.Api.IntegrationTests/UnitTest1.cs`
- `tests/AulaPedidos.Application.Tests/UnitTest1.cs`
- `tests/AulaPedidos.ArchitectureTests/UnitTest1.cs`
- `tests/AulaPedidos.Infrastructure.Tests/AulaPedidos.Infrastructure.Tests.csproj`
- `tests/AulaPedidos.Infrastructure.Tests/UnitTest1.cs`

## Estado base observado (antes de BASE-002)
- Existe contrato mínimo `IRepository<T>` con `AddAsync` y `RemoveAsync`.
- No se observan aún contratos/implementaciones explícitas de **Unit of Work**.
- Infrastructure no referencia todavía paquetes EF Core/SQLite en el `.csproj` consultado.
- Hay proyectos de pruebas xUnit ya incorporados, pero actualmente centrados en DI/arquitectura y endpoints base.

## Matriz de casos propuesta

> Nota: los casos están definidos para guiar implementación y validación de BASE-002. La columna **Comprobación** distingue estado actual (solo análisis) vs ejecución real pendiente.

### 1) Repositorio genérico y predicados en gets

| Requisito | Caso | Entrada | Resultado esperado | Defecto que detectaría | Comprobación |
|---|---|---|---|---|---|
| Agregar entidad en repositorio genérico | R-001 AddAsync persiste en contexto | Entidad válida + `CancellationToken.None` | Entidad queda en `Added`/persistida tras commit UoW | `AddAsync` no rastrea entidad o no llega a BD | **Análisis de diseño:** definido. **Ejecución:** pendiente |
| Eliminar entidad existente | R-002 RemoveAsync elimina entidad rastreada | Entidad existente | Entidad removida tras commit | `RemoveAsync` no marca `Deleted` o no aplica cambio | **Análisis:** definido. **Ejecución:** pendiente |
| Get por predicado simple | R-003 Get(s) con predicado exacto | Predicado `x => x.Prop == value` | Devuelve solo entidades que cumplen condición | Ignora predicado y devuelve todo | **Análisis:** definido. **Ejecución:** pendiente |
| Get con predicado sin coincidencias | R-004 Predicado sin match | Predicado no cumplido por ningún registro | Resultado vacío (no excepción) | Falso positivo por seed incorrecto o fallback no controlado | **Análisis:** definido. **Ejecución:** pendiente |
| Get con predicado nulo (si diseño lo prohíbe) | R-005 Guard clause de predicado | `predicate = null` | `ArgumentNullException` (o comportamiento explícito documentado) | NRE tardío / comportamiento ambiguo | **Análisis:** definido. **Ejecución:** pendiente |
| Cancelación en operación async | R-006 Respeto de CancellationToken | Token cancelado antes/durante query | Lanza `OperationCanceledException` cuando aplique | Ignorar cancelación y bloquear ejecución | **Análisis:** definido. **Ejecución:** pendiente |

### 2) Composición sobre IQueryable (filtros, orden, paginación)

| Requisito | Caso | Entrada | Resultado esperado | Defecto que detectaría | Comprobación |
|---|---|---|---|---|---|
| Filtros combinados AND | Q-001 Composición de predicados | `Where(A).Where(B)` o equivalente | Solo elementos que cumplen A y B | Aplicación parcial de filtros | **Análisis:** definido. **Ejecución:** pendiente |
| Filtros con OR explícito | Q-002 Predicado OR | `x => A || B` | Incluye registros que cumplan cualquiera | Traducción incorrecta del OR | **Análisis:** definido. **Ejecución:** pendiente |
| Orden ascendente/descendente | Q-003 Orden determinista | `OrderBy` / `OrderByDescending` | Secuencia estable esperada | Orden no aplicado o no determinista | **Análisis:** definido. **Ejecución:** pendiente |
| Paginación básica | Q-004 Skip/Take | `skip = n`, `take = m` | Devuelve ventana correcta y tamaño esperado | Off-by-one en página/índices | **Análisis:** definido. **Ejecución:** pendiente |
| Paginación con orden obligatorio | Q-005 Orden + página | Dataset > 1 página | Misma página produce mismos resultados entre ejecuciones | Página inestable por falta de orden previo | **Análisis:** definido. **Ejecución:** pendiente |
| Query materializada una sola vez | Q-006 Evitar evaluación prematura | Composición + ToListAsync final | Sin materializaciones intermedias inesperadas | Coste extra / resultados parciales | **Análisis:** definido. **Ejecución:** pendiente |

### 3) Unit of Work (commit, rollback lógico, cancelación, errores)

| Requisito | Caso | Entrada | Resultado esperado | Defecto que detectaría | Comprobación |
|---|---|---|---|---|---|
| Commit exitoso | U-001 SaveChanges confirma cambios | Cambios válidos en 1+ repositorios | `Commit` persiste todo en una transacción lógica | Cambios no persistidos pese a commit OK | **Análisis:** definido. **Ejecución:** pendiente |
| Sin commit no hay persistencia confirmada | U-002 Cambios sin confirmar | Add/Remove sin commit | Estado final sin cambios confirmados (según diseño) | Persistencia accidental sin UoW | **Análisis:** definido. **Ejecución:** pendiente |
| Error durante commit | U-003 Manejo de excepción de persistencia | Violación de restricción / error DB | Excepción propagada o envuelta según contrato; estado coherente | Estado parcial inconsistente | **Análisis:** definido. **Ejecución:** pendiente |
| Rollback lógico (si diseño lo contempla) | U-004 Descartar cambios pendientes | Operaciones previas + rollback lógico/dispose | Cambios pendientes no se confirman | Rollback no efectivo | **Análisis:** definido. **Ejecución:** pendiente |
| Cancelación en commit | U-005 Commit cancelable | `CommitAsync(tokenCancelado)` | Responde a cancelación sin confirmar cambios | Ignorar token en SaveChangesAsync | **Análisis:** definido. **Ejecución:** pendiente |
| Reuso indebido de UoW | U-006 UoW disposed no reutilizable | Llamadas tras dispose | Lanza `ObjectDisposedException` o contrato equivalente | Uso después de dispose silencioso | **Análisis:** definido. **Ejecución:** pendiente |

## Cobertura mínima recomendada (unit/integration)

### Unit tests (rápidos, por contrato)
- Guard clauses (`null`, argumentos inválidos, dispose).
- Delegación correcta de repositorio/UoW (llamadas esperadas, token propagado).
- Reglas de composición de query cuando estén encapsuladas en métodos propios (sin acoplar a SQL literal).

### Integration tests (obligatorios para BASE-002)
- **Repositorio + SQLite real** (preferible `DataSource=:memory:` con conexión abierta por test para aislamiento).
- Traducción real de LINQ a SQL (filtros combinados, orden, paginación).
- Comportamiento real de `CommitAsync` y errores de persistencia.
- Escenarios de concurrencia/cancelación básicos en operaciones async.

### Reparto mínimo sugerido
- 30-40% unit (contratos y validaciones).
- 60-70% integration (comportamiento EF Core/SQLite real). 

## Riesgos de falsos positivos y control de datos de prueba

1. **Proveedor inadecuado para validar consultas**
   - Riesgo: usar doubles/mocks y no detectar diferencias de traducción SQL.
   - Mitigación: pruebas clave sobre SQLite real.

2. **Uso de InMemory provider como sustituto de SQLite**
   - Riesgo: pasar tests que fallarán en SQL real (comparaciones, nulls, ordering, funciones).
   - Mitigación: reservar InMemory solo para unit de contrato, no para validación de query.

3. **Datos de prueba poco discriminantes**
   - Riesgo: predicados incorrectos siguen pasando por dataset trivial.
   - Mitigación: seeds con casos positivos, negativos y borde (duplicados, nullables, valores límite).

4. **Orden no determinista en paginación**
   - Riesgo: tests intermitentes por falta de `OrderBy` previo.
   - Mitigación: exigir orden explícito antes de `Skip/Take`.

5. **Aislamiento deficiente entre pruebas**
   - Riesgo: contaminación de datos y falsos verdes/rojos.
   - Mitigación: base aislada por test, reinicialización de schema, fixture controlada.

## Propuestas para Implementador/alumno
- Definir contrato explícito de lectura en `IRepository<T>` para predicados (p. ej., `GetAsync`/`ListAsync` con `Expression<Func<T,bool>>`) antes de codificar tests.
- Definir contrato explícito de `IUnitOfWork` (`CommitAsync`, comportamiento de rollback lógico y dispose).
- Priorizar primero integración de repositorio/UoW con SQLite y luego unit tests de guard clauses.
- Mantener trazabilidad caso↔test usando IDs `R-*`, `Q-*`, `U-*` en nombre de test.

## Supuestos
- BASE-002 incorporará EF Core y proveedor SQLite en Infrastructure.
- Se mantendrá arquitectura por capas y async/await con `CancellationToken`.
- El diseño final puede ajustar nombres de métodos, pero debe preservar los comportamientos verificables aquí definidos.

## Evidencia ejecutada por otros
No se aportaron salidas de ejecución (`dotnet build`, `dotnet test`) para BASE-002 en este paso.

## Comprobaciones pendientes
1. Confirmar contrato final de `IRepository<T>` para operaciones de lectura con predicados.
2. Confirmar contrato final de `IUnitOfWork` (commit, rollback lógico/dispose, cancelación).
3. Implementar pruebas según matriz R/Q/U y ejecutar:
   - `dotnet build CursoNETIA.slnx`
   - `dotnet test CursoNETIA.slnx`
4. Reportar resultados reales por suite y mapear fallos contra IDs de caso.

## Conclusiones
- Con el estado actual, la estrategia de pruebas para BASE-002 debe centrarse en integración EF Core + SQLite para evitar falsos positivos.
- La matriz propuesta cubre comportamiento mínimo esperado de repositorio genérico, composición `IQueryable` y Unit of Work.
- No es posible afirmar cumplimiento hasta contar con implementación y evidencia de ejecución real.

## Siguiente paso recomendado
Pasar al Implementador para definir contratos BASE-002 (repositorio de lectura + Unit of Work), implementar y luego ejecutar la matriz de casos R/Q/U con evidencia de resultados.

## Ruta guardada
`docs/informes/BASE-002/2B-pruebas.md`

---

## Evaluación posterior

### Contexto
Se evalúa la implementación reportada en `docs/informes/BASE-002/3-implmentacion.md`, conservando el análisis inicial anterior de este informe.

### Archivos adicionales consultados en esta evaluación posterior
- `docs/informes/BASE-002/3-implmentacion.md`
- `src/AulaPedidos.Application/Contracts/Persistence/IRepository.cs`
- `src/AulaPedidos.Application/Contracts/Persistence/IUnitOfWork.cs`
- `src/AulaPedidos.Infrastructure/Persistence/AulaPedidosDbContext.cs`
- `src/AulaPedidos.Infrastructure/Persistence/EfRepository.cs`
- `src/AulaPedidos.Infrastructure/Persistence/EfUnitOfWork.cs`
- `src/AulaPedidos.Infrastructure/DependencyInjection.cs`
- `src/AulaPedidos.Infrastructure/AulaPedidos.Infrastructure.csproj`
- `src/AulaPedidos.Api/appsettings.json`
- `src/AulaPedidos.Api/appsettings.Development.json`
- `tests/AulaPedidos.ArchitectureTests/UnitTest1.cs`
- `tests/AulaPedidos.Infrastructure.Tests/AulaPedidos.Infrastructure.Tests.csproj`
- `tests/AulaPedidos.Infrastructure.Tests/UnitTest1.cs`

### Evaluación de implementación contra matriz de pruebas BASE-002

| Requisito | Caso | Entrada | Resultado esperado | Comprobación |
|---|---|---|---|---|
| Repositorio genérico: alta + commit | R-001 | Entidad válida + `SaveChangesAsync` | Persistencia confirmada | **Código de test existe:** `EfRepository_PredicateQueriesAndCommit_WorkAsExpected`. **Ejecución independiente:** pendiente |
| Repositorio genérico: baja + commit | R-002 | Entidad existente + `RemoveAsync` + commit | Entidad eliminada tras commit | **Código de test existe:** `EfRepository_RemoveAndSaveChangesAsync_PersistsDeletion`. **Ejecución independiente:** pendiente |
| Gets por predicado | R-003 / R-004 | Predicados con y sin match | Filtrado correcto y vacío cuando no hay match | **Código de test existe:** `FirstOrDefaultAsync`, `ListAsync`, `AnyAsync`, `CountAsync` validados en test de integración. **Ejecución independiente:** pendiente |
| Guard clause de predicado nulo | R-005 | `predicate = null` | Excepción explícita (`ArgumentNullException`) | **Análisis de código:** `EfRepository` valida `predicate`, pero **no hay test dedicado** |
| Cancelación en repositorio | R-006 | `CancellationToken` cancelado | `OperationCanceledException` cuando aplica | **Código de test existe:** caso de cancelación en `ListAsync`. **Ejecución independiente:** pendiente |
| Composición AND/OR de filtros | Q-001 / Q-002 | Predicados combinados | Traducción y resultados correctos | **Cobertura parcial:** hay filtro con `Contains`, pero **no hay casos explícitos AND/OR** |
| Ordenamiento | Q-003 | `OrderBy`/`OrderByDescending` | Secuencia determinista | **Sin cobertura observada** |
| Paginación | Q-004 / Q-005 | `Skip/Take` (si aplica diseño) | Ventana correcta y estable | **No aplica aún en contrato actual** (no hay API de paginación en `IRepository`); **sin cobertura** |
| Materialización controlada | Q-006 | Composición y materialización final | Sin evaluación prematura no deseada | **Sin cobertura explícita** |
| UoW commit | U-001 | Cambios válidos + `SaveChangesAsync` | Persiste cambios | **Código de test existe:** commit validado. **Ejecución independiente:** pendiente |
| UoW sin commit | U-002 | Add/Remove sin commit | No hay confirmación persistida | **Sin cobertura explícita** |
| UoW error persistencia | U-003 | Error de DB/restricción | Excepción + estado coherente | **Sin cobertura explícita** |
| Rollback lógico/dispose | U-004 | Descartar cambios pendientes | Cambios no confirmados | **No implementado como contrato explícito**; **sin cobertura** |
| UoW cancelación | U-005 | `SaveChangesAsync(tokenCancelado)` | Cancela sin confirmar | **Código de test existe**. **Ejecución independiente:** pendiente |
| Reuso tras dispose | U-006 | Uso después de `Dispose` | Error esperado (`ObjectDisposedException` o contrato) | **Sin cobertura explícita** |

### Evidencia ejecutada por otros
- En `docs/informes/BASE-002/3-implmentacion.md` se reporta:
  - `dotnet build CursoNETIA.slnx` OK.
  - `dotnet test tests/AulaPedidos.Infrastructure.Tests/...` OK (7/7).
  - `dotnet test CursoNETIA.slnx` OK (18 passed, 0 failed).
- **Estado en esta evaluación:** evidencia **reportada por implementador**, no reejecutada ni verificada de forma independiente en este paso.

### Conclusiones de la evaluación posterior
- La implementación **sí cubre el núcleo** de BASE-002: contrato de repositorio con predicados, `IUnitOfWork.SaveChangesAsync`, implementación EF Core + SQLite y registro DI.
- La suite actual valida parte importante de `R-*` y `U-*`, pero quedan huecos frente a la matriz original: `R-005`, `Q-001..Q-006` (parcial/sin cobertura), `U-002`, `U-003`, `U-006`.
- El alcance de paginación/orden no está expuesto en contrato público actual; por tanto, esos casos quedan como **pendientes de diseño futuro** o de pruebas sobre consultas de aplicación específicas.

### Propuestas
1. Añadir pruebas de guard clauses explícitas para predicados nulos (`FirstOrDefaultAsync`, `ListAsync`, `AnyAsync`, `CountAsync`).
2. Añadir al menos un caso de composición AND y otro OR en integración SQLite.
3. Añadir prueba UoW “sin commit no persiste” y prueba de excepción de persistencia controlada.
4. Mantener trazabilidad con IDs `R-*`, `Q-*`, `U-*` en nombres de tests para facilitar reevaluaciones.

### Pruebas faltantes a agregar por Implementador

Archivo objetivo principal: `tests/AulaPedidos.Infrastructure.Tests/UnitTest1.cs`.

| Caso | Nombre sugerido del test | Tipo | Entrada clave | Resultado esperado |
|---|---|---|---|---|
| R-005 | `R005_FirstOrDefaultAsync_WithNullPredicate_ThrowsArgumentNullException` | Integración | `predicate = null` | Lanza `ArgumentNullException` |
| R-005 | `R005_ListAsync_WithNullPredicate_ThrowsArgumentNullException` | Integración | `predicate = null` | Lanza `ArgumentNullException` |
| R-005 | `R005_AnyAsync_WithNullPredicate_ThrowsArgumentNullException` | Integración | `predicate = null` | Lanza `ArgumentNullException` |
| R-005 | `R005_CountAsync_WithNullPredicate_ThrowsArgumentNullException` | Integración | `predicate = null` | Lanza `ArgumentNullException` |
| Q-001 | `Q001_ListAsync_WithCombinedAndPredicate_ReturnsExpectedRows` | Integración SQLite | Datos semilla con casos que cumplen A y B y otros que no | Devuelve solo filas que cumplen A && B |
| Q-002 | `Q002_ListAsync_WithCombinedOrPredicate_ReturnsExpectedRows` | Integración SQLite | Datos semilla con casos que cumplen A o B | Devuelve filas que cumplen A \|\| B |
| U-002 | `U002_AddAsync_WithoutSaveChanges_DoesNotPersist` | Integración SQLite | `AddAsync` sin `SaveChangesAsync` y nueva scope de lectura | No existe el registro agregado |
| U-003 | `U003_SaveChangesAsync_WhenConstraintIsViolated_ThrowsDbUpdateException` | Integración SQLite | Entidad de prueba con índice único + insert duplicado | `DbUpdateException` (o derivada) |
| U-006 | `U006_SaveChangesAsync_AfterScopeDisposed_ThrowsObjectDisposedException` | Integración | Resolver UoW en scope, cerrar scope y reutilizar instancia | Lanza `ObjectDisposedException` o excepción de contrato documentada |

#### Datos de prueba mínimos recomendados para los casos faltantes
- Reutilizar SQLite en memoria con conexión abierta por prueba (`Data Source=:memory:`).
- Para Q-001/Q-002 sembrar al menos 4 filas con combinaciones diferenciables (cumple A/B, solo A, solo B, ninguna).
- Para U-003 crear `DbContext` de prueba con `HasIndex(...).IsUnique()` en entidad local de test.
- Aislar cada prueba con su propio `ServiceProvider`/scope para evitar contaminación de estado.

#### Criterio de terminado de esta ampliación
1. Todas las pruebas de la tabla anterior existen con trazabilidad de ID (`R005`, `Q001`, etc.).
2. `dotnet test CursoNETIA.slnx` pasa sin fallos.
3. El Implementador adjunta salida real (total/passed/failed) para reevaluación final.

### Supuestos
- BASE-002 mantiene UoW mínimo sin transacciones explícitas ni rollback programático.
- Paginación y ordenación se validarán cuando exista API de aplicación que las exponga como comportamiento requerido.

### Comprobaciones pendientes
1. Verificación independiente de ejecución de tests (`dotnet test CursoNETIA.slnx`) en sesión de pruebas separada.
2. Completar cobertura faltante priorizando `R-005`, `U-002`, `U-003` y composición de filtros `Q-001/Q-002`.

### Siguiente paso recomendado
Pasar al Implementador para cerrar la cobertura faltante prioritaria y devolver salida real de ejecución para cierre final de pruebas BASE-002.
