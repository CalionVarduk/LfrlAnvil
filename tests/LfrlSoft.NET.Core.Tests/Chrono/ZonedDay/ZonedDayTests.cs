using System;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlSoft.NET.TestExtensions;
using LfrlSoft.NET.TestExtensions.Attributes;
using Xunit;
using AutoFixture;
using LfrlSoft.NET.Core.Chrono;
using LfrlSoft.NET.Core.Chrono.Exceptions;
using LfrlSoft.NET.Core.Chrono.Extensions;
using LfrlSoft.NET.Core.Functional;
using LfrlSoft.NET.Core.Tests.Chrono.ZonedDateTime;

namespace LfrlSoft.NET.Core.Tests.Chrono.ZonedDay
{
    [TestClass( typeof( ZonedDayTestsData ) )]
    public class ZonedDayTests : TestsBase
    {
        [Fact]
        public void Default_ShouldReturnStartOfUnixEpochDayInUtcTimeZone()
        {
            var result = default( Core.Chrono.ZonedDay );
            var expectedStart = Core.Chrono.ZonedDateTime.CreateUtc( DateTime.UnixEpoch );
            var expectedEnd = Core.Chrono.ZonedDateTime.CreateUtc( DateTime.UnixEpoch.GetEndOfDay() );

            using ( new AssertionScope() )
            {
                result.Start.Should().Be( expectedStart );
                result.End.Should().Be( expectedEnd );
                AssertDateProperties( result );
                result.TimeZone.Should().Be( TimeZoneInfo.Utc );
                result.Duration.Should().Be( Core.Chrono.Duration.FromHours( 24 ) );
                result.IsLocal.Should().BeFalse();
                result.IsUtc.Should().BeTrue();
            }
        }

        [Theory]
        [MethodData( nameof( ZonedDayTestsData.GetCreateData ) )]
        public void Create_ShouldReturnCorrectResult(DateTime dateTime, TimeZoneInfo timeZone)
        {
            var expectedStart = Core.Chrono.ZonedDateTime.Create( dateTime.GetStartOfDay(), timeZone );
            var expectedEnd = Core.Chrono.ZonedDateTime.Create( dateTime.GetEndOfDay(), timeZone );
            var sut = Core.Chrono.ZonedDay.Create( dateTime, timeZone );

            using ( new AssertionScope() )
            {
                sut.Start.Should().Be( expectedStart );
                sut.End.Should().Be( expectedEnd );
                AssertDateProperties( sut );
                sut.TimeZone.Should().Be( timeZone );
                sut.Duration.Should().Be( Core.Chrono.Duration.FromHours( 24 ) );
                sut.IsLocal.Should().BeFalse();
                sut.IsUtc.Should().BeFalse();
            }
        }

        [Theory]
        [MethodData( nameof( ZonedDayTestsData.GetCreateWithContainedInvalidityRangeData ) )]
        public void Create_WithContainedInvalidityRange_ShouldReturnCorrectResult(
            DateTime dateTime,
            TimeZoneInfo timeZone,
            Core.Chrono.Duration expectedDuration)
        {
            var expectedStart = Core.Chrono.ZonedDateTime.Create( dateTime.GetStartOfDay(), timeZone );
            var expectedEnd = Core.Chrono.ZonedDateTime.Create( dateTime.GetEndOfDay(), timeZone );

            var sut = Core.Chrono.ZonedDay.Create( dateTime, timeZone );

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
            Core.Chrono.Duration expectedDuration)
        {
            var expectedStart = Core.Chrono.ZonedDateTime.Create( dateTime.GetStartOfDay(), timeZone );
            var expectedEnd = Core.Chrono.ZonedDateTime.Create( dateTime.GetEndOfDay(), timeZone );

            var sut = Core.Chrono.ZonedDay.Create( dateTime, timeZone );

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
            Core.Chrono.Duration expectedDuration)
        {
            var expectedStart = Core.Chrono.ZonedDateTime.Create( expectedStartValue, timeZone );
            var expectedEnd = Core.Chrono.ZonedDateTime.Create( dateTime.GetEndOfDay(), timeZone );

            var sut = Core.Chrono.ZonedDay.Create( dateTime, timeZone );

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
            Core.Chrono.Duration expectedDuration)
        {
            var expectedStart = Core.Chrono.ZonedDateTime.Create( dateTime.GetStartOfDay(), timeZone );
            var expectedEnd = Core.Chrono.ZonedDateTime.Create( expectedEndValue, timeZone );

            var sut = Core.Chrono.ZonedDay.Create( dateTime, timeZone );

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
            Core.Chrono.Duration expectedDuration)
        {
            var expectedStart = Core.Chrono.ZonedDateTime.Create( dateTime.GetStartOfDay(), timeZone );
            if ( forceInDaylightSavingMode )
                expectedStart = expectedStart.GetOppositeAmbiguousDateTime() ?? expectedStart;

            var expectedEnd = Core.Chrono.ZonedDateTime.Create( dateTime.GetEndOfDay(), timeZone );

            var sut = Core.Chrono.ZonedDay.Create( dateTime, timeZone );

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
            Core.Chrono.Duration expectedDuration)
        {
            var expectedStart = Core.Chrono.ZonedDateTime.Create( dateTime.GetStartOfDay(), timeZone );
            var expectedEnd = Core.Chrono.ZonedDateTime.Create( dateTime.GetEndOfDay(), timeZone );
            if ( forceInDaylightSavingMode )
                expectedEnd = expectedEnd.GetOppositeAmbiguousDateTime() ?? expectedEnd;

            var sut = Core.Chrono.ZonedDay.Create( dateTime, timeZone );

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
            Core.Chrono.Duration expectedDuration)
        {
            var expectedStart = Core.Chrono.ZonedDateTime.Create( expectedStartValue, timeZone );
            var expectedEnd = Core.Chrono.ZonedDateTime.Create( dateTime.GetEndOfDay(), timeZone );
            if ( forceInDaylightSavingMode )
                expectedEnd = expectedEnd.GetOppositeAmbiguousDateTime() ?? expectedEnd;

            var sut = Core.Chrono.ZonedDay.Create( dateTime, timeZone );

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
            Core.Chrono.Duration expectedDuration)
        {
            var expectedStart = Core.Chrono.ZonedDateTime.Create( dateTime.GetStartOfDay(), timeZone );
            if ( forceInDaylightSavingMode )
                expectedStart = expectedStart.GetOppositeAmbiguousDateTime() ?? expectedStart;

            var expectedEnd = Core.Chrono.ZonedDateTime.Create( expectedEndValue, timeZone );

            var sut = Core.Chrono.ZonedDay.Create( dateTime, timeZone );

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
        public void Create_WithZonedDateTime_ShouldBeAnAliasForCreateWithDateTimeAndTimeZoneInfo()
        {
            var dateTime = Fixture.Create<DateTime>();
            var timeZoneOffset = Fixture.Create<int>() % 12;
            var timeZone = ZonedDateTimeTestsData.GetTimeZone( $"{timeZoneOffset}", timeZoneOffset );
            var source = Core.Chrono.ZonedDateTime.Create( dateTime, timeZone );
            var expected = Core.Chrono.ZonedDay.Create( dateTime, timeZone );

            var result = Core.Chrono.ZonedDay.Create( source );

            result.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void CreateUtc_WithTimestamp_ShouldReturnCorrectDayInUtc()
        {
            var timestamp = new Core.Chrono.Timestamp( Fixture.Create<DateTime>() );
            var expectedStart = Core.Chrono.ZonedDateTime.CreateUtc( timestamp.UtcValue.GetStartOfDay() );
            var expectedEnd = Core.Chrono.ZonedDateTime.CreateUtc( timestamp.UtcValue.GetEndOfDay() );

            var result = Core.Chrono.ZonedDay.CreateUtc( timestamp );

            using ( new AssertionScope() )
            {
                result.Start.Should().Be( expectedStart );
                result.End.Should().Be( expectedEnd );
                AssertDateProperties( result );
                result.TimeZone.Should().Be( TimeZoneInfo.Utc );
                result.Duration.Should().Be( Core.Chrono.Duration.FromHours( 24 ) );
                result.IsLocal.Should().BeFalse();
                result.IsUtc.Should().BeTrue();
            }
        }

        [Fact]
        public void CreateUtc_WithDateTime_ShouldReturnCorrectDayInUtc()
        {
            var dateTime = Fixture.Create<DateTime>();
            var expectedStart = Core.Chrono.ZonedDateTime.CreateUtc( dateTime.GetStartOfDay() );
            var expectedEnd = Core.Chrono.ZonedDateTime.CreateUtc( dateTime.GetEndOfDay() );

            var result = Core.Chrono.ZonedDay.CreateUtc( dateTime );

            using ( new AssertionScope() )
            {
                result.Start.Should().Be( expectedStart );
                result.End.Should().Be( expectedEnd );
                AssertDateProperties( result );
                result.TimeZone.Should().Be( TimeZoneInfo.Utc );
                result.Duration.Should().Be( Core.Chrono.Duration.FromHours( 24 ) );
                result.IsLocal.Should().BeFalse();
                result.IsUtc.Should().BeTrue();
            }
        }

        [Fact]
        public void CreateLocal_ShouldReturnCorrectDayInLocal()
        {
            var dateTime = Fixture.Create<DateTime>();
            var expected = Core.Chrono.ZonedDay.Create( dateTime, TimeZoneInfo.Local );

            var result = Core.Chrono.ZonedDay.CreateLocal( dateTime );

            result.Should().BeEquivalentTo( expected );
        }

        [Theory]
        [MethodData( nameof( ZonedDayTestsData.GetToStringData ) )]
        public void ToString_ShouldReturnCorrectResult(DateTime day, TimeZoneInfo timeZone, string expected)
        {
            var sut = Core.Chrono.ZonedDay.Create( day, timeZone );
            var result = sut.ToString();
            result.Should().Be( expected );
        }

        [Fact]
        public void GetHashCode_ShouldReturnCorrectResult()
        {
            var dateTime = Fixture.Create<DateTime>();
            var timeZoneOffset = Fixture.Create<int>() % 12;
            var timeZone = ZonedDateTimeTestsData.GetTimeZone( $"{timeZoneOffset}", timeZoneOffset );
            var sut = Core.Chrono.ZonedDay.Create( dateTime, timeZone );
            var expected = Core.Hash.Default.Add( sut.Start.Timestamp ).Add( sut.End.Timestamp ).Add( sut.TimeZone.Id ).Value;

            var result = sut.GetHashCode();

            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( ZonedDayTestsData.GetEqualsData ) )]
        public void Equals_ShouldReturnCorrectResult(DateTime dt1, TimeZoneInfo tz1, DateTime dt2, TimeZoneInfo tz2, bool expected)
        {
            var a = Core.Chrono.ZonedDay.Create( dt1, tz1 );
            var b = Core.Chrono.ZonedDay.Create( dt2, tz2 );

            var result = a.Equals( b );

            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( ZonedDayTestsData.GetCompareToData ) )]
        public void CompareTo_ShouldReturnCorrectResult(DateTime dt1, TimeZoneInfo tz1, DateTime dt2, TimeZoneInfo tz2, int expectedSign)
        {
            var a = Core.Chrono.ZonedDay.Create( dt1, tz1 );
            var b = Core.Chrono.ZonedDay.Create( dt2, tz2 );

            var result = a.CompareTo( b );

            Math.Sign( result ).Should().Be( expectedSign );
        }

        [Fact]
        public void ToTimeZone_ShouldReturnCorrectResult()
        {
            var dateTime = Fixture.Create<DateTime>();
            var timeZoneOffset = Fixture.Create<int>() % 12;
            var timeZone = ZonedDateTimeTestsData.GetTimeZone( $"{timeZoneOffset}", timeZoneOffset );
            var targetTimeZoneOffset = Fixture.Create<int>() % 12;
            var targetTimeZone = ZonedDateTimeTestsData.GetTimeZone( $"{targetTimeZoneOffset}", targetTimeZoneOffset );

            var sut = Core.Chrono.ZonedDay.Create( dateTime, timeZone );
            var expected = Core.Chrono.ZonedDay.Create( dateTime, targetTimeZone );

            var result = sut.ToTimeZone( targetTimeZone );

            result.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void ToUtcTimeZone_ShouldReturnCorrectResult()
        {
            var dateTime = Fixture.Create<DateTime>();
            var timeZoneOffset = Fixture.Create<int>() % 12;
            var timeZone = ZonedDateTimeTestsData.GetTimeZone( $"{timeZoneOffset}", timeZoneOffset );

            var sut = Core.Chrono.ZonedDay.Create( dateTime, timeZone );
            var expected = Core.Chrono.ZonedDay.Create( dateTime, TimeZoneInfo.Utc );

            var result = sut.ToUtcTimeZone();

            result.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void ToLocalTimeZone_ShouldReturnCorrectResult()
        {
            var dateTime = Fixture.Create<DateTime>();
            var timeZoneOffset = Fixture.Create<int>() % 12;
            var timeZone = ZonedDateTimeTestsData.GetTimeZone( $"{timeZoneOffset}", timeZoneOffset );

            var sut = Core.Chrono.ZonedDay.Create( dateTime, timeZone );
            var expected = Core.Chrono.ZonedDay.Create( dateTime, TimeZoneInfo.Local );

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
            var dateTime = Core.Chrono.ZonedDateTime.Create( dateTimeValue, timeZone );
            var sut = Core.Chrono.ZonedDay.Create( day, dayTimeZone );

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
            var dateTime = Core.Chrono.ZonedDateTime.Create( dateTimeValue, timeZone );
            if ( forceInDaylightSavingMode )
                dateTime = dateTime.GetOppositeAmbiguousDateTime() ?? dateTime;

            var sut = Core.Chrono.ZonedDay.Create( day, timeZone );

            var result = sut.Contains( dateTime );

            result.Should().Be( expected );
        }

        [Fact]
        public void GetNext_ShouldReturnTheNextDay()
        {
            var dateTime = Fixture.Create<DateTime>();
            var timeZoneOffset = Fixture.Create<int>() % 12;
            var timeZone = ZonedDateTimeTestsData.GetTimeZone( $"{timeZoneOffset}", timeZoneOffset );

            var sut = Core.Chrono.ZonedDay.Create( dateTime, timeZone );
            var expected = sut.AddDays( 1 );

            var result = sut.GetNext();

            result.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void GetPrevious_ShouldReturnThePreviousDay()
        {
            var dateTime = Fixture.Create<DateTime>();
            var timeZoneOffset = Fixture.Create<int>() % 12;
            var timeZone = ZonedDateTimeTestsData.GetTimeZone( $"{timeZoneOffset}", timeZoneOffset );

            var sut = Core.Chrono.ZonedDay.Create( dateTime, timeZone );
            var expected = sut.AddDays( -1 );

            var result = sut.GetPrevious();

            result.Should().BeEquivalentTo( expected );
        }

        [Theory]
        [MethodData( nameof( ZonedDayTestsData.GetAddDaysData ) )]
        public void AddDays_ShouldReturnCorrectResult(DateTime day, TimeZoneInfo timeZone, int daysToAdd, DateTime expectedDay)
        {
            var sut = Core.Chrono.ZonedDay.Create( day, timeZone );
            var expected = Core.Chrono.ZonedDay.Create( expectedDay, timeZone );

            var result = sut.AddDays( daysToAdd );

            result.Should().BeEquivalentTo( expected );
        }

        [Theory]
        [MethodData( nameof( ZonedDayTestsData.GetAddData ) )]
        public void Add_ShouldReturnCorrectResult(
            DateTime day,
            TimeZoneInfo timeZone,
            Core.Chrono.Period period,
            DateTime expectedDay)
        {
            var sut = Core.Chrono.ZonedDay.Create( day, timeZone );
            var expected = Core.Chrono.ZonedDay.Create( expectedDay, timeZone );

            var result = sut.Add( period );

            result.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void SubtractDays_ShouldBeAnAliasForAddDays()
        {
            var dayCount = Fixture.Create<sbyte>();
            var dateTime = Fixture.Create<DateTime>();
            var timeZoneOffset = Fixture.Create<int>() % 12;
            var timeZone = ZonedDateTimeTestsData.GetTimeZone( $"{timeZoneOffset}", timeZoneOffset );

            var sut = Core.Chrono.ZonedDay.Create( dateTime, timeZone );
            var expected = sut.AddDays( -dayCount );

            var result = sut.SubtractDays( dayCount );

            result.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void Subtract_ShouldBeAnAliasForAdd()
        {
            var periodToSubtract = new Core.Chrono.Period(
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
            var timeZoneOffset = Fixture.Create<int>() % 12;
            var timeZone = ZonedDateTimeTestsData.GetTimeZone( $"{timeZoneOffset}", timeZoneOffset );

            var sut = Core.Chrono.ZonedDay.Create( dateTime, timeZone );
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
            var sut = Core.Chrono.ZonedDay.Create( day, timeZone );
            var other = Core.Chrono.ZonedDay.Create( otherDay, otherTimeZone );
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
            var sut = Core.Chrono.ZonedDay.Create( day, timeZone );
            var other = Core.Chrono.ZonedDay.Create( otherDay, otherTimeZone );
            var expected = sut.Start.GetGreedyPeriodOffset( other.Start, units );

            var result = sut.GetGreedyPeriodOffset( other, units );

            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( ZonedDayTestsData.GetSetYearData ) )]
        public void SetYear_ShouldReturnTargetWithChangedYear(DateTime day, TimeZoneInfo timeZone, int newYear, DateTime expectedDay)
        {
            var sut = Core.Chrono.ZonedDay.Create( day, timeZone );
            var expected = Core.Chrono.ZonedDay.Create( expectedDay, timeZone );

            var result = sut.SetYear( newYear );

            result.Should().BeEquivalentTo( expected );
        }

        [Theory]
        [MethodData( nameof( ZonedDayTestsData.GetSetYearThrowData ) )]
        public void SetYear_ShouldThrowArgumentOutOfRangeException_WhenYearIsInvalid(DateTime day, TimeZoneInfo timeZone, int newYear)
        {
            var sut = Core.Chrono.ZonedDay.Create( day, timeZone );
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
            var sut = Core.Chrono.ZonedDay.Create( day, timeZone );
            var expected = Core.Chrono.ZonedDay.Create( expectedDay, timeZone );

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
            var sut = Core.Chrono.ZonedDay.Create( day, timeZone );
            var expected = Core.Chrono.ZonedDay.Create( expectedDay, timeZone );

            var result = sut.SetDayOfMonth( newDay );

            result.Should().BeEquivalentTo( expected );
        }

        [Theory]
        [MethodData( nameof( ZonedDayTestsData.GetSetDayOfMonthThrowData ) )]
        public void SetDayOfMonth_ShouldThrowArgumentOutOfRangeException_WhenDayIsInvalid(DateTime day, TimeZoneInfo timeZone, int newDay)
        {
            var sut = Core.Chrono.ZonedDateTime.Create( day, timeZone );
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
            var sut = Core.Chrono.ZonedDay.Create( day, timeZone );
            var expected = Core.Chrono.ZonedDay.Create( expectedDay, timeZone );

            var result = sut.SetDayOfYear( newDay );

            result.Should().BeEquivalentTo( expected );
        }

        [Theory]
        [MethodData( nameof( ZonedDayTestsData.GetSetDayOfYearThrowData ) )]
        public void SetDayOfYear_ShouldThrowArgumentOutOfRangeException_WhenDayIsInvalid(DateTime day, TimeZoneInfo timeZone, int newDay)
        {
            var sut = Core.Chrono.ZonedDateTime.Create( day, timeZone );
            var action = Lambda.Of( () => sut.SetDayOfYear( newDay ) );
            action.Should().ThrowExactly<ArgumentOutOfRangeException>();
        }

        [Theory]
        [MethodData( nameof( ZonedDayTestsData.GetGetDateTimeData ) )]
        public void GetDateTime_ShouldReturnDayWithAddedTimeOfDay(
            DateTime day,
            TimeZoneInfo timeZone,
            Core.Chrono.TimeOfDay timeOfDay,
            DateTime expectedValue)
        {
            var sut = Core.Chrono.ZonedDay.Create( day, timeZone );
            var expected = Core.Chrono.ZonedDateTime.Create( expectedValue, timeZone );

            var result = sut.GetDateTime( timeOfDay );

            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( ZonedDayTestsData.GetGetDateTimeThrowData ) )]
        public void GetDateTime_ShouldThrowInvalidZonedDateTimeException_WhenTimeIsInvalid(
            DateTime day,
            TimeZoneInfo timeZone,
            Core.Chrono.TimeOfDay timeOfDay)
        {
            var sut = Core.Chrono.ZonedDay.Create( day, timeZone );
            var action = Lambda.Of( () => sut.GetDateTime( timeOfDay ) );
            action.Should().ThrowExactly<InvalidZonedDateTimeException>();
        }

        [Theory]
        [MethodData( nameof( ZonedDayTestsData.GetGetDateTimeData ) )]
        public void TryGetDateTime_ShouldReturnDayWithAddedTimeOfDay(
            DateTime day,
            TimeZoneInfo timeZone,
            Core.Chrono.TimeOfDay timeOfDay,
            DateTime expectedValue)
        {
            var sut = Core.Chrono.ZonedDay.Create( day, timeZone );
            var expected = Core.Chrono.ZonedDateTime.Create( expectedValue, timeZone );

            var result = sut.TryGetDateTime( timeOfDay );

            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( ZonedDayTestsData.GetGetDateTimeThrowData ) )]
        public void TryGetDateTime_ShouldReturnNull_WhenTimeIsInvalid(
            DateTime day,
            TimeZoneInfo timeZone,
            Core.Chrono.TimeOfDay timeOfDay)
        {
            var sut = Core.Chrono.ZonedDay.Create( day, timeZone );
            var result = sut.TryGetDateTime( timeOfDay );
            result.Should().BeNull();
        }

        [Theory]
        [MethodData( nameof( ZonedDayTestsData.GetGetIntersectingInvalidityRangeData ) )]
        public void GetIntersectingInvalidityRange_ShouldReturnCorrectIntersectingInvalidityRange(
            DateTime day,
            TimeZoneInfo timeZone,
            DateTime expectedRangeStart,
            DateTime expectedRangeEnd)
        {
            var sut = Core.Chrono.ZonedDay.Create( day, timeZone );
            var result = sut.GetIntersectingInvalidityRange();
            result.Should().Be( Core.Bounds.Create( expectedRangeStart, expectedRangeEnd ) );
        }

        [Theory]
        [MethodData( nameof( ZonedDayTestsData.GetGetIntersectingInvalidityRangeNullData ) )]
        public void GetIntersectingInvalidityRange_ShouldReturnNull_WhenNoInvalidityRangeIntersectsDay(DateTime day, TimeZoneInfo timeZone)
        {
            var sut = Core.Chrono.ZonedDay.Create( day, timeZone );
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
            var sut = Core.Chrono.ZonedDay.Create( day, timeZone );
            var result = sut.GetIntersectingAmbiguityRange();
            result.Should().Be( Core.Bounds.Create( expectedRangeStart, expectedRangeEnd ) );
        }

        [Theory]
        [MethodData( nameof( ZonedDayTestsData.GetGetIntersectingAmbiguityRangeNullData ) )]
        public void GetIntersectingAmbiguityRange_ShouldReturnNull_WhenNoAmbiguityRangeIntersectsDay(DateTime day, TimeZoneInfo timeZone)
        {
            var sut = Core.Chrono.ZonedDay.Create( day, timeZone );
            var result = sut.GetIntersectingAmbiguityRange();
            result.Should().BeNull();
        }

        [Fact]
        public void ZonedDateTimeConversionOperator_ShouldReturnUnderlyingStart()
        {
            var dateTime = Fixture.Create<DateTime>();
            var timeZoneOffset = Fixture.Create<int>() % 12;
            var timeZone = ZonedDateTimeTestsData.GetTimeZone( $"{timeZoneOffset}", timeZoneOffset );
            var sut = Core.Chrono.ZonedDay.Create( dateTime, timeZone );

            var result = (Core.Chrono.ZonedDateTime)sut;

            result.Should().Be( sut.Start );
        }

        [Fact]
        public void DateTimeConversionOperator_ShouldReturnUnderlyingStartValue()
        {
            var dateTime = Fixture.Create<DateTime>();
            var timeZoneOffset = Fixture.Create<int>() % 12;
            var timeZone = ZonedDateTimeTestsData.GetTimeZone( $"{timeZoneOffset}", timeZoneOffset );
            var sut = Core.Chrono.ZonedDay.Create( dateTime, timeZone );

            var result = (DateTime)sut;

            result.Should().Be( sut.Start.Value );
        }

        [Fact]
        public void AddOperator_ShouldBeAnAliasForAdd()
        {
            var periodToSubtract = new Core.Chrono.Period(
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
            var timeZoneOffset = Fixture.Create<int>() % 12;
            var timeZone = ZonedDateTimeTestsData.GetTimeZone( $"{timeZoneOffset}", timeZoneOffset );

            var sut = Core.Chrono.ZonedDay.Create( dateTime, timeZone );
            var expected = sut.Add( periodToSubtract );

            var result = sut + periodToSubtract;

            result.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void SubtractOperator_ShouldBeAnAliasForAdd()
        {
            var periodToSubtract = new Core.Chrono.Period(
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
            var timeZoneOffset = Fixture.Create<int>() % 12;
            var timeZone = ZonedDateTimeTestsData.GetTimeZone( $"{timeZoneOffset}", timeZoneOffset );

            var sut = Core.Chrono.ZonedDay.Create( dateTime, timeZone );
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
            var a = Core.Chrono.ZonedDay.Create( dt1, tz1 );
            var b = Core.Chrono.ZonedDay.Create( dt2, tz2 );

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
            var a = Core.Chrono.ZonedDay.Create( dt1, tz1 );
            var b = Core.Chrono.ZonedDay.Create( dt2, tz2 );

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
            var a = Core.Chrono.ZonedDay.Create( dt1, tz1 );
            var b = Core.Chrono.ZonedDay.Create( dt2, tz2 );

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
            var a = Core.Chrono.ZonedDay.Create( dt1, tz1 );
            var b = Core.Chrono.ZonedDay.Create( dt2, tz2 );

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
            var a = Core.Chrono.ZonedDay.Create( dt1, tz1 );
            var b = Core.Chrono.ZonedDay.Create( dt2, tz2 );

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
            var a = Core.Chrono.ZonedDay.Create( dt1, tz1 );
            var b = Core.Chrono.ZonedDay.Create( dt2, tz2 );

            var result = a >= b;

            result.Should().Be( expected );
        }

        private static void AssertDateProperties(Core.Chrono.ZonedDay result)
        {
            result.Year.Should().Be( result.Start.Year );
            result.Month.Should().Be( result.Start.Month );
            result.DayOfMonth.Should().Be( result.Start.DayOfMonth );
            result.DayOfYear.Should().Be( result.Start.DayOfYear );
            result.DayOfWeek.Should().Be( result.Start.DayOfWeek );
        }
    }
}
