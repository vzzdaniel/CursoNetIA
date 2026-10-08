---
applyTo: "src/AulaPedidos.Api/**/*.cs"
---

Usa exclusivamente Minimal APIs: `MapGroup` y módulos en `Endpoints`, con `RequireAuthorization`, `TypedResults` y validación nativa mediante `AddValidation`. En records de entrada dirige DataAnnotations a las propiedades con `property:`. Handlers HTTP delgados; DTOs sin entidades EF. Declara versión, estados HTTP y OpenAPI coherentes. Autenticación antes de autorización. Políticas por permisos más protección de propietario. No conviertas toda excepción en 400. Expón ProblemDetails sanitizado y correlación de trazas. Emisión de tokens de desarrollo sólo en Development; nunca habilitarla en producción para resolver un fallo. No registrar Authorization ni secretos.

En el expediente multiagente fija rutas, identidad y esperado HTTP antes de generar código. A propone contrato y B busca sobreasignación, acceso ajeno y errores incompatibles sin consultar A. Tras síntesis y decisión humana sólo el implementador cambia API/tests/docs. Pruebas diseña casos positivos y negativos desde el contrato; Revisor inspecciona diff y salidas reales. Los lectores no editan ni inventan ejecuciones. Sigue docs/multiagente-copilot.md.
