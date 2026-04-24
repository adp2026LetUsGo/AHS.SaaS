# Data Contract: Xinfer Prediction Input
**Contract Version: input_v1**

This contract defines the schema for the input payload required by the Xinfer cell to generate a thermal excursion risk prediction.

## 1. Schema Definition

| Property | Type | Mandatory | Range / Validation | Description |
| :--- | :--- | :---: | :--- | :--- |
| `route_id` | String | Yes | UUID or AlphaNumeric | Unique identifier of the transport route. |
| `carrier` | String | Yes | - | Name of the carrier service provider. |
| `external_temp_avg` | Float | Yes | -50.0 to +60.0 | Expected average external temperature in Celsius. |
| `transit_time_hrs` | Float | Yes | 0.5 to 720.0 | Expected duration of the transit in decimal hours. |
| `packaging_type` | Enum | Yes | `standard`, `vip`, `active`, `passive` | Type of thermal insulation and packaging used. |
| `departure_timestamp` | String | Yes | ISO 8601 (UTC) | Planned departure date and time. |

## 2. Validations
- **Range Enforcement**: Values outside the specified ranges for `external_temp_avg` and `transit_time_hrs` should be rejected with a `400 Bad Request` status.
- **Enumeration**: The `packaging_type` must be one of the four specified values.
- **Immutability**: Once an inference is requested, the input values should be logged as immutable for audit purposes.

## 3. JSON Example
```json
{
  "route_id": "RT-LHR-JFK-402",
  "carrier": "GlobalLogistics-Air",
  "external_temp_avg": 24.5,
  "transit_time_hrs": 8.5,
  "packaging_type": "vip",
  "departure_timestamp": "2026-05-12T14:30:00Z"
}
```

---
*Authorized by: Senior Data Engineer*
