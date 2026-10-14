# Productos — TAREA-003

Implementación del [issue #2](https://github.com/vzzdaniel/CursoNetIA/issues/2), con Mediator de martinothamar y el Repository/UnitOfWork existente.

| Método | Ruta | Resultado |
|---|---|---|
| POST | `/products` | 201, DTO y `Location: /products/{id}` |
| GET | `/products` | 200, array; vacío `[]`; sin contrato de orden |
| GET | `/products/{id}` | 200 DTO o 404 |
| PUT | `/products/{id}` | 200 DTO conservando Id o 404 |
| DELETE | `/products/{id}` | 204 o 404 |

POST/PUT reciben `price` (decimal), `description` y `alternateDescription` (opcional). Los DTO incluyen también `id` (Guid). Description es obligatoria, se recorta y admite hasta 200 caracteres. AlternateDescription se recorta, admite hasta 200 caracteres y vacío/espacios se normaliza a null. Price admite cero y no negativos. Las infracciones devuelven 400 ProblemDetails sin persistir; los Guid mal formados devuelven 404 por la restricción de ruta. PUT sobre Id inexistente devuelve 404 antes de validar las nuevas propiedades.

La API aplica `Database.MigrateAsync` al iniciar. La migración `AddProducts` crea únicamente Products y el historial EF; no recrea ni elimina la base ni sus tablas existentes. Una base creada previamente sin tablas Product admite esta migración. Una tabla Products creada manualmente sin historial requiere conciliación humana; no se modifica ni elimina automáticamente. En despliegues con varias instancias, coordinar la migración y los permisos de escritura antes de arrancar.

La configuración EF conserva `HasPrecision(18,2)`. SQLite almacena decimal como TEXT: esa precisión es metadato, no redondea ni impone un máximo. No se añade un precio máximo ni redondeo que el issue no solicita. No se ofrecen filtros/orden por precio. Las consultas reutilizan el seguimiento del repositorio existente; las actualizaciones mutan la entidad rastreada y se guardan mediante UnitOfWork.

Desde la raíz: `dotnet build CursoNETIA.slnx`, `dotnet test CursoNETIA.slnx` y `dotnet run --project src/AulaPedidos.Api`. El último comando inicia la aplicación y aplica la migración; no se ejecutó sobre una base compartida durante esta tarea. Las pruebas utilizan SQLite en memoria aislado, incluidas las pruebas previas de health/OpenAPI.
