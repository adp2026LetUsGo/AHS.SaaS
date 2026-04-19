# PRD-009: PRD Alignment Strategy

**Status**: Accepted  
**Date**: 2026-Q2  
**Deciders**: C1 Architect, C2 Lead Engineer

## Context
- V3.1 introduces multiple Cells con autonomía y contratos.
- Cada Cell tiene su PRD local.  
- Necesitamos asegurar que los PRDs reflejen la estrategia del Blueprint y respeten ADRs.

## Decision
1. **Cross-Cell Alignment**
   - Cada PRD debe referenciar los ADR relevantes:
     - ADR-001: Database-per-Cell
     - ADR-002: Native AOT
     - ADR-003: Service Bus inter-Cell
     - ADR-006: SignedCommand
     - ADR-008: UI Component Boundaries
   - Debe reflejar las restricciones de Technology Radar.

2. **Feature Alignment**
   - P0/P1/P2 features del PRD deben mapear al roadmap del Blueprint.  
   - Out-of-scope features deben indicar qué Cell los cubre.

3. **Domain Model Consistency**
   - Agregados y roles deben estar alineados con el modelo global.  
   - Relaciones inter-Cell deben indicar eventos o APIs expuestos.

4. **Regulatory & Success Metrics**
   - Cada PRD debe reflejar regulaciones y métricas que se aplican a esa Cell.  
   - Si métricas dependen de otras Cells, referenciar PRD correspondiente.

5. **Versioning**
   - PRD local: control de versión por Cell (ej. 3.0, 3.1).  
   - PRD global: resumen de versiones y roadmap agregado.

## Consequences
- Facilita revisión arquitectónica cruzada.  
- Evita duplicidad y divergencia entre Cells.  
- Permite generación de doc y tooling automatizado confiable.

## Implementation
- Cada Cell mantiene su PRD.md.  
- PRD Alignment.md se mantiene en docs/global o docs/alignment para referencia de arquitectos.