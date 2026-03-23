// src/Cells/ColdChain/AHS.Cell.ColdChain.Application/Commands/RecordExcursionCommand.cs
using AHS.Common.Application;
using AHS.Cell.ColdChain.Domain.Enums;

namespace AHS.Cell.ColdChain.Application.Commands;

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
