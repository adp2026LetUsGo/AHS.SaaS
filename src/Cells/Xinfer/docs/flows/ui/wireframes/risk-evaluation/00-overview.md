# Risk Evaluation Flow

## Goal
Evaluar el riesgo de un embarque antes de su ejecución

## Entry Point
User ingresa datos del shipment

## Exit Point
Visualización del resultado de riesgo + recomendaciones

## Screens (High Level)
1. Shipment Input
2. Conditions Input
3. Data Readiness
4. Risk Result

## Actors
- User
- Xinfer System

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