// src/Cells/Xinfer/AHS.Cell.Xinfer.Contracts/ShipmentExcursionDetected.cs
using System;
using AHS.Common.Contracts;

namespace AHS.Cell.Xinfer.Contracts;

public record ShipmentExcursionDetected(
    Guid           EventId,
    string         TenantSlug,
    DateTimeOffset OccurredAt,
    Guid           ShipmentId,
    string         ZoneId,
    double         ObservedCelsius,
    string         Severity
) : ICellEvent
{
    public string CellName => "Xinfer";
}
