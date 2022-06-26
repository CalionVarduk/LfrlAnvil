using System;
using FluentAssertions;
using LfrlAnvil.Chrono.Extensions;
using LfrlAnvil.TestExtensions.Attributes;
using Xunit;

namespace LfrlAnvil.Chrono.Tests.ExtensionsTests.TimeZoneInfoTests;

[TestClass( typeof( TransitionTimeExtensionsTestsData ) )]
public class TransitionTimeExtensionsTests
{
    [Theory]
    [MethodData( nameof( TransitionTimeExtensionsTestsData.GetToDateTimeWithFixedTimeData ) )]
    public void ToDateTime_WithFixedTime_ShouldReturnCorrectResult(DateTime time, int year, DateTime expected)
    {
        var sut = TimeZoneFactory.CreateFixedTime( time );
        var result = sut.ToDateTime( year );
        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( TransitionTimeExtensionsTestsData.GetToDateTimeWithFloatingTimeData ) )]
    public void ToDateTime_WithFloatingTime_ShouldReturnCorrectResult(
        TimeSpan timeOfDay,
        int month,
        int week,
        DayOfWeek dayOfWeek,
        int year,
        DateTime expected)
    {
        var sut = TimeZoneFactory.CreateFloatingTime(
            monthAndTime: new DateTime( 1, month, 1 ) + timeOfDay,
            week: week,
            day: dayOfWeek );

        var result = sut.ToDateTime( year );

        result.Should().Be( expected );
    }
}