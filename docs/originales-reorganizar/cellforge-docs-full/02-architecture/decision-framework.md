# Architectural Decision Framework

## Create a new cell when:
- Domain is independent
- Scaling differs
- Security boundary differs
- High change frequency
- Performance isolation needed

## Extend existing cell when:
- Same domain
- Strong cohesion
- No new scaling or security concerns

## Reject changes when:
- Introduces cross-cell coupling
- Requires shared DB
- Breaks AOT constraints
- Adds unnecessary complexity

**Golden Rule:** Prefer simplicity and isolation over reuse.
