namespace AHS.Cell.Xinfer.Domain.ValueObjects;

public readonly record struct CarrierProfile(
    string CarrierId,
    double ReliabilityScore, // 0.0-1.0
    int Incidents12M
)
{
    // Risk modifier: higher incidents = higher risk contribution
    // Blueprint Section 7, Responsibility 1
    public double RiskModifier =>
        1.0 + (Incidents12M * 0.05) + ((1 - ReliabilityScore) * 0.15);
}
