// src/Cells/ColdChain/AHS.Cell.ColdChain.Application/Commands/SealShipmentCommand.cs
using AHS.Common.Application;
using AHS.Cell.ColdChain.Domain.Enums;

namespace AHS.Cell.ColdChain.Application.Commands;

public record SealShipmentCommand(
    Guid           ShipmentId,
    ShipmentStatus  FinalStatus,
    QualityDecision QualityDecision,
    Guid           TenantId,
    Guid           SignedById,
    string         SignedByName,
    string         ReasonForChange
) : SignedCommand(SignedById, SignedByName, ReasonForChange);
