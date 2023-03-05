using System.Collections.Generic;

namespace LfrlAnvil.Tests.NumericsTests.FractionTests;

public class FractionTestsData
{
    public static TheoryData<long, ulong, long, ulong, int> GetCompareToData(IFixture fixture)
    {
        return new TheoryData<long, ulong, long, ulong, int>
        {
            { 0, 1, 0, 1, 0 },
            { 1, 1, 0, 1, 1 },
            { 0, 1, 1, 1, -1 },
            { -1, 1, 0, 1, -1 },
            { 0, 1, -1, 1, 1 },
            { 123456, 1000, 123456, 1000, 0 },
            { 123457, 1000, 123456, 1000, 1 },
            { 123456, 1000, 123457, 1000, -1 },
            { -123456, 1000, -123456, 1000, 0 },
            { -123457, 1000, -123456, 1000, -1 },
            { -123456, 1000, -123457, 1000, 1 },
            { 123456, 1000, -123456, 1000, 1 },
            { -123456, 1000, 123456, 1000, -1 },
            { 0, 1, 0, 10, 0 },
            { 0, 10, 0, 1, 0 },
            { 123456, 1000, 123456, 10000, 1 },
            { 123456, 10000, 123456, 1000, -1 },
            { 123000, 1000, 12300000, 100000, 0 },
            { 12300000, 100000, 123000, 1000, 0 },
            { 123000, 1000, 12300001, 100000, -1 },
            { 12300000, 100000, 123001, 1000, -1 },
            { 123001, 1000, 12300000, 100000, 1 },
            { 12300001, 100000, 123000, 1000, 1 },
            { 123456, 1000, 12345600, 100000, 0 },
            { 12345600, 100000, 123456, 1000, 0 },
            { 123456, 1000, 12345601, 100000, -1 },
            { 12345601, 100000, 123456, 1000, 1 },
            { 123455, 1000, 12345600, 100000, -1 },
            { 12345600, 100000, 123455, 1000, 1 },
            { -123456, 1000, -123456, 10000, -1 },
            { -123456, 10000, -123456, 1000, 1 },
            { -123000, 1000, -12300000, 100000, 0 },
            { -12300000, 100000, -123000, 1000, 0 },
            { -123000, 1000, -12300001, 100000, 1 },
            { -12300000, 100000, -123001, 1000, 1 },
            { -123001, 1000, -12300000, 100000, -1 },
            { -12300001, 100000, -123000, 1000, -1 },
            { -123456, 1000, -12345600, 100000, 0 },
            { -12345600, 100000, -123456, 1000, 0 },
            { -123456, 1000, -12345601, 100000, 1 },
            { -12345601, 100000, -123456, 1000, -1 },
            { -123455, 1000, -12345600, 100000, 1 },
            { -12345600, 100000, -123455, 1000, -1 },
            { -123000, 1000, 12300000, 100000, -1 },
            { -12300000, 100000, 123000, 1000, -1 },
            { -123456, 1000, 12345600, 100000, -1 },
            { -12345600, 100000, 123456, 1000, -1 },
            { 123000, 1000, -12300000, 100000, 1 },
            { 12300000, 100000, -123000, 1000, 1 },
            { 123456, 1000, -12345600, 100000, 1 },
            { 12345600, 100000, -123456, 1000, 1 },
            { long.MaxValue, 1, long.MinValue, 1, 1 },
            { long.MaxValue, 1, 1, 1000000000000000000, 1 },
            { long.MinValue, 1, -1, 1000000000000000000, -1 },
            { long.MaxValue, 1000, long.MaxValue, 1000, 0 },
            { long.MinValue, 1000, long.MinValue, 1000, 0 }
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
