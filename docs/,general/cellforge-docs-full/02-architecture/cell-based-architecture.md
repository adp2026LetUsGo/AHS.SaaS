# Cell-Based Architecture

## Principles
- Each cell owns its domain, data, and logic
- Fully independent and deployable
- Communicates only through gRPC contracts
- No shared state or databases
- Deterministic and low-latency execution

## Anti-Patterns
- Shared databases
- Hidden coupling
- Runtime discovery
- Reflection-heavy frameworks
- Cross-cell state
