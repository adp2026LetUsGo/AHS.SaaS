// src/Cells/Xinfer/AHS.Cell.Xinfer.Domain/Events/PredictionRecorded.cs
using AHS.Common.Domain;
using System;

namespace AHS.Cell.Xinfer.Domain.Events;

public record PredictionRecorded(
    Guid ShipmentId,
    double RiskScore,
    double AccuracyScore,
    double ReliabilityScore,
    Guid TenantId
) : DomainEvent
{
    public new Guid TenantId { get; init; } = TenantId;
}
