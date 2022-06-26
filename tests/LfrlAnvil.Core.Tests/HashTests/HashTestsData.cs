using System.Collections.Generic;
using AutoFixture;
using LfrlAnvil.TestExtensions;
using Xunit;

namespace LfrlAnvil.Tests.HashTests;

public class HashTestsData
{
    public static TheoryData<int, int, bool> GetEqualsData(IFixture fixture)
    {
        var (_1, _2) = fixture.CreateDistinctCollection<int>( 2 );

        return new TheoryData<int, int, bool>
        {
            { _1, _1, true },
            { _1, _2, false }
        };
    }

    public static TheoryData<int, int, int> GetCompareToData(IFixture fixture)
    {
        var (_1, _2) = fixture.CreateDistinctSortedCollection<int>( 2 );

        return new TheoryData<int, int, int>
        {
            { _1, _1, 0 },
            { _1, _2, -1 },
            { _2, _1, 1 }
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
