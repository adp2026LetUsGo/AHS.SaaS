// src/Cells/Xinfer/AHS.Cell.Xinfer.Domain/Events/ExcursionResolved.cs
using AHS.Common.Domain;

namespace AHS.Cell.Xinfer.Domain.Events;

public record ExcursionResolved(
    Guid ShipmentId,
    Guid ExcursionEventId,
    DateTimeOffset ResolvedAt,
    string ResolutionNote,
    Guid TenantId
) : DomainEvent { public new Guid TenantId { get; init; } = TenantId; }
