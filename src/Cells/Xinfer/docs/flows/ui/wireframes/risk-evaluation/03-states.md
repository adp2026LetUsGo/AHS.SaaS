# UI States

## NotAcceptable
- Bloquea avance
- Muestra issues y recomendaciones
- CTA: Volver a corregir
- Ejemplo:

Issues: ["Faltan datos en Shipment", "Condición inválida"]
Recommendations: ["Completar todos los campos", "Revisar condiciones"]


## Risky
- Permite continuar
- Muestra explicación
- CTA: Confirmar
- Ejemplo:

Explanation: "Riesgo moderado, continuar con precaución"


## Acceptable
- Auto-continue
- CTA implícita: avanzar a Result
- Ejemplo:

State: AcceptableState()