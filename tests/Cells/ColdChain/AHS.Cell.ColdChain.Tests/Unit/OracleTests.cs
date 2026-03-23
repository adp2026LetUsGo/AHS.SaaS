// tests/Cells/ColdChain/AHS.Cell.ColdChain.Tests/Unit/OracleTests.cs
using AHS.Cell.ColdChain.Application.Oracle;
using AHS.Cell.ColdChain.Domain.Enums;
using FluentAssertions;
using Xunit;
using System.Diagnostics;

namespace AHS.Cell.ColdChain.Tests.Unit;

public class OracleTests
{
    private readonly LogisticsOracle _oracle = new();

    [Fact]
    public async Task Passive_insulation_adds_exactly_15_percent_penalty()
    {
        var activeReq = CreateBaseRequest(InsulationType.Active);
        var passiveReq = CreateBaseRequest(InsulationType.Passive);

        var activeResult = await _oracle.CalculateAsync(activeReq, default);
        var passiveResult = await _oracle.CalculateAsync(passiveReq, default);

        (passiveResult.RiskScore - activeResult.RiskScore).Should().BeApproximately(15.0f, 0.01f);
    }

    [Fact]
    public async Task XAI_DNA_contains_exactly_14_factors()
    {
        var req = CreateBaseRequest(InsulationType.Active);
        var result = await _oracle.CalculateAsync(req, default);

        typeof(XaiDna).GetProperties().Length.Should().Be(14); // It's a record struct with 14 positional properties
        // Positional properties show up as fields or compiler generated properties. 
        // We'll trust the record struct definition from Application layer.
    }

    [Fact]
    public async Task Oracle_P99_under_10ms()
    {
        var req = CreateBaseRequest(InsulationType.Passive);
        var sw = new Stopwatch();
        
        // Warmup
        for (int i = 0; i < 10; i++) await _oracle.CalculateAsync(req, default);

        var latencies = new List<long>();
        for (int i = 0; i < 100; i++)
        {
            sw.Restart();
            await _oracle.CalculateAsync(req, default);
            sw.Stop();
            latencies.Add(sw.ElapsedMilliseconds);
        }

        var p99 = latencies.OrderBy(x => x).ElementAt(98);
        p99.Should().BeLessThan(10);
    }

    private OracleRequest CreateBaseRequest(InsulationType insulation) => new(
        "R1", CargoType.Pharmaceutical, insulation, "Nominal", "C1", 0.95f, 0, 25.0f, 15.0f, 50.0f, 2.0f, 8.0f, 10.0f);
}
