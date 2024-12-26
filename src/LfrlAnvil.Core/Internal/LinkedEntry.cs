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

namespace LfrlAnvil.Internal;

internal struct LinkedEntry<T>
{
    private const ulong TypeMask = 3UL << 62;
    private const ulong FreeListMarker = 2UL << 62;
    private const ulong OccupiedListMarker = 1UL << 62;

    private ulong _flags;

    internal T Value;
    internal bool IsUnused => _flags == 0;
    internal bool IsInFreeList => (_flags & TypeMask) == FreeListMarker;
    internal bool IsInOccupiedList => (_flags & TypeMask) == OccupiedListMarker;
    internal NullableIndex Prev => NullableIndex.CreateUnsafe( unchecked( ( int )(_flags >> 31) & NullableIndex.NullValue ) );
    internal NullableIndex Next => NullableIndex.CreateUnsafe( unchecked( ( int )_flags & NullableIndex.NullValue ) );

    [Pure]
    public override string ToString()
    {
        return IsUnused
            ? "(unused)"
            : IsInFreeList
                ? $"(free) Prev: {Prev}, Next: {Next}"
                : $"Value: {Value}, Prev: {Prev}, Next: {Next}";
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void MakeOccupied(NullableIndex prev, NullableIndex next)
    {
        Assume.IsGreaterThanOrEqualTo( prev.Value, 0 );
        Assume.IsGreaterThanOrEqualTo( next.Value, 0 );
        _flags = unchecked( ( uint )next.Value | (( ulong )prev.Value << 31) | OccupiedListMarker );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void MakeFree(NullableIndex prev, NullableIndex next)
    {
        Assume.IsGreaterThanOrEqualTo( prev.Value, 0 );
        Assume.IsGreaterThanOrEqualTo( next.Value, 0 );
        _flags = unchecked( ( uint )next.Value | (( ulong )prev.Value << 31) | FreeListMarker );
    }
}
