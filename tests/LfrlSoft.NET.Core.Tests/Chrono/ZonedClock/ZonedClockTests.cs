using System;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Chrono.ZonedClock
{
    public class ZonedClockTests : ZonedClockTestsBase
    {
        [Fact]
        public void Utc_ShouldReturnCorrectResult()
        {
            var sut = Core.Chrono.ZonedClock.Utc;
            sut.TimeZone.Should().Be( TimeZoneInfo.Utc );
        }

        [Fact]
        public void Local_ShouldReturnCorrectResult()
        {
            var sut = Core.Chrono.ZonedClock.Local;
            sut.TimeZone.Should().Be( TimeZoneInfo.Local );
        }

        [Fact]
        public void Ctor_ShouldReturnCorrectResult()
        {
            var timeZone = CreateTimeZone();
            var sut = new Core.Chrono.ZonedClock( timeZone );
            sut.TimeZone.Should().Be( timeZone );
        }

        [Fact]
        public void GetNow_ShouldReturnCorrectResult()
        {
            var timeZone = CreateTimeZone();
            var sut = new Core.Chrono.ZonedClock( timeZone );

            var expectedMinTimestamp = new Core.Chrono.Timestamp( DateTime.UtcNow );
            var result = sut.GetNow();
            var expectedMaxTimestamp = new Core.Chrono.Timestamp( DateTime.UtcNow );

            using ( new AssertionScope() )
            {
                result.Timestamp.Should().BeGreaterOrEqualTo( expectedMinTimestamp ).And.BeLessOrEqualTo( expectedMaxTimestamp );
                result.TimeZone.Should().Be( timeZone );
            }
        }
    }
}
