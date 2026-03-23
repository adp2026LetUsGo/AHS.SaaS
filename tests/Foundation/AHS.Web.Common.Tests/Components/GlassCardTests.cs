// tests/Foundation/AHS.Web.Common.Tests/Components/GlassCardTests.cs
using AHS.Web.Common.Components;
using Bunit;
using FluentAssertions;
using Xunit;

namespace AHS.Web.Common.Tests.Components;

public class GlassCardTests : BunitContext
{
    [Fact]
    public void GlassCard_renders_child_content()
    {
        var cut = Render<GlassCard>(parameters => parameters
            .AddChildContent("<span>Test Content</span>"));

        cut.Find("span").MarkupMatches("<span>Test Content</span>");
    }

    [Fact]
    public void GlassCard_applies_custom_class()
    {
        var cut = Render<GlassCard>(parameters => parameters
            .Add(p => p.Class, "custom-class"));

        cut.Find(".glass-card").ClassList.Should().Contain("custom-class");
    }
}
