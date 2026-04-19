// src/Cells/Xinfer/AHS.Cell.Xinfer.API/XinferJsonContext.cs
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using AHS.Cell.Xinfer.Application.DTOs;
using AHS.Cell.Xinfer.Application.Oracle;
using AHS.Cell.Xinfer.Domain.Enums;
using AHS.Common.Domain;
using InsulationType = AHS.Cell.Xinfer.Domain.Enums.InsulationType;

namespace AHS.Cell.Xinfer.API;

[JsonSerializable(typeof(ShipmentDto))]
[JsonSerializable(typeof(ShipmentSummaryDto))]
[JsonSerializable(typeof(XinferReportDto))]
[JsonSerializable(typeof(List<ShipmentSummaryDto>))]
[JsonSerializable(typeof(CreateShipmentRequest))]
[JsonSerializable(typeof(SealShipmentRequest))]
[JsonSerializable(typeof(OracleRequest))]
[JsonSerializable(typeof(OracleResult))]
[JsonSerializable(typeof(CargoType))]
[JsonSerializable(typeof(InsulationType))]
[JsonSerializable(typeof(ShipmentStatus))]
[JsonSerializable(typeof(QualityDecision))]
[JsonSerializable(typeof(ProblemDetails))]
[JsonSerializable(typeof(XinferHealthDto))]
[JsonSerializable(typeof(XinferLifecycleState))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = false,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal sealed partial class XinferJsonContext : JsonSerializerContext { }

internal sealed record CreateShipmentRequest(
    CargoType CargoType,
    InsulationType InsulationType,
    string OriginLocation,
    string DestinationLocation,
    DateTimeOffset PlannedDeparture,
    string ReasonForChange
);

internal sealed record SealShipmentRequest(
    ShipmentStatus FinalStatus,
    QualityDecision QualityDecision,
    string ReasonForChange
);
