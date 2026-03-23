// src/Cells/ColdChain/AHS.Cell.ColdChain.Application/Commands/RegisterShipmentCommand.cs
using AHS.Common.Application;
using AHS.Cell.ColdChain.Domain.Enums;
using AHS.Common.Domain;
using InsulationType = AHS.Cell.ColdChain.Domain.Enums.InsulationType;

namespace AHS.Cell.ColdChain.Application.Commands;

public record RegisterShipmentCommand(
    CargoType       CargoType,
    InsulationType  InsulationType,
    string         OriginLocation,
    string         DestinationLocation,
    DateTimeOffset PlannedDeparture,
    Guid           TenantId,
    Guid           SignedById,
    string         SignedByName,
    string         ReasonForChange
) : SignedCommand(SignedById, SignedByName, ReasonForChange);
