
# Skill: .NET 10 SaaS Core Development (Monorepo Optimized)
# ID: AHS-DOTNET10-CORE

## Context
This skill defines the coding standards for the AHS.SaaS project using .NET 10 and the `.slnx` solution format. It is optimized for the src/ hierarchy (shared-kernel, common-application, platform, suites).

## Coding Guidelines
- **Architecture:** Strictly follow Bounded Contexts. Every suite under `src/suites/` must remain independent.
- **C# 14 Features:** - Use `Primary Constructors` for clean, boilerplate-free DI.
    - Use the `field` keyword for properties when backing fields are needed.
    - Leverage `Generic Attributes` for metadata-driven domain logic.
- **Solution Format:** Maintain the root `AHS.Platform.slnx`. AG must use Solution Folders to group projects by Suite.

## Constraints & Native AOT
- **Zero Reflection:** Do not use runtime reflection. Use **Source Generators** for DI, Logging, and JSON.
- **Purity:** No business logic in Controllers/Minimal APIs. All logic must reside in Domain or Application layers.
- **Compatibility:** Every `.csproj` must pass the AOT analyzer without warnings.

## Core Technical Requirements
- **Financial Calculations:** Use `static abstract` members in interfaces (Generic Math) for precision in FinTech modules.
- **High Performance:** - Use `JsonSourceGenerationOptions` for serialization.
    - Implement `Source Generated Logging` for the Audit Trail in `src/platform/`.
- **Minimal APIs:** Mandatory for the Gateway and Suite-specific endpoints to ensure minimal cold-start.

## Project Structure (SLNX Alignment)
- **Shared Kernel:** Semantic commonality (Value Objects).
- **Common Application:** Result types, MediatR behaviors, and App-level exceptions.
- **Platform:** Identity, Audit, Messaging (AOT-ready wrappers).