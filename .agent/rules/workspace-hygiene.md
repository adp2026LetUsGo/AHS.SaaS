
# Rule: Workspace Architecture (Anti-Gravity)
**ID:** R-002
**Applies to:** Global Monorepo Workspace (`AHS.SaaS/src`)

## 1. Namespace Alignment
Namespaces must strictly follow the functional hierarchy to ensure zero ambiguity:
- **Core:** `AHS.Shared.Kernel.[SubDomain]`
- **Application:** `AHS.Common.Application.[Component]`
- **Suites:** `AHS.Suite.[SuiteName].[BoundedContext].[Layer]`
  *(Example: AHS.Suite.FinTech.Payments.Domain)*

## 2. Decoupling & Purity
- **Domain Purity:** No direct references to `Ml.Inference`, `Microsoft.EntityFrameworkCore`, or external Infrastructure are allowed from any `Domain` or `Kernel` layer.
- **Interfaces:** All external capabilities must be consumed via interfaces defined in the Domain/Kernel and implemented in `src/platform/` or the Suite's `Infrastructure` layer.

## 3. Hygiene & Performance
- **Dead Code:** Agents must remove unused `using` statements, unreachable code, and commented-out blocks immediately.
- **AOT Cleanup:** Immediately replace any detected `System.Reflection` usage with equivalent Source Generators to maintain .NET 10 AOT compliance.