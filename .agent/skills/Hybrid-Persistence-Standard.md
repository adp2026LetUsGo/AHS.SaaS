
# Skill: Hybrid Persistence Strategy
# ID: AHS-PERSISTENCE-STRATEGY

## Objective
Enable Bounded Contexts (Suites) to switch between SQL, Excel, or CSV storage based on Tenant configuration without breaking Native AOT compatibility.

## Implementation Rules
1. **Abstraction:** Every BC defines its own `IRepository<T>` in the `Domain` layer.
2. **Static Provider Factory:** The `PersistenceProviderFactory` must use **Source-Generated** registration or a switch statement mapping `TenantStorageType` to concrete implementations. **Do not use Reflection/Activator**.
3. **In-Memory Performance:** For Excel/CSV tenants, use `Span<T>` and `Memory<T>` for parsing and load data into a `FrozenDictionary` or `DataTable` for high-speed querying.

## ML & Data Synchronization Rules
- **Source of Truth:** For file-based tenants, the `PredictionService` monitors the file timestamp in `src/platform/` messaging.
- **Data Integrity:** Validate `Temp_Excursion` column (0/1). Use a **Source-Generated Validator** to ensure no nulls before triggering ML.NET training.
- **Exception Policy:** Trigger `DataIntegrityException` for schema mismatches. AG must report these to the `AuditTrail` immediately.

## Constraints
- **Domain Purity:** Zero `DbContext` or `FileStream` references in Domain logic.
- **AOT Readiness:** All persistence implementations must be registered in the `Infrastructure` layer of the Suite using the `AHS-DOTNET10-CORE` DI patterns.
- **Security:** Encrypt file-based storage paths. Data in transit must use `SecretDescriptor` from the Platform Identity service.