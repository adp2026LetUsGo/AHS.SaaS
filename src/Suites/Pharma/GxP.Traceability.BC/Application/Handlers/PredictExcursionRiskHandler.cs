using AHS.Common;
using AHS.SharedKernel;
using AHS.Engines.Inference.Prediction.Engine;
using AHS.Platform.Compliance;
using AHS.Suites.Pharma.GxP.Traceability.BC.Application.Commands;

namespace AHS.Suites.Pharma.GxP.Traceability.BC.Application.Handlers;

public class PredictExcursionRiskHandler : IRequestHandler<PredictExcursionRiskCommand, Result<PredictionResponseDTO>>
{
    private readonly TemperaturePredictionEngine _engine = new();

    public Task<Result<PredictionResponseDTO>> Handle(PredictExcursionRiskCommand request)
    {
        var input = new PredictionInput(
            request.RouteId,
            request.Carrier,
            request.TransitTimeHrs,
            request.ExternalTempAvg,
            request.PackagingType,
            request.DelayFlag);

        var score = _engine.Predict(input);
        var isHighRisk = ExcursionPredictionPolicy.IsHighRisk(score);

        var result = Result.Success(new PredictionResponseDTO(score, isHighRisk));

        // GxP Compliance: Hash the prediction result for audit trail
        var hash = ComplianceService.HashSha256($"Score={score}|HighRisk={isHighRisk}");
        Console.WriteLine($"[AUDIT] Hash: {hash} | Score: {score} | HighRisk: {isHighRisk}");

        return Task.FromResult(result);
    }
}
