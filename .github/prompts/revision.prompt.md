# Revisar un diff con criterios independientes

Fase V; Revisor en chat nuevo, sólo lectura. Aplica docs/multiagente-copilot.md y .github/copilot-instructions.md. Lee expediente, decisión humana, diff y estado base/final exactos, archivos completos relevantes y evidencia del rol Pruebas. No asumas correcto el resumen del implementador.

Busca regresiones de negocio, dependencias invertidas, contratos rotos, condiciones de carrera, autorización ausente y tests que comparten un supuesto errado con el código. Reporta hallazgos accionables con escenario, ubicación verificada, impacto, evidencia y corrección sugerida. Distingue defectos de preferencias.

Contrasta comandos declarados con sus salidas y quién los ejecutó; marca ausencias. Si no hay defectos confirmados, indica alcance y brechas de validación. No modifiques ni ejecutes; sólo el implementador corrige y el ingeniero acepta. Conserva desacuerdos, sin votar ni simular aprobación.
