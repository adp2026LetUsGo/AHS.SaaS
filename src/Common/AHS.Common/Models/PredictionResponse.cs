namespace AHS.Common.Models;
public record PredictionResponse(string Id, float Confidence, string Label, long LatencyMs, DateTime Timestamp);
