🛡️ AHS Universal Constitution — Blueprint V3.1
The Cellular Architecture for Autonomous & Integrated Micro-SaaS
1. Vision: The AHS Cellular Manifesto
The AHS Ecosystem is a factory of Autonomous Cells. Each Cell is a self-contained Micro-SaaS that can be sold as a standalone product or plugged into the AHS Control Tower (Suite).
Standalone First: Every cell must be able to run, persist data, and provide value without external dependencies.
AHS Compliant: Every cell follows the Sovereign Elite UI and the Performance-First (AOT) rule.
2. The 5-Layer Skill Stack (Cellular Focus)
C1 — Runtime: .NET 10 / Native AOT. (Crucial para el "Asset Manager" por el procesamiento de datos GPS en tiempo real).
C2 — Architecture: Clean + DDD + CQRS. Cada Célula tiene su propio Command/Query Bus.
C3 — Infrastructure: Persistence per Cell. (Ej: PostGIS para el Asset Manager, SQL para Finanzas).
C4 — Quality: NetArchTest para asegurar que la Célula de "Activos" no se mezcle con la de "Contabilidad" a nivel de código.
C5 — Context: Vertical de negocio específica (Ej: Asset Tracking & Logistics).
3. Communication & C4 Standard
C4 Nivel 1 (Contexto): Define si la Célula es Standalone (Cliente -> Asset Manager) o Integrated (Cliente -> AHS Suite -> Asset Manager).
C4 Nivel 2 (Contenedores): Cada Célula es un contenedor independiente (API + DB).
C4 Nivel 3 (Componentes): El interior de la célula sigue el patrón Clean Architecture.
4. Cellular Namespace Hierarchy
Reemplazamos la estructura genérica por una estructura de Células Autónomas:
AHS.Cell.[Name].Domain: El corazón del Micro-SaaS. Lógica pura, records inmutables.
AHS.Cell.[Name].Application: Casos de uso (ej: RegisterGPSCoordinateCommand).
AHS.Cell.[Name].Infrastructure: Implementación de adaptadores, GPS Gateways, DB.
AHS.Cell.[Name].UX: La interfaz Sovereign Elite específica para este Micro-SaaS.
AHS.Shared.Lib: (Opcional) Solo para utilidades transversales como hashing SHA256 o Helpers de SIMD.
5. Architectural Guardrails for Cells
The Database-per-Cell Rule: Ninguna célula puede leer la base de datos de otra. La comunicación es vía APIs o Eventos.
The Sovereign UI Rule: Si el "Asset Manager" se vende solo, usa su propia consola. Si se integra, se "embebe" en el AHS Shell conservando los mismos colores y estilos.
The Predictive Power: Cada célula debe incluir un Engine (C1/C2). En el caso de Activos: Predictive Maintenance basado en uso/tiempo.
