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

namespace LfrlSoft.NET.Core.Tests.ChronoTests.ExtensionsTests.TimeZoneInfoTests
{
    [TestClass( typeof( TimeZoneInfoExtensionsTestsData ) )]
    public class TimeZoneInfoExtensionsTests : TestsBase
    {
        [Fact]
        public void GetDateTimeKind_ShouldReturnCorrectResultForUtcTimeZone()
        {
            var sut = TimeZoneInfo.Utc;
            var kind = sut.GetDateTimeKind();
            kind.Should().Be( DateTimeKind.Utc );
        }

        [Fact]
        public void GetDateTimeKind_ShouldReturnCorrectResultForLocalTimeZone()
        {
            var sut = TimeZoneInfo.Local;
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
            DateTime dateTimeToTest,
            IEnumerable<(DateTime Start, DateTime End)> ruleRanges)
        {
            var timeZoneOffset = Fixture.Create<int>() % 12;

            var sut = TimeZoneInfoExtensionsTestsData.GetTimeZone(
                $"{timeZoneOffset}",
                timeZoneOffset,
                ruleRanges
                    .Select(
                        r => TimeZoneInfoExtensionsTestsData.CreateAdjustmentRule(
                            r.Start,
                            r.End,
                            new DateTime( 1, 2, 1, 12, 0, 0 ),
                            new DateTime( 1, 11, 1, 12, 0, 0 ) ) )
                    .ToArray() );

            var result = sut.GetActiveAdjustmentRule( dateTimeToTest );

            result.Should().BeNull();
        }

        [Theory]
        [MethodData( nameof( TimeZoneInfoExtensionsTestsData.GetGetActiveAdjustmentRuleData ) )]
        public void GetActiveAdjustmentRule_ShouldReturnCorrectActiveRule(
            DateTime dateTimeToTest,
            IEnumerable<(DateTime Start, DateTime End)> ruleRanges,
            (DateTime Start, DateTime End) expectedRange)
        {
            var timeZoneOffset = Fixture.Create<int>() % 12;

            var sut = TimeZoneInfoExtensionsTestsData.GetTimeZone(
                $"{timeZoneOffset}",
                timeZoneOffset,
                ruleRanges
                    .Select(
                        r => TimeZoneInfoExtensionsTestsData.CreateAdjustmentRule(
                            r.Start,
                            r.End,
                            new DateTime( 1, 2, 1, 12, 0, 0 ),
                            new DateTime( 1, 11, 1, 12, 0, 0 ) ) )
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
            DateTime dateTimeToTest,
            IEnumerable<(DateTime Start, DateTime End)> ruleRanges)
        {
            var timeZoneOffset = Fixture.Create<int>() % 12;

            var sut = TimeZoneInfoExtensionsTestsData.GetTimeZone(
                $"{timeZoneOffset}",
                timeZoneOffset,
                ruleRanges
                    .Select(
                        r => TimeZoneInfoExtensionsTestsData.CreateAdjustmentRule(
                            r.Start,
                            r.End,
                            new DateTime( 1, 2, 1, 12, 0, 0 ),
                            new DateTime( 1, 11, 1, 12, 0, 0 ) ) )
                    .ToArray() );

            var result = sut.GetActiveAdjustmentRuleIndex( dateTimeToTest );

            result.Should().Be( -1 );
        }

        [Theory]
        [MethodData( nameof( TimeZoneInfoExtensionsTestsData.GetGetActiveAdjustmentRuleIndexData ) )]
        public void GetActiveAdjustmentRuleIndex_ShouldReturnCorrectActiveRule(
            DateTime dateTimeToTest,
            IEnumerable<(DateTime Start, DateTime End)> ruleRanges,
            int expected)
        {
            var timeZoneOffset = Fixture.Create<int>() % 12;

            var sut = TimeZoneInfoExtensionsTestsData.GetTimeZone(
                $"{timeZoneOffset}",
                timeZoneOffset,
                ruleRanges
                    .Select(
                        r => TimeZoneInfoExtensionsTestsData.CreateAdjustmentRule(
                            r.Start,
                            r.End,
                            new DateTime( 1, 2, 1, 12, 0, 0 ),
                            new DateTime( 1, 11, 1, 12, 0, 0 ) ) )
                    .ToArray() );

            var result = sut.GetActiveAdjustmentRuleIndex( dateTimeToTest );

            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( TimeZoneInfoExtensionsTestsData.GetGetContainingInvalidityRangeData ) )]
        private void GetContainingInvalidityRange_ShouldReturnCorrectResult(
            TimeZoneInfo timeZone,
            DateTime dateTime,
            (DateTime Start, DateTime End)? expected)
        {
            var expectedBounds = expected is null
                ? (Bounds<DateTime>?)null
                : Bounds.Create( expected.Value.Start, expected.Value.End );

            var result = timeZone.GetContainingInvalidityRange( dateTime );

            using ( new AssertionScope() )
            {
                result.Should().Be( expectedBounds );
                timeZone.IsInvalidTime( dateTime ).Should().Be( expected is not null );
            }
        }

        [Theory]
        [MethodData( nameof( TimeZoneInfoExtensionsTestsData.GetGetContainingAmbiguityRangeData ) )]
        private void GetContainingAmbiguityRange_ShouldReturnCorrectResult(
            TimeZoneInfo timeZone,
            DateTime dateTime,
            (DateTime Start, DateTime End)? expected)
        {
            var expectedBounds = expected is null
                ? (Bounds<DateTime>?)null
                : Bounds.Create( expected.Value.Start, expected.Value.End );

            var result = timeZone.GetContainingAmbiguityRange( dateTime );

            using ( new AssertionScope() )
            {
                result.Should().Be( expectedBounds );
                timeZone.IsAmbiguousTime( dateTime ).Should().Be( expected is not null );
            }
        }
    }
}
