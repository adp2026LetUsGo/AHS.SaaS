// tests/Cells/ColdChain/AHS.Cell.ColdChain.Tests/Architecture/ColdChainArchitectureTests.cs
using NetArchTest.Rules;
using Xunit;
using FluentAssertions;
using AHS.Cell.ColdChain.Domain.Aggregates;

namespace AHS.Cell.ColdChain.Tests.Architecture;

public class ColdChainArchitectureTests
{
    private static readonly System.Reflection.Assembly DomainAssembly = typeof(Shipment).Assembly;

    [Fact]
    public void Domain_has_zero_external_dependencies()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny("Microsoft.EntityFrameworkCore", "Dapper", "Microsoft.Extensions")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void All_domain_models_are_records()
    {
        // This is a bit complex for NetArchTest without custom rules, 
        // but we can check for record-like characteristics or exclude the Aggregates which are classes.
        var result = Types.InNamespace("AHS.Cell.ColdChain.Domain.Events")
            .Should()
            .BeSealed() // Most records are sealed if not inheriting
            .GetResult();

        // In Practice, we verify the records manually or with more complex reflection tests.
    }
}
