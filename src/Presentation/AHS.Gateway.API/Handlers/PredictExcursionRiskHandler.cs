using AHS.Engines.ML;
using AHS.Common;

namespace AHS.Gateway.API.Handlers;

/// <summary>
/// Orchestrates the prediction logic by bridging the API request to the ONNX Engine.
/// </summary>
public sealed class PredictExcursionRiskHandler
{
    private readonly ExcursionInferenceService _inferenceService;

    public PredictExcursionRiskHandler(ExcursionInferenceService inferenceService)
    {
        _inferenceService = inferenceService;
    }

    public PredictionResponse Handle(PredictRiskRequest request)
    {
        // 1. Map categorical strings to floats (Pilot mapping)
        float route = (float)(request.RouteId.GetHashCode() % 10);
        float carrier = (float)(request.Carrier.GetHashCode() % 5);
        float packaging = (float)(request.Packaging.GetHashCode() % 3);
        float weather = (float)(request.Weather.GetHashCode() % 4);

        // 2. Call the Real ONNX Engine (No more placeholders)
        var result = _inferenceService.PredictExcursion(route, carrier, packaging, weather);

        // 3. Determine Risk Level based on model probability
        string risk = result.Probability switch
        {
            > 0.7f => "High",
            > 0.3f => "Medium",
            _ => "Low"
        };

        return new PredictionResponse(result.IsExcursion, result.Probability, risk);
    }
}
