// src/Cells/Xinfer/AHS.Cell.Xinfer.Application/Commands/SealShipmentCommand.cs
using AHS.Common.Application;
using AHS.Cell.Xinfer.Domain.Enums;

namespace AHS.Cell.Xinfer.Application.Commands;

public record SealShipmentCommand(
    Guid           ShipmentId,
    ShipmentStatus  FinalStatus,
    QualityDecision QualityDecision,
    Guid           TenantId,
    Guid           SignedById,
    string         SignedByName,
    string         ReasonForChange
) : SignedCommand(SignedById, SignedByName, ReasonForChange);
