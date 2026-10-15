# TAREA-005 — Coordinación inicial

- **ID:** TAREA-005
- **Objetivo:** Definir el contrato `Result` en Api y el manejo global de excepciones no controladas.
- **Rol:** Coordinador
- **Fecha:** 2026-10-15

## Contexto disponible revisado

- `docs/adr/007-Result.md` (borrador de ADR con requisitos confirmados y dudas pendientes).
- `docs/informes/README.md` (convención de guardado).
- `docs/informes/indice.md` (IDs existentes; se registró TAREA-005).
- `docs/multiagente-copilot.md` (flujo por roles).
- `docs/github-pr.md` (gestión de issue y publicación de resúmenes).

## Issue de la tarea (verificado)

- **Repositorio:** `vzzdaniel/CursoNetIA`
- **Issue:** `#13`
- **URL:** https://github.com/vzzdaniel/CursoNetIA/issues/13
- **Estado:** creado y listo para usar en consultas 2A/2B y síntesis.

## Alcance inicial

1. Validar si la propuesta actual (middleware de excepciones en Api + `Result` solo en Api) cumple el objetivo de uniformidad sin romper contratos existentes.
2. Delimitar vacíos funcionales pendientes (`401/403/404`, `204`, streaming, validaciones, cancelaciones).
3. Definir criterios de aceptación verificables antes de pasar a implementación.

## Plan breve por pasos

1. **Arquitecto (2A):** revisar estructura/capas y proponer opción recomendada con límites técnicos explícitos.
2. **Pruebas (2B):** diseñar casos prioritarios (éxito, error esperado, excepción inesperada, borde) y evidencias mínimas.
3. **Coordinador (síntesis):** comparar `02-arquitectura.md` y `03-pruebas.md`, registrar acuerdos/diferencias y preparar alcance recomendado para Implementador.

## Prompts listos para copiar

### Prompt para Arquitecto (2A)

```text
Rol: Arquitecto

Trabaja la tarea TAREA-005.

Contexto de entrada obligatorio:
- docs/adr/007-Result.md
- docs/informes/TAREA-005/01-coordinacion.md

Salida obligatoria:
- docs/informes/TAREA-005/02-arquitectura.md

Issue verificado de la tarea:
- Repo: vzzdaniel/CursoNetIA
- Issue: #13
- URL: https://github.com/vzzdaniel/CursoNetIA/issues/13

Objetivo:
Evaluar la decisión propuesta para uniformar respuestas en Api (`IsSuccess`, `Message`, `Data`) y manejo global de excepciones, manteniendo Mediator sin contrato Result.

Entrega:
- Propuesta técnica principal y alternativas descartadas.
- Riesgos y límites (401/403/404 sin excepción, 204, streaming, cancelaciones, validaciones).
- Criterios de aceptación técnicos.
- Supuestos y pendientes.
- Sección “Publicación en GitHub” con estado real del resumen de fase arquitectura en el issue #13.
```

### Prompt para Pruebas (2B)

```text
Rol: Pruebas

Trabaja la tarea TAREA-005.

Contexto de entrada obligatorio:
- docs/adr/007-Result.md
- docs/informes/TAREA-005/01-coordinacion.md

Salida obligatoria:
- docs/informes/TAREA-005/03-pruebas.md

Issue verificado de la tarea:
- Repo: vzzdaniel/CursoNetIA
- Issue: #13
- URL: https://github.com/vzzdaniel/CursoNetIA/issues/13

Objetivo:
Definir estrategia de pruebas para el contrato Result en Api y manejo global de excepciones no controladas.

Entrega:
- Casos prioritarios (positivos, negativos y borde).
- Evidencia esperada por caso (HTTP status, cuerpo serializado, nulabilidad de campos, no exposición de detalles internos).
- Riesgos de regresión y pendientes de verificación.
- Criterios de aceptación comprobables.
- Sección “Publicación en GitHub” con estado real del resumen de fase pruebas-inicial en el issue #13.
```

### Prompt para Coordinador (síntesis posterior)

```text
Rol: Coordinador

Continúa la tarea TAREA-005 después de recibir:
- docs/informes/TAREA-005/02-arquitectura.md
- docs/informes/TAREA-005/03-pruebas.md

Genera:
- docs/informes/TAREA-005/04-sintesis.md

Issue verificado:
- Repo: vzzdaniel/CursoNetIA
- Issue: #13
- URL: https://github.com/vzzdaniel/CursoNetIA/issues/13

Objetivo de síntesis:
Comparar conclusiones reales, registrar acuerdos/diferencias, alcance recomendado, aceptación y pendientes, distinguiendo propuesta técnica de decisión humana.

Además:
Publicar comentario de fase sintesis en el issue #13 con deduplicación por marcador y registrar id/URL y estado real en 04-sintesis.md.
```

## Estado

- ID resuelto y registrado: **TAREA-005**.
- Coordinación inicial guardada.
- Listo para lanzar consultas a Arquitecto y Pruebas.