using System;
using System.Collections.Generic;
using AutoFixture;
using LfrlSoft.NET.Core.Chrono;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.ChronoTests.ZonedYearTests
{
    public class ZonedYearTestsData
    {
        public static TheoryData<DateTime, TimeZoneInfo, bool> GetCreateData(IFixture fixture)
        {
            var timeZone = TimeZoneFactory.CreateRandom( fixture );

            return new TheoryData<DateTime, TimeZoneInfo, bool>
            {
                { new DateTime( 2021, 1, 1 ), timeZone, false },
                { new DateTime( 2019, 3, 14 ), timeZone, false },
                { new DateTime( 2018, 12, 31, 23, 59, 59, 999 ).AddTicks( 9999 ), timeZone, false },
                { new DateTime( 2020, 1, 1 ), timeZone, true },
                { new DateTime( 2000, 1, 1 ), timeZone, true },
                { new DateTime( 1900, 1, 1 ), timeZone, false }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, DateTime, DateTime, bool> GetCreateWithInvalidStartTimeOrEndTimeData(
            IFixture fixture)
        {
            var positiveTimeZone1 = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 12, 31, 23, 1, 0 ),
                    transitionEnd: new DateTime( 1, 10, 26, 3, 0, 0 ) ) );

            var positiveTimeZone2 = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 12, 31, 23, 30, 0 ),
                    transitionEnd: new DateTime( 1, 10, 26, 3, 0, 0 ) ) );

            var positiveTimeZone3 = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 12, 31, 23, 59, 0 ),
                    transitionEnd: new DateTime( 1, 10, 26, 3, 0, 0 ) ) );

            var positiveTimeZone4 = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 1, 1, 0, 0, 0 ),
                    transitionEnd: new DateTime( 1, 10, 26, 3, 0, 0 ) ) );

            var negativeTimeZone1 = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 4, 26, 2, 0, 0 ),
                    transitionEnd: new DateTime( 1, 12, 31, 23, 1, 0 ),
                    daylightDeltaInHours: -1 ) );

            var negativeTimeZone2 = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 4, 26, 2, 0, 0 ),
                    transitionEnd: new DateTime( 1, 12, 31, 23, 30, 0 ),
                    daylightDeltaInHours: -1 ) );

            var negativeTimeZone3 = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 4, 26, 2, 0, 0 ),
                    transitionEnd: new DateTime( 1, 12, 31, 23, 59, 0 ),
                    daylightDeltaInHours: -1 ) );

            var negativeTimeZone4 = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 4, 26, 2, 0, 0 ),
                    transitionEnd: new DateTime( 1, 1, 1, 0, 0, 0 ),
                    daylightDeltaInHours: -1 ) );

            return new TheoryData<DateTime, TimeZoneInfo, DateTime, DateTime, bool>
            {
                {
                    new DateTime( 2021, 1, 1 ),
                    positiveTimeZone1,
                    new DateTime( 2021, 1, 1, 0, 1, 0 ),
                    new DateTime( 2021, 12, 31, 23, 0, 59, 999 ).AddTicks( 9999 ),
                    false
                },
                {
                    new DateTime( 2020, 1, 1 ),
                    positiveTimeZone2,
                    new DateTime( 2020, 1, 1, 0, 30, 0 ),
                    new DateTime( 2020, 12, 31, 23, 29, 59, 999 ).AddTicks( 9999 ),
                    true
                },
                {
                    new DateTime( 2021, 1, 1 ),
                    positiveTimeZone3,
                    new DateTime( 2021, 1, 1, 0, 59, 0 ),
                    new DateTime( 2021, 12, 31, 23, 58, 59, 999 ).AddTicks( 9999 ),
                    false
                },
                {
                    new DateTime( 2020, 1, 1 ),
                    positiveTimeZone4,
                    new DateTime( 2020, 1, 1, 1, 0, 0 ),
                    new DateTime( 2020, 12, 31, 23, 59, 59, 999 ).AddTicks( 9999 ),
                    true
                },
                {
                    new DateTime( 2021, 1, 1 ),
                    negativeTimeZone1,
                    new DateTime( 2021, 1, 1, 0, 1, 0 ),
                    new DateTime( 2021, 12, 31, 23, 0, 59, 999 ).AddTicks( 9999 ),
                    false
                },
                {
                    new DateTime( 2020, 1, 1 ),
                    negativeTimeZone2,
                    new DateTime( 2020, 1, 1, 0, 30, 0 ),
                    new DateTime( 2020, 12, 31, 23, 29, 59, 999 ).AddTicks( 9999 ),
                    true
                },
                {
                    new DateTime( 2021, 1, 1 ),
                    negativeTimeZone3,
                    new DateTime( 2021, 1, 1, 0, 59, 0 ),
                    new DateTime( 2021, 12, 31, 23, 58, 59, 999 ).AddTicks( 9999 ),
                    false
                },
                {
                    new DateTime( 2020, 1, 1 ),
                    negativeTimeZone4,
                    new DateTime( 2020, 1, 1, 1, 0, 0 ),
                    new DateTime( 2020, 12, 31, 23, 59, 59, 999 ).AddTicks( 9999 ),
                    true
                }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, bool> GetCreateWithContainedInvalidityAndAmbiguityRangesData(IFixture fixture)
        {
            var timeZone1 = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 3, 10, 2, 0, 0 ),
                    transitionEnd: new DateTime( 1, 3, 20, 3, 0, 0 ) ) );

            var timeZone2 = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 4, 20, 2, 0, 0 ),
                    transitionEnd: new DateTime( 1, 4, 10, 3, 0, 0 ) ) );

            var timeZone3 = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 5, 10, 3, 0, 0 ),
                    transitionEnd: new DateTime( 1, 5, 20, 2, 0, 0 ),
                    daylightDeltaInHours: -1.0 ) );

            var timeZone4 = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 6, 20, 3, 0, 0 ),
                    transitionEnd: new DateTime( 1, 6, 10, 2, 0, 0 ),
                    daylightDeltaInHours: -1.0 ) );

            return new TheoryData<DateTime, TimeZoneInfo, bool>
            {
                { new DateTime( 2021, 1, 1 ), timeZone1, false },
                { new DateTime( 2020, 1, 1 ), timeZone2, true },
                { new DateTime( 2021, 1, 1 ), timeZone3, false },
                { new DateTime( 2020, 1, 1 ), timeZone4, true }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, string> GetToStringData(IFixture fixture)
        {
            var tz1 = TimeZoneFactory.Create(
                utcOffsetInHours: 3,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 3, 26, 12, 0, 0 ),
                    transitionEnd: new DateTime( 1, 9, 26, 12, 0, 0 ) ) );

            var tz2 = TimeZoneFactory.Create(
                utcOffsetInHours: 3,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 9, 26, 12, 0, 0 ),
                    transitionEnd: new DateTime( 1, 3, 26, 12, 0, 0 ) ) );

            var tz3 = TimeZoneFactory.Create(
                utcOffsetInHours: -5,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 3, 26, 12, 0, 0 ),
                    transitionEnd: new DateTime( 1, 9, 26, 12, 0, 0 ) ) );

            var tz4 = TimeZoneFactory.Create(
                utcOffsetInHours: -5,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 9, 26, 12, 0, 0 ),
                    transitionEnd: new DateTime( 1, 3, 26, 12, 0, 0 ) ) );

            return new TheoryData<DateTime, TimeZoneInfo, string>
            {
                { new DateTime( 2019, 1, 1 ), tz1, $"2019 +03:00 ({tz1.Id})" },
                { new DateTime( 2020, 1, 1 ), tz2, $"2020 +03:00 ({tz2.Id})" },
                { new DateTime( 2021, 1, 1 ), tz3, $"2021 -05:00 ({tz3.Id})" },
                { new DateTime( 2022, 1, 1 ), tz4, $"2022 -05:00 ({tz4.Id})" }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, DateTime, TimeZoneInfo, bool> GetEqualsData(IFixture fixture)
        {
            var (dt1, dt2) = (new DateTime( 2020, 1, 1 ), new DateTime( 2021, 1, 1 ));
            var (tz1, tz2) = (TimeZoneFactory.Create( 3 ), TimeZoneFactory.Create( 5 ));

            return new TheoryData<DateTime, TimeZoneInfo, DateTime, TimeZoneInfo, bool>
            {
                { dt1, tz1, dt1, tz1, true },
                { dt1, tz1, dt1, tz2, false },
                { dt1, tz1, dt2, tz1, false },
                { dt1, tz1, dt2, tz2, false },
                { dt1, tz2, dt2, tz1, false }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, DateTime, TimeZoneInfo, int> GetCompareToData(IFixture fixture)
        {
            var (dt1, dt2) = (new DateTime( 2020, 1, 1 ), new DateTime( 2021, 1, 1 ));
            var (tz1, tz2, tz3) = (
                TimeZoneFactory.Create( 3 ),
                TimeZoneFactory.Create( 5 ),
                TimeZoneFactory.Create( 3, "Other" ));

            return new TheoryData<DateTime, TimeZoneInfo, DateTime, TimeZoneInfo, int>
            {
                { dt1, tz1, dt1, tz1, 0 },
                { dt1, tz1, dt1, tz2, 1 },
                { dt1, tz2, dt1, tz1, -1 },
                { dt2, tz1, dt1, tz1, 1 },
                { dt1, tz1, dt2, tz1, -1 },
                { dt1, tz1, dt2, tz2, -1 },
                { dt2, tz2, dt1, tz1, 1 },
                { dt1, tz2, dt2, tz1, -1 },
                { dt2, tz1, dt1, tz2, 1 },
                { dt1, tz1, dt1, tz3, -1 },
                { dt1, tz3, dt1, tz1, 1 }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, DateTime, TimeZoneInfo, bool> GetContainsData(IFixture fixture)
        {
            var year = new DateTime( 2021, 1, 1 );
            var tz1 = TimeZoneFactory.Create( 1 );
            var tz3 = TimeZoneFactory.Create( 3 );

            return new TheoryData<DateTime, TimeZoneInfo, DateTime, TimeZoneInfo, bool>
            {
                { year, tz1, new DateTime( 2021, 1, 1 ), tz1, true },
                { year, tz1, new DateTime( 2021, 12, 31, 23, 59, 59, 999 ).AddTicks( 9999 ), tz1, true },
                { year, tz1, new DateTime( 2022, 1, 1 ), tz1, false },
                { year, tz1, new DateTime( 2020, 12, 31, 23, 59, 59, 999 ).AddTicks( 9999 ), tz1, false },
                { year, tz1, new DateTime( 2021, 1, 1, 2, 0, 0 ), tz3, true },
                { year, tz1, new DateTime( 2021, 1, 1, 1, 59, 59, 999 ).AddTicks( 9999 ), tz3, false },
                { year, tz1, new DateTime( 2022, 1, 1, 1, 59, 59, 999 ).AddTicks( 9999 ), tz3, true },
                { year, tz1, new DateTime( 2022, 1, 1, 2, 0, 0 ), tz3, false }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, DateTime, TimeZoneInfo, bool> GetContainsWithZonedDayData(IFixture fixture)
        {
            var year = new DateTime( 2021, 1, 1 );
            var tz1 = TimeZoneFactory.Create( 1 );
            var tz3 = TimeZoneFactory.Create( 3 );

            return new TheoryData<DateTime, TimeZoneInfo, DateTime, TimeZoneInfo, bool>
            {
                { year, tz1, new DateTime( 2021, 1, 1 ), tz1, true },
                { year, tz1, new DateTime( 2021, 12, 31 ), tz1, true },
                { year, tz1, new DateTime( 2022, 1, 1 ), tz1, false },
                { year, tz1, new DateTime( 2020, 12, 31 ), tz1, false },
                { year, tz1, new DateTime( 2021, 1, 1 ), tz3, false },
                { year, tz1, new DateTime( 2021, 1, 2 ), tz3, true },
                { year, tz1, new DateTime( 2022, 1, 1 ), tz3, false },
                { year, tz1, new DateTime( 2021, 12, 31 ), tz3, true }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, DateTime, TimeZoneInfo, bool> GetContainsWithZonedMonthData(IFixture fixture)
        {
            var year = new DateTime( 2021, 1, 1 );
            var tz1 = TimeZoneFactory.Create( 1 );
            var tz3 = TimeZoneFactory.Create( 3 );

            return new TheoryData<DateTime, TimeZoneInfo, DateTime, TimeZoneInfo, bool>
            {
                { year, tz1, new DateTime( 2021, 1, 1 ), tz1, true },
                { year, tz1, new DateTime( 2021, 12, 1 ), tz1, true },
                { year, tz1, new DateTime( 2022, 1, 1 ), tz1, false },
                { year, tz1, new DateTime( 2020, 12, 1 ), tz1, false },
                { year, tz1, new DateTime( 2021, 1, 1 ), tz3, false },
                { year, tz1, new DateTime( 2021, 2, 1 ), tz3, true },
                { year, tz1, new DateTime( 2022, 1, 1 ), tz3, false },
                { year, tz1, new DateTime( 2021, 12, 1 ), tz3, true }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, int, DateTime> GetAddYearsData(IFixture fixture)
        {
            var year = new DateTime( 2021, 1, 1 );
            var timeZone = TimeZoneFactory.Create( 1 );

            return new TheoryData<DateTime, TimeZoneInfo, int, DateTime>
            {
                { year, timeZone, 0, year },
                { year, timeZone, 1, new DateTime( 2022, 1, 1 ) },
                { year, timeZone, -1, new DateTime( 2020, 1, 1 ) },
                { year, timeZone, 10, new DateTime( 2031, 1, 1 ) },
                { year, timeZone, -10, new DateTime( 2011, 1, 1 ) }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, Period, DateTime> GetAddData(IFixture fixture)
        {
            var year = new DateTime( 2021, 1, 1 );
            var timeZone = TimeZoneFactory.Create( 1 );
            var timeZoneWithDaylightSaving = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 9, 26, 2, 0, 0 ),
                    transitionEnd: new DateTime( 1, 10, 26, 3, 0, 0 ) ) );

            return new TheoryData<DateTime, TimeZoneInfo, Period, DateTime>
            {
                { year, timeZone, Period.Empty, year },
                { year, timeZone, Period.FromYears( 1 ), new DateTime( 2022, 1, 1 ) },
                { year, timeZone, Period.FromMonths( 11 ), year },
                { year, timeZone, Period.FromMonths( 12 ), new DateTime( 2022, 1, 1 ) },
                { year, timeZone, Period.FromWeeks( 52 ), year },
                { year, timeZone, Period.FromWeeks( 53 ), new DateTime( 2022, 1, 1 ) },
                { year, timeZone, Period.FromDays( 364 ), year },
                { year, timeZone, Period.FromDays( 365 ), new DateTime( 2022, 1, 1 ) },
                { year, timeZone, Period.FromHours( 8759 ), year },
                { year, timeZone, Period.FromHours( 8760 ), new DateTime( 2022, 1, 1 ) },
                { year, timeZone, Period.FromMinutes( 525599 ), year },
                { year, timeZone, Period.FromMinutes( 525600 ), new DateTime( 2022, 1, 1 ) },
                { year, timeZone, Period.FromSeconds( 31535999 ), year },
                { year, timeZone, Period.FromSeconds( 31536000 ), new DateTime( 2022, 1, 1 ) },
                { year, timeZone, Period.FromMilliseconds( 31535999999 ), year },
                { year, timeZone, Period.FromMilliseconds( 31536000000 ), new DateTime( 2022, 1, 1 ) },
                { year, timeZone, Period.FromTicks( 315359999999999 ), year },
                { year, timeZone, Period.FromTicks( 315360000000000 ), new DateTime( 2022, 1, 1 ) },
                { year, timeZone, new Period( 1, 11, 3, 9, 22, 90, 1700, 80000, 200000000 ), new DateTime( 2023, 1, 1 ) },
                {
                    year,
                    timeZoneWithDaylightSaving,
                    Period.FromYears( 1 ).AddMonths( 9 ).AddDays( 25 ).AddHours( 2 ),
                    new DateTime( 2022, 1, 1 )
                },
                {
                    year,
                    timeZoneWithDaylightSaving,
                    Period.FromYears( 1 ).AddMonths( 9 ).AddDays( 25 ).AddHours( 3 ).SubtractTicks( 1 ),
                    new DateTime( 2022, 1, 1 )
                },
                { year, timeZone, Period.FromTicks( -1 ), new DateTime( 2020, 1, 1 ) }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, DateTime, TimeZoneInfo, PeriodUnits> GetGetPeriodOffsetData(IFixture fixture)
        {
            var year = new DateTime( 2021, 1, 1 );
            var otherYear = new DateTime( 2015, 1, 1 );
            var timeZone = TimeZoneFactory.Create( 1 );

            return new TheoryData<DateTime, TimeZoneInfo, DateTime, TimeZoneInfo, PeriodUnits>
            {
                { year, timeZone, otherYear, timeZone, PeriodUnits.All },
                { year, timeZone, otherYear, timeZone, PeriodUnits.Date },
                { year, timeZone, otherYear, timeZone, PeriodUnits.Time },
                { year, timeZone, otherYear, timeZone, PeriodUnits.Years },
                { year, timeZone, otherYear, timeZone, PeriodUnits.Months },
                { year, timeZone, otherYear, timeZone, PeriodUnits.Weeks },
                { year, timeZone, otherYear, timeZone, PeriodUnits.Days },
                { year, timeZone, otherYear, timeZone, PeriodUnits.Hours },
                { year, timeZone, otherYear, timeZone, PeriodUnits.Minutes },
                { year, timeZone, otherYear, timeZone, PeriodUnits.Seconds },
                { year, timeZone, otherYear, timeZone, PeriodUnits.Milliseconds },
                { year, timeZone, otherYear, timeZone, PeriodUnits.Ticks }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, DateTime, TimeZoneInfo, PeriodUnits> GetGetGreedyPeriodOffsetData(IFixture fixture)
        {
            var year = new DateTime( 2021, 1, 1 );
            var otherYear = new DateTime( 2015, 1, 1 );
            var timeZone = TimeZoneFactory.Create( 1 );

            return new TheoryData<DateTime, TimeZoneInfo, DateTime, TimeZoneInfo, PeriodUnits>
            {
                { year, timeZone, otherYear, timeZone, PeriodUnits.All },
                { year, timeZone, otherYear, timeZone, PeriodUnits.Date },
                { year, timeZone, otherYear, timeZone, PeriodUnits.Time },
                { year, timeZone, otherYear, timeZone, PeriodUnits.Years },
                { year, timeZone, otherYear, timeZone, PeriodUnits.Months },
                { year, timeZone, otherYear, timeZone, PeriodUnits.Weeks },
                { year, timeZone, otherYear, timeZone, PeriodUnits.Days },
                { year, timeZone, otherYear, timeZone, PeriodUnits.Hours },
                { year, timeZone, otherYear, timeZone, PeriodUnits.Minutes },
                { year, timeZone, otherYear, timeZone, PeriodUnits.Seconds },
                { year, timeZone, otherYear, timeZone, PeriodUnits.Milliseconds },
                { year, timeZone, otherYear, timeZone, PeriodUnits.Ticks }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, IsoMonthOfYear, DateTime> GetGetMonthData(IFixture fixture)
        {
            var year = new DateTime( 2021, 1, 1 );
            var timeZone = TimeZoneFactory.CreateRandom( fixture );

            return new TheoryData<DateTime, TimeZoneInfo, IsoMonthOfYear, DateTime>
            {
                { year, timeZone, IsoMonthOfYear.January, new DateTime( 2021, 1, 1 ) },
                { year, timeZone, IsoMonthOfYear.February, new DateTime( 2021, 2, 1 ) },
                { year, timeZone, IsoMonthOfYear.March, new DateTime( 2021, 3, 1 ) },
                { year, timeZone, IsoMonthOfYear.April, new DateTime( 2021, 4, 1 ) },
                { year, timeZone, IsoMonthOfYear.May, new DateTime( 2021, 5, 1 ) },
                { year, timeZone, IsoMonthOfYear.June, new DateTime( 2021, 6, 1 ) },
                { year, timeZone, IsoMonthOfYear.July, new DateTime( 2021, 7, 1 ) },
                { year, timeZone, IsoMonthOfYear.August, new DateTime( 2021, 8, 1 ) },
                { year, timeZone, IsoMonthOfYear.September, new DateTime( 2021, 9, 1 ) },
                { year, timeZone, IsoMonthOfYear.October, new DateTime( 2021, 10, 1 ) },
                { year, timeZone, IsoMonthOfYear.November, new DateTime( 2021, 11, 1 ) },
                { year, timeZone, IsoMonthOfYear.December, new DateTime( 2021, 12, 1 ) }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, int, DateTime> GetGetDayOfYearData(IFixture fixture)
        {
            var timeZone = TimeZoneFactory.CreateRandom( fixture );

            return new TheoryData<DateTime, TimeZoneInfo, int, DateTime>
            {
                { new DateTime( 2021, 1, 1 ), timeZone, 1, new DateTime( 2021, 1, 1 ) },
                { new DateTime( 2021, 1, 1 ), timeZone, 31, new DateTime( 2021, 1, 31 ) },
                { new DateTime( 2021, 1, 1 ), timeZone, 32, new DateTime( 2021, 2, 1 ) },
                { new DateTime( 2021, 1, 1 ), timeZone, 59, new DateTime( 2021, 2, 28 ) },
                { new DateTime( 2021, 1, 1 ), timeZone, 60, new DateTime( 2021, 3, 1 ) },
                { new DateTime( 2021, 1, 1 ), timeZone, 90, new DateTime( 2021, 3, 31 ) },
                { new DateTime( 2021, 1, 1 ), timeZone, 91, new DateTime( 2021, 4, 1 ) },
                { new DateTime( 2021, 1, 1 ), timeZone, 120, new DateTime( 2021, 4, 30 ) },
                { new DateTime( 2021, 1, 1 ), timeZone, 121, new DateTime( 2021, 5, 1 ) },
                { new DateTime( 2021, 1, 1 ), timeZone, 151, new DateTime( 2021, 5, 31 ) },
                { new DateTime( 2021, 1, 1 ), timeZone, 152, new DateTime( 2021, 6, 1 ) },
                { new DateTime( 2021, 1, 1 ), timeZone, 181, new DateTime( 2021, 6, 30 ) },
                { new DateTime( 2021, 1, 1 ), timeZone, 182, new DateTime( 2021, 7, 1 ) },
                { new DateTime( 2021, 1, 1 ), timeZone, 212, new DateTime( 2021, 7, 31 ) },
                { new DateTime( 2021, 1, 1 ), timeZone, 213, new DateTime( 2021, 8, 1 ) },
                { new DateTime( 2021, 1, 1 ), timeZone, 243, new DateTime( 2021, 8, 31 ) },
                { new DateTime( 2021, 1, 1 ), timeZone, 244, new DateTime( 2021, 9, 1 ) },
                { new DateTime( 2021, 1, 1 ), timeZone, 273, new DateTime( 2021, 9, 30 ) },
                { new DateTime( 2021, 1, 1 ), timeZone, 274, new DateTime( 2021, 10, 1 ) },
                { new DateTime( 2021, 1, 1 ), timeZone, 304, new DateTime( 2021, 10, 31 ) },
                { new DateTime( 2021, 1, 1 ), timeZone, 305, new DateTime( 2021, 11, 1 ) },
                { new DateTime( 2021, 1, 1 ), timeZone, 334, new DateTime( 2021, 11, 30 ) },
                { new DateTime( 2021, 1, 1 ), timeZone, 335, new DateTime( 2021, 12, 1 ) },
                { new DateTime( 2021, 1, 1 ), timeZone, 365, new DateTime( 2021, 12, 31 ) },
                { new DateTime( 2020, 1, 1 ), timeZone, 32, new DateTime( 2020, 2, 1 ) },
                { new DateTime( 2020, 1, 1 ), timeZone, 60, new DateTime( 2020, 2, 29 ) },
                { new DateTime( 2020, 1, 1 ), timeZone, 61, new DateTime( 2020, 3, 1 ) },
                { new DateTime( 2020, 1, 1 ), timeZone, 365, new DateTime( 2020, 12, 30 ) },
                { new DateTime( 2020, 1, 1 ), timeZone, 366, new DateTime( 2020, 12, 31 ) }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, int> GetGetDayOfYearThrowData(IFixture fixture)
        {
            var timeZone = TimeZoneFactory.CreateRandom( fixture );

            return new TheoryData<DateTime, TimeZoneInfo, int>
            {
                { new DateTime( 2021, 1, 1 ), timeZone, -1 },
                { new DateTime( 2021, 1, 1 ), timeZone, 0 },
                { new DateTime( 2021, 1, 1 ), timeZone, 366 },
                { new DateTime( 2020, 1, 1 ), timeZone, 367 }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, IsoDayOfWeek, int> GetGetWeekCountData(IFixture fixture)
        {
            var timeZone = TimeZoneFactory.CreateRandom( fixture );

            return new TheoryData<DateTime, TimeZoneInfo, IsoDayOfWeek, int>
            {
                { new DateTime( 2021, 1, 1 ), timeZone, IsoDayOfWeek.Monday, 52 },
                { new DateTime( 2021, 1, 1 ), timeZone, IsoDayOfWeek.Tuesday, 52 },
                { new DateTime( 2021, 1, 1 ), timeZone, IsoDayOfWeek.Wednesday, 52 },
                { new DateTime( 2021, 1, 1 ), timeZone, IsoDayOfWeek.Thursday, 52 },
                { new DateTime( 2021, 1, 1 ), timeZone, IsoDayOfWeek.Friday, 52 },
                { new DateTime( 2021, 1, 1 ), timeZone, IsoDayOfWeek.Saturday, 53 },
                { new DateTime( 2021, 1, 1 ), timeZone, IsoDayOfWeek.Sunday, 52 },
                { new DateTime( 2020, 1, 1 ), timeZone, IsoDayOfWeek.Monday, 53 },
                { new DateTime( 2020, 1, 1 ), timeZone, IsoDayOfWeek.Tuesday, 52 },
                { new DateTime( 2020, 1, 1 ), timeZone, IsoDayOfWeek.Wednesday, 52 },
                { new DateTime( 2020, 1, 1 ), timeZone, IsoDayOfWeek.Thursday, 53 },
                { new DateTime( 2020, 1, 1 ), timeZone, IsoDayOfWeek.Friday, 53 },
                { new DateTime( 2020, 1, 1 ), timeZone, IsoDayOfWeek.Saturday, 52 },
                { new DateTime( 2020, 1, 1 ), timeZone, IsoDayOfWeek.Sunday, 52 },
                { new DateTime( 2019, 1, 1 ), timeZone, IsoDayOfWeek.Monday, 52 },
                { new DateTime( 2019, 1, 1 ), timeZone, IsoDayOfWeek.Tuesday, 52 },
                { new DateTime( 2019, 1, 1 ), timeZone, IsoDayOfWeek.Wednesday, 53 },
                { new DateTime( 2019, 1, 1 ), timeZone, IsoDayOfWeek.Thursday, 52 },
                { new DateTime( 2019, 1, 1 ), timeZone, IsoDayOfWeek.Friday, 52 },
                { new DateTime( 2019, 1, 1 ), timeZone, IsoDayOfWeek.Saturday, 52 },
                { new DateTime( 2019, 1, 1 ), timeZone, IsoDayOfWeek.Sunday, 52 }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, IsoDayOfWeek, int> GetGetAllWeeksData(IFixture fixture)
        {
            return GetGetWeekCountData( fixture );
        }

        public static TheoryData<DateTime, TimeZoneInfo, int> GetGetAllDaysData(IFixture fixture)
        {
            var timeZone = TimeZoneFactory.CreateRandom( fixture );

            return new TheoryData<DateTime, TimeZoneInfo, int>
            {
                { new DateTime( 2021, 1, 1 ), timeZone, 365 },
                { new DateTime( 2020, 1, 1 ), timeZone, 366 }
            };
        }

        public static IEnumerable<object?[]> GetNotEqualsData(IFixture fixture)
        {
            return GetEqualsData( fixture ).ConvertResult( (bool r) => ! r );
        }

        public static IEnumerable<object?[]> GetGreaterThanComparisonData(IFixture fixture)
        {
            return GetCompareToData( fixture ).ConvertResult( (int r) => r > 0 );
        }

        public static IEnumerable<object?[]> GetGreaterThanOrEqualToComparisonData(IFixture fixture)
        {
            return GetCompareToData( fixture ).ConvertResult( (int r) => r >= 0 );
        }

        public static IEnumerable<object?[]> GetLessThanComparisonData(IFixture fixture)
        {
            return GetCompareToData( fixture ).ConvertResult( (int r) => r < 0 );
        }

        public static IEnumerable<object?[]> GetLessThanOrEqualToComparisonData(IFixture fixture)
        {
            return GetCompareToData( fixture ).ConvertResult( (int r) => r <= 0 );
        }
    }
}
