using System;
using System.Collections.Generic;
using AutoFixture;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Chrono.Duration
{
    public class DurationTestsData
    {
        public static TheoryData<long> GetTicksData(IFixture fixture)
        {
            return new()
            {
                0,
                -1000001,
                1234567
            };
        }

        public static TheoryData<int, int, int, long> GetCtorWithSecondsPrecisionData(IFixture fixture)
        {
            return new()
            {
                { 3, 111, 321, 17781 },
                { -4, 40, 1, -11999 },
                { 78, -765, -303, 234597 }
            };
        }

        public static TheoryData<int, int, int, int, long> GetCtorWithMsPrecisionData(IFixture fixture)
        {
            return new()
            {
                { 3, 111, 321, 987, 17781987 },
                { -4, 40, 1, 3456, -11995544 },
                { 78, -765, -303, -12345, 234584655 }
            };
        }

        public static TheoryData<int, int, int, int, int, long> GetCtorWithTicksPrecisionData(IFixture fixture)
        {
            return new()
            {
                { 3, 111, 321, 987, 123456, 17781987 },
                { -4, 40, 1, 3456, 789, -11995544 },
                { 78, -765, -303, -12345, -9876543, 234584655 }
            };
        }

        public static TheoryData<TimeSpan> GetCtorWithTimeSpanData(IFixture fixture)
        {
            return new()
            {
                TimeSpan.Zero,
                TimeSpan.MinValue,
                TimeSpan.MaxValue,
                new TimeSpan( 2, 3, 40, 50, 678 ),
                new TimeSpan( -1, -2, -3, -4, -5 )
            };
        }

        public static TheoryData<int, int, long> GetFullMillisecondsData(IFixture fixture)
        {
            return new()
            {
                { 300, 0, 300 },
                { 400, 5000, 400 },
                { 999, 9999, 999 },
                { 12345, 1234, 12345 },
                { -300, 0, -300 },
                { -400, -5000, -400 },
                { -999, -9999, -999 },
                { -12345, -1234, -12345 }
            };
        }

        public static TheoryData<int, int, int, long> GetFullSecondsData(IFixture fixture)
        {
            return new()
            {
                { 30, 0, 0, 30 },
                { 40, 400, 5000, 40 },
                { 59, 999, 9999, 59 },
                { 12345, 123, 4567, 12345 },
                { -30, 0, 0, -30 },
                { -40, -400, -5000, -40 },
                { -59, -999, -9999, -59 },
                { -12345, -123, -4567, -12345 }
            };
        }

        public static TheoryData<int, int, int, int, long> GetFullMinutesData(IFixture fixture)
        {
            return new()
            {
                { 30, 0, 0, 0, 30 },
                { 50, 40, 400, 5000, 50 },
                { 59, 59, 999, 9999, 59 },
                { 12345, 12, 123, 4567, 12345 },
                { -30, 0, 0, 0, -30 },
                { -50, -40, -400, -5000, -50 },
                { -59, -59, -999, -9999, -59 },
                { -12345, -12, -123, -4567, -12345 }
            };
        }

        public static TheoryData<int, int, int, int, int, long> GetFullHoursData(IFixture fixture)
        {
            return new()
            {
                { 30, 0, 0, 0, 0, 30 },
                { 70, 50, 40, 400, 5000, 70 },
                { 25, 59, 59, 999, 9999, 25 },
                { 12345, 34, 12, 123, 4567, 12345 },
                { -30, 0, 0, 0, 0, -30 },
                { -70, -50, -40, -400, -5000, -70 },
                { -25, -59, -59, -999, -9999, -25 },
                { -12345, -34, -12, -123, -4567, -12345 }
            };
        }

        public static TheoryData<int, int, int> GetTicksInMillisecondData(IFixture fixture)
        {
            return new()
            {
                { 300, 0, 0 },
                { 400, 5000, 5000 },
                { 999, 9999, 9999 },
                { 12345, 1234, 1234 },
                { -300, 0, 0 },
                { -400, -5000, -5000 },
                { -999, -9999, -9999 },
                { -12345, -1234, -1234 }
            };
        }

        public static TheoryData<int, int, int, int> GetMillisecondsInSecondData(IFixture fixture)
        {
            return new()
            {
                { 30, 0, 0, 0 },
                { 40, 400, 5000, 400 },
                { 59, 999, 9999, 999 },
                { 12345, 123, 4567, 123 },
                { -30, 0, 0, 0 },
                { -40, -400, -5000, -400 },
                { -59, -999, -9999, -999 },
                { -12345, -123, -4567, -123 }
            };
        }

        public static TheoryData<int, int, int, int, int> GetSecondsInMinuteData(IFixture fixture)
        {
            return new()
            {
                { 30, 0, 0, 0, 0 },
                { 50, 40, 400, 5000, 40 },
                { 59, 59, 999, 9999, 59 },
                { 12345, 12, 123, 4567, 12 },
                { -30, 0, 0, 0, 0 },
                { -50, -40, -400, -5000, -40 },
                { -59, -59, -999, -9999, -59 },
                { -12345, -12, -123, -4567, -12 }
            };
        }

        public static TheoryData<int, int, int, int, int, int> GetMinutesInHourData(IFixture fixture)
        {
            return new()
            {
                { 30, 0, 0, 0, 0, 0 },
                { 70, 50, 40, 400, 5000, 50 },
                { 25, 59, 59, 999, 9999, 59 },
                { 12345, 34, 12, 123, 4567, 34 },
                { -30, 0, 0, 0, 0, 0 },
                { -70, -50, -40, -400, -5000, -50 },
                { -25, -59, -59, -999, -9999, -59 },
                { -12345, -34, -12, -123, -4567, -34 }
            };
        }

        public static TheoryData<int, int, double> GetTotalMillisecondsData(IFixture fixture)
        {
            return new()
            {
                { 300, 0, 300.0 },
                { 400, 5000, 400.5 },
                { 999, 9999, 999.9999 },
                { 12345, 1234, 12345.1234 },
                { -300, 0, -300.0 },
                { -400, -5000, -400.5 },
                { -999, -9999, -999.9999 },
                { -12345, -1234, -12345.1234 }
            };
        }

        public static TheoryData<int, int, int, double> GetTotalSecondsData(IFixture fixture)
        {
            return new()
            {
                { 30, 0, 0, 30.0 },
                { 40, 400, 5000, 40.4005 },
                { 59, 999, 9999, 59.9999999 },
                { 12345, 123, 4567, 12345.1234567 },
                { -30, 0, 0, -30.0 },
                { -40, -400, -5000, -40.4005 },
                { -59, -999, -9999, -59.9999999 },
                { -12345, -123, -4567, -12345.1234567 }
            };
        }

        public static TheoryData<int, int, int, int, double> GetTotalMinutesData(IFixture fixture)
        {
            return new()
            {
                { 30, 0, 0, 0, 30.0 },
                { 50, 40, 400, 5000, 50.67334166667 },
                { 59, 59, 999, 9999, 59.99999999833 },
                { 12345, 12, 123, 4567, 12345.20205761167 },
                { -30, 0, 0, 0, -30.0 },
                { -50, -40, -400, -5000, -50.67334166667 },
                { -59, -59, -999, -9999, -59.99999999833 },
                { -12345, -12, -123, -4567, -12345.20205761167 }
            };
        }

        public static TheoryData<int, int, int, int, int, double> GetTotalHoursData(IFixture fixture)
        {
            return new()
            {
                { 30, 0, 0, 0, 0, 30.0 },
                { 70, 50, 40, 400, 5000, 70.84455569444 },
                { 25, 59, 59, 999, 9999, 25.99999999997 },
                { 12345, 34, 12, 123, 4567, 12345.57003429353 },
                { -30, 0, 0, 0, 0, -30.0 },
                { -70, -50, -40, -400, -5000, -70.84455569444 },
                { -25, -59, -59, -999, -9999, -25.99999999997 },
                { -12345, -34, -12, -123, -4567, -12345.57003429353 }
            };
        }

        public static TheoryData<double, long> GetFromMillisecondsWithDoubleData(IFixture fixture)
        {
            return new()
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
            return new()
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
            return new()
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
            return new()
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
            return new()
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
            return new()
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
            return new()
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
            return new()
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
            return new()
            {
                { 0, "0 second(s)" },
                { 18046875, "1.8046875 second(s)" },
                { -50123515625, "-5012.3515625 second(s)" },
                { 2020000000, "202 second(s)" }
            };
        }

        public static TheoryData<long, long, bool> GetEqualsData(IFixture fixture)
        {
            return new()
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
            return new()
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
            return new()
            {
                { 0, 0 },
                { 1, -1 },
                { -1, 1 }
            };
        }

        public static TheoryData<long, long, long> GetAddTicksData(IFixture fixture)
        {
            return new()
            {
                { 0, 0, 0 },
                { 1, 2, 3 },
                { -1, -2, -3 },
                { 1, -2, -1 },
                { -1, 2, 1 }
            };
        }

        public static TheoryData<long, double, long> GetAddMillisecondsWithDoubleData(IFixture fixture)
        {
            return new()
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
            return new()
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
            return new()
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
            return new()
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
            return new()
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
            return new()
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
            return new()
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
            return new()
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
            return new()
            {
                { 0, 0, 0 },
                { 1, 2, -1 },
                { -1, -2, 1 },
                { 1, -2, 3 },
                { -1, 2, -3 }
            };
        }

        public static TheoryData<long, double, long> GetSubtractMillisecondsWithDoubleData(IFixture fixture)
        {
            return new()
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
            return new()
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
            return new()
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
            return new()
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
            return new()
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
            return new()
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
            return new()
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
            return new()
            {
                { 0, 0, 0 },
                { 1, 2, -71999999999 },
                { -1, -2, 71999999999 },
                { 1, -2, 72000000001 },
                { -1, 2, -72000000001 }
            };
        }

        public static TheoryData<long, long> GetTrimToMillisecondData(IFixture fixture)
        {
            return new()
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
            return new()
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
            return new()
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
            return new()
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
    }
}
