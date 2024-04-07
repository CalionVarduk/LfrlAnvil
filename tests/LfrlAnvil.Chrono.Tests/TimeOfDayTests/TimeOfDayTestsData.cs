using System.Collections.Generic;

namespace LfrlAnvil.Chrono.Tests.TimeOfDayTests;

public class TimeOfDayTestsData
{
    public static TheoryData<int, int, int, int, int, int> GetCtorWithTickPrecisionData(IFixture fixture)
    {
        return new TheoryData<int, int, int, int, int, int>
        {
            { 0, 0, 0, 0, 0, 0 },
            { 1, 1, 1, 1, 1, 1 },
            { 12, 34, 25, 698, 789, 5 },
            { 23, 59, 59, 999, 999, 9 },
            { 0, 0, 0, 0, 0, 1 },
            { 0, 0, 0, 0, 0, 4 },
            { 0, 0, 0, 0, 0, 9 },
            { 0, 0, 0, 1, 0, 0 },
            { 0, 0, 0, 1, 1, 1 },
            { 0, 0, 0, 1, 1, 4 },
            { 0, 0, 0, 1, 1, 9 },
            { 0, 0, 0, 567, 0, 0 },
            { 0, 0, 0, 567, 999, 9 }
        };
    }

    public static TheoryData<int, int, int, int, int, int> GetCtorWithTickPrecisionThrowData(IFixture fixture)
    {
        return new TheoryData<int, int, int, int, int, int>
        {
            { -1, 0, 0, 0, 0, 0 },
            { 0, -1, 0, 0, 0, 0 },
            { 0, 0, -1, 0, 0, 0 },
            { 0, 0, 0, -1, 0, 0 },
            { 0, 0, 0, 0, -1, 0 },
            { 0, 0, 0, 0, 0, -1 },
            { 24, 0, 0, 0, 0, 0 },
            { 0, 60, 0, 0, 0, 0 },
            { 0, 0, 60, 0, 0, 0 },
            { 0, 0, 0, 1000, 0, 0 },
            { 0, 0, 0, 0, 1000, 0 },
            { 0, 0, 0, 0, 0, 10 }
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
            ChronoConstants.TicksPerStandardDay - 2,
            ChronoConstants.TicksPerStandardDay - 1
        };
    }

    public static TheoryData<long> GetCtorWithTimeSpanThrowData(IFixture fixture)
    {
        return new TheoryData<long>
        {
            -2,
            -1,
            ChronoConstants.TicksPerStandardDay,
            ChronoConstants.TicksPerStandardDay + 1
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
            { ChronoConstants.TicksPerStandardDay - 2, "23h 59m 59.9999998s" },
            { ChronoConstants.TicksPerStandardDay - 1, "23h 59m 59.9999999s" }
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
            ChronoConstants.TicksPerStandardDay - 2,
            ChronoConstants.TicksPerStandardDay - 1
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
            { 1, ChronoConstants.TicksPerStandardDay - 1 },
            { 15671234, ChronoConstants.TicksPerStandardDay - 15671234 },
            { 971234567, ChronoConstants.TicksPerStandardDay - 971234567 },
            { 475398765432, ChronoConstants.TicksPerStandardDay - 475398765432 },
            { ChronoConstants.TicksPerStandardDay - 2, 2 },
            { ChronoConstants.TicksPerStandardDay - 1, 1 }
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

    public static TheoryData<long, long> GetTrimToMicrosecondData(IFixture fixture)
    {
        return new TheoryData<long, long>
        {
            { 0, 0 },
            { 1, 0 },
            { 9, 0 },
            { 10, 10 },
            { 11, 10 },
            { 29, 20 }
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
            ( int )ChronoConstants.TicksPerMicrosecond,
            ( int )ChronoConstants.TicksPerMicrosecond + 1,
            ( int )ChronoConstants.TicksPerMicrosecond + 2
        };
    }

    public static TheoryData<long, int, long> GetSetTickData(IFixture fixture)
    {
        return new TheoryData<long, int, long>
        {
            { 0, 0, 0 },
            { 0, 4, 4 },
            { 4, 0, 0 },
            { 4, 9, 9 },
            { 36630046789, 0, 36630046780 },
            { 36630046780, 9, 36630046789 },
            { 36630046789, 4, 36630046784 }
        };
    }

    public static TheoryData<int> GetSetMicrosecondThrowData(IFixture fixture)
    {
        return new TheoryData<int>
        {
            -1,
            -2,
            ChronoConstants.MicrosecondsPerMillisecond,
            ChronoConstants.MicrosecondsPerMillisecond + 1,
            ChronoConstants.MicrosecondsPerMillisecond + 2
        };
    }

    public static TheoryData<long, int, long> GetSetMicrosecondData(IFixture fixture)
    {
        return new TheoryData<long, int, long>
        {
            { 0, 0, 0 },
            { 0, 456, 4560 },
            { 1230, 0, 0 },
            { 1230, 999, 9990 },
            { 36637895, 0, 36630005 },
            { 36637895, 999, 36639995 },
            { 36637895, 123, 36631235 }
        };
    }

    public static TheoryData<int> GetSetMillisecondThrowData(IFixture fixture)
    {
        return new TheoryData<int>
        {
            -1,
            -2,
            ChronoConstants.MillisecondsPerSecond,
            ChronoConstants.MillisecondsPerSecond + 1,
            ChronoConstants.MillisecondsPerSecond + 2
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
            ChronoConstants.SecondsPerMinute,
            ChronoConstants.SecondsPerMinute + 1,
            ChronoConstants.SecondsPerMinute + 2
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
            ChronoConstants.MinutesPerHour,
            ChronoConstants.MinutesPerHour + 1,
            ChronoConstants.MinutesPerHour + 2
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
            ChronoConstants.HoursPerStandardDay,
            ChronoConstants.HoursPerStandardDay + 1,
            ChronoConstants.HoursPerStandardDay + 2
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
            ChronoConstants.TicksPerStandardDay - 2,
            ChronoConstants.TicksPerStandardDay - 1
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
