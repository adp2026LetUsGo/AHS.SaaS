// src/Foundation/AHS.Common/Domain/PredictiveShieldModels.cs
namespace AHS.Common.Domain;

public sealed class PredictRiskRequest { 
    public string RouteId { get; set; } = string.Empty; 
    public string Carrier { get; set; } = string.Empty; 
    public string Packaging { get; set; } = string.Empty; 
    public string Weather { get; set; } = string.Empty; 
    public double TransitTimeHrs { get; set; } 
    public double ExternalTempAvg { get; set; } 
    public bool DelayFlag { get; set; } 
    
    public PredictRiskRequest() { } 
    
    public PredictRiskRequest(string routeId, string carrier, string packaging, string weather) { 
        RouteId = routeId; 
        Carrier = carrier; 
        Packaging = packaging; 
        Weather = weather; 
    } 
    
    public PredictRiskRequest(string routeId, string carrier, int transitTimeHrs, double externalTempAvg, string packaging, bool delayFlag) { 
        RouteId = routeId; 
        Carrier = carrier; 
        TransitTimeHrs = transitTimeHrs; 
        ExternalTempAvg = externalTempAvg; 
        Packaging = packaging; 
        DelayFlag = delayFlag; 
    } 
}

public sealed record PredictRiskResponse(double Score, string Status, bool IsHighRisk);

public record PredictionResponse(
    string Id,
    float Confidence,
    string Label,
    long LatencyMs,
    DateTime Timestamp,
    float Recall,
    float Precision,
    float F1Score,
    string RootCause,
    Dictionary<string, float> TopFactors
);
