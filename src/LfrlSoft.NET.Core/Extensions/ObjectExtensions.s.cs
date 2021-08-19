using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlSoft.NET.Core.Collections;
using LfrlSoft.NET.Core.Collections.Internal;
using LfrlSoft.NET.Core.Functional;

namespace LfrlSoft.NET.Core.Extensions
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
        public static Maybe<T> ToMaybe<T>(this T? source)
            where T : notnull
        {
            return source;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static PartialEither<T> ToEither<T>(this T source)
        {
            return new PartialEither<T>( source );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Unsafe<T> ToUnsafe<T>(this T source)
        {
            return new Unsafe<T>( source );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Unsafe<T> ToUnsafe<T>(this Exception source)
        {
            return new Unsafe<T>( source );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Unsafe<Nil> ToUnsafe(this Exception source)
        {
            return source.ToUnsafe<Nil>();
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static IEnumerable<T2> Memoize<T1, T2>(this T1 source, Func<T1, IEnumerable<T2>> selector)
        {
            return new MemoizedEnumerable<T2>( selector( source ) );
        }

        [Pure]
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
        public static IEnumerable<T> VisitMany<T>(this T source, Func<T, IEnumerable<T>> nodeRangeSelector)
        {
            return nodeRangeSelector( source ).VisitMany( nodeRangeSelector );
        }

        [Pure]
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
        public static IEnumerable<T> VisitManyWithSelf<T>(this T source, Func<T, IEnumerable<T>> nodeRangeSelector)
        {
            yield return source;

            foreach ( var node in nodeRangeSelector( source ).VisitMany( nodeRangeSelector ) )
                yield return node;
        }
    }
}
