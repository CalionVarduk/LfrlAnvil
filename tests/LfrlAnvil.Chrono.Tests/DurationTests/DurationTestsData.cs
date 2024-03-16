using System.Collections.Generic;

namespace LfrlAnvil.Chrono.Tests.DurationTests;

public class DurationTestsData
{
    public static TheoryData<long> GetTicksData(IFixture fixture)
    {
        return new TheoryData<long>
        {
            0,
            -1000001,
            1234567
        };
    }

    public static TheoryData<int, int, int, int, int, int, long> GetCtorWithTicksPrecisionData(IFixture fixture)
    {
        return new TheoryData<int, int, int, int, int, int, long>
        {
            { 3, 111, 321, 987, 12345, 6, 177819993456 },
            { -4, 40, 1, 3456, 7, 89, -119955439841 },
            { 78, -765, -303, -12345, -9876, -543, 2345846450697 }
        };
    }

    public static TheoryData<TimeSpan> GetCtorWithTimeSpanData(IFixture fixture)
    {
        return new TheoryData<TimeSpan>
        {
            TimeSpan.Zero,
            TimeSpan.MinValue,
            TimeSpan.MaxValue,
            new TimeSpan( 2, 3, 40, 50, 678 ),
            new TimeSpan( -1, -2, -3, -4, -5 )
        };
    }

    public static TheoryData<int, int, long> GetFullMicrosecondsData(IFixture fixture)
    {
        return new TheoryData<int, int, long>
        {
            { 300, 0, 300 },
            { 400, 5, 400 },
            { 999, 9, 999 },
            { 12345, 4, 12345 },
            { -300, 0, -300 },
            { -400, -5, -400 },
            { -999, -9, -999 },
            { -12345, -4, -12345 }
        };
    }

    public static TheoryData<int, int, int, long> GetFullMillisecondsData(IFixture fixture)
    {
        return new TheoryData<int, int, int, long>
        {
            { 300, 0, 0, 300 },
            { 400, 500, 0, 400 },
            { 999, 999, 9, 999 },
            { 12345, 123, 4, 12345 },
            { -300, 0, 0, -300 },
            { -400, -500, 0, -400 },
            { -999, -999, -9, -999 },
            { -12345, -123, -4, -12345 }
        };
    }

    public static TheoryData<int, int, int, int, long> GetFullSecondsData(IFixture fixture)
    {
        return new TheoryData<int, int, int, int, long>
        {
            { 30, 0, 0, 0, 30 },
            { 40, 400, 500, 0, 40 },
            { 59, 999, 999, 9, 59 },
            { 12345, 123, 456, 7, 12345 },
            { -30, 0, 0, 0, -30 },
            { -40, -400, -500, 0, -40 },
            { -59, -999, -999, -9, -59 },
            { -12345, -123, -456, -7, -12345 }
        };
    }

    public static TheoryData<int, int, int, int, int, long> GetFullMinutesData(IFixture fixture)
    {
        return new TheoryData<int, int, int, int, int, long>
        {
            { 30, 0, 0, 0, 0, 30 },
            { 50, 40, 400, 500, 0, 50 },
            { 59, 59, 999, 999, 9, 59 },
            { 12345, 12, 123, 456, 7, 12345 },
            { -30, 0, 0, 0, 0, -30 },
            { -50, -40, -400, -500, 0, -50 },
            { -59, -59, -999, -999, -9, -59 },
            { -12345, -12, -123, -456, -7, -12345 }
        };
    }

    public static TheoryData<int, int, int, int, int, int, long> GetFullHoursData(IFixture fixture)
    {
        return new TheoryData<int, int, int, int, int, int, long>
        {
            { 30, 0, 0, 0, 0, 0, 30 },
            { 70, 50, 40, 400, 500, 0, 70 },
            { 25, 59, 59, 999, 999, 9, 25 },
            { 12345, 34, 12, 123, 456, 7, 12345 },
            { -30, 0, 0, 0, 0, 0, -30 },
            { -70, -50, -40, -400, -500, 0, -70 },
            { -25, -59, -59, -999, -999, -9, -25 },
            { -12345, -34, -12, -123, -456, -7, -12345 }
        };
    }

    public static TheoryData<int, int, int> GetTicksInMicrosecondData(IFixture fixture)
    {
        return new TheoryData<int, int, int>
        {
            { 300, 0, 0 },
            { 400, 5, 5 },
            { 999, 9, 9 },
            { 12345, 4, 4 },
            { -300, 0, 0 },
            { -400, -5, -5 },
            { -999, -9, -9 },
            { -12345, -4, -4 }
        };
    }

    public static TheoryData<int, int, int, int> GetMicrosecondsInMillisecondData(IFixture fixture)
    {
        return new TheoryData<int, int, int, int>
        {
            { 300, 0, 0, 0 },
            { 400, 400, 5, 400 },
            { 999, 999, 9, 999 },
            { 12345, 123, 4, 123 },
            { -300, 0, 0, 0 },
            { -400, -400, -5, -400 },
            { -999, -999, -9, -999 },
            { -12345, -123, -4, -123 }
        };
    }

    public static TheoryData<int, int, int, int, int> GetMillisecondsInSecondData(IFixture fixture)
    {
        return new TheoryData<int, int, int, int, int>
        {
            { 30, 0, 0, 0, 0 },
            { 40, 400, 500, 0, 400 },
            { 59, 999, 999, 9, 999 },
            { 12345, 123, 456, 7, 123 },
            { -30, 0, 0, 0, 0 },
            { -40, -400, -500, 0, -400 },
            { -59, -999, -999, -9, -999 },
            { -12345, -123, -456, -7, -123 }
        };
    }

    public static TheoryData<int, int, int, int, int, int> GetSecondsInMinuteData(IFixture fixture)
    {
        return new TheoryData<int, int, int, int, int, int>
        {
            { 30, 0, 0, 0, 0, 0 },
            { 50, 40, 400, 500, 0, 40 },
            { 59, 59, 999, 999, 9, 59 },
            { 12345, 12, 123, 456, 7, 12 },
            { -30, 0, 0, 0, 0, 0 },
            { -50, -40, -400, -500, 0, -40 },
            { -59, -59, -999, -999, -9, -59 },
            { -12345, -12, -123, -456, -7, -12 }
        };
    }

    public static TheoryData<int, int, int, int, int, int, int> GetMinutesInHourData(IFixture fixture)
    {
        return new TheoryData<int, int, int, int, int, int, int>
        {
            { 30, 0, 0, 0, 0, 0, 0 },
            { 70, 50, 40, 400, 500, 0, 50 },
            { 25, 59, 59, 999, 999, 9, 59 },
            { 12345, 34, 12, 123, 456, 7, 34 },
            { -30, 0, 0, 0, 0, 0, 0 },
            { -70, -50, -40, -400, -500, 0, -50 },
            { -25, -59, -59, -999, -999, -9, -59 },
            { -12345, -34, -12, -123, -456, -7, -34 }
        };
    }

    public static TheoryData<int, int, double> GetTotalMicrosecondsData(IFixture fixture)
    {
        return new TheoryData<int, int, double>
        {
            { 300, 0, 300.0 },
            { 400, 5, 400.5 },
            { 999, 9, 999.9 },
            { 12345, 4, 12345.4 },
            { -300, 0, -300.0 },
            { -400, -5, -400.5 },
            { -999, -9, -999.9 },
            { -12345, -4, -12345.4 }
        };
    }

    public static TheoryData<int, int, int, double> GetTotalMillisecondsData(IFixture fixture)
    {
        return new TheoryData<int, int, int, double>
        {
            { 300, 0, 0, 300.0 },
            { 400, 500, 0, 400.5 },
            { 999, 999, 9, 999.9999 },
            { 12345, 123, 4, 12345.1234 },
            { -300, 0, 0, -300.0 },
            { -400, -500, 0, -400.5 },
            { -999, -999, -9, -999.9999 },
            { -12345, -123, -4, -12345.1234 }
        };
    }

    public static TheoryData<int, int, int, int, double> GetTotalSecondsData(IFixture fixture)
    {
        return new TheoryData<int, int, int, int, double>
        {
            { 30, 0, 0, 0, 30.0 },
            { 40, 400, 500, 0, 40.4005 },
            { 59, 999, 999, 9, 59.9999999 },
            { 12345, 123, 456, 7, 12345.1234567 },
            { -30, 0, 0, 0, -30.0 },
            { -40, -400, -500, 0, -40.4005 },
            { -59, -999, -999, -9, -59.9999999 },
            { -12345, -123, -456, -7, -12345.1234567 }
        };
    }

    public static TheoryData<int, int, int, int, int, double> GetTotalMinutesData(IFixture fixture)
    {
        return new TheoryData<int, int, int, int, int, double>
        {
            { 30, 0, 0, 0, 0, 30.0 },
            { 50, 40, 400, 500, 0, 50.67334166667 },
            { 59, 59, 999, 999, 9, 59.99999999833 },
            { 12345, 12, 123, 456, 7, 12345.20205761167 },
            { -30, 0, 0, 0, 0, -30.0 },
            { -50, -40, -400, -500, 0, -50.67334166667 },
            { -59, -59, -999, -999, -9, -59.99999999833 },
            { -12345, -12, -123, -456, -7, -12345.20205761167 }
        };
    }

    public static TheoryData<int, int, int, int, int, int, double> GetTotalHoursData(IFixture fixture)
    {
        return new TheoryData<int, int, int, int, int, int, double>
        {
            { 30, 0, 0, 0, 0, 0, 30.0 },
            { 70, 50, 40, 400, 500, 0, 70.84455569444 },
            { 25, 59, 59, 999, 999, 9, 25.99999999997 },
            { 12345, 34, 12, 123, 456, 7, 12345.57003429353 },
            { -30, 0, 0, 0, 0, 0, -30.0 },
            { -70, -50, -40, -400, -500, 0, -70.84455569444 },
            { -25, -59, -59, -999, -999, -9, -25.99999999997 },
            { -12345, -34, -12, -123, -456, -7, -12345.57003429353 }
        };
    }

    public static TheoryData<double, long> GetFromMicrosecondsWithDoubleData(IFixture fixture)
    {
        return new TheoryData<double, long>
        {
            { 300.0, 3000 },
            { 400.5, 4005 },
            { 999.949999, 9999 },
            { 999.950001, 10000 },
            { 12345.120001, 123451 },
            { -300.0, -3000 },
            { -400.5, -4005 },
            { -999.949999, -9999 },
            { -999.950001, -10000 },
            { -12345.120001, -123451 }
        };
    }

    public static TheoryData<long, long> GetFromMicrosecondsWithLongData(IFixture fixture)
    {
        return new TheoryData<long, long>
        {
            { 300, 3000 },
            { 400, 4000 },
            { 999, 9990 },
            { 12345, 123450 },
            { -300, -3000 },
            { -400, -4000 },
            { -999, -9990 },
            { -12345, -123450 }
        };
    }

    public static TheoryData<double, long> GetFromMillisecondsWithDoubleData(IFixture fixture)
    {
        return new TheoryData<double, long>
        {
            { 300.0, 3000000 },
            { 400.5, 4005000 },
            { 999.999949999, 9999999 },
            { 999.999950001, 10000000 },
            { 12345.123450001, 123451235 },
            { -300.0, -3000000 },
            { -400.5, -4005000 },
            { -999.999949999, -9999999 },
            { -999.999950001, -10000000 },
            { -12345.123450001, -123451235 }
        };
    }

    public static TheoryData<long, long> GetFromMillisecondsWithLongData(IFixture fixture)
    {
        return new TheoryData<long, long>
        {
            { 300, 3000000 },
            { 400, 4000000 },
            { 999, 9990000 },
            { 12345, 123450000 },
            { -300, -3000000 },
            { -400, -4000000 },
            { -999, -9990000 },
            { -12345, -123450000 }
        };
    }

    public static TheoryData<double, long> GetFromSecondsWithDoubleData(IFixture fixture)
    {
        return new TheoryData<double, long>
        {
            { 30.0, 300000000 },
            { 40.5, 405000000 },
            { 59.999999949999, 599999999 },
            { 59.999999950001, 600000000 },
            { 12345.12345670001, 123451234567 },
            { -30.0, -300000000 },
            { -40.5, -405000000 },
            { -59.999999949999, -599999999 },
            { -59.999999950001, -600000000 },
            { -12345.12345670001, -123451234567 }
        };
    }

    public static TheoryData<long, long> GetFromSecondsWithLongData(IFixture fixture)
    {
        return new TheoryData<long, long>
        {
            { 30, 300000000 },
            { 40, 400000000 },
            { 59, 590000000 },
            { 12345, 123450000000 },
            { -30, -300000000 },
            { -40, -400000000 },
            { -59, -590000000 },
            { -12345, -123450000000 }
        };
    }

    public static TheoryData<double, long> GetFromMinutesWithDoubleData(IFixture fixture)
    {
        return new TheoryData<double, long>
        {
            { 30.0, 18000000000 },
            { 40.5, 24300000000 },
            { 59.9999999991666, 35999999999 },
            { 59.9999999991667, 36000000000 },
            { 123.12345679001, 73874074074 },
            { -30.0, -18000000000 },
            { -40.5, -24300000000 },
            { -59.9999999991666, -35999999999 },
            { -59.9999999991667, -36000000000 },
            { -123.12345679001, -73874074074 }
        };
    }

    public static TheoryData<long, long> GetFromMinutesWithLongData(IFixture fixture)
    {
        return new TheoryData<long, long>
        {
            { 30, 18000000000 },
            { 40, 24000000000 },
            { 59, 35400000000 },
            { 123, 73800000000 },
            { -30, -18000000000 },
            { -40, -24000000000 },
            { -59, -35400000000 },
            { -123, -73800000000 }
        };
    }

    public static TheoryData<double, long> GetFromHoursWithDoubleData(IFixture fixture)
    {
        return new TheoryData<double, long>
        {
            { 3.0, 108000000000 },
            { 4.5, 162000000000 },
            { 1.99999999998611, 71999999999 },
            { 1.99999999998612, 72000000000 },
            { 23.1234567890139, 832444444405 },
            { -3.0, -108000000000 },
            { -4.5, -162000000000 },
            { -1.99999999998611, -71999999999 },
            { -1.99999999998612, -72000000000 },
            { -23.1234567890139, -832444444405 }
        };
    }

    public static TheoryData<long, long> GetFromHoursWithLongData(IFixture fixture)
    {
        return new TheoryData<long, long>
        {
            { 3, 108000000000 },
            { 4, 144000000000 },
            { 23, 828000000000 },
            { 123, 4428000000000 },
            { -3, -108000000000 },
            { -4, -144000000000 },
            { -23, -828000000000 },
            { -123, -4428000000000 }
        };
    }

    public static TheoryData<long, string> GetToStringData(IFixture fixture)
    {
        return new TheoryData<long, string>
        {
            { 0, "0 second(s)" },
            { 18046875, "1.8046875 second(s)" },
            { -50123515625, "-5012.3515625 second(s)" },
            { 2020000000, "202 second(s)" },
            { 1, "0.0000001 second(s)" },
            { -1, "-0.0000001 second(s)" }
        };
    }

    public static TheoryData<long, long, bool> GetEqualsData(IFixture fixture)
    {
        return new TheoryData<long, long, bool>
        {
            { 0, 0, true },
            { 0, 1, false },
            { -1, 0, false },
            { 111, 111, true },
            { -111, -111, true },
            { 111, -111, false }
        };
    }

    public static IEnumerable<object?[]> GetNotEqualsData(IFixture fixture)
    {
        return GetEqualsData( fixture ).ConvertResult( (bool r) => ! r );
    }

    public static TheoryData<long, long, int> GetCompareToData(IFixture fixture)
    {
        return new TheoryData<long, long, int>
        {
            { 0, 0, 0 },
            { 0, 1, -1 },
            { -1, 0, -1 },
            { 1, 0, 1 },
            { 111, 111, 0 },
            { -111, -111, 0 },
            { 111, -111, 1 }
        };
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

    public static TheoryData<long, long> GetNegateData(IFixture fixture)
    {
        return new TheoryData<long, long>
        {
            { 0, 0 },
            { 1, -1 },
            { -1, 1 }
        };
    }

    public static TheoryData<long, long> GetAbsData(IFixture fixture)
    {
        return new TheoryData<long, long>
        {
            { 0, 0 },
            { 1, 1 },
            { -1, 1 },
            { 10, 10 },
            { -10, 10 }
        };
    }

    public static TheoryData<long, long, long> GetAddTicksData(IFixture fixture)
    {
        return new TheoryData<long, long, long>
        {
            { 0, 0, 0 },
            { 1, 2, 3 },
            { -1, -2, -3 },
            { 1, -2, -1 },
            { -1, 2, 1 }
        };
    }

    public static TheoryData<long, double, long> GetAddMicrosecondsWithDoubleData(IFixture fixture)
    {
        return new TheoryData<long, double, long>
        {
            { 0, 0.0, 0 },
            { 1, 2.0, 21 },
            { -1, -2.0, -21 },
            { 1, -2.0, -19 },
            { -1, 2.0, 19 },
            { 1, 0.949999, 10 },
            { 1, 0.950001, 11 },
            { -1, -0.949999, -10 },
            { -1, -0.950001, -11 }
        };
    }

    public static TheoryData<long, long, long> GetAddMicrosecondsWithLongData(IFixture fixture)
    {
        return new TheoryData<long, long, long>
        {
            { 0, 0, 0 },
            { 1, 2, 21 },
            { -1, -2, -21 },
            { 1, -2, -19 },
            { -1, 2, 19 }
        };
    }

    public static TheoryData<long, double, long> GetAddMillisecondsWithDoubleData(IFixture fixture)
    {
        return new TheoryData<long, double, long>
        {
            { 0, 0.0, 0 },
            { 1, 2.0, 20001 },
            { -1, -2.0, -20001 },
            { 1, -2.0, -19999 },
            { -1, 2.0, 19999 },
            { 1, 0.999949999, 10000 },
            { 1, 0.999950001, 10001 },
            { -1, -0.999949999, -10000 },
            { -1, -0.999950001, -10001 }
        };
    }

    public static TheoryData<long, long, long> GetAddMillisecondsWithLongData(IFixture fixture)
    {
        return new TheoryData<long, long, long>
        {
            { 0, 0, 0 },
            { 1, 2, 20001 },
            { -1, -2, -20001 },
            { 1, -2, -19999 },
            { -1, 2, 19999 }
        };
    }

    public static TheoryData<long, double, long> GetAddSecondsWithDoubleData(IFixture fixture)
    {
        return new TheoryData<long, double, long>
        {
            { 0, 0.0, 0 },
            { 1, 2.0, 20000001 },
            { -1, -2.0, -20000001 },
            { 1, -2.0, -19999999 },
            { -1, 2.0, 19999999 },
            { 1, 0.999999949999, 10000000 },
            { 1, 0.999999950001, 10000001 },
            { -1, -0.999999949999, -10000000 },
            { -1, -0.999999950001, -10000001 }
        };
    }

    public static TheoryData<long, long, long> GetAddSecondsWithLongData(IFixture fixture)
    {
        return new TheoryData<long, long, long>
        {
            { 0, 0, 0 },
            { 1, 2, 20000001 },
            { -1, -2, -20000001 },
            { 1, -2, -19999999 },
            { -1, 2, 19999999 }
        };
    }

    public static TheoryData<long, double, long> GetAddMinutesWithDoubleData(IFixture fixture)
    {
        return new TheoryData<long, double, long>
        {
            { 0, 0.0, 0 },
            { 1, 2.0, 1200000001 },
            { -1, -2.0, -1200000001 },
            { 1, -2.0, -1199999999 },
            { -1, 2.0, 1199999999 },
            { 1, 0.9999999991666, 600000000 },
            { 1, 0.9999999991667, 600000001 },
            { -1, -0.9999999991666, -600000000 },
            { -1, -0.9999999991667, -600000001 }
        };
    }

    public static TheoryData<long, long, long> GetAddMinutesWithLongData(IFixture fixture)
    {
        return new TheoryData<long, long, long>
        {
            { 0, 0, 0 },
            { 1, 2, 1200000001 },
            { -1, -2, -1200000001 },
            { 1, -2, -1199999999 },
            { -1, 2, 1199999999 }
        };
    }

    public static TheoryData<long, double, long> GetAddHoursWithDoubleData(IFixture fixture)
    {
        return new TheoryData<long, double, long>
        {
            { 0, 0.0, 0 },
            { 1, 2.0, 72000000001 },
            { -1, -2.0, -72000000001 },
            { 1, -2.0, -71999999999 },
            { -1, 2.0, 71999999999 },
            { 1, 0.99999999998611, 36000000000 },
            { 1, 0.99999999998612, 36000000001 },
            { -1, -0.99999999998611, -36000000000 },
            { -1, -0.99999999998612, -36000000001 }
        };
    }

    public static TheoryData<long, long, long> GetAddHoursWithLongData(IFixture fixture)
    {
        return new TheoryData<long, long, long>
        {
            { 0, 0, 0 },
            { 1, 2, 72000000001 },
            { -1, -2, -72000000001 },
            { 1, -2, -71999999999 },
            { -1, 2, 71999999999 }
        };
    }

    public static TheoryData<long, long, long> GetSubtractTicksData(IFixture fixture)
    {
        return new TheoryData<long, long, long>
        {
            { 0, 0, 0 },
            { 1, 2, -1 },
            { -1, -2, 1 },
            { 1, -2, 3 },
            { -1, 2, -3 }
        };
    }

    public static TheoryData<long, double, long> GetSubtractMicrosecondsWithDoubleData(IFixture fixture)
    {
        return new TheoryData<long, double, long>
        {
            { 0, 0.0, 0 },
            { 1, 2.0, -19 },
            { -1, -2.0, 19 },
            { 1, -2.0, 21 },
            { -1, 2.0, -21 },
            { 1, 0.949999, -8 },
            { 1, 0.950001, -9 },
            { -1, -0.949999, 8 },
            { -1, -0.950001, 9 }
        };
    }

    public static TheoryData<long, long, long> GetSubtractMicrosecondsWithLongData(IFixture fixture)
    {
        return new TheoryData<long, long, long>
        {
            { 0, 0, 0 },
            { 1, 2, -19 },
            { -1, -2, 19 },
            { 1, -2, 21 },
            { -1, 2, -21 }
        };
    }

    public static TheoryData<long, double, long> GetSubtractMillisecondsWithDoubleData(IFixture fixture)
    {
        return new TheoryData<long, double, long>
        {
            { 0, 0.0, 0 },
            { 1, 2.0, -19999 },
            { -1, -2.0, 19999 },
            { 1, -2.0, 20001 },
            { -1, 2.0, -20001 },
            { 1, 0.999949999, -9998 },
            { 1, 0.999950001, -9999 },
            { -1, -0.999949999, 9998 },
            { -1, -0.999950001, 9999 }
        };
    }

    public static TheoryData<long, long, long> GetSubtractMillisecondsWithLongData(IFixture fixture)
    {
        return new TheoryData<long, long, long>
        {
            { 0, 0, 0 },
            { 1, 2, -19999 },
            { -1, -2, 19999 },
            { 1, -2, 20001 },
            { -1, 2, -20001 }
        };
    }

    public static TheoryData<long, double, long> GetSubtractSecondsWithDoubleData(IFixture fixture)
    {
        return new TheoryData<long, double, long>
        {
            { 0, 0.0, 0 },
            { 1, 2.0, -19999999 },
            { -1, -2.0, 19999999 },
            { 1, -2.0, 20000001 },
            { -1, 2.0, -20000001 },
            { 1, 0.999999949999, -9999998 },
            { 1, 0.999999950001, -9999999 },
            { -1, -0.999999949999, 9999998 },
            { -1, -0.999999950001, 9999999 }
        };
    }

    public static TheoryData<long, long, long> GetSubtractSecondsWithLongData(IFixture fixture)
    {
        return new TheoryData<long, long, long>
        {
            { 0, 0, 0 },
            { 1, 2, -19999999 },
            { -1, -2, 19999999 },
            { 1, -2, 20000001 },
            { -1, 2, -20000001 }
        };
    }

    public static TheoryData<long, double, long> GetSubtractMinutesWithDoubleData(IFixture fixture)
    {
        return new TheoryData<long, double, long>
        {
            { 0, 0.0, 0 },
            { 1, 2.0, -1199999999 },
            { -1, -2.0, 1199999999 },
            { 1, -2.0, 1200000001 },
            { -1, 2.0, -1200000001 },
            { 1, 0.9999999991666, -599999998 },
            { 1, 0.9999999991667, -599999999 },
            { -1, -0.9999999991666, 599999998 },
            { -1, -0.9999999991667, 599999999 }
        };
    }

    public static TheoryData<long, long, long> GetSubtractMinutesWithLongData(IFixture fixture)
    {
        return new TheoryData<long, long, long>
        {
            { 0, 0, 0 },
            { 1, 2, -1199999999 },
            { -1, -2, 1199999999 },
            { 1, -2, 1200000001 },
            { -1, 2, -1200000001 }
        };
    }

    public static TheoryData<long, double, long> GetSubtractHoursWithDoubleData(IFixture fixture)
    {
        return new TheoryData<long, double, long>
        {
            { 0, 0.0, 0 },
            { 1, 2.0, -71999999999 },
            { -1, -2.0, 71999999999 },
            { 1, -2.0, 72000000001 },
            { -1, 2.0, -72000000001 },
            { 1, 0.99999999998611, -35999999998 },
            { 1, 0.99999999998612, -35999999999 },
            { -1, -0.99999999998611, 35999999998 },
            { -1, -0.99999999998612, 35999999999 }
        };
    }

    public static TheoryData<long, long, long> GetSubtractHoursWithLongData(IFixture fixture)
    {
        return new TheoryData<long, long, long>
        {
            { 0, 0, 0 },
            { 1, 2, -71999999999 },
            { -1, -2, 71999999999 },
            { 1, -2, 72000000001 },
            { -1, 2, -72000000001 }
        };
    }

    public static TheoryData<long, double, long> GetMultiplyData(IFixture fixture)
    {
        return new TheoryData<long, double, long>
        {
            { 0, 0.0, 0 },
            { 1, 0.0, 0 },
            { -1, 0.0, 0 },
            { 5, 1.0, 5 },
            { -5, -1.0, 5 },
            { 5, -1.0, -5 },
            { -5, 1.0, -5 },
            { 2, 1.5, 3 },
            { 3, 1.5, 5 },
            { 4, 0.5, 2 },
            { 5, 0.5, 3 },
            { 7, 11.0, 77 },
            { 7, -11.0, -77 }
        };
    }

    public static TheoryData<long, double, long> GetDivideData(IFixture fixture)
    {
        return new TheoryData<long, double, long>
        {
            { 0, 1.0, 0 },
            { 5, 1.0, 5 },
            { -5, -1.0, 5 },
            { 5, -1.0, -5 },
            { -5, 1.0, -5 },
            { 2, 0.666, 3 },
            { 3, 0.666, 5 },
            { 4, 2.0, 2 },
            { 5, 2.0, 3 },
            { 77, 11.0, 7 },
            { 77, -11.0, -7 }
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
            { 29, 20 },
            { -1, 0 },
            { -9, 0 },
            { -10, -10 },
            { -11, -10 },
            { -29, -20 }
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
            { 29999, 20000 },
            { -1, 0 },
            { -9999, 0 },
            { -10000, -10000 },
            { -10001, -10000 },
            { -29999, -20000 }
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
            { 29999999, 20000000 },
            { -1, 0 },
            { -9999999, 0 },
            { -10000000, -10000000 },
            { -10000001, -10000000 },
            { -29999999, -20000000 }
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
            { 1799999999, 1200000000 },
            { -1, 0 },
            { -599999999, 0 },
            { -600000000, -600000000 },
            { -600000001, -600000000 },
            { -1799999999, -1200000000 }
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
            { 107999999999, 72000000000 },
            { -1, 0 },
            { -35999999999, 0 },
            { -36000000000, -36000000000 },
            { -36000000001, -36000000000 },
            { -107999999999, -72000000000 }
        };
    }

    public static TheoryData<long, int> GetSetTicksInMicrosecondThrowData(IFixture fixture)
    {
        return new TheoryData<long, int>
        {
            { 1, -1 },
            { 1, -2 },
            { 1, (int)ChronoConstants.TicksPerMicrosecond },
            { 1, (int)ChronoConstants.TicksPerMicrosecond + 1 },
            { 1, (int)ChronoConstants.TicksPerMicrosecond + 2 },
            { -1, 1 },
            { -1, 2 },
            { -1, (int)-ChronoConstants.TicksPerMicrosecond },
            { -1, (int)-ChronoConstants.TicksPerMicrosecond - 1 },
            { -1, (int)-ChronoConstants.TicksPerMicrosecond - 2 },
            { 0, (int)ChronoConstants.TicksPerMicrosecond },
            { 0, (int)ChronoConstants.TicksPerMicrosecond + 1 },
            { 0, (int)ChronoConstants.TicksPerMicrosecond + 2 },
            { 0, (int)-ChronoConstants.TicksPerMicrosecond },
            { 0, (int)-ChronoConstants.TicksPerMicrosecond - 1 },
            { 0, (int)-ChronoConstants.TicksPerMicrosecond - 2 }
        };
    }

    public static TheoryData<long, int, long> GetSetTicksInMicrosecondData(IFixture fixture)
    {
        return new TheoryData<long, int, long>
        {
            { 0, 0, 0 },
            { 0, 4, 4 },
            { 0, -4, -4 },
            { 4, 0, 0 },
            { 4, 9, 9 },
            { -4, 0, 0 },
            { -4, -9, -9 },
            { 36630046, 0, 36630040 },
            { 36630046, 9, 36630049 },
            { 36630046, 4, 36630044 },
            { -36630046, 0, -36630040 },
            { -36630046, -9, -36630049 },
            { -36630046, -4, -36630044 }
        };
    }

    public static TheoryData<long, int> GetSetMicrosecondsInMillisecondThrowData(IFixture fixture)
    {
        return new TheoryData<long, int>
        {
            { 1, -1 },
            { 1, -2 },
            { 1, ChronoConstants.MicrosecondsPerMillisecond },
            { 1, ChronoConstants.MicrosecondsPerMillisecond + 1 },
            { 1, ChronoConstants.MicrosecondsPerMillisecond + 2 },
            { -1, 1 },
            { -1, 2 },
            { -1, -ChronoConstants.MicrosecondsPerMillisecond },
            { -1, -ChronoConstants.MicrosecondsPerMillisecond - 1 },
            { -1, -ChronoConstants.MicrosecondsPerMillisecond - 2 },
            { 0, ChronoConstants.MicrosecondsPerMillisecond },
            { 0, ChronoConstants.MicrosecondsPerMillisecond + 1 },
            { 0, ChronoConstants.MicrosecondsPerMillisecond + 2 },
            { 0, -ChronoConstants.MicrosecondsPerMillisecond },
            { 0, -ChronoConstants.MicrosecondsPerMillisecond - 1 },
            { 0, -ChronoConstants.MicrosecondsPerMillisecond - 2 }
        };
    }

    public static TheoryData<long, int, long> GetSetMicrosecondsInMillisecondData(IFixture fixture)
    {
        return new TheoryData<long, int, long>
        {
            { 0, 0, 0 },
            { 0, 456, 4560 },
            { 0, -456, -4560 },
            { 1230, 0, 0 },
            { 1230, 999, 9990 },
            { -1230, 0, 0 },
            { -1230, -999, -9990 },
            { 36637895, 0, 36630005 },
            { 36637895, 999, 36639995 },
            { 36637895, 123, 36631235 },
            { -36637895, 0, -36630005 },
            { -36637895, -999, -36639995 },
            { -36637895, -123, -36631235 }
        };
    }

    public static TheoryData<long, int> GetSetMillisecondsInSecondThrowData(IFixture fixture)
    {
        return new TheoryData<long, int>
        {
            { 1, -1 },
            { 1, -2 },
            { 1, ChronoConstants.MillisecondsPerSecond },
            { 1, ChronoConstants.MillisecondsPerSecond + 1 },
            { 1, ChronoConstants.MillisecondsPerSecond + 2 },
            { -1, 1 },
            { -1, 2 },
            { -1, -ChronoConstants.MillisecondsPerSecond },
            { -1, -ChronoConstants.MillisecondsPerSecond - 1 },
            { -1, -ChronoConstants.MillisecondsPerSecond - 2 },
            { 0, ChronoConstants.MillisecondsPerSecond },
            { 0, ChronoConstants.MillisecondsPerSecond + 1 },
            { 0, ChronoConstants.MillisecondsPerSecond + 2 },
            { 0, -ChronoConstants.MillisecondsPerSecond },
            { 0, -ChronoConstants.MillisecondsPerSecond - 1 },
            { 0, -ChronoConstants.MillisecondsPerSecond - 2 }
        };
    }

    public static TheoryData<long, int, long> GetSetMillisecondsInSecondData(IFixture fixture)
    {
        return new TheoryData<long, int, long>
        {
            { 0, 0, 0 },
            { 0, 456, 4560000 },
            { 0, -456, -4560000 },
            { 1230000, 0, 0 },
            { 1230000, 999, 9990000 },
            { -1230000, 0, 0 },
            { -1230000, -999, -9990000 },
            { 36637890005, 0, 36630000005 },
            { 36637890005, 999, 36639990005 },
            { 36637890005, 123, 36631230005 },
            { -36637890005, 0, -36630000005 },
            { -36637890005, -999, -36639990005 },
            { -36637890005, -123, -36631230005 }
        };
    }

    public static TheoryData<long, int> GetSetSecondsInMinuteThrowData(IFixture fixture)
    {
        return new TheoryData<long, int>
        {
            { 1, -1 },
            { 1, -2 },
            { 1, ChronoConstants.SecondsPerMinute },
            { 1, ChronoConstants.SecondsPerMinute + 1 },
            { 1, ChronoConstants.SecondsPerMinute + 2 },
            { -1, 1 },
            { -1, 2 },
            { -1, -ChronoConstants.SecondsPerMinute },
            { -1, -ChronoConstants.SecondsPerMinute - 1 },
            { -1, -ChronoConstants.SecondsPerMinute - 2 },
            { 0, ChronoConstants.SecondsPerMinute },
            { 0, ChronoConstants.SecondsPerMinute + 1 },
            { 0, ChronoConstants.SecondsPerMinute + 2 },
            { 0, -ChronoConstants.SecondsPerMinute },
            { 0, -ChronoConstants.SecondsPerMinute - 1 },
            { 0, -ChronoConstants.SecondsPerMinute - 2 }
        };
    }

    public static TheoryData<long, int, long> GetSetSecondsInMinuteData(IFixture fixture)
    {
        return new TheoryData<long, int, long>
        {
            { 0, 0, 0 },
            { 0, 45, 450000000 },
            { 0, -45, -450000000 },
            { 120000000, 0, 0 },
            { 120000000, 59, 590000000 },
            { -120000000, 0, 0 },
            { -120000000, -59, -590000000 },
            { 36940040005, 0, 36600040005 },
            { 36940040005, 59, 37190040005 },
            { 36940040005, 12, 36720040005 },
            { -36940040005, 0, -36600040005 },
            { -36940040005, -59, -37190040005 },
            { -36940040005, -12, -36720040005 }
        };
    }

    public static TheoryData<long, int> GetSetMinutesInHourThrowData(IFixture fixture)
    {
        return new TheoryData<long, int>
        {
            { 1, -1 },
            { 1, -2 },
            { 1, ChronoConstants.MinutesPerHour },
            { 1, ChronoConstants.MinutesPerHour + 1 },
            { 1, ChronoConstants.MinutesPerHour + 2 },
            { -1, 1 },
            { -1, 2 },
            { -1, -ChronoConstants.MinutesPerHour },
            { -1, -ChronoConstants.MinutesPerHour - 1 },
            { -1, -ChronoConstants.MinutesPerHour - 2 },
            { 0, ChronoConstants.MinutesPerHour },
            { 0, ChronoConstants.MinutesPerHour + 1 },
            { 0, ChronoConstants.MinutesPerHour + 2 },
            { 0, -ChronoConstants.MinutesPerHour },
            { 0, -ChronoConstants.MinutesPerHour - 1 },
            { 0, -ChronoConstants.MinutesPerHour - 2 }
        };
    }

    public static TheoryData<long, int, long> GetSetMinutesInHourData(IFixture fixture)
    {
        return new TheoryData<long, int, long>
        {
            { 0, 0, 0 },
            { 0, 45, 27000000000 },
            { 0, -45, -27000000000 },
            { 7200000000, 0, 0 },
            { 7200000000, 59, 35400000000 },
            { -7200000000, 0, 0 },
            { -7200000000, -59, -35400000000 },
            { 56430040005, 0, 36030040005 },
            { 56430040005, 59, 71430040005 },
            { 56430040005, 12, 43230040005 },
            { -56430040005, 0, -36030040005 },
            { -56430040005, -59, -71430040005 },
            { -56430040005, -12, -43230040005 }
        };
    }

    public static TheoryData<long, int> GetSetHoursThrowData(IFixture fixture)
    {
        return new TheoryData<long, int>
        {
            { 1, -1 },
            { 1, -2 },
            { -1, 1 },
            { -1, 2 }
        };
    }

    public static TheoryData<long, int, long> GetSetHoursData(IFixture fixture)
    {
        return new TheoryData<long, int, long>
        {
            { 0, 0, 0 },
            { 0, 12, 432000000000 },
            { 0, -12, -432000000000 },
            { 109230040005, 0, 1230040005 },
            { 109230040005, 12, 433230040005 },
            { -109230040005, 0, -1230040005 },
            { -109230040005, -12, -433230040005 }
        };
    }
}
