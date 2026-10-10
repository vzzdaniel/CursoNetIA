# Expediente BASE-002

## Objetivo
Implementar **Repository Pattern + Unit of Work** sobre BASE-001, definiendo contratos en Application e implementaciones con **EF Core + SQLite** en Infrastructure, con foco en repositorio reutilizable para lecturas por predicado y posible composición sobre `IQueryable`.

## Estado base
- Base de trabajo: **BASE-001**.
- Framework objetivo: **.NET 10**.
- Arquitectura esperada por capas: Domain / Application / Infrastructure / Api.

## Fuentes consultadas
- `docs/informes/BASE-002/2A-arquitectura.md`
- `docs/informes/BASE-002/2B-pruebas.md`
- `docs/informes/BASE-002/3-sintesis.md`

## Acuerdos consolidados
1. Application define contratos; Infrastructure implementa; Api compone DI.
2. Incluir `IUnitOfWork` con `SaveChangesAsync(CancellationToken)` como mínimo.
3. Priorizar operaciones de lectura por `Expression<Func<TEntity, bool>>` en repositorio reutilizable.
4. Persistencia objetivo: EF Core + SQLite.
5. Validación clave con pruebas de integración reales (SQLite), además de arquitectura/DI.

## Diferencias detectadas (A/B)
1. Exposición de `IQueryable` en contratos (permitirlo o limitarse a métodos por predicado).
2. Alcance transaccional en BASE-002 (solo commit mínimo vs superficies adicionales de rollback/transacción).
3. Profundidad relativa de pruebas (arquitectura/DI vs matriz funcional completa R/Q/U).

## Alcance recomendado para implementación
1. **Application**
   - Ajustar contratos de repositorio para consultas por predicado (async + `CancellationToken`).
   - Crear contrato `IUnitOfWork` con `SaveChangesAsync`.
2. **Infrastructure**
   - Agregar `DbContext` de EF Core.
   - Implementar repositorio genérico EF y UoW EF.
   - Configurar proveedor SQLite y registrar servicios scoped.
3. **Api**
   - Mantener composición en DI (`AddInfrastructure(builder.Configuration)`), sin lógica de persistencia en endpoints.
4. **Pruebas**
   - Integración SQLite para predicados, composición (si aplica), commit/cancelación.
   - Mantener/ajustar pruebas de arquitectura y DI.

## Casos de aceptación
1. Contratos en Application sin dependencia de EF Core.
2. Repositorio con lecturas por predicado funcionando en coincidencia/no coincidencia/cancelación.
3. Si se habilita `IQueryable`, composición de filtros/orden/paginación con resultados deterministas.
4. `IUnitOfWork.SaveChangesAsync` confirma cambios y respeta cancelación.
5. DI resuelve `DbContext`, repositorio y UoW sin romper capas.
6. Validación con `dotnet build CursoNETIA.slnx` y `dotnet test CursoNETIA.slnx` (evidencia real en fase de implementación).

## Pendientes y decisiones humanas
1. Definir si `IQueryable` se expone en contrato público o se limita a métodos de lectura por predicado.
2. Confirmar si BASE-002 incluye solo commit mínimo o también API explícita de rollback/transacción.

## Propuesta vs decisión
- **Propuesta técnica**: avanzar con UoW mínimo + repositorio por predicados + integración SQLite como validación principal.
- **Decisión humana requerida**: alcance final de `IQueryable` y capacidades transaccionales avanzadas.

## Siguiente paso
Pasar al Implementador con `docs/informes/BASE-002/3-sintesis.md` como guía operativa y este expediente como registro consolidado.