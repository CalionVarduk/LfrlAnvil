using System;
using System.Collections.Generic;
using AutoFixture;
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
                        dateStart: DateTime.MinValue,
                        dateEnd: DateTime.MaxValue,
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
