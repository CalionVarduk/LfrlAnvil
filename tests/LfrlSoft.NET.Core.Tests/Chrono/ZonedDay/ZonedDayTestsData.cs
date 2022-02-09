using System;
using System.Collections.Generic;
using AutoFixture;
using LfrlSoft.NET.Core.Chrono;
using LfrlSoft.NET.Core.Tests.Chrono.ZonedDateTime;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Chrono.ZonedDay
{
    public class ZonedDayTestsData
    {
        public static TheoryData<DateTime, TimeZoneInfo> GetCreateData(IFixture fixture)
        {
            var date = fixture.Create<DateTime>().Date;
            var timeZoneOffset = fixture.Create<int>() % 12;
            var timeZone = ZonedDateTimeTestsData.GetTimeZone( $"{timeZoneOffset}", timeZoneOffset );

            return new TheoryData<DateTime, TimeZoneInfo>
            {
                { date, timeZone },
                { date.Add( new TimeSpan( 0, 12, 30, 40, 500 ) ).AddTicks( 6001 ), timeZone },
                { date.Add( new TimeSpan( 0, 23, 59, 59, 999 ) ).AddTicks( 9999 ), timeZone }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, Core.Chrono.Duration> GetCreateWithContainedInvalidityRangeData(IFixture fixture)
        {
            var positiveTimeZone = ZonedDateTimeTestsData.GetTimeZone(
                "1 (+DS)",
                1,
                new DateTime( 1, 8, 26, 2, 0, 0 ),
                new DateTime( 1, 10, 26, 3, 0, 0 ) );

            var negativeTimeZone = ZonedDateTimeTestsData.GetTimeZone(
                "1 (-DS)",
                1,
                new DateTime( 1, 6, 26, 3, 0, 0 ),
                new DateTime( 1, 8, 26, 2, 0, 0 ),
                daylightSavingOffsetInHours: -1 );

            return new TheoryData<DateTime, TimeZoneInfo, Core.Chrono.Duration>
            {
                { new DateTime( 2021, 8, 26 ), positiveTimeZone, Core.Chrono.Duration.FromHours( 23 ) },
                { new DateTime( 2021, 8, 26 ), negativeTimeZone, Core.Chrono.Duration.FromHours( 23 ) },
                { new DateTime( 2021, 8, 26, 23, 59, 59, 999 ).AddTicks( 9999 ), positiveTimeZone, Core.Chrono.Duration.FromHours( 23 ) },
                { new DateTime( 2021, 8, 26, 23, 59, 59, 999 ).AddTicks( 9999 ), negativeTimeZone, Core.Chrono.Duration.FromHours( 23 ) },
                { new DateTime( 2021, 8, 26, 2, 0, 0 ), positiveTimeZone, Core.Chrono.Duration.FromHours( 23 ) },
                { new DateTime( 2021, 8, 26, 2, 59, 59, 999 ).AddTicks( 9999 ), positiveTimeZone, Core.Chrono.Duration.FromHours( 23 ) },
                { new DateTime( 2021, 8, 26, 2, 0, 0 ), negativeTimeZone, Core.Chrono.Duration.FromHours( 23 ) },
                { new DateTime( 2021, 8, 26, 2, 59, 59, 999 ).AddTicks( 9999 ), negativeTimeZone, Core.Chrono.Duration.FromHours( 23 ) }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, Core.Chrono.Duration> GetCreateWithContainedAmbiguityRangeData(IFixture fixture)
        {
            var positiveTimeZone = ZonedDateTimeTestsData.GetTimeZone(
                "1 (+DS)",
                1,
                new DateTime( 1, 6, 26, 2, 0, 0 ),
                new DateTime( 1, 8, 26, 3, 0, 0 ) );

            var negativeTimeZone = ZonedDateTimeTestsData.GetTimeZone(
                "1 (-DS)",
                1,
                new DateTime( 1, 8, 26, 3, 0, 0 ),
                new DateTime( 1, 10, 26, 2, 0, 0 ),
                daylightSavingOffsetInHours: -1 );

            return new TheoryData<DateTime, TimeZoneInfo, Core.Chrono.Duration>
            {
                { new DateTime( 2021, 8, 26 ), positiveTimeZone, Core.Chrono.Duration.FromHours( 25 ) },
                { new DateTime( 2021, 8, 26 ), negativeTimeZone, Core.Chrono.Duration.FromHours( 25 ) },
                { new DateTime( 2021, 8, 26, 23, 59, 59, 999 ).AddTicks( 9999 ), positiveTimeZone, Core.Chrono.Duration.FromHours( 25 ) },
                { new DateTime( 2021, 8, 26, 23, 59, 59, 999 ).AddTicks( 9999 ), negativeTimeZone, Core.Chrono.Duration.FromHours( 25 ) },
                { new DateTime( 2021, 8, 26, 2, 0, 0 ), positiveTimeZone, Core.Chrono.Duration.FromHours( 25 ) },
                { new DateTime( 2021, 8, 26, 2, 59, 59, 999 ).AddTicks( 9999 ), positiveTimeZone, Core.Chrono.Duration.FromHours( 25 ) },
                { new DateTime( 2021, 8, 26, 2, 0, 0 ), negativeTimeZone, Core.Chrono.Duration.FromHours( 25 ) },
                { new DateTime( 2021, 8, 26, 2, 59, 59, 999 ).AddTicks( 9999 ), negativeTimeZone, Core.Chrono.Duration.FromHours( 25 ) }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, DateTime, Core.Chrono.Duration>
            GetCreateWithInvalidStartTimeData(IFixture fixture)
        {
            var positiveTimeZone1 = ZonedDateTimeTestsData.GetTimeZone(
                "1 (+DS)",
                1,
                new DateTime( 1, 8, 25, 23, 1, 0 ),
                new DateTime( 1, 10, 26, 3, 0, 0 ) );

            var positiveTimeZone2 = ZonedDateTimeTestsData.GetTimeZone(
                "1 (+DS)",
                1,
                new DateTime( 1, 8, 25, 23, 30, 0 ),
                new DateTime( 1, 10, 26, 3, 0, 0 ) );

            var positiveTimeZone3 = ZonedDateTimeTestsData.GetTimeZone(
                "1 (+DS)",
                1,
                new DateTime( 1, 8, 25, 23, 59, 0 ),
                new DateTime( 1, 10, 26, 3, 0, 0 ) );

            var positiveTimeZone4 = ZonedDateTimeTestsData.GetTimeZone(
                "1 (+DS)",
                1,
                new DateTime( 1, 8, 26, 0, 0, 0 ),
                new DateTime( 1, 10, 26, 3, 0, 0 ) );

            var negativeTimeZone1 = ZonedDateTimeTestsData.GetTimeZone(
                "1 (-DS)",
                1,
                new DateTime( 1, 6, 26, 2, 0, 0 ),
                new DateTime( 1, 8, 25, 23, 1, 0 ),
                daylightSavingOffsetInHours: -1 );

            var negativeTimeZone2 = ZonedDateTimeTestsData.GetTimeZone(
                "1 (-DS)",
                1,
                new DateTime( 1, 6, 26, 2, 0, 0 ),
                new DateTime( 1, 8, 25, 23, 30, 0 ),
                daylightSavingOffsetInHours: -1 );

            var negativeTimeZone3 = ZonedDateTimeTestsData.GetTimeZone(
                "1 (-DS)",
                1,
                new DateTime( 1, 6, 26, 2, 0, 0 ),
                new DateTime( 1, 8, 25, 23, 59, 0 ),
                daylightSavingOffsetInHours: -1 );

            var negativeTimeZone4 = ZonedDateTimeTestsData.GetTimeZone(
                "1 (-DS)",
                1,
                new DateTime( 1, 6, 26, 2, 0, 0 ),
                new DateTime( 1, 8, 26, 0, 0, 0 ),
                daylightSavingOffsetInHours: -1 );

            return new TheoryData<DateTime, TimeZoneInfo, DateTime, Core.Chrono.Duration>
            {
                {
                    new DateTime( 2021, 8, 26 ),
                    positiveTimeZone1,
                    new DateTime( 2021, 8, 26, 0, 1, 0 ),
                    new Core.Chrono.Duration( 23, 59, 0 )
                },
                {
                    new DateTime( 2021, 8, 26 ),
                    positiveTimeZone2,
                    new DateTime( 2021, 8, 26, 0, 30, 0 ),
                    new Core.Chrono.Duration( 23, 30, 0 )
                },
                {
                    new DateTime( 2021, 8, 26 ),
                    positiveTimeZone3,
                    new DateTime( 2021, 8, 26, 0, 59, 0 ),
                    new Core.Chrono.Duration( 23, 1, 0 )
                },
                {
                    new DateTime( 2021, 8, 26 ),
                    positiveTimeZone4,
                    new DateTime( 2021, 8, 26, 1, 0, 0 ),
                    new Core.Chrono.Duration( 23, 0, 0 )
                },
                {
                    new DateTime( 2021, 8, 26 ),
                    negativeTimeZone1,
                    new DateTime( 2021, 8, 26, 0, 1, 0 ),
                    new Core.Chrono.Duration( 23, 59, 0 )
                },
                {
                    new DateTime( 2021, 8, 26 ),
                    negativeTimeZone2,
                    new DateTime( 2021, 8, 26, 0, 30, 0 ),
                    new Core.Chrono.Duration( 23, 30, 0 )
                },
                {
                    new DateTime( 2021, 8, 26 ),
                    negativeTimeZone3,
                    new DateTime( 2021, 8, 26, 0, 59, 0 ),
                    new Core.Chrono.Duration( 23, 1, 0 )
                },
                {
                    new DateTime( 2021, 8, 26 ),
                    negativeTimeZone4,
                    new DateTime( 2021, 8, 26, 1, 0, 0 ),
                    new Core.Chrono.Duration( 23, 0, 0 )
                }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, DateTime, Core.Chrono.Duration>
            GetCreateWithInvalidEndTimeData(IFixture fixture)
        {
            var positiveTimeZone1 = ZonedDateTimeTestsData.GetTimeZone(
                "1 (+DS)",
                1,
                new DateTime( 1, 8, 25, 23, 1, 0 ),
                new DateTime( 1, 10, 26, 3, 0, 0 ) );

            var positiveTimeZone2 = ZonedDateTimeTestsData.GetTimeZone(
                "1 (+DS)",
                1,
                new DateTime( 1, 8, 25, 23, 30, 0 ),
                new DateTime( 1, 10, 26, 3, 0, 0 ) );

            var positiveTimeZone3 = ZonedDateTimeTestsData.GetTimeZone(
                "1 (+DS)",
                1,
                new DateTime( 1, 8, 25, 23, 59, 0 ),
                new DateTime( 1, 10, 26, 3, 0, 0 ) );

            var positiveTimeZone4 = ZonedDateTimeTestsData.GetTimeZone(
                "1 (+DS)",
                1,
                new DateTime( 1, 8, 25, 23, 0, 0 ),
                new DateTime( 1, 10, 26, 3, 0, 0 ) );

            var negativeTimeZone1 = ZonedDateTimeTestsData.GetTimeZone(
                "1 (-DS)",
                1,
                new DateTime( 1, 6, 26, 2, 0, 0 ),
                new DateTime( 1, 8, 25, 23, 1, 0 ),
                daylightSavingOffsetInHours: -1 );

            var negativeTimeZone2 = ZonedDateTimeTestsData.GetTimeZone(
                "1 (-DS)",
                1,
                new DateTime( 1, 6, 26, 2, 0, 0 ),
                new DateTime( 1, 8, 25, 23, 30, 0 ),
                daylightSavingOffsetInHours: -1 );

            var negativeTimeZone3 = ZonedDateTimeTestsData.GetTimeZone(
                "1 (-DS)",
                1,
                new DateTime( 1, 6, 26, 2, 0, 0 ),
                new DateTime( 1, 8, 25, 23, 59, 0 ),
                daylightSavingOffsetInHours: -1 );

            var negativeTimeZone4 = ZonedDateTimeTestsData.GetTimeZone(
                "1 (-DS)",
                1,
                new DateTime( 1, 6, 26, 2, 0, 0 ),
                new DateTime( 1, 8, 25, 23, 0, 0 ),
                daylightSavingOffsetInHours: -1 );

            return new TheoryData<DateTime, TimeZoneInfo, DateTime, Core.Chrono.Duration>
            {
                {
                    new DateTime( 2021, 8, 25 ),
                    positiveTimeZone1,
                    new DateTime( 2021, 8, 25, 23, 0, 59, 999 ).AddTicks( 9999 ),
                    new Core.Chrono.Duration( 23, 1, 0 )
                },
                {
                    new DateTime( 2021, 8, 25 ),
                    positiveTimeZone2,
                    new DateTime( 2021, 8, 25, 23, 29, 59, 999 ).AddTicks( 9999 ),
                    new Core.Chrono.Duration( 23, 30, 0 )
                },
                {
                    new DateTime( 2021, 8, 25 ),
                    positiveTimeZone3,
                    new DateTime( 2021, 8, 25, 23, 58, 59, 999 ).AddTicks( 9999 ),
                    new Core.Chrono.Duration( 23, 59, 0 )
                },
                {
                    new DateTime( 2021, 8, 25 ),
                    positiveTimeZone4,
                    new DateTime( 2021, 8, 25, 22, 59, 59, 999 ).AddTicks( 9999 ),
                    new Core.Chrono.Duration( 23, 0, 0 )
                },
                {
                    new DateTime( 2021, 8, 25 ),
                    negativeTimeZone1,
                    new DateTime( 2021, 8, 25, 23, 0, 59, 999 ).AddTicks( 9999 ),
                    new Core.Chrono.Duration( 23, 1, 0 )
                },
                {
                    new DateTime( 2021, 8, 25 ),
                    negativeTimeZone2,
                    new DateTime( 2021, 8, 25, 23, 29, 59, 999 ).AddTicks( 9999 ),
                    new Core.Chrono.Duration( 23, 30, 0 )
                },
                {
                    new DateTime( 2021, 8, 25 ),
                    negativeTimeZone3,
                    new DateTime( 2021, 8, 25, 23, 58, 59, 999 ).AddTicks( 9999 ),
                    new Core.Chrono.Duration( 23, 59, 0 )
                },
                {
                    new DateTime( 2021, 8, 25 ),
                    negativeTimeZone4,
                    new DateTime( 2021, 8, 25, 22, 59, 59, 999 ).AddTicks( 9999 ),
                    new Core.Chrono.Duration( 23, 0, 0 )
                }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, bool, Core.Chrono.Duration>
            GetCreateWithAmbiguousStartTimeData(IFixture fixture)
        {
            var positiveTimeZone1 = ZonedDateTimeTestsData.GetTimeZone(
                "1 (+DS)",
                1,
                new DateTime( 1, 6, 26, 2, 0, 0 ),
                new DateTime( 1, 8, 26, 0, 1, 0 ) );

            var positiveTimeZone2 = ZonedDateTimeTestsData.GetTimeZone(
                "1 (+DS)",
                1,
                new DateTime( 1, 6, 26, 2, 0, 0 ),
                new DateTime( 1, 8, 26, 0, 30, 0 ) );

            var positiveTimeZone3 = ZonedDateTimeTestsData.GetTimeZone(
                "1 (+DS)",
                1,
                new DateTime( 1, 6, 26, 2, 0, 0 ),
                new DateTime( 1, 8, 26, 0, 59, 0 ) );

            var positiveTimeZone4 = ZonedDateTimeTestsData.GetTimeZone(
                "1 (+DS)",
                1,
                new DateTime( 1, 6, 26, 2, 0, 0 ),
                new DateTime( 1, 8, 26, 1, 0, 0 ) );

            var negativeTimeZone1 = ZonedDateTimeTestsData.GetTimeZone(
                "1 (-DS)",
                1,
                new DateTime( 1, 8, 26, 0, 1, 0 ),
                new DateTime( 1, 10, 26, 2, 0, 0 ),
                daylightSavingOffsetInHours: -1 );

            var negativeTimeZone2 = ZonedDateTimeTestsData.GetTimeZone(
                "1 (-DS)",
                1,
                new DateTime( 1, 8, 26, 0, 30, 0 ),
                new DateTime( 1, 10, 26, 2, 0, 0 ),
                daylightSavingOffsetInHours: -1 );

            var negativeTimeZone3 = ZonedDateTimeTestsData.GetTimeZone(
                "1 (-DS)",
                1,
                new DateTime( 1, 8, 26, 0, 59, 0 ),
                new DateTime( 1, 10, 26, 2, 0, 0 ),
                daylightSavingOffsetInHours: -1 );

            var negativeTimeZone4 = ZonedDateTimeTestsData.GetTimeZone(
                "1 (-DS)",
                1,
                new DateTime( 1, 8, 26, 1, 0, 0 ),
                new DateTime( 1, 10, 26, 2, 0, 0 ),
                daylightSavingOffsetInHours: -1 );

            return new TheoryData<DateTime, TimeZoneInfo, bool, Core.Chrono.Duration>
            {
                {
                    new DateTime( 2021, 8, 26 ),
                    positiveTimeZone1,
                    true,
                    new Core.Chrono.Duration( 24, 1, 0 )
                },
                {
                    new DateTime( 2021, 8, 26 ),
                    positiveTimeZone2,
                    true,
                    new Core.Chrono.Duration( 24, 30, 0 )
                },
                {
                    new DateTime( 2021, 8, 26 ),
                    positiveTimeZone3,
                    true,
                    new Core.Chrono.Duration( 24, 59, 0 )
                },
                {
                    new DateTime( 2021, 8, 26 ),
                    positiveTimeZone4,
                    true,
                    new Core.Chrono.Duration( 25, 0, 0 )
                },
                {
                    new DateTime( 2021, 8, 26 ),
                    negativeTimeZone1,
                    false,
                    new Core.Chrono.Duration( 24, 1, 0 )
                },
                {
                    new DateTime( 2021, 8, 26 ),
                    negativeTimeZone2,
                    false,
                    new Core.Chrono.Duration( 24, 30, 0 )
                },
                {
                    new DateTime( 2021, 8, 26 ),
                    negativeTimeZone3,
                    false,
                    new Core.Chrono.Duration( 24, 59, 0 )
                },
                {
                    new DateTime( 2021, 8, 26 ),
                    negativeTimeZone4,
                    false,
                    new Core.Chrono.Duration( 25, 0, 0 )
                }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, bool, Core.Chrono.Duration>
            GetCreateWithAmbiguousEndTimeData(IFixture fixture)
        {
            var positiveTimeZone1 = ZonedDateTimeTestsData.GetTimeZone(
                "1 (+DS)",
                1,
                new DateTime( 1, 6, 26, 2, 0, 0 ),
                new DateTime( 1, 8, 27, 0, 1, 0 ) );

            var positiveTimeZone2 = ZonedDateTimeTestsData.GetTimeZone(
                "1 (+DS)",
                1,
                new DateTime( 1, 6, 26, 2, 0, 0 ),
                new DateTime( 1, 8, 27, 0, 30, 0 ) );

            var positiveTimeZone3 = ZonedDateTimeTestsData.GetTimeZone(
                "1 (+DS)",
                1,
                new DateTime( 1, 6, 26, 2, 0, 0 ),
                new DateTime( 1, 8, 27, 0, 59, 0 ) );

            var positiveTimeZone4 = ZonedDateTimeTestsData.GetTimeZone(
                "1 (+DS)",
                1,
                new DateTime( 1, 6, 26, 2, 0, 0 ),
                new DateTime( 1, 8, 27, 0, 0, 0 ) );

            var negativeTimeZone1 = ZonedDateTimeTestsData.GetTimeZone(
                "1 (-DS)",
                1,
                new DateTime( 1, 8, 26, 0, 1, 0 ),
                new DateTime( 1, 10, 26, 2, 0, 0 ),
                daylightSavingOffsetInHours: -1 );

            var negativeTimeZone2 = ZonedDateTimeTestsData.GetTimeZone(
                "1 (-DS)",
                1,
                new DateTime( 1, 8, 26, 0, 30, 0 ),
                new DateTime( 1, 10, 26, 2, 0, 0 ),
                daylightSavingOffsetInHours: -1 );

            var negativeTimeZone3 = ZonedDateTimeTestsData.GetTimeZone(
                "1 (-DS)",
                1,
                new DateTime( 1, 8, 26, 0, 59, 0 ),
                new DateTime( 1, 10, 26, 2, 0, 0 ),
                daylightSavingOffsetInHours: -1 );

            var negativeTimeZone4 = ZonedDateTimeTestsData.GetTimeZone(
                "1 (-DS)",
                1,
                new DateTime( 1, 8, 26, 0, 0, 0 ),
                new DateTime( 1, 10, 26, 2, 0, 0 ),
                daylightSavingOffsetInHours: -1 );

            return new TheoryData<DateTime, TimeZoneInfo, bool, Core.Chrono.Duration>
            {
                {
                    new DateTime( 2021, 8, 26 ),
                    positiveTimeZone1,
                    false,
                    new Core.Chrono.Duration( 24, 59, 0 )
                },
                {
                    new DateTime( 2021, 8, 26 ),
                    positiveTimeZone2,
                    false,
                    new Core.Chrono.Duration( 24, 30, 0 )
                },
                {
                    new DateTime( 2021, 8, 26 ),
                    positiveTimeZone3,
                    false,
                    new Core.Chrono.Duration( 24, 1, 0 )
                },
                {
                    new DateTime( 2021, 8, 26 ),
                    positiveTimeZone4,
                    false,
                    new Core.Chrono.Duration( 25, 0, 0 )
                },
                {
                    new DateTime( 2021, 8, 25 ),
                    negativeTimeZone1,
                    true,
                    new Core.Chrono.Duration( 24, 59, 0 )
                },
                {
                    new DateTime( 2021, 8, 25 ),
                    negativeTimeZone2,
                    true,
                    new Core.Chrono.Duration( 24, 30, 0 )
                },
                {
                    new DateTime( 2021, 8, 25 ),
                    negativeTimeZone3,
                    true,
                    new Core.Chrono.Duration( 24, 1, 0 )
                },
                {
                    new DateTime( 2021, 8, 25 ),
                    negativeTimeZone4,
                    true,
                    new Core.Chrono.Duration( 25, 0, 0 )
                }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, DateTime, bool, Core.Chrono.Duration>
            GetCreateWithInvalidStartTimeAndAmbiguousEndTimeData(IFixture fixture)
        {
            var positiveTimeZone1 = ZonedDateTimeTestsData.GetTimeZone(
                "1 (+DS)",
                1,
                new DateTime( 1, 8, 25, 23, 1, 0 ),
                new DateTime( 1, 8, 27, 0, 1, 0 ) );

            var positiveTimeZone2 = ZonedDateTimeTestsData.GetTimeZone(
                "1 (+DS)",
                1,
                new DateTime( 1, 8, 25, 23, 59, 0 ),
                new DateTime( 1, 8, 27, 0, 59, 0 ) );

            var positiveTimeZone3 = ZonedDateTimeTestsData.GetTimeZone(
                "1 (+DS)",
                1,
                new DateTime( 1, 8, 26, 0, 0, 0 ),
                new DateTime( 1, 8, 27, 0, 0, 0 ) );

            var negativeTimeZone1 = ZonedDateTimeTestsData.GetTimeZone(
                "1 (-DS)",
                1,
                new DateTime( 1, 8, 27, 0, 1, 0 ),
                new DateTime( 1, 8, 25, 23, 1, 0 ),
                daylightSavingOffsetInHours: -1 );

            var negativeTimeZone2 = ZonedDateTimeTestsData.GetTimeZone(
                "1 (-DS)",
                1,
                new DateTime( 1, 8, 27, 0, 59, 0 ),
                new DateTime( 1, 8, 25, 23, 59, 0 ),
                daylightSavingOffsetInHours: -1 );

            var negativeTimeZone3 = ZonedDateTimeTestsData.GetTimeZone(
                "1 (-DS)",
                1,
                new DateTime( 1, 8, 27, 0, 0, 0 ),
                new DateTime( 1, 8, 26, 0, 0, 0 ),
                daylightSavingOffsetInHours: -1 );

            return new TheoryData<DateTime, TimeZoneInfo, DateTime, bool, Core.Chrono.Duration>
            {
                {
                    new DateTime( 2021, 8, 26 ),
                    positiveTimeZone1,
                    new DateTime( 2021, 8, 26, 0, 1, 0 ),
                    false,
                    new Core.Chrono.Duration( 24, 58, 0 )
                },
                {
                    new DateTime( 2021, 8, 26 ),
                    positiveTimeZone2,
                    new DateTime( 2021, 8, 26, 0, 59, 0 ),
                    false,
                    new Core.Chrono.Duration( 23, 2, 0 )
                },
                {
                    new DateTime( 2021, 8, 26 ),
                    positiveTimeZone3,
                    new DateTime( 2021, 8, 26, 1, 0, 0 ),
                    false,
                    new Core.Chrono.Duration( 24, 0, 0 )
                },
                {
                    new DateTime( 2021, 8, 26 ),
                    negativeTimeZone1,
                    new DateTime( 2021, 8, 26, 0, 1, 0 ),
                    true,
                    new Core.Chrono.Duration( 24, 58, 0 )
                },
                {
                    new DateTime( 2021, 8, 26 ),
                    negativeTimeZone2,
                    new DateTime( 2021, 8, 26, 0, 59, 0 ),
                    true,
                    new Core.Chrono.Duration( 23, 2, 0 )
                },
                {
                    new DateTime( 2021, 8, 26 ),
                    negativeTimeZone3,
                    new DateTime( 2021, 8, 26, 1, 0, 0 ),
                    true,
                    new Core.Chrono.Duration( 24, 0, 0 )
                }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, DateTime, bool, Core.Chrono.Duration>
            GetCreateWithAmbiguousStartTimeAndInvalidEndTimeData(IFixture fixture)
        {
            var positiveTimeZone1 = ZonedDateTimeTestsData.GetTimeZone(
                "1 (+DS)",
                1,
                new DateTime( 1, 8, 26, 23, 59, 0 ),
                new DateTime( 1, 8, 26, 0, 59, 0 ) );

            var positiveTimeZone2 = ZonedDateTimeTestsData.GetTimeZone(
                "1 (+DS)",
                1,
                new DateTime( 1, 8, 26, 23, 1, 0 ),
                new DateTime( 1, 8, 26, 0, 1, 0 ) );

            var positiveTimeZone3 = ZonedDateTimeTestsData.GetTimeZone(
                "1 (+DS)",
                1,
                new DateTime( 1, 8, 26, 23, 0, 0 ),
                new DateTime( 1, 8, 26, 1, 0, 0 ) );

            var negativeTimeZone1 = ZonedDateTimeTestsData.GetTimeZone(
                "1 (-DS)",
                1,
                new DateTime( 1, 8, 26, 0, 59, 0 ),
                new DateTime( 1, 8, 26, 23, 59, 0 ),
                daylightSavingOffsetInHours: -1 );

            var negativeTimeZone2 = ZonedDateTimeTestsData.GetTimeZone(
                "1 (-DS)",
                1,
                new DateTime( 1, 8, 26, 0, 1, 0 ),
                new DateTime( 1, 8, 26, 23, 1, 0 ),
                daylightSavingOffsetInHours: -1 );

            var negativeTimeZone3 = ZonedDateTimeTestsData.GetTimeZone(
                "1 (-DS)",
                1,
                new DateTime( 1, 8, 26, 1, 0, 0 ),
                new DateTime( 1, 8, 26, 23, 0, 0 ),
                daylightSavingOffsetInHours: -1 );

            return new TheoryData<DateTime, TimeZoneInfo, DateTime, bool, Core.Chrono.Duration>
            {
                {
                    new DateTime( 2021, 8, 26 ),
                    positiveTimeZone1,
                    new DateTime( 2021, 8, 26, 23, 58, 59, 999 ).AddTicks( 9999 ),
                    true,
                    new Core.Chrono.Duration( 24, 58, 0 )
                },
                {
                    new DateTime( 2021, 8, 26 ),
                    positiveTimeZone2,
                    new DateTime( 2021, 8, 26, 23, 0, 59, 999 ).AddTicks( 9999 ),
                    true,
                    new Core.Chrono.Duration( 23, 2, 0 )
                },
                {
                    new DateTime( 2021, 8, 26 ),
                    positiveTimeZone3,
                    new DateTime( 2021, 8, 26, 22, 59, 59, 999 ).AddTicks( 9999 ),
                    true,
                    new Core.Chrono.Duration( 24, 0, 0 )
                },
                {
                    new DateTime( 2021, 8, 26 ),
                    negativeTimeZone1,
                    new DateTime( 2021, 8, 26, 23, 58, 59, 999 ).AddTicks( 9999 ),
                    false,
                    new Core.Chrono.Duration( 24, 58, 0 )
                },
                {
                    new DateTime( 2021, 8, 26 ),
                    negativeTimeZone2,
                    new DateTime( 2021, 8, 26, 23, 0, 59, 999 ).AddTicks( 9999 ),
                    false,
                    new Core.Chrono.Duration( 23, 2, 0 )
                },
                {
                    new DateTime( 2021, 8, 26 ),
                    negativeTimeZone3,
                    new DateTime( 2021, 8, 26, 22, 59, 59, 999 ).AddTicks( 9999 ),
                    false,
                    new Core.Chrono.Duration( 24, 0, 0 )
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

            var tz1 = ZonedDateTimeTestsData.GetTimeZone(
                "03.00",
                3,
                new DateTime( 1, 3, 26, 12, 0, 0 ),
                new DateTime( 1, 9, 26, 12, 0, 0 ) );

            var tz2 = ZonedDateTimeTestsData.GetTimeZone(
                "-05.00",
                -5,
                new DateTime( 1, 3, 26, 12, 0, 0 ),
                new DateTime( 1, 9, 26, 12, 0, 0 ) );

            return new TheoryData<DateTime, TimeZoneInfo, string>
            {
                { dt1, tz1, "2021-02-21 +03:00 (Test Time Zone [03.00])" },
                { dt2, tz1, "2021-08-05 +04:00 (Test Time Zone [03.00])" },
                { dt1, tz2, "2021-02-21 -05:00 (Test Time Zone [-05.00])" },
                { dt2, tz2, "2021-08-05 -04:00 (Test Time Zone [-05.00])" },
                { dt3, tz1, "2021-03-26 +03:00 +04:00 (Test Time Zone [03.00])" },
                { dt4, tz1, "2021-09-26 +04:00 +03:00 (Test Time Zone [03.00])" }
            };
        }

        public static IEnumerable<object[]> GetEqualsData(IFixture fixture)
        {
            var (dt1, dt2) = (new DateTime( 2021, 8, 24 ), new DateTime( 2021, 8, 25 ));
            var (tz1, tz2) = (ZonedDateTimeTestsData.GetTimeZone( "03.00", 3 ), ZonedDateTimeTestsData.GetTimeZone( "05.00", 5 ));

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
            var (dt1, dt2) = (new DateTime( 2021, 8, 24 ), new DateTime( 2021, 8, 25 ));
            var (tz1, tz2, tz3) = (
                ZonedDateTimeTestsData.GetTimeZone( "03.00", 3 ),
                ZonedDateTimeTestsData.GetTimeZone( "05.00", 5 ),
                ZonedDateTimeTestsData.GetTimeZone( "Other 03.00", 3 ));

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

        public static TheoryData<DateTime, TimeZoneInfo, DateTime, TimeZoneInfo, bool> GetContainsData(IFixture fixture)
        {
            var day = new DateTime( 2021, 8, 26 );
            var tz1 = ZonedDateTimeTestsData.GetTimeZone( "1", 1 );
            var tz3 = ZonedDateTimeTestsData.GetTimeZone( "3", 3 );

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

            var startTz = ZonedDateTimeTestsData.GetTimeZone(
                "1 (+DS)",
                1,
                new DateTime( 1, 6, 26, 2, 0, 0 ),
                new DateTime( 1, 8, 26, 0, 30, 0 ) );

            var endTz = ZonedDateTimeTestsData.GetTimeZone(
                "1 (+DS)",
                1,
                new DateTime( 1, 6, 26, 2, 0, 0 ),
                new DateTime( 1, 8, 27, 0, 30, 0 ) );

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
            var timeZone = ZonedDateTimeTestsData.GetTimeZone( "1", 1 );

            return new TheoryData<DateTime, TimeZoneInfo, int, DateTime>
            {
                { day, timeZone, 0, day },
                { day, timeZone, 1, new DateTime( 2021, 8, 27 ) },
                { day, timeZone, -1, new DateTime( 2021, 8, 25 ) },
                { day, timeZone, 10, new DateTime( 2021, 9, 5 ) },
                { day, timeZone, -10, new DateTime( 2021, 8, 16 ) }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, Core.Chrono.Period, DateTime> GetAddData(IFixture fixture)
        {
            var day = new DateTime( 2021, 8, 26 );
            var timeZone = ZonedDateTimeTestsData.GetTimeZone( "1", 1 );
            var timeZoneWithDaylightSaving = ZonedDateTimeTestsData.GetTimeZone(
                "1",
                1,
                new DateTime( 1, 9, 26, 2, 0, 0 ),
                new DateTime( 1, 10, 26, 3, 0, 0 ) );

            return new TheoryData<DateTime, TimeZoneInfo, Core.Chrono.Period, DateTime>
            {
                { day, timeZone, Core.Chrono.Period.Empty, day },
                { day, timeZone, Core.Chrono.Period.FromYears( 1 ), new DateTime( 2022, 8, 26 ) },
                { day, timeZone, Core.Chrono.Period.FromMonths( 1 ), new DateTime( 2021, 9, 26 ) },
                { day, timeZone, Core.Chrono.Period.FromWeeks( 1 ), new DateTime( 2021, 9, 2 ) },
                { day, timeZone, Core.Chrono.Period.FromDays( 1 ), new DateTime( 2021, 8, 27 ) },
                { day, timeZone, Core.Chrono.Period.FromHours( 23 ), day },
                { day, timeZone, Core.Chrono.Period.FromHours( 24 ), new DateTime( 2021, 8, 27 ) },
                { day, timeZone, Core.Chrono.Period.FromMinutes( 1439 ), day },
                { day, timeZone, Core.Chrono.Period.FromMinutes( 1440 ), new DateTime( 2021, 8, 27 ) },
                { day, timeZone, Core.Chrono.Period.FromSeconds( 86399 ), day },
                { day, timeZone, Core.Chrono.Period.FromSeconds( 86400 ), new DateTime( 2021, 8, 27 ) },
                { day, timeZone, Core.Chrono.Period.FromMilliseconds( 86399999 ), day },
                { day, timeZone, Core.Chrono.Period.FromMilliseconds( 86400000 ), new DateTime( 2021, 8, 27 ) },
                { day, timeZone, Core.Chrono.Period.FromTicks( 863999999999 ), day },
                { day, timeZone, Core.Chrono.Period.FromTicks( 864000000000 ), new DateTime( 2021, 8, 27 ) },
                { day, timeZone, new Core.Chrono.Period( 1, 2, 3, 4, 22, 90, 1700, 80000, 200000000 ), new DateTime( 2022, 11, 21 ) },
                { day, timeZoneWithDaylightSaving, Core.Chrono.Period.FromMonths( 1 ).AddHours( 2 ), new DateTime( 2021, 9, 26 ) },
                {
                    day,
                    timeZoneWithDaylightSaving,
                    Core.Chrono.Period.FromMonths( 1 ).AddHours( 3 ).SubtractTicks( 1 ),
                    new DateTime( 2021, 9, 26 )
                },
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, DateTime, TimeZoneInfo, PeriodUnits> GetGetPeriodOffsetData(IFixture fixture)
        {
            var day = new DateTime( 2021, 8, 26 );
            var otherDay = new DateTime( 2019, 10, 24 );
            var timeZone = ZonedDateTimeTestsData.GetTimeZone( "1", 1 );

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
            var timeZone = ZonedDateTimeTestsData.GetTimeZone( "1", 1 );

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
            var timeZone = ZonedDateTimeTestsData.GetTimeZone( "1", 1 );
            var timeZoneWithInvalidity = ZonedDateTimeTestsData.GetTimeZone(
                "1 (+DS)",
                1,
                DateTime.MinValue,
                new DateTime( 2020, 1, 1 ),
                new DateTime( 1, 8, 25, 23, 30, 0 ),
                new DateTime( 1, 10, 26, 2, 0, 0 ) );

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
            var timeZone = ZonedDateTimeTestsData.GetTimeZone( "1", 1 );

            return new TheoryData<DateTime, TimeZoneInfo, int>
            {
                { fixture.Create<DateTime>(), timeZone, DateTime.MinValue.Year - 1 },
                { fixture.Create<DateTime>(), timeZone, DateTime.MaxValue.Year + 1 }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, IsoMonthOfYear, DateTime> GetSetMonthData(IFixture fixture)
        {
            var timeZone = ZonedDateTimeTestsData.GetTimeZone( "1", 1 );
            var timeZoneWithInvalidity = ZonedDateTimeTestsData.GetTimeZone(
                "1 (+DS)",
                1,
                new DateTime( 1, 8, 25, 23, 30, 0 ),
                new DateTime( 1, 10, 26, 2, 0, 0 ) );

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
            var timeZone = ZonedDateTimeTestsData.GetTimeZone( "1", 1 );
            var timeZoneWithInvalidity = ZonedDateTimeTestsData.GetTimeZone(
                "1 (+DS)",
                1,
                new DateTime( 1, 8, 25, 23, 30, 0 ),
                new DateTime( 1, 10, 26, 2, 0, 0 ) );

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
            var timeZone = ZonedDateTimeTestsData.GetTimeZone( "1", 1 );

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
                { new DateTime( 2021, 1, 1 ), timeZone, Constants.DaysInJanuary + 1 },
                { new DateTime( 2021, 2, 1 ), timeZone, Constants.DaysInFebruary + 1 },
                { new DateTime( 2020, 2, 1 ), timeZone, Constants.DaysInLeapFebruary + 1 },
                { new DateTime( 2021, 3, 1 ), timeZone, Constants.DaysInMarch + 1 },
                { new DateTime( 2021, 4, 1 ), timeZone, Constants.DaysInApril + 1 },
                { new DateTime( 2021, 5, 1 ), timeZone, Constants.DaysInMay + 1 },
                { new DateTime( 2021, 6, 1 ), timeZone, Constants.DaysInJune + 1 },
                { new DateTime( 2021, 7, 1 ), timeZone, Constants.DaysInJuly + 1 },
                { new DateTime( 2021, 8, 1 ), timeZone, Constants.DaysInAugust + 1 },
                { new DateTime( 2021, 9, 1 ), timeZone, Constants.DaysInSeptember + 1 },
                { new DateTime( 2021, 10, 1 ), timeZone, Constants.DaysInOctober + 1 },
                { new DateTime( 2021, 11, 1 ), timeZone, Constants.DaysInNovember + 1 },
                { new DateTime( 2021, 12, 1 ), timeZone, Constants.DaysInDecember + 1 },
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, int, DateTime> GetSetDayOfYearData(IFixture fixture)
        {
            var timeZone = ZonedDateTimeTestsData.GetTimeZone( "1", 1 );
            var timeZoneWithInvalidity = ZonedDateTimeTestsData.GetTimeZone(
                "1 (+DS)",
                1,
                new DateTime( 1, 8, 25, 23, 30, 0 ),
                new DateTime( 1, 10, 26, 2, 0, 0 ) );

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
            var timeZone = ZonedDateTimeTestsData.GetTimeZone( "1", 1 );

            return new TheoryData<DateTime, TimeZoneInfo, int>
            {
                { new DateTime( 2021, 1, 1 ), timeZone, 0 },
                { new DateTime( 2021, 1, 1 ), timeZone, Constants.DaysInYear + 1 },
                { new DateTime( 2020, 1, 1 ), timeZone, Constants.DaysInLeapYear + 1 }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, Core.Chrono.TimeOfDay, DateTime> GetGetDateTimeData(IFixture fixture)
        {
            var day = new DateTime( 2021, 8, 26 );
            var timeZone = ZonedDateTimeTestsData.GetTimeZone( "1", 1 );
            var timeZoneWithInvalidity = ZonedDateTimeTestsData.GetTimeZone(
                "1 (+DS)",
                1,
                new DateTime( 1, 8, 26, 12, 0, 0 ),
                new DateTime( 1, 10, 26, 12, 0, 0 ) );

            return new TheoryData<DateTime, TimeZoneInfo, Core.Chrono.TimeOfDay, DateTime>
            {
                { day, timeZone, Core.Chrono.TimeOfDay.Start, day },
                { day, timeZone, Core.Chrono.TimeOfDay.Mid, new DateTime( 2021, 8, 26, 12, 0, 0 ) },
                { day, timeZone, Core.Chrono.TimeOfDay.End, new DateTime( 2021, 8, 26, 23, 59, 59, 999 ).AddTicks( 9999 ) },
                {
                    day,
                    timeZone,
                    new Core.Chrono.TimeOfDay( 12, 30, 40, 500, 6001 ),
                    new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 )
                },
                {
                    day,
                    timeZoneWithInvalidity,
                    new Core.Chrono.TimeOfDay( 11, 59, 59, 999, 9999 ),
                    new DateTime( 2021, 8, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
                },
                {
                    day,
                    timeZoneWithInvalidity,
                    new Core.Chrono.TimeOfDay( 13 ),
                    new DateTime( 2021, 8, 26, 13, 0, 0 )
                }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, Core.Chrono.TimeOfDay> GetGetDateTimeThrowData(IFixture fixture)
        {
            var day = new DateTime( 2021, 8, 26 );
            var timeZoneWithInvalidity = ZonedDateTimeTestsData.GetTimeZone(
                "1 (+DS)",
                1,
                new DateTime( 1, 8, 26, 12, 0, 0 ),
                new DateTime( 1, 10, 26, 12, 0, 0 ) );

            return new TheoryData<DateTime, TimeZoneInfo, Core.Chrono.TimeOfDay>
            {
                { day, timeZoneWithInvalidity, new Core.Chrono.TimeOfDay( 12 ) },
                { day, timeZoneWithInvalidity, new Core.Chrono.TimeOfDay( 12, 59, 59, 999, 9999 ) }
            };
        }

        public static TheoryData<DateTime, TimeZoneInfo, DateTime, DateTime> GetGetIntersectingInvalidityRangeData(IFixture fixture)
        {
            var positiveTimeZoneWithInvalidity = ZonedDateTimeTestsData.GetTimeZone(
                "1 (+DS)",
                1,
                new DateTime( 1, 8, 26, 12, 0, 0 ),
                new DateTime( 1, 10, 26, 12, 0, 0 ) );

            var negativeTimeZoneWithInvalidity = ZonedDateTimeTestsData.GetTimeZone(
                "1 (-DS)",
                1,
                new DateTime( 1, 8, 26, 12, 0, 0 ),
                new DateTime( 1, 10, 26, 12, 0, 0 ),
                daylightSavingOffsetInHours: -1 );

            var positiveTimeZoneWithInvalidMidnight = ZonedDateTimeTestsData.GetTimeZone(
                "1 (+DS)",
                1,
                new DateTime( 1, 8, 25, 23, 30, 0 ),
                new DateTime( 1, 10, 26, 12, 0, 0 ) );

            var negativeTimeZoneWithInvalidMidnight = ZonedDateTimeTestsData.GetTimeZone(
                "1 (-DS)",
                1,
                new DateTime( 1, 8, 26, 12, 0, 0 ),
                new DateTime( 1, 10, 25, 23, 30, 0 ),
                daylightSavingOffsetInHours: -1 );

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
            var timeZone = ZonedDateTimeTestsData.GetTimeZone( "1", 1 );
            var positiveTimeZoneWithInvalidity = ZonedDateTimeTestsData.GetTimeZone(
                "1 (+DS)",
                1,
                new DateTime( 1, 8, 26, 12, 0, 0 ),
                new DateTime( 1, 10, 26, 12, 0, 0 ) );

            var negativeTimeZoneWithInvalidity = ZonedDateTimeTestsData.GetTimeZone(
                "1 (-DS)",
                1,
                new DateTime( 1, 8, 26, 12, 0, 0 ),
                new DateTime( 1, 10, 26, 12, 0, 0 ),
                daylightSavingOffsetInHours: -1 );

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
            var positiveTimeZoneWithInvalidity = ZonedDateTimeTestsData.GetTimeZone(
                "1 (+DS)",
                1,
                new DateTime( 1, 8, 26, 12, 0, 0 ),
                new DateTime( 1, 10, 26, 12, 0, 0 ) );

            var negativeTimeZoneWithInvalidity = ZonedDateTimeTestsData.GetTimeZone(
                "1 (-DS)",
                1,
                new DateTime( 1, 8, 26, 12, 0, 0 ),
                new DateTime( 1, 10, 26, 12, 0, 0 ),
                daylightSavingOffsetInHours: -1 );

            var positiveTimeZoneWithAmbiguousMidnight = ZonedDateTimeTestsData.GetTimeZone(
                "1 (+DS)",
                1,
                new DateTime( 1, 8, 26, 12, 0, 0 ),
                new DateTime( 1, 10, 26, 0, 30, 0 ) );

            var negativeTimeZoneWithAmbiguousMidnight = ZonedDateTimeTestsData.GetTimeZone(
                "1 (-DS)",
                1,
                new DateTime( 1, 8, 26, 0, 30, 0 ),
                new DateTime( 1, 10, 26, 12, 0, 0 ),
                daylightSavingOffsetInHours: -1 );

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
            var timeZone = ZonedDateTimeTestsData.GetTimeZone( "1", 1 );
            var positiveTimeZoneWithInvalidity = ZonedDateTimeTestsData.GetTimeZone(
                "1 (+DS)",
                1,
                new DateTime( 1, 8, 26, 12, 0, 0 ),
                new DateTime( 1, 10, 26, 12, 0, 0 ) );

            var negativeTimeZoneWithInvalidity = ZonedDateTimeTestsData.GetTimeZone(
                "1 (-DS)",
                1,
                new DateTime( 1, 8, 26, 12, 0, 0 ),
                new DateTime( 1, 10, 26, 12, 0, 0 ),
                daylightSavingOffsetInHours: -1 );

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
}
