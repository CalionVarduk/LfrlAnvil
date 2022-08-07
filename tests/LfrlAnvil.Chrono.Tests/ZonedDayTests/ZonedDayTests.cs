using FluentAssertions.Execution;
using LfrlAnvil.Chrono.Exceptions;
using LfrlAnvil.Chrono.Extensions;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions.Attributes;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Chrono.Tests.ZonedDayTests;

[TestClass( typeof( ZonedDayTestsData ) )]
public class ZonedDayTests : TestsBase
{
    [Fact]
    public void Default_ShouldReturnStartOfUnixEpochDayInUtcTimeZone()
    {
        var result = default( ZonedDay );
        var expectedStart = ZonedDateTime.CreateUtc( DateTime.UnixEpoch );
        var expectedEnd = ZonedDateTime.CreateUtc( DateTime.UnixEpoch.GetEndOfDay() );

        using ( new AssertionScope() )
        {
            result.Start.Should().Be( expectedStart );
            result.End.Should().Be( expectedEnd );
            AssertDateProperties( result );
            result.TimeZone.Should().Be( TimeZoneInfo.Utc );
            result.Duration.Should().Be( Duration.FromHours( 24 ) );
            result.IsLocal.Should().BeFalse();
            result.IsUtc.Should().BeTrue();
        }
    }

    [Theory]
    [MethodData( nameof( ZonedDayTestsData.GetCreateData ) )]
    public void Create_ShouldReturnCorrectResult(DateTime dateTime, TimeZoneInfo timeZone)
    {
        var expectedStart = ZonedDateTime.Create( dateTime.GetStartOfDay(), timeZone );
        var expectedEnd = ZonedDateTime.Create( dateTime.GetEndOfDay(), timeZone );
        var sut = ZonedDay.Create( dateTime, timeZone );

        using ( new AssertionScope() )
        {
            sut.Start.Should().Be( expectedStart );
            sut.End.Should().Be( expectedEnd );
            AssertDateProperties( sut );
            sut.TimeZone.Should().Be( timeZone );
            sut.Duration.Should().Be( Duration.FromHours( 24 ) );
            sut.IsLocal.Should().BeFalse();
            sut.IsUtc.Should().BeFalse();
        }
    }

    [Theory]
    [MethodData( nameof( ZonedDayTestsData.GetCreateWithContainedInvalidityRangeData ) )]
    public void Create_WithContainedInvalidityRange_ShouldReturnCorrectResult(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        Duration expectedDuration)
    {
        var expectedStart = ZonedDateTime.Create( dateTime.GetStartOfDay(), timeZone );
        var expectedEnd = ZonedDateTime.Create( dateTime.GetEndOfDay(), timeZone );

        var sut = ZonedDay.Create( dateTime, timeZone );

        using ( new AssertionScope() )
        {
            sut.Start.Should().Be( expectedStart );
            sut.End.Should().Be( expectedEnd );
            AssertDateProperties( sut );
            sut.TimeZone.Should().Be( timeZone );
            sut.Duration.Should().Be( expectedDuration );
            sut.IsLocal.Should().BeFalse();
            sut.IsUtc.Should().BeFalse();
        }
    }

    [Theory]
    [MethodData( nameof( ZonedDayTestsData.GetCreateWithContainedAmbiguityRangeData ) )]
    public void Create_WithContainedAmbiguityRange_ShouldReturnCorrectResult(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        Duration expectedDuration)
    {
        var expectedStart = ZonedDateTime.Create( dateTime.GetStartOfDay(), timeZone );
        var expectedEnd = ZonedDateTime.Create( dateTime.GetEndOfDay(), timeZone );

        var sut = ZonedDay.Create( dateTime, timeZone );

        using ( new AssertionScope() )
        {
            sut.Start.Should().Be( expectedStart );
            sut.End.Should().Be( expectedEnd );
            AssertDateProperties( sut );
            sut.TimeZone.Should().Be( timeZone );
            sut.Duration.Should().Be( expectedDuration );
            sut.IsLocal.Should().BeFalse();
            sut.IsUtc.Should().BeFalse();
        }
    }

    [Theory]
    [MethodData( nameof( ZonedDayTestsData.GetCreateWithInvalidStartTimeData ) )]
    public void Create_WithInvalidStartTime_ShouldReturnResultWithEarliestPossibleStart(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        DateTime expectedStartValue,
        Duration expectedDuration)
    {
        var expectedStart = ZonedDateTime.Create( expectedStartValue, timeZone );
        var expectedEnd = ZonedDateTime.Create( dateTime.GetEndOfDay(), timeZone );

        var sut = ZonedDay.Create( dateTime, timeZone );

        using ( new AssertionScope() )
        {
            sut.Start.Should().Be( expectedStart );
            sut.End.Should().Be( expectedEnd );
            AssertDateProperties( sut );
            sut.TimeZone.Should().Be( timeZone );
            sut.Duration.Should().Be( expectedDuration );
            sut.IsLocal.Should().BeFalse();
            sut.IsUtc.Should().BeFalse();
        }
    }

    [Theory]
    [MethodData( nameof( ZonedDayTestsData.GetCreateWithInvalidEndTimeData ) )]
    public void Create_WithInvalidEndTime_ShouldReturnResultWithLatestPossibleEnd(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        DateTime expectedEndValue,
        Duration expectedDuration)
    {
        var expectedStart = ZonedDateTime.Create( dateTime.GetStartOfDay(), timeZone );
        var expectedEnd = ZonedDateTime.Create( expectedEndValue, timeZone );

        var sut = ZonedDay.Create( dateTime, timeZone );

        using ( new AssertionScope() )
        {
            sut.Start.Should().Be( expectedStart );
            sut.End.Should().Be( expectedEnd );
            AssertDateProperties( sut );
            sut.TimeZone.Should().Be( timeZone );
            sut.Duration.Should().Be( expectedDuration );
            sut.IsLocal.Should().BeFalse();
            sut.IsUtc.Should().BeFalse();
        }
    }

    [Theory]
    [MethodData( nameof( ZonedDayTestsData.GetCreateWithAmbiguousStartTimeData ) )]
    public void Create_WithAmbiguousStartTime_ShouldReturnResultWithEarliestPossibleStart(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        bool forceInDaylightSavingMode,
        Duration expectedDuration)
    {
        var expectedStart = ZonedDateTime.Create( dateTime.GetStartOfDay(), timeZone );
        if ( forceInDaylightSavingMode )
            expectedStart = expectedStart.GetOppositeAmbiguousDateTime() ?? expectedStart;

        var expectedEnd = ZonedDateTime.Create( dateTime.GetEndOfDay(), timeZone );

        var sut = ZonedDay.Create( dateTime, timeZone );

        using ( new AssertionScope() )
        {
            sut.Start.Should().Be( expectedStart );
            sut.End.Should().Be( expectedEnd );
            AssertDateProperties( sut );
            sut.TimeZone.Should().Be( timeZone );
            sut.Duration.Should().Be( expectedDuration );
            sut.IsLocal.Should().BeFalse();
            sut.IsUtc.Should().BeFalse();
        }
    }

    [Theory]
    [MethodData( nameof( ZonedDayTestsData.GetCreateWithAmbiguousEndTimeData ) )]
    public void Create_WithAmbiguousEndTime_ShouldReturnResultWithLatestPossibleEnd(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        bool forceInDaylightSavingMode,
        Duration expectedDuration)
    {
        var expectedStart = ZonedDateTime.Create( dateTime.GetStartOfDay(), timeZone );
        var expectedEnd = ZonedDateTime.Create( dateTime.GetEndOfDay(), timeZone );
        if ( forceInDaylightSavingMode )
            expectedEnd = expectedEnd.GetOppositeAmbiguousDateTime() ?? expectedEnd;

        var sut = ZonedDay.Create( dateTime, timeZone );

        using ( new AssertionScope() )
        {
            sut.Start.Should().Be( expectedStart );
            sut.End.Should().Be( expectedEnd );
            AssertDateProperties( sut );
            sut.TimeZone.Should().Be( timeZone );
            sut.Duration.Should().Be( expectedDuration );
            sut.IsLocal.Should().BeFalse();
            sut.IsUtc.Should().BeFalse();
        }
    }

    [Theory]
    [MethodData( nameof( ZonedDayTestsData.GetCreateWithInvalidStartTimeAndAmbiguousEndTimeData ) )]
    public void Create_WithInvalidStartTimeAndAmbiguousEndTime_ShouldReturnResultWithEarliestPossibleStart_AndLatestPossibleEnd(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        DateTime expectedStartValue,
        bool forceInDaylightSavingMode,
        Duration expectedDuration)
    {
        var expectedStart = ZonedDateTime.Create( expectedStartValue, timeZone );
        var expectedEnd = ZonedDateTime.Create( dateTime.GetEndOfDay(), timeZone );
        if ( forceInDaylightSavingMode )
            expectedEnd = expectedEnd.GetOppositeAmbiguousDateTime() ?? expectedEnd;

        var sut = ZonedDay.Create( dateTime, timeZone );

        using ( new AssertionScope() )
        {
            sut.Start.Should().Be( expectedStart );
            sut.End.Should().Be( expectedEnd );
            AssertDateProperties( sut );
            sut.TimeZone.Should().Be( timeZone );
            sut.Duration.Should().Be( expectedDuration );
            sut.IsLocal.Should().BeFalse();
            sut.IsUtc.Should().BeFalse();
        }
    }

    [Theory]
    [MethodData( nameof( ZonedDayTestsData.GetCreateWithAmbiguousStartTimeAndInvalidEndTimeData ) )]
    public void Create_WithAmbiguousStartTimeAndInvalidEndTime_ShouldReturnResultWithEarliestPossibleStart_AndLatestPossibleEnd(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        DateTime expectedEndValue,
        bool forceInDaylightSavingMode,
        Duration expectedDuration)
    {
        var expectedStart = ZonedDateTime.Create( dateTime.GetStartOfDay(), timeZone );
        if ( forceInDaylightSavingMode )
            expectedStart = expectedStart.GetOppositeAmbiguousDateTime() ?? expectedStart;

        var expectedEnd = ZonedDateTime.Create( expectedEndValue, timeZone );

        var sut = ZonedDay.Create( dateTime, timeZone );

        using ( new AssertionScope() )
        {
            sut.Start.Should().Be( expectedStart );
            sut.End.Should().Be( expectedEnd );
            AssertDateProperties( sut );
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
        var expected = ZonedDay.Create( dateTime, timeZone );

        var result = ZonedDay.Create( source );

        result.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void CreateUtc_WithTimestamp_ShouldReturnCorrectDayInUtc()
    {
        var timestamp = new Timestamp( Fixture.Create<DateTime>() );
        var expectedStart = ZonedDateTime.CreateUtc( timestamp.UtcValue.GetStartOfDay() );
        var expectedEnd = ZonedDateTime.CreateUtc( timestamp.UtcValue.GetEndOfDay() );

        var result = ZonedDay.CreateUtc( timestamp );

        using ( new AssertionScope() )
        {
            result.Start.Should().Be( expectedStart );
            result.End.Should().Be( expectedEnd );
            AssertDateProperties( result );
            result.TimeZone.Should().Be( TimeZoneInfo.Utc );
            result.Duration.Should().Be( Duration.FromHours( 24 ) );
            result.IsLocal.Should().BeFalse();
            result.IsUtc.Should().BeTrue();
        }
    }

    [Fact]
    public void CreateUtc_WithDateTime_ShouldReturnCorrectDayInUtc()
    {
        var dateTime = Fixture.Create<DateTime>();
        var expectedStart = ZonedDateTime.CreateUtc( dateTime.GetStartOfDay() );
        var expectedEnd = ZonedDateTime.CreateUtc( dateTime.GetEndOfDay() );

        var result = ZonedDay.CreateUtc( dateTime );

        using ( new AssertionScope() )
        {
            result.Start.Should().Be( expectedStart );
            result.End.Should().Be( expectedEnd );
            AssertDateProperties( result );
            result.TimeZone.Should().Be( TimeZoneInfo.Utc );
            result.Duration.Should().Be( Duration.FromHours( 24 ) );
            result.IsLocal.Should().BeFalse();
            result.IsUtc.Should().BeTrue();
        }
    }

    [Fact]
    public void CreateLocal_ShouldReturnCorrectDayInLocal()
    {
        var dateTime = Fixture.Create<DateTime>();
        var expected = ZonedDay.Create( dateTime, TimeZoneInfo.Local );

        var result = ZonedDay.CreateLocal( dateTime );

        result.Should().BeEquivalentTo( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedDayTestsData.GetToStringData ) )]
    public void ToString_ShouldReturnCorrectResult(DateTime day, TimeZoneInfo timeZone, string expected)
    {
        var sut = ZonedDay.Create( day, timeZone );
        var result = sut.ToString();
        result.Should().Be( expected );
    }

    [Fact]
    public void GetHashCode_ShouldReturnCorrectResult()
    {
        var dateTime = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var sut = ZonedDay.Create( dateTime, timeZone );
        var expected = Hash.Default.Add( sut.Start.Timestamp ).Add( sut.End.Timestamp ).Add( sut.TimeZone.Id ).Value;

        var result = sut.GetHashCode();

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedDayTestsData.GetEqualsData ) )]
    public void Equals_ShouldReturnCorrectResult(DateTime dt1, TimeZoneInfo tz1, DateTime dt2, TimeZoneInfo tz2, bool expected)
    {
        var a = ZonedDay.Create( dt1, tz1 );
        var b = ZonedDay.Create( dt2, tz2 );

        var result = a.Equals( b );

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedDayTestsData.GetCompareToData ) )]
    public void CompareTo_ShouldReturnCorrectResult(DateTime dt1, TimeZoneInfo tz1, DateTime dt2, TimeZoneInfo tz2, int expectedSign)
    {
        var a = ZonedDay.Create( dt1, tz1 );
        var b = ZonedDay.Create( dt2, tz2 );

        var result = a.CompareTo( b );

        Math.Sign( result ).Should().Be( expectedSign );
    }

    [Fact]
    public void ToTimeZone_ShouldReturnCorrectResult()
    {
        var dateTime = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var targetTimeZone = TimeZoneFactory.CreateRandom( Fixture );

        var sut = ZonedDay.Create( dateTime, timeZone );
        var expected = ZonedDay.Create( dateTime, targetTimeZone );

        var result = sut.ToTimeZone( targetTimeZone );

        result.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void ToUtcTimeZone_ShouldReturnCorrectResult()
    {
        var dateTime = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );

        var sut = ZonedDay.Create( dateTime, timeZone );
        var expected = ZonedDay.Create( dateTime, TimeZoneInfo.Utc );

        var result = sut.ToUtcTimeZone();

        result.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void ToLocalTimeZone_ShouldReturnCorrectResult()
    {
        var dateTime = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );

        var sut = ZonedDay.Create( dateTime, timeZone );
        var expected = ZonedDay.Create( dateTime, TimeZoneInfo.Local );

        var result = sut.ToLocalTimeZone();

        result.Should().BeEquivalentTo( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedDayTestsData.GetContainsData ) )]
    public void Contains_ShouldReturnCorrectResult(
        DateTime day,
        TimeZoneInfo dayTimeZone,
        DateTime dateTimeValue,
        TimeZoneInfo timeZone,
        bool expected)
    {
        var dateTime = ZonedDateTime.Create( dateTimeValue, timeZone );
        var sut = ZonedDay.Create( day, dayTimeZone );

        var result = sut.Contains( dateTime );

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedDayTestsData.GetContainsWithAmbiguousStartOrEndData ) )]
    public void Contains_WithAmbiguousStartOrEnd_ShouldReturnCorrectResult(
        DateTime day,
        TimeZoneInfo timeZone,
        DateTime dateTimeValue,
        bool forceInDaylightSavingMode,
        bool expected)
    {
        var dateTime = ZonedDateTime.Create( dateTimeValue, timeZone );
        if ( forceInDaylightSavingMode )
            dateTime = dateTime.GetOppositeAmbiguousDateTime() ?? dateTime;

        var sut = ZonedDay.Create( day, timeZone );

        var result = sut.Contains( dateTime );

        result.Should().Be( expected );
    }

    [Fact]
    public void GetNext_ShouldReturnTheNextDay()
    {
        var dateTime = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );

        var sut = ZonedDay.Create( dateTime, timeZone );
        var expected = sut.AddDays( 1 );

        var result = sut.GetNext();

        result.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void GetPrevious_ShouldReturnThePreviousDay()
    {
        var dateTime = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );

        var sut = ZonedDay.Create( dateTime, timeZone );
        var expected = sut.AddDays( -1 );

        var result = sut.GetPrevious();

        result.Should().BeEquivalentTo( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedDayTestsData.GetAddDaysData ) )]
    public void AddDays_ShouldReturnCorrectResult(DateTime day, TimeZoneInfo timeZone, int daysToAdd, DateTime expectedDay)
    {
        var sut = ZonedDay.Create( day, timeZone );
        var expected = ZonedDay.Create( expectedDay, timeZone );

        var result = sut.AddDays( daysToAdd );

        result.Should().BeEquivalentTo( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedDayTestsData.GetAddData ) )]
    public void Add_ShouldReturnCorrectResult(
        DateTime day,
        TimeZoneInfo timeZone,
        Period period,
        DateTime expectedDay)
    {
        var sut = ZonedDay.Create( day, timeZone );
        var expected = ZonedDay.Create( expectedDay, timeZone );

        var result = sut.Add( period );

        result.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void SubtractDays_ShouldBeEquivalentToAddDaysWithNegatedDayCount()
    {
        var dayCount = Fixture.Create<sbyte>();
        var dateTime = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );

        var sut = ZonedDay.Create( dateTime, timeZone );
        var expected = sut.AddDays( -dayCount );

        var result = sut.SubtractDays( dayCount );

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
            ticks: Fixture.Create<short>() );

        var dateTime = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );

        var sut = ZonedDay.Create( dateTime, timeZone );
        var expected = sut.Add( -periodToSubtract );

        var result = sut.Subtract( periodToSubtract );

        result.Should().BeEquivalentTo( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedDayTestsData.GetGetPeriodOffsetData ) )]
    public void GetPeriodOffset_ShouldReturnCorrectResult(
        DateTime day,
        TimeZoneInfo timeZone,
        DateTime otherDay,
        TimeZoneInfo otherTimeZone,
        PeriodUnits units)
    {
        var sut = ZonedDay.Create( day, timeZone );
        var other = ZonedDay.Create( otherDay, otherTimeZone );
        var expected = sut.Start.GetPeriodOffset( other.Start, units );

        var result = sut.GetPeriodOffset( other, units );

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedDayTestsData.GetGetGreedyPeriodOffsetData ) )]
    public void GetGreedyPeriodOffset_ShouldReturnCorrectResult(
        DateTime day,
        TimeZoneInfo timeZone,
        DateTime otherDay,
        TimeZoneInfo otherTimeZone,
        PeriodUnits units)
    {
        var sut = ZonedDay.Create( day, timeZone );
        var other = ZonedDay.Create( otherDay, otherTimeZone );
        var expected = sut.Start.GetGreedyPeriodOffset( other.Start, units );

        var result = sut.GetGreedyPeriodOffset( other, units );

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedDayTestsData.GetSetYearData ) )]
    public void SetYear_ShouldReturnTargetWithChangedYear(DateTime day, TimeZoneInfo timeZone, int newYear, DateTime expectedDay)
    {
        var sut = ZonedDay.Create( day, timeZone );
        var expected = ZonedDay.Create( expectedDay, timeZone );

        var result = sut.SetYear( newYear );

        result.Should().BeEquivalentTo( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedDayTestsData.GetSetYearThrowData ) )]
    public void SetYear_ShouldThrowArgumentOutOfRangeException_WhenYearIsInvalid(DateTime day, TimeZoneInfo timeZone, int newYear)
    {
        var sut = ZonedDay.Create( day, timeZone );
        var action = Lambda.Of( () => sut.SetYear( newYear ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [MethodData( nameof( ZonedDayTestsData.GetSetMonthData ) )]
    public void SetMonth_ShouldReturnTargetWithChangedMonth(
        DateTime day,
        TimeZoneInfo timeZone,
        IsoMonthOfYear newMonth,
        DateTime expectedDay)
    {
        var sut = ZonedDay.Create( day, timeZone );
        var expected = ZonedDay.Create( expectedDay, timeZone );

        var result = sut.SetMonth( newMonth );

        result.Should().BeEquivalentTo( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedDayTestsData.GetSetDayOfMonthData ) )]
    public void SetDayOfMonth_ShouldReturnTargetWithChangedDayOfMonth(
        DateTime day,
        TimeZoneInfo timeZone,
        int newDay,
        DateTime expectedDay)
    {
        var sut = ZonedDay.Create( day, timeZone );
        var expected = ZonedDay.Create( expectedDay, timeZone );

        var result = sut.SetDayOfMonth( newDay );

        result.Should().BeEquivalentTo( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedDayTestsData.GetSetDayOfMonthThrowData ) )]
    public void SetDayOfMonth_ShouldThrowArgumentOutOfRangeException_WhenDayIsInvalid(DateTime day, TimeZoneInfo timeZone, int newDay)
    {
        var sut = ZonedDateTime.Create( day, timeZone );
        var action = Lambda.Of( () => sut.SetDayOfMonth( newDay ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [MethodData( nameof( ZonedDayTestsData.GetSetDayOfYearData ) )]
    public void SetDayOfYear_ShouldReturnTargetWithChangedDayOfYear(
        DateTime day,
        TimeZoneInfo timeZone,
        int newDay,
        DateTime expectedDay)
    {
        var sut = ZonedDay.Create( day, timeZone );
        var expected = ZonedDay.Create( expectedDay, timeZone );

        var result = sut.SetDayOfYear( newDay );

        result.Should().BeEquivalentTo( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedDayTestsData.GetSetDayOfYearThrowData ) )]
    public void SetDayOfYear_ShouldThrowArgumentOutOfRangeException_WhenDayIsInvalid(DateTime day, TimeZoneInfo timeZone, int newDay)
    {
        var sut = ZonedDateTime.Create( day, timeZone );
        var action = Lambda.Of( () => sut.SetDayOfYear( newDay ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [MethodData( nameof( ZonedDayTestsData.GetGetDateTimeData ) )]
    public void GetDateTime_ShouldReturnDayWithAddedTimeOfDay(
        DateTime day,
        TimeZoneInfo timeZone,
        TimeOfDay timeOfDay,
        DateTime expectedValue)
    {
        var sut = ZonedDay.Create( day, timeZone );
        var expected = ZonedDateTime.Create( expectedValue, timeZone );

        var result = sut.GetDateTime( timeOfDay );

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedDayTestsData.GetGetDateTimeThrowData ) )]
    public void GetDateTime_ShouldThrowInvalidZonedDateTimeException_WhenTimeIsInvalid(
        DateTime day,
        TimeZoneInfo timeZone,
        TimeOfDay timeOfDay)
    {
        var sut = ZonedDay.Create( day, timeZone );

        var action = Lambda.Of( () => sut.GetDateTime( timeOfDay ) );

        action.Should()
            .ThrowExactly<InvalidZonedDateTimeException>()
            .AndMatch( e => e.DateTime == day + (TimeSpan)timeOfDay && ReferenceEquals( e.TimeZone, timeZone ) );
    }

    [Theory]
    [MethodData( nameof( ZonedDayTestsData.GetGetDateTimeData ) )]
    public void TryGetDateTime_ShouldReturnDayWithAddedTimeOfDay(
        DateTime day,
        TimeZoneInfo timeZone,
        TimeOfDay timeOfDay,
        DateTime expectedValue)
    {
        var sut = ZonedDay.Create( day, timeZone );
        var expected = ZonedDateTime.Create( expectedValue, timeZone );

        var result = sut.TryGetDateTime( timeOfDay );

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedDayTestsData.GetGetDateTimeThrowData ) )]
    public void TryGetDateTime_ShouldReturnNull_WhenTimeIsInvalid(
        DateTime day,
        TimeZoneInfo timeZone,
        TimeOfDay timeOfDay)
    {
        var sut = ZonedDay.Create( day, timeZone );
        var result = sut.TryGetDateTime( timeOfDay );
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
    public void GetWeek_ShouldBeEquivalentToZonedWeekCreate(IsoDayOfWeek weekStart)
    {
        var dateTime = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var sut = ZonedDay.Create( dateTime, timeZone );
        var expected = ZonedWeek.Create( dateTime, timeZone, weekStart );

        var result = sut.GetWeek( weekStart );

        result.Should().Be( expected );
    }

    [Fact]
    public void GetMonth_ShouldBeEquivalentToZonedMonthCreate()
    {
        var dateTime = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var sut = ZonedDay.Create( dateTime, timeZone );
        var expected = ZonedMonth.Create( dateTime, timeZone );

        var result = sut.GetMonth();

        result.Should().Be( expected );
    }

    [Fact]
    public void GetYear_ShouldBeEquivalentToZonedYearCreate()
    {
        var dateTime = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var sut = ZonedDay.Create( dateTime, timeZone );
        var expected = ZonedYear.Create( dateTime, timeZone );

        var result = sut.GetYear();

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedDayTestsData.GetGetIntersectingInvalidityRangeData ) )]
    public void GetIntersectingInvalidityRange_ShouldReturnCorrectIntersectingInvalidityRange(
        DateTime day,
        TimeZoneInfo timeZone,
        DateTime expectedRangeStart,
        DateTime expectedRangeEnd)
    {
        var sut = ZonedDay.Create( day, timeZone );
        var result = sut.GetIntersectingInvalidityRange();
        result.Should().Be( Bounds.Create( expectedRangeStart, expectedRangeEnd ) );
    }

    [Theory]
    [MethodData( nameof( ZonedDayTestsData.GetGetIntersectingInvalidityRangeNullData ) )]
    public void GetIntersectingInvalidityRange_ShouldReturnNull_WhenNoInvalidityRangeIntersectsDay(DateTime day, TimeZoneInfo timeZone)
    {
        var sut = ZonedDay.Create( day, timeZone );
        var result = sut.GetIntersectingInvalidityRange();
        result.Should().BeNull();
    }

    [Theory]
    [MethodData( nameof( ZonedDayTestsData.GetGetIntersectingAmbiguityRangeData ) )]
    public void GetIntersectingInvalidityRange_ShouldReturnCorrectIntersectingAmbiguityRange(
        DateTime day,
        TimeZoneInfo timeZone,
        DateTime expectedRangeStart,
        DateTime expectedRangeEnd)
    {
        var sut = ZonedDay.Create( day, timeZone );
        var result = sut.GetIntersectingAmbiguityRange();
        result.Should().Be( Bounds.Create( expectedRangeStart, expectedRangeEnd ) );
    }

    [Theory]
    [MethodData( nameof( ZonedDayTestsData.GetGetIntersectingAmbiguityRangeNullData ) )]
    public void GetIntersectingAmbiguityRange_ShouldReturnNull_WhenNoAmbiguityRangeIntersectsDay(DateTime day, TimeZoneInfo timeZone)
    {
        var sut = ZonedDay.Create( day, timeZone );
        var result = sut.GetIntersectingAmbiguityRange();
        result.Should().BeNull();
    }

    [Fact]
    public void ToBounds_ShouldReturnBoundsFromStartToEnd()
    {
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var dateTime = Fixture.Create<DateTime>();
        var sut = ZonedDay.Create( dateTime, timeZone );

        var result = sut.ToBounds();

        result.Should().Be( Bounds.Create( sut.Start, sut.End ) );
    }

    [Fact]
    public void ToCheckedBounds_ShouldReturnCorrectRangeWithOneElement_WhenStartAndEndAreUnambiguous()
    {
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var dateTime = Fixture.Create<DateTime>();
        var sut = ZonedDay.Create( dateTime, timeZone );
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

        var sut = ZonedDay.Create( dateTime, timeZone );
        var expected = BoundsRange.Create(
            new[]
            {
                Bounds.Create( sut.Start, sut.Start.Add( new Duration( 0, 39, 59, 999, 9999 ) ) ),
                Bounds.Create( sut.Start.Add( Duration.FromHours( 1 ) ), sut.End )
            } );

        var result = sut.ToCheckedBounds();

        result.Should().BeSequentiallyEqualTo( expected );
    }

    [Fact]
    public void ToCheckedBounds_ShouldReturnCorrectRangeWithTwoElements_WhenEndIsAmbiguous()
    {
        var dateTime = new DateTime( 2021, 8, 25 );
        var timeZone = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 4, 26, 0, 40, 0 ),
                transitionEnd: new DateTime( 1, 8, 26, 0, 40, 0 ) ) );

        var sut = ZonedDay.Create( dateTime, timeZone );
        var expected = BoundsRange.Create(
            new[]
            {
                Bounds.Create( sut.Start, sut.End.Subtract( Duration.FromHours( 1 ) ) ),
                Bounds.Create( sut.End.Subtract( new Duration( 0, 19, 59, 999, 9999 ) ), sut.End )
            } );

        var result = sut.ToCheckedBounds();

        result.Should().BeSequentiallyEqualTo( expected );
    }

    [Fact]
    public void ZonedDateTimeConversionOperator_ShouldReturnUnderlyingStart()
    {
        var dateTime = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var sut = ZonedDay.Create( dateTime, timeZone );

        var result = (ZonedDateTime)sut;

        result.Should().Be( sut.Start );
    }

    [Fact]
    public void DateTimeConversionOperator_ShouldReturnUnderlyingStartValue()
    {
        var dateTime = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var sut = ZonedDay.Create( dateTime, timeZone );

        var result = (DateTime)sut;

        result.Should().Be( sut.Start.Value );
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
            ticks: Fixture.Create<short>() );

        var dateTime = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );

        var sut = ZonedDay.Create( dateTime, timeZone );
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
            ticks: Fixture.Create<short>() );

        var dateTime = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );

        var sut = ZonedDay.Create( dateTime, timeZone );
        var expected = sut.Add( -periodToSubtract );

        var result = sut - periodToSubtract;

        result.Should().BeEquivalentTo( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedDayTestsData.GetEqualsData ) )]
    public void EqualityOperator_ShouldReturnCorrectResult(
        DateTime dt1,
        TimeZoneInfo tz1,
        DateTime dt2,
        TimeZoneInfo tz2,
        bool expected)
    {
        var a = ZonedDay.Create( dt1, tz1 );
        var b = ZonedDay.Create( dt2, tz2 );

        var result = a == b;

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedDayTestsData.GetNotEqualsData ) )]
    public void InequalityOperator_ShouldReturnCorrectResult(
        DateTime dt1,
        TimeZoneInfo tz1,
        DateTime dt2,
        TimeZoneInfo tz2,
        bool expected)
    {
        var a = ZonedDay.Create( dt1, tz1 );
        var b = ZonedDay.Create( dt2, tz2 );

        var result = a != b;

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedDayTestsData.GetGreaterThanComparisonData ) )]
    public void GreaterThanOperator_ShouldReturnCorrectResult(
        DateTime dt1,
        TimeZoneInfo tz1,
        DateTime dt2,
        TimeZoneInfo tz2,
        bool expected)
    {
        var a = ZonedDay.Create( dt1, tz1 );
        var b = ZonedDay.Create( dt2, tz2 );

        var result = a > b;

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedDayTestsData.GetLessThanOrEqualToComparisonData ) )]
    public void LessThanOrEqualToOperator_ShouldReturnCorrectResult(
        DateTime dt1,
        TimeZoneInfo tz1,
        DateTime dt2,
        TimeZoneInfo tz2,
        bool expected)
    {
        var a = ZonedDay.Create( dt1, tz1 );
        var b = ZonedDay.Create( dt2, tz2 );

        var result = a <= b;

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedDayTestsData.GetLessThanComparisonData ) )]
    public void LessThanOperator_ShouldReturnCorrectResult(
        DateTime dt1,
        TimeZoneInfo tz1,
        DateTime dt2,
        TimeZoneInfo tz2,
        bool expected)
    {
        var a = ZonedDay.Create( dt1, tz1 );
        var b = ZonedDay.Create( dt2, tz2 );

        var result = a < b;

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedDayTestsData.GetGreaterThanOrEqualToComparisonData ) )]
    public void GreaterThanOrEqualToOperator_ShouldReturnCorrectResult(
        DateTime dt1,
        TimeZoneInfo tz1,
        DateTime dt2,
        TimeZoneInfo tz2,
        bool expected)
    {
        var a = ZonedDay.Create( dt1, tz1 );
        var b = ZonedDay.Create( dt2, tz2 );

        var result = a >= b;

        result.Should().Be( expected );
    }

    private static void AssertDateProperties(ZonedDay result)
    {
        result.Year.Should().Be( result.Start.Year );
        result.Month.Should().Be( result.Start.Month );
        result.DayOfMonth.Should().Be( result.Start.DayOfMonth );
        result.DayOfYear.Should().Be( result.Start.DayOfYear );
        result.DayOfWeek.Should().Be( result.Start.DayOfWeek );
    }
}
