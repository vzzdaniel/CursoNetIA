---
applyTo: "src/AulaPedidos.Domain/**/*.cs"
---

Mantén el dominio libre de HTTP, EF Core, logging y dependencias de infraestructura. Protege invariantes en operaciones de las entidades, con setters restringidos y colecciones encapsuladas. Los montos usan decimal; conserva el precio histórico de cada línea. Un evento de dominio expresa un hecho pasado y no envía correos ni realiza I/O. Incluye pruebas que fallen si se omite la invariante. No añadas interfaces a cada clase por costumbre.

En A/B contrasta invariantes y contraejemplos sobre el mismo expediente, sin editar ni ver la otra respuesta. El coordinador conserva diferencias y el ingeniero decide la regla y su esperado. Sólo el implementador autorizado cambia entidades y tests. Pruebas y Revisor verifican límites monetarios/transiciones desde esa regla, no desde la fórmula generada; reporta evidencia real y pendientes según docs/multiagente-copilot.md.
