namespace AHS.Engines.Inference.Prediction.Engine;

public record PredictionInput(
    string RouteId,
    string Carrier,
    int TransitTimeHrs,
    double ExternalTempAvg,
    string PackagingType,
    bool DelayFlag);

public class TemperaturePredictionEngine
{
    public double Predict(PredictionInput input)
    {
        double baseScore = 0.05;
        if (input.ExternalTempAvg > 25) baseScore += 0.1;
        if (input.TransitTimeHrs > 48) baseScore += 0.15;
        if (input.DelayFlag) baseScore += 0.2;
        return Math.Min(baseScore, 1.0);
    }
}
