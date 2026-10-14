# TAREA-003 — Pruebas (fase inicial)

- ID: TAREA-003 (alias Producto-003) — CRUD de productos
- Rol: Pruebas. Fase: pruebas-inicial. Análisis de diseño; no se ejecutó nada.
- Issue: vzzdaniel/CursoNetIA#2 (https://github.com/vzzdaniel/CursoNetIA/issues/2)

## Archivos consultados
docs/informes/README.md, docs/informes/indice.md, docs/expedientes/Producto-003.md, docs/github-pr.md, src/AulaPedidos.Api/Program.cs y el cuerpo del issue #2 (requisitos vigentes: Id, Price, Description, AlternateDescription; sustituyen Sku/Name del expediente histórico). Los comentarios del issue se consultaron después de concluir este análisis.
No localizados: Product.cs en Domain, AppDbContext.cs, Repository.cs.

## Estado observado (análisis del código)
Program.cs sólo registra Mediator, Application, Infrastructure y /health. No hay endpoints /products. No se encontró código de Product: la implementación parece pendiente. Evidencia ejecutada por otros: ninguna. Tests ejecutados: ninguno.

## Casos propuestos
Esperados derivados del issue #2. Decisiones humanas: Description obligatoria, AlternateDescription opcional, Price ≥ 0. Límite 200 y precisión (18,2) son propuesta del Coordinador, no decisión humana.

| Requisito | Caso | Entrada | Resultado esperado | Comprobación (defecto detectado) |
|---|---|---|---|---|
| Dominio | D1 | Description " Lápiz ", Alternate " Pencil " | Valores recortados; Id no vacío | Unit (no normaliza) |
| Dominio | D2 | Description null, "" o "   " | Excepción de dominio | Unit (acepta vacíos) |
| Dominio | D3 | Price -0.01 / 0 | Rechaza / acepta | Unit (límite mal) |
| Dominio | D4 | Description 200 / 201 caracteres; Alternate 200 / 201 | Acepta / rechaza | Unit (límite off-by-one) |
| Dominio | D5 | Alternate null, "", "   " | Se guarda null; no falla | Unit (guarda vacío o rechaza) |
| Create | A1 | Comando válido | Id Guid no vacío; persistido; SaveChanges una vez | Handler con repo/UoW real SQLite |
| Create | A2 | Comando inválido | Error de validación; sin guardar | Handler (guarda antes de validar) |
| Update | A3 | Id existente, datos nuevos | Id conservado; datos persistidos | Handler (cambia Id o no guarda; AsNoTracking sin Update) |
| Update/Delete/GetById | A4 | Id inexistente | NotFound | Handler |
| Delete | A5 | Id existente | Eliminado; GetById posterior NotFound | Handler |
| GetAll | A6 | BD vacía / varios | [] / orden estable documentado | Handler |
| Repositorio | R1 | Add, consulta, Update, Remove en SQLite aislado | Persisten; consultas AsNoTracking | Integración Infrastructure |
| Esquema | R2 | Arranque sobre BD existente | Tabla Products creada sin borrar datos | Integración (EnsureDeleted o recreación) |
| Persistencia | P1 | Price 0 y 12.34; Alternate null | Round-trip exacto; null conservado | Integración SQLite (decimal como TEXT, precisión) |
| HTTP | H1 | POST válido | 201, DTO y Location a /products/{id} | WebApplicationFactory con BD aislada |
| HTTP | H2 | POST Description vacía / >200 / precio negativo | 400 ProblemDetails; nada guardado | Integración |
| HTTP | H3 | GET /products vacío | 200 [] | Integración |
| HTTP | H4 | GET/PUT/DELETE id inexistente | 404 | Integración |
| HTTP | H5 | PUT válido; DELETE existente | 200 DTO / 204 y luego 404 | Integración |
| HTTP | H6 | GET /products/abc | Comportamiento documentado (404 por ruta o 400) | Integración |
| Cancelación | C1 | Token cancelado | OperationCanceledException propagada, sin cambios | Handler/endpoint |
| Arquitectura | AR1 | Referencias | Domain sin EF/HTTP; Application sin Infrastructure | Tests de arquitectura existentes |
| Errores | E1 | Fallo técnico | ProblemDetails sin stack trace | Integración |

## Criterios de aceptación
Casos D, A, R, P, H, C y AR pasan.

## Riesgos y pendientes
- Sin implementación localizada: todo está pendiente de ejecutar (dotnet build y dotnet test CursoNETIA.slnx).
- Sin unicidad: el issue no la pide; no se proponen casos de SKU.
- Pendiente: orden del listado (no definido) y GUID mal formado.
- No se leyeron tests existentes ni el paquete BASE-001.
- Tests de integración deben usar SQLite aislado.

## Evaluación posterior
Registro inicial conservado: «No realizada: no hay evidencia de implementación ni de ejecución».

### Seguimiento posterior — TAREA-003 · Pruebas · 2026-10-14

Objetivo: evaluar el CRUD ya implementado contra el cuerpo vigente del issue #2. Fecha de referencia de la evidencia remota: 2026-10-14 UTC. No se ejecutaron comandos ni se modificaron código o tests en esta evaluación.

**Conclusión independiente:** el diseño y los tests inspeccionados cubren el CRUD, validación, persistencia y errores principales. No se identificó un defecto funcional confirmado. Hay lagunas de comprobación concretas; no equivalen a tests fallidos ni a aprobación de la tarea. Esta sección sustituye el estado de implementación pendiente del análisis inicial, que permanece como histórico.

### Fuentes realmente consultadas en este seguimiento

- Guías: `docs/informes/README.md`, `docs/informes/indice.md`, `docs/multiagente-copilot.md`, `docs/github-pr.md` e instrucciones aportadas al chat.
- Requisitos: cuerpo verificado de https://github.com/vzzdaniel/CursoNetIA/issues/2 y `docs/products.md`.
- Domain: `src/AulaPedidos.Domain/Products/Product.cs`.
- Application: `src/AulaPedidos.Application/Products/{CreateProduct,UpdateProduct,DeleteProduct,GetProductById,GetProducts}.cs` y `src/AulaPedidos.Application/AulaPedidos.Application.csproj`.
- Infrastructure: `src/AulaPedidos.Infrastructure/Persistence/{EfRepository,EfUnitOfWork,AulaPedidosDbContext,ProductConfiguration}.cs` y `Migrations/20261014004616_AddProducts.cs` en esa misma carpeta.
- Api: `src/AulaPedidos.Api/Program.cs`, `Endpoints/ProductEndpoints.cs` y `Errors/ProductValidationExceptionHandler.cs` bajo Api.
- Tests: `tests/AulaPedidos.Application.Tests/ProductTests.cs`, `tests/AulaPedidos.Infrastructure.Tests/ProductPersistenceTests.cs`, `tests/AulaPedidos.Api.IntegrationTests/{ProductEndpointTests,ProductApiFactory}.cs` y `tests/AulaPedidos.ArchitectureTests/UnitTest1.cs`.
- Evidencia ajena: `docs/informes/TAREA-003/05-implementacion.md`, `artifacts/tarea-003-followup/build.log`, cabeceras de `artifacts/CoverageReport-final/Cobertura.xml` y `artifacts/CoverageRaw-final/Cobertura.xml`.
- Metadatos y checks del PR #3 mediante MCP; no se usaron conclusiones del Arquitecto para formar esta evaluación. Los comentarios del issue se consultan sólo después de guardar, para deduplicación.

### Análisis del código y correcciones de supuestos iniciales

- `Product.cs:22-45` valida antes de asignar estado: rechaza Description vacía/espacios y >200, AlternateDescription >200 y precio negativo; normaliza las descripciones y permite cero.
- `UpdateProduct.cs:15-22` actualiza una entidad rastreada y guarda con UnitOfWork. `EfRepository.cs:18,27` **no** usa AsNoTracking; la premisa inicial R1 no describe este repositorio. No introducir AsNoTracking sin resolver la actualización. El expediente histórico no demuestra ese contrato.
- `docs/products.md` explicita que el listado no garantiza orden. El issue no exige orden estable: la parte de A6 sobre orden era propuesta, no criterio bloqueante.
- `ProductEndpoints.cs:22-35` restringe los identificadores a GUID y distingue ausencia: GUID mal formado →404. La documentación define 404 antes de validar propiedades en PUT inexistente.
- `Program.cs:31-35` aplica migraciones al arrancar; `AddProducts.cs:12-26` es aditiva. No se observó borrado/recreación en la ruta de arranque. No se ejecutó rollback.
- SQLite guarda Price como TEXT; precisión (18,2) no impone máximo ni redondeo. No inventar esos límites en los esperados.

### Evidencia ejecutada por otros frente a verificación pendiente

| Fuente | Evidencia disponible | Alcance y límite |
|---|---|---|
| Log de compilación leído | Build succeeded; 0 advertencias, 0 errores | Ejecución ajena, Release; no compilación nueva de este perfil |
| Informe 05 y cuerpo del PR #3 | Registran 49/49 correctos, sin omitidos: Domain/Application 10, Architecture 4, Infrastructure 18, API 17 | No se abrieron los TRX individuales; cifras atribuidas al Implementador, no resultados reejecutados ni comprobados directamente aquí |
| XML mantenido leído | 236/239 líneas, 98.74 % | Resultado retenido; exclusión únicamente obj/ declarada por el Implementador, no se recombinó cobertura aquí |
| XML completo leído | 445/827 líneas, 53.81 % | Reporte sin filtro; no ocultar el denominador distinto ni confundirlo con el mantenido |
| MCP, PR #3 | Head `733ca68237cd7fdb8a0df893f1c7bde4be9f8568`, open/draft, 0 checks; status pending, 0 statuses | No hay ejecución remota aprobada; informe/PR afirman código igual a `bcf5692`, sin comparación independiente de todos los archivos locales/remotos |

El umbral ≥80 % de código mantenido está respaldado por el XML retenido, no por un check remoto del head. Los artefactos son locales; su vinculación al código vigente procede del registro del Implementador. No se considera verificada la seguridad remota ni la revisión automática.

### Matriz de comprobación posterior

Todos los casos de esta tabla son revisión de código/test o propuestas pendientes; no ejecuciones propias.

| Requisito | Caso | Entrada | Resultado esperado | Comprobación y defecto que detectaría |
|---|---|---|---|---|
| Crear y recuperar | EP1 | POST precio 12.34 y descripciones con espacios; GET Location | 201, Guid no vacío, Location, datos normalizados y persistidos | `ProductEndpointTests.cs:14-43` y `ProductPersistenceTests.cs:12-46`; diseño cubierto; detecta guardado omitido o DTO distinto |
| Actualizar/eliminar | EP2 | PUT precio cero; DELETE del Id creado | 200, mismo Id y nuevos datos; 204 y GET 404 | Tests CRUD anteriores; detecta mutación no persistida o baja ficticia |
| Validación y opcionalidad | EP3 | Description null/vacía/espacios; límites 200/201; Alternate ausente/espacios; precio -0.01/0 | Rechazo de inválidos sin cambios; acepta cero y alternativa null | `ProductTests.cs:7-49`, `ProductEndpointTests.cs:46-91`; cubierto por diseño, detecta límites incorrectos o cambios parciales |
| PUT inválido atómico | EP4 | Sobre producto guardado: Description null/espacios/201 o Alternate 201, con precio válido y otros datos nuevos | 400 ProblemDetails; GET conserva TODOS los valores originales | **Pendiente específico HTTP**: el PUT inválido actual prueba sólo precio negativo (`ProductEndpointTests.cs:73`); detecta validación dispar o persistencia parcial |
| Listar/ausencia | EP5 | Base vacía; varios productos; Guid aleatorio inexistente y abc | 200 [] o todos los DTO sin asumir orden; 404 GET/PUT/DELETE ausentes o GUID mal formado | Vacío y un elemento cubiertos; añadir varios e Id aleatorio (el test HTTP usa Guid.Empty); detecta listado incompleto |
| Persistencia decimal | EP6 | POST/PUT 0.001m y decimal.MaxValue; GET con nuevo contexto | Round-trip decimal exacto, sin máximo ni redondeo inventados | **Pendiente de borde**: sólo 12.34 y cero comprobados en tests leídos; detecta truncamiento o límite accidental de HasPrecision |
| Migración segura | EP7 | SQLite aislado con tabla Legacy y registro previo; MigrateAsync dos veces | Conserva registro ajeno, crea Products, sin cambios de modelo pendientes | `ProductPersistenceTests.cs:58-82`; diseño cubierto; detecta migración destructiva o no idempotente |
| Cancelación | EP8 | Token ya cancelado en los cinco handlers | OperationCanceledException; sin cambios rastreados | `ProductPersistenceTests.cs:48-55`; cubierto por diseño; detecta ignorar cancelación inicial |
| Cancelación durante I/O | EP9 | Repositorio/UoW de prueba captura token y bloquea hasta cancelar después de iniciar llamada | Mismo token llega a I/O; operación cancelada; ningún guardado nuevo después de cancelar | **Pendiente**: EP8 termina en ThrowIfCancellationRequested y no prueba propagación; detecta usar default en I/O. El código sí pasa el token |
| Errores técnicos | EP10 | IUnitOfWork falla en Production; JSON mal formado | 500 genérico sin detalles y sin persistencia; JSON inválido 400 | `ProductEndpointTests.cs:108-150`; diseño cubierto; detecta exposición de excepción o guardado prematuro |
| Capas | EP11 | Referencias de Domain y Application | Domain sin capas externas/EF/HTTP; Application sin Infrastructure/Api/EF | Test Domain y csproj Application coherentes; **pendiente ampliar aserción Application**: test sólo excluye EF, no Infrastructure/Api; detecta regresión de dependencia |

### Hallazgos, riesgos y aceptación

- **Sin fallos funcionales confirmados** en los archivos inspeccionados; no se declara que toda la solución esté probada.
- **Lagunas de pruebas, prioridad media:** EP4 (otras entradas inválidas de PUT), EP6 (decimal fuera de dos decimales) y EP9 (cancelación durante I/O). Sus condiciones y defectos detectables están en la tabla. La ausencia de esos tests no prueba un fallo del producto.
- **Lagunas de prioridad baja:** EP5 con varios productos y Guid aleatorio; EP11 con referencias prohibidas explícitas de Application.
- **Verificación pendiente:** no hay checks del head; recuperar/leer TRX y volver a ejecutar después de ampliar casos antes de cerrar aceptación. No se deduce ausencia de vulnerabilidades de checks vacíos.
- **Riesgo operativo documentado:** migración al iniciar necesita permisos y coordinación multinstancia; tabla Products manual sin historial requiere conciliación humana. No probar sobre almacenamiento compartido ni ejecutar rollback/borrado.

### Pasos concretos para Implementador o alumno

TAREA-003, carpeta `docs/informes/TAREA-003/`; issue `vzzdaniel/CursoNetIA#2`, https://github.com/vzzdaniel/CursoNetIA/issues/2 (no confundir con PR #3).

1. Leer el cuerpo del issue, `docs/products.md` y esta sección de `docs/informes/TAREA-003/03-pruebas.md`. Usar los tests indicados en EP4/EP5/EP6/EP9/EP11 como destinos de ampliación; no se pide cambiar el contrato del producto.
2. Añadir EP4 como Theory y comparar el DTO entero después de cada PUT inválido; añadir EP6 verificando GET desde un contexto/solicitud posterior y EP5 sin depender del orden.
3. Para EP9, usar dobles controlables en tests de handlers que capturen CancellationToken en Repository y UnitOfWork; cancelar después de confirmar entrada en I/O. No confundir cancelación inicial con propagación. Añadir EP11 al test de arquitectura existente.
4. Desde la raíz, en el perfil con permiso de ejecutar: `dotnet build CursoNETIA.slnx -c Release` y `dotnet test CursoNETIA.slnx -c Release --no-build --logger trx --results-directory artifacts/tarea-003-pruebas-posterior`. Usar siempre SQLite aislado.
5. Regenerar cobertura mantenida con exclusión exclusiva obj/ y también reporte completo; registrar resultados reales, rutas TRX y commit en `docs/informes/TAREA-003/05-implementacion.md`. Si aparece fallo, adjuntar entrada, esperado y observado al mismo issue.
6. Pruebas reevaluará las salidas en este mismo `03-pruebas.md`, sin sustituir el diseño inicial ni presentar casos propuestos como ejecutados.

## Publicación en GitHub
comentario: id 6157741033, https://github.com/vzzdaniel/CursoNetIA/issues/2#issuecomment-6157741033; fecha 2026-10-13; estado: publicado.

- Inicial: repositorio `vzzdaniel/CursoNetIA`, issue #2, https://github.com/vzzdaniel/CursoNetIA/issues/2; fase `pruebas-inicial`, marcador `<!-- aulapedidos:TAREA-003:pruebas-inicial -->`. Se conserva el comentario inicial sin reemplazarlo con esta evaluación.
- Posterior: mismo issue; fase `pruebas-posterior`, marcador `<!-- aulapedidos:TAREA-003:pruebas-posterior -->`; fecha de referencia 2026-10-14; estado **publicado**. Comentario id `6158782280`, URL https://github.com/vzzdaniel/CursoNetIA/issues/2#issuecomment-6158782280, confirmados por respuesta MCP. Deduplicación previa: se leyeron los tres comentarios disponibles, sin comentario de esta fase; no se editaron comentarios ajenos ni el inicial.
- Resumen preparado: sin defecto funcional confirmado; diseño CRUD/validación/persistencia cubierto; 49/49 atribuidos al Implementador, build y XML leídos; pendientes EP4/EP6/EP9, extensión EP5/EP11, TRX individuales y CI del head. Informe local `docs/informes/TAREA-003/03-pruebas.md`.
