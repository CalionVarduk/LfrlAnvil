using System;
using FluentAssertions;
using LfrlSoft.NET.Core.Chrono.Extensions;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.ChronoTests.ExtensionsTests.TimeZoneInfoTests
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

        private static TimeZoneInfo.AdjustmentRule CreateRule(int deltaInHours)
        {
            return TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(
                DateTime.MinValue,
                DateTime.MaxValue,
                TimeSpan.FromHours( deltaInHours ),
                TimeZoneInfo.TransitionTime.CreateFixedDateRule(
                    new DateTime( 1, 1, 1, 2, 0, 0 ),
                    8,
                    26 ),
                TimeZoneInfo.TransitionTime.CreateFixedDateRule(
                    new DateTime( 1, 1, 1, 2, 0, 0 ),
                    10,
                    26 ) );
        }
    }
}
