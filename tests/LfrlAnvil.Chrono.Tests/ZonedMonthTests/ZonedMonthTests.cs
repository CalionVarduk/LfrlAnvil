using System.Linq;
using LfrlAnvil.Chrono.Extensions;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions.Attributes;

namespace LfrlAnvil.Chrono.Tests.ZonedMonthTests;

[TestClass( typeof( ZonedMonthTestsData ) )]
public class ZonedMonthTests : TestsBase
{
    [Fact]
    public void Default_ShouldReturnStartOfUnixEpochMonthInUtcTimeZone()
    {
        var result = default( ZonedMonth );
        var expectedStart = ZonedDateTime.CreateUtc( DateTime.UnixEpoch );
        var expectedEnd = ZonedDateTime.CreateUtc( DateTime.UnixEpoch.GetEndOfMonth() );

        Assertion.All(
                result.Start.TestEquals( expectedStart ),
                result.End.TestEquals( expectedEnd ),
                result.Year.TestEquals( result.Start.Year ),
                result.Month.TestEquals( result.Start.Month ),
                result.DayCount.TestEquals( 31 ),
                result.TimeZone.TestEquals( TimeZoneInfo.Utc ),
                result.Duration.TestEquals( Duration.FromHours( 744 ) ),
                result.IsLocal.TestFalse(),
                result.IsUtc.TestTrue() )
            .Go();
    }

    [Theory]
    [MethodData( nameof( ZonedMonthTestsData.GetCreateData ) )]
    public void Create_ShouldReturnCorrectResult(DateTime dateTime, TimeZoneInfo timeZone, int expectedDayCount)
    {
        var expectedStart = ZonedDateTime.Create( dateTime.GetStartOfMonth(), timeZone );
        var expectedEnd = ZonedDateTime.Create( dateTime.GetEndOfMonth(), timeZone );
        var expectedDuration = Duration.FromHours( 24 * expectedDayCount );
        var sut = ZonedMonth.Create( dateTime, timeZone );

        Assertion.All(
                sut.Start.TestEquals( expectedStart ),
                sut.End.TestEquals( expectedEnd ),
                sut.Year.TestEquals( sut.Start.Year ),
                sut.Month.TestEquals( sut.Start.Month ),
                sut.DayCount.TestEquals( expectedDayCount ),
                sut.TimeZone.TestEquals( timeZone ),
                sut.Duration.TestEquals( expectedDuration ),
                sut.IsLocal.TestFalse(),
                sut.IsUtc.TestFalse() )
            .Go();
    }

    [Theory]
    [MethodData( nameof( ZonedMonthTestsData.GetCreateWithContainedInvalidityRangeData ) )]
    public void Create_WithContainedInvalidityRange_ShouldReturnCorrectResult(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        int expectedDayCount,
        Duration expectedDuration)
    {
        var expectedStart = ZonedDateTime.Create( dateTime.GetStartOfMonth(), timeZone );
        var expectedEnd = ZonedDateTime.Create( dateTime.GetEndOfMonth(), timeZone );

        var sut = ZonedMonth.Create( dateTime, timeZone );

        Assertion.All(
                sut.Start.TestEquals( expectedStart ),
                sut.End.TestEquals( expectedEnd ),
                sut.Year.TestEquals( sut.Start.Year ),
                sut.Month.TestEquals( sut.Start.Month ),
                sut.DayCount.TestEquals( expectedDayCount ),
                sut.TimeZone.TestEquals( timeZone ),
                sut.Duration.TestEquals( expectedDuration ),
                sut.IsLocal.TestFalse(),
                sut.IsUtc.TestFalse() )
            .Go();
    }

    [Theory]
    [MethodData( nameof( ZonedMonthTestsData.GetCreateWithContainedAmbiguityRangeData ) )]
    public void Create_WithContainedAmbiguityRange_ShouldReturnCorrectResult(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        int expectedDayCount,
        Duration expectedDuration)
    {
        var expectedStart = ZonedDateTime.Create( dateTime.GetStartOfMonth(), timeZone );
        var expectedEnd = ZonedDateTime.Create( dateTime.GetEndOfMonth(), timeZone );

        var sut = ZonedMonth.Create( dateTime, timeZone );

        Assertion.All(
                sut.Start.TestEquals( expectedStart ),
                sut.End.TestEquals( expectedEnd ),
                sut.Year.TestEquals( sut.Start.Year ),
                sut.Month.TestEquals( sut.Start.Month ),
                sut.DayCount.TestEquals( expectedDayCount ),
                sut.TimeZone.TestEquals( timeZone ),
                sut.Duration.TestEquals( expectedDuration ),
                sut.IsLocal.TestFalse(),
                sut.IsUtc.TestFalse() )
            .Go();
    }

    [Theory]
    [MethodData( nameof( ZonedMonthTestsData.GetCreateWithInvalidStartTimeData ) )]
    public void Create_WithInvalidStartTime_ShouldReturnResultWithEarliestPossibleStart(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        DateTime expectedStartValue,
        int expectedDayCount,
        Duration expectedDuration)
    {
        var expectedStart = ZonedDateTime.Create( expectedStartValue, timeZone );
        var expectedEnd = ZonedDateTime.Create( dateTime.GetEndOfMonth(), timeZone );

        var sut = ZonedMonth.Create( dateTime, timeZone );

        Assertion.All(
                sut.Start.TestEquals( expectedStart ),
                sut.End.TestEquals( expectedEnd ),
                sut.Year.TestEquals( sut.Start.Year ),
                sut.Month.TestEquals( sut.Start.Month ),
                sut.DayCount.TestEquals( expectedDayCount ),
                sut.TimeZone.TestEquals( timeZone ),
                sut.Duration.TestEquals( expectedDuration ),
                sut.IsLocal.TestFalse(),
                sut.IsUtc.TestFalse() )
            .Go();
    }

    [Theory]
    [MethodData( nameof( ZonedMonthTestsData.GetCreateWithInvalidEndTimeData ) )]
    public void Create_WithInvalidEndTime_ShouldReturnResultWithLatestPossibleEnd(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        DateTime expectedEndValue,
        int expectedDayCount,
        Duration expectedDuration)
    {
        var expectedStart = ZonedDateTime.Create( dateTime.GetStartOfMonth(), timeZone );
        var expectedEnd = ZonedDateTime.Create( expectedEndValue, timeZone );

        var sut = ZonedMonth.Create( dateTime, timeZone );

        Assertion.All(
                sut.Start.TestEquals( expectedStart ),
                sut.End.TestEquals( expectedEnd ),
                sut.Year.TestEquals( sut.Start.Year ),
                sut.Month.TestEquals( sut.Start.Month ),
                sut.DayCount.TestEquals( expectedDayCount ),
                sut.TimeZone.TestEquals( timeZone ),
                sut.Duration.TestEquals( expectedDuration ),
                sut.IsLocal.TestFalse(),
                sut.IsUtc.TestFalse() )
            .Go();
    }

    [Theory]
    [MethodData( nameof( ZonedMonthTestsData.GetCreateWithAmbiguousStartTimeData ) )]
    public void Create_WithAmbiguousStartTime_ShouldReturnResultWithEarliestPossibleStart(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        bool forceInDaylightSavingMode,
        int expectedDayCount,
        Duration expectedDuration)
    {
        var expectedStart = ZonedDateTime.Create( dateTime.GetStartOfMonth(), timeZone );
        if ( forceInDaylightSavingMode )
            expectedStart = expectedStart.GetOppositeAmbiguousDateTime() ?? expectedStart;

        var expectedEnd = ZonedDateTime.Create( dateTime.GetEndOfMonth(), timeZone );

        var sut = ZonedMonth.Create( dateTime, timeZone );

        Assertion.All(
                sut.Start.TestEquals( expectedStart ),
                sut.End.TestEquals( expectedEnd ),
                sut.Year.TestEquals( sut.Start.Year ),
                sut.Month.TestEquals( sut.Start.Month ),
                sut.DayCount.TestEquals( expectedDayCount ),
                sut.TimeZone.TestEquals( timeZone ),
                sut.Duration.TestEquals( expectedDuration ),
                sut.IsLocal.TestFalse(),
                sut.IsUtc.TestFalse() )
            .Go();
    }

    [Theory]
    [MethodData( nameof( ZonedMonthTestsData.GetCreateWithAmbiguousEndTimeData ) )]
    public void Create_WithAmbiguousEndTime_ShouldReturnResultWithLatestPossibleEnd(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        bool forceInDaylightSavingMode,
        int expectedDayCount,
        Duration expectedDuration)
    {
        var expectedStart = ZonedDateTime.Create( dateTime.GetStartOfMonth(), timeZone );
        var expectedEnd = ZonedDateTime.Create( dateTime.GetEndOfMonth(), timeZone );
        if ( forceInDaylightSavingMode )
            expectedEnd = expectedEnd.GetOppositeAmbiguousDateTime() ?? expectedEnd;

        var sut = ZonedMonth.Create( dateTime, timeZone );

        Assertion.All(
                sut.Start.TestEquals( expectedStart ),
                sut.End.TestEquals( expectedEnd ),
                sut.Year.TestEquals( sut.Start.Year ),
                sut.Month.TestEquals( sut.Start.Month ),
                sut.DayCount.TestEquals( expectedDayCount ),
                sut.TimeZone.TestEquals( timeZone ),
                sut.Duration.TestEquals( expectedDuration ),
                sut.IsLocal.TestFalse(),
                sut.IsUtc.TestFalse() )
            .Go();
    }

    [Theory]
    [MethodData( nameof( ZonedMonthTestsData.GetCreateWithInvalidStartTimeAndAmbiguousEndTimeData ) )]
    public void Create_WithInvalidStartTimeAndAmbiguousEndTime_ShouldReturnResultWithEarliestPossibleStart_AndLatestPossibleEnd(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        DateTime expectedStartValue,
        bool forceInDaylightSavingMode,
        int expectedDayCount,
        Duration expectedDuration)
    {
        var expectedStart = ZonedDateTime.Create( expectedStartValue, timeZone );
        var expectedEnd = ZonedDateTime.Create( dateTime.GetEndOfMonth(), timeZone );
        if ( forceInDaylightSavingMode )
            expectedEnd = expectedEnd.GetOppositeAmbiguousDateTime() ?? expectedEnd;

        var sut = ZonedMonth.Create( dateTime, timeZone );

        Assertion.All(
                sut.Start.TestEquals( expectedStart ),
                sut.End.TestEquals( expectedEnd ),
                sut.Year.TestEquals( sut.Start.Year ),
                sut.Month.TestEquals( sut.Start.Month ),
                sut.DayCount.TestEquals( expectedDayCount ),
                sut.TimeZone.TestEquals( timeZone ),
                sut.Duration.TestEquals( expectedDuration ),
                sut.IsLocal.TestFalse(),
                sut.IsUtc.TestFalse() )
            .Go();
    }

    [Theory]
    [MethodData( nameof( ZonedMonthTestsData.GetCreateWithAmbiguousStartTimeAndInvalidEndTimeData ) )]
    public void Create_WithAmbiguousStartTimeAndInvalidEndTime_ShouldReturnResultWithEarliestPossibleStart_AndLatestPossibleEnd(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        DateTime expectedEndValue,
        bool forceInDaylightSavingMode,
        int expectedDayCount,
        Duration expectedDuration)
    {
        var expectedStart = ZonedDateTime.Create( dateTime.GetStartOfMonth(), timeZone );
        if ( forceInDaylightSavingMode )
            expectedStart = expectedStart.GetOppositeAmbiguousDateTime() ?? expectedStart;

        var expectedEnd = ZonedDateTime.Create( expectedEndValue, timeZone );

        var sut = ZonedMonth.Create( dateTime, timeZone );

        Assertion.All(
                sut.Start.TestEquals( expectedStart ),
                sut.End.TestEquals( expectedEnd ),
                sut.Year.TestEquals( sut.Start.Year ),
                sut.Month.TestEquals( sut.Start.Month ),
                sut.DayCount.TestEquals( expectedDayCount ),
                sut.TimeZone.TestEquals( timeZone ),
                sut.Duration.TestEquals( expectedDuration ),
                sut.IsLocal.TestFalse(),
                sut.IsUtc.TestFalse() )
            .Go();
    }

    [Theory]
    [MethodData( nameof( ZonedMonthTestsData.GetCreateWithContainedInvalidityAndAmbiguityRangesData ) )]
    public void Create_WithContainedInvalidityAndAmbiguityRanges_ShouldReturnCorrectResult(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        int expectedDayCount,
        Duration expectedDuration)
    {
        var expectedStart = ZonedDateTime.Create( dateTime.GetStartOfMonth(), timeZone );
        var expectedEnd = ZonedDateTime.Create( dateTime.GetEndOfMonth(), timeZone );

        var sut = ZonedMonth.Create( dateTime, timeZone );

        Assertion.All(
                sut.Start.TestEquals( expectedStart ),
                sut.End.TestEquals( expectedEnd ),
                sut.Year.TestEquals( sut.Start.Year ),
                sut.Month.TestEquals( sut.Start.Month ),
                sut.DayCount.TestEquals( expectedDayCount ),
                sut.TimeZone.TestEquals( timeZone ),
                sut.Duration.TestEquals( expectedDuration ),
                sut.IsLocal.TestFalse(),
                sut.IsUtc.TestFalse() )
            .Go();
    }

    [Fact]
    public void Create_WithZonedDateTime_ShouldBeEquivalentToCreateWithDateTimeAndTimeZoneInfo()
    {
        var dateTime = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var source = ZonedDateTime.Create( dateTime, timeZone );
        var expected = ZonedMonth.Create( dateTime, timeZone );

        var result = ZonedMonth.Create( source );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void Create_WithZonedDay_ShouldBeEquivalentToCreateWithDateTimeAndTimeZoneInfo()
    {
        var dateTime = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var source = ZonedDay.Create( dateTime, timeZone );
        var expected = ZonedMonth.Create( dateTime, timeZone );

        var result = ZonedMonth.Create( source );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void Create_WithYearAndMonthAndTimeZone_ShouldBeEquivalentToCreateWithDateTimeAndTimeZoneInfo()
    {
        var dateTime = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var expected = ZonedMonth.Create( dateTime, timeZone );

        var result = ZonedMonth.Create( dateTime.Year, ( IsoMonthOfYear )dateTime.Month, timeZone );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void CreateUtc_WithTimestamp_ShouldReturnCorrectMonthInUtc()
    {
        var timestamp = new Timestamp( Fixture.Create<DateTime>() );
        var expectedStart = ZonedDateTime.CreateUtc( timestamp.UtcValue.GetStartOfMonth() );
        var expectedEnd = ZonedDateTime.CreateUtc( timestamp.UtcValue.GetEndOfMonth() );
        var expectedDayCount = DateTime.DaysInMonth( timestamp.UtcValue.Year, timestamp.UtcValue.Month );
        var expectedDuration = Duration.FromHours( 24 * expectedDayCount );

        var result = ZonedMonth.CreateUtc( timestamp );

        Assertion.All(
                result.Start.TestEquals( expectedStart ),
                result.End.TestEquals( expectedEnd ),
                result.Year.TestEquals( result.Start.Year ),
                result.Month.TestEquals( result.Start.Month ),
                result.DayCount.TestEquals( expectedDayCount ),
                result.TimeZone.TestEquals( TimeZoneInfo.Utc ),
                result.Duration.TestEquals( expectedDuration ),
                result.IsLocal.TestFalse(),
                result.IsUtc.TestTrue() )
            .Go();
    }

    [Fact]
    public void CreateUtc_WithDateTime_ShouldReturnCorrectMonthInUtc()
    {
        var dateTime = Fixture.Create<DateTime>();
        var expectedStart = ZonedDateTime.CreateUtc( dateTime.GetStartOfMonth() );
        var expectedEnd = ZonedDateTime.CreateUtc( dateTime.GetEndOfMonth() );
        var expectedDayCount = DateTime.DaysInMonth( dateTime.Year, dateTime.Month );
        var expectedDuration = Duration.FromHours( 24 * expectedDayCount );

        var result = ZonedMonth.CreateUtc( dateTime );

        Assertion.All(
                result.Start.TestEquals( expectedStart ),
                result.End.TestEquals( expectedEnd ),
                result.Year.TestEquals( result.Start.Year ),
                result.Month.TestEquals( result.Start.Month ),
                result.DayCount.TestEquals( expectedDayCount ),
                result.TimeZone.TestEquals( TimeZoneInfo.Utc ),
                result.Duration.TestEquals( expectedDuration ),
                result.IsLocal.TestFalse(),
                result.IsUtc.TestTrue() )
            .Go();
    }

    [Fact]
    public void CreateUtc_WithYearAndMonth_ShouldBeEquivalentToCreateUtcWithDateTime()
    {
        var dateTime = Fixture.Create<DateTime>();
        var expected = ZonedMonth.CreateUtc( dateTime );

        var result = ZonedMonth.CreateUtc( dateTime.Year, ( IsoMonthOfYear )dateTime.Month );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void CreateLocal_ShouldReturnCorrectMonthInLocal()
    {
        var dateTime = Fixture.Create<DateTime>();
        var expected = ZonedMonth.Create( dateTime, TimeZoneInfo.Local );

        var result = ZonedMonth.CreateLocal( dateTime );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void CreateLocal_WithYearAndMonth_ShouldBeEquivalentToCreateLocalWithDateTime()
    {
        var dateTime = Fixture.Create<DateTime>();
        var expected = ZonedMonth.CreateLocal( dateTime );

        var result = ZonedMonth.CreateLocal( dateTime.Year, ( IsoMonthOfYear )dateTime.Month );

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [MethodData( nameof( ZonedMonthTestsData.GetToStringData ) )]
    public void ToString_ShouldReturnCorrectResult(DateTime month, TimeZoneInfo timeZone, string expected)
    {
        var sut = ZonedMonth.Create( month, timeZone );
        var result = sut.ToString();
        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void GetHashCode_ShouldReturnCorrectResult()
    {
        var dateTime = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var sut = ZonedMonth.Create( dateTime, timeZone );
        var expected = Hash.Default.Add( sut.Start.Timestamp ).Add( sut.End.Timestamp ).Add( sut.TimeZone.Id ).Value;

        var result = sut.GetHashCode();

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [MethodData( nameof( ZonedMonthTestsData.GetEqualsData ) )]
    public void Equals_ShouldReturnCorrectResult(DateTime dt1, TimeZoneInfo tz1, DateTime dt2, TimeZoneInfo tz2, bool expected)
    {
        var a = ZonedMonth.Create( dt1, tz1 );
        var b = ZonedMonth.Create( dt2, tz2 );

        var result = a.Equals( b );

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [MethodData( nameof( ZonedMonthTestsData.GetCompareToData ) )]
    public void CompareTo_ShouldReturnCorrectResult(DateTime dt1, TimeZoneInfo tz1, DateTime dt2, TimeZoneInfo tz2, int expectedSign)
    {
        var a = ZonedMonth.Create( dt1, tz1 );
        var b = ZonedMonth.Create( dt2, tz2 );

        var result = a.CompareTo( b );

        Math.Sign( result ).TestEquals( expectedSign ).Go();
    }

    [Fact]
    public void ToTimeZone_ShouldReturnCorrectResult()
    {
        var dateTime = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var targetTimeZone = TimeZoneFactory.CreateRandom( Fixture );

        var sut = ZonedMonth.Create( dateTime, timeZone );
        var expected = ZonedMonth.Create( dateTime, targetTimeZone );

        var result = sut.ToTimeZone( targetTimeZone );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void ToUtcTimeZone_ShouldReturnCorrectResult()
    {
        var dateTime = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );

        var sut = ZonedMonth.Create( dateTime, timeZone );
        var expected = ZonedMonth.Create( dateTime, TimeZoneInfo.Utc );

        var result = sut.ToUtcTimeZone();

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void ToLocalTimeZone_ShouldReturnCorrectResult()
    {
        var dateTime = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );

        var sut = ZonedMonth.Create( dateTime, timeZone );
        var expected = ZonedMonth.Create( dateTime, TimeZoneInfo.Local );

        var result = sut.ToLocalTimeZone();

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [MethodData( nameof( ZonedMonthTestsData.GetContainsData ) )]
    public void Contains_WithZonedDateTime_ShouldReturnCorrectResult(
        DateTime month,
        TimeZoneInfo dayTimeZone,
        DateTime dateTimeValue,
        TimeZoneInfo timeZone,
        bool expected)
    {
        var dateTime = ZonedDateTime.Create( dateTimeValue, timeZone );
        var sut = ZonedMonth.Create( month, dayTimeZone );

        var result = sut.Contains( dateTime );

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [MethodData( nameof( ZonedMonthTestsData.GetContainsWithAmbiguousStartOrEndData ) )]
    public void Contains_WithZonedDateTimeAndAmbiguousStartOrEnd_ShouldReturnCorrectResult(
        DateTime month,
        TimeZoneInfo timeZone,
        DateTime dateTimeValue,
        bool forceInDaylightSavingMode,
        bool expected)
    {
        var dateTime = ZonedDateTime.Create( dateTimeValue, timeZone );
        if ( forceInDaylightSavingMode )
            dateTime = dateTime.GetOppositeAmbiguousDateTime() ?? dateTime;

        var sut = ZonedMonth.Create( month, timeZone );

        var result = sut.Contains( dateTime );

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [MethodData( nameof( ZonedMonthTestsData.GetContainsWithZonedDayData ) )]
    public void Contains_WithZonedDay_ShouldReturnCorrectResult(
        DateTime month,
        TimeZoneInfo dayTimeZone,
        DateTime dayValue,
        TimeZoneInfo timeZone,
        bool expected)
    {
        var day = ZonedDay.Create( dayValue, timeZone );
        var sut = ZonedMonth.Create( month, dayTimeZone );

        var result = sut.Contains( day );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void GetNext_ShouldReturnTheNextMonth()
    {
        var dateTime = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );

        var sut = ZonedMonth.Create( dateTime, timeZone );
        var expected = sut.AddMonths( 1 );

        var result = sut.GetNext();

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void GetPrevious_ShouldReturnThePreviousMonth()
    {
        var dateTime = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );

        var sut = ZonedMonth.Create( dateTime, timeZone );
        var expected = sut.AddMonths( -1 );

        var result = sut.GetPrevious();

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [MethodData( nameof( ZonedMonthTestsData.GetAddMonthsData ) )]
    public void AddMonths_ShouldReturnCorrectResult(DateTime month, TimeZoneInfo timeZone, int monthsToAdd, DateTime expectedMonth)
    {
        var sut = ZonedMonth.Create( month, timeZone );
        var expected = ZonedMonth.Create( expectedMonth, timeZone );

        var result = sut.AddMonths( monthsToAdd );

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [MethodData( nameof( ZonedMonthTestsData.GetAddData ) )]
    public void Add_ShouldReturnCorrectResult(
        DateTime month,
        TimeZoneInfo timeZone,
        Period period,
        DateTime expectedMonth)
    {
        var sut = ZonedMonth.Create( month, timeZone );
        var expected = ZonedMonth.Create( expectedMonth, timeZone );

        var result = sut.Add( period );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void SubtractMonths_ShouldBeEquivalentToAddMonthsWithNegatedMonthCount()
    {
        var monthCount = Fixture.Create<sbyte>();
        var dateTime = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );

        var sut = ZonedMonth.Create( dateTime, timeZone );
        var expected = sut.AddMonths( -monthCount );

        var result = sut.SubtractMonths( monthCount );

        result.TestEquals( expected ).Go();
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

        var sut = ZonedMonth.Create( dateTime, timeZone );
        var expected = sut.Add( -periodToSubtract );

        var result = sut.Subtract( periodToSubtract );

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [MethodData( nameof( ZonedMonthTestsData.GetGetPeriodOffsetData ) )]
    public void GetPeriodOffset_ShouldReturnCorrectResult(
        DateTime month,
        TimeZoneInfo timeZone,
        DateTime otherMonth,
        TimeZoneInfo otherTimeZone,
        PeriodUnits units)
    {
        var sut = ZonedMonth.Create( month, timeZone );
        var other = ZonedMonth.Create( otherMonth, otherTimeZone );
        var expected = sut.Start.GetPeriodOffset( other.Start, units );

        var result = sut.GetPeriodOffset( other, units );

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [MethodData( nameof( ZonedMonthTestsData.GetGetGreedyPeriodOffsetData ) )]
    public void GetGreedyPeriodOffset_ShouldReturnCorrectResult(
        DateTime month,
        TimeZoneInfo timeZone,
        DateTime otherMonth,
        TimeZoneInfo otherTimeZone,
        PeriodUnits units)
    {
        var sut = ZonedMonth.Create( month, timeZone );
        var other = ZonedMonth.Create( otherMonth, otherTimeZone );
        var expected = sut.Start.GetGreedyPeriodOffset( other.Start, units );

        var result = sut.GetGreedyPeriodOffset( other, units );

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [MethodData( nameof( ZonedMonthTestsData.GetSetYearData ) )]
    public void SetYear_ShouldReturnTargetWithChangedYear(DateTime month, TimeZoneInfo timeZone, int newYear, DateTime expectedMonth)
    {
        var sut = ZonedMonth.Create( month, timeZone );
        var expected = ZonedMonth.Create( expectedMonth, timeZone );

        var result = sut.SetYear( newYear );

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [MethodData( nameof( ZonedMonthTestsData.GetSetYearThrowData ) )]
    public void SetYear_ShouldThrowArgumentOutOfRangeException_WhenYearIsInvalid(DateTime month, TimeZoneInfo timeZone, int newYear)
    {
        var sut = ZonedMonth.Create( month, timeZone );
        var action = Lambda.Of( () => sut.SetYear( newYear ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Theory]
    [MethodData( nameof( ZonedMonthTestsData.GetSetMonthData ) )]
    public void SetMonth_ShouldReturnTargetWithChangedMonth(
        DateTime month,
        TimeZoneInfo timeZone,
        IsoMonthOfYear newMonth,
        DateTime expectedMonth)
    {
        var sut = ZonedMonth.Create( month, timeZone );
        var expected = ZonedMonth.Create( expectedMonth, timeZone );

        var result = sut.SetMonth( newMonth );

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [MethodData( nameof( ZonedMonthTestsData.GetGetDayOfMonthData ) )]
    public void GetDayOfMonth_ShouldReturnCorrectDay(
        DateTime month,
        TimeZoneInfo timeZone,
        int dayOfMonth,
        DateTime expectedDay)
    {
        var sut = ZonedMonth.Create( month, timeZone );
        var expected = ZonedDay.Create( expectedDay, timeZone );

        var result = sut.GetDayOfMonth( dayOfMonth );

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [MethodData( nameof( ZonedMonthTestsData.GetGetDayOfMonthThrowData ) )]
    public void GetDayOfMonth_ShouldThrowArgumentOutOfRangeException_WhenDayIsInvalid(
        DateTime month,
        TimeZoneInfo timeZone,
        int dayOfMonth)
    {
        var sut = ZonedMonth.Create( month, timeZone );
        var action = Lambda.Of( () => sut.GetDayOfMonth( dayOfMonth ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Theory]
    [MethodData( nameof( ZonedMonthTestsData.GetGetDayOfMonthData ) )]
    public void TryGetDayOfMonth_ShouldReturnCorrectDay(
        DateTime month,
        TimeZoneInfo timeZone,
        int dayOfMonth,
        DateTime expectedDay)
    {
        var sut = ZonedMonth.Create( month, timeZone );
        var expected = ZonedDay.Create( expectedDay, timeZone );

        var result = sut.TryGetDayOfMonth( dayOfMonth );

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [MethodData( nameof( ZonedMonthTestsData.GetGetDayOfMonthThrowData ) )]
    public void TryGetDayOfMonth_ShouldReturnNull_WhenDayIsInvalid(
        DateTime month,
        TimeZoneInfo timeZone,
        int dayOfMonth)
    {
        var sut = ZonedMonth.Create( month, timeZone );
        var result = sut.TryGetDayOfMonth( dayOfMonth );
        result.TestNull().Go();
    }

    [Fact]
    public void GetYear_ShouldBeEquivalentToZonedYearCreate()
    {
        var dateTime = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var sut = ZonedMonth.Create( dateTime, timeZone );
        var expected = ZonedYear.Create( dateTime, timeZone );

        var result = sut.GetYear();

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [MethodData( nameof( ZonedMonthTestsData.GetGetWeekOfMonthData ) )]
    public void GetWeekOfMonth_ShouldReturnCorrectResult(
        DateTime month,
        TimeZoneInfo timeZone,
        IsoDayOfWeek weekStart,
        int weekOfMonth,
        DateTime expectedWeekStart)
    {
        var sut = ZonedMonth.Create( month, timeZone );
        var expected = ZonedWeek.Create( expectedWeekStart, timeZone, weekStart );

        var result = sut.GetWeekOfMonth( weekOfMonth, weekStart );

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( 8 )]
    public void GetWeekOfMonth_ShouldThrowArgumentOutOfRangeException_WhenWeekStartIsInvalid(int weekStart)
    {
        var dateTime = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var sut = ZonedMonth.Create( dateTime, timeZone );
        var action = Lambda.Of( () => sut.GetWeekOfMonth( 1, ( IsoDayOfWeek )weekStart ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Theory]
    [MethodData( nameof( ZonedMonthTestsData.GetGetWeekOfMonthThrowData ) )]
    public void GetWeekOfMonth_ShouldThrowArgumentOutOfRangeException_WhenWeekOfMonthIsInvalid(
        DateTime month,
        TimeZoneInfo timeZone,
        IsoDayOfWeek weekStart,
        int weekOfMonth)
    {
        var sut = ZonedMonth.Create( month, timeZone );
        var action = Lambda.Of( () => sut.GetWeekOfMonth( weekOfMonth, weekStart ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Theory]
    [MethodData( nameof( ZonedMonthTestsData.GetGetWeekOfMonthData ) )]
    public void TryGetWeekOfMonth_ShouldReturnCorrectResult(
        DateTime month,
        TimeZoneInfo timeZone,
        IsoDayOfWeek weekStart,
        int weekOfMonth,
        DateTime expectedWeekStart)
    {
        var sut = ZonedMonth.Create( month, timeZone );
        var expected = ZonedWeek.Create( expectedWeekStart, timeZone, weekStart );

        var result = sut.TryGetWeekOfMonth( weekOfMonth, weekStart );

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( 8 )]
    public void TryGetWeekOfMonth_ShouldThrowArgumentOutOfRangeException_WhenWeekStartIsInvalid(int weekStart)
    {
        var dateTime = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var sut = ZonedMonth.Create( dateTime, timeZone );
        var action = Lambda.Of( () => sut.TryGetWeekOfMonth( 1, ( IsoDayOfWeek )weekStart ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Theory]
    [MethodData( nameof( ZonedMonthTestsData.GetGetWeekOfMonthThrowData ) )]
    public void TryGetWeekOfMonth_ShouldReturnNull_WhenWeekOfMonthIsInvalid(
        DateTime month,
        TimeZoneInfo timeZone,
        IsoDayOfWeek weekStart,
        int weekOfMonth)
    {
        var sut = ZonedMonth.Create( month, timeZone );
        var result = sut.TryGetWeekOfMonth( weekOfMonth, weekStart );
        result.TestNull().Go();
    }

    [Theory]
    [MethodData( nameof( ZonedMonthTestsData.GetGetAllDaysData ) )]
    public void GetAllDays_ShouldReturnCorrectDayCollection(DateTime month, TimeZoneInfo timeZone, int expectedDayCount)
    {
        var sut = ZonedMonth.Create( month, timeZone );

        var result = sut.GetAllDays().ToList();

        Assertion.All(
                result.Select( d => new
                    {
                        d.Year,
                        d.Month,
                        d.TimeZone
                    } )
                    .TestAll( (e, _) => e.TestEquals(
                        new
                        {
                            sut.Year,
                            sut.Month,
                            sut.TimeZone
                        } ) ),
                result.Select( d => d.DayOfMonth ).TestSequence( Enumerable.Range( 1, expectedDayCount ) ) )
            .Go();
    }

    [Theory]
    [MethodData( nameof( ZonedMonthTestsData.GetGetWeekCountData ) )]
    public void GetWeekCount_ShouldReturnCorrectResult(DateTime month, TimeZoneInfo timeZone, IsoDayOfWeek weekStart, int expected)
    {
        var sut = ZonedMonth.Create( month, timeZone );
        var result = sut.GetWeekCount( weekStart );
        result.TestEquals( expected ).Go();
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( 8 )]
    public void GetWeekCount_ShouldThrowArgumentOutOfRangeException_WhenWeekStartIsInvalid(int weekStart)
    {
        var dateTime = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var sut = ZonedMonth.Create( dateTime, timeZone );
        var action = Lambda.Of( () => sut.GetWeekCount( ( IsoDayOfWeek )weekStart ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Theory]
    [MethodData( nameof( ZonedMonthTestsData.GetGetAllWeeksData ) )]
    public void GetAllWeeks_ShouldReturnCorrectWeekCollection(
        DateTime month,
        TimeZoneInfo timeZone,
        IsoDayOfWeek weekStart,
        int expectedWeekCount)
    {
        var sut = ZonedMonth.Create( month, timeZone );

        var result = sut.GetAllWeeks( weekStart ).ToList();

        Assertion.All(
                result.Count.TestEquals( expectedWeekCount ),
                result.Select( w => new
                    {
                        w.Start.DayOfWeek,
                        w.TimeZone
                    } )
                    .TestAll( (e, _) => e.TestEquals(
                        new
                        {
                            DayOfWeek = weekStart,
                            sut.TimeZone
                        } ) ),
                result.TestAll( (w, _) => Assertion.Any(
                    "Month",
                    w.Start.Month.TestEquals( sut.Month ),
                    w.End.Month.TestEquals( sut.Month ) ) ),
                result.TestSequence( result.OrderBy( w => w.Start ) ) )
            .Go();
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( 8 )]
    public void GetAllWeeks_ShouldThrowArgumentOutOfRangeException_WhenWeekStartIsInvalid(int weekStart)
    {
        var dateTime = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var sut = ZonedMonth.Create( dateTime, timeZone );
        var action = Lambda.Of( () => sut.GetAllWeeks( ( IsoDayOfWeek )weekStart ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void ToBounds_ShouldReturnBoundsFromStartToEnd()
    {
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var dateTime = Fixture.Create<DateTime>();
        var sut = ZonedMonth.Create( dateTime, timeZone );

        var result = sut.ToBounds();

        result.TestEquals( Bounds.Create( sut.Start, sut.End ) ).Go();
    }

    [Fact]
    public void ToCheckedBounds_ShouldReturnCorrectRangeWithOneElement_WhenStartAndEndAreUnambiguous()
    {
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var dateTime = Fixture.Create<DateTime>();
        var sut = ZonedMonth.Create( dateTime, timeZone );
        var expected = BoundsRange.Create( Bounds.Create( sut.Start, sut.End ) );

        var result = sut.ToCheckedBounds();

        result.TestSequence( expected ).Go();
    }

    [Fact]
    public void ToCheckedBounds_ShouldReturnCorrectRangeWithTwoElements_WhenStartIsAmbiguous()
    {
        var dateTime = new DateTime( 2021, 8, 1 );
        var timeZone = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 4, 26, 0, 40, 0 ),
                transitionEnd: new DateTime( 1, 8, 1, 0, 40, 0 ) ) );

        var sut = ZonedMonth.Create( dateTime, timeZone );
        var expected = BoundsRange.Create(
            new[]
            {
                Bounds.Create( sut.Start, sut.Start.Add( new Duration( 0, 39, 59, 999, 999, 9 ) ) ),
                Bounds.Create( sut.Start.Add( Duration.FromHours( 1 ) ), sut.End )
            } );

        var result = sut.ToCheckedBounds();

        result.TestSequence( expected ).Go();
    }

    [Fact]
    public void ToCheckedBounds_ShouldReturnCorrectRangeWithTwoElements_WhenEndIsAmbiguous()
    {
        var dateTime = new DateTime( 2021, 7, 1 );
        var timeZone = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 4, 26, 0, 40, 0 ),
                transitionEnd: new DateTime( 1, 8, 1, 0, 40, 0 ) ) );

        var sut = ZonedMonth.Create( dateTime, timeZone );
        var expected = BoundsRange.Create(
            new[]
            {
                Bounds.Create( sut.Start, sut.End.Subtract( Duration.FromHours( 1 ) ) ),
                Bounds.Create( sut.End.Subtract( new Duration( 0, 19, 59, 999, 999, 9 ) ), sut.End )
            } );

        var result = sut.ToCheckedBounds();

        result.TestSequence( expected ).Go();
    }

    [Fact]
    public void ToCheckedBounds_ShouldReturnCorrectRangeWithThreeElements_WhenStartAndEndAreAmbiguous()
    {
        var dateTime = new DateTime( 2021, 8, 1 );
        var timeZone = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateRule(
                start: DateTime.MinValue,
                end: new DateTime( 2021, 8, 2 ),
                transitionStart: new DateTime( 1, 4, 26, 0, 40, 0 ),
                transitionEnd: new DateTime( 1, 8, 1, 0, 40, 0 ) ),
            TimeZoneFactory.CreateRule(
                start: new DateTime( 2021, 8, 3 ),
                end: DateTime.MaxValue,
                transitionStart: new DateTime( 1, 8, 4, 0, 40, 0 ),
                transitionEnd: new DateTime( 1, 9, 1, 0, 40, 0 ) ) );

        var sut = ZonedMonth.Create( dateTime, timeZone );
        var expected = BoundsRange.Create(
            new[]
            {
                Bounds.Create( sut.Start, sut.Start.Add( new Duration( 0, 39, 59, 999, 999, 9 ) ) ),
                Bounds.Create( sut.Start.Add( Duration.FromHours( 1 ) ), sut.End.Subtract( Duration.FromHours( 1 ) ) ),
                Bounds.Create( sut.End.Subtract( new Duration( 0, 19, 59, 999, 999, 9 ) ), sut.End )
            } );

        var result = sut.ToCheckedBounds();

        result.TestSequence( expected ).Go();
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

        var sut = ZonedMonth.Create( dateTime, timeZone );
        var expected = sut.Add( periodToSubtract );

        var result = sut + periodToSubtract;

        result.TestEquals( expected ).Go();
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

        var sut = ZonedMonth.Create( dateTime, timeZone );
        var expected = sut.Add( -periodToSubtract );

        var result = sut - periodToSubtract;

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [MethodData( nameof( ZonedMonthTestsData.GetEqualsData ) )]
    public void EqualityOperator_ShouldReturnCorrectResult(
        DateTime dt1,
        TimeZoneInfo tz1,
        DateTime dt2,
        TimeZoneInfo tz2,
        bool expected)
    {
        var a = ZonedMonth.Create( dt1, tz1 );
        var b = ZonedMonth.Create( dt2, tz2 );

        var result = a == b;

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [MethodData( nameof( ZonedMonthTestsData.GetNotEqualsData ) )]
    public void InequalityOperator_ShouldReturnCorrectResult(
        DateTime dt1,
        TimeZoneInfo tz1,
        DateTime dt2,
        TimeZoneInfo tz2,
        bool expected)
    {
        var a = ZonedMonth.Create( dt1, tz1 );
        var b = ZonedMonth.Create( dt2, tz2 );

        var result = a != b;

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [MethodData( nameof( ZonedMonthTestsData.GetGreaterThanComparisonData ) )]
    public void GreaterThanOperator_ShouldReturnCorrectResult(
        DateTime dt1,
        TimeZoneInfo tz1,
        DateTime dt2,
        TimeZoneInfo tz2,
        bool expected)
    {
        var a = ZonedMonth.Create( dt1, tz1 );
        var b = ZonedMonth.Create( dt2, tz2 );

        var result = a > b;

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [MethodData( nameof( ZonedMonthTestsData.GetLessThanOrEqualToComparisonData ) )]
    public void LessThanOrEqualToOperator_ShouldReturnCorrectResult(
        DateTime dt1,
        TimeZoneInfo tz1,
        DateTime dt2,
        TimeZoneInfo tz2,
        bool expected)
    {
        var a = ZonedMonth.Create( dt1, tz1 );
        var b = ZonedMonth.Create( dt2, tz2 );

        var result = a <= b;

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [MethodData( nameof( ZonedMonthTestsData.GetLessThanComparisonData ) )]
    public void LessThanOperator_ShouldReturnCorrectResult(
        DateTime dt1,
        TimeZoneInfo tz1,
        DateTime dt2,
        TimeZoneInfo tz2,
        bool expected)
    {
        var a = ZonedMonth.Create( dt1, tz1 );
        var b = ZonedMonth.Create( dt2, tz2 );

        var result = a < b;

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [MethodData( nameof( ZonedMonthTestsData.GetGreaterThanOrEqualToComparisonData ) )]
    public void GreaterThanOrEqualToOperator_ShouldReturnCorrectResult(
        DateTime dt1,
        TimeZoneInfo tz1,
        DateTime dt2,
        TimeZoneInfo tz2,
        bool expected)
    {
        var a = ZonedMonth.Create( dt1, tz1 );
        var b = ZonedMonth.Create( dt2, tz2 );

        var result = a >= b;

        result.TestEquals( expected ).Go();
    }
}
