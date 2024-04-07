using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Chrono.Extensions;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions.Attributes;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Chrono.Tests.ZonedWeekTests;

[TestClass( typeof( ZonedWeekTestsData ) )]
public class ZonedWeekTests : TestsBase
{
    [Fact]
    public void Default_ShouldReturnWeekStartingAtUnixEpochInUtcTimeZone()
    {
        var result = default( ZonedWeek );
        var expectedStart = ZonedDateTime.CreateUtc( DateTime.UnixEpoch );
        var expectedEnd = ZonedDateTime.CreateUtc( DateTime.UnixEpoch.AddDays( 7 ).AddTicks( -1 ) );

        using ( new AssertionScope() )
        {
            result.Start.Should().Be( expectedStart );
            result.End.Should().Be( expectedEnd );
            result.Year.Should().Be( result.Start.Year );
            result.WeekOfYear.Should().Be( 1 );
            result.TimeZone.Should().Be( TimeZoneInfo.Utc );
            result.Duration.Should().Be( Duration.FromHours( 168 ) );
            result.IsLocal.Should().BeFalse();
            result.IsUtc.Should().BeTrue();
        }
    }

    [Theory]
    [MethodData( nameof( ZonedWeekTestsData.GetCreateData ) )]
    public void Create_ShouldReturnCorrectResult(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        IsoDayOfWeek weekStart,
        int expectedYear,
        int expectedWeekOfYear)
    {
        var expectedStart = ZonedDateTime.Create( dateTime.GetStartOfWeek( weekStart.ToBcl() ), timeZone );
        var expectedEnd = ZonedDateTime.Create( dateTime.GetEndOfWeek( weekStart.ToBcl() ), timeZone );
        var sut = ZonedWeek.Create( dateTime, timeZone, weekStart );

        using ( new AssertionScope() )
        {
            sut.Start.Should().Be( expectedStart );
            sut.End.Should().Be( expectedEnd );
            sut.Year.Should().Be( expectedYear );
            sut.WeekOfYear.Should().Be( expectedWeekOfYear );
            sut.TimeZone.Should().Be( timeZone );
            sut.Duration.Should().Be( Duration.FromHours( 168 ) );
            sut.IsLocal.Should().BeFalse();
            sut.IsUtc.Should().BeFalse();
        }
    }

    [Theory]
    [MethodData( nameof( ZonedWeekTestsData.GetCreateWithContainedInvalidityRangeData ) )]
    public void Create_WithContainedInvalidityRange_ShouldReturnCorrectResult(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        IsoDayOfWeek weekStart,
        int expectedYear,
        int expectedWeekOfYear,
        Duration expectedDuration)
    {
        var expectedStart = ZonedDateTime.Create( dateTime.GetStartOfWeek( weekStart.ToBcl() ), timeZone );
        var expectedEnd = ZonedDateTime.Create( dateTime.GetEndOfWeek( weekStart.ToBcl() ), timeZone );

        var sut = ZonedWeek.Create( dateTime, timeZone, weekStart );

        using ( new AssertionScope() )
        {
            sut.Start.Should().Be( expectedStart );
            sut.End.Should().Be( expectedEnd );
            sut.Year.Should().Be( expectedYear );
            sut.WeekOfYear.Should().Be( expectedWeekOfYear );
            sut.TimeZone.Should().Be( timeZone );
            sut.Duration.Should().Be( expectedDuration );
            sut.IsLocal.Should().BeFalse();
            sut.IsUtc.Should().BeFalse();
        }
    }

    [Theory]
    [MethodData( nameof( ZonedWeekTestsData.GetCreateWithContainedAmbiguityRangeData ) )]
    public void Create_WithContainedAmbiguityRange_ShouldReturnCorrectResult(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        IsoDayOfWeek weekStart,
        int expectedYear,
        int expectedWeekOfYear,
        Duration expectedDuration)
    {
        var expectedStart = ZonedDateTime.Create( dateTime.GetStartOfWeek( weekStart.ToBcl() ), timeZone );
        var expectedEnd = ZonedDateTime.Create( dateTime.GetEndOfWeek( weekStart.ToBcl() ), timeZone );

        var sut = ZonedWeek.Create( dateTime, timeZone, weekStart );

        using ( new AssertionScope() )
        {
            sut.Start.Should().Be( expectedStart );
            sut.End.Should().Be( expectedEnd );
            sut.Year.Should().Be( expectedYear );
            sut.WeekOfYear.Should().Be( expectedWeekOfYear );
            sut.TimeZone.Should().Be( timeZone );
            sut.Duration.Should().Be( expectedDuration );
            sut.IsLocal.Should().BeFalse();
            sut.IsUtc.Should().BeFalse();
        }
    }

    [Theory]
    [MethodData( nameof( ZonedWeekTestsData.GetCreateWithInvalidStartTimeData ) )]
    public void Create_WithInvalidStartTime_ShouldReturnResultWithEarliestPossibleStart(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        DateTime expectedStartValue,
        IsoDayOfWeek weekStart,
        int expectedYear,
        int expectedWeekOfYear,
        Duration expectedDuration)
    {
        var expectedStart = ZonedDateTime.Create( expectedStartValue, timeZone );
        var expectedEnd = ZonedDateTime.Create( dateTime.GetEndOfWeek( weekStart.ToBcl() ), timeZone );

        var sut = ZonedWeek.Create( dateTime, timeZone, weekStart );

        using ( new AssertionScope() )
        {
            sut.Start.Should().Be( expectedStart );
            sut.End.Should().Be( expectedEnd );
            sut.Year.Should().Be( expectedYear );
            sut.WeekOfYear.Should().Be( expectedWeekOfYear );
            sut.TimeZone.Should().Be( timeZone );
            sut.Duration.Should().Be( expectedDuration );
            sut.IsLocal.Should().BeFalse();
            sut.IsUtc.Should().BeFalse();
        }
    }

    [Theory]
    [MethodData( nameof( ZonedWeekTestsData.GetCreateWithInvalidEndTimeData ) )]
    public void Create_WithInvalidEndTime_ShouldReturnResultWithLatestPossibleEnd(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        DateTime expectedEndValue,
        IsoDayOfWeek weekStart,
        int expectedYear,
        int expectedWeekOfYear,
        Duration expectedDuration)
    {
        var expectedStart = ZonedDateTime.Create( dateTime.GetStartOfWeek( weekStart.ToBcl() ), timeZone );
        var expectedEnd = ZonedDateTime.Create( expectedEndValue, timeZone );

        var sut = ZonedWeek.Create( dateTime, timeZone, weekStart );

        using ( new AssertionScope() )
        {
            sut.Start.Should().Be( expectedStart );
            sut.End.Should().Be( expectedEnd );
            sut.Year.Should().Be( expectedYear );
            sut.WeekOfYear.Should().Be( expectedWeekOfYear );
            sut.TimeZone.Should().Be( timeZone );
            sut.Duration.Should().Be( expectedDuration );
            sut.IsLocal.Should().BeFalse();
            sut.IsUtc.Should().BeFalse();
        }
    }

    [Theory]
    [MethodData( nameof( ZonedWeekTestsData.GetCreateWithAmbiguousStartTimeData ) )]
    public void Create_WithAmbiguousStartTime_ShouldReturnResultWithEarliestPossibleStart(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        bool forceInDaylightSavingMode,
        IsoDayOfWeek weekStart,
        int expectedYear,
        int expectedWeekOfYear,
        Duration expectedDuration)
    {
        var expectedStart = ZonedDateTime.Create( dateTime.GetStartOfWeek( weekStart.ToBcl() ), timeZone );
        if ( forceInDaylightSavingMode )
            expectedStart = expectedStart.GetOppositeAmbiguousDateTime() ?? expectedStart;

        var expectedEnd = ZonedDateTime.Create( dateTime.GetEndOfWeek( weekStart.ToBcl() ), timeZone );

        var sut = ZonedWeek.Create( dateTime, timeZone, weekStart );

        using ( new AssertionScope() )
        {
            sut.Start.Should().Be( expectedStart );
            sut.End.Should().Be( expectedEnd );
            sut.Year.Should().Be( expectedYear );
            sut.WeekOfYear.Should().Be( expectedWeekOfYear );
            sut.TimeZone.Should().Be( timeZone );
            sut.Duration.Should().Be( expectedDuration );
            sut.IsLocal.Should().BeFalse();
            sut.IsUtc.Should().BeFalse();
        }
    }

    [Theory]
    [MethodData( nameof( ZonedWeekTestsData.GetCreateWithAmbiguousEndTimeData ) )]
    public void Create_WithAmbiguousEndTime_ShouldReturnResultWithLatestPossibleEnd(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        bool forceInDaylightSavingMode,
        IsoDayOfWeek weekStart,
        int expectedYear,
        int expectedWeekOfYear,
        Duration expectedDuration)
    {
        var expectedStart = ZonedDateTime.Create( dateTime.GetStartOfWeek( weekStart.ToBcl() ), timeZone );
        var expectedEnd = ZonedDateTime.Create( dateTime.GetEndOfWeek( weekStart.ToBcl() ), timeZone );
        if ( forceInDaylightSavingMode )
            expectedEnd = expectedEnd.GetOppositeAmbiguousDateTime() ?? expectedEnd;

        var sut = ZonedWeek.Create( dateTime, timeZone, weekStart );

        using ( new AssertionScope() )
        {
            sut.Start.Should().Be( expectedStart );
            sut.End.Should().Be( expectedEnd );
            sut.Year.Should().Be( expectedYear );
            sut.WeekOfYear.Should().Be( expectedWeekOfYear );
            sut.TimeZone.Should().Be( timeZone );
            sut.Duration.Should().Be( expectedDuration );
            sut.IsLocal.Should().BeFalse();
            sut.IsUtc.Should().BeFalse();
        }
    }

    [Theory]
    [MethodData( nameof( ZonedWeekTestsData.GetCreateWithInvalidStartTimeAndAmbiguousEndTimeData ) )]
    public void Create_WithInvalidStartTimeAndAmbiguousEndTime_ShouldReturnResultWithEarliestPossibleStart_AndLatestPossibleEnd(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        DateTime expectedStartValue,
        bool forceInDaylightSavingMode,
        IsoDayOfWeek weekStart,
        int expectedYear,
        int expectedWeekOfYear,
        Duration expectedDuration)
    {
        var expectedStart = ZonedDateTime.Create( expectedStartValue, timeZone );
        var expectedEnd = ZonedDateTime.Create( dateTime.GetEndOfWeek( weekStart.ToBcl() ), timeZone );
        if ( forceInDaylightSavingMode )
            expectedEnd = expectedEnd.GetOppositeAmbiguousDateTime() ?? expectedEnd;

        var sut = ZonedWeek.Create( dateTime, timeZone, weekStart );

        using ( new AssertionScope() )
        {
            sut.Start.Should().Be( expectedStart );
            sut.End.Should().Be( expectedEnd );
            sut.Year.Should().Be( expectedYear );
            sut.WeekOfYear.Should().Be( expectedWeekOfYear );
            sut.TimeZone.Should().Be( timeZone );
            sut.Duration.Should().Be( expectedDuration );
            sut.IsLocal.Should().BeFalse();
            sut.IsUtc.Should().BeFalse();
        }
    }

    [Theory]
    [MethodData( nameof( ZonedWeekTestsData.GetCreateWithAmbiguousStartTimeAndInvalidEndTimeData ) )]
    public void Create_WithAmbiguousStartTimeAndInvalidEndTime_ShouldReturnResultWithEarliestPossibleStart_AndLatestPossibleEnd(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        DateTime expectedEndValue,
        bool forceInDaylightSavingMode,
        IsoDayOfWeek weekStart,
        int expectedYear,
        int expectedWeekOfYear,
        Duration expectedDuration)
    {
        var expectedStart = ZonedDateTime.Create( dateTime.GetStartOfWeek( weekStart.ToBcl() ), timeZone );
        if ( forceInDaylightSavingMode )
            expectedStart = expectedStart.GetOppositeAmbiguousDateTime() ?? expectedStart;

        var expectedEnd = ZonedDateTime.Create( expectedEndValue, timeZone );

        var sut = ZonedWeek.Create( dateTime, timeZone, weekStart );

        using ( new AssertionScope() )
        {
            sut.Start.Should().Be( expectedStart );
            sut.End.Should().Be( expectedEnd );
            sut.Year.Should().Be( expectedYear );
            sut.WeekOfYear.Should().Be( expectedWeekOfYear );
            sut.TimeZone.Should().Be( timeZone );
            sut.Duration.Should().Be( expectedDuration );
            sut.IsLocal.Should().BeFalse();
            sut.IsUtc.Should().BeFalse();
        }
    }

    [Theory]
    [MethodData( nameof( ZonedWeekTestsData.GetCreateWithContainedInvalidityAndAmbiguityRangesData ) )]
    public void Create_WithContainedInvalidityAndAmbiguityRanges_ShouldReturnCorrectResult(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        IsoDayOfWeek weekStart,
        int expectedYear,
        int expectedWeekOfYear)
    {
        var expectedStart = ZonedDateTime.Create( dateTime.GetStartOfWeek( weekStart.ToBcl() ), timeZone );
        var expectedEnd = ZonedDateTime.Create( dateTime.GetEndOfWeek( weekStart.ToBcl() ), timeZone );

        var sut = ZonedWeek.Create( dateTime, timeZone, weekStart );

        using ( new AssertionScope() )
        {
            sut.Start.Should().Be( expectedStart );
            sut.End.Should().Be( expectedEnd );
            sut.Year.Should().Be( expectedYear );
            sut.WeekOfYear.Should().Be( expectedWeekOfYear );
            sut.TimeZone.Should().Be( timeZone );
            sut.Duration.Should().Be( Duration.FromHours( 168 ) );
            sut.IsLocal.Should().BeFalse();
            sut.IsUtc.Should().BeFalse();
        }
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( 8 )]
    public void Create_ShouldThrowArgumentOutOfRangeException_WhenWeekStartIsInvalid(int weekStart)
    {
        var dateTime = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var action = Lambda.Of( () => ZonedWeek.Create( dateTime, timeZone, ( IsoDayOfWeek )weekStart ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData( IsoDayOfWeek.Monday )]
    [InlineData( IsoDayOfWeek.Tuesday )]
    [InlineData( IsoDayOfWeek.Wednesday )]
    [InlineData( IsoDayOfWeek.Thursday )]
    [InlineData( IsoDayOfWeek.Friday )]
    [InlineData( IsoDayOfWeek.Saturday )]
    [InlineData( IsoDayOfWeek.Sunday )]
    public void Create_WithZonedDateTime_ShouldBeEquivalentToCreateWithDateTimeAndTimeZoneInfo(IsoDayOfWeek weekStart)
    {
        var dateTime = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var source = ZonedDateTime.Create( dateTime, timeZone );
        var expected = ZonedWeek.Create( dateTime, timeZone, weekStart );

        var result = ZonedWeek.Create( source, weekStart );

        result.Should().BeEquivalentTo( expected );
    }

    [Theory]
    [InlineData( IsoDayOfWeek.Monday )]
    [InlineData( IsoDayOfWeek.Tuesday )]
    [InlineData( IsoDayOfWeek.Wednesday )]
    [InlineData( IsoDayOfWeek.Thursday )]
    [InlineData( IsoDayOfWeek.Friday )]
    [InlineData( IsoDayOfWeek.Saturday )]
    [InlineData( IsoDayOfWeek.Sunday )]
    public void Create_WithZonedDay_ShouldBeEquivalentToCreateWithDateTimeAndTimeZoneInfo(IsoDayOfWeek weekStart)
    {
        var dateTime = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var source = ZonedDay.Create( dateTime, timeZone );
        var expected = ZonedWeek.Create( dateTime, timeZone, weekStart );

        var result = ZonedWeek.Create( source, weekStart );

        result.Should().BeEquivalentTo( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedWeekTestsData.GetCreateWithWeekOfYearData ) )]
    public void Create_WithWeekOfYear_ShouldBeEquivalentToCreateWithDateTimeAndTimeZoneInfo(
        int year,
        int weekOfYear,
        IsoDayOfWeek weekStart,
        DateTime expectedStartDay)
    {
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var expected = ZonedWeek.Create( expectedStartDay, timeZone, weekStart );

        var result = ZonedWeek.Create( year, weekOfYear, timeZone, weekStart );

        result.Should().BeEquivalentTo( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedWeekTestsData.GetCreateWithWeekOfYearThrowData ) )]
    public void Create_WithWeekOfYear_ShouldThrowArgumentOutOfRangeException_WhenWeekOfYearIsInvalid(
        int year,
        int weekOfYear,
        IsoDayOfWeek weekStart)
    {
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var action = Lambda.Of( () => ZonedWeek.Create( year, weekOfYear, timeZone, weekStart ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [MethodData( nameof( ZonedWeekTestsData.GetCreateWithWeekOfYearData ) )]
    public void TryCreate_WithWeekOfYear_ShouldBeEquivalentToCreateWithDateTimeAndTimeZoneInfo(
        int year,
        int weekOfYear,
        IsoDayOfWeek weekStart,
        DateTime expectedStartDay)
    {
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var expected = ZonedWeek.Create( expectedStartDay, timeZone, weekStart );

        var result = ZonedWeek.TryCreate( year, weekOfYear, timeZone, weekStart );

        result.Should().BeEquivalentTo( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedWeekTestsData.GetCreateWithWeekOfYearThrowData ) )]
    public void TryCreate_WithWeekOfYear_ShouldReturnNull_WhenWeekOfYearIsInvalid(
        int year,
        int weekOfYear,
        IsoDayOfWeek weekStart)
    {
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var result = ZonedWeek.TryCreate( year, weekOfYear, timeZone, weekStart );
        result.Should().BeNull();
    }

    [Theory]
    [InlineData( IsoDayOfWeek.Monday )]
    [InlineData( IsoDayOfWeek.Tuesday )]
    [InlineData( IsoDayOfWeek.Wednesday )]
    [InlineData( IsoDayOfWeek.Thursday )]
    [InlineData( IsoDayOfWeek.Friday )]
    [InlineData( IsoDayOfWeek.Saturday )]
    [InlineData( IsoDayOfWeek.Sunday )]
    public void CreateUtc_WithTimestamp_ShouldReturnCorrectWeekInUtc(IsoDayOfWeek weekStart)
    {
        var timestamp = new Timestamp( Fixture.Create<DateTime>() );
        var expected = ZonedWeek.Create( timestamp.UtcValue, TimeZoneInfo.Utc, weekStart );

        var result = ZonedWeek.CreateUtc( timestamp, weekStart );

        result.Should().BeEquivalentTo( expected );
    }

    [Theory]
    [InlineData( IsoDayOfWeek.Monday )]
    [InlineData( IsoDayOfWeek.Tuesday )]
    [InlineData( IsoDayOfWeek.Wednesday )]
    [InlineData( IsoDayOfWeek.Thursday )]
    [InlineData( IsoDayOfWeek.Friday )]
    [InlineData( IsoDayOfWeek.Saturday )]
    [InlineData( IsoDayOfWeek.Sunday )]
    public void CreateUtc_WithDateTime_ShouldReturnCorrectWeekInUtc(IsoDayOfWeek weekStart)
    {
        var dateTime = Fixture.Create<DateTime>();
        var expectedStart = ZonedDateTime.CreateUtc( dateTime.GetStartOfWeek( weekStart.ToBcl() ) );
        var expectedEnd = ZonedDateTime.CreateUtc( dateTime.GetEndOfWeek( weekStart.ToBcl() ) );

        var result = ZonedWeek.CreateUtc( dateTime, weekStart );

        using ( new AssertionScope() )
        {
            result.Start.Should().Be( expectedStart );
            result.End.Should().Be( expectedEnd );
            result.TimeZone.Should().Be( TimeZoneInfo.Utc );
            result.Duration.Should().Be( Duration.FromHours( 168 ) );
            result.IsLocal.Should().BeFalse();
            result.IsUtc.Should().BeTrue();
        }
    }

    [Theory]
    [InlineData( IsoDayOfWeek.Monday )]
    [InlineData( IsoDayOfWeek.Tuesday )]
    [InlineData( IsoDayOfWeek.Wednesday )]
    [InlineData( IsoDayOfWeek.Thursday )]
    [InlineData( IsoDayOfWeek.Friday )]
    [InlineData( IsoDayOfWeek.Saturday )]
    [InlineData( IsoDayOfWeek.Sunday )]
    public void CreateUtc_WithWeekOfYear_ShouldReturnCorrectWeekInUtc(IsoDayOfWeek weekStart)
    {
        var year = Fixture.Create<DateTime>().Year;
        var weekOfYear = Fixture.CreatePositiveInt32() % 50 + 1;
        var expected = ZonedWeek.Create( year, weekOfYear, TimeZoneInfo.Utc, weekStart );

        var result = ZonedWeek.CreateUtc( year, weekOfYear, weekStart );

        result.Should().BeEquivalentTo( expected );
    }

    [Theory]
    [InlineData( IsoDayOfWeek.Monday )]
    [InlineData( IsoDayOfWeek.Tuesday )]
    [InlineData( IsoDayOfWeek.Wednesday )]
    [InlineData( IsoDayOfWeek.Thursday )]
    [InlineData( IsoDayOfWeek.Friday )]
    [InlineData( IsoDayOfWeek.Saturday )]
    [InlineData( IsoDayOfWeek.Sunday )]
    public void TryCreateUtc_WithWeekOfYear_ShouldReturnCorrectWeekInUtc(IsoDayOfWeek weekStart)
    {
        var year = Fixture.Create<DateTime>().Year;
        var weekOfYear = Fixture.CreatePositiveInt32() % 50 + 1;
        var expected = ZonedWeek.Create( year, weekOfYear, TimeZoneInfo.Utc, weekStart );

        var result = ZonedWeek.TryCreateUtc( year, weekOfYear, weekStart );

        result.Should().BeEquivalentTo( expected );
    }

    [Theory]
    [InlineData( IsoDayOfWeek.Monday )]
    [InlineData( IsoDayOfWeek.Tuesday )]
    [InlineData( IsoDayOfWeek.Wednesday )]
    [InlineData( IsoDayOfWeek.Thursday )]
    [InlineData( IsoDayOfWeek.Friday )]
    [InlineData( IsoDayOfWeek.Saturday )]
    [InlineData( IsoDayOfWeek.Sunday )]
    public void CreateLocal_ShouldReturnCorrectWeekInLocal(IsoDayOfWeek weekStart)
    {
        var dateTime = Fixture.Create<DateTime>();
        var expected = ZonedWeek.Create( dateTime, TimeZoneInfo.Local, weekStart );

        var result = ZonedWeek.CreateLocal( dateTime, weekStart );

        result.Should().BeEquivalentTo( expected );
    }

    [Theory]
    [InlineData( IsoDayOfWeek.Monday )]
    [InlineData( IsoDayOfWeek.Tuesday )]
    [InlineData( IsoDayOfWeek.Wednesday )]
    [InlineData( IsoDayOfWeek.Thursday )]
    [InlineData( IsoDayOfWeek.Friday )]
    [InlineData( IsoDayOfWeek.Saturday )]
    [InlineData( IsoDayOfWeek.Sunday )]
    public void CreateLocal_WithWeekOfYear_ShouldReturnCorrectWeekInLocal(IsoDayOfWeek weekStart)
    {
        var year = Fixture.Create<DateTime>().Year;
        var weekOfYear = Fixture.CreatePositiveInt32() % 50 + 1;
        var expected = ZonedWeek.Create( year, weekOfYear, TimeZoneInfo.Local, weekStart );

        var result = ZonedWeek.CreateLocal( year, weekOfYear, weekStart );

        result.Should().BeEquivalentTo( expected );
    }

    [Theory]
    [InlineData( IsoDayOfWeek.Monday )]
    [InlineData( IsoDayOfWeek.Tuesday )]
    [InlineData( IsoDayOfWeek.Wednesday )]
    [InlineData( IsoDayOfWeek.Thursday )]
    [InlineData( IsoDayOfWeek.Friday )]
    [InlineData( IsoDayOfWeek.Saturday )]
    [InlineData( IsoDayOfWeek.Sunday )]
    public void TryCreateLocal_WithWeekOfYear_ShouldReturnCorrectWeekInLocal(IsoDayOfWeek weekStart)
    {
        var year = Fixture.Create<DateTime>().Year;
        var weekOfYear = Fixture.CreatePositiveInt32() % 50 + 1;
        var expected = ZonedWeek.Create( year, weekOfYear, TimeZoneInfo.Local, weekStart );

        var result = ZonedWeek.TryCreateLocal( year, weekOfYear, weekStart );

        result.Should().BeEquivalentTo( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedWeekTestsData.GetToStringData ) )]
    public void ToString_ShouldReturnCorrectResult(
        int year,
        int weekOfYear,
        TimeZoneInfo timeZone,
        IsoDayOfWeek weekStart,
        string expected)
    {
        var sut = ZonedWeek.Create( year, weekOfYear, timeZone, weekStart );
        var result = sut.ToString();
        result.Should().Be( expected );
    }

    [Fact]
    public void GetHashCode_ShouldReturnCorrectResult()
    {
        var dateTime = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var sut = ZonedWeek.Create( dateTime, timeZone );
        var expected = Hash.Default.Add( sut.Start.Timestamp ).Add( sut.End.Timestamp ).Add( sut.TimeZone.Id ).Value;

        var result = sut.GetHashCode();

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedWeekTestsData.GetEqualsData ) )]
    public void Equals_ShouldReturnCorrectResult(
        DateTime dt1,
        TimeZoneInfo tz1,
        IsoDayOfWeek ws1,
        DateTime dt2,
        TimeZoneInfo tz2,
        IsoDayOfWeek ws2,
        bool expected)
    {
        var a = ZonedWeek.Create( dt1, tz1, ws1 );
        var b = ZonedWeek.Create( dt2, tz2, ws2 );

        var result = a.Equals( b );

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedWeekTestsData.GetCompareToData ) )]
    public void CompareTo_ShouldReturnCorrectResult(
        DateTime dt1,
        TimeZoneInfo tz1,
        IsoDayOfWeek ws1,
        DateTime dt2,
        TimeZoneInfo tz2,
        IsoDayOfWeek ws2,
        int expectedSign)
    {
        var a = ZonedWeek.Create( dt1, tz1, ws1 );
        var b = ZonedWeek.Create( dt2, tz2, ws2 );

        var result = a.CompareTo( b );

        Math.Sign( result ).Should().Be( expectedSign );
    }

    [Fact]
    public void ToTimeZone_ShouldReturnCorrectResult()
    {
        var dateTime = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var targetTimeZone = TimeZoneFactory.CreateRandom( Fixture );

        var sut = ZonedWeek.Create( dateTime, timeZone );
        var expected = ZonedWeek.Create( dateTime, targetTimeZone );

        var result = sut.ToTimeZone( targetTimeZone );

        result.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void ToUtcTimeZone_ShouldReturnCorrectResult()
    {
        var dateTime = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );

        var sut = ZonedWeek.Create( dateTime, timeZone );
        var expected = ZonedWeek.Create( dateTime, TimeZoneInfo.Utc );

        var result = sut.ToUtcTimeZone();

        result.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void ToLocalTimeZone_ShouldReturnCorrectResult()
    {
        var dateTime = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );

        var sut = ZonedWeek.Create( dateTime, timeZone );
        var expected = ZonedWeek.Create( dateTime, TimeZoneInfo.Local );

        var result = sut.ToLocalTimeZone();

        result.Should().BeEquivalentTo( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedWeekTestsData.GetContainsData ) )]
    public void Contains_WithZonedDateTime_ShouldReturnCorrectResult(
        DateTime week,
        TimeZoneInfo weekTimeZone,
        DateTime dateTimeValue,
        TimeZoneInfo timeZone,
        bool expected)
    {
        var dateTime = ZonedDateTime.Create( dateTimeValue, timeZone );
        var sut = ZonedWeek.Create( week, weekTimeZone, week.GetDayOfWeek() );

        var result = sut.Contains( dateTime );

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedWeekTestsData.GetContainsWithAmbiguousStartOrEndData ) )]
    public void Contains_WithZonedDateTimeAndAmbiguousStartOrEnd_ShouldReturnCorrectResult(
        DateTime week,
        TimeZoneInfo timeZone,
        DateTime dateTimeValue,
        bool forceInDaylightSavingMode,
        bool expected)
    {
        var dateTime = ZonedDateTime.Create( dateTimeValue, timeZone );
        if ( forceInDaylightSavingMode )
            dateTime = dateTime.GetOppositeAmbiguousDateTime() ?? dateTime;

        var sut = ZonedWeek.Create( week, timeZone, week.GetDayOfWeek() );

        var result = sut.Contains( dateTime );

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedWeekTestsData.GetContainsWithZonedDayData ) )]
    public void Contains_WithZonedDay_ShouldReturnCorrectResult(
        DateTime week,
        TimeZoneInfo weekTimeZone,
        DateTime dayValue,
        TimeZoneInfo timeZone,
        bool expected)
    {
        var day = ZonedDay.Create( dayValue, timeZone );
        var sut = ZonedWeek.Create( week, weekTimeZone, week.GetDayOfWeek() );

        var result = sut.Contains( day );

        result.Should().Be( expected );
    }

    [Fact]
    public void GetNext_ShouldReturnTheNextMonth()
    {
        var dateTime = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );

        var sut = ZonedWeek.Create( dateTime, timeZone );
        var expected = sut.AddWeeks( 1 );

        var result = sut.GetNext();

        result.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void GetPrevious_ShouldReturnThePreviousMonth()
    {
        var dateTime = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );

        var sut = ZonedWeek.Create( dateTime, timeZone );
        var expected = sut.AddWeeks( -1 );

        var result = sut.GetPrevious();

        result.Should().BeEquivalentTo( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedWeekTestsData.GetAddWeeksData ) )]
    public void AddWeeks_ShouldReturnCorrectResult(DateTime week, TimeZoneInfo timeZone, int weeksToAdd, DateTime expectedWeek)
    {
        var sut = ZonedWeek.Create( week, timeZone, week.GetDayOfWeek() );
        var expected = ZonedWeek.Create( expectedWeek, timeZone, expectedWeek.GetDayOfWeek() );

        var result = sut.AddWeeks( weeksToAdd );

        result.Should().BeEquivalentTo( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedWeekTestsData.GetAddData ) )]
    public void Add_ShouldReturnCorrectResult(
        DateTime week,
        TimeZoneInfo timeZone,
        Period period,
        DateTime expectedWeek)
    {
        var sut = ZonedWeek.Create( week, timeZone, week.GetDayOfWeek() );
        var expected = ZonedWeek.Create( expectedWeek, timeZone, expectedWeek.GetDayOfWeek() );

        var result = sut.Add( period );

        result.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void SubtractWeeks_ShouldBeEquivalentToAddWeeksWithNegatedWeekCount()
    {
        var monthCount = Fixture.Create<sbyte>();
        var dateTime = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );

        var sut = ZonedWeek.Create( dateTime, timeZone );
        var expected = sut.AddWeeks( -monthCount );

        var result = sut.SubtractWeeks( monthCount );

        result.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void Subtract_ShouldBeEquivalentToAddWithNegatedPeriod()
    {
        var periodToSubtract = new Period(
            years: Fixture.Create<sbyte>(),
            months: Fixture.Create<sbyte>(),
            weeks: Fixture.Create<short>(),
            days: Fixture.Create<short>(),
            hours: Fixture.Create<short>(),
            minutes: Fixture.Create<short>(),
            seconds: Fixture.Create<short>(),
            milliseconds: Fixture.Create<short>(),
            microseconds: Fixture.Create<short>(),
            ticks: Fixture.Create<short>() );

        var dateTime = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );

        var sut = ZonedWeek.Create( dateTime, timeZone );
        var expected = sut.Add( -periodToSubtract );

        var result = sut.Subtract( periodToSubtract );

        result.Should().BeEquivalentTo( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedWeekTestsData.GetGetPeriodOffsetData ) )]
    public void GetPeriodOffset_ShouldReturnCorrectResult(
        DateTime week,
        TimeZoneInfo timeZone,
        DateTime otherWeek,
        TimeZoneInfo otherTimeZone,
        PeriodUnits units)
    {
        var sut = ZonedWeek.Create( week, timeZone, week.GetDayOfWeek() );
        var other = ZonedWeek.Create( otherWeek, otherTimeZone, otherWeek.GetDayOfWeek() );
        var expected = sut.Start.GetPeriodOffset( other.Start, units );

        var result = sut.GetPeriodOffset( other, units );

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedWeekTestsData.GetGetGreedyPeriodOffsetData ) )]
    public void GetGreedyPeriodOffset_ShouldReturnCorrectResult(
        DateTime week,
        TimeZoneInfo timeZone,
        DateTime otherWeek,
        TimeZoneInfo otherTimeZone,
        PeriodUnits units)
    {
        var sut = ZonedWeek.Create( week, timeZone, week.GetDayOfWeek() );
        var other = ZonedWeek.Create( otherWeek, otherTimeZone, otherWeek.GetDayOfWeek() );
        var expected = sut.Start.GetGreedyPeriodOffset( other.Start, units );

        var result = sut.GetGreedyPeriodOffset( other, units );

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedWeekTestsData.GetSetYearData ) )]
    public void SetYear_ShouldReturnCorrectResult(
        int year,
        int weekOfYear,
        TimeZoneInfo timeZone,
        IsoDayOfWeek weekStart,
        int newYear,
        int expectedNewWeekOfYear)
    {
        var sut = ZonedWeek.Create( year, weekOfYear, timeZone, weekStart );
        var expected = ZonedWeek.Create( newYear, expectedNewWeekOfYear, timeZone, weekStart );

        var result = sut.SetYear( newYear );

        result.Should().BeEquivalentTo( expected );
    }

    [Theory]
    [InlineData( IsoDayOfWeek.Monday )]
    [InlineData( IsoDayOfWeek.Tuesday )]
    [InlineData( IsoDayOfWeek.Wednesday )]
    [InlineData( IsoDayOfWeek.Thursday )]
    [InlineData( IsoDayOfWeek.Friday )]
    [InlineData( IsoDayOfWeek.Saturday )]
    [InlineData( IsoDayOfWeek.Sunday )]
    public void SetWeekOfYear_ShouldBeEquivalentToCreateWithCurrentYearAndNewWeekOfYear(IsoDayOfWeek weekStart)
    {
        var year = Fixture.Create<DateTime>().Year;
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var oldWeekOfYear = Fixture.CreatePositiveInt32() % 50 + 1;
        var newWeekOfYear = Fixture.CreatePositiveInt32() % 50 + 1;
        var sut = ZonedWeek.Create( year, oldWeekOfYear, timeZone, weekStart );
        var expected = ZonedWeek.Create( year, newWeekOfYear, timeZone, weekStart );

        var result = sut.SetWeekOfYear( newWeekOfYear );

        result.Should().BeEquivalentTo( expected );
    }

    [Theory]
    [InlineData( IsoDayOfWeek.Monday )]
    [InlineData( IsoDayOfWeek.Tuesday )]
    [InlineData( IsoDayOfWeek.Wednesday )]
    [InlineData( IsoDayOfWeek.Thursday )]
    [InlineData( IsoDayOfWeek.Friday )]
    [InlineData( IsoDayOfWeek.Saturday )]
    [InlineData( IsoDayOfWeek.Sunday )]
    public void TrySetWeekOfYear_ShouldBeEquivalentToTryCreateWithCurrentYearAndNewWeekOfYear(IsoDayOfWeek weekStart)
    {
        var year = Fixture.Create<DateTime>().Year;
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var oldWeekOfYear = Fixture.CreatePositiveInt32() % 50 + 1;
        var newWeekOfYear = Fixture.CreatePositiveInt32() % 50 + 1;
        var sut = ZonedWeek.Create( year, oldWeekOfYear, timeZone, weekStart );
        var expected = ZonedWeek.Create( year, newWeekOfYear, timeZone, weekStart );

        var result = sut.TrySetWeekOfYear( newWeekOfYear );

        result.Should().BeEquivalentTo( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedWeekTestsData.GetSetWeekStartData ) )]
    public void SetWeekStart_ShouldReturnCorrectResult(
        int year,
        int weekOfYear,
        IsoDayOfWeek weekStart,
        TimeZoneInfo timeZone,
        IsoDayOfWeek newWeekStart)
    {
        var sut = ZonedWeek.Create( year, weekOfYear, timeZone, weekStart );
        var expected = ZonedWeek.Create( year, weekOfYear, timeZone, newWeekStart );

        var result = sut.SetWeekStart( newWeekStart );

        result.Should().BeEquivalentTo( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedWeekTestsData.GetSetWeekStartThrowData ) )]
    public void SetWeekStart_ShouldThrowArgumentOutOfRangeException_WhenNewWeekStartExceedsWeekCountInYear(
        int year,
        int weekOfYear,
        IsoDayOfWeek weekStart,
        TimeZoneInfo timeZone,
        IsoDayOfWeek newWeekStart)
    {
        var sut = ZonedWeek.Create( year, weekOfYear, timeZone, weekStart );
        var action = Lambda.Of( () => sut.SetWeekStart( newWeekStart ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [MethodData( nameof( ZonedWeekTestsData.GetSetWeekStartData ) )]
    public void TrySetWeekStart_ShouldReturnCorrectResult(
        int year,
        int weekOfYear,
        IsoDayOfWeek weekStart,
        TimeZoneInfo timeZone,
        IsoDayOfWeek newWeekStart)
    {
        var sut = ZonedWeek.Create( year, weekOfYear, timeZone, weekStart );
        var expected = ZonedWeek.Create( year, weekOfYear, timeZone, newWeekStart );

        var result = sut.TrySetWeekStart( newWeekStart );

        result.Should().BeEquivalentTo( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedWeekTestsData.GetSetWeekStartThrowData ) )]
    public void TrySetWeekStart_ShouldReturnNull_WhenNewWeekStartExceedsWeekCountInYear(
        int year,
        int weekOfYear,
        IsoDayOfWeek weekStart,
        TimeZoneInfo timeZone,
        IsoDayOfWeek newWeekStart)
    {
        var sut = ZonedWeek.Create( year, weekOfYear, timeZone, weekStart );
        var result = sut.TrySetWeekStart( newWeekStart );
        result.Should().BeNull();
    }

    [Theory]
    [MethodData( nameof( ZonedWeekTestsData.GetGetDayOfWeekData ) )]
    public void GetDayOfWeek_ShouldReturnCorrectResult(DateTime week, TimeZoneInfo timeZone, IsoDayOfWeek day, DateTime expectedDay)
    {
        var sut = ZonedWeek.Create( week, timeZone, week.GetDayOfWeek() );
        var expected = ZonedDay.Create( expectedDay, timeZone );

        var result = sut.GetDayOfWeek( day );

        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( 8 )]
    public void GetDayOfWeek_ShouldThrowArgumentOutOfRangeException_WhenDayIsInvalid(int day)
    {
        var week = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var sut = ZonedWeek.Create( week, timeZone, week.GetDayOfWeek() );
        var action = Lambda.Of( () => sut.GetDayOfWeek( ( IsoDayOfWeek )day ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void GetMonday_ShouldReturnCorrectResult()
    {
        var week = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var sut = ZonedWeek.Create( week, timeZone, week.GetDayOfWeek() );
        var expected = sut.GetDayOfWeek( IsoDayOfWeek.Monday );

        var result = sut.GetMonday();

        result.Should().Be( expected );
    }

    [Fact]
    public void GetTuesday_ShouldReturnCorrectResult()
    {
        var week = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var sut = ZonedWeek.Create( week, timeZone, week.GetDayOfWeek() );
        var expected = sut.GetDayOfWeek( IsoDayOfWeek.Tuesday );

        var result = sut.GetTuesday();

        result.Should().Be( expected );
    }

    [Fact]
    public void GetWednesday_ShouldReturnCorrectResult()
    {
        var week = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var sut = ZonedWeek.Create( week, timeZone, week.GetDayOfWeek() );
        var expected = sut.GetDayOfWeek( IsoDayOfWeek.Wednesday );

        var result = sut.GetWednesday();

        result.Should().Be( expected );
    }

    [Fact]
    public void GetThursday_ShouldReturnCorrectResult()
    {
        var week = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var sut = ZonedWeek.Create( week, timeZone, week.GetDayOfWeek() );
        var expected = sut.GetDayOfWeek( IsoDayOfWeek.Thursday );

        var result = sut.GetThursday();

        result.Should().Be( expected );
    }

    [Fact]
    public void GetFriday_ShouldReturnCorrectResult()
    {
        var week = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var sut = ZonedWeek.Create( week, timeZone, week.GetDayOfWeek() );
        var expected = sut.GetDayOfWeek( IsoDayOfWeek.Friday );

        var result = sut.GetFriday();

        result.Should().Be( expected );
    }

    [Fact]
    public void GetSaturday_ShouldReturnCorrectResult()
    {
        var week = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var sut = ZonedWeek.Create( week, timeZone, week.GetDayOfWeek() );
        var expected = sut.GetDayOfWeek( IsoDayOfWeek.Saturday );

        var result = sut.GetSaturday();

        result.Should().Be( expected );
    }

    [Fact]
    public void GetSunday_ShouldReturnCorrectResult()
    {
        var week = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var sut = ZonedWeek.Create( week, timeZone, week.GetDayOfWeek() );
        var expected = sut.GetDayOfWeek( IsoDayOfWeek.Sunday );

        var result = sut.GetSunday();

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedWeekTestsData.GetGetYearData ) )]
    public void GetYear_ShouldReturnCorrectResult(DateTime week, TimeZoneInfo timeZone, int expectedYear)
    {
        var sut = ZonedWeek.Create( week, timeZone, week.GetDayOfWeek() );
        var expected = ZonedYear.Create( expectedYear, timeZone );

        var result = sut.GetYear();

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedWeekTestsData.GetGetAllDaysData ) )]
    public void GetAllDays_ShouldReturnCorrectDayCollection(DateTime week, TimeZoneInfo timeZone, IReadOnlyList<DateTime> expectedDays)
    {
        var sut = ZonedWeek.Create( week, timeZone, week.GetDayOfWeek() );
        var expected = expectedDays.Select( d => ZonedDay.Create( d, timeZone ) ).ToList();

        var result = sut.GetAllDays().ToList();

        result.Should().BeSequentiallyEqualTo( expected );
    }

    [Theory]
    [InlineData( IsoDayOfWeek.Monday )]
    [InlineData( IsoDayOfWeek.Tuesday )]
    [InlineData( IsoDayOfWeek.Wednesday )]
    [InlineData( IsoDayOfWeek.Thursday )]
    [InlineData( IsoDayOfWeek.Friday )]
    [InlineData( IsoDayOfWeek.Saturday )]
    [InlineData( IsoDayOfWeek.Sunday )]
    public void ToBounds_ShouldReturnBoundsFromStartToEnd(IsoDayOfWeek weekStart)
    {
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var dateTime = Fixture.Create<DateTime>();
        var sut = ZonedWeek.Create( dateTime, timeZone, weekStart );

        var result = sut.ToBounds();

        result.Should().Be( Bounds.Create( sut.Start, sut.End ) );
    }

    [Fact]
    public void ToCheckedBounds_ShouldReturnCorrectRangeWithOneElement_WhenStartAndEndAreUnambiguous()
    {
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var dateTime = Fixture.Create<DateTime>();
        var sut = ZonedWeek.Create( dateTime, timeZone, dateTime.DayOfWeek.ToIso() );
        var expected = BoundsRange.Create( Bounds.Create( sut.Start, sut.End ) );

        var result = sut.ToCheckedBounds();

        result.Should().BeSequentiallyEqualTo( expected );
    }

    [Fact]
    public void ToCheckedBounds_ShouldReturnCorrectRangeWithTwoElements_WhenStartIsAmbiguous()
    {
        var dateTime = new DateTime( 2021, 8, 26 );
        var timeZone = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 4, 26, 0, 40, 0 ),
                transitionEnd: new DateTime( 1, 8, 26, 0, 40, 0 ) ) );

        var sut = ZonedWeek.Create( dateTime, timeZone, dateTime.DayOfWeek.ToIso() );
        var expected = BoundsRange.Create(
            new[]
            {
                Bounds.Create( sut.Start, sut.Start.Add( new Duration( 0, 39, 59, 999, 999, 9 ) ) ),
                Bounds.Create( sut.Start.Add( Duration.FromHours( 1 ) ), sut.End )
            } );

        var result = sut.ToCheckedBounds();

        result.Should().BeSequentiallyEqualTo( expected );
    }

    [Fact]
    public void ToCheckedBounds_ShouldReturnCorrectRangeWithTwoElements_WhenEndIsAmbiguous()
    {
        var dateTime = new DateTime( 2021, 8, 26 );
        var timeZone = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 4, 26, 0, 40, 0 ),
                transitionEnd: new DateTime( 1, 9, 2, 0, 40, 0 ) ) );

        var sut = ZonedWeek.Create( dateTime, timeZone, dateTime.DayOfWeek.ToIso() );
        var expected = BoundsRange.Create(
            new[]
            {
                Bounds.Create( sut.Start, sut.End.Subtract( Duration.FromHours( 1 ) ) ),
                Bounds.Create( sut.End.Subtract( new Duration( 0, 19, 59, 999, 999, 9 ) ), sut.End )
            } );

        var result = sut.ToCheckedBounds();

        result.Should().BeSequentiallyEqualTo( expected );
    }

    [Fact]
    public void ToCheckedBounds_ShouldReturnCorrectRangeWithThreeElements_WhenStartAndEndAreAmbiguous()
    {
        var dateTime = new DateTime( 2021, 8, 26 );
        var timeZone = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateRule(
                start: DateTime.MinValue,
                end: new DateTime( 2021, 8, 27 ),
                transitionStart: new DateTime( 1, 4, 26, 0, 40, 0 ),
                transitionEnd: new DateTime( 1, 8, 26, 0, 40, 0 ) ),
            TimeZoneFactory.CreateRule(
                start: new DateTime( 2021, 8, 28 ),
                end: DateTime.MaxValue,
                transitionStart: new DateTime( 1, 8, 29, 0, 40, 0 ),
                transitionEnd: new DateTime( 1, 9, 2, 0, 40, 0 ) ) );

        var sut = ZonedWeek.Create( dateTime, timeZone, dateTime.DayOfWeek.ToIso() );
        var expected = BoundsRange.Create(
            new[]
            {
                Bounds.Create( sut.Start, sut.Start.Add( new Duration( 0, 39, 59, 999, 999, 9 ) ) ),
                Bounds.Create( sut.Start.Add( Duration.FromHours( 1 ) ), sut.End.Subtract( Duration.FromHours( 1 ) ) ),
                Bounds.Create( sut.End.Subtract( new Duration( 0, 19, 59, 999, 999, 9 ) ), sut.End )
            } );

        var result = sut.ToCheckedBounds();

        result.Should().BeSequentiallyEqualTo( expected );
    }

    [Fact]
    public void AddOperator_ShouldBeEquivalentToAdd()
    {
        var periodToSubtract = new Period(
            years: Fixture.Create<sbyte>(),
            months: Fixture.Create<sbyte>(),
            weeks: Fixture.Create<short>(),
            days: Fixture.Create<short>(),
            hours: Fixture.Create<short>(),
            minutes: Fixture.Create<short>(),
            seconds: Fixture.Create<short>(),
            milliseconds: Fixture.Create<short>(),
            microseconds: Fixture.Create<short>(),
            ticks: Fixture.Create<short>() );

        var dateTime = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );

        var sut = ZonedWeek.Create( dateTime, timeZone );
        var expected = sut.Add( periodToSubtract );

        var result = sut + periodToSubtract;

        result.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void SubtractOperator_ShouldBeEquivalentToAddWithNegatedPeriod()
    {
        var periodToSubtract = new Period(
            years: Fixture.Create<sbyte>(),
            months: Fixture.Create<sbyte>(),
            weeks: Fixture.Create<short>(),
            days: Fixture.Create<short>(),
            hours: Fixture.Create<short>(),
            minutes: Fixture.Create<short>(),
            seconds: Fixture.Create<short>(),
            milliseconds: Fixture.Create<short>(),
            microseconds: Fixture.Create<short>(),
            ticks: Fixture.Create<short>() );

        var dateTime = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );

        var sut = ZonedWeek.Create( dateTime, timeZone );
        var expected = sut.Add( -periodToSubtract );

        var result = sut - periodToSubtract;

        result.Should().BeEquivalentTo( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedWeekTestsData.GetEqualsData ) )]
    public void EqualityOperator_ShouldReturnCorrectResult(
        DateTime dt1,
        TimeZoneInfo tz1,
        IsoDayOfWeek ws1,
        DateTime dt2,
        TimeZoneInfo tz2,
        IsoDayOfWeek ws2,
        bool expected)
    {
        var a = ZonedWeek.Create( dt1, tz1, ws1 );
        var b = ZonedWeek.Create( dt2, tz2, ws2 );

        var result = a == b;

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedWeekTestsData.GetNotEqualsData ) )]
    public void InequalityOperator_ShouldReturnCorrectResult(
        DateTime dt1,
        TimeZoneInfo tz1,
        IsoDayOfWeek ws1,
        DateTime dt2,
        TimeZoneInfo tz2,
        IsoDayOfWeek ws2,
        bool expected)
    {
        var a = ZonedWeek.Create( dt1, tz1, ws1 );
        var b = ZonedWeek.Create( dt2, tz2, ws2 );

        var result = a != b;

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedWeekTestsData.GetGreaterThanComparisonData ) )]
    public void GreaterThanOperator_ShouldReturnCorrectResult(
        DateTime dt1,
        TimeZoneInfo tz1,
        IsoDayOfWeek ws1,
        DateTime dt2,
        TimeZoneInfo tz2,
        IsoDayOfWeek ws2,
        bool expected)
    {
        var a = ZonedWeek.Create( dt1, tz1, ws1 );
        var b = ZonedWeek.Create( dt2, tz2, ws2 );

        var result = a > b;

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedWeekTestsData.GetLessThanOrEqualToComparisonData ) )]
    public void LessThanOrEqualToOperator_ShouldReturnCorrectResult(
        DateTime dt1,
        TimeZoneInfo tz1,
        IsoDayOfWeek ws1,
        DateTime dt2,
        TimeZoneInfo tz2,
        IsoDayOfWeek ws2,
        bool expected)
    {
        var a = ZonedWeek.Create( dt1, tz1, ws1 );
        var b = ZonedWeek.Create( dt2, tz2, ws2 );

        var result = a <= b;

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedWeekTestsData.GetLessThanComparisonData ) )]
    public void LessThanOperator_ShouldReturnCorrectResult(
        DateTime dt1,
        TimeZoneInfo tz1,
        IsoDayOfWeek ws1,
        DateTime dt2,
        TimeZoneInfo tz2,
        IsoDayOfWeek ws2,
        bool expected)
    {
        var a = ZonedWeek.Create( dt1, tz1, ws1 );
        var b = ZonedWeek.Create( dt2, tz2, ws2 );

        var result = a < b;

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedWeekTestsData.GetGreaterThanOrEqualToComparisonData ) )]
    public void GreaterThanOrEqualToOperator_ShouldReturnCorrectResult(
        DateTime dt1,
        TimeZoneInfo tz1,
        IsoDayOfWeek ws1,
        DateTime dt2,
        TimeZoneInfo tz2,
        IsoDayOfWeek ws2,
        bool expected)
    {
        var a = ZonedWeek.Create( dt1, tz1, ws1 );
        var b = ZonedWeek.Create( dt2, tz2, ws2 );

        var result = a >= b;

        result.Should().Be( expected );
    }
}
