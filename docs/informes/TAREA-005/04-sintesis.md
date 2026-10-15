# TAREA-005 — Síntesis

- **ID:** TAREA-005
- **Objetivo:** Definir un contrato `Result` en Api y el flujo global para excepciones no controladas.
- **Rol:** Coordinador (fase `sintesis`)
- **Fecha:** 2026-10-15

## Objetivo de la síntesis

Comparar los informes disponibles de Arquitectura y Pruebas para proponer un alcance de implementación verificable, separando recomendaciones técnicas de decisiones humanas.

## Fuentes realmente leídas

- `docs/informes/TAREA-005/02-arquitectura.md`
- `docs/informes/TAREA-005/03-pruebas.md`
- `docs/informes/TAREA-005/01-coordinacion.md`
- Issue de la tarea: https://github.com/vzzdaniel/CursoNetIA/issues/13
- Comentario Arquitectura (verificado): https://github.com/vzzdaniel/CursoNetIA/issues/13#issuecomment-6201752210
- Comentario Pruebas-inicial (verificado): https://github.com/vzzdaniel/CursoNetIA/issues/13#issuecomment-6201800571

## Acuerdos entre Arquitectura y Pruebas

1. El contrato `Result` debe mantenerse **solo en Api** (sin dependencia desde Application/Domain).
2. La API debe uniformar respuestas JSON con campos `IsSuccess`, `Message` y `Data`.
3. Las excepciones inesperadas, si la respuesta no ha iniciado, deben devolver `500` con respuesta segura (sin detalles internos).
4. Los errores de validación (FluentValidation y dominio) deben salir en `400` bajo el contrato `Result`.
5. `Message` y `Data` deben serializarse aunque sean `null`.

## Diferencias y puntos abiertos

1. **DELETE**
   - Arquitectura recomienda `200` + `Result` con `Data = null` para uniformidad estricta.
   - Pruebas deja abierta alternativa `204` como excepción documentada.
2. **Cancelaciones y respuesta iniciada**
   - Ambos informes lo marcan como borde sensible y pendiente de validación específica.
3. **Cobertura real**
   - Pruebas confirma diseño de casos, pero no hay ejecución real en esta fase.

## Alcance recomendado (propuesta técnica)

> **Propuesta del Coordinador. No equivale a decisión humana aprobada.**

1. Crear `Result<T>` en Api (`IsSuccess`, `Message`, `Data`) con serialización de `null` explícita.
2. Adaptar endpoints de productos para respuestas uniformes en `200/201/400/404/500`.
3. Añadir/ajustar handler global de excepciones inesperadas para responder `500` seguro cuando corresponda.
4. Adaptar handlers de validación actuales para `400` en formato `Result`.
5. Resolver antes de cerrar implementación la política de `DELETE` (200 uniforme vs 204 excepción documentada).
6. Actualizar pruebas de integración para el contrato nuevo y casos borde (cancelación, respuesta iniciada, no fuga de detalles internos).

## Casos de aceptación recomendados

1. `POST/GET/PUT` devuelven JSON con `IsSuccess`, `Message`, `Data`.
2. `404` devuelve `Result` con `IsSuccess=false` y `Data=null`.
3. Validación FluentValidation y validación de dominio devuelven `400` con `Result` (sin `ProblemDetails`).
4. Excepción inesperada previa al inicio de respuesta devuelve `500` con `Result` seguro sin `exception.Message` ni stacktrace.
5. El JSON incluye siempre `message` y `data` aun cuando sean `null`.
6. `Application` y `Domain` no incorporan dependencias al contrato `Result` de Api.
7. `DELETE` cumple la decisión humana final (uniformidad estricta o excepción explícita documentada).

## Pendientes

1. Decisión humana final sobre `DELETE` (`200 + Result` vs `204` excepción).
2. Evidencia ejecutada de pruebas posteriores a implementación.
3. Verificación práctica de cancelaciones y respuesta ya iniciada.

## Decisiones humanas vs propuesta

- **Decisiones humanas confirmadas previas:**
  - El contrato contiene `IsSuccess`, `Message`, `Data`.
  - `Message` y `Data` pueden ser `null` y deben aparecer siempre.
  - El contrato aplica solo a Api.
- **Propuesta técnica (esta síntesis):**
  - Implementación concreta vía endpoints + handlers + manejo global de excepciones + actualización de pruebas.

## Prompt autocontenido para Implementador

```text
Rol: Implementador

Tarea: TAREA-005
Carpeta: docs/informes/TAREA-005/

Lee:
- docs/adr/007-Result.md
- docs/informes/TAREA-005/02-arquitectura.md
- docs/informes/TAREA-005/03-pruebas.md
- docs/informes/TAREA-005/04-sintesis.md

Guarda tu entrega en:
- docs/informes/TAREA-005/05-implementacion.md

Issue verificado:
- Repo: vzzdaniel/CursoNetIA
- Issue: #13
- URL: https://github.com/vzzdaniel/CursoNetIA/issues/13

Objetivo:
Implementar contrato Result en Api y manejo de excepciones no controladas, manteniendo Application/Domain sin dependencia a Result.

Alcance mínimo recomendado:
1) Crear contrato Result<T> en Api con IsSuccess, Message, Data.
2) Adaptar endpoints de productos para usar Result en 200/201/400/404/500.
3) Ajustar handlers de validación para devolver Result en 400.
4) Manejar excepciones inesperadas con respuesta 500 segura cuando la respuesta no haya iniciado.
5) Preservar serialización de message/data cuando sean null.
6) Resolver explícitamente DELETE según decisión humana (200 uniforme o 204 excepción documentada).
7) Actualizar pruebas de integración alineadas al nuevo contrato.

Reporta en 05-implementacion.md:
- archivos modificados
- decisiones tomadas y límites
- comandos/validaciones ejecutadas y resultados reales
- pendientes
- URL del draft PR (según docs/github-pr.md) o estado pendiente con evidencia.
```

## Publicación en GitHub

- **Repositorio:** `vzzdaniel/CursoNetIA`
- **Issue:** `#13` — https://github.com/vzzdaniel/CursoNetIA/issues/13
- **Fase/marcador:** `sintesis` / `<!-- aulapedidos:TAREA-005:sintesis -->`
- **Estado:** `publicado`
- **Comentario (id/url):** `6201959624` — https://github.com/vzzdaniel/CursoNetIA/issues/13#issuecomment-6201959624