using System;
using System.Collections.Generic;

namespace AHS.Common.Models;

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
    Dictionary<string, float> TopFactors);
