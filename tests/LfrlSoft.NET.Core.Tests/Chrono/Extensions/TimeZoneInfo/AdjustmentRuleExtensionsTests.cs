using FluentAssertions;
using LfrlSoft.NET.Core.Chrono.Extensions;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Chrono.Extensions.TimeZoneInfo
{
    public class AdjustmentRuleExtensionsTests : TestsBase
    {
        [Fact]
        public void GetTransitionTimeWithInvalidity_ShouldReturnStartTime_WhenDaylightDeltaIsPositive()
        {
            var deltaInHours = Fixture.CreatePositiveInt32() % 12 + 1;
            var sut = CreateRule( deltaInHours );

            var result = sut.GetTransitionTimeWithInvalidity();

            result.Should().Be( sut.DaylightTransitionStart );
        }

        [Fact]
        public void GetTransitionTimeWithInvalidity_ShouldReturnEndTime_WhenDaylightDeltaIsNegative()
        {
            var deltaInHours = Fixture.CreateNegativeInt32() % 12 - 1;
            var sut = CreateRule( deltaInHours );

            var result = sut.GetTransitionTimeWithInvalidity();

            result.Should().Be( sut.DaylightTransitionEnd );
        }

        [Fact]
        public void GetTransitionTimeWithAmbiguity_ShouldReturnEndTime_WhenDaylightDeltaIsPositive()
        {
            var deltaInHours = Fixture.CreatePositiveInt32() % 12 + 1;
            var sut = CreateRule( deltaInHours );

            var result = sut.GetTransitionTimeWithAmbiguity();

            result.Should().Be( sut.DaylightTransitionEnd );
        }

        [Fact]
        public void GetTransitionTimeWithAmbiguity_ShouldReturnStartTime_WhenDaylightDeltaIsNegative()
        {
            var deltaInHours = Fixture.CreateNegativeInt32() % 12 - 1;
            var sut = CreateRule( deltaInHours );

            var result = sut.GetTransitionTimeWithAmbiguity();

            result.Should().Be( sut.DaylightTransitionStart );
        }

        private static System.TimeZoneInfo.AdjustmentRule CreateRule(int deltaInHours)
        {
            return System.TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(
                System.DateTime.MinValue,
                System.DateTime.MaxValue,
                System.TimeSpan.FromHours( deltaInHours ),
                System.TimeZoneInfo.TransitionTime.CreateFixedDateRule(
                    new System.DateTime( 1, 1, 1, 2, 0, 0 ),
                    8,
                    26 ),
                System.TimeZoneInfo.TransitionTime.CreateFixedDateRule(
                    new System.DateTime( 1, 1, 1, 2, 0, 0 ),
                    10,
                    26 ) );
        }
    }
}
