using System.Collections.Generic;

namespace LfrlAnvil.Chrono.Tests.ZonedWeekTests;

public class ZonedWeekTestsData
{
    public static TheoryData<DateTime, TimeZoneInfo, IsoDayOfWeek, int, int> GetCreateData(IFixture fixture)
    {
        var timeZone = TimeZoneFactory.CreateRandom( fixture );

        return new TheoryData<DateTime, TimeZoneInfo, IsoDayOfWeek, int, int>
        {
            { new DateTime( 2021, 1, 1 ), timeZone, IsoDayOfWeek.Monday, 2020, 53 },
            { new DateTime( 2021, 1, 3 ), timeZone, IsoDayOfWeek.Monday, 2020, 53 },
            { new DateTime( 2021, 1, 4 ), timeZone, IsoDayOfWeek.Monday, 2021, 1 },
            { new DateTime( 2021, 8, 26 ), timeZone, IsoDayOfWeek.Monday, 2021, 34 },
            { new DateTime( 2021, 12, 31 ), timeZone, IsoDayOfWeek.Monday, 2021, 52 },
            { new DateTime( 2022, 1, 2 ), timeZone, IsoDayOfWeek.Monday, 2021, 52 },
            { new DateTime( 2022, 1, 3 ), timeZone, IsoDayOfWeek.Monday, 2022, 1 },
            { new DateTime( 2021, 1, 1 ), timeZone, IsoDayOfWeek.Tuesday, 2021, 1 },
            { new DateTime( 2020, 12, 28 ), timeZone, IsoDayOfWeek.Tuesday, 2020, 52 },
            { new DateTime( 2020, 12, 29 ), timeZone, IsoDayOfWeek.Tuesday, 2021, 1 },
            { new DateTime( 2021, 8, 26 ), timeZone, IsoDayOfWeek.Tuesday, 2021, 35 },
            { new DateTime( 2021, 12, 31 ), timeZone, IsoDayOfWeek.Tuesday, 2022, 1 },
            { new DateTime( 2021, 12, 27 ), timeZone, IsoDayOfWeek.Tuesday, 2021, 52 },
            { new DateTime( 2021, 12, 28 ), timeZone, IsoDayOfWeek.Tuesday, 2022, 1 },
            { new DateTime( 2021, 1, 1 ), timeZone, IsoDayOfWeek.Wednesday, 2021, 1 },
            { new DateTime( 2020, 12, 29 ), timeZone, IsoDayOfWeek.Wednesday, 2020, 52 },
            { new DateTime( 2020, 12, 30 ), timeZone, IsoDayOfWeek.Wednesday, 2021, 1 },
            { new DateTime( 2021, 8, 26 ), timeZone, IsoDayOfWeek.Wednesday, 2021, 35 },
            { new DateTime( 2021, 12, 31 ), timeZone, IsoDayOfWeek.Wednesday, 2022, 1 },
            { new DateTime( 2021, 12, 28 ), timeZone, IsoDayOfWeek.Wednesday, 2021, 52 },
            { new DateTime( 2021, 12, 29 ), timeZone, IsoDayOfWeek.Wednesday, 2022, 1 },
            { new DateTime( 2021, 1, 1 ), timeZone, IsoDayOfWeek.Thursday, 2021, 1 },
            { new DateTime( 2020, 12, 30 ), timeZone, IsoDayOfWeek.Thursday, 2020, 53 },
            { new DateTime( 2020, 12, 31 ), timeZone, IsoDayOfWeek.Thursday, 2021, 1 },
            { new DateTime( 2021, 8, 26 ), timeZone, IsoDayOfWeek.Thursday, 2021, 35 },
            { new DateTime( 2021, 12, 31 ), timeZone, IsoDayOfWeek.Thursday, 2022, 1 },
            { new DateTime( 2021, 12, 29 ), timeZone, IsoDayOfWeek.Thursday, 2021, 52 },
            { new DateTime( 2021, 12, 30 ), timeZone, IsoDayOfWeek.Thursday, 2022, 1 },
            { new DateTime( 2021, 1, 1 ), timeZone, IsoDayOfWeek.Friday, 2021, 1 },
            { new DateTime( 2020, 12, 31 ), timeZone, IsoDayOfWeek.Friday, 2020, 53 },
            { new DateTime( 2021, 8, 26 ), timeZone, IsoDayOfWeek.Friday, 2021, 34 },
            { new DateTime( 2021, 12, 31 ), timeZone, IsoDayOfWeek.Friday, 2022, 1 },
            { new DateTime( 2021, 12, 30 ), timeZone, IsoDayOfWeek.Friday, 2021, 52 },
            { new DateTime( 2021, 1, 1 ), timeZone, IsoDayOfWeek.Saturday, 2021, 1 },
            { new DateTime( 2020, 12, 25 ), timeZone, IsoDayOfWeek.Saturday, 2020, 52 },
            { new DateTime( 2020, 12, 26 ), timeZone, IsoDayOfWeek.Saturday, 2021, 1 },
            { new DateTime( 2021, 8, 26 ), timeZone, IsoDayOfWeek.Saturday, 2021, 35 },
            { new DateTime( 2021, 12, 31 ), timeZone, IsoDayOfWeek.Saturday, 2021, 53 },
            { new DateTime( 2022, 1, 1 ), timeZone, IsoDayOfWeek.Saturday, 2022, 1 },
            { new DateTime( 2021, 1, 1 ), timeZone, IsoDayOfWeek.Sunday, 2021, 1 },
            { new DateTime( 2020, 12, 26 ), timeZone, IsoDayOfWeek.Sunday, 2020, 52 },
            { new DateTime( 2020, 12, 27 ), timeZone, IsoDayOfWeek.Sunday, 2021, 1 },
            { new DateTime( 2021, 8, 26 ), timeZone, IsoDayOfWeek.Sunday, 2021, 35 },
            { new DateTime( 2021, 12, 31 ), timeZone, IsoDayOfWeek.Sunday, 2022, 1 },
            { new DateTime( 2021, 12, 25 ), timeZone, IsoDayOfWeek.Sunday, 2021, 52 },
            { new DateTime( 2021, 12, 26 ), timeZone, IsoDayOfWeek.Sunday, 2022, 1 }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, IsoDayOfWeek, int, int, Duration> GetCreateWithContainedInvalidityRangeData(
        IFixture fixture)
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

        return new TheoryData<DateTime, TimeZoneInfo, IsoDayOfWeek, int, int, Duration>
        {
            { new DateTime( 2021, 8, 26 ), positiveTimeZone, IsoDayOfWeek.Thursday, 2021, 35, Duration.FromHours( 167 ) },
            { new DateTime( 2021, 8, 26 ), negativeTimeZone, IsoDayOfWeek.Thursday, 2021, 35, Duration.FromHours( 167 ) },
            { new DateTime( 2019, 8, 26 ), positiveTimeZone, IsoDayOfWeek.Monday, 2019, 35, Duration.FromHours( 167 ) },
            { new DateTime( 2019, 8, 26 ), negativeTimeZone, IsoDayOfWeek.Monday, 2019, 35, Duration.FromHours( 167 ) },
            { new DateTime( 2021, 8, 26, 2, 0, 0 ), positiveTimeZone, IsoDayOfWeek.Thursday, 2021, 35, Duration.FromHours( 167 ) },
            {
                new DateTime( 2021, 8, 26, 2, 59, 59, 999 ).AddTicks( 9999 ),
                negativeTimeZone,
                IsoDayOfWeek.Thursday,
                2021,
                35,
                Duration.FromHours( 167 )
            },
            { new DateTime( 2019, 8, 26, 2, 0, 0 ), positiveTimeZone, IsoDayOfWeek.Monday, 2019, 35, Duration.FromHours( 167 ) },
            {
                new DateTime( 2019, 8, 26, 2, 59, 59, 999 ).AddTicks( 9999 ),
                negativeTimeZone,
                IsoDayOfWeek.Monday,
                2019,
                35,
                Duration.FromHours( 167 )
            }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, IsoDayOfWeek, int, int, Duration> GetCreateWithContainedAmbiguityRangeData(
        IFixture fixture)
    {
        var positiveTimeZone = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 8, 26, 2, 0, 0 ),
                transitionEnd: new DateTime( 1, 9, 26, 3, 0, 0 ) ) );

        var negativeTimeZone = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 9, 26, 3, 0, 0 ),
                transitionEnd: new DateTime( 1, 10, 26, 2, 0, 0 ),
                daylightDeltaInHours: -1 ) );

        return new TheoryData<DateTime, TimeZoneInfo, IsoDayOfWeek, int, int, Duration>
        {
            { new DateTime( 2021, 9, 26 ), positiveTimeZone, IsoDayOfWeek.Sunday, 2021, 40, Duration.FromHours( 169 ) },
            { new DateTime( 2021, 9, 26 ), negativeTimeZone, IsoDayOfWeek.Sunday, 2021, 40, Duration.FromHours( 169 ) },
            { new DateTime( 2018, 9, 26 ), positiveTimeZone, IsoDayOfWeek.Wednesday, 2018, 40, Duration.FromHours( 169 ) },
            { new DateTime( 2018, 9, 26 ), negativeTimeZone, IsoDayOfWeek.Wednesday, 2018, 40, Duration.FromHours( 169 ) },
            { new DateTime( 2021, 9, 26, 2, 0, 0 ), positiveTimeZone, IsoDayOfWeek.Sunday, 2021, 40, Duration.FromHours( 169 ) },
            {
                new DateTime( 2021, 9, 26, 2, 59, 59, 999 ).AddTicks( 9999 ),
                negativeTimeZone,
                IsoDayOfWeek.Sunday,
                2021,
                40,
                Duration.FromHours( 169 )
            },
            { new DateTime( 2018, 9, 26, 2, 0, 0 ), positiveTimeZone, IsoDayOfWeek.Wednesday, 2018, 40, Duration.FromHours( 169 ) },
            {
                new DateTime( 2018, 9, 26, 2, 59, 59, 999 ).AddTicks( 9999 ),
                negativeTimeZone,
                IsoDayOfWeek.Wednesday,
                2018,
                40,
                Duration.FromHours( 169 )
            }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, DateTime, IsoDayOfWeek, int, int, Duration>
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

        return new TheoryData<DateTime, TimeZoneInfo, DateTime, IsoDayOfWeek, int, int, Duration>
        {
            {
                new DateTime( 2021, 8, 1 ),
                positiveTimeZone1,
                new DateTime( 2021, 8, 1, 0, 1, 0 ),
                IsoDayOfWeek.Sunday,
                2021,
                32,
                Duration.FromHours( 168 ).SubtractMinutes( 1 )
            },
            {
                new DateTime( 2021, 7, 1 ),
                positiveTimeZone2,
                new DateTime( 2021, 7, 1, 0, 30, 0 ),
                IsoDayOfWeek.Thursday,
                2021,
                27,
                Duration.FromHours( 168 ).SubtractMinutes( 30 )
            },
            {
                new DateTime( 2021, 6, 1 ),
                positiveTimeZone3,
                new DateTime( 2021, 6, 1, 0, 59, 0 ),
                IsoDayOfWeek.Tuesday,
                2021,
                23,
                Duration.FromHours( 168 ).SubtractMinutes( 59 )
            },
            {
                new DateTime( 2021, 5, 1 ),
                positiveTimeZone4,
                new DateTime( 2021, 5, 1, 1, 0, 0 ),
                IsoDayOfWeek.Saturday,
                2021,
                19,
                Duration.FromHours( 167 )
            },
            {
                new DateTime( 2021, 8, 1 ),
                negativeTimeZone1,
                new DateTime( 2021, 8, 1, 0, 1, 0 ),
                IsoDayOfWeek.Sunday,
                2021,
                32,
                Duration.FromHours( 168 ).SubtractMinutes( 1 )
            },
            {
                new DateTime( 2021, 7, 1 ),
                negativeTimeZone2,
                new DateTime( 2021, 7, 1, 0, 30, 0 ),
                IsoDayOfWeek.Thursday,
                2021,
                27,
                Duration.FromHours( 168 ).SubtractMinutes( 30 )
            },
            {
                new DateTime( 2021, 6, 1 ),
                negativeTimeZone3,
                new DateTime( 2021, 6, 1, 0, 59, 0 ),
                IsoDayOfWeek.Tuesday,
                2021,
                23,
                Duration.FromHours( 168 ).SubtractMinutes( 59 )
            },
            {
                new DateTime( 2021, 5, 1 ),
                negativeTimeZone4,
                new DateTime( 2021, 5, 1, 1, 0, 0 ),
                IsoDayOfWeek.Saturday,
                2021,
                19,
                Duration.FromHours( 167 )
            }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, DateTime, IsoDayOfWeek, int, int, Duration>
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

        return new TheoryData<DateTime, TimeZoneInfo, DateTime, IsoDayOfWeek, int, int, Duration>
        {
            {
                new DateTime( 2021, 8, 31 ),
                positiveTimeZone1,
                new DateTime( 2021, 8, 31, 23, 0, 59, 999 ).AddTicks( 9999 ),
                IsoDayOfWeek.Wednesday,
                2021,
                35,
                Duration.FromHours( 168 ).SubtractMinutes( 59 )
            },
            {
                new DateTime( 2021, 9, 30 ),
                positiveTimeZone2,
                new DateTime( 2021, 9, 30, 23, 29, 59, 999 ).AddTicks( 9999 ),
                IsoDayOfWeek.Friday,
                2021,
                39,
                Duration.FromHours( 168 ).SubtractMinutes( 30 )
            },
            {
                new DateTime( 2021, 10, 31 ),
                positiveTimeZone3,
                new DateTime( 2021, 10, 31, 23, 58, 59, 999 ).AddTicks( 9999 ),
                IsoDayOfWeek.Monday,
                2021,
                43,
                Duration.FromHours( 168 ).SubtractMinutes( 1 )
            },
            {
                new DateTime( 2021, 11, 30 ),
                positiveTimeZone4,
                new DateTime( 2021, 11, 30, 22, 59, 59, 999 ).AddTicks( 9999 ),
                IsoDayOfWeek.Wednesday,
                2021,
                48,
                Duration.FromHours( 167 )
            },
            {
                new DateTime( 2021, 8, 31 ),
                negativeTimeZone1,
                new DateTime( 2021, 8, 31, 23, 0, 59, 999 ).AddTicks( 9999 ),
                IsoDayOfWeek.Wednesday,
                2021,
                35,
                Duration.FromHours( 168 ).SubtractMinutes( 59 )
            },
            {
                new DateTime( 2021, 9, 30 ),
                negativeTimeZone2,
                new DateTime( 2021, 9, 30, 23, 29, 59, 999 ).AddTicks( 9999 ),
                IsoDayOfWeek.Friday,
                2021,
                39,
                Duration.FromHours( 168 ).SubtractMinutes( 30 )
            },
            {
                new DateTime( 2021, 10, 31 ),
                negativeTimeZone3,
                new DateTime( 2021, 10, 31, 23, 58, 59, 999 ).AddTicks( 9999 ),
                IsoDayOfWeek.Monday,
                2021,
                43,
                Duration.FromHours( 168 ).SubtractMinutes( 1 )
            },
            {
                new DateTime( 2021, 11, 30 ),
                negativeTimeZone4,
                new DateTime( 2021, 11, 30, 22, 59, 59, 999 ).AddTicks( 9999 ),
                IsoDayOfWeek.Wednesday,
                2021,
                48,
                Duration.FromHours( 167 )
            }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, bool, IsoDayOfWeek, int, int, Duration>
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

        return new TheoryData<DateTime, TimeZoneInfo, bool, IsoDayOfWeek, int, int, Duration>
        {
            {
                new DateTime( 2021, 8, 1 ),
                positiveTimeZone1,
                true,
                IsoDayOfWeek.Sunday,
                2021,
                32,
                Duration.FromHours( 168 ).AddMinutes( 1 )
            },
            {
                new DateTime( 2021, 7, 1 ),
                positiveTimeZone2,
                true,
                IsoDayOfWeek.Thursday,
                2021,
                27,
                Duration.FromHours( 168 ).AddMinutes( 30 )
            },
            {
                new DateTime( 2021, 6, 1 ),
                positiveTimeZone3,
                true,
                IsoDayOfWeek.Tuesday,
                2021,
                23,
                Duration.FromHours( 168 ).AddMinutes( 59 )
            },
            {
                new DateTime( 2021, 5, 1 ),
                positiveTimeZone4,
                true,
                IsoDayOfWeek.Saturday,
                2021,
                19,
                Duration.FromHours( 169 )
            },
            {
                new DateTime( 2021, 8, 1 ),
                negativeTimeZone1,
                false,
                IsoDayOfWeek.Sunday,
                2021,
                32,
                Duration.FromHours( 168 ).AddMinutes( 1 )
            },
            {
                new DateTime( 2021, 7, 1 ),
                negativeTimeZone2,
                false,
                IsoDayOfWeek.Thursday,
                2021,
                27,
                Duration.FromHours( 168 ).AddMinutes( 30 )
            },
            {
                new DateTime( 2021, 6, 1 ),
                negativeTimeZone3,
                false,
                IsoDayOfWeek.Tuesday,
                2021,
                23,
                Duration.FromHours( 168 ).AddMinutes( 59 )
            },
            {
                new DateTime( 2021, 5, 1 ),
                negativeTimeZone4,
                false,
                IsoDayOfWeek.Saturday,
                2021,
                19,
                Duration.FromHours( 169 )
            }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, bool, IsoDayOfWeek, int, int, Duration>
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

        return new TheoryData<DateTime, TimeZoneInfo, bool, IsoDayOfWeek, int, int, Duration>
        {
            {
                new DateTime( 2021, 8, 31 ),
                positiveTimeZone1,
                false,
                IsoDayOfWeek.Wednesday,
                2021,
                35,
                Duration.FromHours( 168 ).AddMinutes( 59 )
            },
            {
                new DateTime( 2021, 9, 30 ),
                positiveTimeZone2,
                false,
                IsoDayOfWeek.Friday,
                2021,
                39,
                Duration.FromHours( 168 ).AddMinutes( 30 )
            },
            {
                new DateTime( 2021, 10, 31 ),
                positiveTimeZone3,
                false,
                IsoDayOfWeek.Monday,
                2021,
                43,
                Duration.FromHours( 168 ).AddMinutes( 1 )
            },
            {
                new DateTime( 2021, 11, 30 ),
                positiveTimeZone4,
                false,
                IsoDayOfWeek.Wednesday,
                2021,
                48,
                Duration.FromHours( 169 )
            },
            {
                new DateTime( 2021, 8, 31 ),
                negativeTimeZone1,
                true,
                IsoDayOfWeek.Wednesday,
                2021,
                35,
                Duration.FromHours( 168 ).AddMinutes( 59 )
            },
            {
                new DateTime( 2021, 9, 30 ),
                negativeTimeZone2,
                true,
                IsoDayOfWeek.Friday,
                2021,
                39,
                Duration.FromHours( 168 ).AddMinutes( 30 )
            },
            {
                new DateTime( 2021, 10, 31 ),
                negativeTimeZone3,
                true,
                IsoDayOfWeek.Monday,
                2021,
                43,
                Duration.FromHours( 168 ).AddMinutes( 1 )
            },
            {
                new DateTime( 2021, 11, 30 ),
                negativeTimeZone4,
                true,
                IsoDayOfWeek.Wednesday,
                2021,
                48,
                Duration.FromHours( 169 )
            }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, DateTime, bool, IsoDayOfWeek, int, int, Duration>
        GetCreateWithInvalidStartTimeAndAmbiguousEndTimeData(IFixture fixture)
    {
        var positiveTimeZone1 = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 7, 31, 23, 1, 0 ),
                transitionEnd: new DateTime( 1, 8, 8, 0, 1, 0 ) ) );

        var positiveTimeZone2 = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 6, 30, 23, 59, 0 ),
                transitionEnd: new DateTime( 1, 7, 8, 0, 59, 0 ) ) );

        var positiveTimeZone3 = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 6, 1, 0, 0, 0 ),
                transitionEnd: new DateTime( 1, 6, 8, 0, 0, 0 ) ) );

        var negativeTimeZone1 = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 8, 8, 0, 1, 0 ),
                transitionEnd: new DateTime( 1, 7, 31, 23, 1, 0 ),
                daylightDeltaInHours: -1 ) );

        var negativeTimeZone2 = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 7, 8, 0, 59, 0 ),
                transitionEnd: new DateTime( 1, 6, 30, 23, 59, 0 ),
                daylightDeltaInHours: -1 ) );

        var negativeTimeZone3 = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 6, 8, 0, 0, 0 ),
                transitionEnd: new DateTime( 1, 6, 1, 0, 0, 0 ),
                daylightDeltaInHours: -1 ) );

        return new TheoryData<DateTime, TimeZoneInfo, DateTime, bool, IsoDayOfWeek, int, int, Duration>
        {
            {
                new DateTime( 2021, 8, 1 ),
                positiveTimeZone1,
                new DateTime( 2021, 8, 1, 0, 1, 0 ),
                false,
                IsoDayOfWeek.Sunday,
                2021,
                32,
                Duration.FromHours( 168 ).AddMinutes( 58 )
            },
            {
                new DateTime( 2021, 7, 1 ),
                positiveTimeZone2,
                new DateTime( 2021, 7, 1, 0, 59, 0 ),
                false,
                IsoDayOfWeek.Thursday,
                2021,
                27,
                Duration.FromHours( 168 ).SubtractMinutes( 58 )
            },
            {
                new DateTime( 2021, 6, 1 ),
                positiveTimeZone3,
                new DateTime( 2021, 6, 1, 1, 0, 0 ),
                false,
                IsoDayOfWeek.Tuesday,
                2021,
                23,
                Duration.FromHours( 168 )
            },
            {
                new DateTime( 2021, 8, 1 ),
                negativeTimeZone1,
                new DateTime( 2021, 8, 1, 0, 1, 0 ),
                true,
                IsoDayOfWeek.Sunday,
                2021,
                32,
                Duration.FromHours( 168 ).AddMinutes( 58 )
            },
            {
                new DateTime( 2021, 7, 1 ),
                negativeTimeZone2,
                new DateTime( 2021, 7, 1, 0, 59, 0 ),
                true,
                IsoDayOfWeek.Thursday,
                2021,
                27,
                Duration.FromHours( 168 ).SubtractMinutes( 58 )
            },
            {
                new DateTime( 2021, 6, 1 ),
                negativeTimeZone3,
                new DateTime( 2021, 6, 1, 1, 0, 0 ),
                true,
                IsoDayOfWeek.Tuesday,
                2021,
                23,
                Duration.FromHours( 168 )
            }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, DateTime, bool, IsoDayOfWeek, int, int, Duration>
        GetCreateWithAmbiguousStartTimeAndInvalidEndTimeData(IFixture fixture)
    {
        var positiveTimeZone1 = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 8, 31, 23, 59, 0 ),
                transitionEnd: new DateTime( 1, 8, 25, 0, 59, 0 ) ) );

        var positiveTimeZone2 = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 7, 31, 23, 1, 0 ),
                transitionEnd: new DateTime( 1, 7, 25, 0, 1, 0 ) ) );

        var positiveTimeZone3 = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 6, 30, 23, 0, 0 ),
                transitionEnd: new DateTime( 1, 6, 24, 1, 0, 0 ) ) );

        var negativeTimeZone1 = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 8, 25, 0, 59, 0 ),
                transitionEnd: new DateTime( 1, 8, 31, 23, 59, 0 ),
                daylightDeltaInHours: -1 ) );

        var negativeTimeZone2 = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 7, 25, 0, 1, 0 ),
                transitionEnd: new DateTime( 1, 7, 31, 23, 1, 0 ),
                daylightDeltaInHours: -1 ) );

        var negativeTimeZone3 = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 6, 24, 1, 0, 0 ),
                transitionEnd: new DateTime( 1, 6, 30, 23, 0, 0 ),
                daylightDeltaInHours: -1 ) );

        return new TheoryData<DateTime, TimeZoneInfo, DateTime, bool, IsoDayOfWeek, int, int, Duration>
        {
            {
                new DateTime( 2021, 8, 31 ),
                positiveTimeZone1,
                new DateTime( 2021, 8, 31, 23, 58, 59, 999 ).AddTicks( 9999 ),
                true,
                IsoDayOfWeek.Wednesday,
                2021,
                35,
                Duration.FromHours( 168 ).AddMinutes( 58 )
            },
            {
                new DateTime( 2021, 7, 31 ),
                positiveTimeZone2,
                new DateTime( 2021, 7, 31, 23, 0, 59, 999 ).AddTicks( 9999 ),
                true,
                IsoDayOfWeek.Sunday,
                2021,
                31,
                Duration.FromHours( 168 ).SubtractMinutes( 58 )
            },
            {
                new DateTime( 2021, 6, 30 ),
                positiveTimeZone3,
                new DateTime( 2021, 6, 30, 22, 59, 59, 999 ).AddTicks( 9999 ),
                true,
                IsoDayOfWeek.Thursday,
                2021,
                26,
                Duration.FromHours( 168 )
            },
            {
                new DateTime( 2021, 8, 31 ),
                negativeTimeZone1,
                new DateTime( 2021, 8, 31, 23, 58, 59, 999 ).AddTicks( 9999 ),
                false,
                IsoDayOfWeek.Wednesday,
                2021,
                35,
                Duration.FromHours( 168 ).AddMinutes( 58 )
            },
            {
                new DateTime( 2021, 7, 31 ),
                negativeTimeZone2,
                new DateTime( 2021, 7, 31, 23, 0, 59, 999 ).AddTicks( 9999 ),
                false,
                IsoDayOfWeek.Sunday,
                2021,
                31,
                Duration.FromHours( 168 ).SubtractMinutes( 58 )
            },
            {
                new DateTime( 2021, 6, 30 ),
                negativeTimeZone3,
                new DateTime( 2021, 6, 30, 22, 59, 59, 999 ).AddTicks( 9999 ),
                false,
                IsoDayOfWeek.Thursday,
                2021,
                26,
                Duration.FromHours( 168 )
            }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, IsoDayOfWeek, int, int> GetCreateWithContainedInvalidityAndAmbiguityRangesData(
        IFixture fixture)
    {
        var timeZone1 = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 3, 10, 2, 0, 0 ),
                transitionEnd: new DateTime( 1, 3, 14, 3, 0, 0 ) ) );

        var timeZone2 = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 4, 14, 2, 0, 0 ),
                transitionEnd: new DateTime( 1, 4, 10, 3, 0, 0 ) ) );

        var timeZone3 = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 5, 10, 3, 0, 0 ),
                transitionEnd: new DateTime( 1, 5, 14, 2, 0, 0 ),
                daylightDeltaInHours: -1.0 ) );

        var timeZone4 = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 6, 14, 3, 0, 0 ),
                transitionEnd: new DateTime( 1, 6, 10, 2, 0, 0 ),
                daylightDeltaInHours: -1.0 ) );

        return new TheoryData<DateTime, TimeZoneInfo, IsoDayOfWeek, int, int>
        {
            { new DateTime( 2021, 3, 10 ), timeZone1, IsoDayOfWeek.Wednesday, 2021, 11 },
            { new DateTime( 2021, 4, 10 ), timeZone2, IsoDayOfWeek.Saturday, 2021, 16 },
            { new DateTime( 2021, 5, 10 ), timeZone3, IsoDayOfWeek.Monday, 2021, 19 },
            { new DateTime( 2021, 6, 10 ), timeZone4, IsoDayOfWeek.Thursday, 2021, 24 }
        };
    }

    public static TheoryData<int, int, IsoDayOfWeek, DateTime> GetCreateWithWeekOfYearData(IFixture fixture)
    {
        return new TheoryData<int, int, IsoDayOfWeek, DateTime>
        {
            { 2020, 53, IsoDayOfWeek.Monday, new DateTime( 2020, 12, 28 ) },
            { 2020, 52, IsoDayOfWeek.Tuesday, new DateTime( 2020, 12, 22 ) },
            { 2020, 52, IsoDayOfWeek.Wednesday, new DateTime( 2020, 12, 23 ) },
            { 2020, 53, IsoDayOfWeek.Thursday, new DateTime( 2020, 12, 24 ) },
            { 2020, 53, IsoDayOfWeek.Friday, new DateTime( 2020, 12, 25 ) },
            { 2020, 52, IsoDayOfWeek.Saturday, new DateTime( 2020, 12, 19 ) },
            { 2020, 52, IsoDayOfWeek.Sunday, new DateTime( 2020, 12, 20 ) },
            { 2021, 1, IsoDayOfWeek.Monday, new DateTime( 2021, 1, 4 ) },
            { 2021, 1, IsoDayOfWeek.Tuesday, new DateTime( 2020, 12, 29 ) },
            { 2021, 1, IsoDayOfWeek.Wednesday, new DateTime( 2020, 12, 30 ) },
            { 2021, 1, IsoDayOfWeek.Thursday, new DateTime( 2020, 12, 31 ) },
            { 2021, 1, IsoDayOfWeek.Friday, new DateTime( 2021, 1, 1 ) },
            { 2021, 1, IsoDayOfWeek.Saturday, new DateTime( 2020, 12, 26 ) },
            { 2021, 1, IsoDayOfWeek.Sunday, new DateTime( 2020, 12, 27 ) },
            { 2021, 27, IsoDayOfWeek.Monday, new DateTime( 2021, 7, 5 ) },
            { 2021, 27, IsoDayOfWeek.Tuesday, new DateTime( 2021, 6, 29 ) },
            { 2021, 27, IsoDayOfWeek.Wednesday, new DateTime( 2021, 6, 30 ) },
            { 2021, 27, IsoDayOfWeek.Thursday, new DateTime( 2021, 7, 1 ) },
            { 2021, 27, IsoDayOfWeek.Friday, new DateTime( 2021, 7, 2 ) },
            { 2021, 27, IsoDayOfWeek.Saturday, new DateTime( 2021, 6, 26 ) },
            { 2021, 27, IsoDayOfWeek.Sunday, new DateTime( 2021, 6, 27 ) }
        };
    }

    public static TheoryData<int, int, IsoDayOfWeek> GetCreateWithWeekOfYearThrowData(IFixture fixture)
    {
        return new TheoryData<int, int, IsoDayOfWeek>
        {
            { 2020, -1, IsoDayOfWeek.Monday },
            { 2020, 0, IsoDayOfWeek.Monday },
            { 2020, 54, IsoDayOfWeek.Monday },
            { 2020, 53, IsoDayOfWeek.Tuesday },
            { 2020, 53, IsoDayOfWeek.Wednesday },
            { 2020, 54, IsoDayOfWeek.Thursday },
            { 2020, 54, IsoDayOfWeek.Friday },
            { 2020, 53, IsoDayOfWeek.Saturday },
            { 2020, 53, IsoDayOfWeek.Sunday },
        };
    }

    public static TheoryData<int, int, TimeZoneInfo, IsoDayOfWeek, string> GetToStringData(IFixture fixture)
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

        return new TheoryData<int, int, TimeZoneInfo, IsoDayOfWeek, string>
        {
            { 2021, 1, tz1, IsoDayOfWeek.Monday, $"2021-W01 (Monday-Sunday) +03:00 ({tz1.Id})" },
            { 2020, 13, tz1, IsoDayOfWeek.Tuesday, $"2020-W13 (Tuesday-Monday) +03:00 +04:00 ({tz1.Id})" },
            { 2019, 20, tz1, IsoDayOfWeek.Wednesday, $"2019-W20 (Wednesday-Tuesday) +04:00 ({tz1.Id})" },
            { 2018, 39, tz1, IsoDayOfWeek.Thursday, $"2018-W39 (Thursday-Wednesday) +04:00 +03:00 ({tz1.Id})" },
            { 2017, 50, tz1, IsoDayOfWeek.Friday, $"2017-W50 (Friday-Thursday) +03:00 ({tz1.Id})" },
            { 2021, 1, tz2, IsoDayOfWeek.Saturday, $"2021-W01 (Saturday-Friday) -05:00 ({tz2.Id})" },
            { 2020, 13, tz2, IsoDayOfWeek.Tuesday, $"2020-W13 (Tuesday-Monday) -05:00 -04:00 ({tz2.Id})" },
            { 2019, 20, tz2, IsoDayOfWeek.Wednesday, $"2019-W20 (Wednesday-Tuesday) -04:00 ({tz2.Id})" },
            { 2018, 39, tz2, IsoDayOfWeek.Thursday, $"2018-W39 (Thursday-Wednesday) -04:00 -05:00 ({tz2.Id})" },
            { 2017, 50, tz2, IsoDayOfWeek.Sunday, $"2017-W50 (Sunday-Saturday) -05:00 ({tz2.Id})" }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, IsoDayOfWeek, DateTime, TimeZoneInfo, IsoDayOfWeek, bool> GetEqualsData(
        IFixture fixture)
    {
        var (dt1, dt2) = (new DateTime( 2021, 8, 1 ), new DateTime( 2021, 9, 1 ));
        var (tz1, tz2) = (TimeZoneFactory.Create( 3 ), TimeZoneFactory.Create( 5 ));

        return new TheoryData<DateTime, TimeZoneInfo, IsoDayOfWeek, DateTime, TimeZoneInfo, IsoDayOfWeek, bool>
        {
            { dt1, tz1, IsoDayOfWeek.Monday, dt1, tz1, IsoDayOfWeek.Monday, true },
            { dt1, tz1, IsoDayOfWeek.Monday, dt1, tz1, IsoDayOfWeek.Sunday, false },
            { dt1, tz1, IsoDayOfWeek.Tuesday, dt1, tz2, IsoDayOfWeek.Tuesday, false },
            { dt1, tz1, IsoDayOfWeek.Wednesday, dt2, tz1, IsoDayOfWeek.Wednesday, false },
            { dt1, tz1, IsoDayOfWeek.Saturday, dt2, tz2, IsoDayOfWeek.Saturday, false },
            { dt1, tz2, IsoDayOfWeek.Sunday, dt2, tz1, IsoDayOfWeek.Sunday, false }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, IsoDayOfWeek, DateTime, TimeZoneInfo, IsoDayOfWeek, int> GetCompareToData(
        IFixture fixture)
    {
        var (dt1, dt2) = (new DateTime( 2021, 8, 1 ), new DateTime( 2021, 9, 1 ));
        var (tz1, tz2, tz3) = (
            TimeZoneFactory.Create( 3 ),
            TimeZoneFactory.Create( 5 ),
            TimeZoneFactory.Create( 3, "Other" ));

        return new TheoryData<DateTime, TimeZoneInfo, IsoDayOfWeek, DateTime, TimeZoneInfo, IsoDayOfWeek, int>
        {
            { dt1, tz1, IsoDayOfWeek.Monday, dt1, tz1, IsoDayOfWeek.Monday, 0 },
            { dt1, tz1, IsoDayOfWeek.Tuesday, dt1, tz1, IsoDayOfWeek.Wednesday, -1 },
            { dt1, tz1, IsoDayOfWeek.Wednesday, dt1, tz1, IsoDayOfWeek.Tuesday, 1 },
            { dt1, tz1, IsoDayOfWeek.Tuesday, dt1, tz2, IsoDayOfWeek.Tuesday, 1 },
            { dt1, tz2, IsoDayOfWeek.Wednesday, dt1, tz1, IsoDayOfWeek.Wednesday, -1 },
            { dt2, tz1, IsoDayOfWeek.Thursday, dt1, tz1, IsoDayOfWeek.Thursday, 1 },
            { dt1, tz1, IsoDayOfWeek.Friday, dt2, tz1, IsoDayOfWeek.Friday, -1 },
            { dt1, tz1, IsoDayOfWeek.Saturday, dt2, tz2, IsoDayOfWeek.Saturday, -1 },
            { dt2, tz2, IsoDayOfWeek.Sunday, dt1, tz1, IsoDayOfWeek.Sunday, 1 },
            { dt1, tz2, IsoDayOfWeek.Monday, dt2, tz1, IsoDayOfWeek.Monday, -1 },
            { dt2, tz1, IsoDayOfWeek.Tuesday, dt1, tz2, IsoDayOfWeek.Tuesday, 1 },
            { dt1, tz1, IsoDayOfWeek.Wednesday, dt1, tz3, IsoDayOfWeek.Wednesday, -1 },
            { dt1, tz3, IsoDayOfWeek.Thursday, dt1, tz1, IsoDayOfWeek.Thursday, 1 }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, DateTime, TimeZoneInfo, bool> GetContainsData(IFixture fixture)
    {
        var week = new DateTime( 2021, 8, 1 );
        var tz1 = TimeZoneFactory.Create( 1 );
        var tz3 = TimeZoneFactory.Create( 3 );

        return new TheoryData<DateTime, TimeZoneInfo, DateTime, TimeZoneInfo, bool>
        {
            { week, tz1, new DateTime( 2021, 8, 1 ), tz1, true },
            { week, tz1, new DateTime( 2021, 8, 7, 23, 59, 59, 999 ).AddTicks( 9999 ), tz1, true },
            { week, tz1, new DateTime( 2021, 8, 8 ), tz1, false },
            { week, tz1, new DateTime( 2021, 7, 31, 23, 59, 59, 999 ).AddTicks( 9999 ), tz1, false },
            { week, tz1, new DateTime( 2021, 8, 1, 2, 0, 0 ), tz3, true },
            { week, tz1, new DateTime( 2021, 8, 1, 1, 59, 59, 999 ).AddTicks( 9999 ), tz3, false },
            { week, tz1, new DateTime( 2021, 8, 8, 1, 59, 59, 999 ).AddTicks( 9999 ), tz3, true },
            { week, tz1, new DateTime( 2021, 8, 8, 2, 0, 0 ), tz3, false }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, DateTime, bool, bool> GetContainsWithAmbiguousStartOrEndData(IFixture fixture)
    {
        var week = new DateTime( 2021, 8, 1 );

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
            { week, startTz, new DateTime( 2021, 8, 1 ), false, true },
            { week, startTz, new DateTime( 2021, 8, 1 ), true, true },
            { week, startTz, new DateTime( 2021, 7, 31, 23, 30, 0 ), false, false },
            { week, startTz, new DateTime( 2021, 7, 31, 23, 30, 0 ), true, false },
            { week, startTz, new DateTime( 2021, 7, 31, 23, 59, 59, 999 ).AddTicks( 9999 ), false, false },
            { week, startTz, new DateTime( 2021, 7, 31, 23, 59, 59, 999 ).AddTicks( 9999 ), true, false },
            { week, endTz, new DateTime( 2021, 8, 7, 23, 59, 59, 999 ).AddTicks( 9999 ), false, true },
            { week, endTz, new DateTime( 2021, 8, 7, 23, 59, 59, 999 ).AddTicks( 9999 ), true, true },
            { week, endTz, new DateTime( 2021, 8, 7, 23, 30, 0 ), false, true },
            { week, endTz, new DateTime( 2021, 8, 7, 23, 30, 0 ), true, true },
            { week, endTz, new DateTime( 2021, 8, 8 ), false, false },
            { week, endTz, new DateTime( 2021, 8, 8 ), true, false }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, DateTime, TimeZoneInfo, bool> GetContainsWithZonedDayData(IFixture fixture)
    {
        var week = new DateTime( 2021, 8, 1 );
        var tz1 = TimeZoneFactory.Create( 1 );
        var tz3 = TimeZoneFactory.Create( 3 );

        return new TheoryData<DateTime, TimeZoneInfo, DateTime, TimeZoneInfo, bool>
        {
            { week, tz1, new DateTime( 2021, 8, 1 ), tz1, true },
            { week, tz1, new DateTime( 2021, 8, 7 ), tz1, true },
            { week, tz1, new DateTime( 2021, 8, 8 ), tz1, false },
            { week, tz1, new DateTime( 2021, 7, 31 ), tz1, false },
            { week, tz1, new DateTime( 2021, 8, 1 ), tz3, false },
            { week, tz1, new DateTime( 2021, 8, 2 ), tz3, true },
            { week, tz1, new DateTime( 2021, 8, 8 ), tz3, false },
            { week, tz1, new DateTime( 2021, 8, 7 ), tz3, true }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, int, DateTime> GetAddWeeksData(IFixture fixture)
    {
        var week = new DateTime( 2021, 8, 1 );
        var timeZone = TimeZoneFactory.Create( 1 );

        return new TheoryData<DateTime, TimeZoneInfo, int, DateTime>
        {
            { week, timeZone, 0, week },
            { week, timeZone, 1, new DateTime( 2021, 8, 8 ) },
            { week, timeZone, -1, new DateTime( 2021, 7, 25 ) },
            { week, timeZone, 10, new DateTime( 2021, 10, 10 ) },
            { week, timeZone, -10, new DateTime( 2021, 5, 23 ) }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, Period, DateTime> GetAddData(IFixture fixture)
    {
        var week = new DateTime( 2021, 8, 1 );
        var timeZone = TimeZoneFactory.Create( 1 );
        var timeZoneWithDaylightSaving = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 9, 26, 2, 0, 0 ),
                transitionEnd: new DateTime( 1, 10, 26, 3, 0, 0 ) ) );

        return new TheoryData<DateTime, TimeZoneInfo, Period, DateTime>
        {
            { week, timeZone, Period.Empty, week },
            { week, timeZone, Period.FromYears( 1 ), new DateTime( 2022, 7, 31 ) },
            { week, timeZone, Period.FromMonths( 1 ), new DateTime( 2021, 8, 29 ) },
            { week, timeZone, Period.FromWeeks( 1 ), new DateTime( 2021, 8, 8 ) },
            { week, timeZone, Period.FromDays( 6 ), week },
            { week, timeZone, Period.FromDays( 7 ), new DateTime( 2021, 8, 8 ) },
            { week, timeZone, Period.FromHours( 167 ), week },
            { week, timeZone, Period.FromHours( 168 ), new DateTime( 2021, 8, 8 ) },
            { week, timeZone, Period.FromMinutes( 10079 ), week },
            { week, timeZone, Period.FromMinutes( 10080 ), new DateTime( 2021, 8, 8 ) },
            { week, timeZone, Period.FromSeconds( 604799 ), week },
            { week, timeZone, Period.FromSeconds( 604800 ), new DateTime( 2021, 8, 8 ) },
            { week, timeZone, Period.FromMilliseconds( 604799999 ), week },
            { week, timeZone, Period.FromMilliseconds( 604800000 ), new DateTime( 2021, 8, 8 ) },
            { week, timeZone, Period.FromTicks( 6047999999999 ), week },
            { week, timeZone, Period.FromTicks( 6048000000000 ), new DateTime( 2021, 8, 8 ) },
            { week, timeZone, new Period( 1, 2, 3, 15, 22, 90, 1700, 80000, 200000000 ), new DateTime( 2022, 11, 6 ) },
            { week, timeZoneWithDaylightSaving, Period.FromMonths( 1 ).AddDays( 25 ).AddHours( 2 ), new DateTime( 2021, 9, 26 ) },
            {
                week,
                timeZoneWithDaylightSaving,
                Period.FromMonths( 1 ).AddDays( 25 ).AddHours( 3 ).SubtractTicks( 1 ),
                new DateTime( 2021, 9, 26 )
            },
            { week, timeZone, Period.FromTicks( -1 ), new DateTime( 2021, 7, 25 ) }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, DateTime, TimeZoneInfo, PeriodUnits> GetGetPeriodOffsetData(IFixture fixture)
    {
        var week = new DateTime( 2021, 8, 1 );
        var otherWeek = new DateTime( 2019, 10, 1 );
        var timeZone = TimeZoneFactory.Create( 1 );

        return new TheoryData<DateTime, TimeZoneInfo, DateTime, TimeZoneInfo, PeriodUnits>
        {
            { week, timeZone, otherWeek, timeZone, PeriodUnits.All },
            { week, timeZone, otherWeek, timeZone, PeriodUnits.Date },
            { week, timeZone, otherWeek, timeZone, PeriodUnits.Time },
            { week, timeZone, otherWeek, timeZone, PeriodUnits.Years },
            { week, timeZone, otherWeek, timeZone, PeriodUnits.Months },
            { week, timeZone, otherWeek, timeZone, PeriodUnits.Weeks },
            { week, timeZone, otherWeek, timeZone, PeriodUnits.Days },
            { week, timeZone, otherWeek, timeZone, PeriodUnits.Hours },
            { week, timeZone, otherWeek, timeZone, PeriodUnits.Minutes },
            { week, timeZone, otherWeek, timeZone, PeriodUnits.Seconds },
            { week, timeZone, otherWeek, timeZone, PeriodUnits.Milliseconds },
            { week, timeZone, otherWeek, timeZone, PeriodUnits.Ticks }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, DateTime, TimeZoneInfo, PeriodUnits> GetGetGreedyPeriodOffsetData(IFixture fixture)
    {
        var week = new DateTime( 2021, 8, 1 );
        var otherWeek = new DateTime( 2019, 10, 1 );
        var timeZone = TimeZoneFactory.Create( 1 );

        return new TheoryData<DateTime, TimeZoneInfo, DateTime, TimeZoneInfo, PeriodUnits>
        {
            { week, timeZone, otherWeek, timeZone, PeriodUnits.All },
            { week, timeZone, otherWeek, timeZone, PeriodUnits.Date },
            { week, timeZone, otherWeek, timeZone, PeriodUnits.Time },
            { week, timeZone, otherWeek, timeZone, PeriodUnits.Years },
            { week, timeZone, otherWeek, timeZone, PeriodUnits.Months },
            { week, timeZone, otherWeek, timeZone, PeriodUnits.Weeks },
            { week, timeZone, otherWeek, timeZone, PeriodUnits.Days },
            { week, timeZone, otherWeek, timeZone, PeriodUnits.Hours },
            { week, timeZone, otherWeek, timeZone, PeriodUnits.Minutes },
            { week, timeZone, otherWeek, timeZone, PeriodUnits.Seconds },
            { week, timeZone, otherWeek, timeZone, PeriodUnits.Milliseconds },
            { week, timeZone, otherWeek, timeZone, PeriodUnits.Ticks }
        };
    }

    public static TheoryData<int, int, TimeZoneInfo, IsoDayOfWeek, int, int> GetSetYearData(IFixture fixture)
    {
        var timeZone = TimeZoneFactory.CreateRandom( fixture );

        return new TheoryData<int, int, TimeZoneInfo, IsoDayOfWeek, int, int>
        {
            { 2021, 1, timeZone, IsoDayOfWeek.Monday, 2022, 1 },
            { 2021, 7, timeZone, IsoDayOfWeek.Tuesday, 2020, 7 },
            { 2021, 13, timeZone, IsoDayOfWeek.Wednesday, 2019, 13 },
            { 2021, 20, timeZone, IsoDayOfWeek.Thursday, 2018, 20 },
            { 2021, 28, timeZone, IsoDayOfWeek.Friday, 2017, 28 },
            { 2021, 36, timeZone, IsoDayOfWeek.Saturday, 2016, 36 },
            { 2021, 44, timeZone, IsoDayOfWeek.Sunday, 2015, 44 },
            { 2020, 53, timeZone, IsoDayOfWeek.Monday, 2021, 52 }
        };
    }

    public static TheoryData<int, int, IsoDayOfWeek, TimeZoneInfo, IsoDayOfWeek> GetSetWeekStartData(IFixture fixture)
    {
        var timeZone = TimeZoneFactory.CreateRandom( fixture );

        return new TheoryData<int, int, IsoDayOfWeek, TimeZoneInfo, IsoDayOfWeek>
        {
            { 2021, 1, IsoDayOfWeek.Monday, timeZone, IsoDayOfWeek.Monday },
            { 2021, 7, IsoDayOfWeek.Monday, timeZone, IsoDayOfWeek.Tuesday },
            { 2021, 13, IsoDayOfWeek.Monday, timeZone, IsoDayOfWeek.Wednesday },
            { 2021, 20, IsoDayOfWeek.Monday, timeZone, IsoDayOfWeek.Thursday },
            { 2021, 28, IsoDayOfWeek.Monday, timeZone, IsoDayOfWeek.Friday },
            { 2021, 36, IsoDayOfWeek.Monday, timeZone, IsoDayOfWeek.Saturday },
            { 2021, 44, IsoDayOfWeek.Monday, timeZone, IsoDayOfWeek.Sunday }
        };
    }

    public static TheoryData<int, int, IsoDayOfWeek, TimeZoneInfo, IsoDayOfWeek> GetSetWeekStartThrowData(IFixture fixture)
    {
        var timeZone = TimeZoneFactory.CreateRandom( fixture );

        return new TheoryData<int, int, IsoDayOfWeek, TimeZoneInfo, IsoDayOfWeek>
        {
            { 2021, 53, IsoDayOfWeek.Saturday, timeZone, IsoDayOfWeek.Monday },
            { 2020, 53, IsoDayOfWeek.Thursday, timeZone, IsoDayOfWeek.Tuesday },
            { 2019, 53, IsoDayOfWeek.Wednesday, timeZone, IsoDayOfWeek.Friday }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, IsoDayOfWeek, DateTime> GetGetDayOfWeekData(IFixture fixture)
    {
        var timeZone = TimeZoneFactory.CreateRandom( fixture );

        return new TheoryData<DateTime, TimeZoneInfo, IsoDayOfWeek, DateTime>
        {
            { new DateTime( 2021, 1, 4 ), timeZone, IsoDayOfWeek.Monday, new DateTime( 2021, 1, 4 ) },
            { new DateTime( 2021, 1, 4 ), timeZone, IsoDayOfWeek.Tuesday, new DateTime( 2021, 1, 5 ) },
            { new DateTime( 2021, 1, 4 ), timeZone, IsoDayOfWeek.Wednesday, new DateTime( 2021, 1, 6 ) },
            { new DateTime( 2021, 1, 4 ), timeZone, IsoDayOfWeek.Thursday, new DateTime( 2021, 1, 7 ) },
            { new DateTime( 2021, 1, 4 ), timeZone, IsoDayOfWeek.Friday, new DateTime( 2021, 1, 8 ) },
            { new DateTime( 2021, 1, 4 ), timeZone, IsoDayOfWeek.Saturday, new DateTime( 2021, 1, 9 ) },
            { new DateTime( 2021, 1, 4 ), timeZone, IsoDayOfWeek.Sunday, new DateTime( 2021, 1, 10 ) },
            { new DateTime( 2020, 12, 31 ), timeZone, IsoDayOfWeek.Monday, new DateTime( 2021, 1, 4 ) },
            { new DateTime( 2020, 12, 31 ), timeZone, IsoDayOfWeek.Tuesday, new DateTime( 2021, 1, 5 ) },
            { new DateTime( 2020, 12, 31 ), timeZone, IsoDayOfWeek.Wednesday, new DateTime( 2021, 1, 6 ) },
            { new DateTime( 2020, 12, 31 ), timeZone, IsoDayOfWeek.Thursday, new DateTime( 2020, 12, 31 ) },
            { new DateTime( 2020, 12, 31 ), timeZone, IsoDayOfWeek.Friday, new DateTime( 2021, 1, 1 ) },
            { new DateTime( 2020, 12, 31 ), timeZone, IsoDayOfWeek.Saturday, new DateTime( 2021, 1, 2 ) },
            { new DateTime( 2020, 12, 31 ), timeZone, IsoDayOfWeek.Sunday, new DateTime( 2021, 1, 3 ) },
            { new DateTime( 2020, 12, 27 ), timeZone, IsoDayOfWeek.Monday, new DateTime( 2020, 12, 28 ) },
            { new DateTime( 2020, 12, 27 ), timeZone, IsoDayOfWeek.Tuesday, new DateTime( 2020, 12, 29 ) },
            { new DateTime( 2020, 12, 27 ), timeZone, IsoDayOfWeek.Wednesday, new DateTime( 2020, 12, 30 ) },
            { new DateTime( 2020, 12, 27 ), timeZone, IsoDayOfWeek.Thursday, new DateTime( 2020, 12, 31 ) },
            { new DateTime( 2020, 12, 27 ), timeZone, IsoDayOfWeek.Friday, new DateTime( 2021, 1, 1 ) },
            { new DateTime( 2020, 12, 27 ), timeZone, IsoDayOfWeek.Saturday, new DateTime( 2021, 1, 2 ) },
            { new DateTime( 2020, 12, 27 ), timeZone, IsoDayOfWeek.Sunday, new DateTime( 2020, 12, 27 ) }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, int> GetGetYearData(IFixture fixture)
    {
        var timeZone = TimeZoneFactory.CreateRandom( fixture );

        return new TheoryData<DateTime, TimeZoneInfo, int>
        {
            { new DateTime( 2021, 1, 4 ), timeZone, 2021 },
            { new DateTime( 2021, 8, 26 ), timeZone, 2021 },
            { new DateTime( 2021, 12, 28 ), timeZone, 2022 }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, IReadOnlyList<DateTime>> GetGetAllDaysData(IFixture fixture)
    {
        var timeZone = TimeZoneFactory.CreateRandom( fixture );

        return new TheoryData<DateTime, TimeZoneInfo, IReadOnlyList<DateTime>>
        {
            {
                new DateTime( 2021, 1, 4 ),
                timeZone,
                new[]
                {
                    new DateTime( 2021, 1, 4 ),
                    new DateTime( 2021, 1, 5 ),
                    new DateTime( 2021, 1, 6 ),
                    new DateTime( 2021, 1, 7 ),
                    new DateTime( 2021, 1, 8 ),
                    new DateTime( 2021, 1, 9 ),
                    new DateTime( 2021, 1, 10 )
                }
            },
            {
                new DateTime( 2020, 12, 31 ),
                timeZone,
                new[]
                {
                    new DateTime( 2020, 12, 31 ),
                    new DateTime( 2021, 1, 1 ),
                    new DateTime( 2021, 1, 2 ),
                    new DateTime( 2021, 1, 3 ),
                    new DateTime( 2021, 1, 4 ),
                    new DateTime( 2021, 1, 5 ),
                    new DateTime( 2021, 1, 6 )
                }
            },
            {
                new DateTime( 2020, 12, 27 ),
                timeZone,
                new[]
                {
                    new DateTime( 2020, 12, 27 ),
                    new DateTime( 2020, 12, 28 ),
                    new DateTime( 2020, 12, 29 ),
                    new DateTime( 2020, 12, 30 ),
                    new DateTime( 2020, 12, 31 ),
                    new DateTime( 2021, 1, 1 ),
                    new DateTime( 2021, 1, 2 )
                }
            }
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
