using System;
using FluentAssertions;
using LfrlSoft.NET.Core.Chrono.Extensions;
using LfrlSoft.NET.TestExtensions.Attributes;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.ChronoTests.ExtensionsTests.TimeZoneInfoTests
{
    [TestClass( typeof( TransitionTimeExtensionsTestsData ) )]
    public class TransitionTimeExtensionsTests
    {
        [Theory]
        [MethodData( nameof( TransitionTimeExtensionsTestsData.GetToDateTimeWithFixedTimeData ) )]
        public void ToDateTime_WithFixedTime_ShouldReturnCorrectResult(DateTime time, int year, DateTime expected)
        {
            var sut = TimeZoneInfo.TransitionTime.CreateFixedDateRule(
                timeOfDay: DateTime.MinValue + time.TimeOfDay,
                month: time.Month,
                day: time.Day );

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
            var sut = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(
                timeOfDay: DateTime.MinValue + timeOfDay,
                month: month,
                week: week,
                dayOfWeek: dayOfWeek );

            var result = sut.ToDateTime( year );

            result.Should().Be( expected );
        }
    }
}
