// Copyright 2024 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LfrlAnvil.Internal;

namespace LfrlAnvil;

/// <summary>
/// Represents a single element node that belongs to a slim linked list instance.
/// </summary>
public readonly struct LinkedListSlimNode<T>
{
    private readonly LinkedEntry<T>[] _items;

    internal LinkedListSlimNode(LinkedEntry<T>[] items, int index)
    {
        _items = items;
        Index = index;
    }

    /// <summary>
    /// Specifies the zero-based index at which this node can be found in its parent list.
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// Gets a reference to this node's element.
    /// </summary>
    public ref T Value
    {
        get
        {
            ref var entry = ref GetEntryRef();
            return ref entry.Value;
        }
    }

    /// <summary>
    /// Gets a predecessor node or null, if this node is the first node in its parent list.
    /// </summary>
    public LinkedListSlimNode<T>? Prev
    {
        get
        {
            ref var entry = ref GetEntryRef();
            var index = entry.Prev;
            return index.HasValue ? new LinkedListSlimNode<T>( _items, index.Value ) : null;
        }
    }

    /// <summary>
    /// Gets a successor node or null, if this node is the last node in its parent list.
    /// </summary>
    public LinkedListSlimNode<T>? Next
    {
        get
        {
            ref var entry = ref GetEntryRef();
            var index = entry.Next;
            return index.HasValue ? new LinkedListSlimNode<T>( _items, index.Value ) : null;
        }
    }

    /// <inheritdoc />
    [Pure]
    public override string ToString()
    {
        return $"[{Index}]: {Value}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private ref LinkedEntry<T> GetEntryRef()
    {
        ref var first = ref MemoryMarshal.GetArrayDataReference( _items );
        ref var entry = ref Unsafe.Add( ref first, Index );
        Assume.True( entry.IsInOccupiedList );
        return ref entry;
    }
}
