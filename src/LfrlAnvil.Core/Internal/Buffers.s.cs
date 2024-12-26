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

using System;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LfrlAnvil.Internal;

internal static class Buffers
{
    internal const int MinCapacity = 1 << 2;

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static int GetCapacity(int minCapacity)
    {
        if ( minCapacity <= MinCapacity )
            minCapacity = MinCapacity;

        var result = BitOperations.RoundUpToPowerOf2( unchecked( ( uint )minCapacity ) );
        return result > int.MaxValue ? int.MaxValue : unchecked( ( int )result );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static void ResetLinkedListCapacity<T>(
        ref LinkedEntry<T>[] items,
        ref NullableIndex freeListHead,
        ref NullableIndex freeListTail,
        NullableIndex occupiedListHead,
        int count,
        int minCapacity)
    {
        if ( count == 0 && minCapacity <= 0 )
        {
            freeListHead = freeListTail = NullableIndex.Null;
            items = Array.Empty<LinkedEntry<T>>();
            return;
        }

        if ( minCapacity < count )
            minCapacity = count;

        var capacity = GetCapacity( minCapacity );
        if ( capacity == items.Length )
            return;

        var prevItems = items;
        if ( capacity > items.Length )
        {
            items = new LinkedEntry<T>[capacity];
            prevItems.AsSpan().CopyTo( items );
            return;
        }

        if ( ! freeListTail.HasValue )
        {
            Assume.False( freeListHead.HasValue );
            items = new LinkedEntry<T>[capacity];
            prevItems.AsSpan( 0, count ).CopyTo( items );
            return;
        }

        var endIndex = FindMaxOccupiedIndex( items, occupiedListHead ) + 1;
        if ( endIndex > minCapacity )
        {
            capacity = GetCapacity( endIndex );
            if ( capacity == items.Length )
                return;
        }

        Assume.IsGreaterThanOrEqualTo( endIndex, count );
        Assume.IsLessThan( capacity, items.Length );
        items = new LinkedEntry<T>[capacity];
        prevItems.AsSpan( 0, endIndex ).CopyTo( items );
        RebuildFreeList( items, out freeListHead, out freeListTail, count, endIndex );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static int FindMaxOccupiedIndex<T>(LinkedEntry<T>[] items, NullableIndex occupiedListHead)
    {
        var result = -1;
        if ( ! occupiedListHead.HasValue )
            return result;

        var next = occupiedListHead;
        ref var first = ref MemoryMarshal.GetArrayDataReference( items );
        do
        {
            if ( result < next )
                result = next.Value;

            ref var entry = ref Unsafe.Add( ref first, next.Value );
            next = entry.Next;
        }
        while ( next.HasValue );

        return result;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static void RebuildFreeList<T>(
        LinkedEntry<T>[] items,
        out NullableIndex freeListHead,
        out NullableIndex freeListTail,
        int count,
        int endIndex)
    {
        freeListHead = freeListTail = NullableIndex.Null;
        var remaining = endIndex - count;
        if ( remaining <= 0 )
            return;

        var index = 0;
        ref var first = ref MemoryMarshal.GetArrayDataReference( items );
        ref var entry = ref first;

        while ( ! entry.IsInFreeList )
        {
            Assume.IsLessThan( index, endIndex );
            entry = ref Unsafe.Add( ref entry, 1 );
            ++index;
        }

        entry.MakeFree( NullableIndex.Null, NullableIndex.Null );
        freeListHead = freeListTail = NullableIndex.Create( index );
        --remaining;

        while ( remaining > 0 )
        {
            Assume.IsLessThan( index, endIndex );
            entry = ref Unsafe.Add( ref entry, 1 );
            ++index;

            if ( ! entry.IsInFreeList )
                continue;

            entry.MakeFree( NullableIndex.Null, freeListHead );
            ref var head = ref Unsafe.Add( ref first, freeListHead.Value );
            freeListHead = NullableIndex.Create( index );
            head.MakeFree( freeListHead, head.Next );
            --remaining;
        }
    }
}
