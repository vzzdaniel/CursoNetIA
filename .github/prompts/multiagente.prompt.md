# Preparar expediente y consultas separadas

Fase E/A/B. Aplica .github/copilot-instructions.md y docs/multiagente-copilot.md. Arquitecto, Pruebas y Revisor no editan. El Coordinador puede guardar el expediente y su paquete común sólo tras confirmación humana explícita. La persona solicitante proporciona:

- Identificador del expediente, directorio de trabajo y estado exacto del código (base, diff y archivos nuevos).
- Objetivo, reglas de negocio y origen del resultado esperado.
- Contexto permitido, archivos/capas y restricciones.
- Criterios positivo, negativo y borde; riesgos y datos sintéticos.
- Herramientas efectivas, responsable humano, presupuesto y condición de parada.

Señala datos faltantes sin inventarlos. Propón docs/expedientes/<ID>.md y docs/expedientes/<ID>.paquete-comun.md; por ejemplo, CATEGORY-001.md y CATEGORY-001.paquete-comun.md. Presenta las rutas y el contenido o diff antes de guardar; espera la confirmación de esa operación y registra su autor y alcance. Guarda sin repetir una confirmación ya recibida para el mismo contenido y destino. Si falta edición, entrega el texto al humano y registra el guardado manual.

Asigna dos tareas separadas A/B a los lectores definidos para la ronda (Arquitecto, Revisor o Pruebas). Por ejemplo, A/Arquitecto explora opciones y B/Pruebas deriva contraejemplos; A/B no son nombres fijos de perfil. Ambas consultas reciben el mismo paquete común congelado, no el expediente acumulativo con informes. Cada informe incluye observaciones, archivos consultados, supuestos, alternativas, riesgos y pruebas sugeridas. No cambies el paquete durante A/B ni pases A a B o B a A antes de concluir ambos. Guarda sus originales en el expediente sólo después de que ambos terminen y con confirmación humana.

En Visual Studio entrega las dos consignas para que el humano las ejecute en chats nuevos. No simules subagentes. En CLI preparado, sólo afirma delegación si hay tareas reales observadas. Máximo una ronda A/B y una aclaración; análisis y síntesis con 10 minutos de referencia, ajustables por el humano antes de iniciar según el riesgo y alcance. Si falta un informe, queda pendiente; no lo redactes en nombre de otro agente.
