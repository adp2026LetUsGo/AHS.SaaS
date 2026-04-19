# ADR-008: Architectural Alignment Strategy

**Status**: Accepted
**Date**: 2026-Q2
**Deciders**: C1 Architect, C2 Lead Engineer
**Supersedes**: Ninguno (establece guía transversal)

## Context

AHS V3.1 introduces multiple Cells, each with autonomy in deployment, contracts, and UI.
Previous ADRs have defined:

* ADR-001: Database-per-Cell + RLS for tenant isolation
* ADR-002: Native AOT compilation
* ADR-003: Azure Service Bus as inter-Cell channel
* ADR-004: PostgreSQL as the standard DB
* ADR-005: HybridCache standard
* ADR-006: SignedCommand universal for state changes
* ADR-007: Cell Contracts versioning
* ADR-008: UI component boundaries

Despite these standards, divergence risks exist:

1. Cells implementing different patterns for DI, caching, or messaging.
2. UI components being duplicated across Cells rather than using shared libraries.
3. Contract versioning inconsistencies leading to consumer breakages.
4. Operational drift (monitoring, logging, CI/CD pipelines) causing cross-cell discrepancies.

Without explicit alignment guidance, the platform risks **technical debt, inconsistent UX, and operational overhead**.

## Decision

Adopt a **formal Architectural Alignment Strategy**:

1. **Cross-Cell Standards Enforcement**

   * Every Cell must implement:

     * Native AOT compilation (`PublishAot=true`)
     * HybridCache configuration per ADR-005
     * SignedCommand enforcement per ADR-006
   * CI pipelines include **linting & validation** for standard patterns (DI registration, logging, telemetry, cache usage).

2. **Contracts Governance**

   * Follow ADR-007 strictly: all breaking changes require `_V[N]` version suffix.
   * A **Contracts Compliance CI** job validates that all published events conform to versioning rules before merge.

3. **UI Alignment**

   * All Cells must consume shared components from `AHS.Web.Common` wherever possible (ADR-008).
   * Cell-specific components allowed only if they import domain types from the Cell.
   * SemVer enforced on shared UI library.
   * Automated **UI diff checks** per release to detect visual regressions.

4. **Operational Alignment**

   * Logging, metrics, and alerting must adhere to platform-wide standards:

     * Log format, correlation IDs, telemetry keys
     * Container image tagging conventions
     * CI/CD job names and pipeline stages

5. **Periodic Architectural Reviews**

   * Quarterly review of all Cells to ensure:

     * Compliance with standards (AOT, caching, messaging, contracts, UI)
     * Detection of drift or duplication
     * Documentation updates in ADR repository

6. **Tooling & Automation**

   * Provide templates and scripts for:

     * CI/CD pipeline setup per Cell
     * HybridCache, telemetry, and logging boilerplate
     * Contract publishing and versioning checks
   * Integrate into GitHub Actions or Azure DevOps pipelines.

## Consequences

**Positive:**

* Minimized divergence across Cells
* Faster onboarding of new Cells (reusable patterns, templates)
* Reduced risk of breaking consumers due to contract misalignment
* Consistent UX and GxP compliance across the platform
* Operational consistency across deployments

**Negative:**

* Additional CI/CD complexity for validation jobs
* Slight overhead for Cell developers to comply with alignment checks
* Shared UI and contracts create tighter coupling (requires careful SemVer management)
* Periodic review requires time from architects and leads

## Implementation

* Update ADR repo: add this ADR as reference for all new Cells
* Extend CI pipelines:

  * `validate-adr-alignment.yml` workflow
  * Checks for:

    * Native AOT compliance
    * HybridCache usage
    * SignedCommand usage
    * Contracts versioning
    * UI component usage
* Create onboarding docs with **alignment checklist** for developers
