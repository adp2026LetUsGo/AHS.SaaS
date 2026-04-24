using AHS.Cell.Xinfer.Domain.Enums;

namespace AHS.Cell.Xinfer.Domain.ValueObjects;

public readonly record struct InfluenceFactor(
    string Name,
    double Value,
    FactorSentiment Sentiment
);
