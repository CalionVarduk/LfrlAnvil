using System;
using FluentAssertions;
using LfrlAnvil.Chrono.Extensions;
using LfrlAnvil.TestExtensions;
using LfrlAnvil.TestExtensions.Attributes;
using Xunit;

namespace LfrlAnvil.Chrono.Tests.ExtensionsTests.BoundsTests
{
    [TestClass( typeof( BoundsExtensionsTestsData ) )]
    public class BoundsExtensionsTests : TestsBase
    {
        [Theory]
        [MethodData( nameof( BoundsExtensionsTestsData.GetGetTimeSpanData ) )]
        public void GetTimeSpan_ShouldReturnStartSubtractedFromEndWithOneMoreTick(DateTime min, DateTime max, TimeSpan expected)
        {
            var sut = Bounds.Create( min, max );
            var result = sut.GetTimeSpan();
            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( BoundsExtensionsTestsData.GetGetPeriodData ) )]
        public void GetPeriod_WithDateTime_ShouldReturnCorrectResult(DateTime min, DateTime max, PeriodUnits units, Period expected)
        {
            var sut = Bounds.Create( min, max );
            var result = sut.GetPeriod( units );
            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( BoundsExtensionsTestsData.GetGetGreedyPeriodData ) )]
        public void GetGreedyPeriod_WithDateTime_ShouldReturnCorrectResult(DateTime min, DateTime max, PeriodUnits units, Period expected)
        {
            var sut = Bounds.Create( min, max );
            var result = sut.GetGreedyPeriod( units );
            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( BoundsExtensionsTestsData.GetGetDurationData ) )]
        public void GetDuration_ShouldReturnStartSubtractedFromEndWithOneMoreTick(DateTime min, DateTime max, Duration expected)
        {
            var timeZone = TimeZoneFactory.CreateRandom( Fixture );
            var sut = Bounds.Create( ZonedDateTime.Create( min, timeZone ), ZonedDateTime.Create( max, timeZone ) );

            var result = sut.GetDuration();

            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( BoundsExtensionsTestsData.GetGetPeriodData ) )]
        public void GetPeriod_WithZonedDateTime_ShouldReturnCorrectResult(DateTime min, DateTime max, PeriodUnits units, Period expected)
        {
            var timeZone = TimeZoneFactory.CreateRandom( Fixture );
            var sut = Bounds.Create( ZonedDateTime.Create( min, timeZone ), ZonedDateTime.Create( max, timeZone ) );

            var result = sut.GetPeriod( units );

            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( BoundsExtensionsTestsData.GetGetGreedyPeriodData ) )]
        public void GetGreedyPeriod_WithZonedDateTime_ShouldReturnCorrectResult(
            DateTime min,
            DateTime max,
            PeriodUnits units,
            Period expected)
        {
            var timeZone = TimeZoneFactory.CreateRandom( Fixture );
            var sut = Bounds.Create( ZonedDateTime.Create( min, timeZone ), ZonedDateTime.Create( max, timeZone ) );

            var result = sut.GetGreedyPeriod( units );

            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( BoundsExtensionsTestsData.GetGetPeriodWithZonedDayData ) )]
        public void GetPeriod_WithZonedDay_ShouldReturnCorrectResult(DateTime min, DateTime max, PeriodUnits units, Period expected)
        {
            var timeZone = TimeZoneFactory.CreateRandom( Fixture );
            var sut = Bounds.Create( ZonedDay.Create( min, timeZone ), ZonedDay.Create( max, timeZone ) );

            var result = sut.GetPeriod( units );

            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( BoundsExtensionsTestsData.GetGetGreedyPeriodWithZonedDayData ) )]
        public void GetGreedyPeriod_WithZonedDay_ShouldReturnCorrectResult(
            DateTime min,
            DateTime max,
            PeriodUnits units,
            Period expected)
        {
            var timeZone = TimeZoneFactory.CreateRandom( Fixture );
            var sut = Bounds.Create( ZonedDay.Create( min, timeZone ), ZonedDay.Create( max, timeZone ) );

            var result = sut.GetGreedyPeriod( units );

            result.Should().Be( expected );
        }
    }
}
