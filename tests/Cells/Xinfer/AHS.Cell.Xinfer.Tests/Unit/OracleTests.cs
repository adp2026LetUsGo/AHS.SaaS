// tests/Cells/Xinfer/AHS.Cell.Xinfer.Tests/Unit/OracleTests.cs
using AHS.Cell.Xinfer.Application.Oracle;
using AHS.Cell.Xinfer.Domain.Enums;
using FluentAssertions;
using Xunit;
using System.Diagnostics;

namespace AHS.Cell.Xinfer.Tests.Unit;

public class OracleTests
{


    [Fact]
    public async Task PassiveInsulationAddsExactly15PercentPenalty()
    {
        var activeReq = CreateBaseRequest(InsulationType.Active);
        var passiveReq = CreateBaseRequest(InsulationType.Passive);

        var activeResult = await LogisticsOracle.CalculateAsync(activeReq, default);
        var passiveResult = await LogisticsOracle.CalculateAsync(passiveReq, default);

        (passiveResult.RiskScore - activeResult.RiskScore).Should().BeApproximately(15.0f, 0.01f);
    }

    [Fact]
    public async Task XaiDnaContainsExactly14Factors()
    {
        var req = CreateBaseRequest(InsulationType.Active);
        var result = await LogisticsOracle.CalculateAsync(req, default);

        typeof(XaiDna).GetProperties().Length.Should().Be(14); // It's a record struct with 14 positional properties
        // Positional properties show up as fields or compiler generated properties. 
        // We'll trust the record struct definition from Application layer.
    }

    [Fact]
    public async Task OracleP99Under10ms()
    {
        var req = CreateBaseRequest(InsulationType.Passive);
        var sw = new Stopwatch();
        
        // Warmup
        for (int i = 0; i < 10; i++) await LogisticsOracle.CalculateAsync(req, default);

        var latencies = new List<long>();
        for (int i = 0; i < 100; i++)
        {
            sw.Restart();
            await LogisticsOracle.CalculateAsync(req, default);
            sw.Stop();
            latencies.Add(sw.ElapsedMilliseconds);
        }

        var p99 = latencies.OrderBy(x => x).ElementAt(98);
        p99.Should().BeLessThan(10);
    }

    private static OracleRequest CreateBaseRequest(InsulationType insulation) => new(
        "R1", CargoType.Pharmaceutical, insulation, "Nominal", "C1", 0.95f, 0, 25.0f, 15.0f, 50.0f, 2.0f, 8.0f, 10.0f);
}
