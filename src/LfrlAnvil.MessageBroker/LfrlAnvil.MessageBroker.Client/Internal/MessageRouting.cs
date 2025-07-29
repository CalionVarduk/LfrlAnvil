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
using System.Buffers.Binary;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Exceptions;
using LfrlAnvil.Memory;
using LfrlAnvil.MessageBroker.Client.Exceptions;

namespace LfrlAnvil.MessageBroker.Client.Internal;

internal struct MessageRouting
{
    internal MemoryPoolToken<byte> Token;
    private Memory<byte> _buffer;
    private int _written;
    internal short TargetCount;

    internal Memory<byte> Data => _buffer.Slice( 0, _written );

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void Add(MessageBrokerClient client, int id, bool clearOnDispose)
    {
        Ensure.IsGreaterThan( id, 0 );
        if ( TargetCount == short.MaxValue )
            ExceptionThrower.Throw( new InvalidOperationException( Resources.MaxMessageRoutingTargetCountReached ) );

        if ( TargetCount == 0 )
            Initialize( client.MemoryPool, sizeof( byte ) + sizeof( uint ), clearOnDispose );

        EnsureRemainingCapacity( sizeof( byte ) + sizeof( uint ) );

        var writer = new BinaryContractWriter( _buffer.Slice( _written ).Span );
        writer.MoveWrite( 0 );
        writer.Write(
            unchecked( ( uint )(client.IsServerLittleEndian != BitConverter.IsLittleEndian
                ? BinaryPrimitives.ReverseEndianness( id )
                : id) ) );

        TargetCount = unchecked( ( short )(TargetCount + 1) );
        _written = unchecked( _written + sizeof( byte ) + sizeof( uint ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void Add(MessageBrokerClient client, string name, bool clearOnDispose)
    {
        Ensure.IsInRange( name.Length, Defaults.NameLengthBounds.Min, Defaults.NameLengthBounds.Max );
        if ( TargetCount == short.MaxValue )
            ExceptionThrower.Throw( new InvalidOperationException( Resources.MaxMessageRoutingTargetCountReached ) );

        var encodedName = TextEncoding.Prepare( name ).GetValueOrThrow();
        if ( TargetCount == 0 )
            Initialize( client.MemoryPool, sizeof( ushort ) + encodedName.ByteCount, clearOnDispose );

        EnsureRemainingCapacity( sizeof( ushort ) + encodedName.ByteCount );

        var header = unchecked( ( ushort )((encodedName.ByteCount << 1) | 1) );
        var writer = new BinaryContractWriter( _buffer.Slice( _written ).Span );
        writer.MoveWrite(
            client.IsServerLittleEndian != BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness( header ) : header );

        encodedName.Encode( writer.GetSpan( encodedName.ByteCount ) ).ThrowIfError();

        TargetCount = unchecked( ( short )(TargetCount + 1) );
        _written = unchecked( _written + sizeof( ushort ) + encodedName.ByteCount );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal int GetRemainingBytes(MessageBrokerClient client)
    {
        return unchecked( client.MaxNetworkPacketBytes - _written );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void Initialize(MemoryPool<byte> pool, int desired, bool clearOnDispose)
    {
        Assume.Equals( TargetCount, 0 );
        Assume.Equals( _buffer.Length, 0 );
        Assume.Equals( _written, 0 );

        Token = pool.Rent(
                Defaults.Memory.GetRoutingBufferCapacity( checked( Protocol.PushMessageRoutingHeader.Length + desired ) ),
                out _buffer )
            .EnableClearing( clearOnDispose );

        _written = Protocol.PushMessageRoutingHeader.Length;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void EnsureRemainingCapacity(int desired)
    {
        var remaining = unchecked( _buffer.Length - _written );
        if ( desired <= remaining )
            return;

        desired = Defaults.Memory.GetRoutingBufferCapacity( checked( _written + desired ) );
        Token.IncreaseLength( desired, out _buffer );
    }
}
