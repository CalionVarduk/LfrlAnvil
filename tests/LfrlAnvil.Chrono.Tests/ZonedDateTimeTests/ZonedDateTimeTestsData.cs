using System.Collections.Generic;

namespace LfrlAnvil.Chrono.Tests.ZonedDateTimeTests;

public class ZonedDateTimeTestsData
{
    public static TheoryData<long> GetCreateUtcData(IFixture fixture)
    {
        return new TheoryData<long>
        {
            0,
            -1,
            1,
            1234567890,
            -1234567890
        };
    }

    public static TheoryData<DateTime> GetCreateUtcWithDateTimeData(IFixture fixture)
    {
        return new TheoryData<DateTime>
        {
            DateTime.UnixEpoch,
            DateTime.MinValue,
            DateTime.MaxValue,
            new DateTime( 2021, 9, 26, 17, 47, 51, 123 )
        };
    }

    public static TheoryData<DateTime> GetCreateLocalData(IFixture fixture)
    {
        return new TheoryData<DateTime>
        {
            new DateTime( 2011, 1, 6, 0, 8, 44, 999 ),
            new DateTime( 2019, 8, 15, 21, 59, 4, 326 ),
            new DateTime( 2021, 9, 26, 17, 47, 51, 123 )
        };
    }

    public static TheoryData<DateTime> GetCreateWithUtcTimeZoneData(IFixture fixture)
    {
        return new TheoryData<DateTime>
        {
            DateTime.UnixEpoch,
            DateTime.MinValue,
            DateTime.MaxValue,
            new DateTime( 2021, 9, 26, 17, 47, 51, 123 )
        };
    }

    public static TheoryData<DateTime> GetCreateWithLocalTimeZoneData(IFixture fixture)
    {
        return new TheoryData<DateTime>
        {
            new DateTime( 2011, 1, 6, 0, 8, 44, 999 ),
            new DateTime( 2019, 8, 15, 21, 59, 4, 326 ),
            new DateTime( 2021, 9, 26, 17, 47, 51, 123 )
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo> GetCreateWithoutDaylightSavingData(IFixture fixture)
    {
        var result = new TheoryData<DateTime, TimeZoneInfo>();
        var baseDateTime = new DateTime( 2021, 9, 26, 12, 41, 2, 567 );

        for ( var hourOffset = -14.0; hourOffset <= 14.0; hourOffset += 0.5 )
        {
            var timeZone = TimeZoneFactory.Create( hourOffset );
            result.Add( baseDateTime, timeZone );
        }

        return result;
    }

    public static TheoryData<DateTime, TimeZoneInfo> GetCreateWithInactiveDaylightSavingData(IFixture fixture)
    {
        var result = new TheoryData<DateTime, TimeZoneInfo>();
        var baseDateTime = new DateTime( 2021, 9, 26, 12, 41, 2, 567 );

        for ( var hourOffset = -13.0; hourOffset <= 13.0; hourOffset += 0.5 )
        {
            var timeZone = TimeZoneFactory.Create(
                hourOffset,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 3, 26, 12, 0, 0 ),
                    transitionEnd: new DateTime( 1, 9, 26, 12, 0, 0 ) ) );

            result.Add( baseDateTime, timeZone );
        }

        return result;
    }

    public static TheoryData<DateTime, TimeZoneInfo> GetCreateWithActiveDaylightSavingData(IFixture fixture)
    {
        var result = new TheoryData<DateTime, TimeZoneInfo>();
        var baseDateTime = new DateTime( 2021, 9, 26, 12, 41, 2, 567 );

        for ( var hourOffset = -13.0; hourOffset <= 13.0; hourOffset += 0.5 )
        {
            var timeZone = TimeZoneFactory.Create(
                hourOffset,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 9, 26, 11, 0, 0 ),
                    transitionEnd: new DateTime( 1, 3, 26, 11, 0, 0 ) ) );

            result.Add( baseDateTime, timeZone );
        }

        return result;
    }

    public static TheoryData<DateTime, TimeZoneInfo> GetCreateShouldThrowInvalidZonedDateTimeExceptionData(IFixture fixture)
    {
        var daylightOffset = 1.0;

        var timeZoneWithPositiveDaylightSaving = TimeZoneFactory.Create(
            daylightOffset,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 9, 26, 12, 0, 0 ),
                transitionEnd: new DateTime( 1, 3, 26, 12, 0, 0 ) ) );

        var timeZoneWithNegativeDaylightSaving = TimeZoneFactory.Create(
            daylightOffset,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 9, 26, 12, 0, 0 ),
                transitionEnd: new DateTime( 1, 3, 26, 12, 0, 0 ),
                daylightDeltaInHours: -1.0 ) );

        return new TheoryData<DateTime, TimeZoneInfo>
        {
            { new DateTime( 2021, 9, 26, 12, 0, 0 ), timeZoneWithPositiveDaylightSaving },
            { new DateTime( 2021, 9, 26, 12, 30, 0 ), timeZoneWithPositiveDaylightSaving },
            { new DateTime( 2021, 9, 26, 12, 59, 59, 999 ).AddTicks( 9999 ), timeZoneWithPositiveDaylightSaving },
            { new DateTime( 2021, 3, 26, 12, 0, 0 ), timeZoneWithNegativeDaylightSaving },
            { new DateTime( 2021, 3, 26, 12, 30, 0 ), timeZoneWithNegativeDaylightSaving },
            { new DateTime( 2021, 3, 26, 12, 59, 59, 999 ).AddTicks( 9999 ), timeZoneWithNegativeDaylightSaving }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, bool> GetCreateWithDateTimeOnTheEdgeOfInvalidityData(IFixture fixture)
    {
        var daylightOffset = 1.0;

        var timeZoneWithPositiveDaylightSaving = TimeZoneFactory.Create(
            daylightOffset,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 9, 26, 12, 0, 0 ),
                transitionEnd: new DateTime( 1, 3, 26, 12, 0, 0 ) ) );

        var timeZoneWithNegativeDaylightSaving = TimeZoneFactory.Create(
            daylightOffset,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 9, 26, 12, 0, 0 ),
                transitionEnd: new DateTime( 1, 3, 26, 12, 0, 0 ),
                daylightDeltaInHours: -1.0 ) );

        return new TheoryData<DateTime, TimeZoneInfo, bool>
        {
            { new DateTime( 2021, 9, 26, 11, 59, 59, 999 ).AddTicks( 9999 ), timeZoneWithPositiveDaylightSaving, false },
            { new DateTime( 2021, 9, 26, 13, 0, 0 ), timeZoneWithPositiveDaylightSaving, true },
            { new DateTime( 2021, 3, 26, 11, 59, 59, 999 ).AddTicks( 9999 ), timeZoneWithNegativeDaylightSaving, true },
            { new DateTime( 2021, 3, 26, 13, 0, 0 ), timeZoneWithNegativeDaylightSaving, false }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo> GetCreateWithAmbiguousDateTimeData(IFixture fixture)
    {
        var daylightOffset = 1.0;

        var timeZoneWithPositiveDaylightSaving = TimeZoneFactory.Create(
            daylightOffset,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 9, 26, 12, 0, 0 ),
                transitionEnd: new DateTime( 1, 3, 26, 12, 0, 0 ) ) );

        var timeZoneWithNegativeDaylightSaving = TimeZoneFactory.Create(
            daylightOffset,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 9, 26, 12, 0, 0 ),
                transitionEnd: new DateTime( 1, 3, 26, 12, 0, 0 ),
                daylightDeltaInHours: -1.0 ) );

        return new TheoryData<DateTime, TimeZoneInfo>
        {
            { new DateTime( 2021, 3, 26, 11, 0, 0 ), timeZoneWithPositiveDaylightSaving },
            { new DateTime( 2021, 3, 26, 11, 30, 0 ), timeZoneWithPositiveDaylightSaving },
            { new DateTime( 2021, 3, 26, 11, 59, 59, 999 ).AddTicks( 9999 ), timeZoneWithPositiveDaylightSaving },
            { new DateTime( 2021, 9, 26, 11, 0, 0 ), timeZoneWithNegativeDaylightSaving },
            { new DateTime( 2021, 9, 26, 11, 30, 0 ), timeZoneWithNegativeDaylightSaving },
            { new DateTime( 2021, 9, 26, 11, 59, 59, 999 ).AddTicks( 9999 ), timeZoneWithNegativeDaylightSaving }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, bool> GetCreateWithDateTimeOnTheEdgeOfAmbiguityData(IFixture fixture)
    {
        var daylightOffset = 1.0;

        var timeZoneWithPositiveDaylightSaving = TimeZoneFactory.Create(
            daylightOffset,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 9, 26, 12, 0, 0 ),
                transitionEnd: new DateTime( 1, 3, 26, 12, 0, 0 ) ) );

        var timeZoneWithNegativeDaylightSaving = TimeZoneFactory.Create(
            daylightOffset,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 9, 26, 12, 0, 0 ),
                transitionEnd: new DateTime( 1, 3, 26, 12, 0, 0 ),
                daylightDeltaInHours: -1.0 ) );

        return new TheoryData<DateTime, TimeZoneInfo, bool>
        {
            { new DateTime( 2021, 3, 26, 10, 59, 59, 999 ).AddTicks( 9999 ), timeZoneWithPositiveDaylightSaving, true },
            { new DateTime( 2021, 3, 26, 12, 0, 0 ), timeZoneWithPositiveDaylightSaving, false },
            { new DateTime( 2021, 9, 26, 10, 59, 59, 999 ).AddTicks( 9999 ), timeZoneWithNegativeDaylightSaving, false },
            { new DateTime( 2021, 9, 26, 12, 0, 0 ), timeZoneWithNegativeDaylightSaving, true }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, string> GetToStringData(IFixture fixture)
    {
        var (dt1, dt2) = (
            new DateTime( 2021, 2, 21, 12, 39, 47, 123 ).AddTicks( 234 ),
            new DateTime( 2021, 8, 5, 1, 7, 3, 67 ).AddTicks( 9870 ));

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
            { dt1, tz1, $"2021-02-21 12:39:47.1230234 +03:00 ({tz1.Id})" },
            { dt2, tz1, $"2021-08-05 01:07:03.0679870 +04:00 ({tz1.Id})" },
            { dt1, tz2, $"2021-02-21 12:39:47.1230234 -05:00 ({tz2.Id})" },
            { dt2, tz2, $"2021-08-05 01:07:03.0679870 -04:00 ({tz2.Id})" },
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, DateTime, TimeZoneInfo, bool> GetEqualsData(IFixture fixture)
    {
        var (dt1, dt2) = (new DateTime( 2021, 8, 24, 12, 0, 0 ), new DateTime( 2021, 8, 24, 14, 0, 0 ));
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
        var (dt1, dt2) = (new DateTime( 2021, 8, 24, 12, 0, 0 ), new DateTime( 2021, 8, 24, 14, 0, 0 ));
        var (tz1, tz2, tz3) = (TimeZoneFactory.Create( 3 ), TimeZoneFactory.Create( 5 ), TimeZoneFactory.Create( 3, "Other" ));

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

    public static TheoryData<DateTime, TimeZoneInfo, DateTime> GetToUtcTimeZoneData(IFixture fixture)
    {
        var dt = new DateTime( 2021, 8, 24, 12, 0, 0 );

        return new TheoryData<DateTime, TimeZoneInfo, DateTime>
        {
            { dt, TimeZoneInfo.Utc, dt },
            { dt, TimeZoneFactory.Create( 0 ), dt },
            { dt, TimeZoneFactory.Create( 1 ), dt.AddHours( -1 ) },
            { dt, TimeZoneFactory.Create( 10 ), dt.AddHours( -10 ) },
            { dt, TimeZoneFactory.Create( -1 ), dt.AddHours( 1 ) },
            { dt, TimeZoneFactory.Create( -10 ), dt.AddHours( 10 ) }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, DateTime> GetToTimeZoneWithUtcTimeZoneData(IFixture fixture)
    {
        return GetToUtcTimeZoneData( fixture );
    }

    public static TheoryData<DateTime, TimeZoneInfo, DateTime> GetToLocalTimeZoneData(IFixture fixture)
    {
        var dt = new DateTime( 2021, 8, 24, 12, 0, 0 );
        var utcOffset = TimeZoneInfo.Local.GetUtcOffset( dt );

        return new TheoryData<DateTime, TimeZoneInfo, DateTime>
        {
            { dt, TimeZoneInfo.Local, dt },
            { dt, TimeZoneFactory.Create( 0 ), dt.Add( utcOffset ) },
            { dt, TimeZoneFactory.Create( 1 ), dt.Add( utcOffset - TimeSpan.FromHours( 1 ) ) },
            { dt, TimeZoneFactory.Create( 10 ), dt.Add( utcOffset - TimeSpan.FromHours( 10 ) ) },
            { dt, TimeZoneFactory.Create( -1 ), dt.Add( utcOffset + TimeSpan.FromHours( 1 ) ) },
            { dt, TimeZoneFactory.Create( -10 ), dt.Add( utcOffset + TimeSpan.FromHours( 10 ) ) }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, DateTime> GetToTimeZoneWithLocalTimeZoneData(IFixture fixture)
    {
        return GetToLocalTimeZoneData( fixture );
    }

    public static TheoryData<DateTime, TimeZoneInfo, TimeZoneInfo, DateTime> GetToTimeZoneWithoutTargetDaylightSavingData(
        IFixture fixture)
    {
        var dt = new DateTime( 2021, 8, 24, 12, 0, 0 );
        var (tz1, tz2) = (TimeZoneFactory.Create( 3 ), TimeZoneFactory.Create( 5 ));

        return new TheoryData<DateTime, TimeZoneInfo, TimeZoneInfo, DateTime>
        {
            { dt, tz1, tz1, dt },
            { dt, tz1, TimeZoneFactory.Create( 0 ), dt.Subtract( tz1.BaseUtcOffset ) },
            { dt, tz1, TimeZoneFactory.Create( 1 ), dt.Subtract( tz1.BaseUtcOffset - TimeSpan.FromHours( 1 ) ) },
            { dt, tz1, TimeZoneFactory.Create( 10 ), dt.Subtract( tz1.BaseUtcOffset - TimeSpan.FromHours( 10 ) ) },
            { dt, tz1, TimeZoneFactory.Create( -1 ), dt.Subtract( tz1.BaseUtcOffset + TimeSpan.FromHours( 1 ) ) },
            { dt, tz1, TimeZoneFactory.Create( -10 ), dt.Subtract( tz1.BaseUtcOffset + TimeSpan.FromHours( 10 ) ) },
            { dt, tz2, tz2, dt },
            { dt, tz2, TimeZoneFactory.Create( 0 ), dt.Subtract( tz2.BaseUtcOffset ) },
            { dt, tz2, TimeZoneFactory.Create( 1 ), dt.Subtract( tz2.BaseUtcOffset - TimeSpan.FromHours( 1 ) ) },
            { dt, tz2, TimeZoneFactory.Create( 10 ), dt.Subtract( tz2.BaseUtcOffset - TimeSpan.FromHours( 10 ) ) },
            { dt, tz2, TimeZoneFactory.Create( -1 ), dt.Subtract( tz2.BaseUtcOffset + TimeSpan.FromHours( 1 ) ) },
            { dt, tz2, TimeZoneFactory.Create( -10 ), dt.Subtract( tz2.BaseUtcOffset + TimeSpan.FromHours( 10 ) ) }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, TimeZoneInfo, DateTime> GetToTimeZoneWithInactiveTargetDaylightSavingData(
        IFixture fixture)
    {
        var dt = new DateTime( 2021, 10, 24, 12, 0, 0 );
        var (tz1, tz2) = (TimeZoneFactory.Create( 3 ), TimeZoneFactory.Create( 5 ));
        var (tStart, tEnd) = (new DateTime( 1, 3, 26, 12, 0, 0 ), new DateTime( 1, 9, 26, 12, 0, 0 ));

        return new TheoryData<DateTime, TimeZoneInfo, TimeZoneInfo, DateTime>
        {
            {
                dt, tz1, TimeZoneFactory.Create( 0, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd ) ),
                dt.Subtract( tz1.BaseUtcOffset )
            },
            {
                dt, tz1, TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd ) ),
                dt.Subtract( tz1.BaseUtcOffset - TimeSpan.FromHours( 1 ) )
            },
            {
                dt, tz1, TimeZoneFactory.Create( 10, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd ) ),
                dt.Subtract( tz1.BaseUtcOffset - TimeSpan.FromHours( 10 ) )
            },
            {
                dt, tz1, TimeZoneFactory.Create( -1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd ) ),
                dt.Subtract( tz1.BaseUtcOffset + TimeSpan.FromHours( 1 ) )
            },
            {
                dt, tz1, TimeZoneFactory.Create( -10, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd ) ),
                dt.Subtract( tz1.BaseUtcOffset + TimeSpan.FromHours( 10 ) )
            },
            {
                dt, tz2, TimeZoneFactory.Create( 0, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd ) ),
                dt.Subtract( tz2.BaseUtcOffset )
            },
            {
                dt, tz2, TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd ) ),
                dt.Subtract( tz2.BaseUtcOffset - TimeSpan.FromHours( 1 ) )
            },
            {
                dt, tz2, TimeZoneFactory.Create( 10, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd ) ),
                dt.Subtract( tz2.BaseUtcOffset - TimeSpan.FromHours( 10 ) )
            },
            {
                dt, tz2, TimeZoneFactory.Create( -1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd ) ),
                dt.Subtract( tz2.BaseUtcOffset + TimeSpan.FromHours( 1 ) )
            },
            {
                dt, tz2, TimeZoneFactory.Create( -10, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd ) ),
                dt.Subtract( tz2.BaseUtcOffset + TimeSpan.FromHours( 10 ) )
            }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, TimeZoneInfo, DateTime> GetToTimeZoneWithActiveTargetDaylightSavingData(
        IFixture fixture)
    {
        var dt = new DateTime( 2021, 8, 24, 12, 0, 0 );
        var (tz1, tz2) = (TimeZoneFactory.Create( 3 ), TimeZoneFactory.Create( 5 ));
        var (tStart, tEnd) = (new DateTime( 1, 3, 26, 12, 0, 0 ), new DateTime( 1, 9, 26, 12, 0, 0 ));

        return new TheoryData<DateTime, TimeZoneInfo, TimeZoneInfo, DateTime>
        {
            {
                dt, tz1, TimeZoneFactory.Create( 0, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd ) ),
                dt.Subtract( tz1.BaseUtcOffset - TimeSpan.FromHours( 1 ) )
            },
            {
                dt, tz1, TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd ) ),
                dt.Subtract( tz1.BaseUtcOffset - TimeSpan.FromHours( 2 ) )
            },
            {
                dt, tz1, TimeZoneFactory.Create( 10, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd ) ),
                dt.Subtract( tz1.BaseUtcOffset - TimeSpan.FromHours( 11 ) )
            },
            {
                dt, tz1, TimeZoneFactory.Create( -1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd ) ),
                dt.Subtract( tz1.BaseUtcOffset )
            },
            {
                dt, tz1, TimeZoneFactory.Create( -10, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd ) ),
                dt.Subtract( tz1.BaseUtcOffset + TimeSpan.FromHours( 9 ) )
            },
            {
                dt, tz2, TimeZoneFactory.Create( 0, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd ) ),
                dt.Subtract( tz2.BaseUtcOffset - TimeSpan.FromHours( 1 ) )
            },
            {
                dt, tz2, TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd ) ),
                dt.Subtract( tz2.BaseUtcOffset - TimeSpan.FromHours( 2 ) )
            },
            {
                dt, tz2, TimeZoneFactory.Create( 10, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd ) ),
                dt.Subtract( tz2.BaseUtcOffset - TimeSpan.FromHours( 11 ) )
            },
            {
                dt, tz2, TimeZoneFactory.Create( -1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd ) ),
                dt.Subtract( tz2.BaseUtcOffset )
            },
            {
                dt, tz2, TimeZoneFactory.Create( -10, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd ) ),
                dt.Subtract( tz2.BaseUtcOffset + TimeSpan.FromHours( 9 ) )
            }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, TimeZoneInfo, DateTime, bool> GetToTimeZoneWithDateTimeOnTheEdgeOfInvalidityData(
        IFixture fixture)
    {
        var (tz1, tz2) = (TimeZoneFactory.Create( 3 ), TimeZoneFactory.Create( 5 ));

        var daylightOffset = 1.0;

        var timeZoneWithPositiveDaylightSaving = TimeZoneFactory.Create(
            daylightOffset,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 9, 26, 12, 0, 0 ),
                transitionEnd: new DateTime( 1, 3, 26, 12, 0, 0 ) ) );

        var timeZoneWithNegativeDaylightSaving = TimeZoneFactory.Create(
            daylightOffset,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 9, 26, 12, 0, 0 ),
                transitionEnd: new DateTime( 1, 3, 26, 12, 0, 0 ),
                daylightDeltaInHours: -1.0 ) );

        return new TheoryData<DateTime, TimeZoneInfo, TimeZoneInfo, DateTime, bool>
        {
            {
                new DateTime( 2021, 9, 26, 13, 59, 59, 999 ).AddTicks( 9999 ),
                tz1,
                timeZoneWithPositiveDaylightSaving,
                new DateTime( 2021, 9, 26, 11, 59, 59, 999 ).AddTicks( 9999 ),
                false
            },
            {
                new DateTime( 2021, 9, 26, 14, 0, 0 ),
                tz1,
                timeZoneWithPositiveDaylightSaving,
                new DateTime( 2021, 9, 26, 13, 0, 0 ),
                true
            },
            {
                new DateTime( 2021, 9, 26, 15, 59, 59, 999 ).AddTicks( 9999 ),
                tz2,
                timeZoneWithPositiveDaylightSaving,
                new DateTime( 2021, 9, 26, 11, 59, 59, 999 ).AddTicks( 9999 ),
                false
            },
            {
                new DateTime( 2021, 9, 26, 16, 0, 0 ),
                tz2,
                timeZoneWithPositiveDaylightSaving,
                new DateTime( 2021, 9, 26, 13, 0, 0 ),
                true
            },
            {
                new DateTime( 2021, 3, 26, 14, 59, 59, 999 ).AddTicks( 9999 ),
                tz1,
                timeZoneWithNegativeDaylightSaving,
                new DateTime( 2021, 3, 26, 11, 59, 59, 999 ).AddTicks( 9999 ),
                true
            },
            {
                new DateTime( 2021, 3, 26, 15, 0, 0 ),
                tz1,
                timeZoneWithNegativeDaylightSaving,
                new DateTime( 2021, 3, 26, 13, 0, 0 ),
                false
            },
            {
                new DateTime( 2021, 3, 26, 16, 59, 59, 999 ).AddTicks( 9999 ),
                tz2,
                timeZoneWithNegativeDaylightSaving,
                new DateTime( 2021, 3, 26, 11, 59, 59, 999 ).AddTicks( 9999 ),
                true
            },
            {
                new DateTime( 2021, 3, 26, 17, 0, 0 ),
                tz2,
                timeZoneWithNegativeDaylightSaving,
                new DateTime( 2021, 3, 26, 13, 0, 0 ),
                false
            }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, TimeZoneInfo, DateTime> GetToTimeZoneWithAmbiguousDateTimeData(
        IFixture fixture)
    {
        var (tz1, tz2) = (TimeZoneFactory.Create( 3 ), TimeZoneFactory.Create( 5 ));

        var daylightOffset = 1.0;

        var timeZoneWithPositiveDaylightSaving = TimeZoneFactory.Create(
            daylightOffset,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 9, 26, 12, 0, 0 ),
                transitionEnd: new DateTime( 1, 3, 26, 12, 0, 0 ) ) );

        var timeZoneWithNegativeDaylightSaving = TimeZoneFactory.Create(
            daylightOffset,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 9, 26, 12, 0, 0 ),
                transitionEnd: new DateTime( 1, 3, 26, 12, 0, 0 ),
                daylightDeltaInHours: -1.0 ) );

        return new TheoryData<DateTime, TimeZoneInfo, TimeZoneInfo, DateTime>
        {
            { new DateTime( 2021, 3, 26, 13, 0, 0 ), tz1, timeZoneWithPositiveDaylightSaving, new DateTime( 2021, 3, 26, 11, 0, 0 ) },
            { new DateTime( 2021, 3, 26, 13, 30, 0 ), tz1, timeZoneWithPositiveDaylightSaving, new DateTime( 2021, 3, 26, 11, 30, 0 ) },
            {
                new DateTime( 2021, 3, 26, 13, 59, 59, 999 ).AddTicks( 9999 ),
                tz1,
                timeZoneWithPositiveDaylightSaving,
                new DateTime( 2021, 3, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
            },
            { new DateTime( 2021, 3, 26, 15, 0, 0 ), tz2, timeZoneWithPositiveDaylightSaving, new DateTime( 2021, 3, 26, 11, 0, 0 ) },
            { new DateTime( 2021, 3, 26, 15, 30, 0 ), tz2, timeZoneWithPositiveDaylightSaving, new DateTime( 2021, 3, 26, 11, 30, 0 ) },
            {
                new DateTime( 2021, 3, 26, 15, 59, 59, 999 ).AddTicks( 9999 ),
                tz2,
                timeZoneWithPositiveDaylightSaving,
                new DateTime( 2021, 3, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
            },
            { new DateTime( 2021, 9, 26, 13, 0, 0 ), tz1, timeZoneWithNegativeDaylightSaving, new DateTime( 2021, 9, 26, 11, 0, 0 ) },
            { new DateTime( 2021, 9, 26, 13, 30, 0 ), tz1, timeZoneWithNegativeDaylightSaving, new DateTime( 2021, 9, 26, 11, 30, 0 ) },
            {
                new DateTime( 2021, 9, 26, 13, 59, 59, 999 ).AddTicks( 9999 ),
                tz1,
                timeZoneWithNegativeDaylightSaving,
                new DateTime( 2021, 9, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
            },
            { new DateTime( 2021, 9, 26, 15, 0, 0 ), tz2, timeZoneWithNegativeDaylightSaving, new DateTime( 2021, 9, 26, 11, 0, 0 ) },
            { new DateTime( 2021, 9, 26, 15, 30, 0 ), tz2, timeZoneWithNegativeDaylightSaving, new DateTime( 2021, 9, 26, 11, 30, 0 ) },
            {
                new DateTime( 2021, 9, 26, 15, 59, 59, 999 ).AddTicks( 9999 ),
                tz2,
                timeZoneWithNegativeDaylightSaving,
                new DateTime( 2021, 9, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
            }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, TimeZoneInfo, DateTime, bool> GetToTimeZoneWithDateTimeOnTheEdgeOfAmbiguityData(
        IFixture fixture)
    {
        var (tz1, tz2) = (TimeZoneFactory.Create( 3 ), TimeZoneFactory.Create( 5 ));

        var daylightOffset = 1.0;

        var timeZoneWithPositiveDaylightSaving = TimeZoneFactory.Create(
            daylightOffset,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 9, 26, 12, 0, 0 ),
                transitionEnd: new DateTime( 1, 3, 26, 12, 0, 0 ) ) );

        var timeZoneWithNegativeDaylightSaving = TimeZoneFactory.Create(
            daylightOffset,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 9, 26, 12, 0, 0 ),
                transitionEnd: new DateTime( 1, 3, 26, 12, 0, 0 ),
                daylightDeltaInHours: -1.0 ) );

        return new TheoryData<DateTime, TimeZoneInfo, TimeZoneInfo, DateTime, bool>
        {
            {
                new DateTime( 2021, 3, 26, 11, 59, 59, 999 ).AddTicks( 9999 ),
                tz1,
                timeZoneWithPositiveDaylightSaving,
                new DateTime( 2021, 3, 26, 10, 59, 59, 999 ).AddTicks( 9999 ),
                true
            },
            {
                new DateTime( 2021, 3, 26, 14, 0, 0 ),
                tz1,
                timeZoneWithPositiveDaylightSaving,
                new DateTime( 2021, 3, 26, 12, 0, 0 ),
                false
            },
            {
                new DateTime( 2021, 3, 26, 13, 59, 59, 999 ).AddTicks( 9999 ),
                tz2,
                timeZoneWithPositiveDaylightSaving,
                new DateTime( 2021, 3, 26, 10, 59, 59, 999 ).AddTicks( 9999 ),
                true
            },
            {
                new DateTime( 2021, 3, 26, 16, 0, 0 ),
                tz2,
                timeZoneWithPositiveDaylightSaving,
                new DateTime( 2021, 3, 26, 12, 0, 0 ),
                false
            },
            {
                new DateTime( 2021, 9, 26, 12, 59, 59, 999 ).AddTicks( 9999 ),
                tz1,
                timeZoneWithNegativeDaylightSaving,
                new DateTime( 2021, 9, 26, 10, 59, 59, 999 ).AddTicks( 9999 ),
                false
            },
            {
                new DateTime( 2021, 9, 26, 15, 0, 0 ),
                tz1,
                timeZoneWithNegativeDaylightSaving,
                new DateTime( 2021, 9, 26, 12, 0, 0 ),
                true
            },
            {
                new DateTime( 2021, 9, 26, 14, 59, 59, 999 ).AddTicks( 9999 ),
                tz2,
                timeZoneWithNegativeDaylightSaving,
                new DateTime( 2021, 9, 26, 10, 59, 59, 999 ).AddTicks( 9999 ),
                false
            },
            {
                new DateTime( 2021, 9, 26, 17, 0, 0 ),
                tz2,
                timeZoneWithNegativeDaylightSaving,
                new DateTime( 2021, 9, 26, 12, 0, 0 ),
                true
            }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, long, DateTime> GetAddWithDurationAndNoChangesToDaylightSavingData(
        IFixture fixture)
    {
        var (tStart, tEnd) = (new DateTime( 1, 3, 26, 12, 0, 0 ), new DateTime( 1, 9, 26, 12, 0, 0 ));

        return new TheoryData<DateTime, TimeZoneInfo, long, DateTime>
        {
            {
                new DateTime( 2021, 8, 12, 12, 30, 17, 123 ).AddTicks( 99 ),
                TimeZoneInfo.Utc,
                0,
                new DateTime( 2021, 8, 12, 12, 30, 17, 123 ).AddTicks( 99 )
            },
            {
                new DateTime( 2021, 8, 12, 12, 30, 17, 123 ).AddTicks( 99 ),
                TimeZoneFactory.Create( 1 ),
                1,
                new DateTime( 2021, 8, 12, 12, 30, 17, 123 ).AddTicks( 100 )
            },
            {
                new DateTime( 2021, 8, 12, 12, 30, 17, 123 ).AddTicks( 99 ),
                TimeZoneFactory.Create( 1 ),
                -1,
                new DateTime( 2021, 8, 12, 12, 30, 17, 123 ).AddTicks( 98 )
            },
            {
                new DateTime( 2021, 8, 12, 12, 30, 17, 123 ).AddTicks( 99 ),
                TimeZoneFactory.Create( 2 ),
                900610010001,
                new DateTime( 2021, 8, 13, 13, 31, 18, 124 ).AddTicks( 100 )
            },
            {
                new DateTime( 2021, 8, 12, 12, 30, 17, 123 ).AddTicks( 99 ),
                TimeZoneFactory.Create( 2 ),
                -900610010001,
                new DateTime( 2021, 8, 11, 11, 29, 16, 122 ).AddTicks( 98 )
            },
            {
                new DateTime( 2021, 8, 12, 12, 30, 17, 123 ).AddTicks( 99 ),
                TimeZoneFactory.Create( 3, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd ) ),
                900610010002,
                new DateTime( 2021, 8, 13, 13, 31, 18, 124 ).AddTicks( 101 )
            },
            {
                new DateTime( 2021, 8, 12, 12, 30, 17, 123 ).AddTicks( 99 ),
                TimeZoneFactory.Create( 3, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd ) ),
                -900610010002,
                new DateTime( 2021, 8, 11, 11, 29, 16, 122 ).AddTicks( 97 )
            },
            {
                new DateTime( 2021, 8, 12, 12, 30, 17, 123 ).AddTicks( 99 ),
                TimeZoneFactory.Create( 3, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd, daylightDeltaInHours: -1 ) ),
                900610010002,
                new DateTime( 2021, 8, 13, 13, 31, 18, 124 ).AddTicks( 101 )
            },
            {
                new DateTime( 2021, 8, 12, 12, 30, 17, 123 ).AddTicks( 99 ),
                TimeZoneFactory.Create( 3, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd, daylightDeltaInHours: -1 ) ),
                -900610010002,
                new DateTime( 2021, 8, 11, 11, 29, 16, 122 ).AddTicks( 97 )
            },
            {
                new DateTime( 2021, 8, 12, 12, 30, 17, 123 ).AddTicks( 99 ),
                TimeZoneFactory.Create( 4, TimeZoneFactory.CreateInfiniteRule( tEnd, tStart ) ),
                900610010002,
                new DateTime( 2021, 8, 13, 13, 31, 18, 124 ).AddTicks( 101 )
            },
            {
                new DateTime( 2021, 8, 12, 12, 30, 17, 123 ).AddTicks( 99 ),
                TimeZoneFactory.Create( 4, TimeZoneFactory.CreateInfiniteRule( tEnd, tStart ) ),
                -900610010002,
                new DateTime( 2021, 8, 11, 11, 29, 16, 122 ).AddTicks( 97 )
            },
            {
                new DateTime( 2021, 8, 12, 12, 30, 17, 123 ).AddTicks( 99 ),
                TimeZoneFactory.Create( 4, TimeZoneFactory.CreateInfiniteRule( tEnd, tStart, daylightDeltaInHours: -1 ) ),
                900610010002,
                new DateTime( 2021, 8, 13, 13, 31, 18, 124 ).AddTicks( 101 )
            },
            {
                new DateTime( 2021, 8, 12, 12, 30, 17, 123 ).AddTicks( 99 ),
                TimeZoneFactory.Create( 4, TimeZoneFactory.CreateInfiniteRule( tEnd, tStart, daylightDeltaInHours: -1 ) ),
                -900610010002,
                new DateTime( 2021, 8, 11, 11, 29, 16, 122 ).AddTicks( 97 )
            },
            {
                new DateTime( 2021, 8, 12, 12, 30, 17, 123 ).AddTicks( 99 ),
                TimeZoneFactory.Create( 5, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd ) ),
                315360000000000,
                new DateTime( 2022, 8, 12, 12, 30, 17, 123 ).AddTicks( 99 )
            },
            {
                new DateTime( 2021, 8, 12, 12, 30, 17, 123 ).AddTicks( 99 ),
                TimeZoneFactory.Create( 5, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd ) ),
                -315360000000000,
                new DateTime( 2020, 8, 12, 12, 30, 17, 123 ).AddTicks( 99 )
            },
            {
                new DateTime( 2021, 8, 12, 12, 30, 17, 123 ).AddTicks( 99 ),
                TimeZoneFactory.Create( 5, TimeZoneFactory.CreateInfiniteRule( tEnd, tStart ) ),
                315360000000000,
                new DateTime( 2022, 8, 12, 12, 30, 17, 123 ).AddTicks( 99 )
            },
            {
                new DateTime( 2021, 8, 12, 12, 30, 17, 123 ).AddTicks( 99 ),
                TimeZoneFactory.Create( 5, TimeZoneFactory.CreateInfiniteRule( tEnd, tStart ) ),
                -315360000000000,
                new DateTime( 2020, 8, 12, 12, 30, 17, 123 ).AddTicks( 99 )
            },
            {
                new DateTime( 2021, 3, 25, 11, 30, 10, 100 ),
                TimeZoneFactory.Create( 2, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd ) ),
                881898999999,
                new DateTime( 2021, 3, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 3, 27, 13, 29, 49, 899 ).AddTicks( 9999 ),
                TimeZoneFactory.Create( 2, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd ) ),
                -881898999999,
                new DateTime( 2021, 3, 26, 13, 0, 0 )
            },
            {
                new DateTime( 2021, 9, 25, 10, 30, 10, 100 ),
                TimeZoneFactory.Create( 2, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd ) ),
                881898999999,
                new DateTime( 2021, 9, 26, 10, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 9, 27, 12, 29, 49, 899 ).AddTicks( 9999 ),
                TimeZoneFactory.Create( 2, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd ) ),
                -881898999999,
                new DateTime( 2021, 9, 26, 12, 0, 0 )
            },
            {
                new DateTime( 2021, 9, 25, 11, 30, 10, 100 ),
                TimeZoneFactory.Create( 2, TimeZoneFactory.CreateInfiniteRule( tEnd, tStart ) ),
                881898999999,
                new DateTime( 2021, 9, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 9, 27, 13, 29, 49, 899 ).AddTicks( 9999 ),
                TimeZoneFactory.Create( 2, TimeZoneFactory.CreateInfiniteRule( tEnd, tStart ) ),
                -881898999999,
                new DateTime( 2021, 9, 26, 13, 0, 0 )
            },
            {
                new DateTime( 2021, 3, 25, 10, 30, 10, 100 ),
                TimeZoneFactory.Create( 2, TimeZoneFactory.CreateInfiniteRule( tEnd, tStart ) ),
                881898999999,
                new DateTime( 2021, 3, 26, 10, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 3, 27, 12, 29, 49, 899 ).AddTicks( 9999 ),
                TimeZoneFactory.Create( 2, TimeZoneFactory.CreateInfiniteRule( tEnd, tStart ) ),
                -881898999999,
                new DateTime( 2021, 3, 26, 12, 0, 0 )
            },
            {
                new DateTime( 2021, 3, 25, 10, 30, 10, 100 ),
                TimeZoneFactory.Create( 2, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd, daylightDeltaInHours: -1 ) ),
                881898999999,
                new DateTime( 2021, 3, 26, 10, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 3, 27, 12, 29, 49, 899 ).AddTicks( 9999 ),
                TimeZoneFactory.Create( 2, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd, daylightDeltaInHours: -1 ) ),
                -881898999999,
                new DateTime( 2021, 3, 26, 12, 0, 0 )
            },
            {
                new DateTime( 2021, 9, 25, 11, 30, 10, 100 ),
                TimeZoneFactory.Create( 2, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd, daylightDeltaInHours: -1 ) ),
                881898999999,
                new DateTime( 2021, 9, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 9, 27, 13, 29, 49, 899 ).AddTicks( 9999 ),
                TimeZoneFactory.Create( 2, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd, daylightDeltaInHours: -1 ) ),
                -881898999999,
                new DateTime( 2021, 9, 26, 13, 0, 0 )
            },
            {
                new DateTime( 2021, 9, 25, 10, 30, 10, 100 ),
                TimeZoneFactory.Create( 2, TimeZoneFactory.CreateInfiniteRule( tEnd, tStart, daylightDeltaInHours: -1 ) ),
                881898999999,
                new DateTime( 2021, 9, 26, 10, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 9, 27, 12, 29, 49, 899 ).AddTicks( 9999 ),
                TimeZoneFactory.Create( 2, TimeZoneFactory.CreateInfiniteRule( tEnd, tStart, daylightDeltaInHours: -1 ) ),
                -881898999999,
                new DateTime( 2021, 9, 26, 12, 0, 0 )
            },
            {
                new DateTime( 2021, 3, 25, 11, 30, 10, 100 ),
                TimeZoneFactory.Create( 2, TimeZoneFactory.CreateInfiniteRule( tEnd, tStart, daylightDeltaInHours: -1 ) ),
                881898999999,
                new DateTime( 2021, 3, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 3, 27, 13, 29, 49, 899 ).AddTicks( 9999 ),
                TimeZoneFactory.Create( 2, TimeZoneFactory.CreateInfiniteRule( tEnd, tStart, daylightDeltaInHours: -1 ) ),
                -881898999999,
                new DateTime( 2021, 3, 26, 13, 0, 0 )
            }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, long, DateTime> GetAddWithDurationAndChangesToDaylightSavingData(
        IFixture fixture)
    {
        var (tStart, tEnd) = (new DateTime( 1, 3, 26, 12, 0, 0 ), new DateTime( 1, 9, 26, 12, 0, 0 ));

        return new TheoryData<DateTime, TimeZoneInfo, long, DateTime>
        {
            {
                new DateTime( 2021, 3, 25, 11, 30, 10, 100 ).AddTicks( 2000 ),
                TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd ) ),
                1728000000000,
                new DateTime( 2021, 3, 27, 12, 30, 10, 100 ).AddTicks( 2000 )
            },
            {
                new DateTime( 2021, 3, 25, 11, 30, 10, 100 ).AddTicks( 2000 ),
                TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd ) ),
                317088000000000,
                new DateTime( 2022, 3, 27, 12, 30, 10, 100 ).AddTicks( 2000 )
            },
            {
                new DateTime( 2021, 3, 27, 12, 30, 10, 100 ).AddTicks( 2000 ),
                TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd ) ),
                -1728000000000,
                new DateTime( 2021, 3, 25, 11, 30, 10, 100 ).AddTicks( 2000 )
            },
            {
                new DateTime( 2022, 3, 27, 12, 30, 10, 100 ).AddTicks( 2000 ),
                TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd ) ),
                -317088000000000,
                new DateTime( 2021, 3, 25, 11, 30, 10, 100 ).AddTicks( 2000 )
            },
            {
                new DateTime( 2021, 9, 25, 12, 30, 10, 100 ).AddTicks( 2000 ),
                TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd ) ),
                1728000000000,
                new DateTime( 2021, 9, 27, 11, 30, 10, 100 ).AddTicks( 2000 )
            },
            {
                new DateTime( 2021, 9, 25, 12, 30, 10, 100 ).AddTicks( 2000 ),
                TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd ) ),
                317088000000000,
                new DateTime( 2022, 9, 27, 11, 30, 10, 100 ).AddTicks( 2000 )
            },
            {
                new DateTime( 2021, 9, 27, 11, 30, 10, 100 ).AddTicks( 2000 ),
                TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd ) ),
                -1728000000000,
                new DateTime( 2021, 9, 25, 12, 30, 10, 100 ).AddTicks( 2000 )
            },
            {
                new DateTime( 2022, 9, 27, 11, 30, 10, 100 ).AddTicks( 2000 ),
                TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd ) ),
                -317088000000000,
                new DateTime( 2021, 9, 25, 12, 30, 10, 100 ).AddTicks( 2000 )
            },
            {
                new DateTime( 2021, 3, 25, 12, 30, 10, 100 ).AddTicks( 2000 ),
                TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd, daylightDeltaInHours: -1 ) ),
                1728000000000,
                new DateTime( 2021, 3, 27, 11, 30, 10, 100 ).AddTicks( 2000 )
            },
            {
                new DateTime( 2021, 3, 25, 12, 30, 10, 100 ).AddTicks( 2000 ),
                TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd, daylightDeltaInHours: -1 ) ),
                317088000000000,
                new DateTime( 2022, 3, 27, 11, 30, 10, 100 ).AddTicks( 2000 )
            },
            {
                new DateTime( 2021, 3, 27, 11, 30, 10, 100 ).AddTicks( 2000 ),
                TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd, daylightDeltaInHours: -1 ) ),
                -1728000000000,
                new DateTime( 2021, 3, 25, 12, 30, 10, 100 ).AddTicks( 2000 )
            },
            {
                new DateTime( 2022, 3, 27, 11, 30, 10, 100 ).AddTicks( 2000 ),
                TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd, daylightDeltaInHours: -1 ) ),
                -317088000000000,
                new DateTime( 2021, 3, 25, 12, 30, 10, 100 ).AddTicks( 2000 )
            },
            {
                new DateTime( 2021, 9, 25, 11, 30, 10, 100 ).AddTicks( 2000 ),
                TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd, daylightDeltaInHours: -1 ) ),
                1728000000000,
                new DateTime( 2021, 9, 27, 12, 30, 10, 100 ).AddTicks( 2000 )
            },
            {
                new DateTime( 2021, 9, 25, 11, 30, 10, 100 ).AddTicks( 2000 ),
                TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd, daylightDeltaInHours: -1 ) ),
                317088000000000,
                new DateTime( 2022, 9, 27, 12, 30, 10, 100 ).AddTicks( 2000 )
            },
            {
                new DateTime( 2021, 9, 27, 12, 30, 10, 100 ).AddTicks( 2000 ),
                TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd, daylightDeltaInHours: -1 ) ),
                -1728000000000,
                new DateTime( 2021, 9, 25, 11, 30, 10, 100 ).AddTicks( 2000 )
            },
            {
                new DateTime( 2022, 9, 27, 12, 30, 10, 100 ).AddTicks( 2000 ),
                TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd, daylightDeltaInHours: -1 ) ),
                -317088000000000,
                new DateTime( 2021, 9, 25, 11, 30, 10, 100 ).AddTicks( 2000 )
            },
            {
                new DateTime( 2021, 3, 26, 11, 59, 59, 999 ).AddTicks( 9999 ),
                TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd ) ),
                1,
                new DateTime( 2021, 3, 26, 13, 0, 0 )
            },
            {
                new DateTime( 2021, 3, 26, 13, 0, 0 ),
                TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd ) ),
                -1,
                new DateTime( 2021, 3, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 9, 26, 10, 59, 59, 999 ).AddTicks( 9999 ),
                TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd ) ),
                72000000001,
                new DateTime( 2021, 9, 26, 12, 0, 0 )
            },
            {
                new DateTime( 2021, 9, 26, 12, 0, 0 ),
                TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd ) ),
                -72000000001,
                new DateTime( 2021, 9, 26, 10, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 3, 26, 10, 59, 59, 999 ).AddTicks( 9999 ),
                TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd, daylightDeltaInHours: -1 ) ),
                72000000001,
                new DateTime( 2021, 3, 26, 12, 0, 0 )
            },
            {
                new DateTime( 2021, 3, 26, 12, 0, 0 ),
                TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd, daylightDeltaInHours: -1 ) ),
                -72000000001,
                new DateTime( 2021, 3, 26, 10, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 9, 26, 11, 59, 59, 999 ).AddTicks( 9999 ),
                TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd, daylightDeltaInHours: -1 ) ),
                1,
                new DateTime( 2021, 9, 26, 13, 0, 0 )
            },
            {
                new DateTime( 2021, 9, 26, 13, 0, 0 ),
                TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd, daylightDeltaInHours: -1 ) ),
                -1,
                new DateTime( 2021, 9, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
            }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, long, DateTime, bool> GetAddWithDurationAndAmbiguousDateTimeData(
        IFixture fixture)
    {
        var (tStart, tEnd) = (new DateTime( 1, 3, 26, 12, 0, 0 ), new DateTime( 1, 9, 26, 12, 0, 0 ));

        return new TheoryData<DateTime, TimeZoneInfo, long, DateTime, bool>
        {
            {
                new DateTime( 2021, 9, 26, 10, 59, 59, 999 ).AddTicks( 9999 ),
                TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd ) ),
                1,
                new DateTime( 2021, 9, 26, 11, 0, 0 ),
                true
            },
            {
                new DateTime( 2021, 9, 26, 10, 59, 59, 999 ).AddTicks( 9999 ),
                TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd ) ),
                18000000001,
                new DateTime( 2021, 9, 26, 11, 30, 0 ),
                true
            },
            {
                new DateTime( 2021, 9, 26, 10, 59, 59, 999 ).AddTicks( 9999 ),
                TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd ) ),
                36000000000,
                new DateTime( 2021, 9, 26, 11, 59, 59, 999 ).AddTicks( 9999 ),
                true
            },
            {
                new DateTime( 2021, 9, 26, 10, 59, 59, 999 ).AddTicks( 9999 ),
                TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd ) ),
                36000000001,
                new DateTime( 2021, 9, 26, 11, 0, 0 ),
                false
            },
            {
                new DateTime( 2021, 9, 26, 10, 59, 59, 999 ).AddTicks( 9999 ),
                TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd ) ),
                54000000001,
                new DateTime( 2021, 9, 26, 11, 30, 0 ),
                false
            },
            {
                new DateTime( 2021, 9, 26, 10, 59, 59, 999 ).AddTicks( 9999 ),
                TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd ) ),
                72000000000,
                new DateTime( 2021, 9, 26, 11, 59, 59, 999 ).AddTicks( 9999 ),
                false
            },
            {
                new DateTime( 2021, 3, 26, 10, 59, 59, 999 ).AddTicks( 9999 ),
                TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd, daylightDeltaInHours: -1 ) ),
                1,
                new DateTime( 2021, 3, 26, 11, 0, 0 ),
                false
            },
            {
                new DateTime( 2021, 3, 26, 10, 59, 59, 999 ).AddTicks( 9999 ),
                TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd, daylightDeltaInHours: -1 ) ),
                18000000001,
                new DateTime( 2021, 3, 26, 11, 30, 0 ),
                false
            },
            {
                new DateTime( 2021, 3, 26, 10, 59, 59, 999 ).AddTicks( 9999 ),
                TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd, daylightDeltaInHours: -1 ) ),
                36000000000,
                new DateTime( 2021, 3, 26, 11, 59, 59, 999 ).AddTicks( 9999 ),
                false
            },
            {
                new DateTime( 2021, 3, 26, 10, 59, 59, 999 ).AddTicks( 9999 ),
                TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd, daylightDeltaInHours: -1 ) ),
                36000000001,
                new DateTime( 2021, 3, 26, 11, 0, 0 ),
                true
            },
            {
                new DateTime( 2021, 3, 26, 10, 59, 59, 999 ).AddTicks( 9999 ),
                TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd, daylightDeltaInHours: -1 ) ),
                54000000001,
                new DateTime( 2021, 3, 26, 11, 30, 0 ),
                true
            },
            {
                new DateTime( 2021, 3, 26, 10, 59, 59, 999 ).AddTicks( 9999 ),
                TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd, daylightDeltaInHours: -1 ) ),
                72000000000,
                new DateTime( 2021, 3, 26, 11, 59, 59, 999 ).AddTicks( 9999 ),
                true
            }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, Period, DateTime> GetAddWithPeriodData(IFixture fixture)
    {
        var dsTimeZone = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 8, 26, 12, 0, 0 ),
                transitionEnd: new DateTime( 1, 10, 26, 12, 0, 0 ) ) );

        return new TheoryData<DateTime, TimeZoneInfo, Period, DateTime>
        {
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                TimeZoneFactory.Create( 1 ),
                Period.Empty,
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                TimeZoneFactory.Create( 1 ),
                new Period( 1, 2, 3, 4, 5, 6 ),
                new DateTime( 2021, 8, 26, 13, 32, 43, 504 ).AddTicks( 6057 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                TimeZoneFactory.Create( 1 ),
                new Period( -1, -2, -3, -4, -5, -6 ),
                new DateTime( 2021, 8, 26, 11, 28, 37, 496 ).AddTicks( 5945 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                TimeZoneFactory.Create( 1 ),
                new Period( 1, 2, 3, 4 ),
                new DateTime( 2022, 11, 20, 12, 30, 40, 500 ).AddTicks( 6001 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                TimeZoneFactory.Create( 1 ),
                new Period( -1, -2, -3, -4 ),
                new DateTime( 2020, 6, 1, 12, 30, 40, 500 ).AddTicks( 6001 )
            },
            {
                new DateTime( 2021, 3, 31, 12, 30, 40, 500 ).AddTicks( 6001 ),
                TimeZoneFactory.Create( 1 ),
                Period.FromMonths( 1 ),
                new DateTime( 2021, 4, 30, 12, 30, 40, 500 ).AddTicks( 6001 )
            },
            {
                new DateTime( 2021, 3, 31, 12, 30, 40, 500 ).AddTicks( 6001 ),
                TimeZoneFactory.Create( 1 ),
                Period.FromMonths( -1 ),
                new DateTime( 2021, 2, 28, 12, 30, 40, 500 ).AddTicks( 6001 )
            },
            {
                new DateTime( 2021, 3, 31, 12, 30, 40, 500 ).AddTicks( 6001 ),
                TimeZoneFactory.Create( 1 ),
                Period.FromYears( -1 ).SubtractMonths( 1 ),
                new DateTime( 2020, 2, 29, 12, 30, 40, 500 ).AddTicks( 6001 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                TimeZoneFactory.Create( 1 ),
                new Period( 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 ),
                new DateTime( 2022, 11, 20, 17, 36, 47, 508 ).AddTicks( 6101 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                TimeZoneFactory.Create( 1 ),
                new Period( 1, -2, 3, -4, 5, -6, 7, -8, 9, -10 ),
                new DateTime( 2022, 7, 13, 17, 24, 47, 492 ).AddTicks( 6081 )
            },
            {
                new DateTime( 2021, 7, 25, 11, 59, 59, 999 ).AddTicks( 9998 ),
                dsTimeZone,
                Period.FromMonths( 1 ).AddDays( 1 ).AddTicks( 1 ),
                new DateTime( 2021, 8, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 8, 26, 11, 0, 0 ),
                dsTimeZone,
                Period.FromHours( 2 ),
                new DateTime( 2021, 8, 26, 13, 0, 0 )
            },
            {
                new DateTime( 2021, 9, 27, 13, 0, 0 ).AddTicks( 1 ),
                dsTimeZone,
                Period.FromMonths( -1 ).SubtractDays( 1 ).SubtractTicks( 1 ),
                new DateTime( 2021, 8, 26, 13, 0, 0 )
            },
            {
                new DateTime( 2021, 8, 26, 13, 0, 0 ),
                dsTimeZone,
                Period.FromHours( -1 ).SubtractTicks( 1 ),
                new DateTime( 2021, 8, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 9, 25, 10, 59, 59, 999 ).AddTicks( 9998 ),
                dsTimeZone,
                Period.FromMonths( 1 ).AddDays( 1 ).AddTicks( 1 ),
                new DateTime( 2021, 10, 26, 10, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 10, 26, 10, 0, 0 ),
                dsTimeZone,
                Period.FromHours( 2 ),
                new DateTime( 2021, 10, 26, 12, 0, 0 )
            },
            {
                new DateTime( 2021, 11, 27, 12, 0, 0 ).AddTicks( 1 ),
                dsTimeZone,
                Period.FromMonths( -1 ).SubtractDays( 1 ).SubtractTicks( 1 ),
                new DateTime( 2021, 10, 26, 12, 0, 0 )
            },
            {
                new DateTime( 2021, 10, 26, 12, 0, 0 ),
                dsTimeZone,
                Period.FromHours( -1 ).SubtractTicks( 1 ),
                new DateTime( 2021, 10, 26, 10, 59, 59, 999 ).AddTicks( 9999 )
            }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, bool, Period, DateTime> GetAddWithPeriodAndAmbiguityData(
        IFixture fixture)
    {
        var timeZone = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 8, 26, 12, 0, 0 ),
                transitionEnd: new DateTime( 1, 10, 26, 12, 0, 0 ) ) );

        return new TheoryData<DateTime, TimeZoneInfo, bool, Period, DateTime>
        {
            {
                new DateTime( 2021, 10, 26, 11, 0, 0 ),
                timeZone,
                false,
                Period.Empty,
                new DateTime( 2021, 10, 26, 11, 0, 0 )
            },
            {
                new DateTime( 2021, 10, 26, 11, 0, 0 ),
                timeZone,
                true,
                Period.Empty,
                new DateTime( 2021, 10, 26, 11, 0, 0 )
            },
            {
                new DateTime( 2022, 11, 27, 16, 0, 0 ),
                timeZone,
                false,
                Period.FromYears( -1 ).SubtractMonths( 1 ).SubtractDays( 1 ).SubtractHours( 5 ),
                new DateTime( 2021, 10, 26, 11, 0, 0 )
            },
            {
                new DateTime( 2020, 9, 25, 6, 0, 0 ),
                timeZone,
                true,
                Period.FromYears( 1 ).AddMonths( 1 ).AddDays( 1 ).AddHours( 5 ),
                new DateTime( 2021, 10, 26, 11, 0, 0 )
            },
            {
                new DateTime( 2021, 10, 26, 11, 59, 59, 999 ).AddTicks( 9999 ),
                timeZone,
                false,
                Period.Empty,
                new DateTime( 2021, 10, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 10, 26, 11, 59, 59, 999 ).AddTicks( 9999 ),
                timeZone,
                true,
                Period.Empty,
                new DateTime( 2021, 10, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2022, 11, 27, 16, 0, 0 ),
                timeZone,
                false,
                Period.FromYears( -1 ).SubtractMonths( 1 ).SubtractDays( 1 ).SubtractHours( 4 ).SubtractTicks( 1 ),
                new DateTime( 2021, 10, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2020, 9, 25, 6, 0, 0 ),
                timeZone,
                true,
                Period.FromYears( 1 ).AddMonths( 1 ).AddDays( 1 ).AddHours( 6 ).SubtractTicks( 1 ),
                new DateTime( 2021, 10, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
            }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, Period> GetAddWithPeriodThrowData(IFixture fixture)
    {
        var timeZoneWithInvalidity = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 8, 26, 12, 0, 0 ),
                transitionEnd: new DateTime( 1, 10, 26, 12, 0, 0 ) ) );

        return new TheoryData<DateTime, TimeZoneInfo, Period>
        {
            {
                new DateTime( 2020, 7, 20, 11, 0, 0 ),
                timeZoneWithInvalidity,
                Period.FromYears( 1 ).AddMonths( 1 ).AddDays( 6 ).AddHours( 1 )
            },
            {
                new DateTime( 2020, 7, 20, 11, 0, 0 ),
                timeZoneWithInvalidity,
                Period.FromYears( 1 ).AddMonths( 1 ).AddDays( 6 ).AddHours( 2 ).SubtractTicks( 1 )
            },
            {
                new DateTime( 2022, 9, 30, 14, 0, 0 ),
                timeZoneWithInvalidity,
                Period.FromYears( -1 ).SubtractMonths( 1 ).SubtractDays( 4 ).SubtractHours( 2 )
            },
            {
                new DateTime( 2022, 9, 30, 14, 0, 0 ),
                timeZoneWithInvalidity,
                Period.FromYears( -1 ).SubtractMonths( 1 ).SubtractDays( 4 ).SubtractHours( 1 ).SubtractTicks( 1 )
            }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, DateTime, PeriodUnits, Period> GetGetPeriodOffsetData(
        IFixture fixture)
    {
        var timeZone = TimeZoneFactory.Create( 1 );

        return new TheoryData<DateTime, TimeZoneInfo, DateTime, PeriodUnits, Period>
        {
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                timeZone,
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                PeriodUnits.All,
                Period.Empty
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                timeZone,
                new DateTime( 2020, 6, 1, 7, 24, 33, 492 ).AddTicks( 5910 ),
                PeriodUnits.All,
                new Period( 1, 2, 3, 4, 5, 6, 7, 8, 9, 1 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                timeZone,
                new DateTime( 2020, 10, 20, 15, 29, 50, 450 ).AddTicks( 6092 ),
                PeriodUnits.All,
                new Period( 0, 10, 0, 5, 21, 0, 50, 49, 990, 9 )
            },
            {
                new DateTime( 2022, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                timeZone,
                new DateTime( 2020, 9, 28, 13, 31, 41, 501 ).AddTicks( 6002 ),
                PeriodUnits.All,
                new Period( 1, 10, 3, 6, 22, 58, 58, 998, 999, 9 )
            },
            {
                new DateTime( 2022, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                timeZone,
                new DateTime( 2020, 10, 20, 15, 29, 50, 450 ).AddTicks( 6101 ),
                PeriodUnits.Years,
                Period.FromYears( 1 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                timeZone,
                new DateTime( 2020, 10, 20, 15, 29, 50, 450 ).AddTicks( 6101 ),
                PeriodUnits.Months,
                Period.FromMonths( 10 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                timeZone,
                new DateTime( 2020, 10, 20, 15, 29, 50, 450 ).AddTicks( 6101 ),
                PeriodUnits.Weeks,
                Period.FromWeeks( 44 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                timeZone,
                new DateTime( 2020, 10, 20, 15, 29, 50, 450 ).AddTicks( 6101 ),
                PeriodUnits.Days,
                Period.FromDays( 309 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                timeZone,
                new DateTime( 2020, 10, 20, 15, 29, 50, 450 ).AddTicks( 6101 ),
                PeriodUnits.Hours,
                Period.FromHours( 7437 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                timeZone,
                new DateTime( 2020, 10, 20, 15, 29, 50, 450 ).AddTicks( 6101 ),
                PeriodUnits.Minutes,
                Period.FromMinutes( 446220 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                timeZone,
                new DateTime( 2020, 10, 20, 15, 29, 50, 450 ).AddTicks( 6101 ),
                PeriodUnits.Seconds,
                Period.FromSeconds( 26773250 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                timeZone,
                new DateTime( 2020, 10, 20, 15, 29, 50, 450 ).AddTicks( 6101 ),
                PeriodUnits.Milliseconds,
                Period.FromMilliseconds( 26773250049 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                timeZone,
                new DateTime( 2020, 10, 20, 15, 29, 50, 450 ).AddTicks( 6101 ),
                PeriodUnits.Ticks,
                Period.FromTicks( 267732500499900 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                timeZone,
                new DateTime( 2020, 10, 20, 15, 29, 50, 450 ).AddTicks( 6101 ),
                PeriodUnits.Date,
                new Period( 0, 10, 0, 5 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                timeZone,
                new DateTime( 2020, 10, 20, 15, 29, 50, 450 ).AddTicks( 6092 ),
                PeriodUnits.Time,
                new Period( 7437, 0, 50, 49, 990, 9 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                timeZone,
                new DateTime( 2020, 10, 20, 15, 29, 50, 450 ).AddTicks( 6101 ),
                PeriodUnits.Months | PeriodUnits.Days | PeriodUnits.Seconds | PeriodUnits.Ticks,
                Period.FromMonths( 10 ).AddDays( 5 ).AddSeconds( 75650 ).AddTicks( 499900 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                timeZone,
                new DateTime( 2021, 6, 28, 13, 31, 42, 503 ).AddTicks( 6005 ),
                PeriodUnits.Months | PeriodUnits.Weeks | PeriodUnits.Hours,
                Period.FromMonths( 1 ).AddWeeks( 3 ).AddHours( 166 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                timeZone,
                new DateTime( 2020, 6, 26, 11, 31, 42, 503 ).AddTicks( 6005 ),
                PeriodUnits.Months | PeriodUnits.Days | PeriodUnits.Minutes,
                Period.FromMonths( 14 ).AddMinutes( 58 )
            }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, DateTime, PeriodUnits, Period> GetGetGreedyPeriodOffsetData(
        IFixture fixture)
    {
        var timeZone = TimeZoneFactory.Create( 1 );

        return new TheoryData<DateTime, TimeZoneInfo, DateTime, PeriodUnits, Period>
        {
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                timeZone,
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                PeriodUnits.All,
                Period.Empty
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                timeZone,
                new DateTime( 2020, 6, 1, 7, 24, 33, 492 ).AddTicks( 5910 ),
                PeriodUnits.All,
                new Period( 1, 2, 3, 4, 5, 6, 7, 8, 9, 1 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                timeZone,
                new DateTime( 2020, 10, 20, 15, 29, 50, 450 ).AddTicks( 7006 ),
                PeriodUnits.All,
                new Period( 1, -2, 0, 6, -3, 1, -10, 50, -100, -5 )
            },
            {
                new DateTime( 2022, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                timeZone,
                new DateTime( 2020, 9, 28, 13, 31, 41, 501 ).AddTicks( 6012 ),
                PeriodUnits.All,
                new Period( 2, -1, 0, -2, -1, -1, -1, -1, -1, -1 )
            },
            {
                new DateTime( 2022, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                timeZone,
                new DateTime( 2020, 10, 20, 15, 29, 50, 450 ).AddTicks( 6101 ),
                PeriodUnits.Years,
                Period.FromYears( 2 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                timeZone,
                new DateTime( 2020, 10, 20, 15, 29, 50, 450 ).AddTicks( 6101 ),
                PeriodUnits.Months,
                Period.FromMonths( 10 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                timeZone,
                new DateTime( 2020, 10, 20, 15, 29, 50, 450 ).AddTicks( 6101 ),
                PeriodUnits.Weeks,
                Period.FromWeeks( 44 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                timeZone,
                new DateTime( 2020, 10, 20, 15, 29, 50, 450 ).AddTicks( 6101 ),
                PeriodUnits.Days,
                Period.FromDays( 310 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                timeZone,
                new DateTime( 2020, 10, 20, 15, 29, 50, 450 ).AddTicks( 6101 ),
                PeriodUnits.Hours,
                Period.FromHours( 7437 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                timeZone,
                new DateTime( 2020, 10, 20, 15, 29, 50, 450 ).AddTicks( 6101 ),
                PeriodUnits.Minutes,
                Period.FromMinutes( 446221 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                timeZone,
                new DateTime( 2020, 10, 20, 15, 29, 50, 450 ).AddTicks( 6101 ),
                PeriodUnits.Seconds,
                Period.FromSeconds( 26773250 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                timeZone,
                new DateTime( 2020, 10, 20, 15, 29, 50, 450 ).AddTicks( 6101 ),
                PeriodUnits.Milliseconds,
                Period.FromMilliseconds( 26773250050 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                timeZone,
                new DateTime( 2020, 10, 20, 15, 29, 50, 450 ).AddTicks( 6101 ),
                PeriodUnits.Ticks,
                Period.FromTicks( 267732500499900 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                timeZone,
                new DateTime( 2020, 10, 20, 15, 29, 50, 450 ).AddTicks( 6101 ),
                PeriodUnits.Date,
                new Period( 1, -2, 0, 6 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                timeZone,
                new DateTime( 2020, 10, 20, 15, 29, 50, 450 ).AddTicks( 7006 ),
                PeriodUnits.Time,
                new Period( 7437, 1, -10, 50, -100, -5 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                timeZone,
                new DateTime( 2020, 10, 20, 15, 29, 50, 450 ).AddTicks( 6101 ),
                PeriodUnits.Months | PeriodUnits.Days | PeriodUnits.Seconds | PeriodUnits.Ticks,
                Period.FromMonths( 10 ).AddDays( 6 ).SubtractSeconds( 10750 ).AddTicks( 499900 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                timeZone,
                new DateTime( 2021, 6, 28, 13, 31, 42, 503 ).AddTicks( 6005 ),
                PeriodUnits.Months | PeriodUnits.Weeks | PeriodUnits.Hours,
                Period.FromMonths( 2 ).SubtractHours( 49 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                timeZone,
                new DateTime( 2020, 6, 26, 11, 31, 42, 503 ).AddTicks( 6005 ),
                PeriodUnits.Months | PeriodUnits.Days | PeriodUnits.Minutes,
                Period.FromMonths( 14 ).AddMinutes( 59 )
            }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, int, DateTime> GetSetYearData(IFixture fixture)
    {
        var simpleTimeZone = TimeZoneFactory.Create( 1 );

        var timeZoneWithInvalidity = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateRule(
                start: DateTime.MinValue,
                end: new DateTime( 2020, 1, 1 ),
                transitionStart: new DateTime( 1, 8, 26, 12, 0, 0 ),
                transitionEnd: new DateTime( 1, 10, 26, 12, 0, 0 ) ) );

        var timeZoneWithYearOverlapInvalidity = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateRule(
                start: DateTime.MinValue,
                end: new DateTime( 2020, 1, 1 ),
                transitionStart: new DateTime( 1, 12, 31, 23, 30, 0 ),
                transitionEnd: new DateTime( 1, 10, 26, 12, 0, 0 ) ) );

        return new TheoryData<DateTime, TimeZoneInfo, int, DateTime>
        {
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                simpleTimeZone,
                DateTime.MinValue.Year,
                new DateTime( DateTime.MinValue.Year, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                simpleTimeZone,
                DateTime.MaxValue.Year,
                new DateTime( DateTime.MaxValue.Year, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                simpleTimeZone,
                2021,
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                simpleTimeZone,
                2022,
                new DateTime( 2022, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 )
            },
            {
                new DateTime( 2020, 2, 29, 12, 30, 40, 500 ).AddTicks( 6001 ),
                simpleTimeZone,
                2016,
                new DateTime( 2016, 2, 29, 12, 30, 40, 500 ).AddTicks( 6001 )
            },
            {
                new DateTime( 2020, 2, 29, 12, 30, 40, 500 ).AddTicks( 6001 ),
                simpleTimeZone,
                2017,
                new DateTime( 2017, 2, 28, 12, 30, 40, 500 ).AddTicks( 6001 )
            },
            {
                new DateTime( 2021, 8, 26, 11, 59, 59, 999 ).AddTicks( 9999 ),
                timeZoneWithInvalidity,
                2019,
                new DateTime( 2019, 8, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 8, 26, 13, 0, 0 ),
                timeZoneWithInvalidity,
                2018,
                new DateTime( 2018, 8, 26, 13, 0, 0 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 0, 0 ),
                timeZoneWithInvalidity,
                2017,
                new DateTime( 2017, 8, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 59, 59, 999 ).AddTicks( 9999 ),
                timeZoneWithInvalidity,
                2016,
                new DateTime( 2016, 8, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 1, 1, 0, 30, 0 ),
                timeZoneWithYearOverlapInvalidity,
                2015,
                new DateTime( 2015, 1, 1, 0, 30, 0 )
            },
            {
                new DateTime( 2021, 1, 1 ),
                timeZoneWithYearOverlapInvalidity,
                2014,
                new DateTime( 2014, 1, 1, 0, 30, 0 )
            },
            {
                new DateTime( 2021, 1, 1, 0, 29, 59, 999 ).AddTicks( 9999 ),
                timeZoneWithYearOverlapInvalidity,
                2013,
                new DateTime( 2013, 1, 1, 0, 30, 0 )
            }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, bool, int, DateTime> GetSetYearWithAmbiguityData(IFixture fixture)
    {
        var timeZone = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 8, 26, 12, 0, 0 ),
                transitionEnd: new DateTime( 1, 10, 26, 12, 0, 0 ) ) );

        return new TheoryData<DateTime, TimeZoneInfo, bool, int, DateTime>
        {
            { new DateTime( 2021, 10, 26, 11, 0, 0 ), timeZone, false, 2021, new DateTime( 2021, 10, 26, 11, 0, 0 ) },
            { new DateTime( 2021, 10, 26, 11, 0, 0 ), timeZone, true, 2021, new DateTime( 2021, 10, 26, 11, 0, 0 ) },
            { new DateTime( 2021, 10, 26, 11, 0, 0 ), timeZone, false, 2019, new DateTime( 2019, 10, 26, 11, 0, 0 ) },
            { new DateTime( 2021, 10, 26, 11, 0, 0 ), timeZone, true, 2019, new DateTime( 2019, 10, 26, 11, 0, 0 ) },
            {
                new DateTime( 2021, 10, 26, 11, 59, 59, 999 ).AddTicks( 9999 ),
                timeZone,
                false,
                2021,
                new DateTime( 2021, 10, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 10, 26, 11, 59, 59, 999 ).AddTicks( 9999 ),
                timeZone,
                true,
                2021,
                new DateTime( 2021, 10, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 10, 26, 11, 59, 59, 999 ).AddTicks( 9999 ),
                timeZone,
                false,
                2019,
                new DateTime( 2019, 10, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 10, 26, 11, 59, 59, 999 ).AddTicks( 9999 ),
                timeZone,
                true,
                2019,
                new DateTime( 2019, 10, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
            }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, int> GetSetYearThrowData(IFixture fixture)
    {
        return new TheoryData<DateTime, TimeZoneInfo, int>
        {
            { fixture.Create<DateTime>(), TimeZoneFactory.Create( 1 ), DateTime.MinValue.Year - 1 },
            { fixture.Create<DateTime>(), TimeZoneFactory.Create( 1 ), DateTime.MaxValue.Year + 1 }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, IsoMonthOfYear, DateTime> GetSetMonthData(IFixture fixture)
    {
        var simpleTimeZone = TimeZoneFactory.Create( 1 );

        var timeZoneWithInvalidity = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 4, 26, 12, 0, 0 ),
                transitionEnd: new DateTime( 1, 10, 26, 12, 0, 0 ) ) );

        var timeZoneWithMonthOverlapInvalidity = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 4, 30, 23, 30, 0 ),
                transitionEnd: new DateTime( 1, 10, 26, 12, 0, 0 ) ) );

        return new TheoryData<DateTime, TimeZoneInfo, IsoMonthOfYear, DateTime>
        {
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                simpleTimeZone,
                IsoMonthOfYear.January,
                new DateTime( 2021, 1, 26, 12, 30, 40, 500 ).AddTicks( 6001 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                simpleTimeZone,
                IsoMonthOfYear.December,
                new DateTime( 2021, 12, 26, 12, 30, 40, 500 ).AddTicks( 6001 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                simpleTimeZone,
                IsoMonthOfYear.August,
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                simpleTimeZone,
                IsoMonthOfYear.March,
                new DateTime( 2021, 3, 26, 12, 30, 40, 500 ).AddTicks( 6001 )
            },
            {
                new DateTime( 2021, 3, 29, 12, 30, 40, 500 ).AddTicks( 6001 ),
                simpleTimeZone,
                IsoMonthOfYear.February,
                new DateTime( 2021, 2, 28, 12, 30, 40, 500 ).AddTicks( 6001 )
            },
            {
                new DateTime( 2021, 4, 30, 12, 30, 40, 500 ).AddTicks( 6001 ),
                simpleTimeZone,
                IsoMonthOfYear.February,
                new DateTime( 2021, 2, 28, 12, 30, 40, 500 ).AddTicks( 6001 )
            },
            {
                new DateTime( 2021, 5, 31, 12, 30, 40, 500 ).AddTicks( 6001 ),
                simpleTimeZone,
                IsoMonthOfYear.February,
                new DateTime( 2021, 2, 28, 12, 30, 40, 500 ).AddTicks( 6001 )
            },
            {
                new DateTime( 2020, 5, 31, 12, 30, 40, 500 ).AddTicks( 6001 ),
                simpleTimeZone,
                IsoMonthOfYear.February,
                new DateTime( 2020, 2, 29, 12, 30, 40, 500 ).AddTicks( 6001 )
            },
            {
                new DateTime( 2021, 8, 26, 11, 59, 59, 999 ).AddTicks( 9999 ),
                timeZoneWithInvalidity,
                IsoMonthOfYear.April,
                new DateTime( 2021, 4, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 8, 26, 13, 0, 0 ),
                timeZoneWithInvalidity,
                IsoMonthOfYear.April,
                new DateTime( 2021, 4, 26, 13, 0, 0 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 0, 0 ),
                timeZoneWithInvalidity,
                IsoMonthOfYear.April,
                new DateTime( 2021, 4, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 59, 59, 999 ).AddTicks( 9999 ),
                timeZoneWithInvalidity,
                IsoMonthOfYear.April,
                new DateTime( 2021, 4, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 8, 1, 0, 30, 0 ),
                timeZoneWithMonthOverlapInvalidity,
                IsoMonthOfYear.May,
                new DateTime( 2021, 5, 1, 0, 30, 0 )
            },
            {
                new DateTime( 2021, 8, 1 ),
                timeZoneWithMonthOverlapInvalidity,
                IsoMonthOfYear.May,
                new DateTime( 2021, 5, 1, 0, 30, 0 )
            },
            {
                new DateTime( 2021, 8, 1, 0, 29, 59, 999 ).AddTicks( 9999 ),
                timeZoneWithMonthOverlapInvalidity,
                IsoMonthOfYear.May,
                new DateTime( 2021, 5, 1, 0, 30, 0 )
            }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, bool, IsoMonthOfYear, DateTime> GetSetMonthWithAmbiguityData(IFixture fixture)
    {
        var timeZone = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 8, 26, 12, 0, 0 ),
                transitionEnd: new DateTime( 1, 10, 26, 12, 0, 0 ) ) );

        return new TheoryData<DateTime, TimeZoneInfo, bool, IsoMonthOfYear, DateTime>
        {
            { new DateTime( 2021, 10, 26, 11, 0, 0 ), timeZone, false, IsoMonthOfYear.October, new DateTime( 2021, 10, 26, 11, 0, 0 ) },
            { new DateTime( 2021, 10, 26, 11, 0, 0 ), timeZone, true, IsoMonthOfYear.October, new DateTime( 2021, 10, 26, 11, 0, 0 ) },
            { new DateTime( 2021, 7, 26, 11, 0, 0 ), timeZone, false, IsoMonthOfYear.October, new DateTime( 2021, 10, 26, 11, 0, 0 ) },
            { new DateTime( 2021, 9, 26, 11, 0, 0 ), timeZone, true, IsoMonthOfYear.October, new DateTime( 2021, 10, 26, 11, 0, 0 ) },
            {
                new DateTime( 2021, 10, 26, 11, 59, 59, 999 ).AddTicks( 9999 ),
                timeZone,
                false,
                IsoMonthOfYear.October,
                new DateTime( 2021, 10, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 10, 26, 11, 59, 59, 999 ).AddTicks( 9999 ),
                timeZone,
                true,
                IsoMonthOfYear.October,
                new DateTime( 2021, 10, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 7, 26, 11, 59, 59, 999 ).AddTicks( 9999 ),
                timeZone,
                false,
                IsoMonthOfYear.October,
                new DateTime( 2021, 10, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 9, 26, 11, 59, 59, 999 ).AddTicks( 9999 ),
                timeZone,
                true,
                IsoMonthOfYear.October,
                new DateTime( 2021, 10, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
            }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, int, DateTime> GetSetDayOfMonthData(IFixture fixture)
    {
        var simpleTimeZone = TimeZoneFactory.Create( 1 );

        var timeZoneWithInvalidity = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 8, 16, 12, 0, 0 ),
                transitionEnd: new DateTime( 1, 10, 26, 12, 0, 0 ) ) );

        var timeZoneWithDayOverlapInvalidity = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 8, 15, 23, 30, 0 ),
                transitionEnd: new DateTime( 1, 10, 26, 12, 0, 0 ) ) );

        return new TheoryData<DateTime, TimeZoneInfo, int, DateTime>
        {
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                simpleTimeZone,
                1,
                new DateTime( 2021, 8, 1, 12, 30, 40, 500 ).AddTicks( 6001 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                simpleTimeZone,
                31,
                new DateTime( 2021, 8, 31, 12, 30, 40, 500 ).AddTicks( 6001 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                simpleTimeZone,
                26,
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 )
            },
            {
                new DateTime( 2021, 6, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                simpleTimeZone,
                30,
                new DateTime( 2021, 6, 30, 12, 30, 40, 500 ).AddTicks( 6001 )
            },
            {
                new DateTime( 2021, 2, 20, 12, 30, 40, 500 ).AddTicks( 6001 ),
                simpleTimeZone,
                28,
                new DateTime( 2021, 2, 28, 12, 30, 40, 500 ).AddTicks( 6001 )
            },
            {
                new DateTime( 2020, 2, 20, 12, 30, 40, 500 ).AddTicks( 6001 ),
                simpleTimeZone,
                29,
                new DateTime( 2020, 2, 29, 12, 30, 40, 500 ).AddTicks( 6001 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                simpleTimeZone,
                11,
                new DateTime( 2021, 8, 11, 12, 30, 40, 500 ).AddTicks( 6001 )
            },
            {
                new DateTime( 2021, 8, 26, 11, 59, 59, 999 ).AddTicks( 9999 ),
                timeZoneWithInvalidity,
                16,
                new DateTime( 2021, 8, 16, 11, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 8, 26, 13, 0, 0 ),
                timeZoneWithInvalidity,
                16,
                new DateTime( 2021, 8, 16, 13, 0, 0 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 0, 0 ),
                timeZoneWithInvalidity,
                16,
                new DateTime( 2021, 8, 16, 11, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 59, 59, 999 ).AddTicks( 9999 ),
                timeZoneWithInvalidity,
                16,
                new DateTime( 2021, 8, 16, 11, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 8, 26, 0, 30, 0 ),
                timeZoneWithDayOverlapInvalidity,
                16,
                new DateTime( 2021, 8, 16, 0, 30, 0 )
            },
            {
                new DateTime( 2021, 8, 26 ),
                timeZoneWithDayOverlapInvalidity,
                16,
                new DateTime( 2021, 8, 16, 0, 30, 0 )
            },
            {
                new DateTime( 2021, 8, 26, 0, 29, 59, 999 ).AddTicks( 9999 ),
                timeZoneWithDayOverlapInvalidity,
                16,
                new DateTime( 2021, 8, 16, 0, 30, 0 )
            }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, bool, int, DateTime> GetSetDayOfMonthWithAmbiguityData(IFixture fixture)
    {
        var timeZone = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 8, 26, 12, 0, 0 ),
                transitionEnd: new DateTime( 1, 10, 26, 12, 0, 0 ) ) );

        return new TheoryData<DateTime, TimeZoneInfo, bool, int, DateTime>
        {
            { new DateTime( 2021, 10, 26, 11, 0, 0 ), timeZone, false, 26, new DateTime( 2021, 10, 26, 11, 0, 0 ) },
            { new DateTime( 2021, 10, 26, 11, 0, 0 ), timeZone, true, 26, new DateTime( 2021, 10, 26, 11, 0, 0 ) },
            { new DateTime( 2021, 10, 30, 11, 0, 0 ), timeZone, false, 26, new DateTime( 2021, 10, 26, 11, 0, 0 ) },
            { new DateTime( 2021, 10, 16, 11, 0, 0 ), timeZone, true, 26, new DateTime( 2021, 10, 26, 11, 0, 0 ) },
            {
                new DateTime( 2021, 10, 26, 11, 59, 59, 999 ).AddTicks( 9999 ),
                timeZone,
                false,
                26,
                new DateTime( 2021, 10, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 10, 26, 11, 59, 59, 999 ).AddTicks( 9999 ),
                timeZone,
                true,
                26,
                new DateTime( 2021, 10, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 10, 30, 11, 59, 59, 999 ).AddTicks( 9999 ),
                timeZone,
                false,
                26,
                new DateTime( 2021, 10, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 10, 16, 11, 59, 59, 999 ).AddTicks( 9999 ),
                timeZone,
                true,
                26,
                new DateTime( 2021, 10, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
            }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, int> GetSetDayOfMonthThrowData(IFixture fixture)
    {
        return new TheoryData<DateTime, TimeZoneInfo, int>
        {
            { new DateTime( 2021, 1, 1 ), TimeZoneFactory.Create( 1 ), 0 },
            { new DateTime( 2021, 2, 1 ), TimeZoneFactory.Create( 1 ), 0 },
            { new DateTime( 2021, 3, 1 ), TimeZoneFactory.Create( 1 ), 0 },
            { new DateTime( 2021, 4, 1 ), TimeZoneFactory.Create( 1 ), 0 },
            { new DateTime( 2021, 5, 1 ), TimeZoneFactory.Create( 1 ), 0 },
            { new DateTime( 2021, 6, 1 ), TimeZoneFactory.Create( 1 ), 0 },
            { new DateTime( 2021, 7, 1 ), TimeZoneFactory.Create( 1 ), 0 },
            { new DateTime( 2021, 8, 1 ), TimeZoneFactory.Create( 1 ), 0 },
            { new DateTime( 2021, 9, 1 ), TimeZoneFactory.Create( 1 ), 0 },
            { new DateTime( 2021, 10, 1 ), TimeZoneFactory.Create( 1 ), 0 },
            { new DateTime( 2021, 11, 1 ), TimeZoneFactory.Create( 1 ), 0 },
            { new DateTime( 2021, 12, 1 ), TimeZoneFactory.Create( 1 ), 0 },
            { new DateTime( 2021, 1, 1 ), TimeZoneFactory.Create( 1 ), ChronoConstants.DaysInJanuary + 1 },
            { new DateTime( 2021, 2, 1 ), TimeZoneFactory.Create( 1 ), ChronoConstants.DaysInFebruary + 1 },
            { new DateTime( 2020, 2, 1 ), TimeZoneFactory.Create( 1 ), ChronoConstants.DaysInLeapFebruary + 1 },
            { new DateTime( 2021, 3, 1 ), TimeZoneFactory.Create( 1 ), ChronoConstants.DaysInMarch + 1 },
            { new DateTime( 2021, 4, 1 ), TimeZoneFactory.Create( 1 ), ChronoConstants.DaysInApril + 1 },
            { new DateTime( 2021, 5, 1 ), TimeZoneFactory.Create( 1 ), ChronoConstants.DaysInMay + 1 },
            { new DateTime( 2021, 6, 1 ), TimeZoneFactory.Create( 1 ), ChronoConstants.DaysInJune + 1 },
            { new DateTime( 2021, 7, 1 ), TimeZoneFactory.Create( 1 ), ChronoConstants.DaysInJuly + 1 },
            { new DateTime( 2021, 8, 1 ), TimeZoneFactory.Create( 1 ), ChronoConstants.DaysInAugust + 1 },
            { new DateTime( 2021, 9, 1 ), TimeZoneFactory.Create( 1 ), ChronoConstants.DaysInSeptember + 1 },
            { new DateTime( 2021, 10, 1 ), TimeZoneFactory.Create( 1 ), ChronoConstants.DaysInOctober + 1 },
            { new DateTime( 2021, 11, 1 ), TimeZoneFactory.Create( 1 ), ChronoConstants.DaysInNovember + 1 },
            { new DateTime( 2021, 12, 1 ), TimeZoneFactory.Create( 1 ), ChronoConstants.DaysInDecember + 1 },
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, int, DateTime> GetSetDayOfYearData(IFixture fixture)
    {
        var simpleTimeZone = TimeZoneFactory.Create( 1 );

        var timeZoneWithInvalidity = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 1, 16, 12, 0, 0 ),
                transitionEnd: new DateTime( 1, 10, 26, 12, 0, 0 ) ) );

        var timeZoneWithDayOverlapInvalidity = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 1, 15, 23, 30, 0 ),
                transitionEnd: new DateTime( 1, 10, 26, 12, 0, 0 ) ) );

        return new TheoryData<DateTime, TimeZoneInfo, int, DateTime>
        {
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                simpleTimeZone,
                1,
                new DateTime( 2021, 1, 1, 12, 30, 40, 500 ).AddTicks( 6001 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                simpleTimeZone,
                31,
                new DateTime( 2021, 1, 31, 12, 30, 40, 500 ).AddTicks( 6001 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                simpleTimeZone,
                32,
                new DateTime( 2021, 2, 1, 12, 30, 40, 500 ).AddTicks( 6001 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                simpleTimeZone,
                59,
                new DateTime( 2021, 2, 28, 12, 30, 40, 500 ).AddTicks( 6001 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                simpleTimeZone,
                60,
                new DateTime( 2021, 3, 1, 12, 30, 40, 500 ).AddTicks( 6001 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                simpleTimeZone,
                90,
                new DateTime( 2021, 3, 31, 12, 30, 40, 500 ).AddTicks( 6001 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                simpleTimeZone,
                91,
                new DateTime( 2021, 4, 1, 12, 30, 40, 500 ).AddTicks( 6001 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                simpleTimeZone,
                238,
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                simpleTimeZone,
                365,
                new DateTime( 2021, 12, 31, 12, 30, 40, 500 ).AddTicks( 6001 )
            },
            {
                new DateTime( 2020, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                simpleTimeZone,
                60,
                new DateTime( 2020, 2, 29, 12, 30, 40, 500 ).AddTicks( 6001 )
            },
            {
                new DateTime( 2020, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                simpleTimeZone,
                61,
                new DateTime( 2020, 3, 1, 12, 30, 40, 500 ).AddTicks( 6001 )
            },
            {
                new DateTime( 2020, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                simpleTimeZone,
                365,
                new DateTime( 2020, 12, 30, 12, 30, 40, 500 ).AddTicks( 6001 )
            },
            {
                new DateTime( 2020, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                simpleTimeZone,
                366,
                new DateTime( 2020, 12, 31, 12, 30, 40, 500 ).AddTicks( 6001 )
            },
            {
                new DateTime( 2021, 8, 26, 11, 59, 59, 999 ).AddTicks( 9999 ),
                timeZoneWithInvalidity,
                16,
                new DateTime( 2021, 1, 16, 11, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 8, 26, 13, 0, 0 ),
                timeZoneWithInvalidity,
                16,
                new DateTime( 2021, 1, 16, 13, 0, 0 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 0, 0 ),
                timeZoneWithInvalidity,
                16,
                new DateTime( 2021, 1, 16, 11, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 59, 59, 999 ).AddTicks( 9999 ),
                timeZoneWithInvalidity,
                16,
                new DateTime( 2021, 1, 16, 11, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 8, 26, 0, 30, 0 ),
                timeZoneWithDayOverlapInvalidity,
                16,
                new DateTime( 2021, 1, 16, 0, 30, 0 )
            },
            {
                new DateTime( 2021, 8, 26 ),
                timeZoneWithDayOverlapInvalidity,
                16,
                new DateTime( 2021, 1, 16, 0, 30, 0 )
            },
            {
                new DateTime( 2021, 8, 26, 0, 29, 59, 999 ).AddTicks( 9999 ),
                timeZoneWithDayOverlapInvalidity,
                16,
                new DateTime( 2021, 1, 16, 0, 30, 0 )
            }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, bool, int, DateTime> GetSetDayOfYearWithAmbiguityData(IFixture fixture)
    {
        var timeZone = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 8, 26, 12, 0, 0 ),
                transitionEnd: new DateTime( 1, 10, 26, 12, 0, 0 ) ) );

        return new TheoryData<DateTime, TimeZoneInfo, bool, int, DateTime>
        {
            { new DateTime( 2021, 10, 26, 11, 0, 0 ), timeZone, false, 299, new DateTime( 2021, 10, 26, 11, 0, 0 ) },
            { new DateTime( 2021, 10, 26, 11, 0, 0 ), timeZone, true, 299, new DateTime( 2021, 10, 26, 11, 0, 0 ) },
            { new DateTime( 2021, 11, 30, 11, 0, 0 ), timeZone, false, 299, new DateTime( 2021, 10, 26, 11, 0, 0 ) },
            { new DateTime( 2021, 9, 16, 11, 0, 0 ), timeZone, true, 299, new DateTime( 2021, 10, 26, 11, 0, 0 ) },
            {
                new DateTime( 2021, 10, 26, 11, 59, 59, 999 ).AddTicks( 9999 ),
                timeZone,
                false,
                299,
                new DateTime( 2021, 10, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 10, 26, 11, 59, 59, 999 ).AddTicks( 9999 ),
                timeZone,
                true,
                299,
                new DateTime( 2021, 10, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 11, 30, 11, 59, 59, 999 ).AddTicks( 9999 ),
                timeZone,
                false,
                299,
                new DateTime( 2021, 10, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 9, 16, 11, 59, 59, 999 ).AddTicks( 9999 ),
                timeZone,
                true,
                299,
                new DateTime( 2021, 10, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
            }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, int> GetSetDayOfYearThrowData(IFixture fixture)
    {
        return new TheoryData<DateTime, TimeZoneInfo, int>
        {
            { new DateTime( 2021, 1, 1 ), TimeZoneFactory.Create( 1 ), 0 },
            { new DateTime( 2021, 1, 1 ), TimeZoneFactory.Create( 1 ), ChronoConstants.DaysInYear + 1 },
            { new DateTime( 2020, 1, 1 ), TimeZoneFactory.Create( 1 ), ChronoConstants.DaysInLeapYear + 1 }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, TimeOfDay, DateTime> GetSetTimeOfDayData(IFixture fixture)
    {
        var simpleTimeZone = TimeZoneFactory.Create( 1 );

        var timeZoneWithInvalidity = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 8, 26, 12, 0, 0 ),
                transitionEnd: new DateTime( 1, 10, 26, 12, 0, 0 ) ) );

        return new TheoryData<DateTime, TimeZoneInfo, TimeOfDay, DateTime>
        {
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                simpleTimeZone,
                TimeOfDay.Start,
                new DateTime( 2021, 8, 26 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                simpleTimeZone,
                TimeOfDay.Mid,
                new DateTime( 2021, 8, 26, 12, 0, 0 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                simpleTimeZone,
                TimeOfDay.End,
                new DateTime( 2021, 8, 26, 23, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                simpleTimeZone,
                new TimeOfDay( 12, 30, 40, 500, 600, 1 ),
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                simpleTimeZone,
                new TimeOfDay( 17, 40, 30, 200, 100, 1 ),
                new DateTime( 2021, 8, 26, 17, 40, 30, 200 ).AddTicks( 1001 )
            },
            {
                new DateTime( 2021, 8, 26 ),
                timeZoneWithInvalidity,
                new TimeOfDay( 11, 59, 59, 999, 999, 9 ),
                new DateTime( 2021, 8, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 8, 26 ),
                timeZoneWithInvalidity,
                new TimeOfDay( 13 ),
                new DateTime( 2021, 8, 26, 13, 0, 0 )
            }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, bool, TimeOfDay, DateTime> GetSetTimeOfDayWithAmbiguityData(
        IFixture fixture)
    {
        var timeZone = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 8, 26, 12, 0, 0 ),
                transitionEnd: new DateTime( 1, 10, 26, 12, 0, 0 ) ) );

        return new TheoryData<DateTime, TimeZoneInfo, bool, TimeOfDay, DateTime>
        {
            {
                new DateTime( 2021, 10, 26, 11, 0, 0 ),
                timeZone,
                false,
                new TimeOfDay( 11 ),
                new DateTime( 2021, 10, 26, 11, 0, 0 )
            },
            {
                new DateTime( 2021, 10, 26, 11, 0, 0 ),
                timeZone,
                true,
                new TimeOfDay( 11 ),
                new DateTime( 2021, 10, 26, 11, 0, 0 )
            },
            {
                new DateTime( 2021, 10, 26, 16, 0, 0 ),
                timeZone,
                false,
                new TimeOfDay( 11 ),
                new DateTime( 2021, 10, 26, 11, 0, 0 )
            },
            {
                new DateTime( 2021, 10, 26, 6, 0, 0 ),
                timeZone,
                true,
                new TimeOfDay( 11 ),
                new DateTime( 2021, 10, 26, 11, 0, 0 )
            },
            {
                new DateTime( 2021, 10, 26, 11, 59, 59, 999 ).AddTicks( 9999 ),
                timeZone,
                false,
                new TimeOfDay( 11, 59, 59, 999, 999, 9 ),
                new DateTime( 2021, 10, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 10, 26, 11, 59, 59, 999 ).AddTicks( 9999 ),
                timeZone,
                true,
                new TimeOfDay( 11, 59, 59, 999, 999, 9 ),
                new DateTime( 2021, 10, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 10, 26, 16, 0, 0 ),
                timeZone,
                false,
                new TimeOfDay( 11, 59, 59, 999, 999, 9 ),
                new DateTime( 2021, 10, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 10, 26, 6, 0, 0 ),
                timeZone,
                true,
                new TimeOfDay( 11, 59, 59, 999, 999, 9 ),
                new DateTime( 2021, 10, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
            }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, TimeOfDay> GetSetTimeOfDayThrowData(IFixture fixture)
    {
        var timeZoneWithInvalidity = TimeZoneFactory.Create(
            utcOffsetInHours: 1,
            TimeZoneFactory.CreateInfiniteRule(
                transitionStart: new DateTime( 1, 8, 26, 12, 0, 0 ),
                transitionEnd: new DateTime( 1, 10, 26, 12, 0, 0 ) ) );

        return new TheoryData<DateTime, TimeZoneInfo, TimeOfDay>
        {
            {
                new DateTime( 2021, 8, 26 ),
                timeZoneWithInvalidity,
                new TimeOfDay( 12 )
            },
            {
                new DateTime( 2021, 8, 26 ),
                timeZoneWithInvalidity,
                new TimeOfDay( 12, 59, 59, 999, 999, 9 )
            }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo> GetGetOppositeAmbiguousDateTimeWithUnambiguousData(
        IFixture fixture)
    {
        var (tStart, tEnd) = (new DateTime( 1, 3, 26, 12, 0, 0 ), new DateTime( 1, 9, 26, 12, 0, 0 ));

        return new TheoryData<DateTime, TimeZoneInfo>
        {
            { new DateTime( 2021, 9, 26, 12, 0, 0 ), TimeZoneFactory.Create( 1 ) },
            { new DateTime( 2021, 9, 26, 12, 0, 0 ), TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd ) ) },
            {
                new DateTime( 2021, 9, 26, 10, 59, 59, 999 ).AddTicks( 9999 ),
                TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd ) )
            },
            {
                new DateTime( 2021, 3, 26, 12, 0, 0 ),
                TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd, daylightDeltaInHours: -1 ) )
            },
            {
                new DateTime( 2021, 3, 26, 10, 59, 59, 999 ).AddTicks( 9999 ),
                TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd, daylightDeltaInHours: -1 ) )
            }
        };
    }

    public static TheoryData<DateTime, TimeZoneInfo, DateTime> GetGetOppositeAmbiguousDateTimeData(
        IFixture fixture)
    {
        var (tStart, tEnd) = (new DateTime( 1, 3, 26, 12, 0, 0 ), new DateTime( 1, 9, 26, 12, 0, 0 ));

        return new TheoryData<DateTime, TimeZoneInfo, DateTime>
        {
            {
                new DateTime( 2021, 9, 26, 9, 0, 0 ), TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd ) ),
                new DateTime( 2021, 9, 26, 10, 0, 0 )
            },
            {
                new DateTime( 2021, 9, 26, 10, 0, 0 ), TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd ) ),
                new DateTime( 2021, 9, 26, 9, 0, 0 )
            },
            {
                new DateTime( 2021, 9, 26, 9, 30, 0 ),
                TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd ) ),
                new DateTime( 2021, 9, 26, 10, 30, 0 )
            },
            {
                new DateTime( 2021, 9, 26, 10, 30, 0 ),
                TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd ) ),
                new DateTime( 2021, 9, 26, 9, 30, 0 )
            },
            {
                new DateTime( 2021, 9, 26, 9, 59, 59, 999 ).AddTicks( 9999 ),
                TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd ) ),
                new DateTime( 2021, 9, 26, 10, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 9, 26, 10, 59, 59, 999 ).AddTicks( 9999 ),
                TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd ) ),
                new DateTime( 2021, 9, 26, 9, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 9, 26, 7, 0, 0 ),
                TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd, daylightDeltaInHours: 3 ) ),
                new DateTime( 2021, 9, 26, 10, 0, 0 )
            },
            {
                new DateTime( 2021, 9, 26, 10, 0, 0 ),
                TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd, daylightDeltaInHours: 3 ) ),
                new DateTime( 2021, 9, 26, 7, 0, 0 )
            },
            {
                new DateTime( 2021, 9, 26, 7, 59, 59, 999 ).AddTicks( 9999 ),
                TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd, daylightDeltaInHours: 3 ) ),
                new DateTime( 2021, 9, 26, 10, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 9, 26, 10, 59, 59, 999 ).AddTicks( 9999 ),
                TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd, daylightDeltaInHours: 3 ) ),
                new DateTime( 2021, 9, 26, 7, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 3, 26, 10, 0, 0 ),
                TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd, daylightDeltaInHours: -1 ) ),
                new DateTime( 2021, 3, 26, 11, 0, 0 )
            },
            {
                new DateTime( 2021, 3, 26, 11, 0, 0 ),
                TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd, daylightDeltaInHours: -1 ) ),
                new DateTime( 2021, 3, 26, 10, 0, 0 )
            },
            {
                new DateTime( 2021, 3, 26, 10, 30, 0 ),
                TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd, daylightDeltaInHours: -1 ) ),
                new DateTime( 2021, 3, 26, 11, 30, 0 )
            },
            {
                new DateTime( 2021, 3, 26, 11, 30, 0 ),
                TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd, daylightDeltaInHours: -1 ) ),
                new DateTime( 2021, 3, 26, 10, 30, 0 )
            },
            {
                new DateTime( 2021, 3, 26, 10, 59, 59, 999 ).AddTicks( 9999 ),
                TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd, daylightDeltaInHours: -1 ) ),
                new DateTime( 2021, 3, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 3, 26, 11, 59, 59, 999 ).AddTicks( 9999 ),
                TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd, daylightDeltaInHours: -1 ) ),
                new DateTime( 2021, 3, 26, 10, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 3, 26, 8, 0, 0 ),
                TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd, daylightDeltaInHours: -3 ) ),
                new DateTime( 2021, 3, 26, 11, 0, 0 )
            },
            {
                new DateTime( 2021, 3, 26, 11, 0, 0 ),
                TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd, daylightDeltaInHours: -3 ) ),
                new DateTime( 2021, 3, 26, 8, 0, 0 )
            },
            {
                new DateTime( 2021, 3, 26, 8, 59, 59, 999 ).AddTicks( 9999 ),
                TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd, daylightDeltaInHours: -3 ) ),
                new DateTime( 2021, 3, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 3, 26, 11, 59, 59, 999 ).AddTicks( 9999 ),
                TimeZoneFactory.Create( 1, TimeZoneFactory.CreateInfiniteRule( tStart, tEnd, daylightDeltaInHours: -3 ) ),
                new DateTime( 2021, 3, 26, 8, 59, 59, 999 ).AddTicks( 9999 )
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
