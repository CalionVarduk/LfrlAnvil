using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Functional.Extensions;

public static class EnumerableExtensions
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<T> SelectValues<T>(this IEnumerable<Maybe<T>> source)
        where T : notnull
    {
        return source.Where( e => e.HasValue ).Select( e => e.Value! );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<T1> SelectFirst<T1, T2>(this IEnumerable<Either<T1, T2>> source)
    {
        return source.Where( e => e.HasFirst ).Select( e => e.First! );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<T2> SelectSecond<T1, T2>(this IEnumerable<Either<T1, T2>> source)
    {
        return source.Where( e => e.HasSecond ).Select( e => e.Second! );
    }

    [Pure]
    public static (List<T1> First, List<T2> Second) Partition<T1, T2>(this IEnumerable<Either<T1, T2>> source)
    {
        var first = new List<T1>();
        var second = new List<T2>();

        foreach ( var e in source )
        {
            if ( e.HasFirst )
                first.Add( e.First! );
            else
                second.Add( e.Second! );
        }

        return (first, second);
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<T> SelectValues<T>(this IEnumerable<Unsafe<T>> source)
    {
        return source.Where( e => e.IsOk ).Select( e => e.Value! );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<Exception> SelectErrors<T>(this IEnumerable<Unsafe<T>> source)
    {
        return source.Where( e => e.HasError ).Select( e => e.Error! );
    }

    [Pure]
    public static (List<T> Values, List<Exception> Errors) Partition<T>(this IEnumerable<Unsafe<T>> source)
    {
        var values = new List<T>();
        var errors = new List<Exception>();

        foreach ( var e in source )
        {
            if ( e.IsOk )
                values.Add( e.Value! );
            else
                errors.Add( e.Error! );
        }

        return (values, errors);
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Maybe<T> TryMin<T>(this IEnumerable<T> source)
        where T : notnull
    {
        return source.TryMin( Comparer<T>.Default );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Maybe<T> TryMin<T>(this IEnumerable<T> source, IComparer<T> comparer)
        where T : notnull
    {
        return source.TryMin( comparer, out var result ) ? new Maybe<T>( result ) : Maybe<T>.None;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Maybe<T> TryMax<T>(this IEnumerable<T> source)
        where T : notnull
    {
        return source.TryMax( Comparer<T>.Default );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Maybe<T> TryMax<T>(this IEnumerable<T> source, IComparer<T> comparer)
        where T : notnull
    {
        return source.TryMax( comparer, out var result ) ? new Maybe<T>( result ) : Maybe<T>.None;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Maybe<T> TryAggregate<T>(this IEnumerable<T> source, Func<T, T, T> func)
        where T : notnull
    {
        return source.TryAggregate( func, out var result ) ? new Maybe<T>( result ) : Maybe<T>.None;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Maybe<T1> TryMaxBy<T1, T2>(this IEnumerable<T1> source, Func<T1, T2> selector)
        where T1 : notnull
    {
        return source.TryMaxBy( selector, Comparer<T2>.Default );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Maybe<T1> TryMaxBy<T1, T2>(this IEnumerable<T1> source, Func<T1, T2> selector, IComparer<T2> comparer)
        where T1 : notnull
    {
        return source.TryMaxBy( selector, comparer, out var result ) ? new Maybe<T1>( result ) : Maybe<T1>.None;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Maybe<T1> TryMinBy<T1, T2>(this IEnumerable<T1> source, Func<T1, T2> selector)
        where T1 : notnull
    {
        return source.TryMinBy( selector, Comparer<T2>.Default );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Maybe<T1> TryMinBy<T1, T2>(this IEnumerable<T1> source, Func<T1, T2> selector, IComparer<T2> comparer)
        where T1 : notnull
    {
        return source.TryMinBy( selector, comparer, out var result ) ? new Maybe<T1>( result ) : Maybe<T1>.None;
    }

    [Pure]
    public static Maybe<T> TryFirst<T>(this IEnumerable<T> source)
        where T : notnull
    {
        if ( source is IReadOnlyList<T> list )
            return list.Count > 0 ? new Maybe<T>( list[0] ) : Maybe<T>.None;

        using var enumerator = source.GetEnumerator();
        return enumerator.MoveNext() ? new Maybe<T>( enumerator.Current! ) : Maybe<T>.None;
    }

    [Pure]
    public static Maybe<T> TryLast<T>(this IEnumerable<T> source)
        where T : notnull
    {
        if ( source is IReadOnlyList<T> list )
            return list.Count > 0 ? new Maybe<T>( list[^1] ) : Maybe<T>.None;

        using var enumerator = source.GetEnumerator();

        if ( ! enumerator.MoveNext() )
            return Maybe<T>.None;

        var last = enumerator.Current;
        while ( enumerator.MoveNext() )
            last = enumerator.Current;

        return new Maybe<T>( last! );
    }

    [Pure]
    public static Maybe<T> TrySingle<T>(this IEnumerable<T> source)
        where T : notnull
    {
        if ( source is IReadOnlyList<T> list )
            return list.Count == 1 ? new Maybe<T>( list[0] ) : Maybe<T>.None;

        using var enumerator = source.GetEnumerator();

        if ( ! enumerator.MoveNext() )
            return Maybe<T>.None;

        var candidate = enumerator.Current;
        return enumerator.MoveNext() ? Maybe<T>.None : new Maybe<T>( candidate! );
    }

    [Pure]
    public static Maybe<T> TryElementAt<T>(this IEnumerable<T> source, int index)
        where T : notnull
    {
        if ( index < 0 )
            return Maybe<T>.None;

        if ( source is IReadOnlyList<T> list )
            return index < list.Count ? new Maybe<T>( list[index] ) : Maybe<T>.None;

        using var enumerator = source.GetEnumerator();

        if ( ! enumerator.MoveNext() )
            return Maybe<T>.None;

        if ( index == 0 )
            return new Maybe<T>( enumerator.Current! );

        --index;
        while ( enumerator.MoveNext() )
        {
            if ( index == 0 )
                return new Maybe<T>( enumerator.Current! );

            --index;
        }

        return Maybe<T>.None;
    }
}
