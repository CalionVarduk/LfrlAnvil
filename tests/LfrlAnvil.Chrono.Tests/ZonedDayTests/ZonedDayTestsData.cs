using System;
using System.Collections.Generic;
using AutoFixture;
using LfrlAnvil.TestExtensions;
using Xunit;

namespace LfrlAnvil.Chrono.Tests.ZonedDayTests;

public class ZonedDayTestsData
{
    public static TheoryData<DateTime, TimeZoneInfo> GetCreateData(IFixture fixture)
    {
        var date = fixture.Create<DateTime>().Date;
        var timeZone = TimeZoneFactory.CreateRandom( fixture );

        return new TheoryData<DateTime, TimeZoneInfo>
        {
            { date, timeZone },
            { date.Add( new TimeSpan( 0, 12, 30, 40, 500 ) ).AddTicks( 6001 ), timeZone },
            { date.Add( new TimeSpan( 0, 23, 59, 59, 999 ) ).AddTicks( 9999 ), timeZone }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, Duration> GetCreateWithContainedInvalidityRangeData(IFixture fixture)
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

        return new TheoryData<DateTime, TimeZoneInfo, Duration>
        {
            { new DateTime( 2021, 8, 26 ), positiveTimeZone, Duration.FromHours( 23 ) },
            { new DateTime( 2021, 8, 26 ), negativeTimeZone, Duration.FromHours( 23 ) },
            { new DateTime( 2021, 8, 26, 23, 59, 59, 999 ).AddTicks( 9999 ), positiveTimeZone, Duration.FromHours( 23 ) },
            { new DateTime( 2021, 8, 26, 23, 59, 59, 999 ).AddTicks( 9999 ), negativeTimeZone, Duration.FromHours( 23 ) },
            { new DateTime( 2021, 8, 26, 2, 0, 0 ), positiveTimeZone, Duration.FromHours( 23 ) },
            { new DateTime( 2021, 8, 26, 2, 59, 59, 999 ).AddTicks( 9999 ), positiveTimeZone, Duration.FromHours( 23 ) },
            { new DateTime( 2021, 8, 26, 2, 0, 0 ), negativeTimeZone, Duration.FromHours( 23 ) },
            { new DateTime( 2021, 8, 26, 2, 59, 59, 999 ).AddTicks( 9999 ), negativeTimeZone, Duration.FromHours( 23 ) }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, Duration> GetCreateWithContainedAmbiguityRangeData(IFixture fixture)
    {
        var positiveTimeZone = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 6, 26, 2, 0, 0 ),
                transitionEnd: new DateTime( 1, 8, 26, 3, 0, 0 ) ) );

        var negativeTimeZone = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 8, 26, 3, 0, 0 ),
                transitionEnd: new DateTime( 1, 10, 26, 2, 0, 0 ),
                daylightDeltaInHours: -1 ) );

        return new TheoryData<DateTime, TimeZoneInfo, Duration>
        {
            { new DateTime( 2021, 8, 26 ), positiveTimeZone, Duration.FromHours( 25 ) },
            { new DateTime( 2021, 8, 26 ), negativeTimeZone, Duration.FromHours( 25 ) },
            { new DateTime( 2021, 8, 26, 23, 59, 59, 999 ).AddTicks( 9999 ), positiveTimeZone, Duration.FromHours( 25 ) },
            { new DateTime( 2021, 8, 26, 23, 59, 59, 999 ).AddTicks( 9999 ), negativeTimeZone, Duration.FromHours( 25 ) },
            { new DateTime( 2021, 8, 26, 2, 0, 0 ), positiveTimeZone, Duration.FromHours( 25 ) },
            { new DateTime( 2021, 8, 26, 2, 59, 59, 999 ).AddTicks( 9999 ), positiveTimeZone, Duration.FromHours( 25 ) },
            { new DateTime( 2021, 8, 26, 2, 0, 0 ), negativeTimeZone, Duration.FromHours( 25 ) },
            { new DateTime( 2021, 8, 26, 2, 59, 59, 999 ).AddTicks( 9999 ), negativeTimeZone, Duration.FromHours( 25 ) }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, DateTime, Duration>
        GetCreateWithInvalidStartTimeData(IFixture fixture)
    {
        var positiveTimeZone1 = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 8, 25, 23, 1, 0 ),
                transitionEnd: new DateTime( 1, 10, 26, 3, 0, 0 ) ) );

        var positiveTimeZone2 = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 8, 25, 23, 30, 0 ),
                transitionEnd: new DateTime( 1, 10, 26, 3, 0, 0 ) ) );

        var positiveTimeZone3 = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 8, 25, 23, 59, 0 ),
                transitionEnd: new DateTime( 1, 10, 26, 3, 0, 0 ) ) );

        var positiveTimeZone4 = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 8, 26, 0, 0, 0 ),
                transitionEnd: new DateTime( 1, 10, 26, 3, 0, 0 ) ) );

        var negativeTimeZone1 = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 6, 26, 2, 0, 0 ),
                transitionEnd: new DateTime( 1, 8, 25, 23, 1, 0 ),
                daylightDeltaInHours: -1 ) );

        var negativeTimeZone2 = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 6, 26, 2, 0, 0 ),
                transitionEnd: new DateTime( 1, 8, 25, 23, 30, 0 ),
                daylightDeltaInHours: -1 ) );

        var negativeTimeZone3 = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 6, 26, 2, 0, 0 ),
                transitionEnd: new DateTime( 1, 8, 25, 23, 59, 0 ),
                daylightDeltaInHours: -1 ) );

        var negativeTimeZone4 = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 6, 26, 2, 0, 0 ),
                transitionEnd: new DateTime( 1, 8, 26, 0, 0, 0 ),
                daylightDeltaInHours: -1 ) );

        return new TheoryData<DateTime, TimeZoneInfo, DateTime, Duration>
        {
            {
                new DateTime( 2021, 8, 26 ),
                positiveTimeZone1,
                new DateTime( 2021, 8, 26, 0, 1, 0 ),
                new Duration( 23, 59, 0 )
            },
            {
                new DateTime( 2021, 8, 26 ),
                positiveTimeZone2,
                new DateTime( 2021, 8, 26, 0, 30, 0 ),
                new Duration( 23, 30, 0 )
            },
            {
                new DateTime( 2021, 8, 26 ),
                positiveTimeZone3,
                new DateTime( 2021, 8, 26, 0, 59, 0 ),
                new Duration( 23, 1, 0 )
            },
            {
                new DateTime( 2021, 8, 26 ),
                positiveTimeZone4,
                new DateTime( 2021, 8, 26, 1, 0, 0 ),
                new Duration( 23, 0, 0 )
            },
            {
                new DateTime( 2021, 8, 26 ),
                negativeTimeZone1,
                new DateTime( 2021, 8, 26, 0, 1, 0 ),
                new Duration( 23, 59, 0 )
            },
            {
                new DateTime( 2021, 8, 26 ),
                negativeTimeZone2,
                new DateTime( 2021, 8, 26, 0, 30, 0 ),
                new Duration( 23, 30, 0 )
            },
            {
                new DateTime( 2021, 8, 26 ),
                negativeTimeZone3,
                new DateTime( 2021, 8, 26, 0, 59, 0 ),
                new Duration( 23, 1, 0 )
            },
            {
                new DateTime( 2021, 8, 26 ),
                negativeTimeZone4,
                new DateTime( 2021, 8, 26, 1, 0, 0 ),
                new Duration( 23, 0, 0 )
            }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, DateTime, Duration>
        GetCreateWithInvalidEndTimeData(IFixture fixture)
    {
        var positiveTimeZone1 = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 8, 25, 23, 1, 0 ),
                transitionEnd: new DateTime( 1, 10, 26, 3, 0, 0 ) ) );

        var positiveTimeZone2 = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 8, 25, 23, 30, 0 ),
                transitionEnd: new DateTime( 1, 10, 26, 3, 0, 0 ) ) );

        var positiveTimeZone3 = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 8, 25, 23, 59, 0 ),
                transitionEnd: new DateTime( 1, 10, 26, 3, 0, 0 ) ) );

        var positiveTimeZone4 = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 8, 25, 23, 0, 0 ),
                transitionEnd: new DateTime( 1, 10, 26, 3, 0, 0 ) ) );

        var negativeTimeZone1 = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 6, 26, 2, 0, 0 ),
                transitionEnd: new DateTime( 1, 8, 25, 23, 1, 0 ),
                daylightDeltaInHours: -1 ) );

        var negativeTimeZone2 = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 6, 26, 2, 0, 0 ),
                transitionEnd: new DateTime( 1, 8, 25, 23, 30, 0 ),
                daylightDeltaInHours: -1 ) );

        var negativeTimeZone3 = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 6, 26, 2, 0, 0 ),
                transitionEnd: new DateTime( 1, 8, 25, 23, 59, 0 ),
                daylightDeltaInHours: -1 ) );

        var negativeTimeZone4 = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 6, 26, 2, 0, 0 ),
                transitionEnd: new DateTime( 1, 8, 25, 23, 0, 0 ),
                daylightDeltaInHours: -1 ) );

        return new TheoryData<DateTime, TimeZoneInfo, DateTime, Duration>
        {
            {
                new DateTime( 2021, 8, 25 ),
                positiveTimeZone1,
                new DateTime( 2021, 8, 25, 23, 0, 59, 999 ).AddTicks( 9999 ),
                new Duration( 23, 1, 0 )
            },
            {
                new DateTime( 2021, 8, 25 ),
                positiveTimeZone2,
                new DateTime( 2021, 8, 25, 23, 29, 59, 999 ).AddTicks( 9999 ),
                new Duration( 23, 30, 0 )
            },
            {
                new DateTime( 2021, 8, 25 ),
                positiveTimeZone3,
                new DateTime( 2021, 8, 25, 23, 58, 59, 999 ).AddTicks( 9999 ),
                new Duration( 23, 59, 0 )
            },
            {
                new DateTime( 2021, 8, 25 ),
                positiveTimeZone4,
                new DateTime( 2021, 8, 25, 22, 59, 59, 999 ).AddTicks( 9999 ),
                new Duration( 23, 0, 0 )
            },
            {
                new DateTime( 2021, 8, 25 ),
                negativeTimeZone1,
                new DateTime( 2021, 8, 25, 23, 0, 59, 999 ).AddTicks( 9999 ),
                new Duration( 23, 1, 0 )
            },
            {
                new DateTime( 2021, 8, 25 ),
                negativeTimeZone2,
                new DateTime( 2021, 8, 25, 23, 29, 59, 999 ).AddTicks( 9999 ),
                new Duration( 23, 30, 0 )
            },
            {
                new DateTime( 2021, 8, 25 ),
                negativeTimeZone3,
                new DateTime( 2021, 8, 25, 23, 58, 59, 999 ).AddTicks( 9999 ),
                new Duration( 23, 59, 0 )
            },
            {
                new DateTime( 2021, 8, 25 ),
                negativeTimeZone4,
                new DateTime( 2021, 8, 25, 22, 59, 59, 999 ).AddTicks( 9999 ),
                new Duration( 23, 0, 0 )
            }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, bool, Duration>
        GetCreateWithAmbiguousStartTimeData(IFixture fixture)
    {
        var positiveTimeZone1 = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 6, 26, 2, 0, 0 ),
                transitionEnd: new DateTime( 1, 8, 26, 0, 1, 0 ) ) );

        var positiveTimeZone2 = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 6, 26, 2, 0, 0 ),
                transitionEnd: new DateTime( 1, 8, 26, 0, 30, 0 ) ) );

        var positiveTimeZone3 = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 6, 26, 2, 0, 0 ),
                transitionEnd: new DateTime( 1, 8, 26, 0, 59, 0 ) ) );

        var positiveTimeZone4 = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 6, 26, 2, 0, 0 ),
                transitionEnd: new DateTime( 1, 8, 26, 1, 0, 0 ) ) );

        var negativeTimeZone1 = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 8, 26, 0, 1, 0 ),
                transitionEnd: new DateTime( 1, 10, 26, 2, 0, 0 ),
                daylightDeltaInHours: -1 ) );

        var negativeTimeZone2 = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 8, 26, 0, 30, 0 ),
                transitionEnd: new DateTime( 1, 10, 26, 2, 0, 0 ),
                daylightDeltaInHours: -1 ) );

        var negativeTimeZone3 = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 8, 26, 0, 59, 0 ),
                transitionEnd: new DateTime( 1, 10, 26, 2, 0, 0 ),
                daylightDeltaInHours: -1 ) );

        var negativeTimeZone4 = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 8, 26, 1, 0, 0 ),
                transitionEnd: new DateTime( 1, 10, 26, 2, 0, 0 ),
                daylightDeltaInHours: -1 ) );

        return new TheoryData<DateTime, TimeZoneInfo, bool, Duration>
        {
            {
                new DateTime( 2021, 8, 26 ),
                positiveTimeZone1,
                true,
                new Duration( 24, 1, 0 )
            },
            {
                new DateTime( 2021, 8, 26 ),
                positiveTimeZone2,
                true,
                new Duration( 24, 30, 0 )
            },
            {
                new DateTime( 2021, 8, 26 ),
                positiveTimeZone3,
                true,
                new Duration( 24, 59, 0 )
            },
            {
                new DateTime( 2021, 8, 26 ),
                positiveTimeZone4,
                true,
                new Duration( 25, 0, 0 )
            },
            {
                new DateTime( 2021, 8, 26 ),
                negativeTimeZone1,
                false,
                new Duration( 24, 1, 0 )
            },
            {
                new DateTime( 2021, 8, 26 ),
                negativeTimeZone2,
                false,
                new Duration( 24, 30, 0 )
            },
            {
                new DateTime( 2021, 8, 26 ),
                negativeTimeZone3,
                false,
                new Duration( 24, 59, 0 )
            },
            {
                new DateTime( 2021, 8, 26 ),
                negativeTimeZone4,
                false,
                new Duration( 25, 0, 0 )
            }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, bool, Duration>
        GetCreateWithAmbiguousEndTimeData(IFixture fixture)
    {
        var positiveTimeZone1 = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 6, 26, 2, 0, 0 ),
                transitionEnd: new DateTime( 1, 8, 27, 0, 1, 0 ) ) );

        var positiveTimeZone2 = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 6, 26, 2, 0, 0 ),
                transitionEnd: new DateTime( 1, 8, 27, 0, 30, 0 ) ) );

        var positiveTimeZone3 = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 6, 26, 2, 0, 0 ),
                transitionEnd: new DateTime( 1, 8, 27, 0, 59, 0 ) ) );

        var positiveTimeZone4 = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 6, 26, 2, 0, 0 ),
                transitionEnd: new DateTime( 1, 8, 27, 0, 0, 0 ) ) );

        var negativeTimeZone1 = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 8, 26, 0, 1, 0 ),
                transitionEnd: new DateTime( 1, 10, 26, 2, 0, 0 ),
                daylightDeltaInHours: -1 ) );

        var negativeTimeZone2 = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 8, 26, 0, 30, 0 ),
                transitionEnd: new DateTime( 1, 10, 26, 2, 0, 0 ),
                daylightDeltaInHours: -1 ) );

        var negativeTimeZone3 = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 8, 26, 0, 59, 0 ),
                transitionEnd: new DateTime( 1, 10, 26, 2, 0, 0 ),
                daylightDeltaInHours: -1 ) );

        var negativeTimeZone4 = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 8, 26, 0, 0, 0 ),
                transitionEnd: new DateTime( 1, 10, 26, 2, 0, 0 ),
                daylightDeltaInHours: -1 ) );

        return new TheoryData<DateTime, TimeZoneInfo, bool, Duration>
        {
            {
                new DateTime( 2021, 8, 26 ),
                positiveTimeZone1,
                false,
                new Duration( 24, 59, 0 )
            },
            {
                new DateTime( 2021, 8, 26 ),
                positiveTimeZone2,
                false,
                new Duration( 24, 30, 0 )
            },
            {
                new DateTime( 2021, 8, 26 ),
                positiveTimeZone3,
                false,
                new Duration( 24, 1, 0 )
            },
            {
                new DateTime( 2021, 8, 26 ),
                positiveTimeZone4,
                false,
                new Duration( 25, 0, 0 )
            },
            {
                new DateTime( 2021, 8, 25 ),
                negativeTimeZone1,
                true,
                new Duration( 24, 59, 0 )
            },
            {
                new DateTime( 2021, 8, 25 ),
                negativeTimeZone2,
                true,
                new Duration( 24, 30, 0 )
            },
            {
                new DateTime( 2021, 8, 25 ),
                negativeTimeZone3,
                true,
                new Duration( 24, 1, 0 )
            },
            {
                new DateTime( 2021, 8, 25 ),
                negativeTimeZone4,
                true,
                new Duration( 25, 0, 0 )
            }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, DateTime, bool, Duration>
        GetCreateWithInvalidStartTimeAndAmbiguousEndTimeData(IFixture fixture)
    {
        var positiveTimeZone1 = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 8, 25, 23, 1, 0 ),
                transitionEnd: new DateTime( 1, 8, 27, 0, 1, 0 ) ) );

        var positiveTimeZone2 = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 8, 25, 23, 59, 0 ),
                transitionEnd: new DateTime( 1, 8, 27, 0, 59, 0 ) ) );

        var positiveTimeZone3 = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 8, 26, 0, 0, 0 ),
                transitionEnd: new DateTime( 1, 8, 27, 0, 0, 0 ) ) );

        var negativeTimeZone1 = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 8, 27, 0, 1, 0 ),
                transitionEnd: new DateTime( 1, 8, 25, 23, 1, 0 ),
                daylightDeltaInHours: -1 ) );

        var negativeTimeZone2 = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 8, 27, 0, 59, 0 ),
                transitionEnd: new DateTime( 1, 8, 25, 23, 59, 0 ),
                daylightDeltaInHours: -1 ) );

        var negativeTimeZone3 = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 8, 27, 0, 0, 0 ),
                transitionEnd: new DateTime( 1, 8, 26, 0, 0, 0 ),
                daylightDeltaInHours: -1 ) );

        return new TheoryData<DateTime, TimeZoneInfo, DateTime, bool, Duration>
        {
            {
                new DateTime( 2021, 8, 26 ),
                positiveTimeZone1,
                new DateTime( 2021, 8, 26, 0, 1, 0 ),
                false,
                new Duration( 24, 58, 0 )
            },
            {
                new DateTime( 2021, 8, 26 ),
                positiveTimeZone2,
                new DateTime( 2021, 8, 26, 0, 59, 0 ),
                false,
                new Duration( 23, 2, 0 )
            },
            {
                new DateTime( 2021, 8, 26 ),
                positiveTimeZone3,
                new DateTime( 2021, 8, 26, 1, 0, 0 ),
                false,
                new Duration( 24, 0, 0 )
            },
            {
                new DateTime( 2021, 8, 26 ),
                negativeTimeZone1,
                new DateTime( 2021, 8, 26, 0, 1, 0 ),
                true,
                new Duration( 24, 58, 0 )
            },
            {
                new DateTime( 2021, 8, 26 ),
                negativeTimeZone2,
                new DateTime( 2021, 8, 26, 0, 59, 0 ),
                true,
                new Duration( 23, 2, 0 )
            },
            {
                new DateTime( 2021, 8, 26 ),
                negativeTimeZone3,
                new DateTime( 2021, 8, 26, 1, 0, 0 ),
                true,
                new Duration( 24, 0, 0 )
            }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, DateTime, bool, Duration>
        GetCreateWithAmbiguousStartTimeAndInvalidEndTimeData(IFixture fixture)
    {
        var positiveTimeZone1 = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 8, 26, 23, 59, 0 ),
                transitionEnd: new DateTime( 1, 8, 26, 0, 59, 0 ) ) );

        var positiveTimeZone2 = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 8, 26, 23, 1, 0 ),
                transitionEnd: new DateTime( 1, 8, 26, 0, 1, 0 ) ) );

        var positiveTimeZone3 = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 8, 26, 23, 0, 0 ),
                transitionEnd: new DateTime( 1, 8, 26, 1, 0, 0 ) ) );

        var negativeTimeZone1 = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 8, 26, 0, 59, 0 ),
                transitionEnd: new DateTime( 1, 8, 26, 23, 59, 0 ),
                daylightDeltaInHours: -1 ) );

        var negativeTimeZone2 = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 8, 26, 0, 1, 0 ),
                transitionEnd: new DateTime( 1, 8, 26, 23, 1, 0 ),
                daylightDeltaInHours: -1 ) );

        var negativeTimeZone3 = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 8, 26, 1, 0, 0 ),
                transitionEnd: new DateTime( 1, 8, 26, 23, 0, 0 ),
                daylightDeltaInHours: -1 ) );

        return new TheoryData<DateTime, TimeZoneInfo, DateTime, bool, Duration>
        {
            {
                new DateTime( 2021, 8, 26 ),
                positiveTimeZone1,
                new DateTime( 2021, 8, 26, 23, 58, 59, 999 ).AddTicks( 9999 ),
                true,
                new Duration( 24, 58, 0 )
            },
            {
                new DateTime( 2021, 8, 26 ),
                positiveTimeZone2,
                new DateTime( 2021, 8, 26, 23, 0, 59, 999 ).AddTicks( 9999 ),
                true,
                new Duration( 23, 2, 0 )
            },
            {
                new DateTime( 2021, 8, 26 ),
                positiveTimeZone3,
                new DateTime( 2021, 8, 26, 22, 59, 59, 999 ).AddTicks( 9999 ),
                true,
                new Duration( 24, 0, 0 )
            },
            {
                new DateTime( 2021, 8, 26 ),
                negativeTimeZone1,
                new DateTime( 2021, 8, 26, 23, 58, 59, 999 ).AddTicks( 9999 ),
                false,
                new Duration( 24, 58, 0 )
            },
            {
                new DateTime( 2021, 8, 26 ),
                negativeTimeZone2,
                new DateTime( 2021, 8, 26, 23, 0, 59, 999 ).AddTicks( 9999 ),
                false,
                new Duration( 23, 2, 0 )
            },
            {
                new DateTime( 2021, 8, 26 ),
                negativeTimeZone3,
                new DateTime( 2021, 8, 26, 22, 59, 59, 999 ).AddTicks( 9999 ),
                false,
                new Duration( 24, 0, 0 )
            }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, string> GetToStringData(IFixture fixture)
    {
        var (dt1, dt2, dt3, dt4) = (
            new DateTime( 2021, 2, 21 ),
            new DateTime( 2021, 8, 5 ),
            new DateTime( 2021, 3, 26 ),
            new DateTime( 2021, 9, 26 ));

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
            { dt1, tz1, $"2021-02-21 +03:00 ({tz1.Id})" },
            { dt2, tz1, $"2021-08-05 +04:00 ({tz1.Id})" },
            { dt1, tz2, $"2021-02-21 -05:00 ({tz2.Id})" },
            { dt2, tz2, $"2021-08-05 -04:00 ({tz2.Id})" },
            { dt3, tz1, $"2021-03-26 +03:00 +04:00 ({tz1.Id})" },
            { dt4, tz1, $"2021-09-26 +04:00 +03:00 ({tz1.Id})" }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, DateTime, TimeZoneInfo, bool> GetEqualsData(IFixture fixture)
    {
        var (dt1, dt2) = (new DateTime( 2021, 8, 24 ), new DateTime( 2021, 8, 25 ));
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
        var (dt1, dt2) = (new DateTime( 2021, 8, 24 ), new DateTime( 2021, 8, 25 ));
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
        var day = new DateTime( 2021, 8, 26 );
        var tz1 = TimeZoneFactory.Create( 1 );
        var tz3 = TimeZoneFactory.Create( 3 );

        return new TheoryData<DateTime, TimeZoneInfo, DateTime, TimeZoneInfo, bool>
        {
            { day, tz1, new DateTime( 2021, 8, 26 ), tz1, true },
            { day, tz1, new DateTime( 2021, 8, 26, 23, 59, 59, 999 ).AddTicks( 9999 ), tz1, true },
            { day, tz1, new DateTime( 2021, 8, 27 ), tz1, false },
            { day, tz1, new DateTime( 2021, 8, 25, 23, 59, 59, 999 ).AddTicks( 9999 ), tz1, false },
            { day, tz1, new DateTime( 2021, 8, 26, 2, 0, 0 ), tz3, true },
            { day, tz1, new DateTime( 2021, 8, 26, 1, 59, 59, 999 ).AddTicks( 9999 ), tz3, false },
            { day, tz1, new DateTime( 2021, 8, 27, 1, 59, 59, 999 ).AddTicks( 9999 ), tz3, true },
            { day, tz1, new DateTime( 2021, 8, 27, 2, 0, 0 ), tz3, false }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, DateTime, bool, bool> GetContainsWithAmbiguousStartOrEndData(IFixture fixture)
    {
        var day = new DateTime( 2021, 8, 26 );

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
            { day, startTz, new DateTime( 2021, 8, 26 ), false, true },
            { day, startTz, new DateTime( 2021, 8, 26 ), true, true },
            { day, startTz, new DateTime( 2021, 8, 25, 23, 30, 0 ), false, false },
            { day, startTz, new DateTime( 2021, 8, 25, 23, 30, 0 ), true, false },
            { day, startTz, new DateTime( 2021, 8, 25, 23, 59, 59, 999 ).AddTicks( 9999 ), false, false },
            { day, startTz, new DateTime( 2021, 8, 25, 23, 59, 59, 999 ).AddTicks( 9999 ), true, false },
            { day, endTz, new DateTime( 2021, 8, 26, 23, 59, 59, 999 ).AddTicks( 9999 ), false, true },
            { day, endTz, new DateTime( 2021, 8, 26, 23, 59, 59, 999 ).AddTicks( 9999 ), true, true },
            { day, endTz, new DateTime( 2021, 8, 26, 23, 30, 0 ), false, true },
            { day, endTz, new DateTime( 2021, 8, 26, 23, 30, 0 ), true, true },
            { day, endTz, new DateTime( 2021, 8, 27 ), false, false },
            { day, endTz, new DateTime( 2021, 8, 27 ), true, false }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, int, DateTime> GetAddDaysData(IFixture fixture)
    {
        var day = new DateTime( 2021, 8, 26 );
        var timeZone = TimeZoneFactory.Create( 1 );

        return new TheoryData<DateTime, TimeZoneInfo, int, DateTime>
        {
            { day, timeZone, 0, day },
            { day, timeZone, 1, new DateTime( 2021, 8, 27 ) },
            { day, timeZone, -1, new DateTime( 2021, 8, 25 ) },
            { day, timeZone, 10, new DateTime( 2021, 9, 5 ) },
            { day, timeZone, -10, new DateTime( 2021, 8, 16 ) }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, Period, DateTime> GetAddData(IFixture fixture)
    {
        var day = new DateTime( 2021, 8, 26 );
        var timeZone = TimeZoneFactory.Create( 1 );
        var timeZoneWithDaylightSaving = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 9, 26, 2, 0, 0 ),
                transitionEnd: new DateTime( 1, 10, 26, 3, 0, 0 ) ) );

        return new TheoryData<DateTime, TimeZoneInfo, Period, DateTime>
        {
            { day, timeZone, Period.Empty, day },
            { day, timeZone, Period.FromYears( 1 ), new DateTime( 2022, 8, 26 ) },
            { day, timeZone, Period.FromMonths( 1 ), new DateTime( 2021, 9, 26 ) },
            { day, timeZone, Period.FromWeeks( 1 ), new DateTime( 2021, 9, 2 ) },
            { day, timeZone, Period.FromDays( 1 ), new DateTime( 2021, 8, 27 ) },
            { day, timeZone, Period.FromHours( 23 ), day },
            { day, timeZone, Period.FromHours( 24 ), new DateTime( 2021, 8, 27 ) },
            { day, timeZone, Period.FromMinutes( 1439 ), day },
            { day, timeZone, Period.FromMinutes( 1440 ), new DateTime( 2021, 8, 27 ) },
            { day, timeZone, Period.FromSeconds( 86399 ), day },
            { day, timeZone, Period.FromSeconds( 86400 ), new DateTime( 2021, 8, 27 ) },
            { day, timeZone, Period.FromMilliseconds( 86399999 ), day },
            { day, timeZone, Period.FromMilliseconds( 86400000 ), new DateTime( 2021, 8, 27 ) },
            { day, timeZone, Period.FromTicks( 863999999999 ), day },
            { day, timeZone, Period.FromTicks( 864000000000 ), new DateTime( 2021, 8, 27 ) },
            { day, timeZone, new Period( 1, 2, 3, 4, 22, 90, 1700, 80000, 200000000 ), new DateTime( 2022, 11, 21 ) },
            { day, timeZoneWithDaylightSaving, Period.FromMonths( 1 ).AddHours( 2 ), new DateTime( 2021, 9, 26 ) },
            {
                day,
                timeZoneWithDaylightSaving,
                Period.FromMonths( 1 ).AddHours( 3 ).SubtractTicks( 1 ),
                new DateTime( 2021, 9, 26 )
            },
            { day, timeZone, Period.FromTicks( -1 ), new DateTime( 2021, 8, 25 ) }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, DateTime, TimeZoneInfo, PeriodUnits> GetGetPeriodOffsetData(IFixture fixture)
    {
        var day = new DateTime( 2021, 8, 26 );
        var otherDay = new DateTime( 2019, 10, 24 );
        var timeZone = TimeZoneFactory.Create( 1 );

        return new TheoryData<DateTime, TimeZoneInfo, DateTime, TimeZoneInfo, PeriodUnits>
        {
            { day, timeZone, otherDay, timeZone, PeriodUnits.All },
            { day, timeZone, otherDay, timeZone, PeriodUnits.Date },
            { day, timeZone, otherDay, timeZone, PeriodUnits.Time },
            { day, timeZone, otherDay, timeZone, PeriodUnits.Years },
            { day, timeZone, otherDay, timeZone, PeriodUnits.Months },
            { day, timeZone, otherDay, timeZone, PeriodUnits.Weeks },
            { day, timeZone, otherDay, timeZone, PeriodUnits.Days },
            { day, timeZone, otherDay, timeZone, PeriodUnits.Hours },
            { day, timeZone, otherDay, timeZone, PeriodUnits.Minutes },
            { day, timeZone, otherDay, timeZone, PeriodUnits.Seconds },
            { day, timeZone, otherDay, timeZone, PeriodUnits.Milliseconds },
            { day, timeZone, otherDay, timeZone, PeriodUnits.Ticks }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, DateTime, TimeZoneInfo, PeriodUnits> GetGetGreedyPeriodOffsetData(IFixture fixture)
    {
        var day = new DateTime( 2021, 8, 26 );
        var otherDay = new DateTime( 2019, 10, 24 );
        var timeZone = TimeZoneFactory.Create( 1 );

        return new TheoryData<DateTime, TimeZoneInfo, DateTime, TimeZoneInfo, PeriodUnits>
        {
            { day, timeZone, otherDay, timeZone, PeriodUnits.All },
            { day, timeZone, otherDay, timeZone, PeriodUnits.Date },
            { day, timeZone, otherDay, timeZone, PeriodUnits.Time },
            { day, timeZone, otherDay, timeZone, PeriodUnits.Years },
            { day, timeZone, otherDay, timeZone, PeriodUnits.Months },
            { day, timeZone, otherDay, timeZone, PeriodUnits.Weeks },
            { day, timeZone, otherDay, timeZone, PeriodUnits.Days },
            { day, timeZone, otherDay, timeZone, PeriodUnits.Hours },
            { day, timeZone, otherDay, timeZone, PeriodUnits.Minutes },
            { day, timeZone, otherDay, timeZone, PeriodUnits.Seconds },
            { day, timeZone, otherDay, timeZone, PeriodUnits.Milliseconds },
            { day, timeZone, otherDay, timeZone, PeriodUnits.Ticks }
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
                transitionStart: new DateTime( 1, 8, 25, 23, 30, 0 ),
                transitionEnd: new DateTime( 1, 10, 26, 2, 0, 0 ) ) );

        return new TheoryData<DateTime, TimeZoneInfo, int, DateTime>
        {
            { new DateTime( 2021, 8, 26 ), timeZone, 2021, new DateTime( 2021, 8, 26 ) },
            { new DateTime( 2021, 8, 26 ), timeZone, 2022, new DateTime( 2022, 8, 26 ) },
            { new DateTime( 2021, 8, 26 ), timeZone, 2020, new DateTime( 2020, 8, 26 ) },
            { new DateTime( 2020, 2, 29 ), timeZone, 2021, new DateTime( 2021, 2, 28 ) },
            { new DateTime( 2021, 8, 26 ), timeZoneWithInvalidity, 2019, new DateTime( 2019, 8, 26 ) },
            { new DateTime( 2021, 8, 25 ), timeZoneWithInvalidity, 2019, new DateTime( 2019, 8, 25 ) }
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
                transitionStart: new DateTime( 1, 8, 25, 23, 30, 0 ),
                transitionEnd: new DateTime( 1, 10, 26, 2, 0, 0 ) ) );

        return new TheoryData<DateTime, TimeZoneInfo, IsoMonthOfYear, DateTime>
        {
            { new DateTime( 2021, 8, 26 ), timeZone, IsoMonthOfYear.August, new DateTime( 2021, 8, 26 ) },
            { new DateTime( 2021, 8, 26 ), timeZone, IsoMonthOfYear.September, new DateTime( 2021, 9, 26 ) },
            { new DateTime( 2021, 8, 26 ), timeZone, IsoMonthOfYear.July, new DateTime( 2021, 7, 26 ) },
            { new DateTime( 2021, 8, 31 ), timeZone, IsoMonthOfYear.April, new DateTime( 2021, 4, 30 ) },
            { new DateTime( 2021, 8, 31 ), timeZone, IsoMonthOfYear.February, new DateTime( 2021, 2, 28 ) },
            { new DateTime( 2020, 8, 31 ), timeZone, IsoMonthOfYear.February, new DateTime( 2020, 2, 29 ) },
            { new DateTime( 2021, 6, 26 ), timeZoneWithInvalidity, IsoMonthOfYear.August, new DateTime( 2021, 8, 26 ) },
            { new DateTime( 2021, 6, 25 ), timeZoneWithInvalidity, IsoMonthOfYear.August, new DateTime( 2021, 8, 25 ) }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, int, DateTime> GetSetDayOfMonthData(IFixture fixture)
    {
        var timeZone = TimeZoneFactory.Create( 1 );
        var timeZoneWithInvalidity = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 8, 25, 23, 30, 0 ),
                transitionEnd: new DateTime( 1, 10, 26, 2, 0, 0 ) ) );

        return new TheoryData<DateTime, TimeZoneInfo, int, DateTime>
        {
            { new DateTime( 2021, 8, 26 ), timeZone, 26, new DateTime( 2021, 8, 26 ) },
            { new DateTime( 2021, 8, 26 ), timeZone, 1, new DateTime( 2021, 8, 1 ) },
            { new DateTime( 2021, 8, 26 ), timeZone, 31, new DateTime( 2021, 8, 31 ) },
            { new DateTime( 2021, 8, 16 ), timeZoneWithInvalidity, 26, new DateTime( 2021, 8, 26 ) },
            { new DateTime( 2021, 8, 16 ), timeZoneWithInvalidity, 25, new DateTime( 2021, 8, 25 ) }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, int> GetSetDayOfMonthThrowData(IFixture fixture)
    {
        var timeZone = TimeZoneFactory.Create( 1 );

        return new TheoryData<DateTime, TimeZoneInfo, int>
        {
            { new DateTime( 2021, 1, 1 ), timeZone, 0 },
            { new DateTime( 2021, 2, 1 ), timeZone, 0 },
            { new DateTime( 2021, 3, 1 ), timeZone, 0 },
            { new DateTime( 2021, 4, 1 ), timeZone, 0 },
            { new DateTime( 2021, 5, 1 ), timeZone, 0 },
            { new DateTime( 2021, 6, 1 ), timeZone, 0 },
            { new DateTime( 2021, 7, 1 ), timeZone, 0 },
            { new DateTime( 2021, 8, 1 ), timeZone, 0 },
            { new DateTime( 2021, 9, 1 ), timeZone, 0 },
            { new DateTime( 2021, 10, 1 ), timeZone, 0 },
            { new DateTime( 2021, 11, 1 ), timeZone, 0 },
            { new DateTime( 2021, 12, 1 ), timeZone, 0 },
            { new DateTime( 2021, 1, 1 ), timeZone, ChronoConstants.DaysInJanuary + 1 },
            { new DateTime( 2021, 2, 1 ), timeZone, ChronoConstants.DaysInFebruary + 1 },
            { new DateTime( 2020, 2, 1 ), timeZone, ChronoConstants.DaysInLeapFebruary + 1 },
            { new DateTime( 2021, 3, 1 ), timeZone, ChronoConstants.DaysInMarch + 1 },
            { new DateTime( 2021, 4, 1 ), timeZone, ChronoConstants.DaysInApril + 1 },
            { new DateTime( 2021, 5, 1 ), timeZone, ChronoConstants.DaysInMay + 1 },
            { new DateTime( 2021, 6, 1 ), timeZone, ChronoConstants.DaysInJune + 1 },
            { new DateTime( 2021, 7, 1 ), timeZone, ChronoConstants.DaysInJuly + 1 },
            { new DateTime( 2021, 8, 1 ), timeZone, ChronoConstants.DaysInAugust + 1 },
            { new DateTime( 2021, 9, 1 ), timeZone, ChronoConstants.DaysInSeptember + 1 },
            { new DateTime( 2021, 10, 1 ), timeZone, ChronoConstants.DaysInOctober + 1 },
            { new DateTime( 2021, 11, 1 ), timeZone, ChronoConstants.DaysInNovember + 1 },
            { new DateTime( 2021, 12, 1 ), timeZone, ChronoConstants.DaysInDecember + 1 },
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, int, DateTime> GetSetDayOfYearData(IFixture fixture)
    {
        var timeZone = TimeZoneFactory.Create( 1 );
        var timeZoneWithInvalidity = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 8, 25, 23, 30, 0 ),
                transitionEnd: new DateTime( 1, 10, 26, 2, 0, 0 ) ) );

        return new TheoryData<DateTime, TimeZoneInfo, int, DateTime>
        {
            { new DateTime( 2021, 8, 26 ), timeZone, 1, new DateTime( 2021, 1, 1 ) },
            { new DateTime( 2021, 8, 26 ), timeZone, 365, new DateTime( 2021, 12, 31 ) },
            { new DateTime( 2020, 8, 26 ), timeZone, 365, new DateTime( 2020, 12, 30 ) },
            { new DateTime( 2020, 8, 26 ), timeZone, 366, new DateTime( 2020, 12, 31 ) },
            { new DateTime( 2021, 8, 26 ), timeZone, 238, new DateTime( 2021, 8, 26 ) },
            { new DateTime( 2020, 8, 26 ), timeZone, 239, new DateTime( 2020, 8, 26 ) },
            { new DateTime( 2021, 6, 16 ), timeZoneWithInvalidity, 238, new DateTime( 2021, 8, 26 ) },
            { new DateTime( 2021, 6, 16 ), timeZoneWithInvalidity, 237, new DateTime( 2021, 8, 25 ) }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, int> GetSetDayOfYearThrowData(IFixture fixture)
    {
        var timeZone = TimeZoneFactory.Create( 1 );

        return new TheoryData<DateTime, TimeZoneInfo, int>
        {
            { new DateTime( 2021, 1, 1 ), timeZone, 0 },
            { new DateTime( 2021, 1, 1 ), timeZone, ChronoConstants.DaysInYear + 1 },
            { new DateTime( 2020, 1, 1 ), timeZone, ChronoConstants.DaysInLeapYear + 1 }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, TimeOfDay, DateTime> GetGetDateTimeData(IFixture fixture)
    {
        var day = new DateTime( 2021, 8, 26 );
        var timeZone = TimeZoneFactory.Create( 1 );
        var timeZoneWithInvalidity = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 8, 26, 12, 0, 0 ),
                transitionEnd: new DateTime( 1, 10, 26, 12, 0, 0 ) ) );

        return new TheoryData<DateTime, TimeZoneInfo, TimeOfDay, DateTime>
        {
            { day, timeZone, TimeOfDay.Start, day },
            { day, timeZone, TimeOfDay.Mid, new DateTime( 2021, 8, 26, 12, 0, 0 ) },
            { day, timeZone, TimeOfDay.End, new DateTime( 2021, 8, 26, 23, 59, 59, 999 ).AddTicks( 9999 ) },
            {
                day,
                timeZone,
                new TimeOfDay( 12, 30, 40, 500, 6001 ),
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 )
            },
            {
                day,
                timeZoneWithInvalidity,
                new TimeOfDay( 11, 59, 59, 999, 9999 ),
                new DateTime( 2021, 8, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                day,
                timeZoneWithInvalidity,
                new TimeOfDay( 13 ),
                new DateTime( 2021, 8, 26, 13, 0, 0 )
            }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, TimeOfDay> GetGetDateTimeThrowData(IFixture fixture)
    {
        var day = new DateTime( 2021, 8, 26 );
        var timeZoneWithInvalidity = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 8, 26, 12, 0, 0 ),
                transitionEnd: new DateTime( 1, 10, 26, 12, 0, 0 ) ) );

        return new TheoryData<DateTime, TimeZoneInfo, TimeOfDay>
        {
            { day, timeZoneWithInvalidity, new TimeOfDay( 12 ) },
            { day, timeZoneWithInvalidity, new TimeOfDay( 12, 59, 59, 999, 9999 ) }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, DateTime, DateTime> GetGetIntersectingInvalidityRangeData(IFixture fixture)
    {
        var positiveTimeZoneWithInvalidity = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 8, 26, 12, 0, 0 ),
                transitionEnd: new DateTime( 1, 10, 26, 12, 0, 0 ) ) );

        var negativeTimeZoneWithInvalidity = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 8, 26, 12, 0, 0 ),
                transitionEnd: new DateTime( 1, 10, 26, 12, 0, 0 ),
                daylightDeltaInHours: -1 ) );

        var positiveTimeZoneWithInvalidMidnight = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 8, 25, 23, 30, 0 ),
                transitionEnd: new DateTime( 1, 10, 26, 12, 0, 0 ) ) );

        var negativeTimeZoneWithInvalidMidnight = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 8, 26, 12, 0, 0 ),
                transitionEnd: new DateTime( 1, 10, 25, 23, 30, 0 ),
                daylightDeltaInHours: -1 ) );

        return new TheoryData<DateTime, TimeZoneInfo, DateTime, DateTime>
        {
            {
                new DateTime( 2021, 8, 26 ),
                positiveTimeZoneWithInvalidity,
                new DateTime( 2021, 8, 26, 12, 0, 0 ),
                new DateTime( 2021, 8, 26, 12, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 10, 26 ),
                negativeTimeZoneWithInvalidity,
                new DateTime( 2021, 10, 26, 12, 0, 0 ),
                new DateTime( 2021, 10, 26, 12, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 8, 26 ),
                positiveTimeZoneWithInvalidMidnight,
                new DateTime( 2021, 8, 25, 23, 30, 0 ),
                new DateTime( 2021, 8, 26, 0, 29, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 8, 25 ),
                positiveTimeZoneWithInvalidMidnight,
                new DateTime( 2021, 8, 25, 23, 30, 0 ),
                new DateTime( 2021, 8, 26, 0, 29, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 10, 26 ),
                negativeTimeZoneWithInvalidMidnight,
                new DateTime( 2021, 10, 25, 23, 30, 0 ),
                new DateTime( 2021, 10, 26, 0, 29, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 10, 25 ),
                negativeTimeZoneWithInvalidMidnight,
                new DateTime( 2021, 10, 25, 23, 30, 0 ),
                new DateTime( 2021, 10, 26, 0, 29, 59, 999 ).AddTicks( 9999 )
            }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo> GetGetIntersectingInvalidityRangeNullData(IFixture fixture)
    {
        var timeZone = TimeZoneFactory.Create( 1 );
        var positiveTimeZoneWithInvalidity = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 8, 26, 12, 0, 0 ),
                transitionEnd: new DateTime( 1, 10, 26, 12, 0, 0 ) ) );

        var negativeTimeZoneWithInvalidity = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 8, 26, 12, 0, 0 ),
                transitionEnd: new DateTime( 1, 10, 26, 12, 0, 0 ),
                daylightDeltaInHours: -1 ) );

        return new TheoryData<DateTime, TimeZoneInfo>
        {
            { new DateTime( 2021, 8, 26 ), timeZone },
            { new DateTime( 2021, 8, 25 ), positiveTimeZoneWithInvalidity },
            { new DateTime( 2021, 8, 27 ), positiveTimeZoneWithInvalidity },
            { new DateTime( 2021, 10, 26 ), positiveTimeZoneWithInvalidity },
            { new DateTime( 2021, 10, 25 ), negativeTimeZoneWithInvalidity },
            { new DateTime( 2021, 10, 27 ), negativeTimeZoneWithInvalidity },
            { new DateTime( 2021, 8, 26 ), negativeTimeZoneWithInvalidity },
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, DateTime, DateTime> GetGetIntersectingAmbiguityRangeData(IFixture fixture)
    {
        var positiveTimeZoneWithInvalidity = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 8, 26, 12, 0, 0 ),
                transitionEnd: new DateTime( 1, 10, 26, 12, 0, 0 ) ) );

        var negativeTimeZoneWithInvalidity = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 8, 26, 12, 0, 0 ),
                transitionEnd: new DateTime( 1, 10, 26, 12, 0, 0 ),
                daylightDeltaInHours: -1 ) );

        var positiveTimeZoneWithAmbiguousMidnight = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 8, 26, 12, 0, 0 ),
                transitionEnd: new DateTime( 1, 10, 26, 0, 30, 0 ) ) );

        var negativeTimeZoneWithAmbiguousMidnight = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 8, 26, 0, 30, 0 ),
                transitionEnd: new DateTime( 1, 10, 26, 12, 0, 0 ),
                daylightDeltaInHours: -1 ) );

        return new TheoryData<DateTime, TimeZoneInfo, DateTime, DateTime>
        {
            {
                new DateTime( 2021, 10, 26 ),
                positiveTimeZoneWithInvalidity,
                new DateTime( 2021, 10, 26, 11, 0, 0 ),
                new DateTime( 2021, 10, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 8, 26 ),
                negativeTimeZoneWithInvalidity,
                new DateTime( 2021, 8, 26, 11, 0, 0 ),
                new DateTime( 2021, 8, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 10, 26 ),
                positiveTimeZoneWithAmbiguousMidnight,
                new DateTime( 2021, 10, 25, 23, 30, 0 ),
                new DateTime( 2021, 10, 26, 0, 29, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 10, 25 ),
                positiveTimeZoneWithAmbiguousMidnight,
                new DateTime( 2021, 10, 25, 23, 30, 0 ),
                new DateTime( 2021, 10, 26, 0, 29, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 8, 26 ),
                negativeTimeZoneWithAmbiguousMidnight,
                new DateTime( 2021, 8, 25, 23, 30, 0 ),
                new DateTime( 2021, 8, 26, 0, 29, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 8, 25 ),
                negativeTimeZoneWithAmbiguousMidnight,
                new DateTime( 2021, 8, 25, 23, 30, 0 ),
                new DateTime( 2021, 8, 26, 0, 29, 59, 999 ).AddTicks( 9999 )
            }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo> GetGetIntersectingAmbiguityRangeNullData(IFixture fixture)
    {
        var timeZone = TimeZoneFactory.Create( 1 );
        var positiveTimeZoneWithInvalidity = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 8, 26, 12, 0, 0 ),
                transitionEnd: new DateTime( 1, 10, 26, 12, 0, 0 ) ) );

        var negativeTimeZoneWithInvalidity = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 8, 26, 12, 0, 0 ),
                transitionEnd: new DateTime( 1, 10, 26, 12, 0, 0 ),
                daylightDeltaInHours: -1 ) );

        return new TheoryData<DateTime, TimeZoneInfo>
        {
            { new DateTime( 2021, 8, 26 ), timeZone },
            { new DateTime( 2021, 10, 25 ), positiveTimeZoneWithInvalidity },
            { new DateTime( 2021, 10, 27 ), positiveTimeZoneWithInvalidity },
            { new DateTime( 2021, 8, 26 ), positiveTimeZoneWithInvalidity },
            { new DateTime( 2021, 8, 25 ), negativeTimeZoneWithInvalidity },
            { new DateTime( 2021, 8, 27 ), negativeTimeZoneWithInvalidity },
            { new DateTime( 2021, 10, 26 ), negativeTimeZoneWithInvalidity },
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