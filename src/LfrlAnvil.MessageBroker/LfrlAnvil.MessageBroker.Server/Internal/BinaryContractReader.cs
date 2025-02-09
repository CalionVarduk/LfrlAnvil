// Copyright 2025 Łukasz Furlepa
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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LfrlAnvil.MessageBroker.Server.Internal;

internal ref struct BinaryContractReader
{
    private ref byte _first;

    internal BinaryContractReader(ReadOnlySpan<byte> span)
        : this( ref MemoryMarshal.GetReference( span ) ) { }

    private BinaryContractReader(ref byte next)
    {
        _first = ref next;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void Move(int offset)
    {
        Assume.IsGreaterThan( offset, 0 );
        _first = ref Unsafe.Add( ref _first, offset );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal byte ReadInt8()
    {
        return Unsafe.ReadUnaligned<byte>( ref _first );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal byte MoveReadInt8()
    {
        var result = ReadInt8();
        Move( sizeof( byte ) );
        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal uint ReadInt32()
    {
        return Unsafe.ReadUnaligned<uint>( ref _first );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal uint MoveReadInt32()
    {
        var result = ReadInt32();
        Move( sizeof( uint ) );
        return result;
    }
}
