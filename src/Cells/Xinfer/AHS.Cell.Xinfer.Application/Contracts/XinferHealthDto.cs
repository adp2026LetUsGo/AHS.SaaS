// src/Cells/Xinfer/AHS.Cell.Xinfer.Application.Contracts/XinferHealthDto.cs
using System;

namespace AHS.Cell.Xinfer.Application.Contracts;

public enum XinferLifecycleState
{
    Operational,
    Degraded,
    RetrainingRequired,
    Maintenance
}

public record XinferHealthDto
{
    public XinferLifecycleState CellState          { get; init; }
    public int                  ActiveModelVersion { get; init; }
    public DateTimeOffset       LastRetrainAt      { get; init; }
    public int                  PendingOutboxCount { get; init; }
    public bool                 Healthy            { get; init; }
}
