# TAREA-003 — Implementación del CRUD de Product

- Rol: Implementador. Fecha: 2026-10-13.
- Fuente vigente: [issue #2](https://github.com/vzzdaniel/CursoNetIA/issues/2).
- Contexto leído: instrucciones del repositorio y personales, convención e índice de informes, guía GitHub, flujo multiagente, informes 02/03, código y proyectos existentes. No existe `04-sintesis.md` ni el paquete BASE-001 indicado por instrucciones históricas.
- Rama remota creada mediante MCP: `codex/tarea-003-product-crud`, desde `master` (`1a5a6f3302f036d9ba2be20c6a5032aed7ffe935`).
- Se preservan todos los cambios locales ajenos, incluidos configuración de agentes/CI e Infrastructure/DependencyInjection.cs; no se publican con esta tarea.
- Draft PR: [#3](https://github.com/vzzdaniel/CursoNetIA/pull/3). Primer commit publicado: `2463da2916cd20b4a1c3a530403ec03474486400`.
- Consulta MCP: sin reviews ni check runs en ese commit. Solicitud real de Copilot code review realizada mediante MCP (respuesta sin contenido; finalización pendiente de verificar).

## Primer incremento verificable (histórico; pendientes actualizados debajo)

- Añadidos `Domain/Products/Product.cs`, `ProductValidationException.cs` y `Application.Tests/ProductTests.cs`.
- Id Guid, precio no negativo, descripciones normalizadas, límite 200 y actualización atómica respecto a validación.
- Ejecutado `dotnet build CursoNETIA.slnx --nologo -warnaserror`: correcto, sin advertencias.
- Ejecutado `dotnet test tests/AulaPedidos.Application.Tests/AulaPedidos.Application.Tests.csproj --no-build --nologo`: 10/10 correctos.
- Pendiente: handlers, persistencia/esquema, endpoints, pruebas completas, cobertura, controles remotos y URL del draft PR.

| Control | Estado | Commit y evidencia |
|---|---|---|
| Compilación y analizadores | correcto | Primer incremento local; comando anterior |
| Pruebas | correcto | Primer incremento local; 10 pruebas de Application/Domain |
| Cobertura combinada ≥80 %, reporte sin filtro | pendiente | Sin medición aún |
| NuGet Audit / Dependency Review | pendiente | Sin ejecución acreditada |
| CodeQL / alertas | pendiente | Sin ejecución ni acceso a alertas acreditados |
| Secret scanning / push protection | pendiente | Disponibilidad remota por verificar |
| Copilot code review | pendiente | Se solicitará al abrir el draft |
| GitHub Code Quality | pendiente | Licencia/configuración por verificar |

## Seguimiento — CRUD completo, 2026-10-13

Código publicado mediante MCP en [`bcf5692b8d3a19d9c7e3c1886fcec4cfbdca50ae`](https://github.com/vzzdaniel/CursoNetIA/commit/bcf5692b8d3a19d9c7e3c1886fcec4cfbdca50ae). La publicación posterior de este informe no cambia código, proyectos, pruebas ni dependencias. Esta sección sustituye los pendientes técnicos del primer incremento; conserva su evidencia histórica.

### Qué cambió y archivos afectados

- `src/AulaPedidos.Domain/Products/{Product,ProductValidationException}.cs`: Guid no vacío, precio no negativo, trim, límite 200 y alternativa vacía→null. Validación antes de modificar estado.
- `src/AulaPedidos.Application/Products/{ProductDto,CreateProduct,UpdateProduct,DeleteProduct,GetProductById,GetProducts}.cs`: mensajes y handlers reales de Mediator, DTO y not-found explícito. Propagación de CancellationToken a Repository/UnitOfWork.
- `src/AulaPedidos.Infrastructure/Persistence/{AulaPedidosDbContext,ProductConfiguration}.cs`: DbSet y mapeo EF con longitudes/precisión. Se conserva el contrato y seguimiento del repositorio; la actualización muta la entidad rastreada.
- `src/AulaPedidos.Infrastructure/Persistence/Migrations/20261014004616_AddProducts{,.Designer}.cs` y `AulaPedidosDbContextModelSnapshot.cs`: migración generada por EF 10.0.12, aditiva, probada sobre SQLite con tabla ajena preexistente; no se ejecutó rollback ni se borró/recreó una base compartida.
- `src/AulaPedidos.Api/{Program.cs,AulaPedidos.Api.csproj,Endpoints/ProductEndpoints.cs,Errors/ProductValidationExceptionHandler.cs}`: CRUD `/products`, 201+Location, 200, 204, 404 y 400 ProblemDetails; errores técnicos genéricos en Production. Migración al iniciar. EF Design 10.0.12 con PrivateAssets=all para generar migraciones; sin nuevos frameworks.
- `tests/AulaPedidos.Application.Tests/ProductTests.cs`: reglas de dominio y límites.
- `tests/AulaPedidos.Infrastructure.Tests/ProductPersistenceTests.cs`: handlers con SQLite/Repository/UoW reales, round-trip, cancelación, migración idempotente y conservación de datos ajenos.
- `tests/AulaPedidos.Api.IntegrationTests/{ProductApiFactory,ProductEndpointTests,UnitTest1}.cs`: HTTP CRUD/validación/ausencias/JSON mal formado/fallo técnico y aislamiento SQLite también para health/OpenAPI. Recursos de la fábrica liberados asincrónicamente.
- `tests/AulaPedidos.ArchitectureTests/UnitTest1.cs`: Domain sin referencias a capas externas/HTTP/EF.
- `docs/products.md`: contrato HTTP, persistencia, migraciones y particularidades de SQLite.
- Este informe: evidencia, publicación y pendientes. No se modificaron los informes 02/03 ni el índice registrado.

### Comprobaciones ejecutadas

1. `dotnet build CursoNETIA.slnx --nologo --tl:off -warnaserror -v:minimal`: correcto.
2. `dotnet build CursoNETIA.slnx -c Release --nologo --tl:off -warnaserror -p:RunAnalyzersDuringBuild=true -v:minimal`: 0 advertencias y 0 errores. [Log local](../../../artifacts/build-release.log).
3. `dotnet test CursoNETIA.slnx -c Release --no-build --nologo --tl:off --logger trx --results-directory artifacts/tests-final -v:minimal`: 49/49 correctos (Application/Domain 10, Architecture 4, Infrastructure 18, API 17), sin omitidas. [TRX locales](../../../artifacts/tests-final/). Ejecución Debug previa: 48/48 antes de añadir el caso de fallo técnico.
4. `dotnet restore CursoNETIA.slnx --force-evaluate --nologo -p:NuGetAudit=true -p:NuGetAuditMode=all -p:NuGetAuditLevel=moderate -warnaserror -v:minimal`: salida 0, sin advertencias de vulnerabilidades. [Log local](../../../artifacts/nuget-audit.log). No acredita alertas Dependabot ni Dependency Review remoto.
5. Infrastructure y API, Release `--no-build --collect:"XPlat Code Coverage" --logger trx`, destinos `artifacts/coverage-final/infrastructure` y `artifacts/coverage-final/api`: 18/18 y 17/17 correctos. ReportGenerator 5.5.11 combina los dos reportes Cobertura originales, sin promediar porcentajes.
6. `reportgenerator "-reports:artifacts/coverage-final/infrastructure/*/coverage.cobertura.xml;artifacts/coverage-final/api/*/coverage.cobertura.xml" "-targetdir:artifacts/CoverageRaw-final" "-reporttypes:Cobertura;MarkdownSummaryGithub"`: reporte sin filtro, **445/827 líneas (53.81 %)**, incluidos generados.
7. Misma combinación en `artifacts/CoverageReport-final`, con **único filtro `-filefilters:-**/obj/**`**: **236/239 líneas mantenidas (98.74 %)**. Umbral ≥80 % comprobado leyendo el XML y fallando si resulta inferior. No se excluyen migraciones ni otro código mantenido. [Reporte mantenido local](../../../artifacts/CoverageReport-final/Cobertura.xml) y [reporte completo local](../../../artifacts/CoverageRaw-final/Cobertura.xml). ReportGenerator avisa de fuentes generadas no materializadas; ambos XML conservan sus contadores reales.
8. Generación, sin aplicar a la base de desarrollo: `dotnet tool install dotnet-ef --version 10.0.12 --tool-path artifacts/tools`; `artifacts/tools/dotnet-ef migrations add AddProducts --project src/AulaPedidos.Infrastructure --startup-project src/AulaPedidos.Api --output-dir Persistence/Migrations --no-build`. Herramienta local ignorada, no publicada.
9. Diff remoto revisado antes de abrir el draft y tras el incremento completo. `git fetch` se usó sólo para verificar; toda publicación se realizó con MCP. Los 24 archivos publicados coinciden con los locales normalizando finales de línea y newline final; comparación UTF-8 corregida tras detectar diferencias de decodificación de PowerShell, sin cambiar contenido. `git diff --check master..origin/codex/tarea-003-product-crud`: correcto. Sin archivos ajenos, secretos conocidos, binarios ni artefactos publicados.

Los enlaces de artifacts son **evidencia local ignorada**, no artefactos de Actions ni archivos publicados. No se ejecutó `dotnet run` contra la base de desarrollo/compartida; se inició la API dentro de las pruebas aisladas. El informe no simula revisiones ni aprobación humana.

### Tabla de controles del contenido vigente

Commit de código: `bcf5692b8d3a19d9c7e3c1886fcec4cfbdca50ae`; el informe posterior sólo añade documentación. La descripción del [PR #3](https://github.com/vzzdaniel/CursoNetIA/pull/3) identifica el head final y conserva la distinción entre evidencia local y remota.

| Control | Estado | Commit y enlace de evidencia |
|---|---|---|
| Compilación y analizadores | correcto | bcf5692; [log local](../../../artifacts/build-release.log), 0 advertencias |
| Pruebas | correcto | bcf5692; [TRX locales](../../../artifacts/tests-final/), 49/49 |
| Cobertura combinada ≥80 % | correcto | bcf5692; [mantenido 98.74 %](../../../artifacts/CoverageReport-final/Cobertura.xml), [completo 53.81 %](../../../artifacts/CoverageRaw-final/Cobertura.xml) |
| NuGet Audit | correcto | bcf5692; [log local](../../../artifacts/nuget-audit.log), salida 0 |
| Dependency Review | pendiente | bcf5692; sin checks ni workflow remoto observado; [PR](https://github.com/vzzdaniel/CursoNetIA/pull/3/checks) |
| CodeQL y alertas code scanning | no disponible | bcf5692; sin workflow remoto; herramientas de alertas/configuración no expuestas en esta sesión. No acredita ausencia de alertas |
| Secret scanning | no disponible | bcf5692; herramienta MCP sobre contenido de Program devolvió `Repository does not have GitHub Advanced Security enabled`; no se completó el análisis |
| Push protection / Dependabot / Copilot Autofix | no disponible | Herramientas de consulta de configuración/alertas no expuestas; licencia/habilitación no verificadas; no se activó ningún plan |
| Copilot code review | en curso | [PR #3](https://github.com/vzzdaniel/CursoNetIA/pull/3); llamada MCP al primer incremento sin error ni contenido; consultas posteriores sin reviews/threads. Se solicita para el head final si no aparece solicitud automática |
| GitHub Code Quality | no disponible | Licencia/configuración no verificables con las capacidades expuestas; no se asume habilitado |

### Publicación y pendientes

- Repositorio privado verificado: `vzzdaniel/CursoNetIA`; base real `master`. Rama única: `codex/tarea-003-product-crud`. [Draft PR #3](https://github.com/vzzdaniel/CursoNetIA/pull/3), enlazado con `Closes #2`. No se aprobó, no se marcó ready, no se habilitó auto-merge ni se fusionó.
- La consulta MCP de `.github` en `master` no mostró workflows; la configuración local de CI/seguridad es trabajo ajeno preexistente y no se publica con este CRUD. `get_check_runs` devolvió cero checks y `get_status` pending sin statuses para el primer incremento. Tras el segundo incremento, cero checks/reviews/threads. No hay resultados remotos correctos acreditados.
- Pendientes: finalización real de Copilot code review, CI remoto/Dependency Review/CodeQL y acceso a alertas/configuración de seguridad. Se conserva el draft con estas limitaciones explícitas; no se sustituyen por comentarios a bots ni aprobación simulada.
- El listado no ofrece orden garantizado (no solicitado). Guid mal formado→404. SQLite conserva decimales exactamente como TEXT; HasPrecision(18,2) es metadato y no añade máximo ni redondeo. Estas decisiones están documentadas, no amplían el CRUD.
- La API aplica migraciones al arrancar: necesita escritura en el esquema. Coordinar arranque multinstancia; una tabla Products manual previa sin historial requiere conciliación humana, no borrado automático.
- La rama local se creó sin mover ni descartar cambios ajenos. Los archivos locales quedan sin staging/commit local; los commits de esta tarea se publicaron directamente mediante MCP y se verificaron con fetch. No se incluyeron los cambios ajenos de Infrastructure/DependencyInjection.cs (sólo un comentario), instrucciones, agentes, workflows o informes de otros roles.
