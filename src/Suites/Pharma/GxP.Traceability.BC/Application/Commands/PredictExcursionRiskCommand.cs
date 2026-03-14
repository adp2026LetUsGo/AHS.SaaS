using AHS.Common.Models;
using System.Collections.Generic;
using AHS.Common;
using AHS.SharedKernel;
using System;

namespace AHS.Suites.Pharma.GxP.Traceability.BC.Application.Commands;

public record PredictExcursionRiskCommand(
    string RouteId,
    string Carrier,
    int TransitTimeHrs,
    double ExternalTempAvg,
    string PackagingType,
    bool DelayFlag
) : AHS.Common.IRequest<Result<PredictionResponse>>;

public static class ExcursionPredictionPolicy
{
    public static bool IsHighRisk(double score) => score > 0.25;
}
