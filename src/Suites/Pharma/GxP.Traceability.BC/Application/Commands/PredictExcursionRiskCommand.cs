using AHS.Common;
using AHS.SharedKernel;

namespace AHS.Suites.Pharma.GxP.Traceability.BC.Application.Commands;

public record PredictExcursionRiskCommand(
    string RouteId,
    string Carrier,
    int TransitTimeHrs,
    double ExternalTempAvg,
    string PackagingType,
    bool DelayFlag) : IRequest<Result<PredictionResponseDTO>>;

public record PredictionResponseDTO(double Score, bool IsHighRisk);

public static class ExcursionPredictionPolicy
{
    public static bool IsHighRisk(double score) => score > 0.25;
}
