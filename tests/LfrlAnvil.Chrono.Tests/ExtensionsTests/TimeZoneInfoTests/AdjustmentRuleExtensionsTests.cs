using System;
using FluentAssertions;
using LfrlAnvil.Chrono.Extensions;
using LfrlAnvil.TestExtensions;
using Xunit;

namespace LfrlAnvil.Chrono.Tests.ExtensionsTests.TimeZoneInfoTests;

public class AdjustmentRuleExtensionsTests : TestsBase
{
    [Fact]
    public void GetTransitionTimeWithInvalidity_ShouldReturnStartTime_WhenDaylightDeltaIsPositive()
    {
        var deltaInHours = Fixture.CreatePositiveInt32() % 12 + 1;
        var sut = CreateRule( deltaInHours );

        var result = sut.GetTransitionTimeWithInvalidity();

        result.Should().Be( sut.DaylightTransitionStart );
    }

    [Fact]
    public void GetTransitionTimeWithInvalidity_ShouldReturnEndTime_WhenDaylightDeltaIsNegative()
    {
        var deltaInHours = Fixture.CreateNegativeInt32() % 12 - 1;
        var sut = CreateRule( deltaInHours );

        var result = sut.GetTransitionTimeWithInvalidity();

        result.Should().Be( sut.DaylightTransitionEnd );
    }

    [Fact]
    public void GetTransitionTimeWithAmbiguity_ShouldReturnEndTime_WhenDaylightDeltaIsPositive()
    {
        var deltaInHours = Fixture.CreatePositiveInt32() % 12 + 1;
        var sut = CreateRule( deltaInHours );

        var result = sut.GetTransitionTimeWithAmbiguity();

        result.Should().Be( sut.DaylightTransitionEnd );
    }

    [Fact]
    public void GetTransitionTimeWithAmbiguity_ShouldReturnStartTime_WhenDaylightDeltaIsNegative()
    {
        var deltaInHours = Fixture.CreateNegativeInt32() % 12 - 1;
        var sut = CreateRule( deltaInHours );

        var result = sut.GetTransitionTimeWithAmbiguity();

        result.Should().Be( sut.DaylightTransitionStart );
    }

    private static TimeZoneInfo.AdjustmentRule CreateRule(int deltaInHours)
    {
        return TimeZoneFactory.CreateInfiniteRule(
            transitionStart: new DateTime( 1, 8, 26, 2, 0, 0 ),
            transitionEnd: new DateTime( 1, 10, 26, 2, 0, 0 ),
            daylightDeltaInHours: deltaInHours );
    }
}