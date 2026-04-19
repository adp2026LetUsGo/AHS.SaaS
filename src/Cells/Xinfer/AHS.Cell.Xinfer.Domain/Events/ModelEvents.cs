using AHS.Common.Domain;

namespace AHS.Cell.Xinfer.Domain.Events;

public record ModelVersionCreated(
    Guid ModelId,
    Guid TenantId,
    int VersionNumber,
    DateTimeOffset TrainedAt,
    int TrainingRecordsCount,
    double AccuracyScore,
    string Reason
) : DomainEvent {
    public new Guid TenantId { get; init; } = TenantId;
}

public record ModelActivated(Guid ModelId) : DomainEvent;
public record ModelDeactivated(Guid ModelId) : DomainEvent;
