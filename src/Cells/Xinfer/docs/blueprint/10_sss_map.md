# System Sequence Specifications Map — AHS.Cell.Xinfer

Este mapa consolida los 5 SSS, mostrando flujos, eventos y componentes críticos.

---

## SSS-001: Shipment Risk Evaluation
**Actor:** Operator / QA Analyst
**Componentes clave:** UI (Blazor), API, Domain, Infra, GxP Ledger, Service Bus

### Secuencia resumida
1. Usuario llena WhatIfSimulator → Click INYECTAR_SIMULACIÓN
2. API → HandleWhatIfAnalysis → Validaciones de formulario
3. POST /api/xinfer/evaluate → SubmitShipmentHandler
4. Dominio: ShipmentProfile → ReadinessValidator → DivergenceDetector → HistoricalSelector → RetrainDecider → PredictionEngine → Recommender
5. Eventos emitidos → Service Bus y GxP Ledger
6. UI actualiza componentes: XaiRiskMonitor, AlphaBox, AuditLedger, TelemetryHud

### Flujos alternativos
- Data Readiness fails → 422, ReadinessFailEvent
- Insufficient historical data → 422, InsufficientHistoricalDataException
- No active model → RuleBasedFallback, aviso UI
- SignedCommand validation fails → 400, ElectronicSignatureRequiredException

### Postconditions
- ShipmentProfile creado
- GxP Ledger sellado
- XinferResult devuelto y UI actualizada

---

## SSS-002: Model Retraining
**Actor:** System (BackgroundService)

### Secuencia resumida
1. Evento RetrainRequiredEvent o timer
2. Evaluación de criterios (50 registros, nuevas rutas, shift estacional, AccuracyScore, etc.)
3. TriggerRetrainHandler → SignedCommand
4. HistoricalSelector → ModelTrainer.TrainAsync → ModelVersion.Activate
5. HybridCache invalidado
6. GxP Ledger → ModelRetrained
7. Service Bus → Notificación AHS Hive

### Flujos alternativos
- Training fails → retrain fallido, alerta
- AccuracyScore < threshold → modelo no activado, revisión manual

### Postconditions
- Nuevo ModelVersion activo
- Cache invalidado
- GxP Ledger actualizado
- Dashboard actualizado

---

## SSS-003: What-If Simulator
**Actor:** Quality Officer

### Secuencia resumida
1. Abrir WhatIfSimulator → Cambiar parámetros
2. Confirmar ReasonForChange
3. ApplyWhatIfChangeCommand → Append WhatIfParameterChanged → PredictionEngine (8 pasos)
4. GxP Ledger actualizado, XinferResult devuelto
5. UI refleja nuevo estado

### Flujos alternativos
- Unauthorized actor → 403
- Sin cambio real → 400
- Reason demasiado corto → validación UI

### Postconditions
- Ledger sellado con SHA256
- PredictionCompleted sellado
- Auditoría reconstruible

---

## SSS-004: GxP Audit Export
**Actor:** FDA Inspector / QA Auditor

### Secuencia resumida
1. Abrir panel de exportación → seleccionar filtro → EXPORT_AUDIT_TRAIL
2. GET /api/xinfer/audit/export
3. AuditExportHandler → Verificación rol, consulta ledger, LedgerHasher.VerifyChain
4. Generar export en PDF/CSV/JSON
5. Append ExportCompleted → Ledger
6. Descarga archivo al navegador

### Flujos alternativos
- Chain integrity broken → flag
- Date range 0 records → 404
- Actor sin rol → 403
- Export grande → 202, background async

### Postconditions
- Archivo exportado y verificado
- Ledger sellado
- Auditor puede validar integridad SHA256

---

## SSS-005: Tenant Onboarding
**Actor:** System Admin

### Secuencia resumida
1. Script Onboard-Tenant.ps1 → Validar parámetros
2. POST /api/admin/tenants → TenantOnboardingHandler
3. DB: Crear schema (Isolated/Shared) → ejecutar migraciones
4. Configurar Entra ID, claims
5. Seed reference data
6. Enviar email de bienvenida
7. GxP Ledger: TenantOnboarded

### Flujos alternativos
- TenantSlug existe → 409
- Upgrade Shared → Isolated → migraciones y sincronización
- Migration fail → rollback
- Entra ID fail → manual steps

### Postconditions
- Tenant activo, esquema creado, Entra ID claims configurados
- GxP Ledger actualizado
- Tenant listo para usar Xinfer

---

*Este mapa mantiene todos los eventos, postconditions y flujos alternativos de los SSS originales, condensando la redundancia de UI/API/Domain/Infra y facilitando la referencia para C2 y AG.*

