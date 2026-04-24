
# Workflow: Project Initialization & Skill Validation
# ID: AHS-INIT-001 (Version 2026.1 - Monorepo Optimized)

## Objective
Ensure the .NET 10 environment, .slnx solution, and Agent Skills are perfectly aligned to prevent legacy errors and path confusion.

## Step 1: Environment & Root Validation
- Verify .NET 10 SDK is installed.
- **Root Check:** Confirm current directory is `C:\Users\armando\Documents\_AHS\projects\AHS.SaaS`.
- **Format Check:** Validate that the solution file is exactly `AHS.Platform.slnx` (XML format).
- **Memory Check:** Confirm presence of `.agent/rules/SaaS-Learned-Factors.md`.

## Step 2: Agentic DNA Loading
- Load **`DotNet10-SaaS-Core.md`** (Updated from MicroSaaS version).
- Apply **"English Only"** and **"Full File Content"** policies.
- Index the **30 Bounded Contexts** in `BLUEPRINT.md` and map them to `src/suites/`.

## Step 3: Architecture & AOT Guardrails
- **Package Management:** Update `Directory.Packages.props` for .NET 10 compatibility.
- **Concurrency Check:** Force-verify that `ITenantService` uses **Scoped** registration to prevent the Jan 2026 concurrency bug.
- **AOT Pre-scan:** Ensure `<PublishAot>true</PublishAot>` is present in all project files.

## Success Criteria
- AG confirms all 5 optimized Skills are indexed and active.
- The `AHS.Platform.slnx` is correctly mapped to the `src/` hierarchy.
- Project compiles with zero AOT-compatibility warnings.