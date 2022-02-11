using System;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlSoft.NET.Core.Chrono;
using LfrlSoft.NET.Core.Chrono.Extensions;
using LfrlSoft.NET.Core.Tests.Chrono.ZonedClock;
using LfrlSoft.NET.TestExtensions;
using NSubstitute;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Chrono.Extensions.ZonedClock
{
    public class ZonedClockExtensionsTests : ZonedClockTestsBase
    {
        [Fact]
        public void Create_ShouldBeAnAliasForZonedDateTimeCreate()
        {
            var dateTime = Fixture.Create<System.DateTime>();
            var timeZone = CreateTimeZone();
            var expected = Core.Chrono.ZonedDateTime.Create( dateTime, timeZone );
            var sut = GetMockedClock( timeZone );

            var result = sut.Create( dateTime );

            result.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void TryCreate_ShouldBeAnAliasForZonedDateTimeTryCreate()
        {
            var dateTime = Fixture.Create<System.DateTime>();
            var timeZone = CreateTimeZone();
            var expected = Core.Chrono.ZonedDateTime.TryCreate( dateTime, timeZone );
            var sut = GetMockedClock( timeZone );

            var result = sut.TryCreate( dateTime );

            result.Should().BeEquivalentTo( expected );
        }

        [Theory]
        [InlineData( 100, 99, true )]
        [InlineData( 100, 100, false )]
        [InlineData( 100, 101, false )]
        public void IsInPast_ShouldReturnCorrectResult(long providerTicks, long ticksToTest, bool expected)
        {
            var timeZone = CreateTimeZone();
            var dateTime = CreateDateTime( ticksToTest, timeZone );
            var sut = GetMockedClock( timeZone, providerTicks );

            var result = sut.IsInPast( dateTime );

            result.Should().Be( expected );
        }

        [Theory]
        [InlineData( 100, 99, false )]
        [InlineData( 100, 100, true )]
        [InlineData( 100, 101, false )]
        public void IsNow_ShouldReturnCorrectResult(long providerTicks, long ticksToTest, bool expected)
        {
            var timeZone = CreateTimeZone();
            var dateTime = CreateDateTime( ticksToTest, timeZone );
            var sut = GetMockedClock( timeZone, providerTicks );

            var result = sut.IsNow( dateTime );

            result.Should().Be( expected );
        }

        [Theory]
        [InlineData( 100, 99, false )]
        [InlineData( 100, 100, false )]
        [InlineData( 100, 101, true )]
        public void IsInFuture_ShouldReturnCorrectResult(long providerTicks, long ticksToTest, bool expected)
        {
            var timeZone = CreateTimeZone();
            var dateTime = CreateDateTime( ticksToTest, timeZone );
            var sut = GetMockedClock( timeZone, providerTicks );

            var result = sut.IsInFuture( dateTime );

            result.Should().Be( expected );
        }

        [Theory]
        [InlineData( 100, 90, -10 )]
        [InlineData( 100, 100, 0 )]
        [InlineData( 100, 110, 10 )]
        public void GetDurationOffset_ShouldReturnCorrectResult(long providerTicks, long ticksToTest, long expectedTicks)
        {
            var expected = new Core.Chrono.Duration( expectedTicks );
            var timeZone = CreateTimeZone();
            var dateTime = CreateDateTime( ticksToTest, timeZone );
            var sut = GetMockedClock( timeZone, providerTicks );

            var result = sut.GetDurationOffset( dateTime );

            result.Should().Be( expected );
        }

        [Theory]
        [InlineData( 100, 90, -10 )]
        [InlineData( 100, 100, 0 )]
        [InlineData( 100, 110, 10 )]
        public void GetDurationOffset_WithOtherClock_ShouldReturnCorrectResult(long providerTicks, long ticksToTest, long expectedTicks)
        {
            var expected = new Core.Chrono.Duration( expectedTicks );
            var timeZone = CreateTimeZone();
            var other = GetMockedClock( timeZone, ticksToTest );
            var sut = GetMockedClock( timeZone, providerTicks );

            var result = sut.GetDurationOffset( other );

            result.Should().Be( expected );
        }

        [Fact]
        public void Freeze_ShouldReturnCorrectResult()
        {
            var timeZone = CreateTimeZone();
            var (first, second) = Fixture.CreateDistinctCollection<int>( count: 2 );
            var sut = GetMockedClock( timeZone, first, second );

            var frozen = sut.Freeze();
            var firstResult = frozen.GetNow();
            var secondResult = frozen.GetNow();

            using ( new AssertionScope() )
            {
                firstResult.Timestamp.UnixEpochTicks.Should().Be( first );
                secondResult.Timestamp.UnixEpochTicks.Should().Be( first );
            }
        }

        private static IZonedClock GetMockedClock(System.TimeZoneInfo timeZone)
        {
            var result = Substitute.For<IZonedClock>();
            result.TimeZone.Returns( timeZone );
            return result;
        }

        private static IZonedClock GetMockedClock(System.TimeZoneInfo timeZone, long timestampTicks, params long[] additionalTimestampTicks)
        {
            var result = GetMockedClock( timeZone );

            result
                .GetNow()
                .Returns(
                    CreateDateTime( timestampTicks, timeZone ),
                    additionalTimestampTicks.Select( t => CreateDateTime( t, timeZone ) ).ToArray() );

            return result;
        }

        private static Core.Chrono.ZonedDateTime CreateDateTime(long ticks, System.TimeZoneInfo timeZone)
        {
            return Core.Chrono.ZonedDateTime.CreateUtc( System.DateTime.UnixEpoch + TimeSpan.FromTicks( ticks ) ).ToTimeZone( timeZone );
        }
    }
}
