// src/Cells/ColdChain/AHS.Cell.ColdChain.Contracts/ShipmentSealed.cs
using System;
using AHS.Common.Contracts;

namespace AHS.Cell.ColdChain.Contracts;

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
    public string CellName => "ColdChain";
}
