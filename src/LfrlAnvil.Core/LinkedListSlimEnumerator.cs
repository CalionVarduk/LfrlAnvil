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

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LfrlAnvil.Internal;

namespace LfrlAnvil;

/// <summary>
/// Lightweight enumerator implementation for a slim linked list.
/// </summary>
public ref struct LinkedListSlimEnumerator<T>
{
    private readonly ref LinkedEntry<T> _first;
    private NullableIndex _current;
    private NullableIndex _next;

    internal LinkedListSlimEnumerator(LinkedEntry<T>[] items, NullableIndex head)
    {
        _first = ref MemoryMarshal.GetArrayDataReference( items );
        _current = NullableIndex.Null;
        _next = head;
    }

    /// <summary>
    /// Gets an element in the view, along with its index, at the current position of the enumerator.
    /// </summary>
    public KeyValuePair<int, T> Current
    {
        get
        {
            ref var entry = ref GetEntryRef( _current );
            return KeyValuePair.Create( _current.Value, entry.Value );
        }
    }

    /// <summary>
    /// Advances the enumerator to the next element of the collection.
    /// </summary>
    /// <returns><b>true</b> if the enumerator was successfully advanced to the next element, otherwise <b>false</b>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool MoveNext()
    {
        if ( ! _next.HasValue )
        {
            _current = NullableIndex.Null;
            return false;
        }

        ref var entry = ref GetEntryRef( _next );
        _current = _next;
        _next = entry.Next;
        return true;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private ref LinkedEntry<T> GetEntryRef(NullableIndex index)
    {
        return ref Unsafe.Add( ref _first, index.Value );
    }
}
