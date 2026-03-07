# AHS.SaaS: Command Center Operational Guide
**Operational Standard (SOP) | Version 2.1**

## 1. Dashboard Interpretation
- **STABLE (Cyan/Blue):** Thermal trends are within the safe corridor. No action required.
- **CRISIS MODE (Pulse Red):** TTF (Time-to-Failure) is under 30 minutes. Immediate intervention required via the *Mitigation Actions* panel.

## 2. Smart Data Ingestion
1. Navigate to the **Smart Ingest** area (Bottom Right).
2. Drag and drop GxP-validated `.csv` or `.txt` thermal logs.
3. The **HPC Bridge** will automatically suspend synthetic simulation and switch to **Live GxP Mode**.
4. Monitor the **Engine DNA** and **Forecast Shadow** for real-time trend analysis.

## 3. Control Protocols
When the **Mitigation Panel** activates:
- **[ACTIVATE_BACKUP_COOLING]:** Triggers secondary thermal dissipation.
- **[DIVERT_LOGISTICS_ROUTE]:** Re-calculates ETA to avoid high-risk zones.
