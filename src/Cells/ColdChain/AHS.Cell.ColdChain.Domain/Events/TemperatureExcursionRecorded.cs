// src/Cells/ColdChain/AHS.Cell.ColdChain.Domain/Events/TemperatureExcursionRecorded.cs
using AHS.Common.Domain;
using AHS.Cell.ColdChain.Domain.Enums;

namespace AHS.Cell.ColdChain.Domain.Events;

public record TemperatureExcursionRecorded(
    Guid ShipmentId,
    string SensorId,
    string ZoneId,
    double ObservedCelsius,
    double MinLimit,
    double MaxLimit,
    DateTimeOffset ExcursionStart,
    ExcursionSeverity Severity,
    Guid TenantId
) : DomainEvent;
