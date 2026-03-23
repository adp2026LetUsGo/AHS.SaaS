// tests/Foundation/AHS.Common.Tests/Domain/SignedCommandTests.cs
using AHS.Common.Application;
using FluentAssertions;
using Xunit;

namespace AHS.Common.Tests.Domain;

public class SignedCommandTests
{
    [Fact]
    public void Empty_reason_throws_ElectronicSignatureRequiredException()
    {
        var act = () => new TestCommand
        {
            SignedById      = Guid.NewGuid(),
            SignedByName    = "Test User",
            ReasonForChange = "",
        };

        act.Should().Throw<ElectronicSignatureRequiredException>()
            .WithMessage("*ReasonForChange*");
    }

    [Fact]
    public void Valid_reason_constructs_successfully()
    {
        var cmd = new TestCommand
        {
            SignedById      = Guid.NewGuid(),
            SignedByName    = "Dr. Test",
            ReasonForChange = "Standard operation per SOP-001",
        };
        cmd.ReasonForChange.Should().NotBeEmpty();
    }

    private record TestCommand : SignedCommand;
}
