# 3 — Síntesis BASE-001

## Objetivo
Consolidar y comparar las conclusiones de `2A-arquitectura.md` y `2B-pruebas.md` para definir el alcance recomendado de implementación/verificación de BASE-001.

## Fuentes
- `docs/informes/BASE-001/2A-arquitectura.md`
- `docs/informes/BASE-001/2B-pruebas.md`

## Acuerdos
1. Mantener `CursoNETIA.slnx`, .NET 10, Minimal APIs y arquitectura en cuatro capas.
2. OpenAPI debe quedar solo en Development y el health check en `/health`.
3. Debe existir `IRepository<T>` como contrato, sin almacenamiento ficticio.
4. Debe usarse Mediator de martinothamar (sin MediatR ni mediador propio).
5. No introducir entidades, lógica de negocio, base de datos concreta ni autenticación en BASE-001.

## Diferencias
1. **Estado temporal del proyecto**:
   - 2A refleja una foto inicial con brechas amplias.
   - 2B incluye una evaluación posterior donde ya aparecen extensiones DI, contrato `IRepository<T>` y proyectos de pruebas, pero con ejecución real pendiente.
2. **Profundidad de verificación**:
   - 2A se centra en propuesta arquitectónica y cambios mínimos.
   - 2B define matriz de casos CP-010..CP-017 y riesgos de diseño de pruebas (resolución real DI, firma completa de métodos, evolución de CP-017).
3. **Evidencia de ejecución**:
   - Ambos coinciden en que faltan salidas reales de `dotnet build`/`dotnet test` para cierre.

## Alcance recomendado
1. Cerrar brechas funcionales de BASE-001 en API y composición:
   - `AddApplication(...)` y `AddInfrastructure(...)` usados desde `Program.cs`.
   - `/health` operativo y OpenAPI condicionado por entorno.
2. Consolidar contrato técnico base:
   - `IRepository<T>` con contrato mínimo asíncrono (`AddAsync`, `RemoveAsync`) y `CancellationToken`.
3. Consolidar integración de Mediator:
   - Referencia/registro de martinothamar en la capa esperada.
4. Mantener y completar base de pruebas xUnit:
   - Unitarias/arquitectura/integración alineadas con CP-010..CP-017.
5. Ejecutar verificación real:
   - `dotnet build CursoNETIA.slnx`
   - `dotnet test CursoNETIA.slnx`

## Casos de aceptación
1. La solución conserva cuatro capas (`src/`) en `net10.0`.
2. `Program.cs` compone Application e Infrastructure por extensiones dedicadas.
3. `GET /health` responde correctamente y OpenAPI se expone solo en Development.
4. `IRepository<T>` existe con contrato mínimo esperado, sin implementación de almacenamiento productivo/fake.
5. Mediator registrado corresponde a martinothamar y no existe uso de MediatR.
6. Existen proyectos de pruebas xUnit en la solución y cubren CP-010..CP-017.
7. Se aporta evidencia real de build/test para confirmar cierre.

## Pendientes
1. Ejecutar build/test y adjuntar resultados verificables por proyecto.
2. Mapear fallos (si aparecen) contra CP-010..CP-017.
3. Ajustar pruebas señaladas por riesgo (resolución real de DI, validación de firma en `IRepository<T>`, acotación de CP-017 en futuras fases).

## Propuesta vs decisión humana
- **Propuesta de síntesis**: continuar con implementación mínima de BASE-001 enfocada en DI por capas, health/openapi, contrato `IRepository<T>`, Mediator y cierre de pruebas con evidencia ejecutada.
- **Decisión humana pendiente**: considerar BASE-001 cerrado solo cuando haya evidencia real de ejecución (`build` + `test`) alineada con los casos de aceptación.

## Prompt autocontenido para Implementador
```text
Implementa y/o ajusta BASE-001 con este alcance:

1) Mantén `CursoNETIA.slnx`, .NET 10, Minimal APIs y 4 capas.
2) Asegura composición por extensiones en Api:
   - `AddApplication(...)`
   - `AddInfrastructure(...)`
3) En Program.cs:
   - health check en `/health`
   - OpenAPI solo en Development
4) Consolida `IRepository<T>` como contrato mínimo asíncrono (`AddAsync`, `RemoveAsync`, `CancellationToken`) sin almacenamiento ficticio.
5) Usa Mediator de martinothamar y evita MediatR.
6) Mantén/completa pruebas xUnit para cubrir CP-010..CP-017.
7) Ejecuta y reporta evidencia real:
   - `dotnet build CursoNETIA.slnx`
   - `dotnet test CursoNETIA.slnx`

Restricciones: no introducir entidades, reglas de negocio, base de datos ni autenticación en BASE-001.
Entrega: resumen por archivo, estado de casos de aceptación y evidencia de ejecución.
```
