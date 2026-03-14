using System.Text.Json.Serialization;
using AHS.Common.Models;
using System.Collections.Generic;

namespace AHS.Common.Serialization;

[JsonSourceGenerationOptions(WriteIndented = false, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(PredictionResponse))]
[JsonSerializable(typeof(Dictionary<string, float>))]
[JsonSerializable(typeof(List<PredictionResponse>))]
[JsonSerializable(typeof(PredictiveShieldMetrics))]
[JsonSerializable(typeof(List<PredictiveShieldMetrics>))]
[JsonSerializable(typeof(AuditEvent))]
[JsonSerializable(typeof(List<AuditEvent>))]
[JsonSerializable(typeof(ShipmentRiskProfile))]
[JsonSerializable(typeof(LogisticsOracleResult))]
public partial class AotJsonContext : JsonSerializerContext { }
