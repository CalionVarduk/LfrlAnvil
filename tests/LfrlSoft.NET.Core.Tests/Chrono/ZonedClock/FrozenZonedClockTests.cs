using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlSoft.NET.Core.Chrono;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Chrono.ZonedClock
{
    public class FrozenZonedClockTests : ZonedClockTestsBase
    {
        [Fact]
        public void Ctor_ShouldReturnCorrectResult()
        {
            var value = Fixture.Create<DateTime>();
            var timeZone = CreateTimeZone();

            var sut = new FrozenZonedClock( Core.Chrono.ZonedDateTime.Create( value, timeZone ) );

            sut.TimeZone.Should().Be( timeZone );
        }

        [Fact]
        public void GetNow_ShouldReturnCorrectResult()
        {
            var value = Fixture.Create<DateTime>();
            var timeZone = CreateTimeZone();
            var expected = Core.Chrono.ZonedDateTime.Create( value, timeZone );
            var sut = new FrozenZonedClock( expected );

            var firstResult = sut.GetNow();
            Task.Delay( TimeSpan.FromMilliseconds( 1 ) ).Wait();
            var secondResult = sut.GetNow();

            using ( new AssertionScope() )
            {
                firstResult.Should().Be( expected );
                secondResult.Should().Be( expected );
            }
        }
    }
}
