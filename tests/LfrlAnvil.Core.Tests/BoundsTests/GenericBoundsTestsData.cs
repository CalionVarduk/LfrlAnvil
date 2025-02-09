using System.Collections.Generic;

namespace LfrlAnvil.Tests.BoundsTests;

public class GenericBoundsTestsData<T>
    where T : IComparable<T>
{
    public static TheoryData<T, T, T, T, bool> GetEqualsData(Fixture fixture)
    {
        var (_1, _2, _3, _4) = fixture.CreateManyDistinctSorted<T>( count: 4 );

        return new TheoryData<T, T, T, T, bool>
        {
            { _1, _2, _1, _2, true },
            { _1, _2, _1, _3, false },
            { _1, _3, _2, _3, false },
            { _1, _2, _3, _4, false }
        };
    }

    public static IEnumerable<object?[]> GetNotEqualsData(Fixture fixture)
    {
        return GetEqualsData( fixture ).ConvertResult( (bool r) => ! r );
    }

    public static TheoryData<T, T, T, T> GetClampData(Fixture fixture)
    {
        var (_1, _2, _3) = fixture.CreateManyDistinctSorted<T>( count: 3 );

        return new TheoryData<T, T, T, T>
        {
            { _2, _3, _1, _2 },
            { _1, _2, _1, _1 },
            { _1, _3, _2, _2 },
            { _1, _2, _2, _2 },
            { _1, _2, _3, _2 }
        };
    }

    public static TheoryData<T, T, T, bool> GetContainsData(Fixture fixture)
    {
        var (_1, _2, _3) = fixture.CreateManyDistinctSorted<T>( count: 3 );

        return new TheoryData<T, T, T, bool>
        {
            { _2, _3, _1, false },
            { _1, _3, _1, true },
            { _1, _3, _2, true },
            { _1, _3, _3, true },
            { _1, _2, _3, false }
        };
    }

    public static TheoryData<T, T, T, bool> GetContainsExclusivelyData(Fixture fixture)
    {
        var (_1, _2, _3) = fixture.CreateManyDistinctSorted<T>( count: 3 );

        return new TheoryData<T, T, T, bool>
        {
            { _2, _3, _1, false },
            { _1, _3, _1, false },
            { _1, _3, _2, true },
            { _1, _3, _3, false },
            { _1, _2, _3, false }
        };
    }

    public static TheoryData<T, T, T, T, bool> GetContainsForBoundsData(Fixture fixture)
    {
        var (_1, _2, _3, _4) = fixture.CreateManyDistinctSorted<T>( count: 4 );

        return new TheoryData<T, T, T, T, bool>
        {
            { _1, _1, _1, _1, true },
            { _1, _2, _1, _2, true },
            { _1, _3, _1, _2, true },
            { _1, _3, _2, _3, true },
            { _1, _4, _2, _3, true },
            { _2, _3, _1, _3, false },
            { _1, _2, _1, _3, false },
            { _2, _3, _1, _4, false },
            { _3, _4, _1, _2, false },
            { _2, _3, _1, _2, false },
            { _1, _2, _3, _4, false },
            { _1, _2, _2, _3, false }
        };
    }

    public static TheoryData<T, T, T, T, bool> GetContainsExclusivelyForBoundsData(Fixture fixture)
    {
        var (_1, _2, _3, _4) = fixture.CreateManyDistinctSorted<T>( count: 4 );

        return new TheoryData<T, T, T, T, bool>
        {
            { _1, _1, _1, _1, false },
            { _1, _2, _1, _2, false },
            { _1, _3, _1, _2, false },
            { _1, _3, _2, _3, false },
            { _1, _4, _2, _3, true },
            { _2, _3, _1, _3, false },
            { _1, _2, _1, _3, false },
            { _2, _3, _1, _4, false },
            { _3, _4, _1, _2, false },
            { _2, _3, _1, _2, false },
            { _1, _2, _3, _4, false },
            { _1, _2, _2, _3, false }
        };
    }

    public static TheoryData<T, T, T, T, bool> GetIntersectsForBoundsData(Fixture fixture)
    {
        var (_1, _2, _3, _4) = fixture.CreateManyDistinctSorted<T>( count: 4 );

        return new TheoryData<T, T, T, T, bool>
        {
            { _1, _1, _1, _1, true },
            { _1, _2, _1, _2, true },
            { _1, _3, _1, _2, true },
            { _1, _3, _2, _3, true },
            { _1, _4, _2, _3, true },
            { _2, _3, _1, _3, true },
            { _1, _2, _1, _3, true },
            { _2, _3, _1, _4, true },
            { _3, _4, _1, _2, false },
            { _2, _3, _1, _2, true },
            { _1, _2, _3, _4, false },
            { _1, _2, _2, _3, true }
        };
    }

    public static TheoryData<T, T, T, T, Bounds<T>?> GetIntersectionData(Fixture fixture)
    {
        var (_1, _2, _3, _4) = fixture.CreateManyDistinctSorted<T>( count: 4 );

        return new TheoryData<T, T, T, T, Bounds<T>?>
        {
            { _1, _1, _1, _1, new Bounds<T>( _1, _1 ) },
            { _1, _2, _1, _2, new Bounds<T>( _1, _2 ) },
            { _1, _3, _1, _2, new Bounds<T>( _1, _2 ) },
            { _1, _3, _2, _3, new Bounds<T>( _2, _3 ) },
            { _1, _4, _2, _3, new Bounds<T>( _2, _3 ) },
            { _2, _3, _1, _3, new Bounds<T>( _2, _3 ) },
            { _1, _2, _1, _3, new Bounds<T>( _1, _2 ) },
            { _2, _3, _1, _4, new Bounds<T>( _2, _3 ) },
            { _3, _4, _1, _2, null },
            { _2, _3, _1, _2, new Bounds<T>( _2, _2 ) },
            { _1, _2, _3, _4, null },
            { _1, _2, _2, _3, new Bounds<T>( _2, _2 ) }
        };
    }

    public static TheoryData<T, T, T, T, Bounds<T>?> GetMergeWithData(Fixture fixture)
    {
        var (_1, _2, _3, _4) = fixture.CreateManyDistinctSorted<T>( count: 4 );

        return new TheoryData<T, T, T, T, Bounds<T>?>
        {
            { _1, _2, _1, _2, new Bounds<T>( _1, _2 ) },
            { _1, _3, _1, _2, new Bounds<T>( _1, _3 ) },
            { _1, _3, _2, _3, new Bounds<T>( _1, _3 ) },
            { _1, _4, _2, _3, new Bounds<T>( _1, _4 ) },
            { _2, _3, _1, _3, new Bounds<T>( _1, _3 ) },
            { _1, _2, _1, _3, new Bounds<T>( _1, _3 ) },
            { _2, _3, _1, _4, new Bounds<T>( _1, _4 ) },
            { _3, _4, _1, _2, null },
            { _2, _3, _1, _2, new Bounds<T>( _1, _3 ) },
            { _1, _2, _3, _4, null },
            { _1, _2, _2, _3, new Bounds<T>( _1, _3 ) }
        };
    }

    public static TheoryData<T, T, T, Bounds<T>, Bounds<T>?> GetSplitAtData(Fixture fixture)
    {
        var (_1, _2, _3) = fixture.CreateManyDistinctSorted<T>( count: 3 );

        return new TheoryData<T, T, T, Bounds<T>, Bounds<T>?>
        {
            { _1, _1, _1, new Bounds<T>( _1, _1 ), null },
            { _1, _2, _1, new Bounds<T>( _1, _2 ), null },
            { _1, _2, _2, new Bounds<T>( _1, _2 ), null },
            { _1, _2, _3, new Bounds<T>( _1, _2 ), null },
            { _2, _3, _1, new Bounds<T>( _2, _3 ), null },
            { _1, _3, _2, new Bounds<T>( _1, _2 ), new Bounds<T>( _2, _3 ) }
        };
    }

    public static TheoryData<T, T, T, T, Bounds<T>?, Bounds<T>?> GetRemoveData(Fixture fixture)
    {
        var (_1, _2, _3, _4) = fixture.CreateManyDistinctSorted<T>( count: 4 );

        return new TheoryData<T, T, T, T, Bounds<T>?, Bounds<T>?>
        {
            { _1, _1, _1, _1, null, null },
            { _1, _1, _1, _2, null, null },
            { _2, _2, _1, _2, null, null },
            { _1, _2, _1, _2, null, null },
            { _1, _3, _1, _2, new Bounds<T>( _2, _3 ), null },
            { _1, _3, _2, _3, new Bounds<T>( _1, _2 ), null },
            { _1, _4, _2, _3, new Bounds<T>( _1, _2 ), new Bounds<T>( _3, _4 ) },
            { _2, _3, _1, _3, null, null },
            { _1, _2, _1, _3, null, null },
            { _2, _3, _1, _4, null, null },
            { _3, _4, _1, _2, new Bounds<T>( _3, _4 ), null },
            { _2, _3, _1, _2, new Bounds<T>( _2, _3 ), null },
            { _1, _2, _3, _4, new Bounds<T>( _1, _2 ), null },
            { _1, _2, _2, _3, new Bounds<T>( _1, _2 ), null }
        };
    }
}
