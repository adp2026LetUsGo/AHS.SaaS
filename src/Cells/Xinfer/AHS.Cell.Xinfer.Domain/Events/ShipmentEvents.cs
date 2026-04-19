using AHS.Common.Domain;
using AHS.Cell.Xinfer.Domain.ValueObjects;

namespace AHS.Cell.Xinfer.Domain.Events;

public record ShipmentProfileCreated(
    Guid ShipmentId,
    Guid TenantId,
    ShipmentIdentity Identity,
    CarrierProfile Carrier
) : DomainEvent {
    public new Guid TenantId { get; init; } = TenantId;
}

public record ReadinessValidated(
    Guid ShipmentId,
    string Status,
    string[] Errors,
    string[] Warnings
) : DomainEvent;

public record DivergenceDetected(
    Guid ShipmentId,
    bool HasDivergence,
    string[] Divergences
) : DomainEvent;

public record HistoricalDatasetSelected(
    Guid ShipmentId,
    string Season,
    int RecordCount,
    string? IncludedCarrier
) : DomainEvent;

public record RetrainDecisionMade(
    Guid ShipmentId,
    bool ShouldRetrain,
    string Reason,
    string Severity
) : DomainEvent;

public record ModelRetrained(
    Guid ShipmentId,
    Guid ModelVersionId,
    int VersionNumber
) : DomainEvent;

public record PredictionCompleted(
    Guid ShipmentId,
    Guid PredictionId,
    double RiskScore,
    string RiskLevel,
    double PessimisticTtfHours,
    XaiDna XaiDna
) : DomainEvent;

public record RecommendationsGenerated(
    Guid ShipmentId,
    int RecommendationCount
) : DomainEvent;
