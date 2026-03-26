// src/Cells/Xinfer/AHS.Cell.Xinfer.Application/Commands/RegisterShipmentCommand.cs
using AHS.Common.Application;
using AHS.Cell.Xinfer.Domain.Enums;
using AHS.Common.Domain;
using InsulationType = AHS.Cell.Xinfer.Domain.Enums.InsulationType;

namespace AHS.Cell.Xinfer.Application.Commands;

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
