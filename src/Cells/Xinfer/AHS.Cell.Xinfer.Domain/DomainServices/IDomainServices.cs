using AHS.Cell.Xinfer.Domain.Entities;
using AHS.Cell.Xinfer.Domain.ValueObjects;
using AHS.Cell.Xinfer.Domain.Aggregates;

namespace AHS.Cell.Xinfer.Domain.DomainServices;

public interface IReadinessValidator
{
    Task<ReadinessResult> ValidateAsync(ShipmentIdentity identity, CarrierProfile carrier, double forecastMax, double forecastMin, double forecastHumidity, double duration, int departureHour, CancellationToken ct);
}

public record DivergenceReport(bool HasDivergence, string[] Divergences);

public interface IDivergenceDetector
{
    Task<DivergenceReport> DetectAsync(ShipmentIdentity identity, CarrierProfile carrier, double forecastMax, double forecastMin, double forecastHumidity, double duration, int departureHour, CancellationToken ct);
}

public record HistoricalDataset(
    IReadOnlyList<HistoricalRecord> Records,
    string Season,
    int RecordCount,
    string? IncludedCarrier
);

public interface IHistoricalSelector
{
    Task<HistoricalDataset> SelectAsync(ShipmentIdentity identity, CarrierProfile carrier, bool includeCarrier, CancellationToken ct);
}

public interface IRetrainDecider
{
    Task<RetrainDecision> EvaluateAsync(HistoricalDataset dataset, DivergenceReport divergence, CancellationToken ct);
}

public record PredictionResult(
    Guid PredictionId,
    DateTimeOffset CalculatedAt,
    double RiskScore,
    string RiskLevel,
    double PessimisticTtfHours,
    double SafeWindowHours,
    XaiDna XaiDna,
    string Recommendation,
    bool RequiresImmediateAction,
    double AccuracyScore,
    double ReliabilityScore
);

public interface IPredictionEngine
{
    ValueTask<PredictionResult> PredictAsync(ShipmentIdentity identity, CarrierProfile carrier, HistoricalDataset dataset, ModelVersion? model, CancellationToken ct);
}

public record Recommendation(string Code, string Narrative, string Priority);

public interface IRecommender
{
    IReadOnlyList<Recommendation> Generate(PredictionResult prediction, ShipmentIdentity identity, CarrierProfile carrier, HistoricalDataset dataset);
}
