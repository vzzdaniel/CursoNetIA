# ADR 006: validación con FluentValidation en el pipeline de Mediator

- Estado: aceptada.
- Aprobación: el usuario confirma «aceptamos el ADR» en esta conversación.
- Fecha y responsables: fecha no registrada; aprobación del usuario solicitante.

## Contexto y restricciones comprobadas

AulaPedidos necesita validar los mensajes antes de ejecutar sus handlers, evitando repetir la coordinación de validación en cada endpoint o caso de uso.

Los archivos de proyecto consultados utilizan .NET 10 y Mediator 3.0.2:

- Application referencia `Mediator.Abstractions`.
- Api referencia `Mediator.SourceGenerator`.

El requisito es integrar FluentValidation mediante los behaviors de Mediator, sin introducir MediatR. No se ha verificado el registro actual de Mediator ni la existencia de un tratamiento centralizado de excepciones.

El alcance comprende comandos y consultas ordinarios. La selección de los mensajes concretos que tendrán validadores se determinará durante la implementación. El contrato de error aceptado es HTTP 400 con `ValidationProblemDetails`.

## Alternativas y costes

| Alternativa | Complejidad y coste | Mantenimiento y pruebas |
|---|---|---|
| Validación en endpoints | Bajo coste inicial. | Acoplada a HTTP; no cubre otros emisores del mismo mensaje. |
| Validación en cada handler | Bajo coste inicial, creciente con el número de casos. | Repite coordinación y facilita omisiones. |
| Behavior de Mediator | Coste inicial moderado. | Centraliza el flujo; permite probar validadores y behavior por separado. |
| Behavior con resultado tipado | Mayor coste si no existe un contrato común. | Errores explícitos, pero condiciona las respuestas de los casos de uso. |

Se elige el behavior por ofrecer un punto común de validación sin acoplar los casos de uso al transporte HTTP.

## Decisión

Incorporar un behavior de validación basado en `IPipelineBehavior<TMessage, TResponse>` para comandos y consultas ordinarios enviados mediante Mediator.

El behavior:

1. Obtendrá los `IValidator<TMessage>` registrados.
2. Ejecutará `ValidateAsync`, propagando el `CancellationToken`.
3. Reunirá los errores de los validadores.
4. Impedirá ejecutar el handler cuando existan errores.
5. Continuará el pipeline cuando la validación sea correcta o no haya validadores.

Los validadores se ejecutarán secuencialmente, evitando operaciones concurrentes sobre dependencias compartidas, como un `DbContext`.

El registro deberá ajustarse al mecanismo admitido por Mediator 3.0.2 y comprobarse mediante una prueba de integración.

### Distribución por capas

| Capa | Responsabilidad |
|---|---|
| Application | Behavior y validadores de comandos y consultas. |
| Api | Composición y traducción de errores al contrato HTTP. |
| Domain | Invariantes y reglas de negocio, sin dependencia de FluentValidation ni Mediator. |
| Infrastructure | Implementaciones de acceso a datos cuando un validador las necesite mediante contratos de Application. |

### Tratamiento de errores

Utilizar `ValidationException` de FluentValidation y traducirla centralizadamente en Api a HTTP 400 con `ValidationProblemDetails`, agrupando mensajes por campo.

Esta elección queda aceptada. No se introducirá un patrón `Result` únicamente para resolver la validación.

Los errores inesperados de infraestructura no se convertirán en errores de entrada. La cancelación tampoco se tratará como una validación fallida.

## Consecuencias positivas y negativas

### Ventajas

- Validación consistente para los mensajes que atraviesen el pipeline.
- Handlers centrados en ejecutar el caso de uso.
- Reglas comprobables mediante pruebas independientes.

### Costes y riesgos

- Nueva dependencia de FluentValidation en Application.
- Necesidad de registrar correctamente validadores y behavior.
- Las llamadas directas a handlers eludirán esta validación.
- Las comprobaciones contra almacenamiento añaden latencia y no garantizan por sí solas unicidad frente a concurrencia.

La validación no sustituye las invariantes del dominio, las restricciones de persistencia ni la autorización. Notificaciones y mensajes en streaming quedan fuera de esta decisión.

## Evidencia y verificaciones pendientes

Fuentes consultadas:

- [Proyecto Application](../../src/AulaPedidos.Application/AulaPedidos.Application.csproj).
- [Proyecto Api](../../src/AulaPedidos.Api/AulaPedidos.Api.csproj).
- [Plantilla de ADR](plantilla.md).

No se ha implementado la integración ni se han ejecutado pruebas que demuestren su funcionamiento. Los siguientes criterios deberán comprobarse durante la implementación:

- Un mensaje válido ejecuta el handler una sola vez.
- Un mensaje inválido no ejecuta el handler.
- Los errores de varios validadores se conservan y agrupan por campo.
- Un mensaje sin validadores continúa normalmente.
- Se propaga la cancelación.
- La API devuelve el contrato de error aprobado sin exponer datos sensibles.
- Una prueba de integración demuestra que el registro real activa el behavior.

## Condición para revisarla

Revisar esta decisión si la aplicación adopta un contrato común de resultados tipados, cambia la versión o el mecanismo de registro de Mediator, o requiere validar notificaciones o mensajes en streaming.

La aceptación del ADR no implica que la integración esté implementada ni que sus criterios de aceptación hayan sido verificados.
