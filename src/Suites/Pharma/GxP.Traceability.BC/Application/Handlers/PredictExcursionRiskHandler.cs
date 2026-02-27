using AHS.Common;
using AHS.Engines.Inference.Prediction.Engine;
using AHS.Suites.Pharma.GxP.Traceability.BC.Application.Commands;
using AHS.Platform.Compliance;

namespace AHS.Suites.Pharma.GxP.Traceability.BC.Application.Handlers;

public class PredictExcursionRiskHandler(AuditTrailService auditService)
{
    private readonly TemperaturePredictionEngine _engine = new();
    private readonly AuditTrailService _auditService = auditService;

    public async Task<Result<PredictionResponseDTO>> Handle(PredictExcursionRiskCommand request)
    {
        try
        {
            var input = new PredictionInput(request.RouteId, request.Carrier, request.TransitTimeHrs, request.ExternalTempAvg, request.PackagingType, request.DelayFlag);
            var score = _engine.Predict(input);
            
            // Logical check for risk
            var isHighRisk = score > 0.7; // Example threshold

            var signature = Sha256Hasher.Hash($"{request.RouteId}|{score}|{DateTime.UtcNow:yyyyMMdd}");
            
            // GxP Compliance: Audit must succeed for the operation to be valid.
            await _auditService.SaveAsync(new AuditRecord(request.RouteId, "TENANT_001", score, signature, DateTime.UtcNow));

            return Result.Success(new PredictionResponseDTO(score, isHighRisk));
        }
        catch (Exception ex)
        {
            // Atomicity: If auditing or processing fails, return a failure.
            return Result.Failure<PredictionResponseDTO>($"Risk prediction or audit logging failed: {ex.Message}");
        }
    }
}
