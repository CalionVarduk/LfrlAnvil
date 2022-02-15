using System;
using System.Collections.Generic;
using AutoFixture;
using LfrlSoft.NET.Core.Chrono;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.ChronoTests.ZonedMonthTests
{
    public class ZonedMonthTestsData
    {
        public static TheoryData<DateTime, TimeZoneInfo, int> GetCreateData(IFixture fixture)
        {
            var timeZone = TimeZoneFactory.CreateRandom( fixture );

            return new TheoryData<DateTime, TimeZoneInfo, int>
            {
                { new DateTime( 2021, 1, 1 ), timeZone, 31 },
                { new DateTime( 2021, 1, 31, 23, 59, 59, 999 ).AddTicks( 9999 ), timeZone, 31 },
                { new DateTime( 2021, 2, 1 ), timeZone, 28 },
                { new DateTime( 2021, 2, 28, 23, 59, 59, 999 ).AddTicks( 9999 ), timeZone, 28 },
                { new DateTime( 2021, 3, 1 ), timeZone, 31 },
                { new DateTime( 2021, 3, 31, 23, 59, 59, 999 ).AddTicks( 9999 ), timeZone, 31 },
                { new DateTime( 2021, 4, 1 ), timeZone, 30 },
                { new DateTime( 2021, 4, 30, 23, 59, 59, 999 ).AddTicks( 9999 ), timeZone, 30 },
                { new DateTime( 2021, 5, 1 ), timeZone, 31 },
                { new DateTime( 2021, 5, 31, 23, 59, 59, 999 ).AddTicks( 9999 ), timeZone, 31 },
                { new DateTime( 2021, 6, 1 ), timeZone, 30 },
                { new DateTime( 2021, 6, 30, 23, 59, 59, 999 ).AddTicks( 9999 ), timeZone, 30 },
                { new DateTime( 2021, 7, 1 ), timeZone, 31 },
                { new DateTime( 2021, 7, 31, 23, 59, 59, 999 ).AddTicks( 9999 ), timeZone, 31 },
                { new DateTime( 2021, 8, 1 ), timeZone, 31 },
                { new DateTime( 2021, 8, 31, 23, 59, 59, 999 ).AddTicks( 9999 ), timeZone, 31 },
                { new DateTime( 2021, 9, 1 ), timeZone, 30 },
                { new DateTime( 2021, 9, 30, 23, 59, 59, 999 ).AddTicks( 9999 ), timeZone, 30 },
                { new DateTime( 2021, 10, 1 ), timeZone, 31 },
                { new DateTime( 2021, 10, 31, 23, 59, 59, 999 ).AddTicks( 9999 ), timeZone, 31 },
                { new DateTime( 2021, 11, 1 ), timeZone, 30 },
                { new DateTime( 2021, 11, 30, 23, 59, 59, 999 ).AddTicks( 9999 ), timeZone, 30 },
                { new DateTime( 2021, 12, 1 ), timeZone, 31 },
                { new DateTime( 2021, 12, 31, 23, 59, 59, 999 ).AddTicks( 9999 ), timeZone, 31 },
                { new DateTime( 2020, 2, 1 ), timeZone, 29 },
                { new DateTime( 2020, 2, 29, 23, 59, 59, 999 ).AddTicks( 9999 ), timeZone, 29 }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, int, Duration> GetCreateWithContainedInvalidityRangeData(IFixture fixture)
        {
            var positiveTimeZone = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 8, 26, 2, 0, 0 ),
                    transitionEnd: new DateTime( 1, 10, 26, 3, 0, 0 ) ) );

            var negativeTimeZone = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 6, 26, 3, 0, 0 ),
                    transitionEnd: new DateTime( 1, 8, 26, 2, 0, 0 ),
                    daylightDeltaInHours: -1 ) );

            return new TheoryData<DateTime, TimeZoneInfo, int, Duration>
            {
                { new DateTime( 2021, 8, 1 ), positiveTimeZone, 31, Duration.FromHours( 743 ) },
                { new DateTime( 2021, 8, 1 ), negativeTimeZone, 31, Duration.FromHours( 743 ) },
                { new DateTime( 2021, 8, 31, 23, 59, 59, 999 ).AddTicks( 9999 ), positiveTimeZone, 31, Duration.FromHours( 743 ) },
                { new DateTime( 2021, 8, 31, 23, 59, 59, 999 ).AddTicks( 9999 ), negativeTimeZone, 31, Duration.FromHours( 743 ) },
                { new DateTime( 2021, 8, 26, 2, 0, 0 ), positiveTimeZone, 31, Duration.FromHours( 743 ) },
                { new DateTime( 2021, 8, 26, 2, 59, 59, 999 ).AddTicks( 9999 ), positiveTimeZone, 31, Duration.FromHours( 743 ) },
                { new DateTime( 2021, 8, 26, 2, 0, 0 ), negativeTimeZone, 31, Duration.FromHours( 743 ) },
                { new DateTime( 2021, 8, 26, 2, 59, 59, 999 ).AddTicks( 9999 ), negativeTimeZone, 31, Duration.FromHours( 743 ) }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, int, Duration> GetCreateWithContainedAmbiguityRangeData(IFixture fixture)
        {
            var positiveTimeZone = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 6, 26, 2, 0, 0 ),
                    transitionEnd: new DateTime( 1, 9, 26, 3, 0, 0 ) ) );

            var negativeTimeZone = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 9, 26, 3, 0, 0 ),
                    transitionEnd: new DateTime( 1, 10, 26, 2, 0, 0 ),
                    daylightDeltaInHours: -1 ) );

            return new TheoryData<DateTime, TimeZoneInfo, int, Duration>
            {
                { new DateTime( 2021, 9, 1 ), positiveTimeZone, 30, Duration.FromHours( 721 ) },
                { new DateTime( 2021, 9, 1 ), negativeTimeZone, 30, Duration.FromHours( 721 ) },
                { new DateTime( 2021, 9, 30, 23, 59, 59, 999 ).AddTicks( 9999 ), positiveTimeZone, 30, Duration.FromHours( 721 ) },
                { new DateTime( 2021, 9, 30, 23, 59, 59, 999 ).AddTicks( 9999 ), negativeTimeZone, 30, Duration.FromHours( 721 ) },
                { new DateTime( 2021, 9, 26, 2, 0, 0 ), positiveTimeZone, 30, Duration.FromHours( 721 ) },
                { new DateTime( 2021, 9, 26, 2, 59, 59, 999 ).AddTicks( 9999 ), positiveTimeZone, 30, Duration.FromHours( 721 ) },
                { new DateTime( 2021, 9, 26, 2, 0, 0 ), negativeTimeZone, 30, Duration.FromHours( 721 ) },
                { new DateTime( 2021, 9, 26, 2, 59, 59, 999 ).AddTicks( 9999 ), negativeTimeZone, 30, Duration.FromHours( 721 ) }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, DateTime, int, Duration>
            GetCreateWithInvalidStartTimeData(IFixture fixture)
        {
            var positiveTimeZone1 = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 7, 31, 23, 1, 0 ),
                    transitionEnd: new DateTime( 1, 10, 26, 3, 0, 0 ) ) );

            var positiveTimeZone2 = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 6, 30, 23, 30, 0 ),
                    transitionEnd: new DateTime( 1, 10, 26, 3, 0, 0 ) ) );

            var positiveTimeZone3 = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 5, 31, 23, 59, 0 ),
                    transitionEnd: new DateTime( 1, 10, 26, 3, 0, 0 ) ) );

            var positiveTimeZone4 = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 5, 1, 0, 0, 0 ),
                    transitionEnd: new DateTime( 1, 10, 26, 3, 0, 0 ) ) );

            var negativeTimeZone1 = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 4, 26, 2, 0, 0 ),
                    transitionEnd: new DateTime( 1, 7, 31, 23, 1, 0 ),
                    daylightDeltaInHours: -1 ) );

            var negativeTimeZone2 = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 4, 26, 2, 0, 0 ),
                    transitionEnd: new DateTime( 1, 6, 30, 23, 30, 0 ),
                    daylightDeltaInHours: -1 ) );

            var negativeTimeZone3 = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 4, 26, 2, 0, 0 ),
                    transitionEnd: new DateTime( 1, 5, 31, 23, 59, 0 ),
                    daylightDeltaInHours: -1 ) );

            var negativeTimeZone4 = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 4, 26, 2, 0, 0 ),
                    transitionEnd: new DateTime( 1, 5, 1, 0, 0, 0 ),
                    daylightDeltaInHours: -1 ) );

            return new TheoryData<DateTime, TimeZoneInfo, DateTime, int, Duration>
            {
                {
                    new DateTime( 2021, 8, 1 ),
                    positiveTimeZone1,
                    new DateTime( 2021, 8, 1, 0, 1, 0 ),
                    31,
                    Duration.FromHours( 744 ).SubtractMinutes( 1 )
                },
                {
                    new DateTime( 2021, 7, 1 ),
                    positiveTimeZone2,
                    new DateTime( 2021, 7, 1, 0, 30, 0 ),
                    31,
                    Duration.FromHours( 744 ).SubtractMinutes( 30 )
                },
                {
                    new DateTime( 2021, 6, 1 ),
                    positiveTimeZone3,
                    new DateTime( 2021, 6, 1, 0, 59, 0 ),
                    30,
                    Duration.FromHours( 720 ).SubtractMinutes( 59 )
                },
                {
                    new DateTime( 2021, 5, 1 ),
                    positiveTimeZone4,
                    new DateTime( 2021, 5, 1, 1, 0, 0 ),
                    31,
                    Duration.FromHours( 743 )
                },
                {
                    new DateTime( 2021, 8, 1 ),
                    negativeTimeZone1,
                    new DateTime( 2021, 8, 1, 0, 1, 0 ),
                    31,
                    Duration.FromHours( 744 ).SubtractMinutes( 1 )
                },
                {
                    new DateTime( 2021, 7, 1 ),
                    negativeTimeZone2,
                    new DateTime( 2021, 7, 1, 0, 30, 0 ),
                    31,
                    Duration.FromHours( 744 ).SubtractMinutes( 30 )
                },
                {
                    new DateTime( 2021, 6, 1 ),
                    negativeTimeZone3,
                    new DateTime( 2021, 6, 1, 0, 59, 0 ),
                    30,
                    Duration.FromHours( 720 ).SubtractMinutes( 59 )
                },
                {
                    new DateTime( 2021, 5, 1 ),
                    negativeTimeZone4,
                    new DateTime( 2021, 5, 1, 1, 0, 0 ),
                    31,
                    Duration.FromHours( 743 )
                }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, DateTime, int, Duration>
            GetCreateWithInvalidEndTimeData(IFixture fixture)
        {
            var positiveTimeZone1 = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 8, 31, 23, 1, 0 ),
                    transitionEnd: new DateTime( 1, 12, 26, 3, 0, 0 ) ) );

            var positiveTimeZone2 = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 9, 30, 23, 30, 0 ),
                    transitionEnd: new DateTime( 1, 12, 26, 3, 0, 0 ) ) );

            var positiveTimeZone3 = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 10, 31, 23, 59, 0 ),
                    transitionEnd: new DateTime( 1, 12, 26, 3, 0, 0 ) ) );

            var positiveTimeZone4 = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 11, 30, 23, 0, 0 ),
                    transitionEnd: new DateTime( 1, 12, 26, 3, 0, 0 ) ) );

            var negativeTimeZone1 = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 6, 26, 2, 0, 0 ),
                    transitionEnd: new DateTime( 1, 8, 31, 23, 1, 0 ),
                    daylightDeltaInHours: -1 ) );

            var negativeTimeZone2 = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 6, 26, 2, 0, 0 ),
                    transitionEnd: new DateTime( 1, 9, 30, 23, 30, 0 ),
                    daylightDeltaInHours: -1 ) );

            var negativeTimeZone3 = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 6, 26, 2, 0, 0 ),
                    transitionEnd: new DateTime( 1, 10, 31, 23, 59, 0 ),
                    daylightDeltaInHours: -1 ) );

            var negativeTimeZone4 = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 6, 26, 2, 0, 0 ),
                    transitionEnd: new DateTime( 1, 11, 30, 23, 0, 0 ),
                    daylightDeltaInHours: -1 ) );

            return new TheoryData<DateTime, TimeZoneInfo, DateTime, int, Duration>
            {
                {
                    new DateTime( 2021, 8, 1 ),
                    positiveTimeZone1,
                    new DateTime( 2021, 8, 31, 23, 0, 59, 999 ).AddTicks( 9999 ),
                    31,
                    Duration.FromHours( 744 ).SubtractMinutes( 59 )
                },
                {
                    new DateTime( 2021, 9, 1 ),
                    positiveTimeZone2,
                    new DateTime( 2021, 9, 30, 23, 29, 59, 999 ).AddTicks( 9999 ),
                    30,
                    Duration.FromHours( 720 ).SubtractMinutes( 30 )
                },
                {
                    new DateTime( 2021, 10, 1 ),
                    positiveTimeZone3,
                    new DateTime( 2021, 10, 31, 23, 58, 59, 999 ).AddTicks( 9999 ),
                    31,
                    Duration.FromHours( 744 ).SubtractMinutes( 1 )
                },
                {
                    new DateTime( 2021, 11, 1 ),
                    positiveTimeZone4,
                    new DateTime( 2021, 11, 30, 22, 59, 59, 999 ).AddTicks( 9999 ),
                    30,
                    Duration.FromHours( 719 )
                },
                {
                    new DateTime( 2021, 8, 1 ),
                    negativeTimeZone1,
                    new DateTime( 2021, 8, 31, 23, 0, 59, 999 ).AddTicks( 9999 ),
                    31,
                    Duration.FromHours( 744 ).SubtractMinutes( 59 )
                },
                {
                    new DateTime( 2021, 9, 1 ),
                    negativeTimeZone2,
                    new DateTime( 2021, 9, 30, 23, 29, 59, 999 ).AddTicks( 9999 ),
                    30,
                    Duration.FromHours( 720 ).SubtractMinutes( 30 )
                },
                {
                    new DateTime( 2021, 10, 1 ),
                    negativeTimeZone3,
                    new DateTime( 2021, 10, 31, 23, 58, 59, 999 ).AddTicks( 9999 ),
                    31,
                    Duration.FromHours( 744 ).SubtractMinutes( 1 )
                },
                {
                    new DateTime( 2021, 11, 1 ),
                    negativeTimeZone4,
                    new DateTime( 2021, 11, 30, 22, 59, 59, 999 ).AddTicks( 9999 ),
                    30,
                    Duration.FromHours( 719 )
                }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, bool, int, Duration>
            GetCreateWithAmbiguousStartTimeData(IFixture fixture)
        {
            var positiveTimeZone1 = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 4, 26, 2, 0, 0 ),
                    transitionEnd: new DateTime( 1, 8, 1, 0, 1, 0 ) ) );

            var positiveTimeZone2 = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 4, 26, 2, 0, 0 ),
                    transitionEnd: new DateTime( 1, 7, 1, 0, 30, 0 ) ) );

            var positiveTimeZone3 = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 4, 26, 2, 0, 0 ),
                    transitionEnd: new DateTime( 1, 6, 1, 0, 59, 0 ) ) );

            var positiveTimeZone4 = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 4, 26, 2, 0, 0 ),
                    transitionEnd: new DateTime( 1, 5, 1, 1, 0, 0 ) ) );

            var negativeTimeZone1 = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 8, 1, 0, 1, 0 ),
                    transitionEnd: new DateTime( 1, 10, 26, 2, 0, 0 ),
                    daylightDeltaInHours: -1 ) );

            var negativeTimeZone2 = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 7, 1, 0, 30, 0 ),
                    transitionEnd: new DateTime( 1, 10, 26, 2, 0, 0 ),
                    daylightDeltaInHours: -1 ) );

            var negativeTimeZone3 = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 6, 1, 0, 59, 0 ),
                    transitionEnd: new DateTime( 1, 10, 26, 2, 0, 0 ),
                    daylightDeltaInHours: -1 ) );

            var negativeTimeZone4 = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 5, 1, 1, 0, 0 ),
                    transitionEnd: new DateTime( 1, 10, 26, 2, 0, 0 ),
                    daylightDeltaInHours: -1 ) );

            return new TheoryData<DateTime, TimeZoneInfo, bool, int, Duration>
            {
                {
                    new DateTime( 2021, 8, 1 ),
                    positiveTimeZone1,
                    true,
                    31,
                    Duration.FromHours( 744 ).AddMinutes( 1 )
                },
                {
                    new DateTime( 2021, 7, 1 ),
                    positiveTimeZone2,
                    true,
                    31,
                    Duration.FromHours( 744 ).AddMinutes( 30 )
                },
                {
                    new DateTime( 2021, 6, 1 ),
                    positiveTimeZone3,
                    true,
                    30,
                    Duration.FromHours( 720 ).AddMinutes( 59 )
                },
                {
                    new DateTime( 2021, 5, 1 ),
                    positiveTimeZone4,
                    true,
                    31,
                    Duration.FromHours( 745 )
                },
                {
                    new DateTime( 2021, 8, 1 ),
                    negativeTimeZone1,
                    false,
                    31,
                    Duration.FromHours( 744 ).AddMinutes( 1 )
                },
                {
                    new DateTime( 2021, 7, 1 ),
                    negativeTimeZone2,
                    false,
                    31,
                    Duration.FromHours( 744 ).AddMinutes( 30 )
                },
                {
                    new DateTime( 2021, 6, 1 ),
                    negativeTimeZone3,
                    false,
                    30,
                    Duration.FromHours( 720 ).AddMinutes( 59 )
                },
                {
                    new DateTime( 2021, 5, 1 ),
                    negativeTimeZone4,
                    false,
                    31,
                    Duration.FromHours( 745 )
                }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, bool, int, Duration>
            GetCreateWithAmbiguousEndTimeData(IFixture fixture)
        {
            var positiveTimeZone1 = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 6, 26, 2, 0, 0 ),
                    transitionEnd: new DateTime( 1, 9, 1, 0, 1, 0 ) ) );

            var positiveTimeZone2 = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 6, 26, 2, 0, 0 ),
                    transitionEnd: new DateTime( 1, 10, 1, 0, 30, 0 ) ) );

            var positiveTimeZone3 = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 6, 26, 2, 0, 0 ),
                    transitionEnd: new DateTime( 1, 11, 1, 0, 59, 0 ) ) );

            var positiveTimeZone4 = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 6, 26, 2, 0, 0 ),
                    transitionEnd: new DateTime( 1, 12, 1, 0, 0, 0 ) ) );

            var negativeTimeZone1 = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 9, 1, 0, 1, 0 ),
                    transitionEnd: new DateTime( 1, 12, 26, 2, 0, 0 ),
                    daylightDeltaInHours: -1 ) );

            var negativeTimeZone2 = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 10, 1, 0, 30, 0 ),
                    transitionEnd: new DateTime( 1, 12, 26, 2, 0, 0 ),
                    daylightDeltaInHours: -1 ) );

            var negativeTimeZone3 = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 11, 1, 0, 59, 0 ),
                    transitionEnd: new DateTime( 1, 12, 26, 2, 0, 0 ),
                    daylightDeltaInHours: -1 ) );

            var negativeTimeZone4 = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 12, 1, 0, 0, 0 ),
                    transitionEnd: new DateTime( 1, 12, 26, 2, 0, 0 ),
                    daylightDeltaInHours: -1 ) );

            return new TheoryData<DateTime, TimeZoneInfo, bool, int, Duration>
            {
                {
                    new DateTime( 2021, 8, 1 ),
                    positiveTimeZone1,
                    false,
                    31,
                    Duration.FromHours( 744 ).AddMinutes( 59 )
                },
                {
                    new DateTime( 2021, 9, 1 ),
                    positiveTimeZone2,
                    false,
                    30,
                    Duration.FromHours( 720 ).AddMinutes( 30 )
                },
                {
                    new DateTime( 2021, 10, 1 ),
                    positiveTimeZone3,
                    false,
                    31,
                    Duration.FromHours( 744 ).AddMinutes( 1 )
                },
                {
                    new DateTime( 2021, 11, 1 ),
                    positiveTimeZone4,
                    false,
                    30,
                    Duration.FromHours( 721 )
                },
                {
                    new DateTime( 2021, 8, 1 ),
                    negativeTimeZone1,
                    true,
                    31,
                    Duration.FromHours( 744 ).AddMinutes( 59 )
                },
                {
                    new DateTime( 2021, 9, 1 ),
                    negativeTimeZone2,
                    true,
                    30,
                    Duration.FromHours( 720 ).AddMinutes( 30 )
                },
                {
                    new DateTime( 2021, 10, 1 ),
                    negativeTimeZone3,
                    true,
                    31,
                    Duration.FromHours( 744 ).AddMinutes( 1 )
                },
                {
                    new DateTime( 2021, 11, 1 ),
                    negativeTimeZone4,
                    true,
                    30,
                    Duration.FromHours( 721 )
                }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, DateTime, bool, int, Duration>
            GetCreateWithInvalidStartTimeAndAmbiguousEndTimeData(IFixture fixture)
        {
            var positiveTimeZone1 = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 7, 31, 23, 1, 0 ),
                    transitionEnd: new DateTime( 1, 9, 1, 0, 1, 0 ) ) );

            var positiveTimeZone2 = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 6, 30, 23, 59, 0 ),
                    transitionEnd: new DateTime( 1, 8, 1, 0, 59, 0 ) ) );

            var positiveTimeZone3 = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 6, 1, 0, 0, 0 ),
                    transitionEnd: new DateTime( 1, 7, 1, 0, 0, 0 ) ) );

            var negativeTimeZone1 = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 9, 1, 0, 1, 0 ),
                    transitionEnd: new DateTime( 1, 7, 31, 23, 1, 0 ),
                    daylightDeltaInHours: -1 ) );

            var negativeTimeZone2 = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 8, 1, 0, 59, 0 ),
                    transitionEnd: new DateTime( 1, 6, 30, 23, 59, 0 ),
                    daylightDeltaInHours: -1 ) );

            var negativeTimeZone3 = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 7, 1, 0, 0, 0 ),
                    transitionEnd: new DateTime( 1, 6, 1, 0, 0, 0 ),
                    daylightDeltaInHours: -1 ) );

            return new TheoryData<DateTime, TimeZoneInfo, DateTime, bool, int, Duration>
            {
                {
                    new DateTime( 2021, 8, 1 ),
                    positiveTimeZone1,
                    new DateTime( 2021, 8, 1, 0, 1, 0 ),
                    false,
                    31,
                    Duration.FromHours( 744 ).AddMinutes( 58 )
                },
                {
                    new DateTime( 2021, 7, 1 ),
                    positiveTimeZone2,
                    new DateTime( 2021, 7, 1, 0, 59, 0 ),
                    false,
                    31,
                    Duration.FromHours( 744 ).SubtractMinutes( 58 )
                },
                {
                    new DateTime( 2021, 6, 1 ),
                    positiveTimeZone3,
                    new DateTime( 2021, 6, 1, 1, 0, 0 ),
                    false,
                    30,
                    Duration.FromHours( 720 )
                },
                {
                    new DateTime( 2021, 8, 1 ),
                    negativeTimeZone1,
                    new DateTime( 2021, 8, 1, 0, 1, 0 ),
                    true,
                    31,
                    Duration.FromHours( 744 ).AddMinutes( 58 )
                },
                {
                    new DateTime( 2021, 7, 1 ),
                    negativeTimeZone2,
                    new DateTime( 2021, 7, 1, 0, 59, 0 ),
                    true,
                    31,
                    Duration.FromHours( 744 ).SubtractMinutes( 58 )
                },
                {
                    new DateTime( 2021, 6, 1 ),
                    negativeTimeZone3,
                    new DateTime( 2021, 6, 1, 1, 0, 0 ),
                    true,
                    30,
                    Duration.FromHours( 720 )
                }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, DateTime, bool, int, Duration>
            GetCreateWithAmbiguousStartTimeAndInvalidEndTimeData(IFixture fixture)
        {
            var positiveTimeZone1 = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 8, 31, 23, 59, 0 ),
                    transitionEnd: new DateTime( 1, 8, 1, 0, 59, 0 ) ) );

            var positiveTimeZone2 = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 7, 31, 23, 1, 0 ),
                    transitionEnd: new DateTime( 1, 7, 1, 0, 1, 0 ) ) );

            var positiveTimeZone3 = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 6, 30, 23, 0, 0 ),
                    transitionEnd: new DateTime( 1, 6, 1, 1, 0, 0 ) ) );

            var negativeTimeZone1 = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 8, 1, 0, 59, 0 ),
                    transitionEnd: new DateTime( 1, 8, 31, 23, 59, 0 ),
                    daylightDeltaInHours: -1 ) );

            var negativeTimeZone2 = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 7, 1, 0, 1, 0 ),
                    transitionEnd: new DateTime( 1, 7, 31, 23, 1, 0 ),
                    daylightDeltaInHours: -1 ) );

            var negativeTimeZone3 = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 6, 1, 1, 0, 0 ),
                    transitionEnd: new DateTime( 1, 6, 30, 23, 0, 0 ),
                    daylightDeltaInHours: -1 ) );

            return new TheoryData<DateTime, TimeZoneInfo, DateTime, bool, int, Duration>
            {
                {
                    new DateTime( 2021, 8, 1 ),
                    positiveTimeZone1,
                    new DateTime( 2021, 8, 31, 23, 58, 59, 999 ).AddTicks( 9999 ),
                    true,
                    31,
                    Duration.FromHours( 744 ).AddMinutes( 58 )
                },
                {
                    new DateTime( 2021, 7, 1 ),
                    positiveTimeZone2,
                    new DateTime( 2021, 7, 31, 23, 0, 59, 999 ).AddTicks( 9999 ),
                    true,
                    31,
                    Duration.FromHours( 744 ).SubtractMinutes( 58 )
                },
                {
                    new DateTime( 2021, 6, 1 ),
                    positiveTimeZone3,
                    new DateTime( 2021, 6, 30, 22, 59, 59, 999 ).AddTicks( 9999 ),
                    true,
                    30,
                    Duration.FromHours( 720 )
                },
                {
                    new DateTime( 2021, 8, 1 ),
                    negativeTimeZone1,
                    new DateTime( 2021, 8, 31, 23, 58, 59, 999 ).AddTicks( 9999 ),
                    false,
                    31,
                    Duration.FromHours( 744 ).AddMinutes( 58 )
                },
                {
                    new DateTime( 2021, 7, 1 ),
                    negativeTimeZone2,
                    new DateTime( 2021, 7, 31, 23, 0, 59, 999 ).AddTicks( 9999 ),
                    false,
                    31,
                    Duration.FromHours( 744 ).SubtractMinutes( 58 )
                },
                {
                    new DateTime( 2021, 6, 1 ),
                    negativeTimeZone3,
                    new DateTime( 2021, 6, 30, 22, 59, 59, 999 ).AddTicks( 9999 ),
                    false,
                    30,
                    Duration.FromHours( 720 )
                }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, int, Duration> GetCreateWithContainedInvalidityAndAmbiguityRangesData(
            IFixture fixture)
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

            return new TheoryData<DateTime, TimeZoneInfo, int, Duration>
            {
                { new DateTime( 2021, 3, 1 ), timeZone1, 31, Duration.FromHours( 744 ) },
                { new DateTime( 2021, 4, 1 ), timeZone2, 30, Duration.FromHours( 720 ) },
                { new DateTime( 2021, 5, 1 ), timeZone3, 31, Duration.FromHours( 744 ) },
                { new DateTime( 2021, 6, 1 ), timeZone4, 30, Duration.FromHours( 720 ) }
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
                utcOffsetInHours: -5,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 3, 26, 12, 0, 0 ),
                    transitionEnd: new DateTime( 1, 9, 26, 12, 0, 0 ) ) );

            return new TheoryData<DateTime, TimeZoneInfo, string>
            {
                { new DateTime( 2019, 1, 1 ), tz1, $"2019-01 +03:00 ({tz1.Id})" },
                { new DateTime( 2020, 3, 1 ), tz1, $"2020-03 +03:00 +04:00 ({tz1.Id})" },
                { new DateTime( 2021, 4, 1 ), tz1, $"2021-04 +04:00 ({tz1.Id})" },
                { new DateTime( 2022, 9, 1 ), tz1, $"2022-09 +04:00 +03:00 ({tz1.Id})" },
                { new DateTime( 2023, 10, 1 ), tz1, $"2023-10 +03:00 ({tz1.Id})" },
                { new DateTime( 2019, 1, 1 ), tz2, $"2019-01 -05:00 ({tz2.Id})" },
                { new DateTime( 2020, 3, 1 ), tz2, $"2020-03 -05:00 -04:00 ({tz2.Id})" },
                { new DateTime( 2021, 4, 1 ), tz2, $"2021-04 -04:00 ({tz2.Id})" },
                { new DateTime( 2022, 9, 1 ), tz2, $"2022-09 -04:00 -05:00 ({tz2.Id})" },
                { new DateTime( 2023, 10, 1 ), tz2, $"2023-10 -05:00 ({tz2.Id})" },
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, DateTime, TimeZoneInfo, bool> GetEqualsData(IFixture fixture)
        {
            var (dt1, dt2) = (new DateTime( 2021, 8, 1 ), new DateTime( 2021, 9, 1 ));
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
            var (dt1, dt2) = (new DateTime( 2021, 8, 1 ), new DateTime( 2021, 9, 1 ));
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
            var month = new DateTime( 2021, 8, 1 );
            var tz1 = TimeZoneFactory.Create( 1 );
            var tz3 = TimeZoneFactory.Create( 3 );

            return new TheoryData<DateTime, TimeZoneInfo, DateTime, TimeZoneInfo, bool>
            {
                { month, tz1, new DateTime( 2021, 8, 1 ), tz1, true },
                { month, tz1, new DateTime( 2021, 8, 31, 23, 59, 59, 999 ).AddTicks( 9999 ), tz1, true },
                { month, tz1, new DateTime( 2021, 9, 1 ), tz1, false },
                { month, tz1, new DateTime( 2021, 7, 31, 23, 59, 59, 999 ).AddTicks( 9999 ), tz1, false },
                { month, tz1, new DateTime( 2021, 8, 1, 2, 0, 0 ), tz3, true },
                { month, tz1, new DateTime( 2021, 8, 1, 1, 59, 59, 999 ).AddTicks( 9999 ), tz3, false },
                { month, tz1, new DateTime( 2021, 9, 1, 1, 59, 59, 999 ).AddTicks( 9999 ), tz3, true },
                { month, tz1, new DateTime( 2021, 9, 1, 2, 0, 0 ), tz3, false }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, DateTime, bool, bool> GetContainsWithAmbiguousStartOrEndData(IFixture fixture)
        {
            var month = new DateTime( 2021, 8, 1 );

            var startTz = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 6, 26, 2, 0, 0 ),
                    transitionEnd: new DateTime( 1, 8, 26, 0, 30, 0 ) ) );

            var endTz = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 6, 26, 2, 0, 0 ),
                    transitionEnd: new DateTime( 1, 8, 27, 0, 30, 0 ) ) );

            return new TheoryData<DateTime, TimeZoneInfo, DateTime, bool, bool>
            {
                { month, startTz, new DateTime( 2021, 8, 1 ), false, true },
                { month, startTz, new DateTime( 2021, 8, 1 ), true, true },
                { month, startTz, new DateTime( 2021, 7, 31, 23, 30, 0 ), false, false },
                { month, startTz, new DateTime( 2021, 7, 31, 23, 30, 0 ), true, false },
                { month, startTz, new DateTime( 2021, 7, 31, 23, 59, 59, 999 ).AddTicks( 9999 ), false, false },
                { month, startTz, new DateTime( 2021, 7, 31, 23, 59, 59, 999 ).AddTicks( 9999 ), true, false },
                { month, endTz, new DateTime( 2021, 8, 31, 23, 59, 59, 999 ).AddTicks( 9999 ), false, true },
                { month, endTz, new DateTime( 2021, 8, 31, 23, 59, 59, 999 ).AddTicks( 9999 ), true, true },
                { month, endTz, new DateTime( 2021, 8, 31, 23, 30, 0 ), false, true },
                { month, endTz, new DateTime( 2021, 8, 31, 23, 30, 0 ), true, true },
                { month, endTz, new DateTime( 2021, 9, 1 ), false, false },
                { month, endTz, new DateTime( 2021, 9, 1 ), true, false }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, DateTime, TimeZoneInfo, bool> GetContainsWithZonedDayData(IFixture fixture)
        {
            var month = new DateTime( 2021, 8, 1 );
            var tz1 = TimeZoneFactory.Create( 1 );
            var tz3 = TimeZoneFactory.Create( 3 );

            return new TheoryData<DateTime, TimeZoneInfo, DateTime, TimeZoneInfo, bool>
            {
                { month, tz1, new DateTime( 2021, 8, 1 ), tz1, true },
                { month, tz1, new DateTime( 2021, 8, 31 ), tz1, true },
                { month, tz1, new DateTime( 2021, 9, 1 ), tz1, false },
                { month, tz1, new DateTime( 2021, 7, 31 ), tz1, false },
                { month, tz1, new DateTime( 2021, 8, 1 ), tz3, false },
                { month, tz1, new DateTime( 2021, 8, 2 ), tz3, true },
                { month, tz1, new DateTime( 2021, 9, 1 ), tz3, false },
                { month, tz1, new DateTime( 2021, 8, 31 ), tz3, true }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, int, DateTime> GetAddMonthsData(IFixture fixture)
        {
            var month = new DateTime( 2021, 8, 1 );
            var timeZone = TimeZoneFactory.Create( 1 );

            return new TheoryData<DateTime, TimeZoneInfo, int, DateTime>
            {
                { month, timeZone, 0, month },
                { month, timeZone, 1, new DateTime( 2021, 9, 1 ) },
                { month, timeZone, -1, new DateTime( 2021, 7, 1 ) },
                { month, timeZone, 10, new DateTime( 2022, 6, 1 ) },
                { month, timeZone, -10, new DateTime( 2020, 10, 1 ) }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, Period, DateTime> GetAddData(IFixture fixture)
        {
            var month = new DateTime( 2021, 8, 1 );
            var timeZone = TimeZoneFactory.Create( 1 );
            var timeZoneWithDaylightSaving = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 9, 26, 2, 0, 0 ),
                    transitionEnd: new DateTime( 1, 10, 26, 3, 0, 0 ) ) );

            return new TheoryData<DateTime, TimeZoneInfo, Period, DateTime>
            {
                { month, timeZone, Period.Empty, month },
                { month, timeZone, Period.FromYears( 1 ), new DateTime( 2022, 8, 1 ) },
                { month, timeZone, Period.FromMonths( 1 ), new DateTime( 2021, 9, 1 ) },
                { month, timeZone, Period.FromWeeks( 4 ), month },
                { month, timeZone, Period.FromWeeks( 5 ), new DateTime( 2021, 9, 1 ) },
                { month, timeZone, Period.FromDays( 30 ), month },
                { month, timeZone, Period.FromDays( 31 ), new DateTime( 2021, 9, 1 ) },
                { month, timeZone, Period.FromHours( 743 ), month },
                { month, timeZone, Period.FromHours( 744 ), new DateTime( 2021, 9, 1 ) },
                { month, timeZone, Period.FromMinutes( 44639 ), month },
                { month, timeZone, Period.FromMinutes( 44640 ), new DateTime( 2021, 9, 1 ) },
                { month, timeZone, Period.FromSeconds( 2678399 ), month },
                { month, timeZone, Period.FromSeconds( 2678400 ), new DateTime( 2021, 9, 1 ) },
                { month, timeZone, Period.FromMilliseconds( 2678399999 ), month },
                { month, timeZone, Period.FromMilliseconds( 2678400000 ), new DateTime( 2021, 9, 1 ) },
                { month, timeZone, Period.FromTicks( 26783999999999 ), month },
                { month, timeZone, Period.FromTicks( 26784000000000 ), new DateTime( 2021, 9, 1 ) },
                { month, timeZone, new Period( 1, 2, 3, 9, 22, 90, 1700, 80000, 200000000 ), new DateTime( 2022, 11, 1 ) },
                { month, timeZoneWithDaylightSaving, Period.FromMonths( 1 ).AddDays( 25 ).AddHours( 2 ), new DateTime( 2021, 9, 1 ) },
                {
                    month,
                    timeZoneWithDaylightSaving,
                    Period.FromMonths( 1 ).AddDays( 25 ).AddHours( 3 ).SubtractTicks( 1 ),
                    new DateTime( 2021, 9, 1 )
                },
                { month, timeZone, Period.FromTicks( -1 ), new DateTime( 2021, 7, 1 ) }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, DateTime, TimeZoneInfo, PeriodUnits> GetGetPeriodOffsetData(IFixture fixture)
        {
            var month = new DateTime( 2021, 8, 1 );
            var otherMonth = new DateTime( 2019, 10, 1 );
            var timeZone = TimeZoneFactory.Create( 1 );

            return new TheoryData<DateTime, TimeZoneInfo, DateTime, TimeZoneInfo, PeriodUnits>
            {
                { month, timeZone, otherMonth, timeZone, PeriodUnits.All },
                { month, timeZone, otherMonth, timeZone, PeriodUnits.Date },
                { month, timeZone, otherMonth, timeZone, PeriodUnits.Time },
                { month, timeZone, otherMonth, timeZone, PeriodUnits.Years },
                { month, timeZone, otherMonth, timeZone, PeriodUnits.Months },
                { month, timeZone, otherMonth, timeZone, PeriodUnits.Weeks },
                { month, timeZone, otherMonth, timeZone, PeriodUnits.Days },
                { month, timeZone, otherMonth, timeZone, PeriodUnits.Hours },
                { month, timeZone, otherMonth, timeZone, PeriodUnits.Minutes },
                { month, timeZone, otherMonth, timeZone, PeriodUnits.Seconds },
                { month, timeZone, otherMonth, timeZone, PeriodUnits.Milliseconds },
                { month, timeZone, otherMonth, timeZone, PeriodUnits.Ticks }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, DateTime, TimeZoneInfo, PeriodUnits> GetGetGreedyPeriodOffsetData(IFixture fixture)
        {
            var month = new DateTime( 2021, 8, 1 );
            var otherMonth = new DateTime( 2019, 10, 1 );
            var timeZone = TimeZoneFactory.Create( 1 );

            return new TheoryData<DateTime, TimeZoneInfo, DateTime, TimeZoneInfo, PeriodUnits>
            {
                { month, timeZone, otherMonth, timeZone, PeriodUnits.All },
                { month, timeZone, otherMonth, timeZone, PeriodUnits.Date },
                { month, timeZone, otherMonth, timeZone, PeriodUnits.Time },
                { month, timeZone, otherMonth, timeZone, PeriodUnits.Years },
                { month, timeZone, otherMonth, timeZone, PeriodUnits.Months },
                { month, timeZone, otherMonth, timeZone, PeriodUnits.Weeks },
                { month, timeZone, otherMonth, timeZone, PeriodUnits.Days },
                { month, timeZone, otherMonth, timeZone, PeriodUnits.Hours },
                { month, timeZone, otherMonth, timeZone, PeriodUnits.Minutes },
                { month, timeZone, otherMonth, timeZone, PeriodUnits.Seconds },
                { month, timeZone, otherMonth, timeZone, PeriodUnits.Milliseconds },
                { month, timeZone, otherMonth, timeZone, PeriodUnits.Ticks }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, int, DateTime> GetSetYearData(IFixture fixture)
        {
            var timeZone = TimeZoneFactory.Create( 1 );
            var timeZoneWithInvalidity = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateRule(
                    start: DateTime.MinValue,
                    end: new DateTime( 2020, 1, 1 ),
                    transitionStart: new DateTime( 1, 7, 31, 23, 30, 0 ),
                    transitionEnd: new DateTime( 1, 10, 26, 2, 0, 0 ) ) );

            return new TheoryData<DateTime, TimeZoneInfo, int, DateTime>
            {
                { new DateTime( 2021, 8, 1 ), timeZone, 2021, new DateTime( 2021, 8, 1 ) },
                { new DateTime( 2021, 8, 1 ), timeZone, 2022, new DateTime( 2022, 8, 1 ) },
                { new DateTime( 2021, 8, 1 ), timeZone, 2020, new DateTime( 2020, 8, 1 ) },
                { new DateTime( 2020, 2, 1 ), timeZone, 2021, new DateTime( 2021, 2, 1 ) },
                { new DateTime( 2021, 8, 1 ), timeZoneWithInvalidity, 2019, new DateTime( 2019, 8, 1 ) },
                { new DateTime( 2021, 7, 1 ), timeZoneWithInvalidity, 2019, new DateTime( 2019, 7, 1 ) }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, int> GetSetYearThrowData(IFixture fixture)
        {
            var timeZone = TimeZoneFactory.Create( 1 );

            return new TheoryData<DateTime, TimeZoneInfo, int>
            {
                { fixture.Create<DateTime>(), timeZone, DateTime.MinValue.Year - 1 },
                { fixture.Create<DateTime>(), timeZone, DateTime.MaxValue.Year + 1 }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, IsoMonthOfYear, DateTime> GetSetMonthData(IFixture fixture)
        {
            var timeZone = TimeZoneFactory.Create( 1 );
            var timeZoneWithInvalidity = TimeZoneFactory.Create(
                utcOffsetInHours: 1,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 7, 31, 23, 30, 0 ),
                    transitionEnd: new DateTime( 1, 10, 26, 2, 0, 0 ) ) );

            return new TheoryData<DateTime, TimeZoneInfo, IsoMonthOfYear, DateTime>
            {
                { new DateTime( 2021, 8, 1 ), timeZone, IsoMonthOfYear.August, new DateTime( 2021, 8, 1 ) },
                { new DateTime( 2021, 8, 1 ), timeZone, IsoMonthOfYear.September, new DateTime( 2021, 9, 1 ) },
                { new DateTime( 2021, 8, 1 ), timeZone, IsoMonthOfYear.July, new DateTime( 2021, 7, 1 ) },
                { new DateTime( 2021, 8, 1 ), timeZone, IsoMonthOfYear.April, new DateTime( 2021, 4, 1 ) },
                { new DateTime( 2021, 8, 1 ), timeZone, IsoMonthOfYear.February, new DateTime( 2021, 2, 1 ) },
                { new DateTime( 2020, 8, 1 ), timeZone, IsoMonthOfYear.February, new DateTime( 2020, 2, 1 ) },
                { new DateTime( 2021, 6, 1 ), timeZoneWithInvalidity, IsoMonthOfYear.August, new DateTime( 2021, 8, 1 ) },
                { new DateTime( 2021, 6, 1 ), timeZoneWithInvalidity, IsoMonthOfYear.July, new DateTime( 2021, 7, 1 ) }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, int, DateTime> GetGetDayOfMonthData(IFixture fixture)
        {
            var timeZone = TimeZoneFactory.CreateRandom( fixture );

            return new TheoryData<DateTime, TimeZoneInfo, int, DateTime>
            {
                { new DateTime( 2021, 1, 1 ), timeZone, 1, new DateTime( 2021, 1, 1 ) },
                { new DateTime( 2021, 1, 1 ), timeZone, 26, new DateTime( 2021, 1, 26 ) },
                { new DateTime( 2021, 1, 1 ), timeZone, 31, new DateTime( 2021, 1, 31 ) },
                { new DateTime( 2021, 2, 1 ), timeZone, 1, new DateTime( 2021, 2, 1 ) },
                { new DateTime( 2021, 2, 1 ), timeZone, 26, new DateTime( 2021, 2, 26 ) },
                { new DateTime( 2021, 2, 1 ), timeZone, 28, new DateTime( 2021, 2, 28 ) },
                { new DateTime( 2020, 2, 1 ), timeZone, 29, new DateTime( 2020, 2, 29 ) },
                { new DateTime( 2021, 3, 1 ), timeZone, 1, new DateTime( 2021, 3, 1 ) },
                { new DateTime( 2021, 3, 1 ), timeZone, 26, new DateTime( 2021, 3, 26 ) },
                { new DateTime( 2021, 3, 1 ), timeZone, 31, new DateTime( 2021, 3, 31 ) },
                { new DateTime( 2021, 4, 1 ), timeZone, 1, new DateTime( 2021, 4, 1 ) },
                { new DateTime( 2021, 4, 1 ), timeZone, 26, new DateTime( 2021, 4, 26 ) },
                { new DateTime( 2021, 4, 1 ), timeZone, 30, new DateTime( 2021, 4, 30 ) },
                { new DateTime( 2021, 5, 1 ), timeZone, 1, new DateTime( 2021, 5, 1 ) },
                { new DateTime( 2021, 5, 1 ), timeZone, 26, new DateTime( 2021, 5, 26 ) },
                { new DateTime( 2021, 5, 1 ), timeZone, 31, new DateTime( 2021, 5, 31 ) },
                { new DateTime( 2021, 6, 1 ), timeZone, 1, new DateTime( 2021, 6, 1 ) },
                { new DateTime( 2021, 6, 1 ), timeZone, 26, new DateTime( 2021, 6, 26 ) },
                { new DateTime( 2021, 6, 1 ), timeZone, 30, new DateTime( 2021, 6, 30 ) },
                { new DateTime( 2021, 7, 1 ), timeZone, 1, new DateTime( 2021, 7, 1 ) },
                { new DateTime( 2021, 7, 1 ), timeZone, 26, new DateTime( 2021, 7, 26 ) },
                { new DateTime( 2021, 7, 1 ), timeZone, 31, new DateTime( 2021, 7, 31 ) },
                { new DateTime( 2021, 8, 1 ), timeZone, 1, new DateTime( 2021, 8, 1 ) },
                { new DateTime( 2021, 8, 1 ), timeZone, 26, new DateTime( 2021, 8, 26 ) },
                { new DateTime( 2021, 8, 1 ), timeZone, 31, new DateTime( 2021, 8, 31 ) },
                { new DateTime( 2021, 9, 1 ), timeZone, 1, new DateTime( 2021, 9, 1 ) },
                { new DateTime( 2021, 9, 1 ), timeZone, 26, new DateTime( 2021, 9, 26 ) },
                { new DateTime( 2021, 9, 1 ), timeZone, 30, new DateTime( 2021, 9, 30 ) },
                { new DateTime( 2021, 10, 1 ), timeZone, 1, new DateTime( 2021, 10, 1 ) },
                { new DateTime( 2021, 10, 1 ), timeZone, 26, new DateTime( 2021, 10, 26 ) },
                { new DateTime( 2021, 10, 1 ), timeZone, 31, new DateTime( 2021, 10, 31 ) },
                { new DateTime( 2021, 11, 1 ), timeZone, 1, new DateTime( 2021, 11, 1 ) },
                { new DateTime( 2021, 11, 1 ), timeZone, 26, new DateTime( 2021, 11, 26 ) },
                { new DateTime( 2021, 11, 1 ), timeZone, 30, new DateTime( 2021, 11, 30 ) },
                { new DateTime( 2021, 12, 1 ), timeZone, 1, new DateTime( 2021, 12, 1 ) },
                { new DateTime( 2021, 12, 1 ), timeZone, 26, new DateTime( 2021, 12, 26 ) },
                { new DateTime( 2021, 12, 1 ), timeZone, 31, new DateTime( 2021, 12, 31 ) }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, int> GetGetDayOfMonthThrowData(IFixture fixture)
        {
            var timeZone = TimeZoneFactory.CreateRandom( fixture );

            return new TheoryData<DateTime, TimeZoneInfo, int>
            {
                { new DateTime( 2021, 1, 1 ), timeZone, -1 },
                { new DateTime( 2021, 1, 1 ), timeZone, 0 },
                { new DateTime( 2021, 1, 1 ), timeZone, 32 },
                { new DateTime( 2021, 2, 1 ), timeZone, 29 },
                { new DateTime( 2020, 2, 1 ), timeZone, 30 },
                { new DateTime( 2021, 3, 1 ), timeZone, 32 },
                { new DateTime( 2021, 4, 1 ), timeZone, 31 },
                { new DateTime( 2021, 5, 1 ), timeZone, 32 },
                { new DateTime( 2021, 6, 1 ), timeZone, 31 },
                { new DateTime( 2021, 7, 1 ), timeZone, 32 },
                { new DateTime( 2021, 8, 1 ), timeZone, 32 },
                { new DateTime( 2021, 9, 1 ), timeZone, 31 },
                { new DateTime( 2021, 10, 1 ), timeZone, 32 },
                { new DateTime( 2021, 11, 1 ), timeZone, 31 },
                { new DateTime( 2021, 12, 1 ), timeZone, 32 }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, int> GetGetAllDaysData(IFixture fixture)
        {
            var timeZone = TimeZoneFactory.CreateRandom( fixture );

            return new TheoryData<DateTime, TimeZoneInfo, int>
            {
                { new DateTime( 2021, 1, 1 ), timeZone, 31 },
                { new DateTime( 2021, 2, 1 ), timeZone, 28 },
                { new DateTime( 2020, 2, 1 ), timeZone, 29 },
                { new DateTime( 2021, 3, 1 ), timeZone, 31 },
                { new DateTime( 2021, 4, 1 ), timeZone, 30 },
                { new DateTime( 2021, 5, 1 ), timeZone, 31 },
                { new DateTime( 2021, 6, 1 ), timeZone, 30 },
                { new DateTime( 2021, 7, 1 ), timeZone, 31 },
                { new DateTime( 2021, 8, 1 ), timeZone, 31 },
                { new DateTime( 2021, 9, 1 ), timeZone, 30 },
                { new DateTime( 2021, 10, 1 ), timeZone, 31 },
                { new DateTime( 2021, 11, 1 ), timeZone, 30 },
                { new DateTime( 2021, 12, 1 ), timeZone, 31 }
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
