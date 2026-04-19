namespace AHS.Cell.Xinfer.Domain.ValueObjects;

public record XaiDna(IReadOnlyList<XaiFactor> Factors)
{
    public static XaiDna Empty => new([]);
}

public record XaiFactor(
    int FactorId,     // 1-14
    string Name,      // e.g. "EXT_TEMP_STORM_IMPACT"
    double Weight,    // 0.0-1.0
    double Contribution, // weighted contribution to final score
    string Narrative  // human-readable explanation
);
