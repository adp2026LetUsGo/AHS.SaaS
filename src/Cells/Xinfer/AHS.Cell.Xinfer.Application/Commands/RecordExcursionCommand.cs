// src/Cells/Xinfer/AHS.Cell.Xinfer.Application/Commands/RecordExcursionCommand.cs
using AHS.Common.Application;
using AHS.Cell.Xinfer.Domain.Enums;

namespace AHS.Cell.Xinfer.Application.Commands;

public record RecordExcursionCommand(
    Guid           ShipmentId,
    string         SensorId,
    string         ZoneId,
    double         ObservedCelsius,
    double         MinLimit,
    double         MaxLimit,
    ExcursionSeverity Severity,
    Guid           TenantId,
    Guid           SignedById,
    string         SignedByName,
    string         ReasonForChange
) : SignedCommand(SignedById, SignedByName, ReasonForChange);
