﻿using System;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlSoft.NET.Core.Chrono;
using LfrlSoft.NET.Core.Chrono.Exceptions;
using LfrlSoft.NET.Core.Chrono.Extensions;
using LfrlSoft.NET.Core.Functional;
using LfrlSoft.NET.TestExtensions;
using LfrlSoft.NET.TestExtensions.Attributes;
using LfrlSoft.NET.TestExtensions.FluentAssertions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.ChronoTests.ZonedMonthTests
{
    [TestClass( typeof( ZonedMonthTestsData ) )]
    public class ZonedMonthTests : TestsBase
    {
        [Fact]
        public void Default_ShouldReturnStartOfUnixEpochMonthInUtcTimeZone()
        {
            var result = default( ZonedMonth );
            var expectedStart = ZonedDateTime.CreateUtc( DateTime.UnixEpoch );
            var expectedEnd = ZonedDateTime.CreateUtc( DateTime.UnixEpoch.GetEndOfMonth() );

            using ( new AssertionScope() )
            {
                result.Start.Should().Be( expectedStart );
                result.End.Should().Be( expectedEnd );
                result.Year.Should().Be( result.Start.Year );
                result.Month.Should().Be( result.Start.Month );
                result.DayCount.Should().Be( 31 );
                result.TimeZone.Should().Be( TimeZoneInfo.Utc );
                result.Duration.Should().Be( Duration.FromHours( 744 ) );
                result.IsLocal.Should().BeFalse();
                result.IsUtc.Should().BeTrue();
            }
        }

        [Theory]
        [MethodData( nameof( ZonedMonthTestsData.GetCreateData ) )]
        public void Create_ShouldReturnCorrectResult(DateTime dateTime, TimeZoneInfo timeZone, int expectedDayCount)
        {
            var expectedStart = ZonedDateTime.Create( dateTime.GetStartOfMonth(), timeZone );
            var expectedEnd = ZonedDateTime.Create( dateTime.GetEndOfMonth(), timeZone );
            var expectedDuration = Duration.FromHours( 24 * expectedDayCount );
            var sut = ZonedMonth.Create( dateTime, timeZone );

            using ( new AssertionScope() )
            {
                sut.Start.Should().Be( expectedStart );
                sut.End.Should().Be( expectedEnd );
                sut.Year.Should().Be( sut.Start.Year );
                sut.Month.Should().Be( sut.Start.Month );
                sut.DayCount.Should().Be( expectedDayCount );
                sut.TimeZone.Should().Be( timeZone );
                sut.Duration.Should().Be( expectedDuration );
                sut.IsLocal.Should().BeFalse();
                sut.IsUtc.Should().BeFalse();
            }
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

            using ( new AssertionScope() )
            {
                sut.Start.Should().Be( expectedStart );
                sut.End.Should().Be( expectedEnd );
                sut.Year.Should().Be( sut.Start.Year );
                sut.Month.Should().Be( sut.Start.Month );
                sut.DayCount.Should().Be( expectedDayCount );
                sut.TimeZone.Should().Be( timeZone );
                sut.Duration.Should().Be( expectedDuration );
                sut.IsLocal.Should().BeFalse();
                sut.IsUtc.Should().BeFalse();
            }
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

            using ( new AssertionScope() )
            {
                sut.Start.Should().Be( expectedStart );
                sut.End.Should().Be( expectedEnd );
                sut.Year.Should().Be( sut.Start.Year );
                sut.Month.Should().Be( sut.Start.Month );
                sut.DayCount.Should().Be( expectedDayCount );
                sut.TimeZone.Should().Be( timeZone );
                sut.Duration.Should().Be( expectedDuration );
                sut.IsLocal.Should().BeFalse();
                sut.IsUtc.Should().BeFalse();
            }
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

            using ( new AssertionScope() )
            {
                sut.Start.Should().Be( expectedStart );
                sut.End.Should().Be( expectedEnd );
                sut.Year.Should().Be( sut.Start.Year );
                sut.Month.Should().Be( sut.Start.Month );
                sut.DayCount.Should().Be( expectedDayCount );
                sut.TimeZone.Should().Be( timeZone );
                sut.Duration.Should().Be( expectedDuration );
                sut.IsLocal.Should().BeFalse();
                sut.IsUtc.Should().BeFalse();
            }
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

            using ( new AssertionScope() )
            {
                sut.Start.Should().Be( expectedStart );
                sut.End.Should().Be( expectedEnd );
                sut.Year.Should().Be( sut.Start.Year );
                sut.Month.Should().Be( sut.Start.Month );
                sut.DayCount.Should().Be( expectedDayCount );
                sut.TimeZone.Should().Be( timeZone );
                sut.Duration.Should().Be( expectedDuration );
                sut.IsLocal.Should().BeFalse();
                sut.IsUtc.Should().BeFalse();
            }
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

            using ( new AssertionScope() )
            {
                sut.Start.Should().Be( expectedStart );
                sut.End.Should().Be( expectedEnd );
                sut.Year.Should().Be( sut.Start.Year );
                sut.Month.Should().Be( sut.Start.Month );
                sut.DayCount.Should().Be( expectedDayCount );
                sut.TimeZone.Should().Be( timeZone );
                sut.Duration.Should().Be( expectedDuration );
                sut.IsLocal.Should().BeFalse();
                sut.IsUtc.Should().BeFalse();
            }
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

            using ( new AssertionScope() )
            {
                sut.Start.Should().Be( expectedStart );
                sut.End.Should().Be( expectedEnd );
                sut.Year.Should().Be( sut.Start.Year );
                sut.Month.Should().Be( sut.Start.Month );
                sut.DayCount.Should().Be( expectedDayCount );
                sut.TimeZone.Should().Be( timeZone );
                sut.Duration.Should().Be( expectedDuration );
                sut.IsLocal.Should().BeFalse();
                sut.IsUtc.Should().BeFalse();
            }
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

            using ( new AssertionScope() )
            {
                sut.Start.Should().Be( expectedStart );
                sut.End.Should().Be( expectedEnd );
                sut.Year.Should().Be( sut.Start.Year );
                sut.Month.Should().Be( sut.Start.Month );
                sut.DayCount.Should().Be( expectedDayCount );
                sut.TimeZone.Should().Be( timeZone );
                sut.Duration.Should().Be( expectedDuration );
                sut.IsLocal.Should().BeFalse();
                sut.IsUtc.Should().BeFalse();
            }
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

            using ( new AssertionScope() )
            {
                sut.Start.Should().Be( expectedStart );
                sut.End.Should().Be( expectedEnd );
                sut.Year.Should().Be( sut.Start.Year );
                sut.Month.Should().Be( sut.Start.Month );
                sut.DayCount.Should().Be( expectedDayCount );
                sut.TimeZone.Should().Be( timeZone );
                sut.Duration.Should().Be( expectedDuration );
                sut.IsLocal.Should().BeFalse();
                sut.IsUtc.Should().BeFalse();
            }
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

            using ( new AssertionScope() )
            {
                sut.Start.Should().Be( expectedStart );
                sut.End.Should().Be( expectedEnd );
                sut.Year.Should().Be( sut.Start.Year );
                sut.Month.Should().Be( sut.Start.Month );
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
            var expected = ZonedMonth.Create( dateTime, timeZone );

            var result = ZonedMonth.Create( source );

            result.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void Create_WithZonedDay_ShouldBeEquivalentToCreateWithDateTimeAndTimeZoneInfo()
        {
            var dateTime = Fixture.Create<DateTime>();
            var timeZone = TimeZoneFactory.CreateRandom( Fixture );
            var source = ZonedDay.Create( dateTime, timeZone );
            var expected = ZonedMonth.Create( dateTime, timeZone );

            var result = ZonedMonth.Create( source );

            result.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void Create_WithYearAndMonthAndTimeZone_ShouldBeEquivalentToCreateWithDateTimeAndTimeZoneInfo()
        {
            var dateTime = Fixture.Create<DateTime>();
            var timeZone = TimeZoneFactory.CreateRandom( Fixture );
            var expected = ZonedMonth.Create( dateTime, timeZone );

            var result = ZonedMonth.Create( dateTime.Year, (IsoMonthOfYear)dateTime.Month, timeZone );

            result.Should().BeEquivalentTo( expected );
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

            using ( new AssertionScope() )
            {
                result.Start.Should().Be( expectedStart );
                result.End.Should().Be( expectedEnd );
                result.Year.Should().Be( result.Start.Year );
                result.Month.Should().Be( result.Start.Month );
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
            var expectedStart = ZonedDateTime.CreateUtc( dateTime.GetStartOfMonth() );
            var expectedEnd = ZonedDateTime.CreateUtc( dateTime.GetEndOfMonth() );
            var expectedDayCount = DateTime.DaysInMonth( dateTime.Year, dateTime.Month );
            var expectedDuration = Duration.FromHours( 24 * expectedDayCount );

            var result = ZonedMonth.CreateUtc( dateTime );

            using ( new AssertionScope() )
            {
                result.Start.Should().Be( expectedStart );
                result.End.Should().Be( expectedEnd );
                result.Year.Should().Be( result.Start.Year );
                result.Month.Should().Be( result.Start.Month );
                result.DayCount.Should().Be( expectedDayCount );
                result.TimeZone.Should().Be( TimeZoneInfo.Utc );
                result.Duration.Should().Be( expectedDuration );
                result.IsLocal.Should().BeFalse();
                result.IsUtc.Should().BeTrue();
            }
        }

        [Fact]
        public void CreateUtc_WithYearAndMonth_ShouldBeEquivalentToCreateUtcWithDateTime()
        {
            var dateTime = Fixture.Create<DateTime>();
            var expected = ZonedMonth.CreateUtc( dateTime );

            var result = ZonedMonth.CreateUtc( dateTime.Year, (IsoMonthOfYear)dateTime.Month );

            result.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void CreateLocal_ShouldReturnCorrectMonthInLocal()
        {
            var dateTime = Fixture.Create<DateTime>();
            var expected = ZonedMonth.Create( dateTime, TimeZoneInfo.Local );

            var result = ZonedMonth.CreateLocal( dateTime );

            result.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void CreateLocal_WithYearAndMonth_ShouldBeEquivalentToCreateLocalWithDateTime()
        {
            var dateTime = Fixture.Create<DateTime>();
            var expected = ZonedMonth.CreateLocal( dateTime );

            var result = ZonedMonth.CreateLocal( dateTime.Year, (IsoMonthOfYear)dateTime.Month );

            result.Should().BeEquivalentTo( expected );
        }

        [Theory]
        [MethodData( nameof( ZonedMonthTestsData.GetToStringData ) )]
        public void ToString_ShouldReturnCorrectResult(DateTime month, TimeZoneInfo timeZone, string expected)
        {
            var sut = ZonedMonth.Create( month, timeZone );
            var result = sut.ToString();
            result.Should().Be( expected );
        }

        [Fact]
        public void GetHashCode_ShouldReturnCorrectResult()
        {
            var dateTime = Fixture.Create<DateTime>();
            var timeZone = TimeZoneFactory.CreateRandom( Fixture );
            var sut = ZonedMonth.Create( dateTime, timeZone );
            var expected = Hash.Default.Add( sut.Start.Timestamp ).Add( sut.End.Timestamp ).Add( sut.TimeZone.Id ).Value;

            var result = sut.GetHashCode();

            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( ZonedMonthTestsData.GetEqualsData ) )]
        public void Equals_ShouldReturnCorrectResult(DateTime dt1, TimeZoneInfo tz1, DateTime dt2, TimeZoneInfo tz2, bool expected)
        {
            var a = ZonedMonth.Create( dt1, tz1 );
            var b = ZonedMonth.Create( dt2, tz2 );

            var result = a.Equals( b );

            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( ZonedMonthTestsData.GetCompareToData ) )]
        public void CompareTo_ShouldReturnCorrectResult(DateTime dt1, TimeZoneInfo tz1, DateTime dt2, TimeZoneInfo tz2, int expectedSign)
        {
            var a = ZonedMonth.Create( dt1, tz1 );
            var b = ZonedMonth.Create( dt2, tz2 );

            var result = a.CompareTo( b );

            Math.Sign( result ).Should().Be( expectedSign );
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

            result.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void ToUtcTimeZone_ShouldReturnCorrectResult()
        {
            var dateTime = Fixture.Create<DateTime>();
            var timeZone = TimeZoneFactory.CreateRandom( Fixture );

            var sut = ZonedMonth.Create( dateTime, timeZone );
            var expected = ZonedMonth.Create( dateTime, TimeZoneInfo.Utc );

            var result = sut.ToUtcTimeZone();

            result.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void ToLocalTimeZone_ShouldReturnCorrectResult()
        {
            var dateTime = Fixture.Create<DateTime>();
            var timeZone = TimeZoneFactory.CreateRandom( Fixture );

            var sut = ZonedMonth.Create( dateTime, timeZone );
            var expected = ZonedMonth.Create( dateTime, TimeZoneInfo.Local );

            var result = sut.ToLocalTimeZone();

            result.Should().BeEquivalentTo( expected );
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

            result.Should().Be( expected );
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

            result.Should().Be( expected );
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

            result.Should().Be( expected );
        }

        [Fact]
        public void GetNext_ShouldReturnTheNextMonth()
        {
            var dateTime = Fixture.Create<DateTime>();
            var timeZone = TimeZoneFactory.CreateRandom( Fixture );

            var sut = ZonedMonth.Create( dateTime, timeZone );
            var expected = sut.AddMonths( 1 );

            var result = sut.GetNext();

            result.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void GetPrevious_ShouldReturnThePreviousMonth()
        {
            var dateTime = Fixture.Create<DateTime>();
            var timeZone = TimeZoneFactory.CreateRandom( Fixture );

            var sut = ZonedMonth.Create( dateTime, timeZone );
            var expected = sut.AddMonths( -1 );

            var result = sut.GetPrevious();

            result.Should().BeEquivalentTo( expected );
        }

        [Theory]
        [MethodData( nameof( ZonedMonthTestsData.GetAddMonthsData ) )]
        public void AddMonths_ShouldReturnCorrectResult(DateTime month, TimeZoneInfo timeZone, int daysToAdd, DateTime expectedMonth)
        {
            var sut = ZonedMonth.Create( month, timeZone );
            var expected = ZonedMonth.Create( expectedMonth, timeZone );

            var result = sut.AddMonths( daysToAdd );

            result.Should().BeEquivalentTo( expected );
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

            result.Should().BeEquivalentTo( expected );
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

            var sut = ZonedMonth.Create( dateTime, timeZone );
            var expected = sut.Add( -periodToSubtract );

            var result = sut.Subtract( periodToSubtract );

            result.Should().BeEquivalentTo( expected );
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

            result.Should().Be( expected );
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

            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( ZonedMonthTestsData.GetSetYearData ) )]
        public void SetYear_ShouldReturnTargetWithChangedYear(DateTime month, TimeZoneInfo timeZone, int newYear, DateTime expectedMonth)
        {
            var sut = ZonedMonth.Create( month, timeZone );
            var expected = ZonedMonth.Create( expectedMonth, timeZone );

            var result = sut.SetYear( newYear );

            result.Should().BeEquivalentTo( expected );
        }

        [Theory]
        [MethodData( nameof( ZonedMonthTestsData.GetSetYearThrowData ) )]
        public void SetYear_ShouldThrowArgumentOutOfRangeException_WhenYearIsInvalid(DateTime month, TimeZoneInfo timeZone, int newYear)
        {
            var sut = ZonedMonth.Create( month, timeZone );
            var action = Lambda.Of( () => sut.SetYear( newYear ) );
            action.Should().ThrowExactly<ArgumentOutOfRangeException>();
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

            result.Should().BeEquivalentTo( expected );
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

            result.Should().Be( expected );
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
            action.Should().ThrowExactly<ArgumentOutOfRangeException>();
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

            result.Should().Be( expected );
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
            result.Should().BeNull();
        }

        [Theory]
        [MethodData( nameof( ZonedMonthTestsData.GetGetDateTimeData ) )]
        public void GetDateTime_ShouldReturnCorrectDayWithAddedTimeOfDay(
            DateTime month,
            TimeZoneInfo timeZone,
            int dayOfMonth,
            TimeOfDay timeOfDay,
            DateTime expectedValue)
        {
            var sut = ZonedMonth.Create( month, timeZone );
            var expected = ZonedDateTime.Create( expectedValue, timeZone );

            var result = sut.GetDateTime( dayOfMonth, timeOfDay );

            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( ZonedMonthTestsData.GetGetDateTimeThrowDueToTimeData ) )]
        public void GetDateTime_ShouldThrowInvalidZonedDateTimeException_WhenTimeIsInvalid(
            DateTime month,
            TimeZoneInfo timeZone,
            int dayOfMonth,
            TimeOfDay timeOfDay)
        {
            var sut = ZonedMonth.Create( month, timeZone );
            var action = Lambda.Of( () => sut.GetDateTime( dayOfMonth, timeOfDay ) );
            action.Should().ThrowExactly<InvalidZonedDateTimeException>();
        }

        [Theory]
        [MethodData( nameof( ZonedMonthTestsData.GetGetDateTimeThrowDueToDayData ) )]
        public void GetDateTime_ShouldThrowArgumentOutOfRangeException_WhenDayIsInvalid(
            DateTime month,
            TimeZoneInfo timeZone,
            int dayOfMonth,
            TimeOfDay timeOfDay)
        {
            var sut = ZonedMonth.Create( month, timeZone );
            var action = Lambda.Of( () => sut.GetDateTime( dayOfMonth, timeOfDay ) );
            action.Should().ThrowExactly<ArgumentOutOfRangeException>();
        }

        [Theory]
        [MethodData( nameof( ZonedMonthTestsData.GetGetDateTimeData ) )]
        public void TryGetDateTime_ShouldReturnCorrectDayWithAddedTimeOfDay(
            DateTime month,
            TimeZoneInfo timeZone,
            int dayOfMonth,
            TimeOfDay timeOfDay,
            DateTime expectedValue)
        {
            var sut = ZonedMonth.Create( month, timeZone );
            var expected = ZonedDateTime.Create( expectedValue, timeZone );

            var result = sut.TryGetDateTime( dayOfMonth, timeOfDay );

            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( ZonedMonthTestsData.GetGetDateTimeThrowDueToTimeData ) )]
        public void TryGetDateTime_ShouldReturnNull_WhenTimeIsInvalid(
            DateTime month,
            TimeZoneInfo timeZone,
            int dayOfMonth,
            TimeOfDay timeOfDay)
        {
            var sut = ZonedMonth.Create( month, timeZone );
            var result = sut.TryGetDateTime( dayOfMonth, timeOfDay );
            result.Should().BeNull();
        }

        [Theory]
        [MethodData( nameof( ZonedMonthTestsData.GetGetDateTimeThrowDueToDayData ) )]
        public void TryGetDateTime_ShouldReturnNull_WhenDayIsInvalid(
            DateTime month,
            TimeZoneInfo timeZone,
            int dayOfMonth,
            TimeOfDay timeOfDay)
        {
            var sut = ZonedMonth.Create( month, timeZone );
            var result = sut.TryGetDateTime( dayOfMonth, timeOfDay );
            result.Should().BeNull();
        }

        [Theory]
        [MethodData( nameof( ZonedMonthTestsData.GetGetAllDaysData ) )]
        public void GetAllDays_ShouldReturnCorrectDayCollection(DateTime month, TimeZoneInfo timeZone, int expectedDayCount)
        {
            var sut = ZonedMonth.Create( month, timeZone );

            var result = sut.GetAllDays().ToList();

            using ( new AssertionScope() )
            {
                result.Select( d => new { d.Year, d.Month, d.TimeZone } )
                    .Should()
                    .AllBeEquivalentTo( new { sut.Year, sut.Month, sut.TimeZone } );

                result.Select( d => d.DayOfMonth ).Should().BeSequentiallyEqualTo( Enumerable.Range( 1, expectedDayCount ) );
            }
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

            var sut = ZonedMonth.Create( dateTime, timeZone );
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

            var sut = ZonedMonth.Create( dateTime, timeZone );
            var expected = sut.Add( -periodToSubtract );

            var result = sut - periodToSubtract;

            result.Should().BeEquivalentTo( expected );
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

            result.Should().Be( expected );
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

            result.Should().Be( expected );
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

            result.Should().Be( expected );
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

            result.Should().Be( expected );
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

            result.Should().Be( expected );
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

            result.Should().Be( expected );
        }
    }
}
