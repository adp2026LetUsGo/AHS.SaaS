---
name: dotnet-service-generator
description: Generates C# service classes and interfaces using .NET 10 conventions for the AHS.SaaS Monorepo.
---

# .NET 10 Service Generator (AHS.SaaS Optimized)
Follow these rules when generating code:

1. **Modern Syntax**: Use file-scoped namespaces and **Primary Constructors** for all Dependency Injection.
2. **Nullable Reference Types**: Strictly respect `#nullable enable`.
3. **Monorepo Placement**: Determine destination based on the **Manifesto-B** structure:
   - **Domain/Interfaces:** `src/shared-kernel/` or `src/suites/[SuiteName]/Domain/`
   - **Application Logic:** `src/common-application/` or `src/suites/[SuiteName]/Application/`
   - **Implementation/Infrastructure:** `src/platform/` or `src/suites/[SuiteName]/Infrastructure/`.
4. **AOT Registration**: Instead of generic python scripts, generate a partial class or an extension method `Add[ProjectName]Services(this IServiceCollection services)` to maintain Native AOT compatibility.
5. **Full Output**: Provide the **COMPLETE** file content for both the Interface and the Implementation. No fragments [cite: 2026-02-14].