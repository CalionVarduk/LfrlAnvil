using System;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlAnvil.TestExtensions;
using Xunit;

namespace LfrlAnvil.Chrono.Tests.ZonedClockTests
{
    public class ZonedClockTests : TestsBase
    {
        [Fact]
        public void Utc_ShouldReturnCorrectResult()
        {
            var sut = ZonedClock.Utc;
            sut.TimeZone.Should().Be( TimeZoneInfo.Utc );
        }

        [Fact]
        public void Local_ShouldReturnCorrectResult()
        {
            var sut = ZonedClock.Local;
            sut.TimeZone.Should().Be( TimeZoneInfo.Local );
        }

        [Fact]
        public void Ctor_ShouldReturnCorrectResult()
        {
            var timeZone = TimeZoneFactory.CreateRandom( Fixture );
            var sut = new ZonedClock( timeZone );
            sut.TimeZone.Should().Be( timeZone );
        }

        [Fact]
        public void GetNow_ShouldReturnCorrectResult()
        {
            var timeZone = TimeZoneFactory.CreateRandom( Fixture );
            var sut = new ZonedClock( timeZone );

            var expectedMinTimestamp = new Timestamp( DateTime.UtcNow );
            var result = sut.GetNow();
            var expectedMaxTimestamp = new Timestamp( DateTime.UtcNow );

            using ( new AssertionScope() )
            {
                result.Timestamp.Should().BeGreaterOrEqualTo( expectedMinTimestamp ).And.BeLessOrEqualTo( expectedMaxTimestamp );
                result.TimeZone.Should().Be( timeZone );
            }
        }
    }
}
