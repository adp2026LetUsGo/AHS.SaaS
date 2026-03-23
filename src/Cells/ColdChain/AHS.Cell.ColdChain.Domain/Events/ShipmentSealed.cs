// src/Cells/ColdChain/AHS.Cell.ColdChain.Domain/Events/ShipmentSealed.cs
using AHS.Common.Domain;
using AHS.Cell.ColdChain.Domain.Enums;

namespace AHS.Cell.ColdChain.Domain.Events;

public record ShipmentSealed(
    Guid ShipmentId,
    ShipmentStatus FinalStatus,
    double MktCelsius,
    QualityDecision QualityDecision,
    Guid TenantId
) : DomainEvent;
