using System;
using FluentAssertions;
using LfrlAnvil.TestExtensions;
using Xunit;

namespace LfrlAnvil.Chrono.Tests.TimestampProviderTests;

public class TimestampProviderTests : TestsBase
{
    [Fact]
    public void GetNow_ShouldReturnCorrectResult()
    {
        var sut = new TimestampProvider();

        var expectedMin = DateTime.UtcNow;
        var result = sut.GetNow();
        var expectedMax = DateTime.UtcNow;

        result.UtcValue.Should().BeOnOrAfter( expectedMin ).And.BeOnOrBefore( expectedMax );
    }
}