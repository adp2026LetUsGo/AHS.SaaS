// src/Foundation/AHS.Common/Serialization/AotJsonContext.cs
using System.Text.Json.Serialization;
using AHS.Common.Domain;
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
[JsonSerializable(typeof(PredictRiskRequest))]
[JsonSerializable(typeof(PredictRiskResponse))]
public partial class AotJsonContext : JsonSerializerContext { }
