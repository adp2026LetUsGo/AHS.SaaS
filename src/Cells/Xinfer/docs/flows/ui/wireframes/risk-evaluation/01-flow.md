# Risk Evaluation Flow

## Paso a paso con trazabilidad


[User] -> /input (Submit Shipment) -> AnalyzeService
-> Get ReadinessResult -> ReadinessState
-> Render UI State: NotAcceptable / Risky / Acceptable
-> Navigate to /result -> Display RiskResult


1. Usuario ingresa un Shipment
2. Sistema evalúa condiciones (Readiness)
3. Estado del UI:
    - **NotAcceptable** → mostrar issues y recomendaciones
    - **Risky** → mostrar explicación y permitir continuar
    - **Acceptable** → continuar automáticamente
4. Resultado final: RiskResult con probabilidad y explicación