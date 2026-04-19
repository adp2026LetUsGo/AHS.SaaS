AHS.Cell.Xinfer.code-workspace (Cell: Xinfer)
│
├── src/Cells/Xinfer/                   # ✅ Core code & DTOs
│   ├── Shipment.cs                     # ↳ DTO usado en AnalyzeResponse
│   ├── ReadinessResult.cs              # ↳ Mapeo a UI State
│   └── RiskResult.cs                    # ↳ Result screen
│
├── src/Foundation/                     # ⚙️ Helpers / Infra → Indexado opcional
│   └── Common utilities / extensions
│
├── src/Control Tower/                  # 🖥️ UI Components → Solo soporte
│   └── Razor / Blazor Pages & Components
│
├── agents/skills/                      # 🤖 Prompts → Indexado
│   ├── analyzeShipment.prompt          # ↳ Uso: UI → API → DTO
│   └── riskEvaluation.prompt           # ↳ UI flow guidance
│
└── docs/                               # 📄 Documentación → Indexado
    ├── blueprint/                      # 🟢 Blueprints
    │   ├── 00-overview.md
    │   ├── 01-domain.md
    │   ├── 02-use-cases.md            # ↳ Define User Task
    │   ├── 03-contracts.md            # ↳ Define API contracts
    │   ├── 04-architecture.md         # ↳ Frontend & Backend design
    │   └── 05-traceability.md         # ↳ Task → UI → System → DTO mapping
    │
    ├── flows/
    │   ├── ui/
    │   │   └── wireframes/risk-evaluation/
    │   │       ├── 00-overview.md     # ↳ Goal, Entry, Exit
    │   │       ├── 01-flow.md         # ↳ Paso a paso del flujo
    │   │       ├── 02-screens.md      # ↳ Pantallas & componentes
    │   │       └── 03-states.md       # ↳ NotAcceptable / Risky / Acceptable
    │   │
    │   └── system/
    │       ├── sss.md                  # ↳ System Sequence Specification
    │       ├── uif.md                  # ↳ User Interaction Flow
    │       └── uta.md                  # ↳ User Task Analysis
    │
    └── drafts/                          # 📝 Borradores → NO indexado

─────────────────────────────────────────────
Flujo de ejecución / trazabilidad:

[User Task: Evaluate Shipment Risk]
        │ docs/blueprint/02-use-cases.md
        ▼
[UI Screen: /input]
        │ docs/flows/ui/wireframes/risk-evaluation/01-flow.md
        ▼ Submit
[UI Screen: Data Readiness]
        │ docs/flows/ui/wireframes/risk-evaluation/03-states.md
        │ States:
        │  ├─ NotAcceptable → bloquea avance, muestra issues/recs
        │  ├─ Risky         → permite continuar, explicación
        │  └─ Acceptable    → auto-continue
        ▼
[API: POST /analyze]
        │ docs/blueprint/03-contracts.md
        ▼ Response: AnalyzeResponse
[DTOs: Shipment, ReadinessResult, RiskResult]
        ▼
[UI Screen: /result]
        │ docs/flows/ui/wireframes/risk-evaluation/02-screens.md
        ▼
[User Confirmation / Restart]