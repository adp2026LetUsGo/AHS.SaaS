// src/Foundation/AHS.Common/Engines/MeanKineticTemperature.cs
namespace AHS.Common.Engines;

public static class MeanKineticTemperature
{
    private const double DefaultEa     = 83_144.0;
    private const double GasConstant   = 8.314;

    // Zero-allocation for ≤256 readings — stackalloc on stack
    public static double Calculate(ReadOnlySpan<double> readings,
        double activationEnergy = DefaultEa)
    {
        if (readings.IsEmpty)
            throw new ArgumentException("No readings provided.", nameof(readings));

        Span<double> exps = readings.Length <= 256
            ? stackalloc double[readings.Length]
            : new double[readings.Length];   // heap fallback only for large datasets

        for (int i = 0; i < readings.Length; i++)
            exps[i] = Math.Exp(-activationEnergy / (GasConstant * (readings[i] + 273.15)));

        double sumExp = 0;
        foreach (var e in exps) sumExp += e;   // ❌ NO: exps.Sum() — LINQ = heap allocation

        double mktK = -activationEnergy / (GasConstant * Math.Log(sumExp / readings.Length));
        return mktK - 273.15;
    }

    public static bool IsWithinLimit(ReadOnlySpan<double> readings, double maxCelsius,
        double activationEnergy = DefaultEa)
        => Calculate(readings, activationEnergy) <= maxCelsius;
}
