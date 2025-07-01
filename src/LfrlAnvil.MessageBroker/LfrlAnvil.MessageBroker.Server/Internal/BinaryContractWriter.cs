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

internal ref struct BinaryContractWriter
{
    private ref byte _first;

    internal BinaryContractWriter(Span<byte> span)
        : this( ref MemoryMarshal.GetReference( span ) ) { }

    private BinaryContractWriter(ref byte next)
    {
        _first = ref next;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void Move(int offset)
    {
        Assume.IsGreaterThanOrEqualTo( offset, 0 );
        _first = ref Unsafe.Add( ref _first, offset );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void Write(byte value)
    {
        Unsafe.WriteUnaligned( ref _first, value );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void MoveWrite(byte value)
    {
        Write( value );
        Move( sizeof( byte ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void Write(ushort value)
    {
        Unsafe.WriteUnaligned( ref _first, value );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void MoveWrite(ushort value)
    {
        Write( value );
        Move( sizeof( ushort ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void Write(uint value)
    {
        Unsafe.WriteUnaligned( ref _first, value );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void MoveWrite(uint value)
    {
        Write( value );
        Move( sizeof( uint ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void Write(ulong value)
    {
        Unsafe.WriteUnaligned( ref _first, value );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void MoveWrite(ulong value)
    {
        Write( value );
        Move( sizeof( ulong ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal Span<byte> GetSpan(int length)
    {
        Assume.IsGreaterThanOrEqualTo( length, 0 );
        return MemoryMarshal.CreateSpan( ref _first, length );
    }
}
