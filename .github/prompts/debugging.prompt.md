# Diagnosticar con evidencia y consultas separadas

Fase A/B; sólo lectura con el expediente, estado exacto, síntoma, reproducción y logs sanitizados. Aplica docs/multiagente-copilot.md y .github/copilot-instructions.md. En chats separados, A y B plantean hipótesis sin conocer el informe del otro.

Separa observaciones de hipótesis. Entrega tres hipótesis ordenadas, comprobación de bajo costo para cada una y siguiente acción. No atribuyas un error al framework sin evidencia ni pidas datos de clientes. El humano o implementador autorizado reproduce; las salidas reales vuelven al coordinador para contrastar hipótesis.

Al identificar causa raíz, propone reparación mínima y prueba de regresión. El ingeniero decide y sólo el implementador aplica la corrección; Pruebas y Revisor comprueban el diff después. No ocultes errores desactivando validación, autorización o tests. Reporta quién ejecutó cada comprobación, resultado y pendientes; una hipótesis no es una causa confirmada.
