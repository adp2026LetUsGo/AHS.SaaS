using AHS.Common;
using AHS.Engines.ML;
using AHS.Suites.Pharma.GxP.Traceability.BC.Application.Commands;
using AHS.Platform.Compliance;

namespace AHS.Suites.Pharma.GxP.Traceability.BC.Application.Handlers;

public class PredictExcursionRiskHandler(AuditTrailService auditService, ExcursionInferenceService inferenceService)
{
    private readonly AuditTrailService _auditService = auditService;
    private readonly ExcursionInferenceService _inferenceService = inferenceService;

    public async Task<Result<PredictionResponse>> Handle(PredictExcursionRiskCommand request)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            var inferenceResult = _inferenceService.PredictExcursion(
                (float)(request.RouteId.GetHashCode() % 10), 
                (float)(request.Carrier.GetHashCode() % 5), 
                (float)(request.PackagingType.GetHashCode() % 3),
                request.DelayFlag ? 1.0f : 0.0f);
            
            var isHighRisk = inferenceResult.IsExcursion;
            float score = inferenceResult.Probability;

            string risk = score switch
            {
                > 0.7f => "High",
                > 0.3f => "Medium",
                _ => "Low"
            };

            var signature = Sha256Hasher.Hash($"{request.RouteId}|{score}|{DateTime.UtcNow:yyyyMMdd}");
            
            // GxP Compliance: Audit must succeed for the operation to be valid.
            await _auditService.SaveAsync(new AuditRecord(request.RouteId, "TENANT_001", (double)score, signature, DateTime.UtcNow)).ConfigureAwait(false);

            return Result.Success(new PredictionResponse(isHighRisk, score, risk));
        }
        catch (Exception ex)
        {
            // Atomicity: If auditing or processing fails, return a failure.
            return Result.Failure<PredictionResponse>($"Risk prediction or audit logging failed: {ex.Message}");
        }
    }
}
