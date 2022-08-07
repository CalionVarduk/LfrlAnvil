using System.Collections.Generic;

namespace LfrlAnvil.Identifiers.Tests.IdentifierTests;

public class IdentifierTestsData
{
    public static TheoryData<ulong, ulong, ushort> GetCtorWithValueData(IFixture fixture)
    {
        return new TheoryData<ulong, ulong, ushort>
        {
            { 0, 0, 0 },
            { 1, 0, 1 },
            { 65535, 0, 65535 },
            { 65536, 1, 0 },
            { 65537, 1, 1 },
            { ulong.MaxValue, 281474976710655, 65535 },
            { ulong.MaxValue - ushort.MaxValue + 1, 281474976710655, 1 },
            { ulong.MaxValue - ushort.MaxValue, 281474976710655, 0 }
        };
    }

    public static TheoryData<ulong, ushort, ulong> GetCtorWithHighAndLowData(IFixture fixture)
    {
        return new TheoryData<ulong, ushort, ulong>
        {
            { 0, 0, 0 },
            { 0, 1, 1 },
            { 0, 65535, 65535 },
            { 1, 0, 65536 },
            { 1, 1, 65537 },
            { 281474976710655, 65535, ulong.MaxValue },
            { 281474976710655, 1, ulong.MaxValue - ushort.MaxValue + 1 },
            { 281474976710655, 0, ulong.MaxValue - ushort.MaxValue }
        };
    }

    public static TheoryData<ulong, string> GetToStringData(IFixture fixture)
    {
        return new TheoryData<ulong, string>
        {
            { 0, "Identifier(0)" },
            { 1, "Identifier(1)" },
            { 65536, "Identifier(65536)" },
            { ulong.MaxValue, $"Identifier({ulong.MaxValue})" }
        };
    }

    public static TheoryData<ulong, ulong, bool> GetEqualsData(IFixture fixture)
    {
        var (a, b) = fixture.CreateDistinctCollection<ulong>( 2 );

        return new TheoryData<ulong, ulong, bool>
        {
            { a, a, true },
            { a, b, false },
            { b, a, false }
        };
    }

    public static TheoryData<ulong, ulong, int> GetCompareToData(IFixture fixture)
    {
        var (a, b) = fixture.CreateDistinctSortedCollection<ulong>( 2 );

        return new TheoryData<ulong, ulong, int>
        {
            { a, a, 0 },
            { a, b, -1 },
            { b, a, 1 }
        };
    }

    public static IEnumerable<object?[]> GetNotEqualsData(IFixture fixture)
    {
        return GetEqualsData( fixture ).ConvertResult( (bool r) => ! r );
    }

    public static IEnumerable<object?[]> CreateGreaterThanComparisonTestData(IFixture fixture)
    {
        return GetCompareToData( fixture ).ConvertResult( (int r) => r > 0 );
    }

    public static IEnumerable<object?[]> CreateGreaterThanOrEqualToComparisonTestData(IFixture fixture)
    {
        return GetCompareToData( fixture ).ConvertResult( (int r) => r >= 0 );
    }

    public static IEnumerable<object?[]> CreateLessThanComparisonTestData(IFixture fixture)
    {
        return GetCompareToData( fixture ).ConvertResult( (int r) => r < 0 );
    }

    public static IEnumerable<object?[]> CreateLessThanOrEqualToComparisonTestData(IFixture fixture)
    {
        return GetCompareToData( fixture ).ConvertResult( (int r) => r <= 0 );
    }
}
