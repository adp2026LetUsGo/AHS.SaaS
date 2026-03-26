// tests/Foundation/AHS.Common.Tests/Domain/SignedCommandTests.cs
using AHS.Common.Application;
using FluentAssertions;
using Xunit;

namespace AHS.Common.Tests.Domain;

public class SignedCommandTests
{
    [Fact]
    public void EmptyReasonThrowsElectronicSignatureRequiredException()
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
    public void ValidReasonConstructsSuccessfully()
    {
        var cmd = new TestCommand
        {
            SignedById      = Guid.NewGuid(),
            SignedByName    = "Dr. Test",
            ReasonForChange = "Standard operation per SOP-001",
        };
        cmd.ReasonForChange.Should().NotBeEmpty();
    }

    private sealed record TestCommand : SignedCommand;
}
