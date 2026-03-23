// src/Cells/ColdChain/AHS.Cell.ColdChain.Contracts/ShipmentExcursionDetected.cs
using System;
using AHS.Common.Contracts;

namespace AHS.Cell.ColdChain.Contracts;

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
    public string CellName => "ColdChain";
}
