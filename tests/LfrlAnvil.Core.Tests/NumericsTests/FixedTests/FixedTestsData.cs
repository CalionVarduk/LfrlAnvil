using System.Collections.Generic;

namespace LfrlAnvil.Tests.NumericsTests.FixedTests;

public class FixedTestsData
{
    public static TheoryData<long, byte> GetCreateRawData(IFixture fixture)
    {
        return new TheoryData<long, byte>
        {
            { 123, 0 },
            { -123, 0 },
            { 123, 1 },
            { -123, 1 },
            { 123, 10 },
            { -123, 10 },
            { 123, 18 },
            { -123, 18 },
        };
    }

    public static TheoryData<long, byte, long> GetCreateWithInt64Data(IFixture fixture)
    {
        return new TheoryData<long, byte, long>
        {
            { 123, 0, 123 },
            { 123, 1, 1230 },
            { 123, 2, 12300 },
            { 123, 3, 123000 },
            { 123, 4, 1230000 },
            { 123, 5, 12300000 },
            { 123, 6, 123000000 },
            { 123, 7, 1230000000 },
            { 123, 8, 12300000000 },
            { 123, 9, 123000000000 },
            { 123, 10, 1230000000000 },
            { 123, 11, 12300000000000 },
            { 123, 12, 123000000000000 },
            { 123, 13, 1230000000000000 },
            { 123, 14, 12300000000000000 },
            { 123, 15, 123000000000000000 },
            { 123, 16, 1230000000000000000 },
            { 12, 17, 1200000000000000000 },
            { 1, 18, 1000000000000000000 },
        };
    }

    public static TheoryData<decimal, byte, long> GetCreateWithDecimalData(IFixture fixture)
    {
        return new TheoryData<decimal, byte, long>
        {
            { 123m, 0, 123 },
            { 123.4999m, 0, 123 },
            { 123.5m, 0, 124 },
            { 123m, 1, 1230 },
            { 123.4499m, 1, 1234 },
            { 123.5m, 1, 1235 },
            { 1230m, 10, 12300000000000 },
            { 1230.125m, 10, 12301250000000 },
            { 1230.12345678914m, 10, 12301234567891 },
            { 1230.12345678915m, 10, 12301234567892 },
            { -123m, 0, -123 },
            { -123.4999m, 0, -123 },
            { -123.5m, 0, -124 },
            { -123m, 1, -1230 },
            { -123.4499m, 1, -1234 },
            { -123.5m, 1, -1235 },
            { -1230m, 10, -12300000000000 },
            { -1230.125m, 10, -12301250000000 },
            { -1230.12345678914m, 10, -12301234567891 },
            { -1230.12345678915m, 10, -12301234567892 }
        };
    }

    public static TheoryData<double, byte, long> GetCreateWithDoubleData(IFixture fixture)
    {
        return new TheoryData<double, byte, long>
        {
            { 123, 0, 123 },
            { 123.4999, 0, 123 },
            { 123.5, 0, 124 },
            { 123, 1, 1230 },
            { 123.4499, 1, 1234 },
            { 123.5, 1, 1235 },
            { 1230, 10, 12300000000000 },
            { 1230.125, 10, 12301250000000 },
            { 1230.12345678914, 10, 12301234567891 },
            { 1230.123456789151, 10, 12301234567892 },
            { -123, 0, -123 },
            { -123.4999, 0, -123 },
            { -123.5, 0, -124 },
            { -123, 1, -1230 },
            { -123.4499, 1, -1234 },
            { -123.5, 1, -1235 },
            { -1230, 10, -12300000000000 },
            { -1230.125, 10, -12301250000000 },
            { -1230.12345678914, 10, -12301234567891 },
            { -1230.123456789151, 10, -12301234567892 }
        };
    }

    public static TheoryData<decimal, byte, int, long> GetRoundData(IFixture fixture)
    {
        return new TheoryData<decimal, byte, int, long>
        {
            { 0m, 0, 0, 0 },
            { 0m, 1, 0, 0 },
            { 0m, 1, 1, 0 },
            { 0m, 1, 2, 0 },
            { 1.49m, 2, 0, 100 },
            { 1.49m, 2, 1, 150 },
            { 1.49m, 2, 2, 149 },
            { 1.49m, 2, 3, 149 },
            { 1.5m, 2, 0, 200 },
            { 1.5m, 2, 1, 150 },
            { 1.5m, 2, 2, 150 },
            { 1.5m, 2, 3, 150 },
            { 1.23456m, 5, 0, 100000 },
            { 1.23456m, 5, 1, 120000 },
            { 1.23456m, 5, 2, 123000 },
            { 1.23456m, 5, 3, 123500 },
            { 1.23456m, 5, 4, 123460 },
            { 1.23456m, 5, 5, 123456 },
            { -1.49m, 2, 0, -100 },
            { -1.49m, 2, 1, -150 },
            { -1.49m, 2, 2, -149 },
            { -1.49m, 2, 3, -149 },
            { -1.5m, 2, 0, -200 },
            { -1.5m, 2, 1, -150 },
            { -1.5m, 2, 2, -150 },
            { -1.5m, 2, 3, -150 },
            { -1.23456m, 5, 0, -100000 },
            { -1.23456m, 5, 1, -120000 },
            { -1.23456m, 5, 2, -123000 },
            { -1.23456m, 5, 3, -123500 },
            { -1.23456m, 5, 4, -123460 },
            { -1.23456m, 5, 5, -123456 }
        };
    }

    public static TheoryData<decimal, byte, long> GetFloorData(IFixture fixture)
    {
        return new TheoryData<decimal, byte, long>
        {
            { 0m, 1, 0 },
            { 1m, 2, 100 },
            { 1.01m, 2, 100 },
            { 1.49m, 2, 100 },
            { 1.5m, 2, 100 },
            { 1.99m, 2, 100 },
            { 5m, 5, 500000 },
            { 5.00001m, 5, 500000 },
            { 5.49999m, 5, 500000 },
            { 5.5m, 5, 500000 },
            { 5.99999m, 5, 500000 },
            { -1m, 2, -100 },
            { -1.01m, 2, -200 },
            { -1.49m, 2, -200 },
            { -1.5m, 2, -200 },
            { -1.99m, 2, -200 },
            { -5m, 5, -500000 },
            { -5.00001m, 5, -600000 },
            { -5.49999m, 5, -600000 },
            { -5.5m, 5, -600000 },
            { -5.99999m, 5, -600000 }
        };
    }

    public static TheoryData<decimal, byte, long> GetCeilingData(IFixture fixture)
    {
        return new TheoryData<decimal, byte, long>
        {
            { 0m, 1, 0 },
            { 1m, 2, 100 },
            { 1.01m, 2, 200 },
            { 1.49m, 2, 200 },
            { 1.5m, 2, 200 },
            { 1.99m, 2, 200 },
            { 5m, 5, 500000 },
            { 5.00001m, 5, 600000 },
            { 5.49999m, 5, 600000 },
            { 5.5m, 5, 600000 },
            { 5.99999m, 5, 600000 },
            { -1m, 2, -100 },
            { -1.01m, 2, -100 },
            { -1.49m, 2, -100 },
            { -1.5m, 2, -100 },
            { -1.99m, 2, -100 },
            { -5m, 5, -500000 },
            { -5.00001m, 5, -500000 },
            { -5.49999m, 5, -500000 },
            { -5.5m, 5, -500000 },
            { -5.99999m, 5, -500000 }
        };
    }

    public static TheoryData<long, byte, long, byte, int> GetCompareToData(IFixture fixture)
    {
        return new TheoryData<long, byte, long, byte, int>
        {
            { 0, 0, 0, 0, 0 },
            { 1, 0, 0, 0, 1 },
            { 0, 0, 1, 0, -1 },
            { -1, 0, 0, 0, -1 },
            { 0, 0, -1, 0, 1 },
            { 123456, 3, 123456, 3, 0 },
            { 123457, 3, 123456, 3, 1 },
            { 123456, 3, 123457, 3, -1 },
            { -123456, 3, -123456, 3, 0 },
            { -123457, 3, -123456, 3, -1 },
            { -123456, 3, -123457, 3, 1 },
            { 123456, 3, -123456, 3, 1 },
            { -123456, 3, 123456, 3, -1 },
            { 0, 0, 0, 1, 0 },
            { 0, 1, 0, 0, 0 },
            { 123456, 3, 123456, 4, 1 },
            { 123456, 4, 123456, 3, -1 },
            { 123000, 3, 12300000, 5, 0 },
            { 12300000, 5, 123000, 3, 0 },
            { 123000, 3, 12300001, 5, -1 },
            { 12300000, 5, 123001, 3, -1 },
            { 123001, 3, 12300000, 5, 1 },
            { 12300001, 5, 123000, 3, 1 },
            { 123456, 3, 12345600, 5, 0 },
            { 12345600, 5, 123456, 3, 0 },
            { 123456, 3, 12345601, 5, -1 },
            { 12345601, 5, 123456, 3, 1 },
            { 123455, 3, 12345600, 5, -1 },
            { 12345600, 5, 123455, 3, 1 },
            { -123456, 3, -123456, 4, -1 },
            { -123456, 4, -123456, 3, 1 },
            { -123000, 3, -12300000, 5, 0 },
            { -12300000, 5, -123000, 3, 0 },
            { -123000, 3, -12300001, 5, 1 },
            { -12300000, 5, -123001, 3, 1 },
            { -123001, 3, -12300000, 5, -1 },
            { -12300001, 5, -123000, 3, -1 },
            { -123456, 3, -12345600, 5, 0 },
            { -12345600, 5, -123456, 3, 0 },
            { -123456, 3, -12345601, 5, 1 },
            { -12345601, 5, -123456, 3, -1 },
            { -123455, 3, -12345600, 5, 1 },
            { -12345600, 5, -123455, 3, -1 },
            { -123000, 3, 12300000, 5, -1 },
            { -12300000, 5, 123000, 3, -1 },
            { -123456, 3, 12345600, 5, -1 },
            { -12345600, 5, 123456, 3, -1 },
            { 123000, 3, -12300000, 5, 1 },
            { 12300000, 5, -123000, 3, 1 },
            { 123456, 3, -12345600, 5, 1 },
            { 12345600, 5, -123456, 3, 1 },
            { long.MaxValue, 0, long.MinValue, 0, 1 },
            { long.MaxValue, 0, 1, 18, 1 },
            { long.MinValue, 0, -1, 18, -1 },
            { long.MaxValue, 3, long.MaxValue, 3, 0 },
            { long.MinValue, 3, long.MinValue, 3, 0 }
        };
    }

    public static IEnumerable<object?[]> GetEqualsData(IFixture fixture)
    {
        return GetCompareToData( fixture ).ConvertResult( (int cmp) => cmp == 0 );
    }

    public static IEnumerable<object?[]> GetNotEqualsData(IFixture fixture)
    {
        return GetCompareToData( fixture ).ConvertResult( (int cmp) => cmp != 0 );
    }

    public static IEnumerable<object?[]> GetLessThanData(IFixture fixture)
    {
        return GetCompareToData( fixture ).ConvertResult( (int cmp) => cmp < 0 );
    }

    public static IEnumerable<object?[]> GetLessThanOrEqualToData(IFixture fixture)
    {
        return GetCompareToData( fixture ).ConvertResult( (int cmp) => cmp <= 0 );
    }

    public static IEnumerable<object?[]> GetGreaterThanData(IFixture fixture)
    {
        return GetCompareToData( fixture ).ConvertResult( (int cmp) => cmp > 0 );
    }

    public static IEnumerable<object?[]> GetGreaterThanOrEqualToData(IFixture fixture)
    {
        return GetCompareToData( fixture ).ConvertResult( (int cmp) => cmp >= 0 );
    }
}
