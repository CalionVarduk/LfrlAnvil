using System;
using System.Collections.Generic;
using AutoFixture;
using LfrlSoft.NET.Core.Chrono;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Chrono.ZonedDateTime
{
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
                var timeZone = GetTimeZone( $"{hourOffset:00.00}", hourOffset );
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
                var timeZone = GetTimeZone(
                    $"{hourOffset:00.00} (DS inactive)",
                    hourOffset,
                    new DateTime( 1, 3, 26, 12, 0, 0 ),
                    new DateTime( 1, 9, 26, 12, 0, 0 ) );

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
                var timeZone = GetTimeZone(
                    $"{hourOffset:00.00} (DS active)",
                    hourOffset,
                    new DateTime( 1, 9, 26, 11, 0, 0 ),
                    new DateTime( 1, 3, 26, 11, 0, 0 ) );

                result.Add( baseDateTime, timeZone );
            }

            return result;
        }

        public static TheoryData<DateTime, TimeZoneInfo> GetCreateShouldThrowInvalidZonedDateTimeExceptionData(IFixture fixture)
        {
            var daylightOffset = 1.0;

            var timeZoneWithPositiveDaylightSaving = GetTimeZone(
                $"{daylightOffset:00.00} (DS positive)",
                daylightOffset,
                new DateTime( 1, 9, 26, 12, 0, 0 ),
                new DateTime( 1, 3, 26, 12, 0, 0 ) );

            var timeZoneWithNegativeDaylightSaving = GetTimeZone(
                $"{daylightOffset:00.00} (DS negative)",
                daylightOffset,
                new DateTime( 1, 9, 26, 12, 0, 0 ),
                new DateTime( 1, 3, 26, 12, 0, 0 ),
                daylightSavingOffsetInHours: -1.0 );

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

            var timeZoneWithPositiveDaylightSaving = GetTimeZone(
                $"{daylightOffset:00.00} (DS positive)",
                daylightOffset,
                new DateTime( 1, 9, 26, 12, 0, 0 ),
                new DateTime( 1, 3, 26, 12, 0, 0 ) );

            var timeZoneWithNegativeDaylightSaving = GetTimeZone(
                $"{daylightOffset:00.00} (DS negative)",
                daylightOffset,
                new DateTime( 1, 9, 26, 12, 0, 0 ),
                new DateTime( 1, 3, 26, 12, 0, 0 ),
                daylightSavingOffsetInHours: -1.0 );

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

            var timeZoneWithPositiveDaylightSaving = GetTimeZone(
                $"{daylightOffset:00.00} (DS positive)",
                daylightOffset,
                new DateTime( 1, 9, 26, 12, 0, 0 ),
                new DateTime( 1, 3, 26, 12, 0, 0 ) );

            var timeZoneWithNegativeDaylightSaving = GetTimeZone(
                $"{daylightOffset:00.00} (DS negative)",
                daylightOffset,
                new DateTime( 1, 9, 26, 12, 0, 0 ),
                new DateTime( 1, 3, 26, 12, 0, 0 ),
                daylightSavingOffsetInHours: -1.0 );

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

            var timeZoneWithPositiveDaylightSaving = GetTimeZone(
                $"{daylightOffset:00.00} (DS positive)",
                daylightOffset,
                new DateTime( 1, 9, 26, 12, 0, 0 ),
                new DateTime( 1, 3, 26, 12, 0, 0 ) );

            var timeZoneWithNegativeDaylightSaving = GetTimeZone(
                $"{daylightOffset:00.00} (DS negative)",
                daylightOffset,
                new DateTime( 1, 9, 26, 12, 0, 0 ),
                new DateTime( 1, 3, 26, 12, 0, 0 ),
                daylightSavingOffsetInHours: -1.0 );

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

            var tz1 = GetTimeZone(
                "03.00",
                3,
                new DateTime( 1, 3, 26, 12, 0, 0 ),
                new DateTime( 1, 9, 26, 12, 0, 0 ) );

            var tz2 = GetTimeZone(
                "-05.00",
                -5,
                new DateTime( 1, 3, 26, 12, 0, 0 ),
                new DateTime( 1, 9, 26, 12, 0, 0 ) );

            return new TheoryData<DateTime, TimeZoneInfo, string>
            {
                { dt1, tz1, "2021-02-21 12:39:47.1230234 +03:00 (Test Time Zone [03.00])" },
                { dt2, tz1, "2021-08-05 01:07:03.0679870 +04:00 (Test Time Zone [03.00])" },
                { dt1, tz2, "2021-02-21 12:39:47.1230234 -05:00 (Test Time Zone [-05.00])" },
                { dt2, tz2, "2021-08-05 01:07:03.0679870 -04:00 (Test Time Zone [-05.00])" },
            };
        }

        public static IEnumerable<object[]> GetEqualsData(IFixture fixture)
        {
            var (dt1, dt2) = (new DateTime( 2021, 8, 24, 12, 0, 0 ), new DateTime( 2021, 8, 24, 14, 0, 0 ));
            var (tz1, tz2) = (GetTimeZone( "03.00", 3 ), GetTimeZone( "05.00", 5 ));

            return new[]
            {
                new object[] { dt1, tz1, dt1, tz1, true },
                new object[] { dt1, tz1, dt1, tz2, false },
                new object[] { dt1, tz1, dt2, tz1, false },
                new object[] { dt1, tz1, dt2, tz2, false },
                new object[] { dt1, tz2, dt2, tz1, false }
            };
        }

        public static IEnumerable<object[]> GetCompareToData(IFixture fixture)
        {
            var (dt1, dt2) = (new DateTime( 2021, 8, 24, 12, 0, 0 ), new DateTime( 2021, 8, 24, 14, 0, 0 ));
            var (tz1, tz2, tz3) = (GetTimeZone( "03.00", 3 ), GetTimeZone( "05.00", 5 ), GetTimeZone( "Other 03.00", 3 ));

            return new[]
            {
                new object[] { dt1, tz1, dt1, tz1, 0 },
                new object[] { dt1, tz1, dt1, tz2, 1 },
                new object[] { dt1, tz2, dt1, tz1, -1 },
                new object[] { dt2, tz1, dt1, tz1, 1 },
                new object[] { dt1, tz1, dt2, tz1, -1 },
                new object[] { dt1, tz1, dt2, tz2, -1 },
                new object[] { dt2, tz2, dt1, tz1, 1 },
                new object[] { dt1, tz2, dt2, tz1, -1 },
                new object[] { dt2, tz1, dt1, tz2, 1 },
                new object[] { dt1, tz1, dt1, tz3, -1 },
                new object[] { dt1, tz3, dt1, tz1, 1 }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, DateTime> GetToUtcTimeZoneData(IFixture fixture)
        {
            var dt = new DateTime( 2021, 8, 24, 12, 0, 0 );

            return new TheoryData<DateTime, TimeZoneInfo, DateTime>
            {
                { dt, TimeZoneInfo.Utc, dt },
                { dt, GetTimeZone( "0", 0 ), dt },
                { dt, GetTimeZone( "1", 1 ), dt.AddHours( -1 ) },
                { dt, GetTimeZone( "10", 10 ), dt.AddHours( -10 ) },
                { dt, GetTimeZone( "-1", -1 ), dt.AddHours( 1 ) },
                { dt, GetTimeZone( "-10", -10 ), dt.AddHours( 10 ) }
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
                { dt, GetTimeZone( "0", 0 ), dt.Add( utcOffset ) },
                { dt, GetTimeZone( "1", 1 ), dt.Add( utcOffset - TimeSpan.FromHours( 1 ) ) },
                { dt, GetTimeZone( "10", 10 ), dt.Add( utcOffset - TimeSpan.FromHours( 10 ) ) },
                { dt, GetTimeZone( "-1", -1 ), dt.Add( utcOffset + TimeSpan.FromHours( 1 ) ) },
                { dt, GetTimeZone( "-10", -10 ), dt.Add( utcOffset + TimeSpan.FromHours( 10 ) ) }
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
            var (tz1, tz2) = (GetTimeZone( "3", 3 ), GetTimeZone( "5", 5 ));

            return new TheoryData<DateTime, TimeZoneInfo, TimeZoneInfo, DateTime>
            {
                { dt, tz1, tz1, dt },
                { dt, tz1, GetTimeZone( "0", 0 ), dt.Subtract( tz1.BaseUtcOffset ) },
                { dt, tz1, GetTimeZone( "1", 1 ), dt.Subtract( tz1.BaseUtcOffset - TimeSpan.FromHours( 1 ) ) },
                { dt, tz1, GetTimeZone( "10", 10 ), dt.Subtract( tz1.BaseUtcOffset - TimeSpan.FromHours( 10 ) ) },
                { dt, tz1, GetTimeZone( "-1", -1 ), dt.Subtract( tz1.BaseUtcOffset + TimeSpan.FromHours( 1 ) ) },
                { dt, tz1, GetTimeZone( "-10", -10 ), dt.Subtract( tz1.BaseUtcOffset + TimeSpan.FromHours( 10 ) ) },
                { dt, tz2, tz2, dt },
                { dt, tz2, GetTimeZone( "0", 0 ), dt.Subtract( tz2.BaseUtcOffset ) },
                { dt, tz2, GetTimeZone( "1", 1 ), dt.Subtract( tz2.BaseUtcOffset - TimeSpan.FromHours( 1 ) ) },
                { dt, tz2, GetTimeZone( "10", 10 ), dt.Subtract( tz2.BaseUtcOffset - TimeSpan.FromHours( 10 ) ) },
                { dt, tz2, GetTimeZone( "-1", -1 ), dt.Subtract( tz2.BaseUtcOffset + TimeSpan.FromHours( 1 ) ) },
                { dt, tz2, GetTimeZone( "-10", -10 ), dt.Subtract( tz2.BaseUtcOffset + TimeSpan.FromHours( 10 ) ) }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, TimeZoneInfo, DateTime> GetToTimeZoneWithInactiveTargetDaylightSavingData(
            IFixture fixture)
        {
            var dt = new DateTime( 2021, 10, 24, 12, 0, 0 );
            var (tz1, tz2) = (GetTimeZone( "3", 3 ), GetTimeZone( "5", 5 ));
            var (tStart, tEnd) = (new DateTime( 1, 3, 26, 12, 0, 0 ), new DateTime( 1, 9, 26, 12, 0, 0 ));

            return new TheoryData<DateTime, TimeZoneInfo, TimeZoneInfo, DateTime>
            {
                { dt, tz1, GetTimeZone( "0", 0, tStart, tEnd ), dt.Subtract( tz1.BaseUtcOffset ) },
                { dt, tz1, GetTimeZone( "1", 1, tStart, tEnd ), dt.Subtract( tz1.BaseUtcOffset - TimeSpan.FromHours( 1 ) ) },
                { dt, tz1, GetTimeZone( "10", 10, tStart, tEnd ), dt.Subtract( tz1.BaseUtcOffset - TimeSpan.FromHours( 10 ) ) },
                { dt, tz1, GetTimeZone( "-1", -1, tStart, tEnd ), dt.Subtract( tz1.BaseUtcOffset + TimeSpan.FromHours( 1 ) ) },
                { dt, tz1, GetTimeZone( "-10", -10, tStart, tEnd ), dt.Subtract( tz1.BaseUtcOffset + TimeSpan.FromHours( 10 ) ) },
                { dt, tz2, GetTimeZone( "0", 0, tStart, tEnd ), dt.Subtract( tz2.BaseUtcOffset ) },
                { dt, tz2, GetTimeZone( "1", 1, tStart, tEnd ), dt.Subtract( tz2.BaseUtcOffset - TimeSpan.FromHours( 1 ) ) },
                { dt, tz2, GetTimeZone( "10", 10, tStart, tEnd ), dt.Subtract( tz2.BaseUtcOffset - TimeSpan.FromHours( 10 ) ) },
                { dt, tz2, GetTimeZone( "-1", -1, tStart, tEnd ), dt.Subtract( tz2.BaseUtcOffset + TimeSpan.FromHours( 1 ) ) },
                { dt, tz2, GetTimeZone( "-10", -10, tStart, tEnd ), dt.Subtract( tz2.BaseUtcOffset + TimeSpan.FromHours( 10 ) ) }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, TimeZoneInfo, DateTime> GetToTimeZoneWithActiveTargetDaylightSavingData(
            IFixture fixture)
        {
            var dt = new DateTime( 2021, 8, 24, 12, 0, 0 );
            var (tz1, tz2) = (GetTimeZone( "3", 3 ), GetTimeZone( "5", 5 ));
            var (tStart, tEnd) = (new DateTime( 1, 3, 26, 12, 0, 0 ), new DateTime( 1, 9, 26, 12, 0, 0 ));

            return new TheoryData<DateTime, TimeZoneInfo, TimeZoneInfo, DateTime>
            {
                { dt, tz1, GetTimeZone( "0", 0, tStart, tEnd ), dt.Subtract( tz1.BaseUtcOffset - TimeSpan.FromHours( 1 ) ) },
                { dt, tz1, GetTimeZone( "1", 1, tStart, tEnd ), dt.Subtract( tz1.BaseUtcOffset - TimeSpan.FromHours( 2 ) ) },
                { dt, tz1, GetTimeZone( "10", 10, tStart, tEnd ), dt.Subtract( tz1.BaseUtcOffset - TimeSpan.FromHours( 11 ) ) },
                { dt, tz1, GetTimeZone( "-1", -1, tStart, tEnd ), dt.Subtract( tz1.BaseUtcOffset ) },
                { dt, tz1, GetTimeZone( "-10", -10, tStart, tEnd ), dt.Subtract( tz1.BaseUtcOffset + TimeSpan.FromHours( 9 ) ) },
                { dt, tz2, GetTimeZone( "0", 0, tStart, tEnd ), dt.Subtract( tz2.BaseUtcOffset - TimeSpan.FromHours( 1 ) ) },
                { dt, tz2, GetTimeZone( "1", 1, tStart, tEnd ), dt.Subtract( tz2.BaseUtcOffset - TimeSpan.FromHours( 2 ) ) },
                { dt, tz2, GetTimeZone( "10", 10, tStart, tEnd ), dt.Subtract( tz2.BaseUtcOffset - TimeSpan.FromHours( 11 ) ) },
                { dt, tz2, GetTimeZone( "-1", -1, tStart, tEnd ), dt.Subtract( tz2.BaseUtcOffset ) },
                { dt, tz2, GetTimeZone( "-10", -10, tStart, tEnd ), dt.Subtract( tz2.BaseUtcOffset + TimeSpan.FromHours( 9 ) ) }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, TimeZoneInfo, DateTime, bool> GetToTimeZoneWithDateTimeOnTheEdgeOfInvalidityData(
            IFixture fixture)
        {
            var (tz1, tz2) = (GetTimeZone( "3", 3 ), GetTimeZone( "5", 5 ));

            var daylightOffset = 1.0;

            var timeZoneWithPositiveDaylightSaving = GetTimeZone(
                $"{daylightOffset:00.00} (DS positive)",
                daylightOffset,
                new DateTime( 1, 9, 26, 12, 0, 0 ),
                new DateTime( 1, 3, 26, 12, 0, 0 ) );

            var timeZoneWithNegativeDaylightSaving = GetTimeZone(
                $"{daylightOffset:00.00} (DS negative)",
                daylightOffset,
                new DateTime( 1, 9, 26, 12, 0, 0 ),
                new DateTime( 1, 3, 26, 12, 0, 0 ),
                daylightSavingOffsetInHours: -1.0 );

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
            var (tz1, tz2) = (GetTimeZone( "3", 3 ), GetTimeZone( "5", 5 ));

            var daylightOffset = 1.0;

            var timeZoneWithPositiveDaylightSaving = GetTimeZone(
                $"{daylightOffset:00.00} (DS positive)",
                daylightOffset,
                new DateTime( 1, 9, 26, 12, 0, 0 ),
                new DateTime( 1, 3, 26, 12, 0, 0 ) );

            var timeZoneWithNegativeDaylightSaving = GetTimeZone(
                $"{daylightOffset:00.00} (DS negative)",
                daylightOffset,
                new DateTime( 1, 9, 26, 12, 0, 0 ),
                new DateTime( 1, 3, 26, 12, 0, 0 ),
                daylightSavingOffsetInHours: -1.0 );

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
            var (tz1, tz2) = (GetTimeZone( "3", 3 ), GetTimeZone( "5", 5 ));

            var daylightOffset = 1.0;

            var timeZoneWithPositiveDaylightSaving = GetTimeZone(
                $"{daylightOffset:00.00} (DS positive)",
                daylightOffset,
                new DateTime( 1, 9, 26, 12, 0, 0 ),
                new DateTime( 1, 3, 26, 12, 0, 0 ) );

            var timeZoneWithNegativeDaylightSaving = GetTimeZone(
                $"{daylightOffset:00.00} (DS negative)",
                daylightOffset,
                new DateTime( 1, 9, 26, 12, 0, 0 ),
                new DateTime( 1, 3, 26, 12, 0, 0 ),
                daylightSavingOffsetInHours: -1.0 );

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
                    GetTimeZone( "1", 1 ),
                    1,
                    new DateTime( 2021, 8, 12, 12, 30, 17, 123 ).AddTicks( 100 )
                },
                {
                    new DateTime( 2021, 8, 12, 12, 30, 17, 123 ).AddTicks( 99 ),
                    GetTimeZone( "1", 1 ),
                    -1,
                    new DateTime( 2021, 8, 12, 12, 30, 17, 123 ).AddTicks( 98 )
                },
                {
                    new DateTime( 2021, 8, 12, 12, 30, 17, 123 ).AddTicks( 99 ),
                    GetTimeZone( "2", 2 ),
                    900610010001,
                    new DateTime( 2021, 8, 13, 13, 31, 18, 124 ).AddTicks( 100 )
                },
                {
                    new DateTime( 2021, 8, 12, 12, 30, 17, 123 ).AddTicks( 99 ),
                    GetTimeZone( "2", 2 ),
                    -900610010001,
                    new DateTime( 2021, 8, 11, 11, 29, 16, 122 ).AddTicks( 98 )
                },
                {
                    new DateTime( 2021, 8, 12, 12, 30, 17, 123 ).AddTicks( 99 ),
                    GetTimeZone( "3 (+DS)", 3, tStart, tEnd ),
                    900610010002,
                    new DateTime( 2021, 8, 13, 13, 31, 18, 124 ).AddTicks( 101 )
                },
                {
                    new DateTime( 2021, 8, 12, 12, 30, 17, 123 ).AddTicks( 99 ),
                    GetTimeZone( "3 (+DS)", 3, tStart, tEnd ),
                    -900610010002,
                    new DateTime( 2021, 8, 11, 11, 29, 16, 122 ).AddTicks( 97 )
                },
                {
                    new DateTime( 2021, 8, 12, 12, 30, 17, 123 ).AddTicks( 99 ),
                    GetTimeZone( "3 (-DS)", 3, tStart, tEnd, daylightSavingOffsetInHours: -1 ),
                    900610010002,
                    new DateTime( 2021, 8, 13, 13, 31, 18, 124 ).AddTicks( 101 )
                },
                {
                    new DateTime( 2021, 8, 12, 12, 30, 17, 123 ).AddTicks( 99 ),
                    GetTimeZone( "3 (-DS)", 3, tStart, tEnd, daylightSavingOffsetInHours: -1 ),
                    -900610010002,
                    new DateTime( 2021, 8, 11, 11, 29, 16, 122 ).AddTicks( 97 )
                },
                {
                    new DateTime( 2021, 8, 12, 12, 30, 17, 123 ).AddTicks( 99 ),
                    GetTimeZone( "4 (+DS)", 4, tEnd, tStart ),
                    900610010002,
                    new DateTime( 2021, 8, 13, 13, 31, 18, 124 ).AddTicks( 101 )
                },
                {
                    new DateTime( 2021, 8, 12, 12, 30, 17, 123 ).AddTicks( 99 ),
                    GetTimeZone( "4 (+DS)", 4, tEnd, tStart ),
                    -900610010002,
                    new DateTime( 2021, 8, 11, 11, 29, 16, 122 ).AddTicks( 97 )
                },
                {
                    new DateTime( 2021, 8, 12, 12, 30, 17, 123 ).AddTicks( 99 ),
                    GetTimeZone( "4 (-DS)", 4, tEnd, tStart, daylightSavingOffsetInHours: -1 ),
                    900610010002,
                    new DateTime( 2021, 8, 13, 13, 31, 18, 124 ).AddTicks( 101 )
                },
                {
                    new DateTime( 2021, 8, 12, 12, 30, 17, 123 ).AddTicks( 99 ),
                    GetTimeZone( "4 (-DS)", 4, tEnd, tStart, daylightSavingOffsetInHours: -1 ),
                    -900610010002,
                    new DateTime( 2021, 8, 11, 11, 29, 16, 122 ).AddTicks( 97 )
                },
                {
                    new DateTime( 2021, 8, 12, 12, 30, 17, 123 ).AddTicks( 99 ),
                    GetTimeZone( "5 (+DS)", 5, tStart, tEnd ),
                    315360000000000,
                    new DateTime( 2022, 8, 12, 12, 30, 17, 123 ).AddTicks( 99 )
                },
                {
                    new DateTime( 2021, 8, 12, 12, 30, 17, 123 ).AddTicks( 99 ),
                    GetTimeZone( "5 (+DS)", 5, tStart, tEnd ),
                    -315360000000000,
                    new DateTime( 2020, 8, 12, 12, 30, 17, 123 ).AddTicks( 99 )
                },
                {
                    new DateTime( 2021, 8, 12, 12, 30, 17, 123 ).AddTicks( 99 ),
                    GetTimeZone( "5 (+DS)", 5, tEnd, tStart ),
                    315360000000000,
                    new DateTime( 2022, 8, 12, 12, 30, 17, 123 ).AddTicks( 99 )
                },
                {
                    new DateTime( 2021, 8, 12, 12, 30, 17, 123 ).AddTicks( 99 ),
                    GetTimeZone( "5 (+DS)", 5, tEnd, tStart ),
                    -315360000000000,
                    new DateTime( 2020, 8, 12, 12, 30, 17, 123 ).AddTicks( 99 )
                },
                {
                    new DateTime( 2021, 3, 25, 11, 30, 10, 100 ),
                    GetTimeZone( "2 (+DS)", 2, tStart, tEnd ),
                    881898999999,
                    new DateTime( 2021, 3, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
                },
                {
                    new DateTime( 2021, 3, 27, 13, 29, 49, 899 ).AddTicks( 9999 ),
                    GetTimeZone( "2 (+DS)", 2, tStart, tEnd ),
                    -881898999999,
                    new DateTime( 2021, 3, 26, 13, 0, 0 )
                },
                {
                    new DateTime( 2021, 9, 25, 10, 30, 10, 100 ),
                    GetTimeZone( "2 (+DS)", 2, tStart, tEnd ),
                    881898999999,
                    new DateTime( 2021, 9, 26, 10, 59, 59, 999 ).AddTicks( 9999 )
                },
                {
                    new DateTime( 2021, 9, 27, 12, 29, 49, 899 ).AddTicks( 9999 ),
                    GetTimeZone( "2 (+DS)", 2, tStart, tEnd ),
                    -881898999999,
                    new DateTime( 2021, 9, 26, 12, 0, 0 )
                },
                {
                    new DateTime( 2021, 9, 25, 11, 30, 10, 100 ),
                    GetTimeZone( "2 (+DS)", 2, tEnd, tStart ),
                    881898999999,
                    new DateTime( 2021, 9, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
                },
                {
                    new DateTime( 2021, 9, 27, 13, 29, 49, 899 ).AddTicks( 9999 ),
                    GetTimeZone( "2 (+DS)", 2, tEnd, tStart ),
                    -881898999999,
                    new DateTime( 2021, 9, 26, 13, 0, 0 )
                },
                {
                    new DateTime( 2021, 3, 25, 10, 30, 10, 100 ),
                    GetTimeZone( "2 (+DS)", 2, tEnd, tStart ),
                    881898999999,
                    new DateTime( 2021, 3, 26, 10, 59, 59, 999 ).AddTicks( 9999 )
                },
                {
                    new DateTime( 2021, 3, 27, 12, 29, 49, 899 ).AddTicks( 9999 ),
                    GetTimeZone( "2 (+DS)", 2, tEnd, tStart ),
                    -881898999999,
                    new DateTime( 2021, 3, 26, 12, 0, 0 )
                },
                {
                    new DateTime( 2021, 3, 25, 10, 30, 10, 100 ),
                    GetTimeZone( "2 (-DS)", 2, tStart, tEnd, daylightSavingOffsetInHours: -1 ),
                    881898999999,
                    new DateTime( 2021, 3, 26, 10, 59, 59, 999 ).AddTicks( 9999 )
                },
                {
                    new DateTime( 2021, 3, 27, 12, 29, 49, 899 ).AddTicks( 9999 ),
                    GetTimeZone( "2 (-DS)", 2, tStart, tEnd, daylightSavingOffsetInHours: -1 ),
                    -881898999999,
                    new DateTime( 2021, 3, 26, 12, 0, 0 )
                },
                {
                    new DateTime( 2021, 9, 25, 11, 30, 10, 100 ),
                    GetTimeZone( "2 (-DS)", 2, tStart, tEnd, daylightSavingOffsetInHours: -1 ),
                    881898999999,
                    new DateTime( 2021, 9, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
                },
                {
                    new DateTime( 2021, 9, 27, 13, 29, 49, 899 ).AddTicks( 9999 ),
                    GetTimeZone( "2 (-DS)", 2, tStart, tEnd, daylightSavingOffsetInHours: -1 ),
                    -881898999999,
                    new DateTime( 2021, 9, 26, 13, 0, 0 )
                },
                {
                    new DateTime( 2021, 9, 25, 10, 30, 10, 100 ),
                    GetTimeZone( "2 (-DS)", 2, tEnd, tStart, daylightSavingOffsetInHours: -1 ),
                    881898999999,
                    new DateTime( 2021, 9, 26, 10, 59, 59, 999 ).AddTicks( 9999 )
                },
                {
                    new DateTime( 2021, 9, 27, 12, 29, 49, 899 ).AddTicks( 9999 ),
                    GetTimeZone( "2 (-DS)", 2, tEnd, tStart, daylightSavingOffsetInHours: -1 ),
                    -881898999999,
                    new DateTime( 2021, 9, 26, 12, 0, 0 )
                },
                {
                    new DateTime( 2021, 3, 25, 11, 30, 10, 100 ),
                    GetTimeZone( "2 (-DS)", 2, tEnd, tStart, daylightSavingOffsetInHours: -1 ),
                    881898999999,
                    new DateTime( 2021, 3, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
                },
                {
                    new DateTime( 2021, 3, 27, 13, 29, 49, 899 ).AddTicks( 9999 ),
                    GetTimeZone( "2 (-DS)", 2, tEnd, tStart, daylightSavingOffsetInHours: -1 ),
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
                    GetTimeZone( "1 (+DS)", 1, tStart, tEnd ),
                    1728000000000,
                    new DateTime( 2021, 3, 27, 12, 30, 10, 100 ).AddTicks( 2000 )
                },
                {
                    new DateTime( 2021, 3, 25, 11, 30, 10, 100 ).AddTicks( 2000 ),
                    GetTimeZone( "1 (+DS)", 1, tStart, tEnd ),
                    317088000000000,
                    new DateTime( 2022, 3, 27, 12, 30, 10, 100 ).AddTicks( 2000 )
                },
                {
                    new DateTime( 2021, 3, 27, 12, 30, 10, 100 ).AddTicks( 2000 ),
                    GetTimeZone( "1 (+DS)", 1, tStart, tEnd ),
                    -1728000000000,
                    new DateTime( 2021, 3, 25, 11, 30, 10, 100 ).AddTicks( 2000 )
                },
                {
                    new DateTime( 2022, 3, 27, 12, 30, 10, 100 ).AddTicks( 2000 ),
                    GetTimeZone( "1 (+DS)", 1, tStart, tEnd ),
                    -317088000000000,
                    new DateTime( 2021, 3, 25, 11, 30, 10, 100 ).AddTicks( 2000 )
                },
                {
                    new DateTime( 2021, 9, 25, 12, 30, 10, 100 ).AddTicks( 2000 ),
                    GetTimeZone( "1 (+DS)", 1, tStart, tEnd ),
                    1728000000000,
                    new DateTime( 2021, 9, 27, 11, 30, 10, 100 ).AddTicks( 2000 )
                },
                {
                    new DateTime( 2021, 9, 25, 12, 30, 10, 100 ).AddTicks( 2000 ),
                    GetTimeZone( "1 (+DS)", 1, tStart, tEnd ),
                    317088000000000,
                    new DateTime( 2022, 9, 27, 11, 30, 10, 100 ).AddTicks( 2000 )
                },
                {
                    new DateTime( 2021, 9, 27, 11, 30, 10, 100 ).AddTicks( 2000 ),
                    GetTimeZone( "1 (+DS)", 1, tStart, tEnd ),
                    -1728000000000,
                    new DateTime( 2021, 9, 25, 12, 30, 10, 100 ).AddTicks( 2000 )
                },
                {
                    new DateTime( 2022, 9, 27, 11, 30, 10, 100 ).AddTicks( 2000 ),
                    GetTimeZone( "1 (+DS)", 1, tStart, tEnd ),
                    -317088000000000,
                    new DateTime( 2021, 9, 25, 12, 30, 10, 100 ).AddTicks( 2000 )
                },
                {
                    new DateTime( 2021, 3, 25, 12, 30, 10, 100 ).AddTicks( 2000 ),
                    GetTimeZone( "1 (-DS)", 1, tStart, tEnd, daylightSavingOffsetInHours: -1 ),
                    1728000000000,
                    new DateTime( 2021, 3, 27, 11, 30, 10, 100 ).AddTicks( 2000 )
                },
                {
                    new DateTime( 2021, 3, 25, 12, 30, 10, 100 ).AddTicks( 2000 ),
                    GetTimeZone( "1 (-DS)", 1, tStart, tEnd, daylightSavingOffsetInHours: -1 ),
                    317088000000000,
                    new DateTime( 2022, 3, 27, 11, 30, 10, 100 ).AddTicks( 2000 )
                },
                {
                    new DateTime( 2021, 3, 27, 11, 30, 10, 100 ).AddTicks( 2000 ),
                    GetTimeZone( "1 (-DS)", 1, tStart, tEnd, daylightSavingOffsetInHours: -1 ),
                    -1728000000000,
                    new DateTime( 2021, 3, 25, 12, 30, 10, 100 ).AddTicks( 2000 )
                },
                {
                    new DateTime( 2022, 3, 27, 11, 30, 10, 100 ).AddTicks( 2000 ),
                    GetTimeZone( "1 (-DS)", 1, tStart, tEnd, daylightSavingOffsetInHours: -1 ),
                    -317088000000000,
                    new DateTime( 2021, 3, 25, 12, 30, 10, 100 ).AddTicks( 2000 )
                },
                {
                    new DateTime( 2021, 9, 25, 11, 30, 10, 100 ).AddTicks( 2000 ),
                    GetTimeZone( "1 (-DS)", 1, tStart, tEnd, daylightSavingOffsetInHours: -1 ),
                    1728000000000,
                    new DateTime( 2021, 9, 27, 12, 30, 10, 100 ).AddTicks( 2000 )
                },
                {
                    new DateTime( 2021, 9, 25, 11, 30, 10, 100 ).AddTicks( 2000 ),
                    GetTimeZone( "1 (-DS)", 1, tStart, tEnd, daylightSavingOffsetInHours: -1 ),
                    317088000000000,
                    new DateTime( 2022, 9, 27, 12, 30, 10, 100 ).AddTicks( 2000 )
                },
                {
                    new DateTime( 2021, 9, 27, 12, 30, 10, 100 ).AddTicks( 2000 ),
                    GetTimeZone( "1 (-DS)", 1, tStart, tEnd, daylightSavingOffsetInHours: -1 ),
                    -1728000000000,
                    new DateTime( 2021, 9, 25, 11, 30, 10, 100 ).AddTicks( 2000 )
                },
                {
                    new DateTime( 2022, 9, 27, 12, 30, 10, 100 ).AddTicks( 2000 ),
                    GetTimeZone( "1 (-DS)", 1, tStart, tEnd, daylightSavingOffsetInHours: -1 ),
                    -317088000000000,
                    new DateTime( 2021, 9, 25, 11, 30, 10, 100 ).AddTicks( 2000 )
                },
                {
                    new DateTime( 2021, 3, 26, 11, 59, 59, 999 ).AddTicks( 9999 ),
                    GetTimeZone( "1 (+DS)", 1, tStart, tEnd ),
                    1,
                    new DateTime( 2021, 3, 26, 13, 0, 0 )
                },
                {
                    new DateTime( 2021, 3, 26, 13, 0, 0 ),
                    GetTimeZone( "1 (+DS)", 1, tStart, tEnd ),
                    -1,
                    new DateTime( 2021, 3, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
                },
                {
                    new DateTime( 2021, 9, 26, 10, 59, 59, 999 ).AddTicks( 9999 ),
                    GetTimeZone( "1 (+DS)", 1, tStart, tEnd ),
                    72000000001,
                    new DateTime( 2021, 9, 26, 12, 0, 0 )
                },
                {
                    new DateTime( 2021, 9, 26, 12, 0, 0 ),
                    GetTimeZone( "1 (+DS)", 1, tStart, tEnd ),
                    -72000000001,
                    new DateTime( 2021, 9, 26, 10, 59, 59, 999 ).AddTicks( 9999 )
                },
                {
                    new DateTime( 2021, 3, 26, 10, 59, 59, 999 ).AddTicks( 9999 ),
                    GetTimeZone( "1 (-DS)", 1, tStart, tEnd, daylightSavingOffsetInHours: -1 ),
                    72000000001,
                    new DateTime( 2021, 3, 26, 12, 0, 0 )
                },
                {
                    new DateTime( 2021, 3, 26, 12, 0, 0 ),
                    GetTimeZone( "1 (-DS)", 1, tStart, tEnd, daylightSavingOffsetInHours: -1 ),
                    -72000000001,
                    new DateTime( 2021, 3, 26, 10, 59, 59, 999 ).AddTicks( 9999 )
                },
                {
                    new DateTime( 2021, 9, 26, 11, 59, 59, 999 ).AddTicks( 9999 ),
                    GetTimeZone( "1 (-DS)", 1, tStart, tEnd, daylightSavingOffsetInHours: -1 ),
                    1,
                    new DateTime( 2021, 9, 26, 13, 0, 0 )
                },
                {
                    new DateTime( 2021, 9, 26, 13, 0, 0 ),
                    GetTimeZone( "1 (-DS)", 1, tStart, tEnd, daylightSavingOffsetInHours: -1 ),
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
                    GetTimeZone( "1 (+DS)", 1, tStart, tEnd ),
                    1,
                    new DateTime( 2021, 9, 26, 11, 0, 0 ),
                    true
                },
                {
                    new DateTime( 2021, 9, 26, 10, 59, 59, 999 ).AddTicks( 9999 ),
                    GetTimeZone( "1 (+DS)", 1, tStart, tEnd ),
                    18000000001,
                    new DateTime( 2021, 9, 26, 11, 30, 0 ),
                    true
                },
                {
                    new DateTime( 2021, 9, 26, 10, 59, 59, 999 ).AddTicks( 9999 ),
                    GetTimeZone( "1 (+DS)", 1, tStart, tEnd ),
                    36000000000,
                    new DateTime( 2021, 9, 26, 11, 59, 59, 999 ).AddTicks( 9999 ),
                    true
                },
                {
                    new DateTime( 2021, 9, 26, 10, 59, 59, 999 ).AddTicks( 9999 ),
                    GetTimeZone( "1 (+DS)", 1, tStart, tEnd ),
                    36000000001,
                    new DateTime( 2021, 9, 26, 11, 0, 0 ),
                    false
                },
                {
                    new DateTime( 2021, 9, 26, 10, 59, 59, 999 ).AddTicks( 9999 ),
                    GetTimeZone( "1 (+DS)", 1, tStart, tEnd ),
                    54000000001,
                    new DateTime( 2021, 9, 26, 11, 30, 0 ),
                    false
                },
                {
                    new DateTime( 2021, 9, 26, 10, 59, 59, 999 ).AddTicks( 9999 ),
                    GetTimeZone( "1 (+DS)", 1, tStart, tEnd ),
                    72000000000,
                    new DateTime( 2021, 9, 26, 11, 59, 59, 999 ).AddTicks( 9999 ),
                    false
                },
                {
                    new DateTime( 2021, 3, 26, 10, 59, 59, 999 ).AddTicks( 9999 ),
                    GetTimeZone( "1 (-DS)", 1, tStart, tEnd, daylightSavingOffsetInHours: -1 ),
                    1,
                    new DateTime( 2021, 3, 26, 11, 0, 0 ),
                    false
                },
                {
                    new DateTime( 2021, 3, 26, 10, 59, 59, 999 ).AddTicks( 9999 ),
                    GetTimeZone( "1 (-DS)", 1, tStart, tEnd, daylightSavingOffsetInHours: -1 ),
                    18000000001,
                    new DateTime( 2021, 3, 26, 11, 30, 0 ),
                    false
                },
                {
                    new DateTime( 2021, 3, 26, 10, 59, 59, 999 ).AddTicks( 9999 ),
                    GetTimeZone( "1 (-DS)", 1, tStart, tEnd, daylightSavingOffsetInHours: -1 ),
                    36000000000,
                    new DateTime( 2021, 3, 26, 11, 59, 59, 999 ).AddTicks( 9999 ),
                    false
                },
                {
                    new DateTime( 2021, 3, 26, 10, 59, 59, 999 ).AddTicks( 9999 ),
                    GetTimeZone( "1 (-DS)", 1, tStart, tEnd, daylightSavingOffsetInHours: -1 ),
                    36000000001,
                    new DateTime( 2021, 3, 26, 11, 0, 0 ),
                    true
                },
                {
                    new DateTime( 2021, 3, 26, 10, 59, 59, 999 ).AddTicks( 9999 ),
                    GetTimeZone( "1 (-DS)", 1, tStart, tEnd, daylightSavingOffsetInHours: -1 ),
                    54000000001,
                    new DateTime( 2021, 3, 26, 11, 30, 0 ),
                    true
                },
                {
                    new DateTime( 2021, 3, 26, 10, 59, 59, 999 ).AddTicks( 9999 ),
                    GetTimeZone( "1 (-DS)", 1, tStart, tEnd, daylightSavingOffsetInHours: -1 ),
                    72000000000,
                    new DateTime( 2021, 3, 26, 11, 59, 59, 999 ).AddTicks( 9999 ),
                    true
                }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, Core.Chrono.Period, DateTime> GetAddWithPeriodData(IFixture fixture)
        {
            var dsTimeZone = GetTimeZone(
                "1 (+DS)",
                1,
                new DateTime( 1, 8, 26, 12, 0, 0 ),
                new DateTime( 1, 10, 26, 12, 0, 0 ) );

            return new TheoryData<DateTime, TimeZoneInfo, Core.Chrono.Period, DateTime>
            {
                {
                    new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                    GetTimeZone( "1", 1 ),
                    Core.Chrono.Period.Empty,
                    new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 )
                },
                {
                    new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                    GetTimeZone( "1", 1 ),
                    new Core.Chrono.Period( 1, 2, 3, 4, 5 ),
                    new DateTime( 2021, 8, 26, 13, 32, 43, 504 ).AddTicks( 6006 )
                },
                {
                    new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                    GetTimeZone( "1", 1 ),
                    new Core.Chrono.Period( -1, -2, -3, -4, -5 ),
                    new DateTime( 2021, 8, 26, 11, 28, 37, 496 ).AddTicks( 5996 )
                },
                {
                    new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                    GetTimeZone( "1", 1 ),
                    new Core.Chrono.Period( 1, 2, 3, 4 ),
                    new DateTime( 2022, 11, 20, 12, 30, 40, 500 ).AddTicks( 6001 )
                },
                {
                    new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                    GetTimeZone( "1", 1 ),
                    new Core.Chrono.Period( -1, -2, -3, -4 ),
                    new DateTime( 2020, 6, 1, 12, 30, 40, 500 ).AddTicks( 6001 )
                },
                {
                    new DateTime( 2021, 3, 31, 12, 30, 40, 500 ).AddTicks( 6001 ),
                    GetTimeZone( "1", 1 ),
                    Core.Chrono.Period.FromMonths( 1 ),
                    new DateTime( 2021, 4, 30, 12, 30, 40, 500 ).AddTicks( 6001 )
                },
                {
                    new DateTime( 2021, 3, 31, 12, 30, 40, 500 ).AddTicks( 6001 ),
                    GetTimeZone( "1", 1 ),
                    Core.Chrono.Period.FromMonths( -1 ),
                    new DateTime( 2021, 2, 28, 12, 30, 40, 500 ).AddTicks( 6001 )
                },
                {
                    new DateTime( 2021, 3, 31, 12, 30, 40, 500 ).AddTicks( 6001 ),
                    GetTimeZone( "1", 1 ),
                    Core.Chrono.Period.FromYears( -1 ).SubtractMonths( 1 ),
                    new DateTime( 2020, 2, 29, 12, 30, 40, 500 ).AddTicks( 6001 )
                },
                {
                    new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                    GetTimeZone( "1", 1 ),
                    new Core.Chrono.Period( 1, 2, 3, 4, 5, 6, 7, 8, 9 ),
                    new DateTime( 2022, 11, 20, 17, 36, 47, 508 ).AddTicks( 6010 )
                },
                {
                    new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                    GetTimeZone( "1", 1 ),
                    new Core.Chrono.Period( 1, -2, 3, -4, 5, -6, 7, -8, 9 ),
                    new DateTime( 2022, 7, 13, 17, 24, 47, 492 ).AddTicks( 6010 )
                },
                {
                    new DateTime( 2021, 7, 25, 11, 59, 59, 999 ).AddTicks( 9998 ),
                    dsTimeZone,
                    Core.Chrono.Period.FromMonths( 1 ).AddDays( 1 ).AddTicks( 1 ),
                    new DateTime( 2021, 8, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
                },
                {
                    new DateTime( 2021, 8, 26, 11, 0, 0 ),
                    dsTimeZone,
                    Core.Chrono.Period.FromHours( 2 ),
                    new DateTime( 2021, 8, 26, 13, 0, 0 )
                },
                {
                    new DateTime( 2021, 9, 27, 13, 0, 0 ).AddTicks( 1 ),
                    dsTimeZone,
                    Core.Chrono.Period.FromMonths( -1 ).SubtractDays( 1 ).SubtractTicks( 1 ),
                    new DateTime( 2021, 8, 26, 13, 0, 0 )
                },
                {
                    new DateTime( 2021, 8, 26, 13, 0, 0 ),
                    dsTimeZone,
                    Core.Chrono.Period.FromHours( -1 ).SubtractTicks( 1 ),
                    new DateTime( 2021, 8, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
                },
                {
                    new DateTime( 2021, 9, 25, 10, 59, 59, 999 ).AddTicks( 9998 ),
                    dsTimeZone,
                    Core.Chrono.Period.FromMonths( 1 ).AddDays( 1 ).AddTicks( 1 ),
                    new DateTime( 2021, 10, 26, 10, 59, 59, 999 ).AddTicks( 9999 )
                },
                {
                    new DateTime( 2021, 10, 26, 10, 0, 0 ),
                    dsTimeZone,
                    Core.Chrono.Period.FromHours( 2 ),
                    new DateTime( 2021, 10, 26, 12, 0, 0 )
                },
                {
                    new DateTime( 2021, 11, 27, 12, 0, 0 ).AddTicks( 1 ),
                    dsTimeZone,
                    Core.Chrono.Period.FromMonths( -1 ).SubtractDays( 1 ).SubtractTicks( 1 ),
                    new DateTime( 2021, 10, 26, 12, 0, 0 )
                },
                {
                    new DateTime( 2021, 10, 26, 12, 0, 0 ),
                    dsTimeZone,
                    Core.Chrono.Period.FromHours( -1 ).SubtractTicks( 1 ),
                    new DateTime( 2021, 10, 26, 10, 59, 59, 999 ).AddTicks( 9999 )
                }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, bool, Core.Chrono.Period, DateTime> GetAddWithPeriodAndAmbiguityData(
            IFixture fixture)
        {
            var timeZone = GetTimeZone(
                "1 (+DS)",
                1,
                new DateTime( 1, 8, 26, 12, 0, 0 ),
                new DateTime( 1, 10, 26, 12, 0, 0 ) );

            return new TheoryData<DateTime, TimeZoneInfo, bool, Core.Chrono.Period, DateTime>
            {
                {
                    new DateTime( 2021, 10, 26, 11, 0, 0 ),
                    timeZone,
                    false,
                    Core.Chrono.Period.Empty,
                    new DateTime( 2021, 10, 26, 11, 0, 0 )
                },
                {
                    new DateTime( 2021, 10, 26, 11, 0, 0 ),
                    timeZone,
                    true,
                    Core.Chrono.Period.Empty,
                    new DateTime( 2021, 10, 26, 11, 0, 0 )
                },
                {
                    new DateTime( 2022, 11, 27, 16, 0, 0 ),
                    timeZone,
                    false,
                    Core.Chrono.Period.FromYears( -1 ).SubtractMonths( 1 ).SubtractDays( 1 ).SubtractHours( 5 ),
                    new DateTime( 2021, 10, 26, 11, 0, 0 )
                },
                {
                    new DateTime( 2020, 9, 25, 6, 0, 0 ),
                    timeZone,
                    true,
                    Core.Chrono.Period.FromYears( 1 ).AddMonths( 1 ).AddDays( 1 ).AddHours( 5 ),
                    new DateTime( 2021, 10, 26, 11, 0, 0 )
                },
                {
                    new DateTime( 2021, 10, 26, 11, 59, 59, 999 ).AddTicks( 9999 ),
                    timeZone,
                    false,
                    Core.Chrono.Period.Empty,
                    new DateTime( 2021, 10, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
                },
                {
                    new DateTime( 2021, 10, 26, 11, 59, 59, 999 ).AddTicks( 9999 ),
                    timeZone,
                    true,
                    Core.Chrono.Period.Empty,
                    new DateTime( 2021, 10, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
                },
                {
                    new DateTime( 2022, 11, 27, 16, 0, 0 ),
                    timeZone,
                    false,
                    Core.Chrono.Period.FromYears( -1 ).SubtractMonths( 1 ).SubtractDays( 1 ).SubtractHours( 4 ).SubtractTicks( 1 ),
                    new DateTime( 2021, 10, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
                },
                {
                    new DateTime( 2020, 9, 25, 6, 0, 0 ),
                    timeZone,
                    true,
                    Core.Chrono.Period.FromYears( 1 ).AddMonths( 1 ).AddDays( 1 ).AddHours( 6 ).SubtractTicks( 1 ),
                    new DateTime( 2021, 10, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
                }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, Core.Chrono.Period> GetAddWithPeriodThrowData(IFixture fixture)
        {
            var timeZoneWithInvalidity = GetTimeZone(
                "1 (+DS)",
                1,
                new DateTime( 1, 8, 26, 12, 0, 0 ),
                new DateTime( 1, 10, 26, 12, 0, 0 ) );

            return new TheoryData<DateTime, TimeZoneInfo, Core.Chrono.Period>
            {
                {
                    new DateTime( 2020, 7, 20, 11, 0, 0 ),
                    timeZoneWithInvalidity,
                    Core.Chrono.Period.FromYears( 1 ).AddMonths( 1 ).AddDays( 6 ).AddHours( 1 )
                },
                {
                    new DateTime( 2020, 7, 20, 11, 0, 0 ),
                    timeZoneWithInvalidity,
                    Core.Chrono.Period.FromYears( 1 ).AddMonths( 1 ).AddDays( 6 ).AddHours( 2 ).SubtractTicks( 1 )
                },
                {
                    new DateTime( 2022, 9, 30, 14, 0, 0 ),
                    timeZoneWithInvalidity,
                    Core.Chrono.Period.FromYears( -1 ).SubtractMonths( 1 ).SubtractDays( 4 ).SubtractHours( 2 )
                },
                {
                    new DateTime( 2022, 9, 30, 14, 0, 0 ),
                    timeZoneWithInvalidity,
                    Core.Chrono.Period.FromYears( -1 ).SubtractMonths( 1 ).SubtractDays( 4 ).SubtractHours( 1 ).SubtractTicks( 1 )
                }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, int, DateTime> GetSetYearData(IFixture fixture)
        {
            var simpleTimeZone = GetTimeZone( "1", 1 );

            var timeZoneWithInvalidity = GetTimeZone(
                "1 (+DS)",
                1,
                DateTime.MinValue,
                new DateTime( 2020, 1, 1 ),
                new DateTime( 1, 8, 26, 12, 0, 0 ),
                new DateTime( 1, 10, 26, 12, 0, 0 ) );

            var timeZoneWithYearOverlapInvalidity = GetTimeZone(
                "1 (+DS)",
                1,
                DateTime.MinValue,
                new DateTime( 2020, 1, 1 ),
                new DateTime( 1, 12, 31, 23, 30, 0 ),
                new DateTime( 1, 10, 26, 12, 0, 0 ) );

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
            var timeZone = GetTimeZone(
                "1 (+DS)",
                1,
                new DateTime( 1, 8, 26, 12, 0, 0 ),
                new DateTime( 1, 10, 26, 12, 0, 0 ) );

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
                { fixture.Create<DateTime>(), GetTimeZone( "1", 1 ), DateTime.MinValue.Year - 1 },
                { fixture.Create<DateTime>(), GetTimeZone( "1", 1 ), DateTime.MaxValue.Year + 1 }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, IsoMonthOfYear, DateTime> GetSetMonthData(IFixture fixture)
        {
            var simpleTimeZone = GetTimeZone( "1", 1 );

            var timeZoneWithInvalidity = GetTimeZone(
                "1 (+DS)",
                1,
                new DateTime( 1, 4, 26, 12, 0, 0 ),
                new DateTime( 1, 10, 26, 12, 0, 0 ) );

            var timeZoneWithMonthOverlapInvalidity = GetTimeZone(
                "1 (+DS)",
                1,
                new DateTime( 1, 4, 30, 23, 30, 0 ),
                new DateTime( 1, 10, 26, 12, 0, 0 ) );

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
            var timeZone = GetTimeZone(
                "1 (+DS)",
                1,
                new DateTime( 1, 8, 26, 12, 0, 0 ),
                new DateTime( 1, 10, 26, 12, 0, 0 ) );

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
            var simpleTimeZone = GetTimeZone( "1", 1 );

            var timeZoneWithInvalidity = GetTimeZone(
                "1 (+DS)",
                1,
                new DateTime( 1, 8, 16, 12, 0, 0 ),
                new DateTime( 1, 10, 26, 12, 0, 0 ) );

            var timeZoneWithDayOverlapInvalidity = GetTimeZone(
                "1 (+DS)",
                1,
                new DateTime( 1, 8, 15, 23, 30, 0 ),
                new DateTime( 1, 10, 26, 12, 0, 0 ) );

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
            var timeZone = GetTimeZone(
                "1 (+DS)",
                1,
                new DateTime( 1, 8, 26, 12, 0, 0 ),
                new DateTime( 1, 10, 26, 12, 0, 0 ) );

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
                { new DateTime( 2021, 1, 1 ), GetTimeZone( "1", 1 ), 0 },
                { new DateTime( 2021, 2, 1 ), GetTimeZone( "1", 1 ), 0 },
                { new DateTime( 2021, 3, 1 ), GetTimeZone( "1", 1 ), 0 },
                { new DateTime( 2021, 4, 1 ), GetTimeZone( "1", 1 ), 0 },
                { new DateTime( 2021, 5, 1 ), GetTimeZone( "1", 1 ), 0 },
                { new DateTime( 2021, 6, 1 ), GetTimeZone( "1", 1 ), 0 },
                { new DateTime( 2021, 7, 1 ), GetTimeZone( "1", 1 ), 0 },
                { new DateTime( 2021, 8, 1 ), GetTimeZone( "1", 1 ), 0 },
                { new DateTime( 2021, 9, 1 ), GetTimeZone( "1", 1 ), 0 },
                { new DateTime( 2021, 10, 1 ), GetTimeZone( "1", 1 ), 0 },
                { new DateTime( 2021, 11, 1 ), GetTimeZone( "1", 1 ), 0 },
                { new DateTime( 2021, 12, 1 ), GetTimeZone( "1", 1 ), 0 },
                { new DateTime( 2021, 1, 1 ), GetTimeZone( "1", 1 ), Constants.DaysInJanuary + 1 },
                { new DateTime( 2021, 2, 1 ), GetTimeZone( "1", 1 ), Constants.DaysInFebruary + 1 },
                { new DateTime( 2020, 2, 1 ), GetTimeZone( "1", 1 ), Constants.DaysInLeapFebruary + 1 },
                { new DateTime( 2021, 3, 1 ), GetTimeZone( "1", 1 ), Constants.DaysInMarch + 1 },
                { new DateTime( 2021, 4, 1 ), GetTimeZone( "1", 1 ), Constants.DaysInApril + 1 },
                { new DateTime( 2021, 5, 1 ), GetTimeZone( "1", 1 ), Constants.DaysInMay + 1 },
                { new DateTime( 2021, 6, 1 ), GetTimeZone( "1", 1 ), Constants.DaysInJune + 1 },
                { new DateTime( 2021, 7, 1 ), GetTimeZone( "1", 1 ), Constants.DaysInJuly + 1 },
                { new DateTime( 2021, 8, 1 ), GetTimeZone( "1", 1 ), Constants.DaysInAugust + 1 },
                { new DateTime( 2021, 9, 1 ), GetTimeZone( "1", 1 ), Constants.DaysInSeptember + 1 },
                { new DateTime( 2021, 10, 1 ), GetTimeZone( "1", 1 ), Constants.DaysInOctober + 1 },
                { new DateTime( 2021, 11, 1 ), GetTimeZone( "1", 1 ), Constants.DaysInNovember + 1 },
                { new DateTime( 2021, 12, 1 ), GetTimeZone( "1", 1 ), Constants.DaysInDecember + 1 },
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, int, DateTime> GetSetDayOfYearData(IFixture fixture)
        {
            var simpleTimeZone = GetTimeZone( "1", 1 );

            var timeZoneWithInvalidity = GetTimeZone(
                "1 (+DS)",
                1,
                new DateTime( 1, 1, 16, 12, 0, 0 ),
                new DateTime( 1, 10, 26, 12, 0, 0 ) );

            var timeZoneWithDayOverlapInvalidity = GetTimeZone(
                "1 (+DS)",
                1,
                new DateTime( 1, 1, 15, 23, 30, 0 ),
                new DateTime( 1, 10, 26, 12, 0, 0 ) );

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
            var timeZone = GetTimeZone(
                "1 (+DS)",
                1,
                new DateTime( 1, 8, 26, 12, 0, 0 ),
                new DateTime( 1, 10, 26, 12, 0, 0 ) );

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
                { new DateTime( 2021, 1, 1 ), GetTimeZone( "1", 1 ), 0 },
                { new DateTime( 2021, 1, 1 ), GetTimeZone( "1", 1 ), Constants.DaysInYear + 1 },
                { new DateTime( 2020, 1, 1 ), GetTimeZone( "1", 1 ), Constants.DaysInLeapYear + 1 }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, Core.Chrono.TimeOfDay, DateTime> GetSetTimeOfDayData(IFixture fixture)
        {
            var simpleTimeZone = GetTimeZone( "1", 1 );

            var timeZoneWithInvalidity = GetTimeZone(
                "1 (+DS)",
                1,
                new DateTime( 1, 8, 26, 12, 0, 0 ),
                new DateTime( 1, 10, 26, 12, 0, 0 ) );

            return new TheoryData<DateTime, TimeZoneInfo, Core.Chrono.TimeOfDay, DateTime>
            {
                {
                    new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                    simpleTimeZone,
                    Core.Chrono.TimeOfDay.Start,
                    new DateTime( 2021, 8, 26 )
                },
                {
                    new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                    simpleTimeZone,
                    Core.Chrono.TimeOfDay.Mid,
                    new DateTime( 2021, 8, 26, 12, 0, 0 )
                },
                {
                    new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                    simpleTimeZone,
                    Core.Chrono.TimeOfDay.End,
                    new DateTime( 2021, 8, 26, 23, 59, 59, 999 ).AddTicks( 9999 )
                },
                {
                    new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                    simpleTimeZone,
                    new Core.Chrono.TimeOfDay( 12, 30, 40, 500, 6001 ),
                    new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 )
                },
                {
                    new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                    simpleTimeZone,
                    new Core.Chrono.TimeOfDay( 17, 40, 30, 200, 1001 ),
                    new DateTime( 2021, 8, 26, 17, 40, 30, 200 ).AddTicks( 1001 )
                },
                {
                    new DateTime( 2021, 8, 26 ),
                    timeZoneWithInvalidity,
                    new Core.Chrono.TimeOfDay( 11, 59, 59, 999, 9999 ),
                    new DateTime( 2021, 8, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
                },
                {
                    new DateTime( 2021, 8, 26 ),
                    timeZoneWithInvalidity,
                    new Core.Chrono.TimeOfDay( 13 ),
                    new DateTime( 2021, 8, 26, 13, 0, 0 )
                }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, bool, Core.Chrono.TimeOfDay, DateTime> GetSetTimeOfDayWithAmbiguityData(
            IFixture fixture)
        {
            var timeZone = GetTimeZone(
                "1 (+DS)",
                1,
                new DateTime( 1, 8, 26, 12, 0, 0 ),
                new DateTime( 1, 10, 26, 12, 0, 0 ) );

            return new TheoryData<DateTime, TimeZoneInfo, bool, Core.Chrono.TimeOfDay, DateTime>
            {
                {
                    new DateTime( 2021, 10, 26, 11, 0, 0 ),
                    timeZone,
                    false,
                    new Core.Chrono.TimeOfDay( 11 ),
                    new DateTime( 2021, 10, 26, 11, 0, 0 )
                },
                {
                    new DateTime( 2021, 10, 26, 11, 0, 0 ),
                    timeZone,
                    true,
                    new Core.Chrono.TimeOfDay( 11 ),
                    new DateTime( 2021, 10, 26, 11, 0, 0 )
                },
                {
                    new DateTime( 2021, 10, 26, 16, 0, 0 ),
                    timeZone,
                    false,
                    new Core.Chrono.TimeOfDay( 11 ),
                    new DateTime( 2021, 10, 26, 11, 0, 0 )
                },
                {
                    new DateTime( 2021, 10, 26, 6, 0, 0 ),
                    timeZone,
                    true,
                    new Core.Chrono.TimeOfDay( 11 ),
                    new DateTime( 2021, 10, 26, 11, 0, 0 )
                },
                {
                    new DateTime( 2021, 10, 26, 11, 59, 59, 999 ).AddTicks( 9999 ),
                    timeZone,
                    false,
                    new Core.Chrono.TimeOfDay( 11, 59, 59, 999, 9999 ),
                    new DateTime( 2021, 10, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
                },
                {
                    new DateTime( 2021, 10, 26, 11, 59, 59, 999 ).AddTicks( 9999 ),
                    timeZone,
                    true,
                    new Core.Chrono.TimeOfDay( 11, 59, 59, 999, 9999 ),
                    new DateTime( 2021, 10, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
                },
                {
                    new DateTime( 2021, 10, 26, 16, 0, 0 ),
                    timeZone,
                    false,
                    new Core.Chrono.TimeOfDay( 11, 59, 59, 999, 9999 ),
                    new DateTime( 2021, 10, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
                },
                {
                    new DateTime( 2021, 10, 26, 6, 0, 0 ),
                    timeZone,
                    true,
                    new Core.Chrono.TimeOfDay( 11, 59, 59, 999, 9999 ),
                    new DateTime( 2021, 10, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
                }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, Core.Chrono.TimeOfDay> GetSetTimeOfDayThrowData(IFixture fixture)
        {
            var timeZoneWithInvalidity = GetTimeZone(
                "1 (+DS)",
                1,
                new DateTime( 1, 8, 26, 12, 0, 0 ),
                new DateTime( 1, 10, 26, 12, 0, 0 ) );

            return new TheoryData<DateTime, TimeZoneInfo, Core.Chrono.TimeOfDay>
            {
                {
                    new DateTime( 2021, 8, 26 ),
                    timeZoneWithInvalidity,
                    new Core.Chrono.TimeOfDay( 12 )
                },
                {
                    new DateTime( 2021, 8, 26 ),
                    timeZoneWithInvalidity,
                    new Core.Chrono.TimeOfDay( 12, 59, 59, 999, 9999 )
                }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo> GetGetOppositeAmbiguousDateTimeWithUnambiguousData(
            IFixture fixture)
        {
            var (tStart, tEnd) = (new DateTime( 1, 3, 26, 12, 0, 0 ), new DateTime( 1, 9, 26, 12, 0, 0 ));

            return new TheoryData<DateTime, TimeZoneInfo>
            {
                { new DateTime( 2021, 9, 26, 12, 0, 0 ), GetTimeZone( "1", 1 ) },
                { new DateTime( 2021, 9, 26, 12, 0, 0 ), GetTimeZone( "1 (+DS)", 1, tStart, tEnd ) },
                { new DateTime( 2021, 9, 26, 10, 59, 59, 999 ).AddTicks( 9999 ), GetTimeZone( "1 (+DS)", 1, tStart, tEnd ) },
                { new DateTime( 2021, 3, 26, 12, 0, 0 ), GetTimeZone( "1 (+DS)", 1, tStart, tEnd, daylightSavingOffsetInHours: -1 ) },
                {
                    new DateTime( 2021, 3, 26, 10, 59, 59, 999 ).AddTicks( 9999 ),
                    GetTimeZone( "1 (+DS)", 1, tStart, tEnd, daylightSavingOffsetInHours: -1 )
                }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, DateTime> GetGetOppositeAmbiguousDateTimeData(
            IFixture fixture)
        {
            var (tStart, tEnd) = (new DateTime( 1, 3, 26, 12, 0, 0 ), new DateTime( 1, 9, 26, 12, 0, 0 ));

            return new TheoryData<DateTime, TimeZoneInfo, DateTime>
            {
                { new DateTime( 2021, 9, 26, 9, 0, 0 ), GetTimeZone( "1 (+DS)", 1, tStart, tEnd ), new DateTime( 2021, 9, 26, 10, 0, 0 ) },
                { new DateTime( 2021, 9, 26, 10, 0, 0 ), GetTimeZone( "1 (+DS)", 1, tStart, tEnd ), new DateTime( 2021, 9, 26, 9, 0, 0 ) },
                {
                    new DateTime( 2021, 9, 26, 9, 30, 0 ),
                    GetTimeZone( "1 (+DS)", 1, tStart, tEnd ),
                    new DateTime( 2021, 9, 26, 10, 30, 0 )
                },
                {
                    new DateTime( 2021, 9, 26, 10, 30, 0 ),
                    GetTimeZone( "1 (+DS)", 1, tStart, tEnd ),
                    new DateTime( 2021, 9, 26, 9, 30, 0 )
                },
                {
                    new DateTime( 2021, 9, 26, 9, 59, 59, 999 ).AddTicks( 9999 ),
                    GetTimeZone( "1 (+DS)", 1, tStart, tEnd ),
                    new DateTime( 2021, 9, 26, 10, 59, 59, 999 ).AddTicks( 9999 )
                },
                {
                    new DateTime( 2021, 9, 26, 10, 59, 59, 999 ).AddTicks( 9999 ),
                    GetTimeZone( "1 (+DS)", 1, tStart, tEnd ),
                    new DateTime( 2021, 9, 26, 9, 59, 59, 999 ).AddTicks( 9999 )
                },
                {
                    new DateTime( 2021, 9, 26, 7, 0, 0 ),
                    GetTimeZone( "1 (+DS)", 1, tStart, tEnd, daylightSavingOffsetInHours: 3 ),
                    new DateTime( 2021, 9, 26, 10, 0, 0 )
                },
                {
                    new DateTime( 2021, 9, 26, 10, 0, 0 ),
                    GetTimeZone( "1 (+DS)", 1, tStart, tEnd, daylightSavingOffsetInHours: 3 ),
                    new DateTime( 2021, 9, 26, 7, 0, 0 )
                },
                {
                    new DateTime( 2021, 9, 26, 7, 59, 59, 999 ).AddTicks( 9999 ),
                    GetTimeZone( "1 (+DS)", 1, tStart, tEnd, daylightSavingOffsetInHours: 3 ),
                    new DateTime( 2021, 9, 26, 10, 59, 59, 999 ).AddTicks( 9999 )
                },
                {
                    new DateTime( 2021, 9, 26, 10, 59, 59, 999 ).AddTicks( 9999 ),
                    GetTimeZone( "1 (+DS)", 1, tStart, tEnd, daylightSavingOffsetInHours: 3 ),
                    new DateTime( 2021, 9, 26, 7, 59, 59, 999 ).AddTicks( 9999 )
                },
                {
                    new DateTime( 2021, 3, 26, 10, 0, 0 ),
                    GetTimeZone( "1 (-DS)", 1, tStart, tEnd, daylightSavingOffsetInHours: -1 ),
                    new DateTime( 2021, 3, 26, 11, 0, 0 )
                },
                {
                    new DateTime( 2021, 3, 26, 11, 0, 0 ),
                    GetTimeZone( "1 (-DS)", 1, tStart, tEnd, daylightSavingOffsetInHours: -1 ),
                    new DateTime( 2021, 3, 26, 10, 0, 0 )
                },
                {
                    new DateTime( 2021, 3, 26, 10, 30, 0 ),
                    GetTimeZone( "1 (-DS)", 1, tStart, tEnd, daylightSavingOffsetInHours: -1 ),
                    new DateTime( 2021, 3, 26, 11, 30, 0 )
                },
                {
                    new DateTime( 2021, 3, 26, 11, 30, 0 ),
                    GetTimeZone( "1 (-DS)", 1, tStart, tEnd, daylightSavingOffsetInHours: -1 ),
                    new DateTime( 2021, 3, 26, 10, 30, 0 )
                },
                {
                    new DateTime( 2021, 3, 26, 10, 59, 59, 999 ).AddTicks( 9999 ),
                    GetTimeZone( "1 (-DS)", 1, tStart, tEnd, daylightSavingOffsetInHours: -1 ),
                    new DateTime( 2021, 3, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
                },
                {
                    new DateTime( 2021, 3, 26, 11, 59, 59, 999 ).AddTicks( 9999 ),
                    GetTimeZone( "1 (-DS)", 1, tStart, tEnd, daylightSavingOffsetInHours: -1 ),
                    new DateTime( 2021, 3, 26, 10, 59, 59, 999 ).AddTicks( 9999 )
                },
                {
                    new DateTime( 2021, 3, 26, 8, 0, 0 ),
                    GetTimeZone( "1 (-DS)", 1, tStart, tEnd, daylightSavingOffsetInHours: -3 ),
                    new DateTime( 2021, 3, 26, 11, 0, 0 )
                },
                {
                    new DateTime( 2021, 3, 26, 11, 0, 0 ),
                    GetTimeZone( "1 (-DS)", 1, tStart, tEnd, daylightSavingOffsetInHours: -3 ),
                    new DateTime( 2021, 3, 26, 8, 0, 0 )
                },
                {
                    new DateTime( 2021, 3, 26, 8, 59, 59, 999 ).AddTicks( 9999 ),
                    GetTimeZone( "1 (-DS)", 1, tStart, tEnd, daylightSavingOffsetInHours: -3 ),
                    new DateTime( 2021, 3, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
                },
                {
                    new DateTime( 2021, 3, 26, 11, 59, 59, 999 ).AddTicks( 9999 ),
                    GetTimeZone( "1 (-DS)", 1, tStart, tEnd, daylightSavingOffsetInHours: -3 ),
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

        public static TimeZoneInfo GetTimeZone(string id, double offsetInHours)
        {
            var fullName = $"Test Time Zone [{id}]";

            return TimeZoneInfo.CreateCustomTimeZone(
                id: fullName,
                baseUtcOffset: TimeSpan.FromHours( offsetInHours ),
                displayName: fullName,
                standardDisplayName: fullName );
        }

        public static TimeZoneInfo GetTimeZone(
            string id,
            double offsetInHours,
            DateTime daylightTransitionStart,
            DateTime daylightTransitionEnd,
            double daylightSavingOffsetInHours = 1.0)
        {
            return GetTimeZone(
                id,
                offsetInHours,
                DateTime.MinValue,
                DateTime.MaxValue,
                daylightTransitionStart,
                daylightTransitionEnd,
                daylightSavingOffsetInHours );
        }

        public static TimeZoneInfo GetTimeZone(
            string id,
            double offsetInHours,
            DateTime daylightRuleStart,
            DateTime daylightRuleEnd,
            DateTime daylightTransitionStart,
            DateTime daylightTransitionEnd,
            double daylightSavingOffsetInHours = 1.0)
        {
            var fullName = $"Test Time Zone [{id}]";

            return TimeZoneInfo.CreateCustomTimeZone(
                id: fullName,
                baseUtcOffset: TimeSpan.FromHours( offsetInHours ),
                displayName: fullName,
                standardDisplayName: fullName,
                daylightDisplayName: fullName,
                adjustmentRules: new[]
                {
                    TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(
                        dateStart: daylightRuleStart,
                        dateEnd: daylightRuleEnd,
                        daylightDelta: TimeSpan.FromHours( daylightSavingOffsetInHours ),
                        daylightTransitionStart: TimeZoneInfo.TransitionTime.CreateFixedDateRule(
                            timeOfDay: new DateTime( daylightTransitionStart.TimeOfDay.Ticks ),
                            month: daylightTransitionStart.Month,
                            day: daylightTransitionStart.Day ),
                        daylightTransitionEnd: TimeZoneInfo.TransitionTime.CreateFixedDateRule(
                            timeOfDay: new DateTime( daylightTransitionEnd.TimeOfDay.Ticks ),
                            month: daylightTransitionEnd.Month,
                            day: daylightTransitionEnd.Day ) )
                } );
        }
    }
}
