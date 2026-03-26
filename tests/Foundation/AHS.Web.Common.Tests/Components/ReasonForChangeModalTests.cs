// tests/Foundation/AHS.Web.Common.Tests/Components/ReasonForChangeModalTests.cs
using AHS.Web.Common.Components;
using Bunit;
using FluentAssertions;
using Xunit;
using Microsoft.AspNetCore.Components.Web;

namespace AHS.Web.Common.Tests.Components;

public class ReasonForChangeModalTests : BunitContext
{
    [Fact]
    public void ModalDoesNotRenderContentWhenClosed()
    {
        var cut = Render<ReasonForChangeModal>(parameters => parameters
            .Add(p => p.IsOpen, false));

        cut.FindAll(".modal-backdrop").Should().BeEmpty();
    }

    [Fact]
    public void ModalValidatesEmptyReason()
    {
        var cut = Render<ReasonForChangeModal>(parameters => parameters
            .Add(p => p.IsOpen, true));

        // Use TriggerEvent to be more explicit if Click() fails
        cut.Find(".btn-primary").TriggerEvent("onclick", new MouseEventArgs());

        cut.Find(".reason-modal__error").TextContent.Should().Be("Reason is required.");
    }

    [Fact]
    public void ModalCallsOnConfirmWithReason()
    {
        string? confirmedReason = null;
        var cut = Render<ReasonForChangeModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnConfirm, (string r) => confirmedReason = r));

        // Use Change to trigger @bind
        cut.Find(".glass-textarea").Change("Testing reason");
        cut.Find(".btn-primary").TriggerEvent("onclick", new MouseEventArgs());

        confirmedReason.Should().Be("Testing reason");
    }
}
