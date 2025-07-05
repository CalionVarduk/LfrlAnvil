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
using LfrlAnvil.Memory;

namespace LfrlAnvil.MessageBroker.Server.Internal;

internal struct RequestQueue
{
    private QueueSlim<IncomingPacketToken> _pendingRequests;

    private RequestQueue(int capacity)
    {
        _pendingRequests = QueueSlim<IncomingPacketToken>.Create( capacity );
    }

    [Pure]
    internal static RequestQueue Create()
    {
        return new RequestQueue( 0 );
    }

    internal Chain<Exception> Dispose(bool extractExceptions)
    {
        var exceptions = Chain<Exception>.Empty;
        foreach ( ref readonly var request in _pendingRequests )
        {
            var exc = request.PoolToken.Return();
            if ( exc is not null && extractExceptions )
                exceptions = exceptions.Extend( exc );
        }

        _pendingRequests = QueueSlim<IncomingPacketToken>.Create();
        return exceptions;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void Enqueue(Protocol.PacketHeader header, MemoryPoolToken<byte> poolToken, Memory<byte> data)
    {
        _pendingRequests.Enqueue( IncomingPacketToken.Ok( header, poolToken, data ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal IncomingPacketToken Dequeue()
    {
        Assume.False( _pendingRequests.IsEmpty );
        var result = _pendingRequests.First();
        Assume.Equals( result.Type, IncomingPacketToken.Result.Ok );
        _pendingRequests.Dequeue();
        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool IsNotEmpty()
    {
        return ! _pendingRequests.IsEmpty;
    }
}
