# CellForge Profile (AI + Human)

## Architecture
- Cell-Based Architecture
- Deterministic, isolated cells
- gRPC for inter-cell communication

## Tech Stack
- .NET 10 (Native AOT)
- C# 14
- EF Core 10 (compiled models)
- Microsoft Entra ID
- Azure Workload Identity
- Chiseled containers

## Code Style
- Strong typing
- Explicit code
- camelCase for variables
- PascalCase for classes/methods
- Immutable and small functions

## Tools
- VS 2026
- Git / GitHub
- Docker + Chiseled containers

## Permanent Goals
- Minimize startup time
- Maximize throughput
- Security-first
- Observability by design
- Maintainability

## Constraints
- Only LTS libraries
- No reflection or dynamic code
- No shared state across cells
- Minimal containers
