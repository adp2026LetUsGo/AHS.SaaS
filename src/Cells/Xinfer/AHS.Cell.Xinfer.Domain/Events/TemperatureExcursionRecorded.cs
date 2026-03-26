// src/Cells/Xinfer/AHS.Cell.Xinfer.Domain/Events/TemperatureExcursionRecorded.cs
using AHS.Common.Domain;
using AHS.Cell.Xinfer.Domain.Enums;

namespace AHS.Cell.Xinfer.Domain.Events;

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
) : DomainEvent { public new Guid TenantId { get; init; } = TenantId; }
