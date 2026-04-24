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

    /// <summary>
    /// GxP Ledger — ALCOA+ Completeness.
    /// Model certainty at inference time. Values &lt; 0.6 indicate low confidence
    /// and must be flagged in the audit trail for human review per 21 CFR Part 11.
    /// </summary>
    public double         ConfidenceScore  { get; init; }
}
