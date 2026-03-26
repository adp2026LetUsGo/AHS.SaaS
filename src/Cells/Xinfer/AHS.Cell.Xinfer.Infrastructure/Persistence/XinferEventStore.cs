// src/Cells/Xinfer/AHS.Cell.Xinfer.Infrastructure/Persistence/XinferEventStore.cs
using System.Text.Json;
using System.Text.Json.Serialization;
using AHS.Common.Domain;

namespace AHS.Cell.Xinfer.Infrastructure.Persistence;

public sealed class XinferEventStore
{
    // AOT-safe dispatch table using switch expression
    public static string Serialize(DomainEvent evt) => evt switch
    {
        _ => JsonSerializer.Serialize(evt, evt.GetType(), XinferInfrastructureJsonContext.Default)
    };
}

[System.Text.Json.Serialization.JsonSerializable(typeof(AHS.Cell.Xinfer.Domain.Events.ShipmentCreated))]
[System.Text.Json.Serialization.JsonSerializable(typeof(AHS.Cell.Xinfer.Domain.Events.TemperatureExcursionRecorded))]
[System.Text.Json.Serialization.JsonSerializable(typeof(AHS.Cell.Xinfer.Domain.Events.ExcursionResolved))]
[System.Text.Json.Serialization.JsonSerializable(typeof(AHS.Cell.Xinfer.Domain.Events.ShipmentSealed))]
[System.Text.Json.Serialization.JsonSerializable(typeof(AHS.Cell.Xinfer.Domain.Events.WhatIfParameterChanged))]
internal sealed partial class XinferInfrastructureJsonContext : JsonSerializerContext { }
