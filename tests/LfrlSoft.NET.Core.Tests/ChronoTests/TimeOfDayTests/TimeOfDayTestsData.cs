using System.Collections.Generic;
using AutoFixture;
using LfrlSoft.NET.Core.Chrono;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.ChronoTests.TimeOfDayTests
{
    public class TimeOfDayTestsData
    {
        public static TheoryData<int> GetCtorWithHourPrecisionData(IFixture fixture)
        {
            return new TheoryData<int>
            {
                0,
                11,
                23
            };
        }

        public static TheoryData<int> GetCtorWithHourPrecisionThrowData(IFixture fixture)
        {
            return new TheoryData<int>
            {
                -2,
                -1,
                24,
                25
            };
        }

        public static TheoryData<int, int> GetCtorWithMinutePrecisionData(IFixture fixture)
        {
            return new TheoryData<int, int>
            {
                { 0, 0 },
                { 0, 1 },
                { 0, 31 },
                { 0, 59 },
                { 11, 0 },
                { 11, 1 },
                { 11, 31 },
                { 11, 59 },
                { 23, 0 },
                { 23, 1 },
                { 23, 31 },
                { 23, 59 }
            };
        }

        public static TheoryData<int, int> GetCtorWithMinutePrecisionThrowData(IFixture fixture)
        {
            return new TheoryData<int, int>
            {
                { -2, 31 },
                { -1, 31 },
                { 11, -2 },
                { 11, -1 },
                { 11, 60 },
                { 11, 61 },
                { 24, 31 },
                { 25, 31 }
            };
        }

        public static TheoryData<int, int, int> GetCtorWithSecondPrecisionData(IFixture fixture)
        {
            return new TheoryData<int, int, int>
            {
                { 0, 0, 0 },
                { 0, 0, 1 },
                { 0, 0, 42 },
                { 0, 0, 59 },
                { 0, 1, 0 },
                { 0, 1, 1 },
                { 0, 1, 42 },
                { 0, 1, 59 },
                { 0, 31, 0 },
                { 0, 31, 1 },
                { 0, 31, 42 },
                { 0, 31, 59 },
                { 0, 59, 0 },
                { 0, 59, 1 },
                { 0, 59, 42 },
                { 0, 59, 59 },
                { 23, 0, 0 },
                { 23, 0, 1 },
                { 23, 0, 42 },
                { 23, 0, 59 },
                { 23, 1, 0 },
                { 23, 1, 1 },
                { 23, 1, 42 },
                { 23, 1, 59 },
                { 23, 31, 0 },
                { 23, 31, 1 },
                { 23, 31, 42 },
                { 23, 31, 59 },
                { 23, 59, 0 },
                { 23, 59, 1 },
                { 23, 59, 42 },
                { 23, 59, 59 }
            };
        }

        public static TheoryData<int, int, int> GetCtorWithSecondPrecisionThrowData(IFixture fixture)
        {
            return new TheoryData<int, int, int>
            {
                { -2, 31, 42 },
                { -1, 31, 42 },
                { 11, -2, 42 },
                { 11, -1, 42 },
                { 11, 31, -2 },
                { 11, 31, -1 },
                { 11, 31, 60 },
                { 11, 31, 61 },
                { 11, 60, 42 },
                { 11, 61, 42 },
                { 24, 31, 42 },
                { 25, 31, 42 }
            };
        }

        public static TheoryData<int, int, int, int> GetCtorWithMsPrecisionData(IFixture fixture)
        {
            return new TheoryData<int, int, int, int>
            {
                { 0, 0, 0, 0 },
                { 0, 0, 0, 1 },
                { 0, 0, 0, 567 },
                { 0, 0, 0, 999 },
                { 0, 0, 1, 0 },
                { 0, 0, 1, 1 },
                { 0, 0, 1, 567 },
                { 0, 0, 1, 999 },
                { 0, 0, 42, 0 },
                { 0, 0, 42, 1 },
                { 0, 0, 42, 567 },
                { 0, 0, 42, 999 },
                { 0, 0, 59, 0 },
                { 0, 0, 59, 1 },
                { 0, 0, 59, 567 },
                { 0, 0, 59, 999 },
                { 0, 59, 0, 0 },
                { 0, 59, 0, 1 },
                { 0, 59, 0, 567 },
                { 0, 59, 0, 999 },
                { 0, 59, 1, 0 },
                { 0, 59, 1, 1 },
                { 0, 59, 1, 567 },
                { 0, 59, 1, 999 },
                { 0, 59, 42, 0 },
                { 0, 59, 42, 1 },
                { 0, 59, 42, 567 },
                { 0, 59, 42, 999 },
                { 0, 59, 59, 0 },
                { 0, 59, 59, 1 },
                { 0, 59, 59, 567 },
                { 0, 59, 59, 999 },
                { 23, 0, 0, 0 },
                { 23, 0, 0, 1 },
                { 23, 0, 0, 567 },
                { 23, 0, 0, 999 },
                { 23, 0, 1, 0 },
                { 23, 0, 1, 1 },
                { 23, 0, 1, 567 },
                { 23, 0, 1, 999 },
                { 23, 0, 42, 0 },
                { 23, 0, 42, 1 },
                { 23, 0, 42, 567 },
                { 23, 0, 42, 999 },
                { 23, 0, 59, 0 },
                { 23, 0, 59, 1 },
                { 23, 0, 59, 567 },
                { 23, 0, 59, 999 },
                { 23, 59, 0, 0 },
                { 23, 59, 0, 1 },
                { 23, 59, 0, 567 },
                { 23, 59, 0, 999 },
                { 23, 59, 1, 0 },
                { 23, 59, 1, 1 },
                { 23, 59, 1, 567 },
                { 23, 59, 1, 999 },
                { 23, 59, 42, 0 },
                { 23, 59, 42, 1 },
                { 23, 59, 42, 567 },
                { 23, 59, 42, 999 },
                { 23, 59, 59, 0 },
                { 23, 59, 59, 1 },
                { 23, 59, 59, 567 },
                { 23, 59, 59, 999 },
            };
        }

        public static TheoryData<int, int, int, int> GetCtorWithMsPrecisionThrowData(IFixture fixture)
        {
            return new TheoryData<int, int, int, int>
            {
                { -2, 31, 42, 567 },
                { -1, 31, 42, 567 },
                { 11, -2, 42, 567 },
                { 11, -1, 42, 567 },
                { 11, 31, -2, 567 },
                { 11, 31, -1, 567 },
                { 11, 31, 42, -2 },
                { 11, 31, 42, -1 },
                { 11, 31, 42, 1000 },
                { 11, 31, 42, 1001 },
                { 11, 31, 60, 567 },
                { 11, 31, 61, 567 },
                { 11, 60, 42, 567 },
                { 11, 61, 42, 567 },
                { 24, 31, 42, 567 },
                { 25, 31, 42, 567 }
            };
        }

        public static TheoryData<int, int, int, int, int> GetCtorWithTickPrecisionData(IFixture fixture)
        {
            return new TheoryData<int, int, int, int, int>
            {
                { 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 1 },
                { 0, 0, 0, 0, 1234 },
                { 0, 0, 0, 0, 9999 },
                { 0, 0, 0, 1, 0 },
                { 0, 0, 0, 1, 1 },
                { 0, 0, 0, 1, 1234 },
                { 0, 0, 0, 1, 9999 },
                { 0, 0, 0, 567, 0 },
                { 0, 0, 0, 567, 1 },
                { 0, 0, 0, 567, 1234 },
                { 0, 0, 0, 567, 9999 },
                { 0, 0, 0, 999, 0 },
                { 0, 0, 0, 999, 1 },
                { 0, 0, 0, 999, 1234 },
                { 0, 0, 0, 999, 9999 },
                { 0, 0, 59, 0, 0 },
                { 0, 0, 59, 0, 1 },
                { 0, 0, 59, 0, 1234 },
                { 0, 0, 59, 0, 9999 },
                { 0, 0, 59, 1, 0 },
                { 0, 0, 59, 1, 1 },
                { 0, 0, 59, 1, 1234 },
                { 0, 0, 59, 1, 9999 },
                { 0, 0, 59, 567, 0 },
                { 0, 0, 59, 567, 1 },
                { 0, 0, 59, 567, 1234 },
                { 0, 0, 59, 567, 9999 },
                { 0, 0, 59, 999, 0 },
                { 0, 0, 59, 999, 1 },
                { 0, 0, 59, 999, 1234 },
                { 0, 0, 59, 999, 9999 },
                { 0, 59, 0, 0, 0 },
                { 0, 59, 0, 0, 1 },
                { 0, 59, 0, 0, 1234 },
                { 0, 59, 0, 0, 9999 },
                { 0, 59, 0, 1, 0 },
                { 0, 59, 0, 1, 1 },
                { 0, 59, 0, 1, 1234 },
                { 0, 59, 0, 1, 9999 },
                { 0, 59, 0, 567, 0 },
                { 0, 59, 0, 567, 1 },
                { 0, 59, 0, 567, 1234 },
                { 0, 59, 0, 567, 9999 },
                { 0, 59, 0, 999, 0 },
                { 0, 59, 0, 999, 1 },
                { 0, 59, 0, 999, 1234 },
                { 0, 59, 0, 999, 9999 },
                { 0, 59, 59, 0, 0 },
                { 0, 59, 59, 0, 1 },
                { 0, 59, 59, 0, 1234 },
                { 0, 59, 59, 0, 9999 },
                { 0, 59, 59, 1, 0 },
                { 0, 59, 59, 1, 1 },
                { 0, 59, 59, 1, 1234 },
                { 0, 59, 59, 1, 9999 },
                { 0, 59, 59, 567, 0 },
                { 0, 59, 59, 567, 1 },
                { 0, 59, 59, 567, 1234 },
                { 0, 59, 59, 567, 9999 },
                { 0, 59, 59, 999, 0 },
                { 0, 59, 59, 999, 1 },
                { 0, 59, 59, 999, 1234 },
                { 0, 59, 59, 999, 9999 },
                { 23, 0, 0, 0, 0 },
                { 23, 0, 0, 0, 1 },
                { 23, 0, 0, 0, 1234 },
                { 23, 0, 0, 0, 9999 },
                { 23, 0, 0, 1, 0 },
                { 23, 0, 0, 1, 1 },
                { 23, 0, 0, 1, 1234 },
                { 23, 0, 0, 1, 9999 },
                { 23, 0, 0, 567, 0 },
                { 23, 0, 0, 567, 1 },
                { 23, 0, 0, 567, 1234 },
                { 23, 0, 0, 567, 9999 },
                { 23, 0, 0, 999, 0 },
                { 23, 0, 0, 999, 1 },
                { 23, 0, 0, 999, 1234 },
                { 23, 0, 0, 999, 9999 },
                { 23, 0, 59, 0, 0 },
                { 23, 0, 59, 0, 1 },
                { 23, 0, 59, 0, 1234 },
                { 23, 0, 59, 0, 9999 },
                { 23, 0, 59, 1, 0 },
                { 23, 0, 59, 1, 1 },
                { 23, 0, 59, 1, 1234 },
                { 23, 0, 59, 1, 9999 },
                { 23, 0, 59, 567, 0 },
                { 23, 0, 59, 567, 1 },
                { 23, 0, 59, 567, 1234 },
                { 23, 0, 59, 567, 9999 },
                { 23, 0, 59, 999, 0 },
                { 23, 0, 59, 999, 1 },
                { 23, 0, 59, 999, 1234 },
                { 23, 0, 59, 999, 9999 },
                { 23, 59, 0, 0, 0 },
                { 23, 59, 0, 0, 1 },
                { 23, 59, 0, 0, 1234 },
                { 23, 59, 0, 0, 9999 },
                { 23, 59, 0, 1, 0 },
                { 23, 59, 0, 1, 1 },
                { 23, 59, 0, 1, 1234 },
                { 23, 59, 0, 1, 9999 },
                { 23, 59, 0, 567, 0 },
                { 23, 59, 0, 567, 1 },
                { 23, 59, 0, 567, 1234 },
                { 23, 59, 0, 567, 9999 },
                { 23, 59, 0, 999, 0 },
                { 23, 59, 0, 999, 1 },
                { 23, 59, 0, 999, 1234 },
                { 23, 59, 0, 999, 9999 },
                { 23, 59, 59, 0, 0 },
                { 23, 59, 59, 0, 1 },
                { 23, 59, 59, 0, 1234 },
                { 23, 59, 59, 0, 9999 },
                { 23, 59, 59, 1, 0 },
                { 23, 59, 59, 1, 1 },
                { 23, 59, 59, 1, 1234 },
                { 23, 59, 59, 1, 9999 },
                { 23, 59, 59, 567, 0 },
                { 23, 59, 59, 567, 1 },
                { 23, 59, 59, 567, 1234 },
                { 23, 59, 59, 567, 9999 },
                { 23, 59, 59, 999, 0 },
                { 23, 59, 59, 999, 1 },
                { 23, 59, 59, 999, 1234 },
                { 23, 59, 59, 999, 9999 },
            };
        }

        public static TheoryData<int, int, int, int, int> GetCtorWithTickPrecisionThrowData(IFixture fixture)
        {
            return new TheoryData<int, int, int, int, int>
            {
                { -2, 31, 42, 567, 1234 },
                { -1, 31, 42, 567, 1234 },
                { 11, -2, 42, 567, 1234 },
                { 11, -1, 42, 567, 1234 },
                { 11, 31, -2, 567, 1234 },
                { 11, 31, -1, 567, 1234 },
                { 11, 31, 42, -2, 1234 },
                { 11, 31, 42, -1, 1234 },
                { 11, 31, 42, 567, -2 },
                { 11, 31, 42, 567, -1 },
                { 11, 31, 42, 567, 10000 },
                { 11, 31, 42, 567, 10001 },
                { 11, 31, 42, 1000, 1234 },
                { 11, 31, 42, 1001, 1234 },
                { 11, 31, 60, 567, 1234 },
                { 11, 31, 61, 567, 1234 },
                { 11, 60, 42, 567, 1234 },
                { 11, 61, 42, 567, 1234 },
                { 24, 31, 42, 567, 1234 },
                { 25, 31, 42, 567, 1234 }
            };
        }

        public static TheoryData<long> GetCtorWithTimeSpanData(IFixture fixture)
        {
            return new TheoryData<long>
            {
                0,
                1,
                15671234,
                971234567,
                475398765432,
                Constants.TicksPerDay - 2,
                Constants.TicksPerDay - 1
            };
        }

        public static TheoryData<long> GetCtorWithTimeSpanThrowData(IFixture fixture)
        {
            return new TheoryData<long>
            {
                -2,
                -1,
                Constants.TicksPerDay,
                Constants.TicksPerDay + 1
            };
        }

        public static TheoryData<long, string> GetToStringData(IFixture fixture)
        {
            return new TheoryData<long, string>
            {
                { 0, "00h 00m 00.0000000s" },
                { 1, "00h 00m 00.0000001s" },
                { 15671234, "00h 00m 01.5671234s" },
                { 971234567, "00h 01m 37.1234567s" },
                { 475398765432, "13h 12m 19.8765432s" },
                { Constants.TicksPerDay - 2, "23h 59m 59.9999998s" },
                { Constants.TicksPerDay - 1, "23h 59m 59.9999999s" }
            };
        }

        public static TheoryData<long> GetGetHashCodeData(IFixture fixture)
        {
            return new TheoryData<long>
            {
                0,
                1,
                15671234,
                971234567,
                475398765432,
                Constants.TicksPerDay - 2,
                Constants.TicksPerDay - 1
            };
        }

        public static TheoryData<long, long, bool> GetEqualsData(IFixture fixture)
        {
            return new TheoryData<long, long, bool>
            {
                { 0, 0, true },
                { 0, 1, false },
                { 1, 0, false }
            };
        }

        public static TheoryData<long, long, int> GetCompareToData(IFixture fixture)
        {
            return new TheoryData<long, long, int>
            {
                { 0, 0, 0 },
                { 0, 1, -1 },
                { 1, 0, 1 }
            };
        }

        public static TheoryData<long, long> GetInvertData(IFixture fixture)
        {
            return new TheoryData<long, long>
            {
                { 1, Constants.TicksPerDay - 1 },
                { 15671234, Constants.TicksPerDay - 15671234 },
                { 971234567, Constants.TicksPerDay - 971234567 },
                { 475398765432, Constants.TicksPerDay - 475398765432 },
                { Constants.TicksPerDay - 2, 2 },
                { Constants.TicksPerDay - 1, 1 }
            };
        }

        public static TheoryData<long, long, long> GetSubtractData(IFixture fixture)
        {
            return new TheoryData<long, long, long>
            {
                { 0, 0, 0 },
                { 3, 7, -4 },
                { 9, 2, 7 }
            };
        }

        public static TheoryData<long, long> GetTrimToMillisecondData(IFixture fixture)
        {
            return new TheoryData<long, long>
            {
                { 0, 0 },
                { 1, 0 },
                { 9999, 0 },
                { 10000, 10000 },
                { 10001, 10000 },
                { 29999, 20000 }
            };
        }

        public static TheoryData<long, long> GetTrimToSecondData(IFixture fixture)
        {
            return new TheoryData<long, long>
            {
                { 0, 0 },
                { 1, 0 },
                { 9999999, 0 },
                { 10000000, 10000000 },
                { 10000001, 10000000 },
                { 29999999, 20000000 }
            };
        }

        public static TheoryData<long, long> GetTrimToMinuteData(IFixture fixture)
        {
            return new TheoryData<long, long>
            {
                { 0, 0 },
                { 1, 0 },
                { 599999999, 0 },
                { 600000000, 600000000 },
                { 600000001, 600000000 },
                { 1799999999, 1200000000 }
            };
        }

        public static TheoryData<long, long> GetTrimToHourData(IFixture fixture)
        {
            return new TheoryData<long, long>
            {
                { 0, 0 },
                { 1, 0 },
                { 35999999999, 0 },
                { 36000000000, 36000000000 },
                { 36000000001, 36000000000 },
                { 107999999999, 72000000000 }
            };
        }

        public static TheoryData<int> GetSetTickThrowData(IFixture fixture)
        {
            return new TheoryData<int>
            {
                -1,
                -2,
                (int)Constants.TicksPerMillisecond,
                (int)Constants.TicksPerMillisecond + 1,
                (int)Constants.TicksPerMillisecond + 2
            };
        }

        public static TheoryData<long, int, long> GetSetTickData(IFixture fixture)
        {
            return new TheoryData<long, int, long>
            {
                { 0, 0, 0 },
                { 0, 4567, 4567 },
                { 1234, 0, 0 },
                { 1234, 9999, 9999 },
                { 36630046789, 0, 36630040000 },
                { 36630046789, 9999, 36630049999 },
                { 36630046789, 1234, 36630041234 }
            };
        }

        public static TheoryData<int> GetSetMillisecondThrowData(IFixture fixture)
        {
            return new TheoryData<int>
            {
                -1,
                -2,
                Constants.MillisecondsPerSecond,
                Constants.MillisecondsPerSecond + 1,
                Constants.MillisecondsPerSecond + 2
            };
        }

        public static TheoryData<long, int, long> GetSetMillisecondData(IFixture fixture)
        {
            return new TheoryData<long, int, long>
            {
                { 0, 0, 0 },
                { 0, 456, 4560000 },
                { 1230000, 0, 0 },
                { 1230000, 999, 9990000 },
                { 36637890005, 0, 36630000005 },
                { 36637890005, 999, 36639990005 },
                { 36637890005, 123, 36631230005 }
            };
        }

        public static TheoryData<int> GetSetSecondThrowData(IFixture fixture)
        {
            return new TheoryData<int>
            {
                -1,
                -2,
                Constants.SecondsPerMinute,
                Constants.SecondsPerMinute + 1,
                Constants.SecondsPerMinute + 2
            };
        }

        public static TheoryData<long, int, long> GetSetSecondData(IFixture fixture)
        {
            return new TheoryData<long, int, long>
            {
                { 0, 0, 0 },
                { 0, 45, 450000000 },
                { 120000000, 0, 0 },
                { 120000000, 59, 590000000 },
                { 36940040005, 0, 36600040005 },
                { 36940040005, 59, 37190040005 },
                { 36940040005, 12, 36720040005 }
            };
        }

        public static TheoryData<int> GetSetMinuteThrowData(IFixture fixture)
        {
            return new TheoryData<int>
            {
                -1,
                -2,
                Constants.MinutesPerHour,
                Constants.MinutesPerHour + 1,
                Constants.MinutesPerHour + 2
            };
        }

        public static TheoryData<long, int, long> GetSetMinuteData(IFixture fixture)
        {
            return new TheoryData<long, int, long>
            {
                { 0, 0, 0 },
                { 0, 45, 27000000000 },
                { 7200000000, 0, 0 },
                { 7200000000, 59, 35400000000 },
                { 56430040005, 0, 36030040005 },
                { 56430040005, 59, 71430040005 },
                { 56430040005, 12, 43230040005 }
            };
        }

        public static TheoryData<int> GetSetHourThrowData(IFixture fixture)
        {
            return new TheoryData<int>
            {
                -1,
                -2,
                Constants.HoursPerDay,
                Constants.HoursPerDay + 1,
                Constants.HoursPerDay + 2
            };
        }

        public static TheoryData<long, int, long> GetSetHourData(IFixture fixture)
        {
            return new TheoryData<long, int, long>
            {
                { 0, 0, 0 },
                { 0, 12, 432000000000 },
                { 108000000000, 0, 0 },
                { 108000000000, 23, 828000000000 },
                { 109230040005, 0, 1230040005 },
                { 109230040005, 23, 829230040005 },
                { 109230040005, 12, 433230040005 }
            };
        }

        public static TheoryData<long> GetConversionOperatorData(IFixture fixture)
        {
            return new TheoryData<long>
            {
                0,
                1,
                15671234,
                971234567,
                475398765432,
                Constants.TicksPerDay - 2,
                Constants.TicksPerDay - 1
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
