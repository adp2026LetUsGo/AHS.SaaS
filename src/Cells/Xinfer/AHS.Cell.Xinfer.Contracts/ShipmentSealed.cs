// src/Cells/Xinfer/AHS.Cell.Xinfer.Contracts/ShipmentSealed.cs
using System;
using AHS.Common.Contracts;

namespace AHS.Cell.Xinfer.Contracts;

public record ShipmentSealed(
    Guid           EventId,
    string         TenantSlug,
    DateTimeOffset OccurredAt,
    Guid           ShipmentId,
    string         FinalStatus,
    string         QualityDecision,
    double         MktCelsius
) : ICellEvent
{
    public string CellName => "Xinfer";
}
