// src/Cells/Xinfer/AHS.Cell.Xinfer.Contracts/RetrainRequiredEvent.cs
using System;
using AHS.Common.Contracts;

namespace AHS.Cell.Xinfer.Contracts;

public record RetrainRequiredEvent : ICellEvent
{
    public Guid           EventId    { get; init; } = Guid.NewGuid();
    public string         TenantSlug { get; init; } = default!;
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
    public string         CellName   => "Xinfer";

    public string         Reason     { get; init; } = default!;
    public string         Severity   { get; init; } = default!;
}
