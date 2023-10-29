using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Memory;

namespace LfrlAnvil.Extensions;

public static class CollectionExtensions
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IReadOnlyCollection<T> EmptyIfNull<T>(this IReadOnlyCollection<T>? source)
    {
        return source ?? Array.Empty<T>();
    }

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

    public static void CopyTo<T>(this IReadOnlyCollection<T> source, RentedMemorySequenceSpan<T> span)
    {
        Ensure.ContainsAtMost( source, span.Length );

        var index = 0;
        foreach ( var e in source )
            span[index++] = e;
    }
}
