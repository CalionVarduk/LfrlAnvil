using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;

namespace LfrlAnvil.TestExtensions
{
    public static class FixtureExtensions
    {
        public static T? CreateDefault<T>(this IFixture fixture)
        {
            return default;
        }

        public static T CreateNotDefault<T>(this IFixture fixture)
        {
            return fixture.Create<Generator<T>>().First( v => ! EqualityComparer<T>.Default.Equals( v, default! ) );
        }

        public static IReadOnlyList<T> CreateDistinctCollection<T>(this IFixture fixture, int count)
        {
            var result = new HashSet<T>();

            for ( var i = 0; i < count; ++i )
            {
                var value = fixture.Create<T>();
                while ( ! result.Add( value ) )
                    value = fixture.Create<T>();
            }

            return result.ToArray();
        }

        public static IReadOnlyList<T> CreateDistinctSortedCollection<T>(this IFixture fixture, int count)
        {
            var result = new HashSet<T>();

            for ( var i = 0; i < count; ++i )
            {
                var value = fixture.Create<T>();
                while ( ! result.Add( value ) )
                    value = fixture.Create<T>();
            }

            return result
                .OrderBy( x => x )
                .ToArray();
        }

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
                .Select(
                    objects =>
                    {
                        if ( objects[^1] is not TSource sourceResult )
                            throw new InvalidCastException( $"Result is not of {typeof( TSource ).FullName} type" );

                        objects[^1] = mapper( sourceResult );
                        return objects;
                    } );
        }

        public static T? CreateNullable<T>(this IFixture fixture)
            where T : struct
        {
            return fixture.Create<T>();
        }

        public static T? CreateDefaultNullable<T>(this IFixture fixture)
            where T : struct
        {
            return fixture.CreateDefault<T>();
        }

        public static int CreatePositiveInt32(this IFixture fixture)
        {
            var value = fixture.Create<Generator<int>>().First( x => x != 0 );
            return Math.Abs( value );
        }

        public static int CreateNegativeInt32(this IFixture fixture)
        {
            var value = fixture.Create<Generator<int>>().First( x => x != 0 );
            return value > 0 ? -value : value;
        }
    }
}
