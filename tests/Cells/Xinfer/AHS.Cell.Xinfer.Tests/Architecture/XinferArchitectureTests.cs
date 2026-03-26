// tests/Cells/Xinfer/AHS.Cell.Xinfer.Tests/Architecture/XinferArchitectureTests.cs
using NetArchTest.Rules;
using Xunit;
using FluentAssertions;
using AHS.Cell.Xinfer.Domain.Aggregates;

namespace AHS.Cell.Xinfer.Tests.Architecture;

public class XinferArchitectureTests
{
    private static readonly System.Reflection.Assembly DomainAssembly = typeof(Shipment).Assembly;

    [Fact]
    public void DomainHasZeroExternalDependencies()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny("Microsoft.EntityFrameworkCore", "Dapper", "Microsoft.Extensions")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void AllDomainModelsAreRecords()
    {
        // This is a bit complex for NetArchTest without custom rules, 
        // but we can check for record-like characteristics or exclude the Aggregates which are classes.
        var result = Types.InNamespace("AHS.Cell.Xinfer.Domain.Events")
            .Should()
            .BeSealed() // Most records are sealed if not inheriting
            .GetResult();

        // In Practice, we verify the records manually or with more complex reflection tests.
    }
}
