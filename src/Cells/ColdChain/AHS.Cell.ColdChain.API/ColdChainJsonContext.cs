// src/Cells/ColdChain/AHS.Cell.ColdChain.API/ColdChainJsonContext.cs
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using AHS.Cell.ColdChain.Application.DTOs;
using AHS.Cell.ColdChain.Application.Oracle;
using AHS.Cell.ColdChain.Domain.Enums;
using AHS.Common.Domain;
using InsulationType = AHS.Cell.ColdChain.Domain.Enums.InsulationType;

namespace AHS.Cell.ColdChain.API;

[JsonSerializable(typeof(ShipmentDto))]
[JsonSerializable(typeof(ShipmentSummaryDto))]
[JsonSerializable(typeof(ColdChainReportDto))]
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
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = false,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public partial class ColdChainJsonContext : JsonSerializerContext { }

public record CreateShipmentRequest(
    CargoType CargoType,
    InsulationType InsulationType,
    string OriginLocation,
    string DestinationLocation,
    DateTimeOffset PlannedDeparture,
    string ReasonForChange
);

public record SealShipmentRequest(
    ShipmentStatus FinalStatus,
    QualityDecision QualityDecision,
    string ReasonForChange
);
