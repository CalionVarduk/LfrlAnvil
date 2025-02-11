using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Functional.Tests.ExtensionsTests.EnumerableTests;

public class GenericEnumerableExtensionsTestsData<T>
{
    public static TheoryData<IEnumerable<T>, T> GetTryMinData(Fixture fixture)
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

    public static TheoryData<IEnumerable<T>, T> GetTryMaxData(Fixture fixture)
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

    public static TheoryData<IReadOnlyList<T>, int> GetTryElementAtWithTooLargeIndexData(Fixture fixture)
    {
        var (_1, _2, _3) = fixture.CreateManyDistinct<T>( count: 3 );

        return new TheoryData<IReadOnlyList<T>, int>
        {
            { Array.Empty<T>(), 0 },
            { new[] { _1 }, 1 },
            { new[] { _1, _2 }, 2 },
            { new[] { _1, _2, _3 }, 3 }
        };
    }

    public static TheoryData<IReadOnlyList<T>, int, T> GetTryElementAtData(Fixture fixture)
    {
        var (_1, _2, _3) = fixture.CreateManyDistinct<T>( count: 3 );

        return new TheoryData<IReadOnlyList<T>, int, T>
        {
            { new[] { _1, _2, _3 }, 0, _1 },
            { new[] { _1, _2, _3 }, 1, _2 },
            { new[] { _1, _2, _3 }, 2, _3 }
        };
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
