// src/Cells/Xinfer/AHS.Cell.Xinfer.Domain/Events/ShipmentCreated.cs
using AHS.Common.Domain;
using AHS.Cell.Xinfer.Domain.Enums;
using InsulationType = AHS.Cell.Xinfer.Domain.Enums.InsulationType;

namespace AHS.Cell.Xinfer.Domain.Events;

public record ShipmentCreated(
    Guid ShipmentId,
    CargoType CargoType,
    InsulationType InsulationType,
    string OriginLocation,
    string DestinationLocation,
    DateTimeOffset PlannedDeparture,
    Guid TenantId
) : DomainEvent { public new Guid TenantId { get; init; } = TenantId; }
