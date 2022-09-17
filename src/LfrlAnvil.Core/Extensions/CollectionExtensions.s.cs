﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Extensions;

public static class CollectionExtensions
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsNullOrEmpty<T>([NotNullWhen( false )] this IReadOnlyCollection<T>? source)
    {
        return source is null || source.IsEmpty();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsEmpty<T>(this IReadOnlyCollection<T> source)
    {
        return source.Count == 0;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool ContainsAtLeast<T>(this IReadOnlyCollection<T> source, int count)
    {
        return source.Count >= count;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool ContainsAtMost<T>(this IReadOnlyCollection<T> source, int count)
    {
        return source.Count <= count;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool ContainsBetween<T>(this IReadOnlyCollection<T> source, int minCount, int maxCount)
    {
        return source.Count >= minCount && source.Count <= maxCount;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool ContainsExactly<T>(this IReadOnlyCollection<T> source, int count)
    {
        return source.Count == count;
    }

    [Pure]
    public static TResult[] ToArray<TSource, TResult>(this IReadOnlyCollection<TSource> source, Func<TSource, TResult> selector)
    {
        var count = source.Count;
        if ( count == 0 )
            return Array.Empty<TResult>();

        var index = 0;
        var result = new TResult[count];
        foreach ( var e in source )
            result[index++] = selector( e );

        return result;
    }
}
