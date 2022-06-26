using System;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlAnvil.TestExtensions;
using Xunit;

namespace LfrlAnvil.Chrono.Tests.DateTimeProviderTests;

public class DateTimeProviderTests : TestsBase
{
    [Fact]
    public void Utc_ShouldReturnCorrectResult()
    {
        var sut = DateTimeProvider.Utc;
        sut.Kind.Should().Be( DateTimeKind.Utc );
    }

    [Fact]
    public void Local_ShouldReturnCorrectResult()
    {
        var sut = DateTimeProvider.Local;
        sut.Kind.Should().Be( DateTimeKind.Local );
    }

    [Fact]
    public void GetNow_ForUtc_ShouldReturnCorrectResult()
    {
        var sut = new UtcDateTimeProvider();

        var expectedMin = DateTime.UtcNow;
        var result = sut.GetNow();
        var expectedMax = DateTime.UtcNow;

        using ( new AssertionScope() )
        {
            result.Kind.Should().Be( DateTimeKind.Utc );
            result.Should().BeOnOrAfter( expectedMin ).And.BeOnOrBefore( expectedMax );
        }
    }

    [Fact]
    public void GetNow_ForLocal_ShouldReturnCorrectResult()
    {
        var sut = new LocalDateTimeProvider();

        var expectedMin = DateTime.Now;
        var result = sut.GetNow();
        var expectedMax = DateTime.Now;

        using ( new AssertionScope() )
        {
            result.Kind.Should().Be( DateTimeKind.Local );
            result.Should().BeOnOrAfter( expectedMin ).And.BeOnOrBefore( expectedMax );
        }
    }
}