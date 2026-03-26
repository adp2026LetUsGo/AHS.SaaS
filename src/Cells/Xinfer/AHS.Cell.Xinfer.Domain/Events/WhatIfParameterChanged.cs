// src/Cells/Xinfer/AHS.Cell.Xinfer.Domain/Events/WhatIfParameterChanged.cs
using AHS.Common.Domain;

namespace AHS.Cell.Xinfer.Domain.Events;

public record WhatIfParameterChanged(
    Guid ShipmentId,
    string ParameterName,
    string PreviousValue,
    string NewValue,
    string ReasonForChange,
    Guid SignedById,
    string SignedByName,
    DateTimeOffset SignedAt,
    Guid TenantId
) : DomainEvent { public new Guid TenantId { get; init; } = TenantId; }
