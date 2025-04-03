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
using System.Threading.Tasks.Sources;
using LfrlAnvil.Async;
using LfrlAnvil.Extensions;
using LfrlAnvil.Memory;

namespace LfrlAnvil.MessageBroker.Server.Internal;

internal struct MessageContextQueue
{
    private StackSlim<ManualResetValueTaskSource<bool>> _writerTokenSourceCache;
    private QueueSlim<ManualResetValueTaskSource<bool>> _pendingOutgoingWriters;
    private QueueSlim<IncomingPacketToken> _incomingRequests;
    private ulong _lastId;

    private MessageContextQueue(ulong lastId)
    {
        _writerTokenSourceCache = StackSlim<ManualResetValueTaskSource<bool>>.Create();
        _pendingOutgoingWriters = QueueSlim<ManualResetValueTaskSource<bool>>.Create();
        _incomingRequests = QueueSlim<IncomingPacketToken>.Create();
        _lastId = lastId;
    }

    [Pure]
    internal static MessageContextQueue Create()
    {
        return new MessageContextQueue( 0 );
    }

    internal void Dispose()
    {
        foreach ( var source in _pendingOutgoingWriters )
        {
            if ( source.Status == ValueTaskSourceStatus.Pending )
                source.SetResult( default );
        }

        foreach ( var request in _incomingRequests )
            request.PoolToken.TryDispose();

        _incomingRequests = QueueSlim<IncomingPacketToken>.Create();
        _pendingOutgoingWriters = QueueSlim<ManualResetValueTaskSource<bool>>.Create();
        _writerTokenSourceCache = StackSlim<ManualResetValueTaskSource<bool>>.Create();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ulong AcquireContextId()
    {
        return ++_lastId;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ManualResetValueTaskSource<bool> AcquireWriterSource()
    {
        if ( ! _writerTokenSourceCache.TryPop( out var result ) )
            result = new ManualResetValueTaskSource<bool>();

        if ( _pendingOutgoingWriters.IsEmpty )
            result.SetResult( true );

        _pendingOutgoingWriters.Enqueue( result );
        return result;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void ResetOutgoingWriter(MessageBrokerRemoteClient client, ManualResetValueTaskSource<bool> source)
    {
        Assume.IsNotNull( source );
        Assume.Equals( source, _pendingOutgoingWriters.First() );

        _pendingOutgoingWriters.Dequeue();
        source.Reset();
        _writerTokenSourceCache.Push( source );

        client.EventScheduler.ResetWriteTimeout();
        if ( _pendingOutgoingWriters.IsEmpty )
            return;

        var next = _pendingOutgoingWriters.First();
        next.SetResult( true );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void EnqueueRequest(Protocol.PacketHeader header, MemoryPoolToken<byte> poolToken, Memory<byte> data)
    {
        _incomingRequests.Enqueue( IncomingPacketToken.Ok( header, poolToken, data ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal IncomingPacketToken DequeueRequest()
    {
        Assume.False( _incomingRequests.IsEmpty );
        var result = _incomingRequests.First();
        Assume.Equals( result.Type, IncomingPacketToken.Result.Ok );
        _incomingRequests.Dequeue();
        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool ContainsEnqueuedRequests()
    {
        return ! _incomingRequests.IsEmpty;
    }
}
