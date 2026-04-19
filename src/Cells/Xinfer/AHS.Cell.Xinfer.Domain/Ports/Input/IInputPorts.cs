using AHS.Common.Application;
using AHS.Cell.Xinfer.Domain.ValueObjects;
using AHS.Cell.Xinfer.Domain.Entities;
using AHS.Cell.Xinfer.Domain.DomainServices;

namespace AHS.Cell.Xinfer.Domain.Ports.Input;

public record ShipmentInput(
    string ProductName,
    string ProductCategory, 
    string PackagingType,
    string RouteId,
    DateTimeOffset DepartureDate,
    double ForecastMaxCelsius,
    double ForecastMinCelsius,
    double ForecastHumidityPct,
    string CarrierId,
    double CarrierReliabilityScore,
    int CarrierIncidents12M,
    double EstimatedDurationHours,
    int DepartureHour
);

public record XinferResult(
    ReadinessResult Readiness,
    bool DivergenceDetected,
    RetrainDecision RetrainDecision,
    PredictionResult? Prediction,
    IReadOnlyList<Recommendation> Recommendations
);

public interface IShipmentInputPort
{
    Task<XinferResult> ProcessAsync(
        ShipmentInput input,
        SignedCommand command,
        CancellationToken ct);
}

public interface IXinferQueryPort
{
    // Queries will be defined here as needed
}
