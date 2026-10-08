# Consultar y revisar seguridad con evidencia

Fase A/B o V, indicada explícitamente; sólo lectura. Aplica docs/multiagente-copilot.md y .github/copilot-instructions.md. En A/B recibe únicamente expediente/estado/contexto comunes, sin respuesta del otro; en V añade decisión, diff y salidas reales.

Sigue el flujo desde HTTP hasta persistencia. Examina autenticación, claims, roles, permisos, pertenencia del recurso, sobreasignación, secretos, inyección, logs, límites, errores y dependencias. Para cada hallazgo da escenario reproducible con datos sintéticos, impacto, archivo/línea verificados, corrección propuesta y test.

Ordena por riesgo; separa confirmado, posible y fuera de alcance. No declares seguridad completa ni pruebes sistemas ajenos o credenciales reales. Entrega observaciones y supuestos al coordinador; el humano decide y un único implementador modifica. Pruebas y Revisor verifican por separado. Marca como pendiente todo comando no ejecutado.
