namespace AHS.Common;

public sealed class PredictRiskRequest
{
    public string RouteId { get; set; } = string.Empty;
    public string Carrier { get; set; } = string.Empty;
    public string Packaging { get; set; } = string.Empty;
    public string Weather { get; set; } = string.Empty;

    public PredictRiskRequest() { }

    public PredictRiskRequest(string routeId, string carrier, string packaging, string weather)
    {
        RouteId = routeId;
        Carrier = carrier;
        Packaging = packaging;
        Weather = weather;
    }
}

public sealed record PredictionResponse(bool IsExcursion, float Probability, string Risk);

public sealed record PredictRiskResponse(double Score, string Status, bool IsHighRisk);
