namespace AHS.Common;

public sealed class PredictRiskRequest
{
    public string RouteId { get; set; } = string.Empty;
    public string Carrier { get; set; } = string.Empty;
    public int TransitTime { get; set; }
    public double AvgTemp { get; set; }
    public string Packaging { get; set; } = string.Empty;
    public bool Delay { get; set; }

    public PredictRiskRequest() { }

    public PredictRiskRequest(string routeId, string carrier, int transitTime, double avgTemp, string packaging, bool delay)
    {
        RouteId = routeId;
        Carrier = carrier;
        TransitTime = transitTime;
        AvgTemp = avgTemp;
        Packaging = packaging;
        Delay = delay;
    }
}

public sealed record PredictRiskResponse(double Score, string Status, bool IsHighRisk);
public sealed record PredictionResponse(double Score, bool IsHighRisk);
