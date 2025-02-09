using System.Collections.Generic;

namespace LfrlAnvil.Tests.HashTests;

public class HashTestsData
{
    public static TheoryData<int, int, bool> GetEqualsData(Fixture fixture)
    {
        var (_1, _2) = fixture.CreateManyDistinct<int>( count: 2 );

        return new TheoryData<int, int, bool>
        {
            { _1, _1, true },
            { _1, _2, false }
        };
    }

    public static TheoryData<int, int, int> GetCompareToData(Fixture fixture)
    {
        var (_1, _2) = fixture.CreateManyDistinctSorted<int>( count: 2 );

        return new TheoryData<int, int, int>
        {
            { _1, _1, 0 },
            { _1, _2, -1 },
            { _2, _1, 1 }
        };
    }

    public static IEnumerable<object?[]> GetNotEqualsData(Fixture fixture)
    {
        return GetEqualsData( fixture ).ConvertResult( (bool r) => ! r );
    }

    public static IEnumerable<object?[]> GetGreaterThanComparisonData(Fixture fixture)
    {
        return GetCompareToData( fixture ).ConvertResult( (int r) => r > 0 );
    }

    public static IEnumerable<object?[]> GetGreaterThanOrEqualToComparisonData(Fixture fixture)
    {
        return GetCompareToData( fixture ).ConvertResult( (int r) => r >= 0 );
    }

    public static IEnumerable<object?[]> GetLessThanComparisonData(Fixture fixture)
    {
        return GetCompareToData( fixture ).ConvertResult( (int r) => r < 0 );
    }

    public static IEnumerable<object?[]> GetLessThanOrEqualToComparisonData(Fixture fixture)
    {
        return GetCompareToData( fixture ).ConvertResult( (int r) => r <= 0 );
    }
}
