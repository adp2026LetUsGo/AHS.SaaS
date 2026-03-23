// src/Cells/ColdChain/AHS.Cell.ColdChain.Domain/Events/WhatIfParameterChanged.cs
using AHS.Common.Domain;

namespace AHS.Cell.ColdChain.Domain.Events;

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
) : DomainEvent;
