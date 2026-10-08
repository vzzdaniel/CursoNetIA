# Sintetizar sin ocultar desacuerdos

Fase S; Coordinador, con guardado del expediente tras confirmación humana. Aplica docs/multiagente-copilot.md y .github/copilot-instructions.md. Recibe expediente y estado, informes A/B completos y su procedencia real. Si tienen estados distintos o uno falta, informa el problema antes de compararlos.

Entrega tabla punto/A/B/evidencia/acuerdo o desacuerdo/comprobación pendiente. Distingue hechos, inferencias y supuestos comunes. Una mayoría no demuestra corrección; conserva la alternativa minoritaria. Propón la comprobación más pequeña para resolver cada desacuerdo relevante y, como máximo, una aclaración dentro del presupuesto.

Recomienda una opción razonada, con alcance/archivos para el único implementador y criterios a contrastar con Pruebas. Indica qué invalidaría la recomendación y qué queda sin validar. No inventes consenso, ejecuciones ni respuestas ausentes. Presenta el diff propuesto del expediente docs/expedientes/<ID>.md; guarda los informes originales y la síntesis sólo después de la confirmación humana de esa operación, sin modificar el paquete común. Registra autor y alcance de la confirmación. La decisión de implementación sigue pendiente del ingeniero: guardar la síntesis no autoriza ni aprueba el cambio.
