// src/Cells/Xinfer/AHS.Cell.Xinfer.Application/Oracle/OracleRequest.cs
using AHS.Cell.Xinfer.Domain.Enums;
using AHS.Common.Domain;
using InsulationType = AHS.Cell.Xinfer.Domain.Enums.InsulationType;

namespace AHS.Cell.Xinfer.Application.Oracle;

public record OracleRequest(
    string         RouteId,
    CargoType       CargoType,
    InsulationType  InsulationType,
    string         RouteCategory,
    string         CarrierId,
    float          CarrierReliability,
    int            CarrierIncidents12M,
    float          ForecastMaxCelsius,
    float          ForecastMinCelsius,
    float          ForecastHumidityPct,
    float          SetpointMinCelsius,
    float          SetpointMaxCelsius,
    float          PhysicalTtfHours
);
