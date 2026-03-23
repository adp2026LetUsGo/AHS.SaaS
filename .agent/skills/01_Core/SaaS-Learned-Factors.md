
# Skill: SaaS Learned Factors & Error Handling
# ID: AHS-ERRORS-LOG

## 🚨 Critical Historical Lessons (January 2026)
- **Multi-tenancy Concurrency:** Static variables previously caused Tenant ID leakage between requests.
- **Fix:** Use `HttpContext.Items` or a Scoped `ITenantProvider`.
- **Mandatory DI Rule:** `ITenantService` MUST be registered as **Scoped**. AG must verify this in `Program.cs` or the Platform Infrastructure layer before every build.

## 🛠️ Prevention Checklist (AHS.SaaS Monorepo)
1. **Driver Compatibility:** Before adding NuGet packages, verify **.NET 10 / Native AOT** compatibility. If a driver uses `System.Reflection.Emit`, it is forbidden.
2. **DI Integrity:** If an `ObjectDisposedException` occurs, AG must analyze the lifetime of the service. Background Tasks (`IHostedService`) must create a new `IServiceScope` to consume Scoped services.
3. **Domain Alignment:** Cross-reference all new Cell logic with the
   **Blueprint V3.1** namespace hierarchy (AHS.Cell.[Name].[Layer])
   to ensure no cross-cell dependency leakage. Domain layer must have
   zero external dependencies (NetArchTest enforces this).

4. **Clean Exit:** Ensure all `IDisposable` resources (especially SQL connections in Dapper wrappers) are handled within `using` blocks or C# 14's enhanced using declarations.


## 🚨 Additional Lessons (March 2026 — V3.1 Migration)
- **MakeGenericMethod + BindingFlags:** Previously used in EF Core
  global query filters. Breaks Native AOT (IL2060).
  Fix: Register HasQueryFilter() explicitly per entity — no reflection.

- **JsonSerializer without JsonSerializerContext:** Any
  JsonSerializer.Serialize(obj) without explicit context breaks AOT.
  Fix: All types crossing API boundary must be in JsonSerializerContext.


## 🚨 Additional Lessons (March 2026 — ColdChain Cell)
- **Aggregate Rehydration con Activator.CreateInstance:** AG usó
  Activator.CreateInstance para rehydration en ShipmentRepository.
  Rompe Native AOT (IL2072).
  Fix: constructor privado + static Rehydrate() factory en el aggregate.
  
  public static Shipment Rehydrate(IEnumerable<DomainEvent> history)
  {
      var s = new Shipment();   // private ctor
      s.Rehydrate(history);     // AggregateRoot.Rehydrate()
      return s;
  }