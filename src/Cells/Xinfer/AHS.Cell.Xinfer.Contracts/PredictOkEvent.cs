// src/Cells/Xinfer/AHS.Cell.Xinfer.Contracts/PredictOkEvent.cs
using System;
using AHS.Common.Contracts;

namespace AHS.Cell.Xinfer.Contracts;

public record PredictOkEvent : ICellEvent
{
    public Guid           EventId          { get; init; } = Guid.NewGuid();
    public string         TenantSlug       { get; init; } = default!;
    public DateTimeOffset OccurredAt       { get; init; } = DateTimeOffset.UtcNow;
    public string         CellName         => "Xinfer";

    public Guid           ShipmentId       { get; init; }
    public double         RiskScore        { get; init; }
    public double         AccuracyScore    { get; init; }
    public double         ReliabilityScore { get; init; }
}
