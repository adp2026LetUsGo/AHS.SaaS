// tests/Foundation/AHS.Common.Tests/Engines/MeanKineticTemperatureTests.cs
using AHS.Common.Engines;
using FluentAssertions;
using Xunit;

namespace AHS.Common.Tests.Engines;

public class MeanKineticTemperatureTests
{
    [Fact]
    public void MKT_of_constant_readings_equals_that_temperature()
    {
        var readings = Enumerable.Repeat(5.0, 100).ToArray();
        var mkt = MeanKineticTemperature.Calculate(readings.AsSpan());
        mkt.Should().BeApproximately(5.0, precision: 0.001);
    }

    [Fact]
    public void MKT_within_range_for_pharma_oscillating_readings()
    {
        var readings = Enumerable.Range(0, 200)
            .Select(i => 2.0 + Math.Sin(i * 0.1) * 3.0)
            .ToArray();
        var mkt = MeanKineticTemperature.Calculate(readings.AsSpan());
        mkt.Should().BeInRange(2.0, 8.0);
    }

    [Fact]
    public void Empty_span_throws_ArgumentException()
    {
        var act = () => MeanKineticTemperature.Calculate(ReadOnlySpan<double>.Empty);
        act.Should().Throw<ArgumentException>();
    }
}
