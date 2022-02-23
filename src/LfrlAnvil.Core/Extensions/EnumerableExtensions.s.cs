﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using LfrlAnvil.Internal;

namespace LfrlAnvil.Extensions
{
    public static class EnumerableExtensions
    {
        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T>? source)
        {
            return source ?? Enumerable.Empty<T>();
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source)
            where T : class
        {
            return source.Where( e => e is not null )!;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source)
            where T : struct
        {
            return source.Where( e => e.HasValue ).Select( e => e!.Value );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source, IEqualityComparer<T> comparer)
        {
            if ( ! Generic<T>.IsReferenceType && ! Generic<T>.IsNullableType )
                return source!;

            return source.Where( e => ! comparer.Equals( e!, default! ) )!;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool ContainsNull<T>(this IEnumerable<T?> source)
            where T : class
        {
            return source.Any( e => e is null );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool ContainsNull<T>(this IEnumerable<T?> source)
            where T : struct
        {
            return source.Any( e => ! e.HasValue );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool ContainsNull<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer)
        {
            if ( ! Generic<T>.IsReferenceType && ! Generic<T>.IsNullableType )
                return false;

            return source.Any( e => comparer.Equals( e!, default! ) );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool IsNullOrEmpty<T>(this IEnumerable<T>? source)
        {
            return source is null || source.IsEmpty();
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool IsEmpty<T>(this IEnumerable<T> source)
        {
            return ! source.Any();
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool ContainsAtLeast<T>(this IEnumerable<T> source, int count)
        {
            return count <= 0 || source.Skip( count - 1 ).Any();
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool ContainsAtMost<T>(this IEnumerable<T> source, int count)
        {
            return count >= 0 && ! source.Skip( count ).Any();
        }

        [Pure]
        public static bool ContainsInRange<T>(this IEnumerable<T> source, int minCount, int maxCount)
        {
            if ( maxCount < minCount )
                return false;

            if ( minCount <= 0 )
                return source.ContainsAtMost( maxCount );

            using var enumerator = source.Skip( minCount - 1 ).GetEnumerator();

            if ( ! enumerator.MoveNext() )
                return false;

            var counter = 1;
            var counterLimit = maxCount - minCount + 1;

            while ( enumerator.MoveNext() )
            {
                if ( ++counter > counterLimit )
                    return false;
            }

            return true;
        }

        [Pure]
        public static bool ContainsExactly<T>(this IEnumerable<T> source, int count)
        {
            if ( count < 0 )
                return false;

            var counter = 0;
            using var enumerator = source.GetEnumerator();

            while ( enumerator.MoveNext() )
            {
                if ( ++counter > count )
                    break;
            }

            return counter == count;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static IEnumerable<T?> AsNullable<T>(this IEnumerable<T> source)
            where T : struct
        {
            return source.Select( e => (T?)e );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static IEnumerable<Pair<T1, T2>> Flatten<T1, T2>(this IEnumerable<T1> source, Func<T1, IEnumerable<T2>> selector)
        {
            return source.Flatten( selector, Pair.Create );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static IEnumerable<TResult> Flatten<T1, T2, TResult>(
            this IEnumerable<T1> source,
            Func<T1, IEnumerable<T2>> selector,
            Func<T1, T2, TResult> resultMapper)
        {
            return source.SelectMany( p => selector( p ).Select( c => resultMapper( p, c ) ) );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool TryMin<T>(this IEnumerable<T> source, [MaybeNullWhen( false )] out T result)
        {
            return source.TryMin( Comparer<T>.Default, out result );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool TryMin<T>(this IEnumerable<T> source, IComparer<T> comparer, [MaybeNullWhen( false )] out T result)
        {
            return source.TryAggregate( (a, b) => comparer.Compare( a, b ) < 0 ? a : b, out result );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool TryMax<T>(this IEnumerable<T> source, [MaybeNullWhen( false )] out T result)
        {
            return source.TryMax( Comparer<T>.Default, out result );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool TryMax<T>(this IEnumerable<T> source, IComparer<T> comparer, [MaybeNullWhen( false )] out T result)
        {
            return source.TryAggregate( (a, b) => comparer.Compare( a, b ) > 0 ? a : b, out result );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool ContainsDuplicates<T>(this IEnumerable<T> source)
        {
            return source.ContainsDuplicates( EqualityComparer<T>.Default );
        }

        [Pure]
        public static bool ContainsDuplicates<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer)
        {
            var set = new HashSet<T>( comparer );

            foreach ( var e in source )
            {
                if ( ! set.Add( e ) )
                    return true;
            }

            return false;
        }

        [Pure]
        public static IEnumerable<T> Repeat<T>(this IEnumerable<T> source, int count)
        {
            if ( count == 0 )
                return Enumerable.Empty<T>();

            if ( count == 1 )
                return source;

            Ensure.IsGreaterThanOrEqualTo( count, 0, nameof( count ) );

            var memoizedSource = source.Memoize();

            var result = memoizedSource.Concat( memoizedSource );
            for ( var i = 2; i < count; ++i )
                result = result.Concat( memoizedSource );

            return result;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static IReadOnlyCollection<T> Materialize<T>(this IEnumerable<T> source)
        {
            return source switch
            {
                IReadOnlyCollection<T> c => c,
                MemoizedEnumerable<T> m => m.Source.Value,
                _ => source.ToList()
            };
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static IEnumerable<T> Memoize<T>(this IEnumerable<T> source)
        {
            return source is MemoizedEnumerable<T> m ? m : new MemoizedEnumerable<T>( source );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool IsMaterialized<T>(this IEnumerable<T> source)
        {
            return source is IReadOnlyCollection<T>;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool IsMemoized<T>(this IEnumerable<T> source)
        {
            return source is MemoizedEnumerable<T>;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool SetEquals<T>(this IEnumerable<T> source, IEnumerable<T> other)
        {
            return source.SetEquals( other, EqualityComparer<T>.Default );
        }

        [Pure]
        public static bool SetEquals<T>(this IEnumerable<T> source, IEnumerable<T> other, IEqualityComparer<T> comparer)
        {
            var sourceSet = source as ISet<T> ?? source.ToHashSet( comparer );
            var otherSet = new HashSet<T>( comparer );

            foreach ( var o in other )
            {
                if ( ! sourceSet.Contains( o ) )
                    return false;

                otherSet.Add( o );
            }

            return sourceSet.Count == otherSet.Count;
        }

        [Pure]
        public static IEnumerable<T> VisitMany<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> nodeRangeSelector)
        {
            var nodesToVisit = new Queue<T>( source );

            foreach ( var n in nodesToVisit )
                yield return n;

            while ( nodesToVisit.Count > 0 )
            {
                var parent = nodesToVisit.Dequeue();
                var nodes = nodeRangeSelector( parent );

                foreach ( var n in nodes )
                {
                    yield return n;

                    nodesToVisit.Enqueue( n );
                }
            }
        }

        // TODO: VisitMany with stopPredicate (predicate causes the node to no longer emit its own subtrees)
        // TODO: this new VisitMany can also be applied to ObjectExtensions

        public static bool TryAggregate<T>(this IEnumerable<T> source, Func<T, T, T> func, [MaybeNullWhen( false )] out T result)
        {
            using var enumerator = source.GetEnumerator();

            if ( ! enumerator.MoveNext() )
            {
                result = default;
                return false;
            }

            result = enumerator.Current;

            while ( enumerator.MoveNext() )
                result = func( result, enumerator.Current );

            return true;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static T1 MaxBy<T1, T2>(this IEnumerable<T1> source, Func<T1, T2> selector)
        {
            return source.MaxBy( selector, Comparer<T2>.Default );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static T1 MaxBy<T1, T2>(this IEnumerable<T1> source, Func<T1, T2> selector, IComparer<T2> comparer)
        {
            return source.Aggregate( (a, b) => comparer.Compare( selector( a ), selector( b ) ) > 0 ? a : b );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static T1 MinBy<T1, T2>(this IEnumerable<T1> source, Func<T1, T2> selector)
        {
            return source.MinBy( selector, Comparer<T2>.Default );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static T1 MinBy<T1, T2>(this IEnumerable<T1> source, Func<T1, T2> selector, IComparer<T2> comparer)
        {
            return source.Aggregate( (a, b) => comparer.Compare( selector( a ), selector( b ) ) < 0 ? a : b );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool TryMaxBy<T1, T2>(this IEnumerable<T1> source, Func<T1, T2> selector, [MaybeNullWhen( false )] out T1 result)
        {
            return source.TryMaxBy( selector, Comparer<T2>.Default, out result );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool TryMaxBy<T1, T2>(
            this IEnumerable<T1> source,
            Func<T1, T2> selector,
            IComparer<T2> comparer,
            [MaybeNullWhen( false )] out T1 result)
        {
            return source.TryAggregate( (a, b) => comparer.Compare( selector( a ), selector( b ) ) > 0 ? a : b, out result );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool TryMinBy<T1, T2>(this IEnumerable<T1> source, Func<T1, T2> selector, [MaybeNullWhen( false )] out T1 result)
        {
            return source.TryMinBy( selector, Comparer<T2>.Default, out result );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool TryMinBy<T1, T2>(
            this IEnumerable<T1> source,
            Func<T1, T2> selector,
            IComparer<T2> comparer,
            [MaybeNullWhen( false )] out T1 result)
        {
            return source.TryAggregate( (a, b) => comparer.Compare( selector( a ), selector( b ) ) < 0 ? a : b, out result );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static IEnumerable<T1> DistinctBy<T1, T2>(this IEnumerable<T1> source, Func<T1, T2> selector)
        {
            return source.DistinctBy( selector, EqualityComparer<T2>.Default );
        }

        [Pure]
        public static IEnumerable<T1> DistinctBy<T1, T2>(this IEnumerable<T1> source, Func<T1, T2> selector, IEqualityComparer<T2> comparer)
        {
            var set = new HashSet<T2>( comparer );

            foreach ( var e in source )
            {
                if ( set.Add( selector( e ) ) )
                    yield return e;
            }
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static IEnumerable<TResult> LeftJoin<T1, T2, TKey, TResult>(
            this IEnumerable<T1> outer,
            IEnumerable<T2> inner,
            Func<T1, TKey> outerKeySelector,
            Func<T2, TKey> innerKeySelector,
            Func<T1, T2?, TResult> resultSelector)
        {
            return outer.LeftJoin( inner, outerKeySelector, innerKeySelector, resultSelector, EqualityComparer<TKey>.Default );
        }

        [Pure]
        public static IEnumerable<TResult> LeftJoin<T1, T2, TKey, TResult>(
            this IEnumerable<T1> outer,
            IEnumerable<T2> inner,
            Func<T1, TKey> outerKeySelector,
            Func<T2, TKey> innerKeySelector,
            Func<T1, T2?, TResult> resultSelector,
            IEqualityComparer<TKey> keyComparer)
        {
            var groups = outer
                .GroupJoin(
                    inner,
                    outerKeySelector,
                    innerKeySelector,
                    (o, i) => (Outer: o, Inner: i),
                    keyComparer );

            foreach ( var (outerElement, innerElementRange) in groups )
            {
                using var innerGroupEnumerator = innerElementRange.GetEnumerator();

                if ( ! innerGroupEnumerator.MoveNext() )
                {
                    yield return resultSelector( outerElement, default );

                    continue;
                }

                yield return resultSelector( outerElement, innerGroupEnumerator.Current );

                while ( innerGroupEnumerator.MoveNext() )
                    yield return resultSelector( outerElement, innerGroupEnumerator.Current );
            }
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static IEnumerable<TResult> FullJoin<T1, T2, TKey, TResult>(
            this IEnumerable<T1> outer,
            IEnumerable<T2> inner,
            Func<T1, TKey> outerKeySelector,
            Func<T2, TKey> innerKeySelector,
            Func<T1?, T2?, TResult> resultSelector)
        {
            return outer.FullJoin( inner, outerKeySelector, innerKeySelector, resultSelector, EqualityComparer<TKey>.Default );
        }

        [Pure]
        public static IEnumerable<TResult> FullJoin<T1, T2, TKey, TResult>(
            this IEnumerable<T1> outer,
            IEnumerable<T2> inner,
            Func<T1, TKey> outerKeySelector,
            Func<T2, TKey> innerKeySelector,
            Func<T1?, T2?, TResult> resultSelector,
            IEqualityComparer<TKey> keyComparer)
        {
            var innerMap = inner.ToLookup( innerKeySelector, keyComparer );
            var joinedOuterKeys = new HashSet<TKey>( keyComparer );

            foreach ( var o in outer )
            {
                var key = outerKeySelector( o );
                var innerGroup = innerMap[key];

                using var innerGroupEnumerator = innerGroup.GetEnumerator();

                if ( ! innerGroupEnumerator.MoveNext() )
                {
                    yield return resultSelector( o, default );

                    continue;
                }

                yield return resultSelector( o, innerGroupEnumerator.Current );

                while ( innerGroupEnumerator.MoveNext() )
                    yield return resultSelector( o, innerGroupEnumerator.Current );

                joinedOuterKeys.Add( key );
            }

            foreach ( var g in innerMap )
            {
                if ( joinedOuterKeys.Contains( g.Key ) )
                    continue;

                foreach ( var i in g )
                    yield return resultSelector( default, i );
            }
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool IsOrdered<T>(this IEnumerable<T> source)
        {
            return source.IsOrdered( Comparer<T>.Default );
        }

        [Pure]
        public static bool IsOrdered<T>(this IEnumerable<T> source, IComparer<T> comparer)
        {
            using var enumerator = source.GetEnumerator();

            if ( ! enumerator.MoveNext() )
                return true;

            var previous = enumerator.Current;

            while ( enumerator.MoveNext() )
            {
                var next = enumerator.Current;
                if ( comparer.Compare( previous, next ) > 0 )
                    return false;

                previous = next;
            }

            return true;
        }

        [Pure]
        public static (List<T> Passed, List<T> Failed) Partition<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            var passed = new List<T>();
            var failed = new List<T>();

            foreach ( var e in source )
            {
                var target = predicate( e ) ? passed : failed;
                target.Add( e );
            }

            return (passed, failed);
        }

        [Pure]
        public static IEnumerable<T[]> Divide<T>(this IEnumerable<T> source, int partLength)
        {
            Ensure.IsGreaterThan( partLength, 0, nameof( partLength ) );
            return DivideImpl( source, partLength );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private static IEnumerable<T[]> DivideImpl<T>(IEnumerable<T> source, int partLength)
        {
            using var enumerator = source.GetEnumerator();

            var partIndex = 0;
            var partBuilder = new T[partLength];

            while ( enumerator.MoveNext() )
            {
                partBuilder[partIndex++] = enumerator.Current;
                if ( partIndex < partLength )
                    continue;

                partIndex = 0;

                var part = new T[partLength];
                for ( var i = 0; i < part.Length; ++i )
                    part[i] = partBuilder[i];

                yield return part;
            }

            if ( partIndex == 0 )
                yield break;

            var lastPart = new T[partIndex];
            for ( var i = 0; i < lastPart.Length; ++i )
                lastPart[i] = partBuilder[i];

            yield return lastPart;
        }
    }
}