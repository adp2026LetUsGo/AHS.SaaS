// src/Cells/Xinfer/AHS.Cell.Xinfer.Application/XinferJsonContext.cs
using System.Text.Json.Serialization;
using AHS.Cell.Xinfer.Application.Contracts;
using AHS.Cell.Xinfer.Application.Oracle;
using AHS.Cell.Xinfer.Domain.Enums;
using AHS.Cell.Xinfer.Domain.Events;
using AHS.Cell.Xinfer.Domain.ValueObjects;
using AHS.Cell.Xinfer.Application.Persistence.Entities;
using AHS.Common.Domain;
using InsulationType = AHS.Cell.Xinfer.Domain.Enums.InsulationType;

namespace AHS.Cell.Xinfer.Application;

[JsonSerializable(typeof(ShipmentDto))]
[JsonSerializable(typeof(ShipmentSummaryDto))]
[JsonSerializable(typeof(XinferReportDto))]
[JsonSerializable(typeof(List<ShipmentSummaryDto>))]
[JsonSerializable(typeof(CreateShipmentRequest))]
[JsonSerializable(typeof(SealShipmentRequest))]
[JsonSerializable(typeof(OracleRequest))]
[JsonSerializable(typeof(OracleResult))]
[JsonSerializable(typeof(CargoType))]
[JsonSerializable(typeof(InsulationType))]
[JsonSerializable(typeof(ShipmentStatus))]
[JsonSerializable(typeof(QualityDecision))]
[JsonSerializable(typeof(XinferHealthDto))]
[JsonSerializable(typeof(XinferLifecycleState))]

// Domain Events for Outbox (Native AOT)
[JsonSerializable(typeof(ShipmentProfileCreated))]
[JsonSerializable(typeof(ReadinessValidated))]
[JsonSerializable(typeof(DivergenceDetected))]
[JsonSerializable(typeof(HistoricalDatasetSelected))]
[JsonSerializable(typeof(RetrainDecisionMade))]
[JsonSerializable(typeof(ModelRetrained))]
[JsonSerializable(typeof(PredictionCompleted))]
[JsonSerializable(typeof(RecommendationsGenerated))]
[JsonSerializable(typeof(ModelVersionCreated))]
[JsonSerializable(typeof(ModelActivated))]
[JsonSerializable(typeof(ModelDeactivated))]

// Explicitly resolve collisions for XaiDna (Domain vs Application)
[JsonSerializable(typeof(AHS.Cell.Xinfer.Domain.ValueObjects.XaiDna), TypeInfoPropertyName = "DomainXaiDna")]
[JsonSerializable(typeof(AHS.Cell.Xinfer.Application.Oracle.XaiDna), TypeInfoPropertyName = "OracleXaiDna")]

// Inference DTOs
[JsonSerializable(typeof(InferenceInput_v1))]
[JsonSerializable(typeof(InferenceOutput_v1))]
[JsonSerializable(typeof(InfluenceFactorDto))]
[JsonSerializable(typeof(ModelMetadataDto))]

// Standard Envelope DTOs
[JsonSerializable(typeof(StandardEnvelope<InferenceOutput_v1>))]
[JsonSerializable(typeof(StandardEnvelope<object>))]
[JsonSerializable(typeof(EnvelopeMetadata))]
[JsonSerializable(typeof(EnvelopeStatus))]

// Entities
[JsonSerializable(typeof(OutboxMessage))]

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
    WriteIndented = false,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public sealed partial class XinferJsonContext : JsonSerializerContext { }

public sealed record CreateShipmentRequest(
    CargoType CargoType,
    InsulationType InsulationType,
    string OriginLocation,
    string DestinationLocation,
    DateTimeOffset PlannedDeparture,
    string ReasonForChange
);

public sealed record SealShipmentRequest(
    ShipmentStatus FinalStatus,
    QualityDecision QualityDecision,
    string ReasonForChange
);
