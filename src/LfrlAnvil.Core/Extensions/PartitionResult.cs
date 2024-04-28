using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace LfrlAnvil.Extensions;

/// <summary>
/// Represents a lightweight result of a <see cref="EnumerableExtensions.Partition{T}(IEnumerable{T},Func{T,Boolean})"/> operation.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
public readonly struct PartitionResult<T>
{
    private readonly List<T>? _items;
    private readonly int _passedItemCount;

    internal PartitionResult(List<T> items, int passedItemCount)
    {
        _items = items;
        _passedItemCount = passedItemCount;
    }

    /// <summary>
    /// All items that took part in the partitioning.
    /// </summary>
    public IReadOnlyList<T> Items => _items ?? ( IReadOnlyList<T> )Array.Empty<T>();

    /// <summary>
    /// All items that passed the given partitioning predicate.
    /// </summary>
    public IEnumerable<T> PassedItems => Items.Take( _passedItemCount );

    /// <summary>
    /// All items that failed the given partitioning predicate.
    /// </summary>
    public IEnumerable<T> FailedItems => Items.Skip( _passedItemCount );

    /// <summary>
    /// All items that passed the given partitioning predicate in a <see cref="ReadOnlySpan{T}"/> form.
    /// </summary>
    public ReadOnlySpan<T> PassedItemsSpan => CollectionsMarshal.AsSpan( _items ).Slice( 0, _passedItemCount );

    /// <summary>
    /// All items that failed the given partitioning predicate in a <see cref="ReadOnlySpan{T}"/> form.
    /// </summary>
    public ReadOnlySpan<T> FailedItemsSpan => CollectionsMarshal.AsSpan( _items ).Slice( _passedItemCount );
}
