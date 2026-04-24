// src/Cells/Xinfer/AHS.Cell.Xinfer.Infrastructure/Services/MockInferenceService.cs
using AHS.Cell.Xinfer.Application.Ports;
using AHS.Cell.Xinfer.Application.Contracts;
using AHS.Cell.Xinfer.Domain.Ports;
using AHS.Cell.Xinfer.Domain.ValueObjects;
using AHS.Cell.Xinfer.Domain.Enums;
using AHS.Common;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Globalization;

namespace AHS.Cell.Xinfer.Infrastructure.Services;

public sealed class MockInferenceService : IInferenceService
{
    private static readonly string[] AllowedPackaging = ["standard", "vip", "active", "passive"];

    private readonly IInferenceEngine _engine;
    private readonly IDataReadinessEngine _readiness;

    public MockInferenceService(IInferenceEngine engine, IDataReadinessEngine readiness)
    {
        _engine = engine;
        _readiness = readiness;
    }

    public async Task<Result<InferenceOutput_v1>> PredictAsync(InferenceInput_v1 input, CancellationToken ct = default)
    {
        var validation = Validate(input);
        if (validation.IsFailure)
            return Result.Failure<InferenceOutput_v1>(validation.Error);

        // SSS-001/003: Data Readiness Check
        var identity = new ShipmentIdentity(
            ProductName: "DemoProduct",
            ProductCategory: "Pharmaceuticals",
            PackagingType: input.PackagingType,
            RouteId: input.RouteId,
            DepartureDate: input.DepartureTimestamp
        );

        var carrier = new CarrierProfile(
            CarrierId: input.Carrier,
            ReliabilityScore: 0.98,
            Incidents12M: 0
        );

        var readiness = _readiness.Evaluate(identity, carrier);
        if (readiness is NotAcceptableState na)
        {
            return Result.Failure<InferenceOutput_v1>($"READINESS_FAIL: {string.Join(", ", na.Issues)}");
        }

        // Delegate to engine (returns Domain PredictionResult)
        var engineResult = await _engine.RunAsync(input, ct).ConfigureAwait(false);
        if (engineResult.IsFailure)
            return Result.Failure<InferenceOutput_v1>(engineResult.Error);

        // ADR-011: Map Domain PredictionResult -> Application DTO
        var domain = engineResult.Value;
        
        var dto = new InferenceOutput_v1(
            RiskScore: (float)domain.RiskScore,
            RiskLevel: domain.RiskLevel,
            ConfidenceScore: (float)domain.ConfidenceScore,
            InfluenceFactors: domain.InfluenceFactors.Select(f => new InfluenceFactorDto(
                Factor: f.Name,
                Weight: (float)f.Value,
                Sentiment: f.Sentiment.ToString().ToUpperInvariant()
            )).ToList(),
            ModelMetadata: new ModelMetadataDto(
                ModelVersion: domain.ModelVersion,
                TrainedAt: domain.TrainedAt,
                AccuracyMetric: (float)domain.AccuracyMetric
            )
        );

        return Result.Success(dto);
    }

    public Result Validate(InferenceInput_v1 input)
    {
        if (input.ExternalTempAvg < -50f || input.ExternalTempAvg > 60f)
            return Result.Failure("External temperature must be between -50.0 and +60.0 Celsius.");

        if (input.TransitTimeHrs < 0.5f || input.TransitTimeHrs > 720f)
            return Result.Failure("Transit time must be between 0.5 and 720.0 hours.");

        if (!AllowedPackaging.Contains(input.PackagingType.ToLowerInvariant()))
            return Result.Failure($"Invalid packaging type. Allowed values: {string.Join(", ", AllowedPackaging)}");

        if (string.IsNullOrWhiteSpace(input.RouteId))
            return Result.Failure("RouteId is required.");

        if (string.IsNullOrWhiteSpace(input.Carrier))
            return Result.Failure("Carrier is required.");

        return Result.Success();
    }
}
