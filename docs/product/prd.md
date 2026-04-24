# Product Requirements Document (PRD)

## Project: AHS.SaaS Ecosystem

### 1. Vision and Goals
The AHS Ecosystem provides an auditable, GxP-compliant platform for logistics and cold-chain management. By utilizing Cellular Architecture, we ensure independent scaling and functional isolation of critical business capabilities like the Xinfer risk inference engine.

### 2. User Personas
- **Logistics Operator**: Hands-on user requiring immediate, explainable feedback on shipment risk and required packaging protocols before dispatch.
- **Quality Auditor**: Relies strictly on the `StandardEnvelope` trace IDs and GxP compliant immutable audit logs to verify that the system is operating within defined regulatory thresholds (FDA 21 CFR Part 11).

### 3. Key Performance Indicators (KPIs)
To measure the success of the Xinfer Cell integration, the following metrics apply:
- **Accuracy**: Inference precision must be > 85% on real-world thermal excursion models.
- **Latency**: End-to-end inference generation and response delivery must be < 200ms.
- **Uptime**: The isolated Xinfer infrastructure must maintain 99.99% availability.
- **Traceability**: 100% of generated inferences must include a valid `trace_id` securely linked to the overarching Outbox system.

### 4. Roadmap
#### Phase 1: MVP (Current)
- Implementation of the `Xinfer` Cell as an autonomous Micro-SaaS.
- Hardcoded/Mock algorithm for inference generation demonstrating the XAI explainability metrics (`InfluenceFactors`).
- Outbox pattern for publishing inference events to the wider AHS Ecosystem.
- B2B standalone UI demo verifying cross-origin CORS communication.

#### Phase 2: Production ML Vision
- Replacement of the internal mock algorithms with a true ONNX-based ML model trained on historical thermal data.
- Integration with live IoT sensor streaming.
- Deployment of Xinfer as a fully commercialized third-party SaaS for external logistics brokers.

### 5. Principios de Diseño (Solid-State UI v2.0)

> Se prioriza la claridad estructural y el alto contraste sobre los efectos de transparencia para garantizar la accesibilidad y el rendimiento óptimo del sistema.

#### Directivas Técnicas (Vinculantes para todos los Cells)

| Principio | Directiva |
|---|---|
| **Layout** | Bento Grid isomorfo — rejilla modular de columnas y filas fijas |
| **Elevación** | Jerarquía cromática (Luma): base oscura → superficies más claras conforme sube el eje Z |
| **Bordes** | Stroke-based design — borde de 1px de alta precisión. Sin sombras difusas |
| **Transparencias** | Eliminadas. Cero `backdrop-filter`, cero `rgba` con alpha < 0.9 en superficies |
| **Transiciones** | Deterministas — instantáneas o cambio de estado de color sólido (≤ 150ms) |
| **Skeletons** | Ocupan la misma celda Bento que el contenido real. La estructura visual es idéntica antes y después de la carga |
| **Optimización** | Renderizado estático — cero cálculos de desenfoque o efectos de composición GPU intensivos |

