// src/Cells/Xinfer/AHS.Cell.Xinfer.Contracts/ReadinessFailEvent.cs
using System;
using AHS.Common.Contracts;

namespace AHS.Cell.Xinfer.Contracts;

public record ReadinessFailEvent : ICellEvent
{
    public Guid           EventId    { get; init; } = Guid.NewGuid();
    public string         TenantSlug { get; init; } = default!;
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
    public string         CellName   => "Xinfer";

    public Guid           ShipmentId { get; init; }
    public string         Reason     { get; init; } = default!;
}
