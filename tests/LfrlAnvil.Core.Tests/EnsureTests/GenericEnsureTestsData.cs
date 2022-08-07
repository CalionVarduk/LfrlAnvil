using System.Collections.Generic;

namespace LfrlAnvil.Tests.EnsureTests;

public class GenericEnsureTestsData<T>
{
    public static TheoryData<int> GetContainsAtLeastPassData(IFixture fixture)
    {
        return new TheoryData<int>
        {
            -1,
            0,
            1,
            2,
            3
        };
    }

    public static TheoryData<int> GetContainsAtLeastThrowData(IFixture fixture)
    {
        return new TheoryData<int>
        {
            4,
            5
        };
    }

    public static TheoryData<int> GetContainsAtMostPassData(IFixture fixture)
    {
        return new TheoryData<int>
        {
            3,
            4,
            5
        };
    }

    public static TheoryData<int> GetContainsAtMostThrowData(IFixture fixture)
    {
        return new TheoryData<int>
        {
            -1,
            0,
            1,
            2
        };
    }

    public static TheoryData<int> GetContainsExactlyThrowData(IFixture fixture)
    {
        return new TheoryData<int>
        {
            2,
            4
        };
    }

    public static TheoryData<int, int> GetContainsInRangePassData(IFixture fixture)
    {
        return new TheoryData<int, int>
        {
            { 0, 3 },
            { 1, 3 },
            { 2, 3 },
            { 3, 3 },
            { 0, 4 },
            { 1, 4 },
            { 2, 4 },
            { 3, 4 },
            { 3, 5 },
            { 3, 6 }
        };
    }

    public static TheoryData<int, int> GetContainsInRangeThrowData(IFixture fixture)
    {
        return new TheoryData<int, int>
        {
            { 0, 2 },
            { 1, 2 },
            { 2, 2 },
            { 4, 4 },
            { 4, 5 },
            { 4, 6 },
            { 3, 2 }
        };
    }

    public static TheoryData<IEnumerable<T>> GetIsOrderedPassData(IFixture fixture)
    {
        var (_1, _2, _3) = fixture.CreateDistinctSortedCollection<T>( 3 );

        return new TheoryData<IEnumerable<T>>
        {
            new[] { _1, _2 },
            new[] { _1, _1 },
            new[] { _1, _1, _2 },
            new[] { _1, _2, _2 },
            new[] { _1, _2, _3 }
        };
    }

    public static TheoryData<IEnumerable<T>> GetIsOrderedThrowData(IFixture fixture)
    {
        var (_1, _2, _3) = fixture.CreateDistinctSortedCollection<T>( 3 );

        return new TheoryData<IEnumerable<T>>
        {
            new[] { _2, _1 },
            new[] { _2, _2, _1 },
            new[] { _2, _1, _1 },
            new[] { _3, _2, _1 },
            new[] { _1, _3, _2 },
            new[] { _2, _1, _3 }
        };
    }
}
