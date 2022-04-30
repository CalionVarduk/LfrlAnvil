using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Extensions
{
    public static class ObjectExtensions
    {
        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static One<T> ToOne<T>(this T source)
        {
            return One.Create( source );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static T? ToNullable<T>(this T source)
            where T : struct
        {
            return source;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static IMemoizedCollection<T2> Memoize<T1, T2>(this T1 source, Func<T1, IEnumerable<T2>> selector)
        {
            return selector( source ).Memoize();
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static IEnumerable<T> Visit<T>(this T? source, Func<T, T?> nodeSelector)
            where T : class
        {
            return source.Visit( nodeSelector!, e => e is null )!;
        }

        [Pure]
        public static IEnumerable<T> Visit<T>(this T source, Func<T, T> nodeSelector, Func<T, bool> breakPredicate)
        {
            if ( breakPredicate( source ) )
                yield break;

            var current = nodeSelector( source );

            while ( ! breakPredicate( current ) )
            {
                yield return current;

                current = nodeSelector( current );
            }
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static IEnumerable<T> VisitMany<T>(this T source, Func<T, IEnumerable<T>> nodeRangeSelector)
        {
            return nodeRangeSelector( source ).VisitMany( nodeRangeSelector );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static IEnumerable<T> VisitMany<T>(this T source, Func<T, IEnumerable<T>> nodeRangeSelector, Func<T, bool> stopPredicate)
        {
            if ( stopPredicate( source ) )
                return Enumerable.Empty<T>();

            return nodeRangeSelector( source ).VisitMany( nodeRangeSelector, stopPredicate );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static IEnumerable<T> VisitWithSelf<T>(this T? source, Func<T, T?> nodeSelector)
            where T : class
        {
            return source.VisitWithSelf( nodeSelector!, e => e is null )!;
        }

        [Pure]
        public static IEnumerable<T> VisitWithSelf<T>(this T source, Func<T, T> nodeSelector, Func<T, bool> breakPredicate)
        {
            if ( breakPredicate( source ) )
                yield break;

            yield return source;

            var current = nodeSelector( source );

            while ( ! breakPredicate( current ) )
            {
                yield return current;

                current = nodeSelector( current );
            }
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static IEnumerable<T> VisitManyWithSelf<T>(this T source, Func<T, IEnumerable<T>> nodeRangeSelector)
        {
            return source.VisitMany( nodeRangeSelector ).Prepend( source );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static IEnumerable<T> VisitManyWithSelf<T>(
            this T source,
            Func<T, IEnumerable<T>> nodeRangeSelector,
            Func<T, bool> stopPredicate)
        {
            return source.VisitMany( nodeRangeSelector, stopPredicate ).Prepend( source );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static T Min<T>(this T source, T other)
            where T : IComparable<T>
        {
            return source.CompareTo( other ) <= 0 ? source : other;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static T Max<T>(this T source, T other)
            where T : IComparable<T>
        {
            return source.CompareTo( other ) >= 0 ? source : other;
        }
    }
}
