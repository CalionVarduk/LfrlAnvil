using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlSoft.NET.Core.Chrono.Extensions;
using LfrlSoft.NET.TestExtensions;
using LfrlSoft.NET.TestExtensions.Attributes;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Chrono.Extensions.TimeZoneInfo
{
    [TestClass( typeof( TimeZoneInfoExtensionsTestsData ) )]
    public class TimeZoneInfoExtensionsTests : TestsBase
    {
        [Fact]
        public void GetDateTimeKind_ShouldReturnCorrectResultForUtcTimeZone()
        {
            var sut = System.TimeZoneInfo.Utc;
            var kind = sut.GetDateTimeKind();
            kind.Should().Be( DateTimeKind.Utc );
        }

        [Fact]
        public void GetDateTimeKind_ShouldReturnCorrectResultForLocalTimeZone()
        {
            var sut = System.TimeZoneInfo.Local;
            var kind = sut.GetDateTimeKind();
            kind.Should().Be( DateTimeKind.Local );
        }

        [Fact]
        public void GetDateTimeKind_ShouldReturnCorrectResultForOtherTimeZone()
        {
            var timeZoneOffset = Fixture.Create<int>() % 12;
            var sut = TimeZoneInfoExtensionsTestsData.GetTimeZone( $"{timeZoneOffset}", timeZoneOffset );
            var kind = sut.GetDateTimeKind();
            kind.Should().Be( DateTimeKind.Unspecified );
        }

        [Theory]
        [MethodData( nameof( TimeZoneInfoExtensionsTestsData.GetGetActiveAdjustmentRuleWithNullResultData ) )]
        public void GetActiveAdjustmentRule_ShouldReturnNullForTimeZoneWithoutAnyActiveRule(
            System.DateTime dateTimeToTest,
            IEnumerable<(System.DateTime Start, System.DateTime End)> ruleRanges)
        {
            var timeZoneOffset = Fixture.Create<int>() % 12;

            var sut = TimeZoneInfoExtensionsTestsData.GetTimeZone(
                $"{timeZoneOffset}",
                timeZoneOffset,
                ruleRanges
                    .Select( r => CreateAdjustmentRule( r.Start, r.End ) )
                    .ToArray() );

            var result = sut.GetActiveAdjustmentRule( dateTimeToTest );

            result.Should().BeNull();
        }

        [Theory]
        [MethodData( nameof( TimeZoneInfoExtensionsTestsData.GetGetActiveAdjustmentRuleData ) )]
        public void GetActiveAdjustmentRule_ShouldReturnCorrectActiveRule(
            System.DateTime dateTimeToTest,
            IEnumerable<(System.DateTime Start, System.DateTime End)> ruleRanges,
            (System.DateTime Start, System.DateTime End) expectedRange)
        {
            var timeZoneOffset = Fixture.Create<int>() % 12;

            var sut = TimeZoneInfoExtensionsTestsData.GetTimeZone(
                $"{timeZoneOffset}",
                timeZoneOffset,
                ruleRanges
                    .Select( r => CreateAdjustmentRule( r.Start, r.End ) )
                    .ToArray() );

            var result = sut.GetActiveAdjustmentRule( dateTimeToTest );

            using ( new AssertionScope() )
            {
                result.Should().NotBeNull();
                result?.DateStart.Should().Be( expectedRange.Start );
                result?.DateEnd.Should().Be( expectedRange.End );
            }
        }

        [Theory]
        [MethodData( nameof( TimeZoneInfoExtensionsTestsData.GetGetActiveAdjustmentRuleWithNullResultData ) )]
        public void GetActiveAdjustmentRuleIndex_ShouldReturnMinusOneForTimeZoneWithoutAnyActiveRule(
            System.DateTime dateTimeToTest,
            IEnumerable<(System.DateTime Start, System.DateTime End)> ruleRanges)
        {
            var timeZoneOffset = Fixture.Create<int>() % 12;

            var sut = TimeZoneInfoExtensionsTestsData.GetTimeZone(
                $"{timeZoneOffset}",
                timeZoneOffset,
                ruleRanges
                    .Select( r => CreateAdjustmentRule( r.Start, r.End ) )
                    .ToArray() );

            var result = sut.GetActiveAdjustmentRuleIndex( dateTimeToTest );

            result.Should().Be( -1 );
        }

        [Theory]
        [MethodData( nameof( TimeZoneInfoExtensionsTestsData.GetGetActiveAdjustmentRuleIndexData ) )]
        public void GetActiveAdjustmentRuleIndex_ShouldReturnCorrectActiveRule(
            System.DateTime dateTimeToTest,
            IEnumerable<(System.DateTime Start, System.DateTime End)> ruleRanges,
            int expected)
        {
            var timeZoneOffset = Fixture.Create<int>() % 12;

            var sut = TimeZoneInfoExtensionsTestsData.GetTimeZone(
                $"{timeZoneOffset}",
                timeZoneOffset,
                ruleRanges
                    .Select( r => CreateAdjustmentRule( r.Start, r.End ) )
                    .ToArray() );

            var result = sut.GetActiveAdjustmentRuleIndex( dateTimeToTest );

            result.Should().Be( expected );
        }

        private static System.TimeZoneInfo.AdjustmentRule CreateAdjustmentRule(
            System.DateTime start,
            System.DateTime end,
            double daylightDeltaInHours = 1)
        {
            return System.TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(
                dateStart: start,
                dateEnd: end,
                daylightDelta: TimeSpan.FromHours( daylightDeltaInHours ),
                daylightTransitionStart: System.TimeZoneInfo.TransitionTime.CreateFixedDateRule(
                    timeOfDay: new System.DateTime( 1, 1, 1, 12, 0, 0 ),
                    month: 2,
                    day: 1 ),
                daylightTransitionEnd: System.TimeZoneInfo.TransitionTime.CreateFixedDateRule(
                    timeOfDay: new System.DateTime( 1, 1, 1, 12, 0, 0 ),
                    month: 11,
                    day: 1 ) );
        }
    }
}
