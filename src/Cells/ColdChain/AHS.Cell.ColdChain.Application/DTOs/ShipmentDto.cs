// src/Cells/ColdChain/AHS.Cell.ColdChain.Application/DTOs/ShipmentDto.cs
using AHS.Cell.ColdChain.Domain.Enums;
using AHS.Cell.ColdChain.Domain.Ports;
using AHS.Common.Domain;
using InsulationType = AHS.Cell.ColdChain.Domain.Enums.InsulationType;
using AHS.Common.Contracts;

namespace AHS.Cell.ColdChain.Application.DTOs;

public record ShipmentDto(
    Guid           Id,
    Guid           TenantId,
    CargoType       CargoType,
    InsulationType  InsulationType,
    ShipmentStatus  Status,
    string         OriginLocation,
    string         DestinationLocation,
    DateTimeOffset PlannedDeparture,
    DateTimeOffset? ActualDeparture,
    DateTimeOffset? SealedAt,
    bool           IsSealed
);

public record ShipmentSummaryDto(
    Guid           Id,
    CargoType       CargoType,
    ShipmentStatus  Status,
    InsulationType  InsulationType,
    bool           IsSealed,
    Guid           TenantId
);
