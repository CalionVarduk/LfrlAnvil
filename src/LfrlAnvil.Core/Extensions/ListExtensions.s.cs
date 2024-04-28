using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Extensions;

/// <summary>
/// Contains <see cref="IList{T}"/> extension methods.
/// </summary>
public static class ListExtensions
{
    /// <summary>
    /// Swaps two items in the provided list.
    /// </summary>
    /// <param name="list">Source list.</param>
    /// <param name="index1">Index of the first item to swap.</param>
    /// <param name="index2">Index of the second item to swap.</param>
    /// <typeparam name="T">List item type.</typeparam>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void SwapItems<T>(this IList<T> list, int index1, int index2)
    {
        (list[index2], list[index1]) = (list[index1], list[index2]);
    }

    /// <summary>
    /// Removes the last item from the provided list.
    /// </summary>
    /// <param name="list">Source list.</param>
    /// <typeparam name="T">List item type.</typeparam>
    /// <exception cref="ArgumentOutOfRangeException">When list is empty.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void RemoveLast<T>(this IList<T> list)
    {
        list.RemoveAt( list.Count - 1 );
    }
}
