// src/Cells/ColdChain/AHS.Cell.ColdChain.Domain/Events/ExcursionResolved.cs
using AHS.Common.Domain;

namespace AHS.Cell.ColdChain.Domain.Events;

public record ExcursionResolved(
    Guid ShipmentId,
    Guid ExcursionEventId,
    DateTimeOffset ResolvedAt,
    string ResolutionNote,
    Guid TenantId
) : DomainEvent;
