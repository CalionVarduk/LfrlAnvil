using System.Collections.Generic;
using System.Linq;

namespace LfrlAnvil.TestExtensions;

public static class TestExtensions
{
    public static void Deconstruct<T>(this IReadOnlyList<T> list, out T v1)
    {
        v1 = list[0];
    }

    public static void Deconstruct<T>(this IReadOnlyList<T> list, out T v1, out T v2)
    {
        list.Deconstruct( out v1 );
        v2 = list[1];
    }

    public static void Deconstruct<T>(this IReadOnlyList<T> list, out T v1, out T v2, out T v3)
    {
        list.Deconstruct( out v1, out v2 );
        v3 = list[2];
    }

    public static void Deconstruct<T>(this IReadOnlyList<T> list, out T v1, out T v2, out T v3, out T v4)
    {
        list.Deconstruct( out v1, out v2, out v3 );
        v4 = list[3];
    }

    public static void Deconstruct<T>(this IReadOnlyList<T> list, out T v1, out T v2, out T v3, out T v4, out T v5)
    {
        list.Deconstruct( out v1, out v2, out v3, out v4 );
        v5 = list[4];
    }

    public static void Deconstruct<T>(this IReadOnlyList<T> list, out T v1, out T v2, out T v3, out T v4, out T v5, out T v6)
    {
        list.Deconstruct( out v1, out v2, out v3, out v4, out v5 );
        v6 = list[5];
    }

    public static IEnumerable<object?[]> ConvertResult<TSource, TDest>(this IEnumerable<object?[]> source, Func<TSource, TDest> mapper)
    {
        return source
            .Select( objects =>
            {
                if ( objects[^1] is not TSource sourceResult )
                    throw new InvalidCastException( $"Result is not of {typeof( TSource ).FullName} type" );

                objects[^1] = mapper( sourceResult );
                return objects;
            } );
    }
}
