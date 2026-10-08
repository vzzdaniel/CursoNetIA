---
applyTo: "**/*.cs"
---

# Convenciones de C# — AulaPedidos

## Lenguaje y formato

- Usa nombres de código en inglés y explicaciones en español.
- Usa PascalCase para tipos, métodos y propiedades públicas.
- Usa camelCase para parámetros y variables locales.
- Usa _camelCase para campos privados cuando corresponda.
- Usa nombres descriptivos y coherentes con el código existente.
- Respeta .editorconfig: cuatro espacios y namespaces file-scoped.
- Conserva los contratos existentes al nombrar implementaciones.

## Diseño y arquitectura

- Respeta Clean Architecture y las decisiones de docs/adr/.
- Aplica SOLID según la necesidad; evita abstracciones innecesarias.
- Domain protege reglas de negocio y no depende de HTTP ni EF Core.
- Application coordina casos de uso mediante contratos.
- Infrastructure implementa persistencia y servicios externos.
- Api usa Minimal APIs y delega el negocio a Application.
- Conserva el mediador propio y Result donde ya se utilizan.
- Realiza cambios pequeños dentro del alcance autorizado.
