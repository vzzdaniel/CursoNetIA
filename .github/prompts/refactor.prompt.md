# Refactorizar tras contrastar alternativas

Fase A/B por defecto; adjunta expediente, estado exacto y archivos. Aplica docs/multiagente-copilot.md y .github/copilot-instructions.md. A/B analizan en chats separados sin compartir respuestas. Explica comportamiento observable y problema de mantenimiento; identifica caracterización faltante y dos opciones, incluyendo no cambiar cuando proceda.

Conserva contratos HTTP, reglas y persistencia. Coordinador sintetiza desacuerdos; ingeniero decide y delimita archivos. Sólo en fase I explícita, con ese paquete y como Implementador único, aplica el refactor aprobado. No cambies nombres públicos, paquetes ni capas por estética.

Entrega diff y evidencia antes/después: comandos realmente ejecutados, comportamiento conservado y pendientes. Reporta cualquier cambio semántico como tal. Pruebas y Revisor verifican en sesiones separadas; el humano decide el cierre.
