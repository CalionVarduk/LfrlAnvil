using System.Collections.Generic;

namespace LfrlAnvil.Collections.Tests.MultiHashSetTests;

public class GenericMultiHashSetTestsData<T>
{
    public static TheoryData<int, int, bool> GetContainsData(IFixture fixture)
    {
        return new TheoryData<int, int, bool>
        {
            { 1, -1, true },
            { 1, 0, true },
            { 1, 1, true },
            { 3, 1, true },
            { 3, 2, true },
            { 3, 3, true },
            { 1, 2, false },
            { 3, 4, false }
        };
    }

    public static TheoryData<IEnumerable<Pair<T, int>>, IEnumerable<Pair<T, int>>, IEnumerable<Pair<T, int>>> GetExceptWithData(
        IFixture fixture)
    {
        var (a, b, c, d, e, f) = fixture.CreateDistinctCollection<T>( 6 );

        return new TheoryData<IEnumerable<Pair<T, int>>, IEnumerable<Pair<T, int>>, IEnumerable<Pair<T, int>>>
        {
            {
                Array.Empty<Pair<T, int>>(),
                Array.Empty<Pair<T, int>>(),
                Array.Empty<Pair<T, int>>()
            },
            {
                Array.Empty<Pair<T, int>>(),
                new[] { Get( a, 2 ), Get( b, 3 ) },
                Array.Empty<Pair<T, int>>()
            },
            {
                Array.Empty<Pair<T, int>>(),
                new[] { Get( a, 0 ), Get( b, -1 ) },
                Array.Empty<Pair<T, int>>()
            },
            {
                new[] { Get( a, 2 ), Get( b, 3 ) },
                Array.Empty<Pair<T, int>>(),
                new[] { Get( a, 2 ), Get( b, 3 ) }
            },
            {
                new[] { Get( a, 2 ), Get( b, 3 ), Get( c, 4 ) },
                new[] { Get( a, 2 ), Get( b, 3 ), Get( c, 4 ) },
                Array.Empty<Pair<T, int>>()
            },
            {
                new[] { Get( a, 2 ), Get( b, 3 ), Get( c, 4 ) },
                new[] { Get( a, 3 ), Get( b, 4 ), Get( c, 5 ) },
                Array.Empty<Pair<T, int>>()
            },
            {
                new[] { Get( a, 3 ), Get( b, 5 ), Get( c, 7 ) },
                new[] { Get( a, 2 ), Get( b, 3 ), Get( c, 4 ) },
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ) }
            },
            {
                new[] { Get( a, 2 ), Get( b, 3 ), Get( c, 4 ) },
                new[] { Get( a, 0 ), Get( b, -1 ), Get( c, -2 ) },
                new[] { Get( a, 2 ), Get( b, 3 ), Get( c, 4 ) }
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( b, 2 ), Get( c, 3 ) },
                new[] { Get( a, 1 ), Get( d, 4 ) }
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( b, 3 ), Get( c, 4 ) },
                new[] { Get( a, 1 ), Get( d, 4 ) }
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 4 ), Get( d, 4 ) },
                new[] { Get( b, 1 ), Get( c, 2 ) },
                new[] { Get( a, 1 ), Get( b, 1 ), Get( c, 2 ), Get( d, 4 ) }
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( b, 0 ), Get( c, -1 ) },
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) }
            },
            {
                new[] { Get( b, 1 ), Get( c, 2 ) },
                new[] { Get( a, 1 ), Get( b, 1 ), Get( c, 2 ), Get( d, 4 ) },
                Array.Empty<Pair<T, int>>()
            },
            {
                new[] { Get( b, 1 ), Get( c, 2 ) },
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                Array.Empty<Pair<T, int>>()
            },
            {
                new[] { Get( b, 3 ), Get( c, 5 ) },
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( b, 1 ), Get( c, 2 ) }
            },
            {
                new[] { Get( b, 1 ), Get( c, 2 ) },
                new[] { Get( a, 0 ), Get( b, -1 ), Get( c, -2 ), Get( d, -3 ) },
                new[] { Get( b, 1 ), Get( c, 2 ) }
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( c, 3 ), Get( d, 4 ), Get( e, 1 ), Get( f, 2 ) },
                new[] { Get( a, 1 ), Get( b, 2 ) }
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( c, 4 ), Get( d, 5 ), Get( e, 1 ), Get( f, 2 ) },
                new[] { Get( a, 1 ), Get( b, 2 ) }
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 5 ) },
                new[] { Get( c, 2 ), Get( d, 3 ), Get( e, 1 ), Get( f, 2 ) },
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 1 ), Get( d, 2 ) }
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( c, 0 ), Get( d, -1 ), Get( e, -2 ), Get( f, -3 ) },
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) }
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ) },
                new[] { Get( d, 1 ), Get( e, 2 ), Get( f, 3 ) },
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ) }
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ) },
                new[] { Get( d, 0 ), Get( e, -1 ), Get( f, -2 ) },
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ) }
            }
        };
    }

    public static TheoryData<IEnumerable<Pair<T, int>>, IEnumerable<Pair<T, int>>, IEnumerable<Pair<T, int>>> GetUnionWithData(
        IFixture fixture)
    {
        var (a, b, c, d, e, f) = fixture.CreateDistinctCollection<T>( 6 );

        return new TheoryData<IEnumerable<Pair<T, int>>, IEnumerable<Pair<T, int>>, IEnumerable<Pair<T, int>>>
        {
            {
                Array.Empty<Pair<T, int>>(),
                Array.Empty<Pair<T, int>>(),
                Array.Empty<Pair<T, int>>()
            },
            {
                Array.Empty<Pair<T, int>>(),
                new[] { Get( a, 2 ), Get( b, 3 ) },
                new[] { Get( a, 2 ), Get( b, 3 ) }
            },
            {
                Array.Empty<Pair<T, int>>(),
                new[] { Get( a, 0 ), Get( b, -1 ) },
                Array.Empty<Pair<T, int>>()
            },
            {
                new[] { Get( a, 2 ), Get( b, 3 ) },
                Array.Empty<Pair<T, int>>(),
                new[] { Get( a, 2 ), Get( b, 3 ) }
            },
            {
                new[] { Get( a, 2 ), Get( b, 3 ), Get( c, 4 ) },
                new[] { Get( a, 2 ), Get( b, 3 ), Get( c, 4 ) },
                new[] { Get( a, 2 ), Get( b, 3 ), Get( c, 4 ) }
            },
            {
                new[] { Get( a, 2 ), Get( b, 3 ), Get( c, 4 ) },
                new[] { Get( a, 3 ), Get( b, 4 ), Get( c, 5 ) },
                new[] { Get( a, 3 ), Get( b, 4 ), Get( c, 5 ) }
            },
            {
                new[] { Get( a, 3 ), Get( b, 5 ), Get( c, 7 ) },
                new[] { Get( a, 2 ), Get( b, 3 ), Get( c, 4 ) },
                new[] { Get( a, 3 ), Get( b, 5 ), Get( c, 7 ) }
            },
            {
                new[] { Get( a, 2 ), Get( b, 3 ), Get( c, 4 ) },
                new[] { Get( a, 0 ), Get( b, -1 ), Get( c, -2 ) },
                new[] { Get( a, 2 ), Get( b, 3 ), Get( c, 4 ) }
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( b, 2 ), Get( c, 3 ) },
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) }
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( b, 3 ), Get( c, 4 ) },
                new[] { Get( a, 1 ), Get( b, 3 ), Get( c, 4 ), Get( d, 4 ) }
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 4 ), Get( d, 4 ) },
                new[] { Get( b, 1 ), Get( c, 2 ) },
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 4 ), Get( d, 4 ) }
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( b, 0 ), Get( c, -1 ) },
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) }
            },
            {
                new[] { Get( b, 1 ), Get( c, 2 ) },
                new[] { Get( a, 1 ), Get( b, 1 ), Get( c, 2 ), Get( d, 4 ) },
                new[] { Get( a, 1 ), Get( b, 1 ), Get( c, 2 ), Get( d, 4 ) }
            },
            {
                new[] { Get( b, 1 ), Get( c, 2 ) },
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) }
            },
            {
                new[] { Get( b, 3 ), Get( c, 5 ) },
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( a, 1 ), Get( b, 3 ), Get( c, 5 ), Get( d, 4 ) }
            },
            {
                new[] { Get( b, 1 ), Get( c, 2 ) },
                new[] { Get( a, 0 ), Get( b, -1 ), Get( c, -2 ), Get( d, -3 ) },
                new[] { Get( b, 1 ), Get( c, 2 ) }
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( c, 3 ), Get( d, 4 ), Get( e, 1 ), Get( f, 2 ) },
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ), Get( e, 1 ), Get( f, 2 ) }
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( c, 4 ), Get( d, 5 ), Get( e, 1 ), Get( f, 2 ) },
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 4 ), Get( d, 5 ), Get( e, 1 ), Get( f, 2 ) }
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 5 ) },
                new[] { Get( c, 2 ), Get( d, 3 ), Get( e, 1 ), Get( f, 2 ) },
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 5 ), Get( e, 1 ), Get( f, 2 ) }
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( c, 0 ), Get( d, -1 ), Get( e, -2 ), Get( f, -3 ) },
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) }
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ) },
                new[] { Get( d, 1 ), Get( e, 2 ), Get( f, 3 ) },
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 1 ), Get( e, 2 ), Get( f, 3 ) }
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ) },
                new[] { Get( d, 0 ), Get( e, -1 ), Get( f, -2 ) },
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ) }
            }
        };
    }

    public static TheoryData<IEnumerable<Pair<T, int>>, IEnumerable<Pair<T, int>>, IEnumerable<Pair<T, int>>> GetIntersectWithData(
        IFixture fixture)
    {
        var (a, b, c, d, e, f) = fixture.CreateDistinctCollection<T>( 6 );

        return new TheoryData<IEnumerable<Pair<T, int>>, IEnumerable<Pair<T, int>>, IEnumerable<Pair<T, int>>>
        {
            {
                Array.Empty<Pair<T, int>>(),
                Array.Empty<Pair<T, int>>(),
                Array.Empty<Pair<T, int>>()
            },
            {
                Array.Empty<Pair<T, int>>(),
                new[] { Get( a, 2 ), Get( b, 3 ) },
                Array.Empty<Pair<T, int>>()
            },
            {
                Array.Empty<Pair<T, int>>(),
                new[] { Get( a, 0 ), Get( b, -1 ) },
                Array.Empty<Pair<T, int>>()
            },
            {
                new[] { Get( a, 2 ), Get( b, 3 ) },
                Array.Empty<Pair<T, int>>(),
                Array.Empty<Pair<T, int>>()
            },
            {
                new[] { Get( a, 2 ), Get( b, 3 ), Get( c, 4 ) },
                new[] { Get( a, 2 ), Get( b, 3 ), Get( c, 4 ) },
                new[] { Get( a, 2 ), Get( b, 3 ), Get( c, 4 ) }
            },
            {
                new[] { Get( a, 2 ), Get( b, 3 ), Get( c, 4 ) },
                new[] { Get( a, 3 ), Get( b, 4 ), Get( c, 5 ) },
                new[] { Get( a, 2 ), Get( b, 3 ), Get( c, 4 ) }
            },
            {
                new[] { Get( a, 3 ), Get( b, 5 ), Get( c, 7 ) },
                new[] { Get( a, 2 ), Get( b, 3 ), Get( c, 4 ) },
                new[] { Get( a, 2 ), Get( b, 3 ), Get( c, 4 ) }
            },
            {
                new[] { Get( a, 2 ), Get( b, 3 ), Get( c, 4 ) },
                new[] { Get( a, 0 ), Get( b, -1 ), Get( c, -2 ) },
                Array.Empty<Pair<T, int>>()
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( b, 2 ), Get( c, 3 ) },
                new[] { Get( b, 2 ), Get( c, 3 ) }
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( b, 3 ), Get( c, 4 ) },
                new[] { Get( b, 2 ), Get( c, 3 ) }
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 4 ), Get( d, 4 ) },
                new[] { Get( b, 1 ), Get( c, 2 ) },
                new[] { Get( b, 1 ), Get( c, 2 ) }
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( b, 0 ), Get( c, -1 ) },
                Array.Empty<Pair<T, int>>()
            },
            {
                new[] { Get( b, 1 ), Get( c, 2 ) },
                new[] { Get( a, 1 ), Get( b, 1 ), Get( c, 2 ), Get( d, 4 ) },
                new[] { Get( b, 1 ), Get( c, 2 ) }
            },
            {
                new[] { Get( b, 1 ), Get( c, 2 ) },
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( b, 1 ), Get( c, 2 ) }
            },
            {
                new[] { Get( b, 3 ), Get( c, 5 ) },
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( b, 2 ), Get( c, 3 ) }
            },
            {
                new[] { Get( b, 1 ), Get( c, 2 ) },
                new[] { Get( a, 0 ), Get( b, -1 ), Get( c, -2 ), Get( d, -3 ) },
                Array.Empty<Pair<T, int>>()
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( c, 3 ), Get( d, 4 ), Get( e, 1 ), Get( f, 2 ) },
                new[] { Get( c, 3 ), Get( d, 4 ) }
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( c, 4 ), Get( d, 5 ), Get( e, 1 ), Get( f, 2 ) },
                new[] { Get( c, 3 ), Get( d, 4 ) }
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 5 ) },
                new[] { Get( c, 2 ), Get( d, 3 ), Get( e, 1 ), Get( f, 2 ) },
                new[] { Get( c, 2 ), Get( d, 3 ) }
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( c, 0 ), Get( d, -1 ), Get( e, -2 ), Get( f, -3 ) },
                Array.Empty<Pair<T, int>>()
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ) },
                new[] { Get( d, 1 ), Get( e, 2 ), Get( f, 3 ) },
                Array.Empty<Pair<T, int>>()
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ) },
                new[] { Get( d, 0 ), Get( e, -1 ), Get( f, -2 ) },
                Array.Empty<Pair<T, int>>()
            }
        };
    }

    public static TheoryData<IEnumerable<Pair<T, int>>, IEnumerable<Pair<T, int>>, IEnumerable<Pair<T, int>>>
        GetSymmetricExceptWithData(
            IFixture fixture)
    {
        var (a, b, c, d, e, f) = fixture.CreateDistinctCollection<T>( 6 );

        return new TheoryData<IEnumerable<Pair<T, int>>, IEnumerable<Pair<T, int>>, IEnumerable<Pair<T, int>>>
        {
            {
                Array.Empty<Pair<T, int>>(),
                Array.Empty<Pair<T, int>>(),
                Array.Empty<Pair<T, int>>()
            },
            {
                Array.Empty<Pair<T, int>>(),
                new[] { Get( a, 2 ), Get( b, 3 ) },
                new[] { Get( a, 2 ), Get( b, 3 ) }
            },
            {
                Array.Empty<Pair<T, int>>(),
                new[] { Get( a, 0 ), Get( b, -1 ) },
                Array.Empty<Pair<T, int>>()
            },
            {
                new[] { Get( a, 2 ), Get( b, 3 ) },
                Array.Empty<Pair<T, int>>(),
                new[] { Get( a, 2 ), Get( b, 3 ) }
            },
            {
                new[] { Get( a, 2 ), Get( b, 3 ), Get( c, 4 ) },
                new[] { Get( a, 2 ), Get( b, 3 ), Get( c, 4 ) },
                Array.Empty<Pair<T, int>>()
            },
            {
                new[] { Get( a, 2 ), Get( b, 3 ), Get( c, 4 ) },
                new[] { Get( a, 3 ), Get( b, 4 ), Get( c, 5 ) },
                new[] { Get( a, 1 ), Get( b, 1 ), Get( c, 1 ) }
            },
            {
                new[] { Get( a, 3 ), Get( b, 5 ), Get( c, 7 ) },
                new[] { Get( a, 2 ), Get( b, 3 ), Get( c, 4 ) },
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ) }
            },
            {
                new[] { Get( a, 2 ), Get( b, 3 ), Get( c, 4 ) },
                new[] { Get( a, 0 ), Get( b, -1 ), Get( c, -2 ) },
                new[] { Get( a, 2 ), Get( b, 3 ), Get( c, 4 ) }
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( b, 2 ), Get( c, 3 ) },
                new[] { Get( a, 1 ), Get( d, 4 ) }
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( b, 3 ), Get( c, 4 ) },
                new[] { Get( a, 1 ), Get( b, 1 ), Get( c, 1 ), Get( d, 4 ) }
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 4 ), Get( d, 4 ) },
                new[] { Get( b, 1 ), Get( c, 2 ) },
                new[] { Get( a, 1 ), Get( b, 1 ), Get( c, 2 ), Get( d, 4 ) }
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( b, 0 ), Get( c, -1 ) },
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) }
            },
            {
                new[] { Get( b, 1 ), Get( c, 2 ) },
                new[] { Get( a, 1 ), Get( b, 1 ), Get( c, 2 ), Get( d, 4 ) },
                new[] { Get( a, 1 ), Get( d, 4 ) }
            },
            {
                new[] { Get( b, 1 ), Get( c, 2 ) },
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( a, 1 ), Get( b, 1 ), Get( c, 1 ), Get( d, 4 ) }
            },
            {
                new[] { Get( b, 3 ), Get( c, 5 ) },
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( a, 1 ), Get( b, 1 ), Get( c, 2 ), Get( d, 4 ) }
            },
            {
                new[] { Get( b, 1 ), Get( c, 2 ) },
                new[] { Get( a, 0 ), Get( b, -1 ), Get( c, -2 ), Get( d, -3 ) },
                new[] { Get( b, 1 ), Get( c, 2 ) }
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( c, 3 ), Get( d, 4 ), Get( e, 1 ), Get( f, 2 ) },
                new[] { Get( a, 1 ), Get( b, 2 ), Get( e, 1 ), Get( f, 2 ) }
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( c, 4 ), Get( d, 5 ), Get( e, 1 ), Get( f, 2 ) },
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 1 ), Get( d, 1 ), Get( e, 1 ), Get( f, 2 ) }
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 5 ) },
                new[] { Get( c, 2 ), Get( d, 3 ), Get( e, 1 ), Get( f, 2 ) },
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 1 ), Get( d, 2 ), Get( e, 1 ), Get( f, 2 ) }
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( c, 0 ), Get( d, -1 ), Get( e, -2 ), Get( f, -3 ) },
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) }
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ) },
                new[] { Get( d, 1 ), Get( e, 2 ), Get( f, 3 ) },
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 1 ), Get( e, 2 ), Get( f, 3 ) }
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ) },
                new[] { Get( d, 0 ), Get( e, -1 ), Get( f, -2 ) },
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ) }
            }
        };
    }

    public static TheoryData<IEnumerable<Pair<T, int>>, IEnumerable<Pair<T, int>>, bool> GetOverlapsData(
        IFixture fixture)
    {
        var (a, b, c, d, e, f) = fixture.CreateDistinctCollection<T>( 6 );

        return new TheoryData<IEnumerable<Pair<T, int>>, IEnumerable<Pair<T, int>>, bool>
        {
            {
                Array.Empty<Pair<T, int>>(),
                Array.Empty<Pair<T, int>>(),
                false
            },
            {
                Array.Empty<Pair<T, int>>(),
                new[] { Get( a, 2 ), Get( b, 3 ) },
                false
            },
            {
                Array.Empty<Pair<T, int>>(),
                new[] { Get( a, 0 ), Get( b, -1 ) },
                false
            },
            {
                new[] { Get( a, 2 ), Get( b, 3 ) },
                Array.Empty<Pair<T, int>>(),
                false
            },
            {
                new[] { Get( a, 2 ), Get( b, 3 ), Get( c, 4 ) },
                new[] { Get( a, 2 ), Get( b, 3 ), Get( c, 4 ) },
                true
            },
            {
                new[] { Get( a, 2 ), Get( b, 3 ), Get( c, 4 ) },
                new[] { Get( a, 3 ), Get( b, 4 ), Get( c, 5 ) },
                true
            },
            {
                new[] { Get( a, 3 ), Get( b, 5 ), Get( c, 7 ) },
                new[] { Get( a, 2 ), Get( b, 3 ), Get( c, 4 ) },
                true
            },
            {
                new[] { Get( a, 2 ), Get( b, 3 ), Get( c, 4 ) },
                new[] { Get( a, 0 ), Get( b, -1 ), Get( c, -2 ) },
                false
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( b, 2 ), Get( c, 3 ) },
                true
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( b, 3 ), Get( c, 4 ) },
                true
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 4 ), Get( d, 4 ) },
                new[] { Get( b, 1 ), Get( c, 2 ) },
                true
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( b, 0 ), Get( c, -1 ) },
                false
            },
            {
                new[] { Get( b, 1 ), Get( c, 2 ) },
                new[] { Get( a, 1 ), Get( b, 1 ), Get( c, 2 ), Get( d, 4 ) },
                true
            },
            {
                new[] { Get( b, 1 ), Get( c, 2 ) },
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                true
            },
            {
                new[] { Get( b, 3 ), Get( c, 5 ) },
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                true
            },
            {
                new[] { Get( b, 1 ), Get( c, 2 ) },
                new[] { Get( a, 0 ), Get( b, -1 ), Get( c, -2 ), Get( d, -3 ) },
                false
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( c, 3 ), Get( d, 4 ), Get( e, 1 ), Get( f, 2 ) },
                true
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( c, 4 ), Get( d, 5 ), Get( e, 1 ), Get( f, 2 ) },
                true
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 5 ) },
                new[] { Get( c, 2 ), Get( d, 3 ), Get( e, 1 ), Get( f, 2 ) },
                true
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( c, 0 ), Get( d, -1 ), Get( e, -2 ), Get( f, -3 ) },
                false
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ) },
                new[] { Get( d, 1 ), Get( e, 2 ), Get( f, 3 ) },
                false
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ) },
                new[] { Get( d, 0 ), Get( e, -1 ), Get( f, -2 ) },
                false
            }
        };
    }

    public static TheoryData<IEnumerable<Pair<T, int>>, IEnumerable<Pair<T, int>>, bool> GetSetEqualsData(
        IFixture fixture)
    {
        var (a, b, c, d, e, f) = fixture.CreateDistinctCollection<T>( 6 );

        return new TheoryData<IEnumerable<Pair<T, int>>, IEnumerable<Pair<T, int>>, bool>
        {
            {
                Array.Empty<Pair<T, int>>(),
                Array.Empty<Pair<T, int>>(),
                true
            },
            {
                Array.Empty<Pair<T, int>>(),
                new[] { Get( a, 2 ), Get( b, 3 ) },
                false
            },
            {
                Array.Empty<Pair<T, int>>(),
                new[] { Get( a, 0 ), Get( b, -1 ) },
                true
            },
            {
                new[] { Get( a, 2 ), Get( b, 3 ) },
                Array.Empty<Pair<T, int>>(),
                false
            },
            {
                new[] { Get( a, 2 ), Get( b, 3 ), Get( c, 4 ) },
                new[] { Get( a, 2 ), Get( b, 3 ), Get( c, 4 ) },
                true
            },
            {
                new[] { Get( a, 2 ), Get( b, 3 ), Get( c, 4 ) },
                new[] { Get( a, 3 ), Get( b, 4 ), Get( c, 5 ) },
                false
            },
            {
                new[] { Get( a, 3 ), Get( b, 5 ), Get( c, 7 ) },
                new[] { Get( a, 2 ), Get( b, 3 ), Get( c, 4 ) },
                false
            },
            {
                new[] { Get( a, 2 ), Get( b, 3 ), Get( c, 4 ) },
                new[] { Get( a, 0 ), Get( b, -1 ), Get( c, -2 ) },
                false
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( b, 2 ), Get( c, 3 ) },
                false
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( b, 3 ), Get( c, 4 ) },
                false
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 4 ), Get( d, 4 ) },
                new[] { Get( b, 1 ), Get( c, 2 ) },
                false
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( b, 0 ), Get( c, -1 ) },
                false
            },
            {
                new[] { Get( b, 1 ), Get( c, 2 ) },
                new[] { Get( a, 1 ), Get( b, 1 ), Get( c, 2 ), Get( d, 4 ) },
                false
            },
            {
                new[] { Get( b, 1 ), Get( c, 2 ) },
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                false
            },
            {
                new[] { Get( b, 3 ), Get( c, 5 ) },
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                false
            },
            {
                new[] { Get( b, 1 ), Get( c, 2 ) },
                new[] { Get( a, 0 ), Get( b, -1 ), Get( c, -2 ), Get( d, -3 ) },
                false
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( c, 3 ), Get( d, 4 ), Get( e, 1 ), Get( f, 2 ) },
                false
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( c, 4 ), Get( d, 5 ), Get( e, 1 ), Get( f, 2 ) },
                false
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 5 ) },
                new[] { Get( c, 2 ), Get( d, 3 ), Get( e, 1 ), Get( f, 2 ) },
                false
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( c, 0 ), Get( d, -1 ), Get( e, -2 ), Get( f, -3 ) },
                false
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ) },
                new[] { Get( d, 1 ), Get( e, 2 ), Get( f, 3 ) },
                false
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ) },
                new[] { Get( d, 0 ), Get( e, -1 ), Get( f, -2 ) },
                false
            }
        };
    }

    public static TheoryData<IEnumerable<Pair<T, int>>, IEnumerable<Pair<T, int>>, bool> GetIsSupersetOfData(
        IFixture fixture)
    {
        var (a, b, c, d, e, f) = fixture.CreateDistinctCollection<T>( 6 );

        return new TheoryData<IEnumerable<Pair<T, int>>, IEnumerable<Pair<T, int>>, bool>
        {
            {
                Array.Empty<Pair<T, int>>(),
                Array.Empty<Pair<T, int>>(),
                true
            },
            {
                Array.Empty<Pair<T, int>>(),
                new[] { Get( a, 2 ), Get( b, 3 ) },
                false
            },
            {
                Array.Empty<Pair<T, int>>(),
                new[] { Get( a, 0 ), Get( b, -1 ) },
                true
            },
            {
                new[] { Get( a, 2 ), Get( b, 3 ) },
                Array.Empty<Pair<T, int>>(),
                true
            },
            {
                new[] { Get( a, 2 ), Get( b, 3 ), Get( c, 4 ) },
                new[] { Get( a, 2 ), Get( b, 3 ), Get( c, 4 ) },
                true
            },
            {
                new[] { Get( a, 2 ), Get( b, 3 ), Get( c, 4 ) },
                new[] { Get( a, 3 ), Get( b, 4 ), Get( c, 5 ) },
                false
            },
            {
                new[] { Get( a, 3 ), Get( b, 5 ), Get( c, 7 ) },
                new[] { Get( a, 2 ), Get( b, 3 ), Get( c, 4 ) },
                true
            },
            {
                new[] { Get( a, 2 ), Get( b, 3 ), Get( c, 4 ) },
                new[] { Get( a, 0 ), Get( b, -1 ), Get( c, -2 ) },
                true
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( b, 2 ), Get( c, 3 ) },
                true
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( b, 3 ), Get( c, 4 ) },
                false
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 4 ), Get( d, 4 ) },
                new[] { Get( b, 1 ), Get( c, 2 ) },
                true
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( b, 0 ), Get( c, -1 ) },
                true
            },
            {
                new[] { Get( b, 1 ), Get( c, 2 ) },
                new[] { Get( a, 1 ), Get( b, 1 ), Get( c, 2 ), Get( d, 4 ) },
                false
            },
            {
                new[] { Get( b, 1 ), Get( c, 2 ) },
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                false
            },
            {
                new[] { Get( b, 3 ), Get( c, 5 ) },
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                false
            },
            {
                new[] { Get( b, 1 ), Get( c, 2 ) },
                new[] { Get( a, 0 ), Get( b, -1 ), Get( c, -2 ), Get( d, -3 ) },
                true
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( c, 3 ), Get( d, 4 ), Get( e, 1 ), Get( f, 2 ) },
                false
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( c, 4 ), Get( d, 5 ), Get( e, 1 ), Get( f, 2 ) },
                false
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 5 ) },
                new[] { Get( c, 2 ), Get( d, 3 ), Get( e, 1 ), Get( f, 2 ) },
                false
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( c, 0 ), Get( d, -1 ), Get( e, -2 ), Get( f, -3 ) },
                true
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ) },
                new[] { Get( d, 1 ), Get( e, 2 ), Get( f, 3 ) },
                false
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ) },
                new[] { Get( d, 0 ), Get( e, -1 ), Get( f, -2 ) },
                true
            }
        };
    }

    public static TheoryData<IEnumerable<Pair<T, int>>, IEnumerable<Pair<T, int>>, bool> GetIsProperSupersetOfData(
        IFixture fixture)
    {
        var (a, b, c, d, e, f) = fixture.CreateDistinctCollection<T>( 6 );

        return new TheoryData<IEnumerable<Pair<T, int>>, IEnumerable<Pair<T, int>>, bool>
        {
            {
                Array.Empty<Pair<T, int>>(),
                Array.Empty<Pair<T, int>>(),
                false
            },
            {
                Array.Empty<Pair<T, int>>(),
                new[] { Get( a, 2 ), Get( b, 3 ) },
                false
            },
            {
                Array.Empty<Pair<T, int>>(),
                new[] { Get( a, 0 ), Get( b, -1 ) },
                false
            },
            {
                new[] { Get( a, 2 ), Get( b, 3 ) },
                Array.Empty<Pair<T, int>>(),
                true
            },
            {
                new[] { Get( a, 2 ), Get( b, 3 ), Get( c, 4 ) },
                new[] { Get( a, 2 ), Get( b, 3 ), Get( c, 4 ) },
                false
            },
            {
                new[] { Get( a, 2 ), Get( b, 3 ), Get( c, 4 ) },
                new[] { Get( a, 3 ), Get( b, 4 ), Get( c, 5 ) },
                false
            },
            {
                new[] { Get( a, 3 ), Get( b, 5 ), Get( c, 7 ) },
                new[] { Get( a, 2 ), Get( b, 3 ), Get( c, 4 ) },
                true
            },
            {
                new[] { Get( a, 2 ), Get( b, 3 ), Get( c, 4 ) },
                new[] { Get( a, 0 ), Get( b, -1 ), Get( c, -2 ) },
                true
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( b, 2 ), Get( c, 3 ) },
                true
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( b, 3 ), Get( c, 4 ) },
                false
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 4 ), Get( d, 4 ) },
                new[] { Get( b, 1 ), Get( c, 2 ) },
                true
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( b, 0 ), Get( c, -1 ) },
                true
            },
            {
                new[] { Get( b, 1 ), Get( c, 2 ) },
                new[] { Get( a, 1 ), Get( b, 1 ), Get( c, 2 ), Get( d, 4 ) },
                false
            },
            {
                new[] { Get( b, 1 ), Get( c, 2 ) },
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                false
            },
            {
                new[] { Get( b, 3 ), Get( c, 5 ) },
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                false
            },
            {
                new[] { Get( b, 1 ), Get( c, 2 ) },
                new[] { Get( a, 0 ), Get( b, -1 ), Get( c, -2 ), Get( d, -3 ) },
                true
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( c, 3 ), Get( d, 4 ), Get( e, 1 ), Get( f, 2 ) },
                false
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( c, 4 ), Get( d, 5 ), Get( e, 1 ), Get( f, 2 ) },
                false
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 5 ) },
                new[] { Get( c, 2 ), Get( d, 3 ), Get( e, 1 ), Get( f, 2 ) },
                false
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( c, 0 ), Get( d, -1 ), Get( e, -2 ), Get( f, -3 ) },
                true
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ) },
                new[] { Get( d, 1 ), Get( e, 2 ), Get( f, 3 ) },
                false
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ) },
                new[] { Get( d, 0 ), Get( e, -1 ), Get( f, -2 ) },
                true
            }
        };
    }

    public static TheoryData<IEnumerable<Pair<T, int>>, IEnumerable<Pair<T, int>>, bool> GetIsSubsetOfData(
        IFixture fixture)
    {
        var (a, b, c, d, e, f) = fixture.CreateDistinctCollection<T>( 6 );

        return new TheoryData<IEnumerable<Pair<T, int>>, IEnumerable<Pair<T, int>>, bool>
        {
            {
                Array.Empty<Pair<T, int>>(),
                Array.Empty<Pair<T, int>>(),
                true
            },
            {
                Array.Empty<Pair<T, int>>(),
                new[] { Get( a, 2 ), Get( b, 3 ) },
                true
            },
            {
                Array.Empty<Pair<T, int>>(),
                new[] { Get( a, 0 ), Get( b, -1 ) },
                true
            },
            {
                new[] { Get( a, 2 ), Get( b, 3 ) },
                Array.Empty<Pair<T, int>>(),
                false
            },
            {
                new[] { Get( a, 2 ), Get( b, 3 ), Get( c, 4 ) },
                new[] { Get( a, 2 ), Get( b, 3 ), Get( c, 4 ) },
                true
            },
            {
                new[] { Get( a, 2 ), Get( b, 3 ), Get( c, 4 ) },
                new[] { Get( a, 3 ), Get( b, 4 ), Get( c, 5 ) },
                true
            },
            {
                new[] { Get( a, 3 ), Get( b, 5 ), Get( c, 7 ) },
                new[] { Get( a, 2 ), Get( b, 3 ), Get( c, 4 ) },
                false
            },
            {
                new[] { Get( a, 2 ), Get( b, 3 ), Get( c, 4 ) },
                new[] { Get( a, 0 ), Get( b, -1 ), Get( c, -2 ) },
                false
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( b, 2 ), Get( c, 3 ) },
                false
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( b, 3 ), Get( c, 4 ) },
                false
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 4 ), Get( d, 4 ) },
                new[] { Get( b, 1 ), Get( c, 2 ) },
                false
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( b, 0 ), Get( c, -1 ) },
                false
            },
            {
                new[] { Get( b, 1 ), Get( c, 2 ) },
                new[] { Get( a, 1 ), Get( b, 1 ), Get( c, 2 ), Get( d, 4 ) },
                true
            },
            {
                new[] { Get( b, 1 ), Get( c, 2 ) },
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                true
            },
            {
                new[] { Get( b, 3 ), Get( c, 5 ) },
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                false
            },
            {
                new[] { Get( b, 1 ), Get( c, 2 ) },
                new[] { Get( a, 0 ), Get( b, -1 ), Get( c, -2 ), Get( d, -3 ) },
                false
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( c, 3 ), Get( d, 4 ), Get( e, 1 ), Get( f, 2 ) },
                false
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( c, 4 ), Get( d, 5 ), Get( e, 1 ), Get( f, 2 ) },
                false
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 5 ) },
                new[] { Get( c, 2 ), Get( d, 3 ), Get( e, 1 ), Get( f, 2 ) },
                false
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( c, 0 ), Get( d, -1 ), Get( e, -2 ), Get( f, -3 ) },
                false
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ) },
                new[] { Get( d, 1 ), Get( e, 2 ), Get( f, 3 ) },
                false
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ) },
                new[] { Get( d, 0 ), Get( e, -1 ), Get( f, -2 ) },
                false
            }
        };
    }

    public static TheoryData<IEnumerable<Pair<T, int>>, IEnumerable<Pair<T, int>>, bool> GetIsProperSubsetOfData(
        IFixture fixture)
    {
        var (a, b, c, d, e, f) = fixture.CreateDistinctCollection<T>( 6 );

        return new TheoryData<IEnumerable<Pair<T, int>>, IEnumerable<Pair<T, int>>, bool>
        {
            {
                Array.Empty<Pair<T, int>>(),
                Array.Empty<Pair<T, int>>(),
                false
            },
            {
                Array.Empty<Pair<T, int>>(),
                new[] { Get( a, 2 ), Get( b, 3 ) },
                true
            },
            {
                Array.Empty<Pair<T, int>>(),
                new[] { Get( a, 0 ), Get( b, -1 ) },
                false
            },
            {
                new[] { Get( a, 2 ), Get( b, 3 ) },
                Array.Empty<Pair<T, int>>(),
                false
            },
            {
                new[] { Get( a, 2 ), Get( b, 3 ), Get( c, 4 ) },
                new[] { Get( a, 2 ), Get( b, 3 ), Get( c, 4 ) },
                false
            },
            {
                new[] { Get( a, 2 ), Get( b, 3 ), Get( c, 4 ) },
                new[] { Get( a, 3 ), Get( b, 4 ), Get( c, 5 ) },
                true
            },
            {
                new[] { Get( a, 3 ), Get( b, 5 ), Get( c, 7 ) },
                new[] { Get( a, 2 ), Get( b, 3 ), Get( c, 4 ) },
                false
            },
            {
                new[] { Get( a, 2 ), Get( b, 3 ), Get( c, 4 ) },
                new[] { Get( a, 0 ), Get( b, -1 ), Get( c, -2 ) },
                false
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( b, 2 ), Get( c, 3 ) },
                false
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( b, 3 ), Get( c, 4 ) },
                false
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 4 ), Get( d, 4 ) },
                new[] { Get( b, 1 ), Get( c, 2 ) },
                false
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( b, 0 ), Get( c, -1 ) },
                false
            },
            {
                new[] { Get( b, 1 ), Get( c, 2 ) },
                new[] { Get( a, 1 ), Get( b, 1 ), Get( c, 2 ), Get( d, 4 ) },
                true
            },
            {
                new[] { Get( b, 1 ), Get( c, 2 ) },
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                true
            },
            {
                new[] { Get( b, 3 ), Get( c, 5 ) },
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                false
            },
            {
                new[] { Get( b, 1 ), Get( c, 2 ) },
                new[] { Get( a, 0 ), Get( b, -1 ), Get( c, -2 ), Get( d, -3 ) },
                false
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( c, 3 ), Get( d, 4 ), Get( e, 1 ), Get( f, 2 ) },
                false
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( c, 4 ), Get( d, 5 ), Get( e, 1 ), Get( f, 2 ) },
                false
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 5 ) },
                new[] { Get( c, 2 ), Get( d, 3 ), Get( e, 1 ), Get( f, 2 ) },
                false
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ), Get( d, 4 ) },
                new[] { Get( c, 0 ), Get( d, -1 ), Get( e, -2 ), Get( f, -3 ) },
                false
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ) },
                new[] { Get( d, 1 ), Get( e, 2 ), Get( f, 3 ) },
                false
            },
            {
                new[] { Get( a, 1 ), Get( b, 2 ), Get( c, 3 ) },
                new[] { Get( d, 0 ), Get( e, -1 ), Get( f, -2 ) },
                false
            }
        };
    }

    private static Pair<T, int> Get(T item, int multiplicity)
    {
        return Pair.Create( item, multiplicity );
    }
}
