using System;
using FluentAssertions;
using LfrlSoft.NET.Core.Chrono.Extensions;
using LfrlSoft.NET.TestExtensions.Attributes;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Chrono.Extensions.TimeZoneInfo
{
    [TestClass( typeof( TransitionTimeExtensionsTestsData ) )]
    public class TransitionTimeExtensionsTests
    {
        [Theory]
        [MethodData( nameof( TransitionTimeExtensionsTestsData.GetToDateTimeWithFixedTimeData ) )]
        public void ToDateTime_WithFixedTime_ShouldReturnCorrectResult(System.DateTime time, int year, System.DateTime expected)
        {
            var sut = System.TimeZoneInfo.TransitionTime.CreateFixedDateRule(
                timeOfDay: System.DateTime.MinValue + time.TimeOfDay,
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
            System.DayOfWeek dayOfWeek,
            int year,
            System.DateTime expected)
        {
            var sut = System.TimeZoneInfo.TransitionTime.CreateFloatingDateRule(
                timeOfDay: System.DateTime.MinValue + timeOfDay,
                month: month,
                week: week,
                dayOfWeek: dayOfWeek );

            var result = sut.ToDateTime( year );

            result.Should().Be( expected );
        }
    }
}
