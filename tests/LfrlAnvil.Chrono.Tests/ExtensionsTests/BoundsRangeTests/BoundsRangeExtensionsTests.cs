using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using LfrlAnvil.Chrono.Extensions;
using LfrlAnvil.TestExtensions;
using LfrlAnvil.TestExtensions.Attributes;
using LfrlAnvil.TestExtensions.FluentAssertions;
using Xunit;

namespace LfrlAnvil.Chrono.Tests.ExtensionsTests.BoundsRangeTests
{
    [TestClass( typeof( BoundsRangeExtensionsTestsData ) )]
    public class BoundsRangeExtensionsTests : TestsBase
    {
        [Fact]
        public void Normalize_WithDateTime_ShouldMergeBoundsThatAreOneTickApart()
        {
            var dt1 = new DateTime( 2021, 8, 26 );
            var dt2 = new DateTime( 2021, 8, 26 ).AddTicks( 1 );
            var dt3 = new DateTime( 2021, 8, 26, 1, 0, 0 );
            var dt4 = new DateTime( 2021, 8, 26, 2, 0, 0 );
            var dt5 = new DateTime( 2021, 8, 26, 3, 0, 0 );
            var dt6 = new DateTime( 2021, 8, 26, 4, 0, 0 ).AddTicks( -1 );
            var dt7 = new DateTime( 2021, 8, 26, 4, 0, 0 );
            var dt8 = new DateTime( 2021, 8, 26, 5, 0, 0 ).AddTicks( -1 );
            var dt9 = new DateTime( 2021, 8, 26, 5, 0, 0 );
            var dt10 = new DateTime( 2021, 8, 26, 6, 0, 0 );
            var dt11 = new DateTime( 2021, 8, 26, 6, 0, 0 ).AddTicks( 2 );
            var dt12 = new DateTime( 2021, 8, 26, 7, 0, 0 );

            var sut = BoundsRange.Create(
                new[]
                {
                    Bounds.Create( dt1, dt1 ),
                    Bounds.Create( dt2, dt2 ),
                    Bounds.Create( dt3, dt4 ),
                    Bounds.Create( dt5, dt6 ),
                    Bounds.Create( dt7, dt8 ),
                    Bounds.Create( dt9, dt10 ),
                    Bounds.Create( dt11, dt12 )
                } );

            var result = sut.Normalize();

            result.Should()
                .BeSequentiallyEqualTo(
                    Bounds.Create( dt1, dt2 ),
                    Bounds.Create( dt3, dt4 ),
                    Bounds.Create( dt5, dt10 ),
                    Bounds.Create( dt11, dt12 ) );
        }

        [Theory]
        [MethodData( nameof( BoundsRangeExtensionsTestsData.GetGetTimeSpanData ) )]
        public void GetTimeSpan_ShouldReturnStartSubtractedFromEndWithOneMoreTick(
            IEnumerable<(DateTime Min, DateTime Max)> range,
            TimeSpan expected)
        {
            var sut = BoundsRange.Create( range.Select( r => Bounds.Create( r.Min, r.Max ) ) );
            var result = sut.GetTimeSpan();
            result.Should().Be( expected );
        }

        [Fact]
        public void Normalize_WithZonedDateTime_ShouldMergeBoundsThatAreOneTickApart()
        {
            var timeZone = TimeZoneFactory.CreateRandom( Fixture );

            var dt1 = ZonedDateTime.Create( new DateTime( 2021, 8, 26 ), timeZone );
            var dt2 = ZonedDateTime.Create( new DateTime( 2021, 8, 26 ).AddTicks( 1 ), timeZone );
            var dt3 = ZonedDateTime.Create( new DateTime( 2021, 8, 26, 1, 0, 0 ), timeZone );
            var dt4 = ZonedDateTime.Create( new DateTime( 2021, 8, 26, 2, 0, 0 ), timeZone );
            var dt5 = ZonedDateTime.Create( new DateTime( 2021, 8, 26, 3, 0, 0 ), timeZone );
            var dt6 = ZonedDateTime.Create( new DateTime( 2021, 8, 26, 4, 0, 0 ).AddTicks( -1 ), timeZone );
            var dt7 = ZonedDateTime.Create( new DateTime( 2021, 8, 26, 4, 0, 0 ), timeZone );
            var dt8 = ZonedDateTime.Create( new DateTime( 2021, 8, 26, 5, 0, 0 ).AddTicks( -1 ), timeZone );
            var dt9 = ZonedDateTime.Create( new DateTime( 2021, 8, 26, 5, 0, 0 ), timeZone );
            var dt10 = ZonedDateTime.Create( new DateTime( 2021, 8, 26, 6, 0, 0 ), timeZone );
            var dt11 = ZonedDateTime.Create( new DateTime( 2021, 8, 26, 6, 0, 0 ).AddTicks( 2 ), timeZone );
            var dt12 = ZonedDateTime.Create( new DateTime( 2021, 8, 26, 7, 0, 0 ), timeZone );

            var sut = BoundsRange.Create(
                new[]
                {
                    Bounds.Create( dt1, dt1 ),
                    Bounds.Create( dt2, dt2 ),
                    Bounds.Create( dt3, dt4 ),
                    Bounds.Create( dt5, dt6 ),
                    Bounds.Create( dt7, dt8 ),
                    Bounds.Create( dt9, dt10 ),
                    Bounds.Create( dt11, dt12 )
                } );

            var result = sut.Normalize();

            result.Should()
                .BeSequentiallyEqualTo(
                    Bounds.Create( dt1, dt2 ),
                    Bounds.Create( dt3, dt4 ),
                    Bounds.Create( dt5, dt10 ),
                    Bounds.Create( dt11, dt12 ) );
        }

        [Theory]
        [MethodData( nameof( BoundsRangeExtensionsTestsData.GetGetDurationData ) )]
        public void GetDuration_ShouldReturnStartSubtractedFromEndWithOneMoreTick(
            IEnumerable<(DateTime Min, DateTime Max)> range,
            Duration expected)
        {
            var timeZone = TimeZoneFactory.CreateRandom( Fixture );
            var sut = BoundsRange.Create(
                range.Select( r => Bounds.Create( ZonedDateTime.Create( r.Min, timeZone ), ZonedDateTime.Create( r.Max, timeZone ) ) ) );

            var result = sut.GetDuration();

            result.Should().Be( expected );
        }

        [Fact]
        public void Normalize_WithZonedDay_ShouldMergeBoundsThatAreOneDayApart()
        {
            var timeZone = TimeZoneFactory.CreateRandom( Fixture );

            var dt1 = ZonedDay.Create( new DateTime( 2021, 8, 26 ), timeZone );
            var dt2 = ZonedDay.Create( new DateTime( 2021, 8, 27 ), timeZone );
            var dt3 = ZonedDay.Create( new DateTime( 2021, 8, 29 ), timeZone );
            var dt4 = ZonedDay.Create( new DateTime( 2021, 8, 30 ), timeZone );
            var dt5 = ZonedDay.Create( new DateTime( 2021, 9, 1 ), timeZone );
            var dt6 = ZonedDay.Create( new DateTime( 2021, 9, 3 ), timeZone );
            var dt7 = ZonedDay.Create( new DateTime( 2021, 9, 4 ), timeZone );
            var dt8 = ZonedDay.Create( new DateTime( 2021, 9, 6 ), timeZone );
            var dt9 = ZonedDay.Create( new DateTime( 2021, 9, 7 ), timeZone );
            var dt10 = ZonedDay.Create( new DateTime( 2021, 9, 9 ), timeZone );
            var dt11 = ZonedDay.Create( new DateTime( 2021, 9, 11 ), timeZone );
            var dt12 = ZonedDay.Create( new DateTime( 2021, 9, 13 ), timeZone );

            var sut = BoundsRange.Create(
                new[]
                {
                    Bounds.Create( dt1, dt1 ),
                    Bounds.Create( dt2, dt2 ),
                    Bounds.Create( dt3, dt4 ),
                    Bounds.Create( dt5, dt6 ),
                    Bounds.Create( dt7, dt8 ),
                    Bounds.Create( dt9, dt10 ),
                    Bounds.Create( dt11, dt12 )
                } );

            var result = sut.Normalize();

            result.Should()
                .BeSequentiallyEqualTo(
                    Bounds.Create( dt1, dt2 ),
                    Bounds.Create( dt3, dt4 ),
                    Bounds.Create( dt5, dt10 ),
                    Bounds.Create( dt11, dt12 ) );
        }
    }
}
