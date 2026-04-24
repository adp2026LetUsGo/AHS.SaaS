using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AHS.Cell.Xinfer.Application.Contracts;

public sealed record InferenceInput_v1(
    [property: JsonPropertyName("route_id")] string RouteId,
    [property: JsonPropertyName("carrier")] string Carrier,
    [property: JsonPropertyName("external_temp_avg")] float ExternalTempAvg,
    [property: JsonPropertyName("transit_time_hrs")] float TransitTimeHrs,
    [property: JsonPropertyName("packaging_type")] string PackagingType,
    [property: JsonPropertyName("departure_timestamp")] DateTimeOffset DepartureTimestamp
);

public sealed record InferenceOutput_v1(
    [property: JsonPropertyName("risk_score")]       float RiskScore,
    [property: JsonPropertyName("risk_level")]       string RiskLevel,
    [property: JsonPropertyName("confidence_score")] float ConfidenceScore,
    [property: JsonPropertyName("influence_factors")] IReadOnlyList<InfluenceFactorDto> InfluenceFactors,
    [property: JsonPropertyName("model_metadata")]   ModelMetadataDto ModelMetadata
);

public sealed record InfluenceFactorDto(
    [property: JsonPropertyName("factor")] string Factor,
    [property: JsonPropertyName("weight")] float Weight,
    [property: JsonPropertyName("sentiment")] string Sentiment
);

public sealed record ModelMetadataDto(
    [property: JsonPropertyName("model_version")] string ModelVersion,
    [property: JsonPropertyName("trained_at")] DateTimeOffset TrainedAt,
    [property: JsonPropertyName("accuracy_metric")] float AccuracyMetric
);
