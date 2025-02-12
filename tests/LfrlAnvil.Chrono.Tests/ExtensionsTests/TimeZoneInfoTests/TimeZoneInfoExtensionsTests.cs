using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Chrono.Extensions;
using LfrlAnvil.TestExtensions.Attributes;

namespace LfrlAnvil.Chrono.Tests.ExtensionsTests.TimeZoneInfoTests;

[TestClass( typeof( TimeZoneInfoExtensionsTestsData ) )]
public class TimeZoneInfoExtensionsTests : TestsBase
{
    [Fact]
    public void GetDateTimeKind_ShouldReturnCorrectResultForUtcTimeZone()
    {
        var sut = TimeZoneInfo.Utc;
        var kind = sut.GetDateTimeKind();
        kind.TestEquals( DateTimeKind.Utc ).Go();
    }

    [Fact]
    public void GetDateTimeKind_ShouldReturnCorrectResultForLocalTimeZone()
    {
        var sut = TimeZoneInfo.Local;
        var kind = sut.GetDateTimeKind();
        kind.TestEquals( DateTimeKind.Local ).Go();
    }

    [Fact]
    public void GetDateTimeKind_ShouldReturnCorrectResultForOtherTimeZone()
    {
        var sut = TimeZoneFactory.CreateRandom( Fixture );
        var kind = sut.GetDateTimeKind();
        kind.TestEquals( DateTimeKind.Unspecified ).Go();
    }

    [Theory]
    [MethodData( nameof( TimeZoneInfoExtensionsTestsData.GetGetActiveAdjustmentRuleWithNullResultData ) )]
    public void GetActiveAdjustmentRule_ShouldReturnNullForTimeZoneWithoutAnyActiveRule(
        DateTime dateTimeToTest,
        IEnumerable<(DateTime Start, DateTime End)> ruleRanges)
    {
        var timeZoneOffset = TimeZoneFactory.CreateRandomOffset( Fixture, absMax: 13 );

        var sut = TimeZoneFactory.Create(
            timeZoneOffset,
            ruleRanges
                .Select(
                    r => TimeZoneFactory.CreateRule(
                        start: r.Start,
                        end: r.End,
                        transitionStart: new DateTime( 1, 2, 1, 12, 0, 0 ),
                        transitionEnd: new DateTime( 1, 11, 1, 12, 0, 0 ) ) )
                .ToArray() );

        var result = sut.GetActiveAdjustmentRule( dateTimeToTest );

        result.TestNull().Go();
    }

    [Theory]
    [MethodData( nameof( TimeZoneInfoExtensionsTestsData.GetGetActiveAdjustmentRuleData ) )]
    public void GetActiveAdjustmentRule_ShouldReturnCorrectActiveRule(
        DateTime dateTimeToTest,
        IEnumerable<(DateTime Start, DateTime End)> ruleRanges,
        (DateTime Start, DateTime End) expectedRange)
    {
        var timeZoneOffset = TimeZoneFactory.CreateRandomOffset( Fixture, absMax: 13 );

        var sut = TimeZoneFactory.Create(
            timeZoneOffset,
            ruleRanges
                .Select(
                    r => TimeZoneFactory.CreateRule(
                        start: r.Start,
                        end: r.End,
                        transitionStart: new DateTime( 1, 2, 1, 12, 0, 0 ),
                        transitionEnd: new DateTime( 1, 11, 1, 12, 0, 0 ) ) )
                .ToArray() );

        var result = sut.GetActiveAdjustmentRule( dateTimeToTest );

        Assertion.All(
                result.TestNotNull(),
                result.TestIf()
                    .NotNull(
                        r => Assertion.All(
                            "result.Value",
                            r.DateStart.TestEquals( expectedRange.Start ),
                            r.DateEnd.TestEquals( expectedRange.End ) ) ) )
            .Go();
    }

    [Theory]
    [MethodData( nameof( TimeZoneInfoExtensionsTestsData.GetGetActiveAdjustmentRuleWithNullResultData ) )]
    public void GetActiveAdjustmentRuleIndex_ShouldReturnMinusOneForTimeZoneWithoutAnyActiveRule(
        DateTime dateTimeToTest,
        IEnumerable<(DateTime Start, DateTime End)> ruleRanges)
    {
        var timeZoneOffset = TimeZoneFactory.CreateRandomOffset( Fixture, absMax: 13 );

        var sut = TimeZoneFactory.Create(
            timeZoneOffset,
            ruleRanges
                .Select(
                    r => TimeZoneFactory.CreateRule(
                        start: r.Start,
                        end: r.End,
                        transitionStart: new DateTime( 1, 2, 1, 12, 0, 0 ),
                        transitionEnd: new DateTime( 1, 11, 1, 12, 0, 0 ) ) )
                .ToArray() );

        var result = sut.GetActiveAdjustmentRuleIndex( dateTimeToTest );

        result.TestEquals( -1 ).Go();
    }

    [Theory]
    [MethodData( nameof( TimeZoneInfoExtensionsTestsData.GetGetActiveAdjustmentRuleIndexData ) )]
    public void GetActiveAdjustmentRuleIndex_ShouldReturnCorrectActiveRule(
        DateTime dateTimeToTest,
        IEnumerable<(DateTime Start, DateTime End)> ruleRanges,
        int expected)
    {
        var timeZoneOffset = TimeZoneFactory.CreateRandomOffset( Fixture, absMax: 13 );

        var sut = TimeZoneFactory.Create(
            timeZoneOffset,
            ruleRanges
                .Select(
                    r => TimeZoneFactory.CreateRule(
                        start: r.Start,
                        end: r.End,
                        transitionStart: new DateTime( 1, 2, 1, 12, 0, 0 ),
                        transitionEnd: new DateTime( 1, 11, 1, 12, 0, 0 ) ) )
                .ToArray() );

        var result = sut.GetActiveAdjustmentRuleIndex( dateTimeToTest );

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [MethodData( nameof( TimeZoneInfoExtensionsTestsData.GetGetContainingInvalidityRangeData ) )]
    public void GetContainingInvalidityRange_ShouldReturnCorrectResult(
        TimeZoneInfo timeZone,
        DateTime dateTime,
        (DateTime Start, DateTime End)? expected)
    {
        var expectedBounds = expected is null
            ? ( Bounds<DateTime>? )null
            : Bounds.Create( expected.Value.Start, expected.Value.End );

        var result = timeZone.GetContainingInvalidityRange( dateTime );

        Assertion.All(
                result.TestEquals( expectedBounds ),
                timeZone.IsInvalidTime( dateTime ).TestEquals( expected is not null ) )
            .Go();
    }

    [Theory]
    [MethodData( nameof( TimeZoneInfoExtensionsTestsData.GetGetContainingAmbiguityRangeData ) )]
    public void GetContainingAmbiguityRange_ShouldReturnCorrectResult(
        TimeZoneInfo timeZone,
        DateTime dateTime,
        (DateTime Start, DateTime End)? expected)
    {
        var expectedBounds = expected is null
            ? ( Bounds<DateTime>? )null
            : Bounds.Create( expected.Value.Start, expected.Value.End );

        var result = timeZone.GetContainingAmbiguityRange( dateTime );

        Assertion.All(
                result.TestEquals( expectedBounds ),
                timeZone.IsAmbiguousTime( dateTime ).TestEquals( expected is not null ) )
            .Go();
    }
}
