namespace AHS.Cell.Xinfer.Domain.ValueObjects;

public readonly record struct RiskScore(double Value)
{
    public static RiskScore operator +(RiskScore a, RiskScore b)
        => new(Math.Min(100, a.Value + b.Value));

    public static RiskScore Zero => new(0);

    public bool IsCritical => Value >= 75;
    public bool IsElevated => Value >= 50 && Value < 75;

    public string RiskLevel => Value >= 75 ? "Critical" 
                             : Value >= 50 ? "Elevated" : "Nominal";
}
