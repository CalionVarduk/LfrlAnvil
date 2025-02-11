using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace LfrlAnvil.Tests.ExtensionsTests.EnumerableTests;

public class GenericEnumerableExtensionsTestsData<T>
{
    public static TheoryData<int> GetIsEmptyData(Fixture fixture)
    {
        return new TheoryData<int>
        {
            1,
            3
        };
    }

    public static TheoryData<int, int, bool> GetContainsAtLeastData(Fixture fixture)
    {
        return new TheoryData<int, int, bool>
        {
            { 0, -1, true },
            { 0, 0, true },
            { 0, 1, false },
            { 1, -1, true },
            { 1, 0, true },
            { 1, 1, true },
            { 1, 2, false },
            { 3, -1, true },
            { 3, 0, true },
            { 3, 1, true },
            { 3, 2, true },
            { 3, 3, true },
            { 3, 4, false }
        };
    }

    public static TheoryData<int, int, bool> GetContainsAtMostData(Fixture fixture)
    {
        return new TheoryData<int, int, bool>
        {
            { 0, -1, false },
            { 0, 0, true },
            { 0, 1, true },
            { 1, -1, false },
            { 1, 0, false },
            { 1, 1, true },
            { 1, 2, true },
            { 3, -1, false },
            { 3, 0, false },
            { 3, 1, false },
            { 3, 2, false },
            { 3, 3, true },
            { 3, 4, true }
        };
    }

    public static TheoryData<int> GetContainsInRangeForMaxCountLessThanMinCountData(Fixture fixture)
    {
        return new TheoryData<int>
        {
            0,
            1,
            3
        };
    }

    public static TheoryData<int, int, bool> GetContainsInRangeForZeroMinCountData(Fixture fixture)
    {
        return new TheoryData<int, int, bool>
        {
            { 0, -1, false },
            { 0, 0, true },
            { 0, 1, true },
            { 1, -1, false },
            { 1, 0, false },
            { 1, 1, true },
            { 1, 2, true },
            { 3, -1, false },
            { 3, 0, false },
            { 3, 1, false },
            { 3, 2, false },
            { 3, 3, true },
            { 3, 4, true }
        };
    }

    public static TheoryData<int, int, bool> GetContainsInRangeForNegativeMinCountData(Fixture fixture)
    {
        return new TheoryData<int, int, bool>
        {
            { 0, -1, false },
            { 0, 0, true },
            { 0, 1, true },
            { 1, -1, false },
            { 1, 0, false },
            { 1, 1, true },
            { 1, 2, true },
            { 3, -1, false },
            { 3, 0, false },
            { 3, 1, false },
            { 3, 2, false },
            { 3, 3, true },
            { 3, 4, true }
        };
    }

    public static TheoryData<int, int> GetContainsInRangeForCountLessThanMinCountData(Fixture fixture)
    {
        return new TheoryData<int, int>
        {
            { 0, 1 },
            { 0, 2 },
            { 1, 2 },
            { 1, 3 },
            { 3, 4 },
            { 3, 5 }
        };
    }

    public static TheoryData<int, int> GetContainsInRangeForCountGreaterThanMaxCountData(Fixture fixture)
    {
        return new TheoryData<int, int>
        {
            { 3, 2 },
            { 4, 3 },
            { 4, 2 },
            { 5, 4 },
            { 5, 3 }
        };
    }

    public static TheoryData<int, int, int> GetContainsInRangeForCountBetweenMinAndMaxData(Fixture fixture)
    {
        return new TheoryData<int, int, int>
        {
            { 1, 1, 1 },
            { 1, 1, 2 },
            { 1, 1, 3 },
            { 3, 1, 3 },
            { 3, 1, 4 },
            { 3, 1, 5 },
            { 3, 2, 3 },
            { 3, 2, 4 },
            { 3, 2, 5 },
            { 3, 3, 3 },
            { 3, 3, 4 },
            { 3, 3, 5 }
        };
    }

    public static TheoryData<int> GetContainsExactlyForNegativeCountData(Fixture fixture)
    {
        return new TheoryData<int>
        {
            0,
            1,
            3
        };
    }

    public static TheoryData<int, int, bool> GetContainsExactlyForNonNegativeCountData(Fixture fixture)
    {
        return new TheoryData<int, int, bool>
        {
            { 0, 0, true },
            { 0, 1, false },
            { 1, 0, false },
            { 1, 1, true },
            { 1, 2, false },
            { 3, 2, false },
            { 3, 3, true },
            { 3, 4, false }
        };
    }

    public static TheoryData<IReadOnlyList<Pair<T, IEnumerable<T>>>, IEnumerable<Pair<T, T>>> GetFlattenData(Fixture fixture)
    {
        var (_1, _2, _3) = fixture.CreateManyDistinct<T>( count: 3 );

        return new TheoryData<IReadOnlyList<Pair<T, IEnumerable<T>>>, IEnumerable<Pair<T, T>>>
        {
            { Array.Empty<Pair<T, IEnumerable<T>>>(), Array.Empty<Pair<T, T>>() },
            { new[] { new Pair<T, IEnumerable<T>>( _1, new[] { _2 } ) }, new[] { Pair.Create( _1, _2 ) } },
            {
                new[] { new Pair<T, IEnumerable<T>>( _1, new[] { _2, _3 } ), new Pair<T, IEnumerable<T>>( _2, new[] { _1, _3 } ) },
                new[] { Pair.Create( _1, _2 ), Pair.Create( _1, _3 ), Pair.Create( _2, _1 ), Pair.Create( _2, _3 ) }
            }
        };
    }

    public static TheoryData<IEnumerable<T>, T> GetMinData(Fixture fixture)
    {
        var (_1, _2, _3) = fixture.CreateManyDistinctSorted<T>( count: 3 );

        return new TheoryData<IEnumerable<T>, T>
        {
            { new[] { _1 }, _1 },
            { new[] { _1, _2 }, _1 },
            { new[] { _2, _1 }, _1 },
            { new[] { _1, _1 }, _1 },
            { new[] { _1, _2, _3 }, _1 },
            { new[] { _1, _3, _2 }, _1 },
            { new[] { _3, _1, _2 }, _1 },
            { new[] { _3, _2, _1 }, _1 },
            { new[] { _1, _1, _1 }, _1 }
        };
    }

    public static TheoryData<IEnumerable<T>, T> GetMaxData(Fixture fixture)
    {
        var (_1, _2, _3) = fixture.CreateManyDistinctSorted<T>( count: 3 );

        return new TheoryData<IEnumerable<T>, T>
        {
            { new[] { _1 }, _1 },
            { new[] { _1, _2 }, _2 },
            { new[] { _2, _1 }, _2 },
            { new[] { _1, _1 }, _1 },
            { new[] { _1, _2, _3 }, _3 },
            { new[] { _1, _3, _2 }, _3 },
            { new[] { _3, _1, _2 }, _3 },
            { new[] { _3, _2, _1 }, _3 },
            { new[] { _1, _1, _1 }, _1 }
        };
    }

    public static TheoryData<IEnumerable<T>, T, T> GetMinMaxData(Fixture fixture)
    {
        var (_1, _2, _3) = fixture.CreateManyDistinctSorted<T>( count: 3 );

        return new TheoryData<IEnumerable<T>, T, T>
        {
            { new[] { _1 }, _1, _1 },
            { new[] { _1, _2 }, _1, _2 },
            { new[] { _2, _1 }, _1, _2 },
            { new[] { _1, _1 }, _1, _1 },
            { new[] { _1, _2, _3 }, _1, _3 },
            { new[] { _1, _3, _2 }, _1, _3 },
            { new[] { _3, _1, _2 }, _1, _3 },
            { new[] { _3, _2, _1 }, _1, _3 },
            { new[] { _1, _1, _1 }, _1, _1 }
        };
    }

    public static TheoryData<IEnumerable<T>, bool> GetContainsDuplicatesData(Fixture fixture)
    {
        var (_1, _2, _3) = fixture.CreateManyDistinct<T>( count: 3 );

        return new TheoryData<IEnumerable<T>, bool>
        {
            { new[] { _1 }, false },
            { new[] { _1, _1 }, true },
            { new[] { _1, _2 }, false },
            { new[] { _1, _1, _1 }, true },
            { new[] { _1, _1, _2 }, true },
            { new[] { _1, _2, _2 }, true },
            { new[] { _1, _2, _3 }, false },
            { new[] { _1, _1, _2, _3 }, true },
            { new[] { _1, _2, _2, _3 }, true },
            { new[] { _1, _2, _3, _3 }, true }
        };
    }

    public static TheoryData<int> GetRepeatForZeroOrOneCountData(Fixture fixture)
    {
        return new TheoryData<int>
        {
            0,
            1,
            3
        };
    }

    public static TheoryData<IEnumerable<T>, int, IEnumerable<T>> GetRepeatForCountGreaterThanOneData(Fixture fixture)
    {
        var (_1, _2, _3) = fixture.CreateManyDistinct<T>( count: 3 );

        return new TheoryData<IEnumerable<T>, int, IEnumerable<T>>
        {
            { Array.Empty<T>(), 2, Array.Empty<T>() },
            { Array.Empty<T>(), 5, Array.Empty<T>() },
            { new[] { _1 }, 2, new[] { _1, _1 } },
            { new[] { _1 }, 5, new[] { _1, _1, _1, _1, _1 } },
            { new[] { _1, _2, _3 }, 2, new[] { _1, _2, _3, _1, _2, _3 } },
            { new[] { _1, _2, _3 }, 5, new[] { _1, _2, _3, _1, _2, _3, _1, _2, _3, _1, _2, _3, _1, _2, _3 } }
        };
    }

    public static TheoryData<int> GetRepeatForMemoizationWithCountGreaterThanOneData(Fixture fixture)
    {
        return new TheoryData<int>
        {
            2,
            3,
            5
        };
    }

    public static TheoryData<int, int> GetMemoizeData(Fixture fixture)
    {
        return new TheoryData<int, int>
        {
            { 0, 0 },
            { 0, 1 },
            { 0, 3 },
            { 0, 5 },
            { 1, 0 },
            { 1, 1 },
            { 1, 3 },
            { 1, 5 },
            { 3, 0 },
            { 3, 1 },
            { 3, 3 },
            { 3, 5 }
        };
    }

    public static TheoryData<IEnumerable<T>, IEnumerable<T>, bool> GetSetEqualsData(Fixture fixture)
    {
        var (_1, _2, _3) = fixture.CreateManyDistinct<T>( count: 3 );

        return new TheoryData<IEnumerable<T>, IEnumerable<T>, bool>
        {
            { Array.Empty<T>(), Array.Empty<T>(), true },
            { Array.Empty<T>(), new[] { _1 }, false },
            { new[] { _1 }, Array.Empty<T>(), false },
            { new[] { _1 }, new[] { _1 }, true },
            { new[] { _1, _1 }, new[] { _1 }, true },
            { new[] { _1 }, new[] { _1, _1 }, true },
            { new[] { _1, _1 }, new[] { _1, _1 }, true },
            { new[] { _1 }, new[] { _2 }, false },
            { new[] { _1 }, new[] { _1, _2 }, false },
            { new[] { _1 }, new[] { _2, _1 }, false },
            { new[] { _1, _2, _3 }, Array.Empty<T>(), false },
            { new[] { _1, _2, _3 }, new[] { _1, _2, _3 }, true },
            { new[] { _1, _3, _2 }, new[] { _2, _1, _3 }, true },
            { new[] { _1, _1, _2, _2, _3, _3 }, new[] { _1, _2, _3 }, true },
            { new[] { _3, _2, _1 }, new[] { _1, _2, _3, _1, _2, _3 }, true },
            { new[] { _1, _1, _2, _2, _3, _3 }, new[] { _3, _2, _1, _3, _2, _1 }, true },
            { new[] { _1, _1, _2, _2, _3, _3 }, new[] { _3, _2, _1, _3, _2, _1 }, true },
            { new[] { _1, _2, _3 }, new[] { _1, _2 }, false },
            { new[] { _1, _2, _3 }, new[] { _3, _1 }, false },
            { new[] { _1, _2, _3 }, new[] { _2 }, false },
            { new[] { _1, _2 }, new[] { _2, _3 }, false },
            { new HashSet<T> { _1 }, new[] { _1 }, true },
            { new HashSet<T> { _1 }, new[] { _2 }, false }
        };
    }

    public static TheoryData<IEnumerable<T>, int, int, T[]> GetSliceData(Fixture fixture)
    {
        var items = fixture.CreateManyDistinct<T>( count: 3 );
        var (_1, _2, _3) = items;

        return new TheoryData<IEnumerable<T>, int, int, T[]>
        {
            { Array.Empty<T>(), 0, 0, Array.Empty<T>() },
            { Array.Empty<T>(), 0, 1, Array.Empty<T>() },
            { Array.Empty<T>(), 1, 1, Array.Empty<T>() },
            { items, 0, -1, Array.Empty<T>() },
            { items, -1, -1, Array.Empty<T>() },
            { items, -2, 1, Array.Empty<T>() },
            { items, -2, 2, Array.Empty<T>() },
            { items, -2, 3, new[] { _1 } },
            { items, -2, 4, new[] { _1, _2 } },
            { items, -2, 5, new[] { _1, _2, _3 } },
            { items, -2, 6, new[] { _1, _2, _3 } },
            { items, -1, 0, Array.Empty<T>() },
            { items, -1, 1, Array.Empty<T>() },
            { items, -1, 2, new[] { _1 } },
            { items, -1, 3, new[] { _1, _2 } },
            { items, -1, 4, new[] { _1, _2, _3 } },
            { items, -1, 5, new[] { _1, _2, _3 } },
            { items, 0, 0, Array.Empty<T>() },
            { items, 0, 1, new[] { _1 } },
            { items, 0, 2, new[] { _1, _2 } },
            { items, 0, 3, new[] { _1, _2, _3 } },
            { items, 0, 4, new[] { _1, _2, _3 } },
            { items, 1, 0, Array.Empty<T>() },
            { items, 1, 1, new[] { _2 } },
            { items, 1, 2, new[] { _2, _3 } },
            { items, 1, 3, new[] { _2, _3 } },
            { items, 2, 0, Array.Empty<T>() },
            { items, 2, 1, new[] { _3 } },
            { items, 2, 2, new[] { _3 } },
            { items, 3, 0, Array.Empty<T>() },
            { items, 3, 1, Array.Empty<T>() }
        };
    }

    public static TheoryData<IEnumerable<T>, bool> GetIsOrderedData(Fixture fixture)
    {
        var (_1, _2, _3) = fixture.CreateManyDistinctSorted<T>( count: 3 );

        return new TheoryData<IEnumerable<T>, bool>
        {
            { new[] { _1, _2 }, true },
            { new[] { _1, _1 }, true },
            { new[] { _1, _1, _2 }, true },
            { new[] { _1, _2, _2 }, true },
            { new[] { _1, _2, _3 }, true },
            { new[] { _2, _1 }, false },
            { new[] { _2, _2, _1 }, false },
            { new[] { _2, _1, _1 }, false },
            { new[] { _3, _2, _1 }, false },
            { new[] { _1, _3, _2 }, false },
            { new[] { _2, _1, _3 }, false }
        };
    }
}

public sealed class VisitManyNode<T>
{
    public T? Value { get; init; }
    public List<VisitManyNode<T>> Children { get; init; } = new();

    public override string ToString()
    {
        return $"{{{Value}}} -> {{{string.Join( ", ", Children )}}}";
    }
}

public sealed class Contained<T>
{
    public T? Value { get; init; }

    public override string ToString()
    {
        return $"{{{Value}}}";
    }

    [Pure]
    public override bool Equals(object? obj)
    {
        return ReferenceEquals( obj, this ) || (obj is Contained<T> c && EqualityComparer<T>.Default.Equals( Value, c.Value ));
    }

    [Pure]
    public override int GetHashCode()
    {
        return HashCode.Combine( Value );
    }
}

public sealed class TestCollection<T> : IReadOnlyCollection<T>
{
    public int Count => 0;

    public IEnumerator<T> GetEnumerator()
    {
        return Enumerable.Empty<T>().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
