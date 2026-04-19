                                      ┌─────────────────────────┐
                                      │  User Task: Evaluate    │
                                      │  Shipment Risk          │
                                      │  (docs/blueprint/02)    │
                                      └─────────────┬──────────┘
                                                    │
                                                    ▼
                                        ┌───────────────────────┐
                                        │ UI Screen: /input      │
                                        │ Component: InputForm   │
                                        │ (02-screens.md)        │
                                        └─────────────┬─────────┘
                                                      │ Submit
                                                      ▼
                                        ┌───────────────────────┐
                                        │ UI Screen: Data        │
                                        │ Readiness              │
                                        │ (03-states.md)         │
                                        └─────────────┬─────────┘
                                                      │
               ┌───────────────┬──────────────────────┼──────────────────┐
               ▼               ▼                      ▼                  │
     ┌───────────────┐ ┌───────────────┐ ┌────────────────┐           │
     │ NotAcceptable │ │ Risky         │ │ Acceptable     │           │
     │ Issues & Recs │ │ Explanation   │ │ Auto-continue  │           │
     │ CTA: Correct  │ │ CTA: Confirm  │ │ Navigate /result│           │
     └─────┬─────────┘ └─────┬─────────┘ └──────┬─────────┘           │
           │                 │                  │                     │
           └─────────────┬───┴──────────────────┴───────────────┐     │
                         ▼                                      │     │
                ┌───────────────────────┐                       │     │
                │ API: POST /analyze     │                       │     │
                │ (03-contracts.md)      │                       │     │
                └─────────────┬─────────┘                       │     │
                              │ Response: AnalyzeResponse       │     │
                              ▼                                  │     │
                ┌────────────────────────────┐                  │     │
                │ DTOs: Shipment, Readiness, │                  │     │
                │ RiskResult                  │                  │     │
                └─────────────┬──────────────┘                  │     │
                              ▼                                 │     │
                ┌────────────────────────────┐                  │     │
                │ UI Screen: /result          │                  │     │
                │ Component: RiskResult       │                  │     │
                │ (02-screens.md)             │                  │     │
                └─────────────┬──────────────┘                  │     │
                              ▼                                 │     │
                ┌────────────────────────────┐                  │     │
                │ User Confirmation / Restart │◄─────────────────┘     │
                └────────────────────────────┘                        │
                                                                         │
───────────────────────────────────────────────────────────────────────────
📂 Carpetas / indexación:
───────────────────────────────────────────────────────────────────────────
✅ src/Cells/Xinfer       → Core code & DTOs (Shipment, ReadinessResult, RiskResult)
⚙️ src/Foundation        → Helpers / infra (opcionalmente indexado)
🖥️ src/Control Tower     → UI components / Razor (solo soporte)
🤖 agents/skills         → Prompts y AI skills (indexado)
🟢 docs/blueprint         → Blueprints completos (indexado)
🟢 docs/flows/ui          → UI flows y wireframes (indexado)
🟢 docs/flows/system      → System flows (SSS, UIF, UTA) (indexado)
📝 docs/drafts            → Borradores (no indexado)