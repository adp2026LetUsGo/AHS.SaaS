# AHS Skills — Mapa de Distribución Definitivo
## Blueprint V3.1.1 | Fecha: 2026-03-19

---

## SISTEMA C1 — Google AI Studio (System Instructions)
**Rol:** Architect & PM — razona dominio, estrategia, producto

### Orden de pegado en System Instructions (de arriba a abajo)

```
1. c1-architect-pm                  ← rol e identidad de C1
2. brainstorming                    ← proceso antes de diseñar
3. multi-agent-brainstorming        ← validación antes del Prompt Maestro
4. ddd-strategic-design             ← Bounded Contexts, Aggregates, Ubiquitous Language
5. regulatory-compliance-matrix     ← FDA, GDPR, HACCP, ISO 27001
6. ahs-product-cell-canvas          ← Cell Viability Test, PRD template
7. c4-documentation-standard        ← C4 L1-L2 Mermaid (C1 produce L1 y L2)
```

---

## SISTEMA C2 — Google AI Studio (System Instructions)
**Rol:** Lead Engineer — diseña técnico, produce Prompt Maestro

### Orden de pegado en System Instructions (de arriba a abajo)

```
1. c2-lead-engineer                 ← rol e identidad de C2
2. cqrs-clean-architecture-patterns ← EF Core vs Dapper, handlers, domain services
3. cell-integration-patterns        ← Outbox, Saga, Service Bus, cross-cell testing
4. prompt-engineering-ag            ← Prompt Maestro template 9 secciones
5. c4-documentation-standard        ← C4 L3-L4 (C2 produce L3 y L4)
```

---

## AG — Antigravity (disco: .agent\skills\)
**Rol:** Executor — genera archivos físicos de código

### Distribución por directorio

```
.agent\skills\
│
├── 00_Constitution\
│   ├── Blueprint.MD                          ← ya existe
│   └── Blueprint_Supplement_V3.1.1.MD        ← AÑADIR
│
├── 01_Core\
│   ├── brainstorming.md                      ← ya existe (conservar)
│   ├── Micro-SaaS Launcher.md                ← ya existe (conservar)
│   ├── product-manager.md                    ← ya existe (conservar)
│   ├── product-manager-toolkit.md            ← ya existe (conservar)
│   ├── SaaS MVP Launcher.md                  ← ya existe (conservar)
│   ├── SaaS-Learned-Factors.md               ← ya existe (conservar)
│   ├── native-aot\SKILL.md                   ← AÑADIR
│   ├── simd-vectorization-csharp\SKILL.md    ← AÑADIR
│   ├── sha256-cryptographic-sealing\SKILL.md ← AÑADIR
│   └── regulatory-compliance-matrix\SKILL.md ← AÑADIR
│
├── 02_Architecture\
│   ├── Antigravity Workflows.md              ← ya existe (conservar)
│   ├── antigravity-skill-orchestrator.md     ← ya existe (conservar)
│   ├── CQRS Implementation.md               ← ya existe (conservar)
│   ├── comm-architect-review.md             ← ya existe (conservar)
│   ├── comm-architecture-patterns.md        ← ya existe (conservar)
│   ├── DDD Context Mapping.md               ← ya existe (conservar)
│   ├── ahs-dotnet-architect.md              ← REEMPLAZAR con versión fusionada V3.1
│   ├── ahs-cellular-architecture\SKILL.md   ← AÑADIR
│   ├── c4-documentation-standard\SKILL.md   ← AÑADIR
│   ├── ddd-strategic-design\SKILL.md        ← AÑADIR
│   ├── cqrs-clean-architecture-patterns\SKILL.md ← AÑADIR
│   ├── cell-integration-patterns\SKILL.md   ← AÑADIR
│   └── multi-agent-brainstorming\SKILL.md   ← AÑADIR
│
├── 03_Backend\
│   ├── Clean Code.md                        ← ya existe (conservar)
│   ├── database-architect.md                ← ya existe (conservar — pendiente análisis)
│   ├── DotNet10-SaaS-Core.md               ← ya existe (conservar — pendiente análisis)
│   ├── Hybrid-Persistence-Standard.md       ← ya existe (conservar — pendiente análisis)
│   ├── multitenancy\SKILL.md               ← AÑADIR
│   ├── gxp-ledger-eventsourcing\SKILL.md   ← AÑADIR
│   ├── industrial-cold-chain-logic\SKILL.md ← AÑADIR
│   ├── logistics-oracle-xai\SKILL.md        ← AÑADIR
│   ├── ahs-testing-quality\SKILL.md         ← AÑADIR
│   └── azure-devops-coldchain\SKILL.md      ← AÑADIR
│
├── 04_UI_UX\
│   ├── Antigravity-UI-and-Motion Design Expert.md ← ya existe (conservar)
│   ├── blazor-razor-expert.md               ← REEMPLAZAR con versión .NET 10
│   └── tailwind-glassmorphism-system\SKILL.md ← AÑADIR
│
└── 05_Workflow\                             ← CREAR directorio nuevo
    ├── prompt-engineering-ag\SKILL.md       ← AÑADIR
    └── ahs-cell-template\SKILL.md           ← AÑADIR
```

---

## Resumen de acciones

| Acción | Cantidad |
|---|---|
| Conservar sin tocar | 12 skills existentes |
| Reemplazar | 2 (ahs-dotnet-architect, blazor-razor-expert) |
| Añadir en disco (AG) | 19 skills nuevos |
| Añadir en C1 System Instructions | 7 skills |
| Añadir en C2 System Instructions | 5 skills |
| Crear directorio nuevo | 1 (05_Workflow) |
| Pendiente análisis (skills comunidad) | 3 (database-architect, DotNet10-SaaS-Core, Hybrid-Persistence-Standard) |

---

## Regla de decisión (para futuros skills)

```
¿Responde "qué construir y por qué"?     → C1 System Instructions
¿Responde "cómo diseñar técnicamente"?   → C2 System Instructions
¿Responde "cómo escribir el código"?     → AG en disco
¿Aplica a los tres?                      → AG en disco es suficiente
                                           (C2 lo referencia en el Prompt Maestro)
```

---

## Skills pendientes de análisis (continuar sesión)

Los siguientes skills de comunidad están en 03_Backend y requieren
revisión antes de decidir conservar, actualizar o reemplazar:

1. database-architect.md
2. DotNet10-SaaS-Core.md
3. Hybrid-Persistence-Standard.md

Además, continúa el análisis de los skills restantes que el usuario
proporcionará en la sesión de revisión.
