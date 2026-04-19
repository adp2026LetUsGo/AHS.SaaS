// src/Cells/Xinfer/AHS.Cell.Xinfer.Application/Commands/SubmitShipmentCommand.cs
using System;
using AHS.Common.Application;
using AHS.Cell.Xinfer.Domain.Enums;
using AHS.Common.Domain;

namespace AHS.Cell.Xinfer.Application.Commands;

public record SubmitShipmentCommand(
    Guid           TenantId,
    string         TenantSlug,
    Guid           ShipmentId,
    string         CarrierId,
    string         RouteId,
    CargoType      CargoType,
    string         PackagingType,
    DateTimeOffset PlannedDeparture,
    double         EstimatedDurationHours,
    double         ForecastMaxCelsius,
    double         ForecastMinCelsius,
    double         ForecastHumidityPct,
    double         CarrierReliabilityScore,
    int            CarrierIncidents12M,
    Guid           SignedById,
    string         SignedByName,
    string         ReasonForChange
) : SignedCommand(SignedById, SignedByName, ReasonForChange);
