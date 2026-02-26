
# Skill: SaaS Learned Factors & Error Handling
# ID: AHS-ERRORS-LOG

## 🚨 Critical Historical Lessons (January 2026)
- **Multi-tenancy Concurrency:** Static variables previously caused Tenant ID leakage between requests.
- **Fix:** Use `HttpContext.Items` or a Scoped `ITenantProvider`.
- **Mandatory DI Rule:** `ITenantService` MUST be registered as **Scoped**. AG must verify this in `Program.cs` or the Platform Infrastructure layer before every build.

## 🛠️ Prevention Checklist (AHS.SaaS Monorepo)
1. **Driver Compatibility:** Before adding NuGet packages, verify **.NET 10 / Native AOT** compatibility. If a driver uses `System.Reflection.Emit`, it is forbidden.
2. **DI Integrity:** If an `ObjectDisposedException` occurs, AG must analyze the lifetime of the service. Background Tasks (`IHostedService`) must create a new `IServiceScope` to consume Scoped services.
3. **Domain Alignment:** Cross-reference all new Suite logic with the **Manifesto-B** folder structure to ensure no "Shared Kernel" leakage into "Suites".
4. **Clean Exit:** Ensure all `IDisposable` resources (especially SQL connections in Dapper wrappers) are handled within `using` blocks or C# 14's enhanced using declarations.