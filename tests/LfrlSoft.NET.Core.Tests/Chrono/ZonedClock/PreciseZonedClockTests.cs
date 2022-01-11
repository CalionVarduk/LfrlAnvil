using System;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlSoft.NET.Core.Chrono;
using LfrlSoft.NET.Core.Functional;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Chrono.ZonedClock
{
    public class PreciseZonedClockTests : ZonedClockTestsBase
    {
        [Fact]
        public void Utc_ShouldReturnCorrectResult()
        {
            var sut = PreciseZonedClock.Utc;

            using ( new AssertionScope() )
            {
                sut.TimeZone.Should().Be( TimeZoneInfo.Utc );
                sut.MaxIdleTimeInTicks.Should().Be( Constants.TicksPerSecond );
            }
        }

        [Fact]
        public void Local_ShouldReturnCorrectResult()
        {
            var sut = PreciseZonedClock.Local;

            using ( new AssertionScope() )
            {
                sut.TimeZone.Should().Be( TimeZoneInfo.Local );
                sut.MaxIdleTimeInTicks.Should().Be( Constants.TicksPerSecond );
            }
        }

        [Theory]
        [InlineData( 0 )]
        [InlineData( -1 )]
        [InlineData( -2 )]
        public void Ctor_ShouldThrow_WhenMaxIdleTimeInTicksIsLessThanOne(long value)
        {
            var timeZone = CreateTimeZone();
            var action = Lambda.Of( () => new PreciseZonedClock( timeZone, value ) );
            action.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Theory]
        [InlineData( 1 )]
        [InlineData( 100 )]
        [InlineData( 12345 )]
        public void Ctor_ShouldReturnCorrectResult(long value)
        {
            var timeZone = CreateTimeZone();
            var sut = new PreciseZonedClock( timeZone, value );

            using ( new AssertionScope() )
            {
                sut.TimeZone.Should().Be( timeZone );
                sut.MaxIdleTimeInTicks.Should().Be( value );
            }
        }

        [Fact]
        public void DefaultCtor_ShouldReturnCorrectResult()
        {
            var timeZone = CreateTimeZone();
            var sut = new PreciseZonedClock( timeZone );

            using ( new AssertionScope() )
            {
                sut.TimeZone.Should().Be( timeZone );
                sut.MaxIdleTimeInTicks.Should().Be( Constants.TicksPerSecond );
            }
        }

        [Fact]
        public void GetNow_ShouldReturnCorrectResult()
        {
            var expectedMinTimestamp = new Core.Chrono.Timestamp( DateTime.UtcNow );
            var timeZone = CreateTimeZone();
            var sut = new PreciseZonedClock( timeZone, Constants.TicksPerDay );

            var result = sut.GetNow();
            var expectedMaxTimestamp = new Core.Chrono.Timestamp( DateTime.UtcNow );

            using ( new AssertionScope() )
            {
                result.Timestamp.Should().BeGreaterOrEqualTo( expectedMinTimestamp ).And.BeLessOrEqualTo( expectedMaxTimestamp );
                result.TimeZone.Should().Be( timeZone );
            }
        }

        [Fact]
        public void GetNow_ShouldReturnCorrectResult_WhenMaxIdleTimeInTicksIsExceeded()
        {
            var timeZone = CreateTimeZone();
            var sut = new PreciseZonedClock( timeZone, maxIdleTimeInTicks: 1 );

            Task.Delay( TimeSpan.FromMilliseconds( 1 ) ).Wait();

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
