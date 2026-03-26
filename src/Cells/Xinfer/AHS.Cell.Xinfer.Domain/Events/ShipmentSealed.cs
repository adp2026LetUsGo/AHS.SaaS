// src/Cells/Xinfer/AHS.Cell.Xinfer.Domain/Events/ShipmentSealed.cs
using AHS.Common.Domain;
using AHS.Cell.Xinfer.Domain.Enums;

namespace AHS.Cell.Xinfer.Domain.Events;

public record ShipmentSealed(
    Guid ShipmentId,
    ShipmentStatus FinalStatus,
    double MktCelsius,
    QualityDecision QualityDecision,
    Guid TenantId
) : DomainEvent { public new Guid TenantId { get; init; } = TenantId; }
