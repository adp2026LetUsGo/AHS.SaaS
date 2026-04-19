# Domain

- Dominio principal: Gestión de embarques y evaluación de riesgo.
- Objetivo: Determinar si un embarque es **NotAcceptable**, **Risky** o **Acceptable**.
- Actores: Usuario, Sistema Xinfer.

## Entities

### ShipmentInput

```
product
category
origin
destination
departureDate
packaging
carrier
conditions (optional)
```

### ShipmentHistory

```
route
carrier
season
packaging
duration
temperatureProfile
excursion (bool)
```

### ModelMetadata

```
modelId
version
trainedAt
dataWindow
performanceMetrics
```
