// src/Cells/ColdChain/AHS.Cell.ColdChain.Infrastructure/Persistence/ColdChainEventStore.cs
using System.Text.Json;
using System.Text.Json.Serialization;
using AHS.Common.Domain;

namespace AHS.Cell.ColdChain.Infrastructure.Persistence;

public sealed class ColdChainEventStore
{
    // AOT-safe dispatch table using switch expression
    public string Serialize(DomainEvent evt) => evt switch
    {
        _ => JsonSerializer.Serialize(evt, evt.GetType(), ColdChainInfrastructureJsonContext.Default)
    };
}

[System.Text.Json.Serialization.JsonSerializable(typeof(AHS.Cell.ColdChain.Domain.Events.ShipmentCreated))]
[System.Text.Json.Serialization.JsonSerializable(typeof(AHS.Cell.ColdChain.Domain.Events.TemperatureExcursionRecorded))]
[System.Text.Json.Serialization.JsonSerializable(typeof(AHS.Cell.ColdChain.Domain.Events.ExcursionResolved))]
[System.Text.Json.Serialization.JsonSerializable(typeof(AHS.Cell.ColdChain.Domain.Events.ShipmentSealed))]
[System.Text.Json.Serialization.JsonSerializable(typeof(AHS.Cell.ColdChain.Domain.Events.WhatIfParameterChanged))]
internal partial class ColdChainInfrastructureJsonContext : JsonSerializerContext { }
