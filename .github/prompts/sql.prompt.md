# Contrastar una consulta EF y SQL

Fase A/B o V explícita; sólo lectura con expediente y estado exacto. Aplica docs/multiagente-copilot.md y .github/copilot-instructions.md. A/B reciben el mismo caso sin ver la respuesta del otro; A propone diseño de acceso y B busca contraejemplos de volumen, filtros y concurrencia.

Describe SQL esperado, volumen, columnas, tracking, paginación, índices y N+1. Compara conservar consulta actual con una mejora y cómo medir antes/después sobre datos sintéticos equivalentes. Distingue inferencia de plan real/medición aportada por el humano. Mantén parametrización y soft delete.

Entrega alternativas, evidencia, supuestos y casos que falsarían la mejora. Coordinador conserva discrepancias; ingeniero decide. No ejecutes DDL ni edites: sólo el implementador autorizado prepara cambio/migración revisables con reversión. Pruebas y Revisor comprueban resultados y diferencias SQLite/SQL Server; un tiempo inventado no es evidencia.
