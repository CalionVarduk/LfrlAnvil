using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace LfrlAnvil.Extensions;

public readonly struct PartitionResult<T>
{
    private readonly List<T>? _items;
    private readonly int _passedItemCount;

    internal PartitionResult(List<T> items, int passedItemCount)
    {
        _items = items;
        _passedItemCount = passedItemCount;
    }

    public IReadOnlyList<T> Items => _items ?? (IReadOnlyList<T>)Array.Empty<T>();
    public IEnumerable<T> PassedItems => Items.Take( _passedItemCount );
    public IEnumerable<T> FailedItems => Items.Skip( _passedItemCount );
    public ReadOnlySpan<T> PassedItemsSpan => CollectionsMarshal.AsSpan( _items ).Slice( 0, _passedItemCount );
    public ReadOnlySpan<T> FailedItemsSpan => CollectionsMarshal.AsSpan( _items ).Slice( _passedItemCount );
}
