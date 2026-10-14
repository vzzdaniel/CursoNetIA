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
No realizada: no hay evidencia de implementación ni de ejecución.

## Publicación en GitHub
comentario: id 6157741033, https://github.com/vzzdaniel/CursoNetIA/issues/2#issuecomment-6157741033; fecha 2026-10-13; estado: publicado.
