# Use Cases

1. Ingreso de Shipment
2. Evaluación de condiciones y readiness
3. Análisis de riesgo y recomendaciones
4. Resultado final con explicación del riesgo

## Core Flow

```
Input → Data Readiness → (block or continue) → Prediction
```

## Use Case: Predict Shipment Risk

```
1. User inputs shipment data
2. System evaluates Data Readiness
3. IF NotAcceptable → block prediction
4. IF Risky/Acceptable → continue
5. System predicts risk
6. System returns explanation and recommendations
```

## Use Case: Data Validation Only

```
1. User inputs shipment data
2. System evaluates Data Readiness
3. System returns readiness status
```