using System.Linq;
using LfrlAnvil.Chrono.Extensions;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions.Attributes;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Chrono.Tests.ZonedYearTests;

[TestClass( typeof( ZonedYearTestsData ) )]
public class ZonedYearTests : TestsBase
{
    [Fact]
    public void Default_ShouldReturnStartOfUnixEpochYearInUtcTimeZone()
    {
        var result = default( ZonedYear );
        var expectedStart = ZonedDateTime.CreateUtc( DateTime.UnixEpoch );
        var expectedEnd = ZonedDateTime.CreateUtc( DateTime.UnixEpoch.GetEndOfYear() );

        using ( new AssertionScope() )
        {
            result.Start.Should().Be( expectedStart );
            result.End.Should().Be( expectedEnd );
            result.Year.Should().Be( result.Start.Year );
            result.IsLeap.Should().BeFalse();
            result.DayCount.Should().Be( 365 );
            result.TimeZone.Should().Be( TimeZoneInfo.Utc );
            result.Duration.Should().Be( Duration.FromHours( 8760 ) );
            result.IsLocal.Should().BeFalse();
            result.IsUtc.Should().BeTrue();
        }
    }

    [Theory]
    [MethodData( nameof( ZonedYearTestsData.GetCreateData ) )]
    public void Create_ShouldReturnCorrectResult(DateTime dateTime, TimeZoneInfo timeZone, bool isLeap)
    {
        var expectedStart = ZonedDateTime.Create( dateTime.GetStartOfYear(), timeZone );
        var expectedEnd = ZonedDateTime.Create( dateTime.GetEndOfYear(), timeZone );
        var expectedDayCount = isLeap ? 366 : 365;
        var expectedDuration = Duration.FromHours( 24 * expectedDayCount );
        var sut = ZonedYear.Create( dateTime, timeZone );

        using ( new AssertionScope() )
        {
            sut.Start.Should().Be( expectedStart );
            sut.End.Should().Be( expectedEnd );
            sut.Year.Should().Be( sut.Start.Year );
            sut.IsLeap.Should().Be( isLeap );
            sut.DayCount.Should().Be( expectedDayCount );
            sut.TimeZone.Should().Be( timeZone );
            sut.Duration.Should().Be( expectedDuration );
            sut.IsLocal.Should().BeFalse();
            sut.IsUtc.Should().BeFalse();
        }
    }

    [Theory]
    [MethodData( nameof( ZonedYearTestsData.GetCreateWithInvalidStartTimeOrEndTimeData ) )]
    public void Create_WithInvalidStartTimeOrEndTime_ShouldReturnResultWithEarliestPossibleStart(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        DateTime expectedStartValue,
        DateTime expectedEndValue,
        bool isLeap)
    {
        var expectedStart = ZonedDateTime.Create( expectedStartValue, timeZone );
        var expectedEnd = ZonedDateTime.Create( expectedEndValue, timeZone );
        var expectedDayCount = isLeap ? 366 : 365;
        var expectedDuration = Duration.FromHours( 24 * expectedDayCount );

        var sut = ZonedYear.Create( dateTime, timeZone );

        using ( new AssertionScope() )
        {
            sut.Start.Should().Be( expectedStart );
            sut.End.Should().Be( expectedEnd );
            sut.Year.Should().Be( sut.Start.Year );
            sut.IsLeap.Should().Be( isLeap );
            sut.DayCount.Should().Be( expectedDayCount );
            sut.TimeZone.Should().Be( timeZone );
            sut.Duration.Should().Be( expectedDuration );
            sut.IsLocal.Should().BeFalse();
            sut.IsUtc.Should().BeFalse();
        }
    }

    [Theory]
    [MethodData( nameof( ZonedYearTestsData.GetCreateWithContainedInvalidityAndAmbiguityRangesData ) )]
    public void Create_WithContainedInvalidityAndAmbiguityRanges_ShouldReturnCorrectResult(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        bool isLeap)
    {
        var expectedStart = ZonedDateTime.Create( dateTime.GetStartOfYear(), timeZone );
        var expectedEnd = ZonedDateTime.Create( dateTime.GetEndOfYear(), timeZone );
        var expectedDayCount = isLeap ? 366 : 365;
        var expectedDuration = Duration.FromHours( 24 * expectedDayCount );

        var sut = ZonedYear.Create( dateTime, timeZone );

        using ( new AssertionScope() )
        {
            sut.Start.Should().Be( expectedStart );
            sut.End.Should().Be( expectedEnd );
            sut.Year.Should().Be( sut.Start.Year );
            sut.IsLeap.Should().Be( isLeap );
            sut.DayCount.Should().Be( expectedDayCount );
            sut.TimeZone.Should().Be( timeZone );
            sut.Duration.Should().Be( expectedDuration );
            sut.IsLocal.Should().BeFalse();
            sut.IsUtc.Should().BeFalse();
        }
    }

    [Fact]
    public void Create_WithZonedDateTime_ShouldBeEquivalentToCreateWithDateTimeAndTimeZoneInfo()
    {
        var dateTime = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var source = ZonedDateTime.Create( dateTime, timeZone );
        var expected = ZonedYear.Create( dateTime, timeZone );

        var result = ZonedYear.Create( source );

        result.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void Create_WithZonedDay_ShouldBeEquivalentToCreateWithDateTimeAndTimeZoneInfo()
    {
        var dateTime = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var source = ZonedDay.Create( dateTime, timeZone );
        var expected = ZonedYear.Create( dateTime, timeZone );

        var result = ZonedYear.Create( source );

        result.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void Create_WithZonedMonth_ShouldBeEquivalentToCreateWithDateTimeAndTimeZoneInfo()
    {
        var dateTime = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var source = ZonedMonth.Create( dateTime, timeZone );
        var expected = ZonedYear.Create( dateTime, timeZone );

        var result = ZonedYear.Create( source );

        result.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void Create_WithYear_ShouldBeEquivalentToCreateWithDateTimeAndTimeZoneInfo()
    {
        var dateTime = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var expected = ZonedYear.Create( dateTime, timeZone );

        var result = ZonedYear.Create( dateTime.Year, timeZone );

        result.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void CreateUtc_WithTimestamp_ShouldReturnCorrectYearInUtc()
    {
        var timestamp = new Timestamp( Fixture.Create<DateTime>() );
        var expectedStart = ZonedDateTime.CreateUtc( timestamp.UtcValue.GetStartOfYear() );
        var expectedEnd = ZonedDateTime.CreateUtc( timestamp.UtcValue.GetEndOfYear() );
        var isLeap = DateTime.IsLeapYear( timestamp.UtcValue.Year );
        var expectedDayCount = isLeap ? 366 : 365;
        var expectedDuration = Duration.FromHours( 24 * expectedDayCount );

        var result = ZonedYear.CreateUtc( timestamp );

        using ( new AssertionScope() )
        {
            result.Start.Should().Be( expectedStart );
            result.End.Should().Be( expectedEnd );
            result.Year.Should().Be( result.Start.Year );
            result.IsLeap.Should().Be( isLeap );
            result.DayCount.Should().Be( expectedDayCount );
            result.TimeZone.Should().Be( TimeZoneInfo.Utc );
            result.Duration.Should().Be( expectedDuration );
            result.IsLocal.Should().BeFalse();
            result.IsUtc.Should().BeTrue();
        }
    }

    [Fact]
    public void CreateUtc_WithDateTime_ShouldReturnCorrectMonthInUtc()
    {
        var dateTime = Fixture.Create<DateTime>();
        var expectedStart = ZonedDateTime.CreateUtc( dateTime.GetStartOfYear() );
        var expectedEnd = ZonedDateTime.CreateUtc( dateTime.GetEndOfYear() );
        var isLeap = DateTime.IsLeapYear( dateTime.Year );
        var expectedDayCount = isLeap ? 366 : 365;
        var expectedDuration = Duration.FromHours( 24 * expectedDayCount );

        var result = ZonedYear.CreateUtc( dateTime );

        using ( new AssertionScope() )
        {
            result.Start.Should().Be( expectedStart );
            result.End.Should().Be( expectedEnd );
            result.Year.Should().Be( result.Start.Year );
            result.IsLeap.Should().Be( isLeap );
            result.DayCount.Should().Be( expectedDayCount );
            result.TimeZone.Should().Be( TimeZoneInfo.Utc );
            result.Duration.Should().Be( expectedDuration );
            result.IsLocal.Should().BeFalse();
            result.IsUtc.Should().BeTrue();
        }
    }

    [Fact]
    public void CreateUtc_WithYear_ShouldBeEquivalentToCreateUtcWithDateTime()
    {
        var dateTime = Fixture.Create<DateTime>();
        var expected = ZonedYear.CreateUtc( dateTime );

        var result = ZonedYear.CreateUtc( dateTime.Year );

        result.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void CreateLocal_ShouldReturnCorrectYearInLocal()
    {
        var dateTime = Fixture.Create<DateTime>();
        var expected = ZonedYear.Create( dateTime, TimeZoneInfo.Local );

        var result = ZonedYear.CreateLocal( dateTime );

        result.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void CreateLocal_WithYear_ShouldBeEquivalentToCreateLocalWithDateTime()
    {
        var dateTime = Fixture.Create<DateTime>();
        var expected = ZonedYear.CreateLocal( dateTime );

        var result = ZonedYear.CreateLocal( dateTime.Year );

        result.Should().BeEquivalentTo( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedYearTestsData.GetToStringData ) )]
    public void ToString_ShouldReturnCorrectResult(DateTime year, TimeZoneInfo timeZone, string expected)
    {
        var sut = ZonedYear.Create( year, timeZone );
        var result = sut.ToString();
        result.Should().Be( expected );
    }

    [Fact]
    public void GetHashCode_ShouldReturnCorrectResult()
    {
        var dateTime = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var sut = ZonedYear.Create( dateTime, timeZone );
        var expected = Hash.Default.Add( sut.Start.Timestamp ).Add( sut.End.Timestamp ).Add( sut.TimeZone.Id ).Value;

        var result = sut.GetHashCode();

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedYearTestsData.GetEqualsData ) )]
    public void Equals_ShouldReturnCorrectResult(DateTime dt1, TimeZoneInfo tz1, DateTime dt2, TimeZoneInfo tz2, bool expected)
    {
        var a = ZonedYear.Create( dt1, tz1 );
        var b = ZonedYear.Create( dt2, tz2 );

        var result = a.Equals( b );

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedYearTestsData.GetCompareToData ) )]
    public void CompareTo_ShouldReturnCorrectResult(DateTime dt1, TimeZoneInfo tz1, DateTime dt2, TimeZoneInfo tz2, int expectedSign)
    {
        var a = ZonedYear.Create( dt1, tz1 );
        var b = ZonedYear.Create( dt2, tz2 );

        var result = a.CompareTo( b );

        Math.Sign( result ).Should().Be( expectedSign );
    }

    [Fact]
    public void ToTimeZone_ShouldReturnCorrectResult()
    {
        var dateTime = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var targetTimeZone = TimeZoneFactory.CreateRandom( Fixture );

        var sut = ZonedYear.Create( dateTime, timeZone );
        var expected = ZonedYear.Create( dateTime, targetTimeZone );

        var result = sut.ToTimeZone( targetTimeZone );

        result.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void ToUtcTimeZone_ShouldReturnCorrectResult()
    {
        var dateTime = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );

        var sut = ZonedYear.Create( dateTime, timeZone );
        var expected = ZonedYear.Create( dateTime, TimeZoneInfo.Utc );

        var result = sut.ToUtcTimeZone();

        result.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void ToLocalTimeZone_ShouldReturnCorrectResult()
    {
        var dateTime = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );

        var sut = ZonedYear.Create( dateTime, timeZone );
        var expected = ZonedYear.Create( dateTime, TimeZoneInfo.Local );

        var result = sut.ToLocalTimeZone();

        result.Should().BeEquivalentTo( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedYearTestsData.GetContainsData ) )]
    public void Contains_WithZonedDateTime_ShouldReturnCorrectResult(
        DateTime year,
        TimeZoneInfo dayTimeZone,
        DateTime dateTimeValue,
        TimeZoneInfo timeZone,
        bool expected)
    {
        var dateTime = ZonedDateTime.Create( dateTimeValue, timeZone );
        var sut = ZonedYear.Create( year, dayTimeZone );

        var result = sut.Contains( dateTime );

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedYearTestsData.GetContainsWithZonedDayData ) )]
    public void Contains_WithZonedDay_ShouldReturnCorrectResult(
        DateTime year,
        TimeZoneInfo dayTimeZone,
        DateTime dayValue,
        TimeZoneInfo timeZone,
        bool expected)
    {
        var day = ZonedDay.Create( dayValue, timeZone );
        var sut = ZonedYear.Create( year, dayTimeZone );

        var result = sut.Contains( day );

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedYearTestsData.GetContainsWithZonedMonthData ) )]
    public void Contains_WithZonedMonth_ShouldReturnCorrectResult(
        DateTime year,
        TimeZoneInfo dayTimeZone,
        DateTime monthValue,
        TimeZoneInfo timeZone,
        bool expected)
    {
        var day = ZonedMonth.Create( monthValue, timeZone );
        var sut = ZonedYear.Create( year, dayTimeZone );

        var result = sut.Contains( day );

        result.Should().Be( expected );
    }

    [Fact]
    public void GetNext_ShouldReturnTheNextYear()
    {
        var dateTime = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );

        var sut = ZonedYear.Create( dateTime, timeZone );
        var expected = sut.AddYears( 1 );

        var result = sut.GetNext();

        result.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void GetPrevious_ShouldReturnThePreviousYear()
    {
        var dateTime = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );

        var sut = ZonedYear.Create( dateTime, timeZone );
        var expected = sut.AddYears( -1 );

        var result = sut.GetPrevious();

        result.Should().BeEquivalentTo( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedYearTestsData.GetAddYearsData ) )]
    public void AddYears_ShouldReturnCorrectResult(DateTime year, TimeZoneInfo timeZone, int yearsToAdd, DateTime expectedYear)
    {
        var sut = ZonedYear.Create( year, timeZone );
        var expected = ZonedYear.Create( expectedYear, timeZone );

        var result = sut.AddYears( yearsToAdd );

        result.Should().BeEquivalentTo( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedYearTestsData.GetAddData ) )]
    public void Add_ShouldReturnCorrectResult(
        DateTime year,
        TimeZoneInfo timeZone,
        Period period,
        DateTime expectedYear)
    {
        var sut = ZonedYear.Create( year, timeZone );
        var expected = ZonedYear.Create( expectedYear, timeZone );

        var result = sut.Add( period );

        result.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void SubtractYears_ShouldBeEquivalentToAddYearsWithNegatedYearCount()
    {
        var yearCount = Fixture.Create<sbyte>();
        var dateTime = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );

        var sut = ZonedYear.Create( dateTime, timeZone );
        var expected = sut.AddYears( -yearCount );

        var result = sut.SubtractYears( yearCount );

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

        var sut = ZonedYear.Create( dateTime, timeZone );
        var expected = sut.Add( -periodToSubtract );

        var result = sut.Subtract( periodToSubtract );

        result.Should().BeEquivalentTo( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedYearTestsData.GetGetPeriodOffsetData ) )]
    public void GetPeriodOffset_ShouldReturnCorrectResult(
        DateTime year,
        TimeZoneInfo timeZone,
        DateTime otherYear,
        TimeZoneInfo otherTimeZone,
        PeriodUnits units)
    {
        var sut = ZonedYear.Create( year, timeZone );
        var other = ZonedYear.Create( otherYear, otherTimeZone );
        var expected = sut.Start.GetPeriodOffset( other.Start, units );

        var result = sut.GetPeriodOffset( other, units );

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedYearTestsData.GetGetGreedyPeriodOffsetData ) )]
    public void GetGreedyPeriodOffset_ShouldReturnCorrectResult(
        DateTime year,
        TimeZoneInfo timeZone,
        DateTime otherYear,
        TimeZoneInfo otherTimeZone,
        PeriodUnits units)
    {
        var sut = ZonedYear.Create( year, timeZone );
        var other = ZonedYear.Create( otherYear, otherTimeZone );
        var expected = sut.Start.GetGreedyPeriodOffset( other.Start, units );

        var result = sut.GetGreedyPeriodOffset( other, units );

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedYearTestsData.GetGetMonthData ) )]
    public void GetMonth_ShouldReturnCorrectResult(DateTime year, TimeZoneInfo timeZone, IsoMonthOfYear month, DateTime expectedMonth)
    {
        var sut = ZonedYear.Create( year, timeZone );
        var expected = ZonedMonth.Create( expectedMonth, timeZone );

        var result = sut.GetMonth( month );

        result.Should().Be( expected );
    }

    [Fact]
    public void GetJanuary_ShouldReturnCorrectResult()
    {
        var year = Fixture.Create<DateTime>().Year;
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var sut = ZonedYear.Create( year, timeZone );
        var expected = ZonedMonth.Create( year, IsoMonthOfYear.January, timeZone );

        var result = sut.GetJanuary();

        result.Should().Be( expected );
    }

    [Fact]
    public void GetFebruary_ShouldReturnCorrectResult()
    {
        var year = Fixture.Create<DateTime>().Year;
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var sut = ZonedYear.Create( year, timeZone );
        var expected = ZonedMonth.Create( year, IsoMonthOfYear.February, timeZone );

        var result = sut.GetFebruary();

        result.Should().Be( expected );
    }

    [Fact]
    public void GetMarch_ShouldReturnCorrectResult()
    {
        var year = Fixture.Create<DateTime>().Year;
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var sut = ZonedYear.Create( year, timeZone );
        var expected = ZonedMonth.Create( year, IsoMonthOfYear.March, timeZone );

        var result = sut.GetMarch();

        result.Should().Be( expected );
    }

    [Fact]
    public void GetApril_ShouldReturnCorrectResult()
    {
        var year = Fixture.Create<DateTime>().Year;
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var sut = ZonedYear.Create( year, timeZone );
        var expected = ZonedMonth.Create( year, IsoMonthOfYear.April, timeZone );

        var result = sut.GetApril();

        result.Should().Be( expected );
    }

    [Fact]
    public void GetMay_ShouldReturnCorrectResult()
    {
        var year = Fixture.Create<DateTime>().Year;
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var sut = ZonedYear.Create( year, timeZone );
        var expected = ZonedMonth.Create( year, IsoMonthOfYear.May, timeZone );

        var result = sut.GetMay();

        result.Should().Be( expected );
    }

    [Fact]
    public void GetJune_ShouldReturnCorrectResult()
    {
        var year = Fixture.Create<DateTime>().Year;
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var sut = ZonedYear.Create( year, timeZone );
        var expected = ZonedMonth.Create( year, IsoMonthOfYear.June, timeZone );

        var result = sut.GetJune();

        result.Should().Be( expected );
    }

    [Fact]
    public void GetJuly_ShouldReturnCorrectResult()
    {
        var year = Fixture.Create<DateTime>().Year;
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var sut = ZonedYear.Create( year, timeZone );
        var expected = ZonedMonth.Create( year, IsoMonthOfYear.July, timeZone );

        var result = sut.GetJuly();

        result.Should().Be( expected );
    }

    [Fact]
    public void GetAugust_ShouldReturnCorrectResult()
    {
        var year = Fixture.Create<DateTime>().Year;
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var sut = ZonedYear.Create( year, timeZone );
        var expected = ZonedMonth.Create( year, IsoMonthOfYear.August, timeZone );

        var result = sut.GetAugust();

        result.Should().Be( expected );
    }

    [Fact]
    public void GetSeptember_ShouldReturnCorrectResult()
    {
        var year = Fixture.Create<DateTime>().Year;
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var sut = ZonedYear.Create( year, timeZone );
        var expected = ZonedMonth.Create( year, IsoMonthOfYear.September, timeZone );

        var result = sut.GetSeptember();

        result.Should().Be( expected );
    }

    [Fact]
    public void GetOctober_ShouldReturnCorrectResult()
    {
        var year = Fixture.Create<DateTime>().Year;
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var sut = ZonedYear.Create( year, timeZone );
        var expected = ZonedMonth.Create( year, IsoMonthOfYear.October, timeZone );

        var result = sut.GetOctober();

        result.Should().Be( expected );
    }

    [Fact]
    public void GetNovember_ShouldReturnCorrectResult()
    {
        var year = Fixture.Create<DateTime>().Year;
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var sut = ZonedYear.Create( year, timeZone );
        var expected = ZonedMonth.Create( year, IsoMonthOfYear.November, timeZone );

        var result = sut.GetNovember();

        result.Should().Be( expected );
    }

    [Fact]
    public void GetDecember_ShouldReturnCorrectResult()
    {
        var year = Fixture.Create<DateTime>().Year;
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var sut = ZonedYear.Create( year, timeZone );
        var expected = ZonedMonth.Create( year, IsoMonthOfYear.December, timeZone );

        var result = sut.GetDecember();

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedYearTestsData.GetGetDayOfYearData ) )]
    public void GetDayOfYear_ShouldReturnCorrectDay(
        DateTime year,
        TimeZoneInfo timeZone,
        int dayOfYear,
        DateTime expectedDay)
    {
        var sut = ZonedYear.Create( year, timeZone );
        var expected = ZonedDay.Create( expectedDay, timeZone );

        var result = sut.GetDayOfYear( dayOfYear );

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedYearTestsData.GetGetDayOfYearThrowData ) )]
    public void GetDayOfYear_ShouldThrowArgumentOutOfRangeException_WhenDayIsInvalid(
        DateTime year,
        TimeZoneInfo timeZone,
        int dayOfYear)
    {
        var sut = ZonedYear.Create( year, timeZone );
        var action = Lambda.Of( () => sut.GetDayOfYear( dayOfYear ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [MethodData( nameof( ZonedYearTestsData.GetGetDayOfYearData ) )]
    public void TryGetDayOfYear_ShouldReturnCorrectDay(
        DateTime year,
        TimeZoneInfo timeZone,
        int dayOfYear,
        DateTime expectedDay)
    {
        var sut = ZonedYear.Create( year, timeZone );
        var expected = ZonedDay.Create( expectedDay, timeZone );

        var result = sut.TryGetDayOfYear( dayOfYear );

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedYearTestsData.GetGetDayOfYearThrowData ) )]
    public void TryGetDayOfYear_ShouldReturnNull_WhenDayIsInvalid(
        DateTime year,
        TimeZoneInfo timeZone,
        int dayOfYear)
    {
        var sut = ZonedYear.Create( year, timeZone );
        var result = sut.TryGetDayOfYear( dayOfYear );
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
    public void GetWeekOfYear_ShouldBeEquivalentToToZonedWeekCreate(IsoDayOfWeek weekStart)
    {
        var dateTime = Fixture.Create<DateTime>();
        var weekOfYear = Fixture.CreatePositiveInt32() % 50 + 1;
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var sut = ZonedYear.Create( dateTime, timeZone );
        var expected = ZonedWeek.Create( dateTime.Year, weekOfYear, timeZone, weekStart );

        var result = sut.GetWeekOfYear( weekOfYear, weekStart );

        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( IsoDayOfWeek.Monday )]
    [InlineData( IsoDayOfWeek.Tuesday )]
    [InlineData( IsoDayOfWeek.Wednesday )]
    [InlineData( IsoDayOfWeek.Thursday )]
    [InlineData( IsoDayOfWeek.Friday )]
    [InlineData( IsoDayOfWeek.Saturday )]
    [InlineData( IsoDayOfWeek.Sunday )]
    public void TryGetWeekOfYear_ShouldBeEquivalentToToZonedWeekCreate(IsoDayOfWeek weekStart)
    {
        var dateTime = Fixture.Create<DateTime>();
        var weekOfYear = Fixture.CreatePositiveInt32() % 50 + 1;
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var sut = ZonedYear.Create( dateTime, timeZone );
        var expected = ZonedWeek.Create( dateTime.Year, weekOfYear, timeZone, weekStart );

        var result = sut.TryGetWeekOfYear( weekOfYear, weekStart );

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedYearTestsData.GetGetWeekCountData ) )]
    public void GetWeekCount_ShouldReturnCorrectResult(DateTime year, TimeZoneInfo timeZone, IsoDayOfWeek weekStart, int expected)
    {
        var sut = ZonedYear.Create( year, timeZone );
        var result = sut.GetWeekCount( weekStart );
        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( 8 )]
    public void GetWeekCount_ShouldThrowArgumentOutOfRangeException_WhenWeekStartIsInvalid(int weekStart)
    {
        var dateTime = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var sut = ZonedYear.Create( dateTime, timeZone );
        var action = Lambda.Of( () => sut.GetWeekCount( ( IsoDayOfWeek )weekStart ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [MethodData( nameof( ZonedYearTestsData.GetGetAllWeeksData ) )]
    public void GetAllWeeks_ShouldReturnCorrectWeekCollection(
        DateTime year,
        TimeZoneInfo timeZone,
        IsoDayOfWeek weekStart,
        int expectedWeekCount)
    {
        var sut = ZonedYear.Create( year, timeZone );

        var result = sut.GetAllWeeks( weekStart ).ToList();

        using ( new AssertionScope() )
        {
            result.Select(
                    w => new
                    {
                        w.Year,
                        w.Start.DayOfWeek,
                        w.TimeZone
                    } )
                .Should()
                .AllBeEquivalentTo(
                    new
                    {
                        sut.Year,
                        DayOfWeek = weekStart,
                        sut.TimeZone
                    } );

            result.Select( w => w.WeekOfYear ).Should().BeSequentiallyEqualTo( Enumerable.Range( 1, expectedWeekCount ) );
        }
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( 8 )]
    public void GetAllWeeks_ShouldThrowArgumentOutOfRangeException_WhenWeekStartIsInvalid(int weekStart)
    {
        var dateTime = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var sut = ZonedYear.Create( dateTime, timeZone );
        var action = Lambda.Of( () => sut.GetAllWeeks( ( IsoDayOfWeek )weekStart ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void GetAllMonths_ShouldReturnCorrectMonthCollection()
    {
        var year = Fixture.Create<DateTime>().Year;
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var sut = ZonedYear.Create( year, timeZone );

        var result = sut.GetAllMonths().ToList();

        using ( new AssertionScope() )
        {
            result.Select(
                    d => new
                    {
                        d.Year,
                        d.TimeZone
                    } )
                .Should()
                .AllBeEquivalentTo(
                    new
                    {
                        sut.Year,
                        sut.TimeZone
                    } );

            result.Select( d => ( int )d.Month ).Should().BeSequentiallyEqualTo( Enumerable.Range( 1, 12 ) );
        }
    }

    [Theory]
    [MethodData( nameof( ZonedYearTestsData.GetGetAllDaysData ) )]
    public void GetAllDays_ShouldReturnCorrectDayCollection(DateTime year, TimeZoneInfo timeZone, int expectedDayCount)
    {
        var sut = ZonedYear.Create( year, timeZone );

        var result = sut.GetAllDays().ToList();

        using ( new AssertionScope() )
        {
            result.Select(
                    d => new
                    {
                        d.Year,
                        d.TimeZone
                    } )
                .Should()
                .AllBeEquivalentTo(
                    new
                    {
                        sut.Year,
                        sut.TimeZone
                    } );

            result.Select( d => d.DayOfYear ).Should().BeSequentiallyEqualTo( Enumerable.Range( 1, expectedDayCount ) );
        }
    }

    [Fact]
    public void ToBounds_ShouldReturnBoundsFromStartToEnd()
    {
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var dateTime = Fixture.Create<DateTime>();
        var sut = ZonedYear.Create( dateTime, timeZone );

        var result = sut.ToBounds();

        result.Should().Be( Bounds.Create( sut.Start, sut.End ) );
    }

    [Fact]
    public void ToCheckedBounds_ShouldReturnCorrectRangeWithOneElement_WhenStartAndEndAreUnambiguous()
    {
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var dateTime = Fixture.Create<DateTime>();
        var sut = ZonedYear.Create( dateTime, timeZone );
        var expected = BoundsRange.Create( Bounds.Create( sut.Start, sut.End ) );

        var result = sut.ToCheckedBounds();

        result.Should().BeSequentiallyEqualTo( expected );
    }

    [Fact]
    public void ToCheckedBounds_ShouldReturnCorrectRangeWithTwoElements_WhenStartIsAmbiguous()
    {
        var dateTime = new DateTime( 2021, 1, 1 );
        var timeZone = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateRule(
                start: DateTime.MinValue,
                end: new DateTime( 2021, 2, 1 ),
                transitionStart: new DateTime( 1, 4, 26, 0, 40, 0 ),
                transitionEnd: new DateTime( 1, 1, 1, 0, 40, 0 ) ) );

        var sut = ZonedYear.Create( dateTime, timeZone );
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
        var dateTime = new DateTime( 2021, 1, 1 );
        var timeZone = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateRule(
                start: new DateTime( 2021, 2, 1 ),
                end: DateTime.MaxValue,
                transitionStart: new DateTime( 1, 4, 26, 0, 40, 0 ),
                transitionEnd: new DateTime( 1, 1, 1, 0, 40, 0 ) ) );

        var sut = ZonedYear.Create( dateTime, timeZone );
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
        var dateTime = new DateTime( 2021, 1, 1 );
        var timeZone = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 8, 4, 0, 40, 0 ),
                transitionEnd: new DateTime( 1, 1, 1, 0, 40, 0 ) ) );

        var sut = ZonedYear.Create( dateTime, timeZone );
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

        var sut = ZonedYear.Create( dateTime, timeZone );
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

        var sut = ZonedYear.Create( dateTime, timeZone );
        var expected = sut.Add( -periodToSubtract );

        var result = sut - periodToSubtract;

        result.Should().BeEquivalentTo( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedYearTestsData.GetEqualsData ) )]
    public void EqualityOperator_ShouldReturnCorrectResult(
        DateTime dt1,
        TimeZoneInfo tz1,
        DateTime dt2,
        TimeZoneInfo tz2,
        bool expected)
    {
        var a = ZonedYear.Create( dt1, tz1 );
        var b = ZonedYear.Create( dt2, tz2 );

        var result = a == b;

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedYearTestsData.GetNotEqualsData ) )]
    public void InequalityOperator_ShouldReturnCorrectResult(
        DateTime dt1,
        TimeZoneInfo tz1,
        DateTime dt2,
        TimeZoneInfo tz2,
        bool expected)
    {
        var a = ZonedYear.Create( dt1, tz1 );
        var b = ZonedYear.Create( dt2, tz2 );

        var result = a != b;

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedYearTestsData.GetGreaterThanComparisonData ) )]
    public void GreaterThanOperator_ShouldReturnCorrectResult(
        DateTime dt1,
        TimeZoneInfo tz1,
        DateTime dt2,
        TimeZoneInfo tz2,
        bool expected)
    {
        var a = ZonedYear.Create( dt1, tz1 );
        var b = ZonedYear.Create( dt2, tz2 );

        var result = a > b;

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedYearTestsData.GetLessThanOrEqualToComparisonData ) )]
    public void LessThanOrEqualToOperator_ShouldReturnCorrectResult(
        DateTime dt1,
        TimeZoneInfo tz1,
        DateTime dt2,
        TimeZoneInfo tz2,
        bool expected)
    {
        var a = ZonedYear.Create( dt1, tz1 );
        var b = ZonedYear.Create( dt2, tz2 );

        var result = a <= b;

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedYearTestsData.GetLessThanComparisonData ) )]
    public void LessThanOperator_ShouldReturnCorrectResult(
        DateTime dt1,
        TimeZoneInfo tz1,
        DateTime dt2,
        TimeZoneInfo tz2,
        bool expected)
    {
        var a = ZonedYear.Create( dt1, tz1 );
        var b = ZonedYear.Create( dt2, tz2 );

        var result = a < b;

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedYearTestsData.GetGreaterThanOrEqualToComparisonData ) )]
    public void GreaterThanOrEqualToOperator_ShouldReturnCorrectResult(
        DateTime dt1,
        TimeZoneInfo tz1,
        DateTime dt2,
        TimeZoneInfo tz2,
        bool expected)
    {
        var a = ZonedYear.Create( dt1, tz1 );
        var b = ZonedYear.Create( dt2, tz2 );

        var result = a >= b;

        result.Should().Be( expected );
    }
}
