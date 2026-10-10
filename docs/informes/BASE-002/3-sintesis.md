# BASE-002 — Informe 3 (Síntesis)

## Objetivo
Consolidar los informes 2A (Arquitectura) y 2B (Pruebas) para definir un alcance recomendado de implementación de **Repository Pattern + Unit of Work** sobre BASE-001, con contratos en Application e implementación EF Core + SQLite en Infrastructure.

## Fuentes
- `docs/informes/BASE-002/2A-arquitectura.md`
- `docs/informes/BASE-002/2B-pruebas.md`

## Acuerdos
1. La implementación debe mantener la arquitectura por capas (Application define contratos; Infrastructure implementa; Api compone DI).
2. Debe existir contrato de **Unit of Work** en Application con commit asíncrono y `CancellationToken`.
3. El repositorio debe priorizar lecturas por predicado (`Expression<Func<TEntity, bool>>`) y ejecución asíncrona.
4. La persistencia objetivo para BASE-002 es **EF Core + SQLite**.
5. La validación clave debe apoyarse en pruebas de integración reales (no solo mocks) para evitar falsos positivos en LINQ/SQL.

## Diferencias
1. **Exposición de `IQueryable`**:
   - 2A: la considera opción preferida si se separa contrato de lectura y se documenta su uso.
   - 2B: exige validar composición (filtros, orden y paginación) si `IQueryable` forma parte del diseño final.
2. **Alcance transaccional inicial**:
   - 2A: propone `SaveChangesAsync` mínimo para BASE-002 y deja transacciones avanzadas como opcional futuro.
   - 2B: contempla casos de rollback lógico/dispose condicionados a lo que se defina en contrato final.
3. **Profundidad de pruebas**:
   - 2A: añade foco en pruebas de arquitectura y DI.
   - 2B: prioriza matriz funcional R/Q/U con integración SQLite y mapeo de resultados por caso.

## Alcance recomendado
1. **Application**
   - Mantener/ajustar contrato de repositorio para incluir lecturas por predicado y operaciones auxiliares de consulta asíncronas.
   - Introducir `IUnitOfWork` con `SaveChangesAsync(CancellationToken)` como frontera mínima de commit.
2. **Infrastructure**
   - Incorporar `DbContext` EF Core.
   - Implementar repositorio genérico EF y Unit of Work EF.
   - Configurar SQLite y registro DI scoped coherente.
3. **Api**
   - Componer dependencias vía `AddInfrastructure(builder.Configuration)` sin lógica de persistencia en endpoints.
4. **Pruebas**
   - Integración en Infrastructure con SQLite (preferible in-memory con conexión abierta por prueba).
   - Cobertura mínima de casos de predicados, composición de consultas y commit/cancelación de UoW.
   - Verificación de arquitectura/DI según patrones existentes del repositorio.

## Casos de aceptación
1. Contratos de Application compilan y no dependen de EF Core.
2. Repositorio permite lecturas por predicado con comportamiento correcto en coincidencia, no coincidencia y cancelación.
3. Si se expone composición consultable, filtros combinados + orden + paginación producen resultados deterministas en SQLite.
4. `IUnitOfWork.SaveChangesAsync` confirma cambios esperados y respeta `CancellationToken`.
5. DI resuelve `DbContext`, repositorio y UoW en Api/Infrastructure sin romper capas.
6. `dotnet build CursoNETIA.slnx` y `dotnet test CursoNETIA.slnx` pasan con evidencia real.

## Pendientes
1. Decisión humana: confirmar si se expondrá `IQueryable` en contrato público o solo métodos de consulta por predicado.
2. Decisión humana: confirmar si BASE-002 incluye únicamente commit mínimo o también superficie explícita de rollback/transacciones.
3. Implementar y ejecutar la matriz de pruebas R/Q/U con reporte por caso.

## Propuesta vs decisión humana
- **Propuesta de esta síntesis**: avanzar con UoW mínimo (`SaveChangesAsync`), repositorio con predicados y validación fuerte en integración SQLite; permitir composición consultable solo si queda explícitamente acotada en contrato y documentación.
- **Decisión humana requerida**: alcance final de `IQueryable` y de capacidades transaccionales adicionales para BASE-002.

## Prompt autocontenido para Implementador
```text
Implementa BASE-002 sobre BASE-001 con alcance acotado:

Objetivo:
- Repository Pattern + Unit of Work.
- Contratos en Application.
- Implementación EF Core + SQLite en Infrastructure.
- Repositorio reutilizable con énfasis en gets por predicado (`Expression<Func<TEntity, bool>>`) y composición consultable solo si se aprueba explícitamente.

Requisitos técnicos:
1) Application:
   - Ajustar/crear contratos de repositorio de lectura/escritura con async + CancellationToken.
   - Crear `IUnitOfWork` con `SaveChangesAsync(CancellationToken)`.
   - Evitar referencias a EF Core en contratos.
2) Infrastructure:
   - Crear `DbContext` EF Core.
   - Implementar repositorio genérico EF y UoW EF.
   - Configurar SQLite y registrar DI scoped.
3) Api:
   - Mantener composición en Program/DI sin lógica de persistencia en endpoints.
4) Pruebas:
   - Integración con SQLite para predicados, composición (si aplica), commit/cancelación.
   - Mantener/actualizar pruebas de arquitectura y DI según convenciones del repo.
5) Validación:
   - Ejecutar build/test de la solución y reportar resultados reales.

Entrega esperada:
- Resumen por archivo modificado.
- Decisiones tomadas y pendientes.
- Evidencia de pruebas vinculada a casos de aceptación.
```

## Ruta guardada
`docs/informes/BASE-002/3-sintesis.md`