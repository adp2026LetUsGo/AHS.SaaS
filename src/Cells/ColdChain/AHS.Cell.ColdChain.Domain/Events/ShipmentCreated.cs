// src/Cells/ColdChain/AHS.Cell.ColdChain.Domain/Events/ShipmentCreated.cs
using AHS.Common.Domain;
using AHS.Cell.ColdChain.Domain.Enums;
using InsulationType = AHS.Cell.ColdChain.Domain.Enums.InsulationType;

namespace AHS.Cell.ColdChain.Domain.Events;

public record ShipmentCreated(
    Guid ShipmentId,
    CargoType CargoType,
    InsulationType InsulationType,
    string OriginLocation,
    string DestinationLocation,
    DateTimeOffset PlannedDeparture,
    Guid TenantId
) : DomainEvent;
