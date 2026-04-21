# Global Data Contract Standard: Style Manual
**Version: 1.0 (2026-04-21)**

This document establishes the mandatory standards for all data contracts within the AHS SaaS ecosystem. Every cell must adhere to these rules to ensure seamless integration and future-proofing.

## 1. Versioning Obligations
All data contracts must include a version suffix in their filenames and internal metadata.
- **Filename**: `[contract_name]_v{n}.md` (e.g., `shipment_v1.md`).
- **Breaking Changes**: Any change that breaks backward compatibility requires a version increment (e.g., `v1` to `v2`).
- **Non-breaking Changes**: Minor updates can be documented within the same version if compatibility is maintained.

## 2. Nomenclature Standards
Consistency in naming is non-negotiable for cross-cell interoperability.
- **Objects & Classes**: Use `PascalCase`. (e.g., `ShipmentHeader`, `SensorReading`).
- **Properties & Fields**: Use `snake_case`. (e.g., `cell_id`, `request_id`, `temperature_celsius`).
- **Endpoints**: Use `kebab-case` for URI segments.

## 3. Platform Independence
Contracts must be **logic-first** and **technology-agnostic**.
- Do not include database-specific types (e.g., `NpgsqlTimeSpan`).
- Do not reference programming language-specific class names unless they are universal primitives.
- Use ISO 8601 for all date and time representations in UTC.

## 4. Native AOT & Serialization
To maintain high-performance benchmarks and Native AOT compliance:
- Favor `System.Text.Json` with Source Generation.
- Avoid reflection-based serialization at all costs.
- Ensure all property names are explicitly mapped to their `snake_case` equivalent in the serializer configuration.

---
*Authorized by: Senior Systems Architect*
