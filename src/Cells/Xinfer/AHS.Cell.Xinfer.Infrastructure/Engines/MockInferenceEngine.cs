// src/Cells/Xinfer/AHS.Cell.Xinfer.Infrastructure/Engines/MockInferenceEngine.cs
using AHS.Cell.Xinfer.Application.Ports;
using AHS.Cell.Xinfer.Application.Contracts;
using AHS.Cell.Xinfer.Domain.ValueObjects;
using AHS.Cell.Xinfer.Domain.Enums;
using AHS.Common;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace AHS.Cell.Xinfer.Infrastructure.Engines;

public sealed class MockInferenceEngine : IInferenceEngine
{
    public string EngineId => "mock-v1-xai";

    public async Task<Result<PredictionResult>> RunAsync(InferenceInput_v1 input, CancellationToken ct = default)
    {
        // Deterministic simulation based on input
        double riskScore = CalculateRisk(input);
        string riskLevel = riskScore > 0.7 ? "CRITICAL" : riskScore > 0.4 ? "ELEVATED" : "NOMINAL";
        double confidence = 0.85 + (new Random().NextDouble() * 0.1);

        var factors = new List<InfluenceFactor>
        {
            new InfluenceFactor("THERMAL_EXCURSION_RISK", riskScore * 0.6, FactorSentiment.Aggravating),
            new InfluenceFactor("PACKAGING_EFFICIENCY", 0.15, riskScore > 0.5 ? FactorSentiment.Mitigating : FactorSentiment.Neutral),
            new InfluenceFactor("CARRIER_RELIABILITY", 0.1, FactorSentiment.Mitigating)
        };

        // Add medical context factor if route implies pharmaceutical transport
        if (input.RouteId.Contains("MED") || input.RouteId.Contains("PHARMA"))
        {
            factors.Add(new InfluenceFactor("MEDICAL_CONSISTENCY_CHECK", 0.95, FactorSentiment.Mitigating));
        }

        var result = new PredictionResult(
            RiskScore: riskScore,
            RiskLevel: riskLevel,
            ConfidenceScore: confidence,
            InfluenceFactors: factors,
            ModelVersion: "mock-xai-1.0.0",
            TrainedAt: DateTimeOffset.UtcNow.AddDays(-5),
            AccuracyMetric: 0.992
        );

        return await Task.FromResult(Result.Success(result)).ConfigureAwait(false);
    }

    private double CalculateRisk(InferenceInput_v1 input)
    {
        double score = 0.1;
        
        // Temperature penalty
        if (input.ExternalTempAvg > 30) score += 0.4;
        else if (input.ExternalTempAvg > 20) score += 0.2;

        // Transit time penalty
        if (input.TransitTimeHrs > 48) score += 0.3;
        
        // Packaging mitigation
        if (input.PackagingType.Equals("active", StringComparison.OrdinalIgnoreCase)) score -= 0.2;
        
        return Math.Clamp(score, 0.0, 1.0);
    }
}
