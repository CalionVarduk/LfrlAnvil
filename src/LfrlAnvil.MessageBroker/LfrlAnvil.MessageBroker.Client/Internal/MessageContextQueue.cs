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

using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading.Tasks.Sources;
using LfrlAnvil.Async;
using LfrlAnvil.Chrono;
using LfrlAnvil.MessageBroker.Client.Events;

namespace LfrlAnvil.MessageBroker.Client.Internal;

internal struct MessageContextQueue
{
    private StackSlim<ManualResetValueTaskSource<bool>> _writerTokenSourceCache;
    private StackSlim<ManualResetValueTaskSource<IncomingPacketToken>> _incomingPendingResponseSourceCache;
    private QueueSlim<ManualResetValueTaskSource<bool>> _pendingOutgoingWriters;
    private QueueSlim<PendingResponseSource> _pendingResponses;
    private ulong _lastContextId;
    private int _activePendingResponses;

    private MessageContextQueue(ulong lastContextId)
    {
        _writerTokenSourceCache = StackSlim<ManualResetValueTaskSource<bool>>.Create();
        _incomingPendingResponseSourceCache = StackSlim<ManualResetValueTaskSource<IncomingPacketToken>>.Create();
        _pendingOutgoingWriters = QueueSlim<ManualResetValueTaskSource<bool>>.Create();
        _pendingResponses = QueueSlim<PendingResponseSource>.Create();
        _lastContextId = lastContextId;
        _activePendingResponses = 0;
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
                source.SetResult( false );
        }

        _pendingOutgoingWriters = QueueSlim<ManualResetValueTaskSource<bool>>.Create();

        _activePendingResponses = 0;
        foreach ( var source in _pendingResponses )
        {
            Assume.IsNotNull( source.Source );
            if ( source.Source.Status == ValueTaskSourceStatus.Pending )
                source.Source.SetResult( default );
        }

        _pendingResponses = QueueSlim<PendingResponseSource>.Create();
        _writerTokenSourceCache = StackSlim<ManualResetValueTaskSource<bool>>.Create();
        _incomingPendingResponseSourceCache = StackSlim<ManualResetValueTaskSource<IncomingPacketToken>>.Create();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ulong AcquireContextId()
    {
        return ++_lastContextId;
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
    internal ManualResetValueTaskSource<IncomingPacketToken> AcquirePendingResponseSource(
        ulong contextId,
        MessageBrokerServerEndpoint serverEndpoint)
    {
        if ( ! _incomingPendingResponseSourceCache.TryPop( out var result ) )
            result = new ManualResetValueTaskSource<IncomingPacketToken>();

        _pendingResponses.Enqueue( new PendingResponseSource( result, contextId, serverEndpoint ) );
        return result;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void ActivatePendingResponseSource(MessageBrokerClient client, ManualResetValueTaskSource<IncomingPacketToken> source)
    {
        if ( source.Status != ValueTaskSourceStatus.Pending )
            return;

        ref var token = ref _pendingResponses[_activePendingResponses++];
        Assume.Equals( source, token.Source );
        Assume.Equals( token.Timeout, TimeoutEntry.MaxTimestamp );
        token.Timeout = client.SynchronousScheduler.GetPendingResponseTimeout( client );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void ResetOutgoingWriter(MessageBrokerClient client, ManualResetValueTaskSource<bool> source)
    {
        Assume.Equals( source, _pendingOutgoingWriters.First() );

        _pendingOutgoingWriters.Dequeue();
        source.Reset();
        _writerTokenSourceCache.Push( source );

        client.SynchronousScheduler.ResetWriteTimeout();
        if ( _pendingOutgoingWriters.IsEmpty )
            return;

        var next = _pendingOutgoingWriters.First();
        next.SetResult( true );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void NotifyPendingResponseSource(ManualResetValueTaskSource<IncomingPacketToken> source, IncomingPacketToken token)
    {
        Assume.False( _pendingResponses.IsEmpty );
        ref var first = ref _pendingResponses.First();
        Assume.Equals( source, first.Source );

        if ( first.Timeout != TimeoutEntry.MaxTimestamp )
            --_activePendingResponses;

        _pendingResponses.Dequeue();
        source.SetResult( token );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void ResetPendingResponseSource(ManualResetValueTaskSource<IncomingPacketToken> source)
    {
        source.Reset();
        _incomingPendingResponseSourceCache.Push( source );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal PendingResponseSource GetNextPendingResponse()
    {
        ref var result = ref _pendingResponses.First();
        return Unsafe.IsNullRef( ref result ) ? default : result;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void ProcessPendingResponseTimeouts(Timestamp now)
    {
        while ( ! _pendingResponses.IsEmpty )
        {
            ref var first = ref _pendingResponses.First();
            if ( first.Timeout > now )
                break;

            Assume.IsNotNull( first.Source );
            if ( first.Source.Status != ValueTaskSourceStatus.Pending )
                break;

            first.Source.SetResult( IncomingPacketToken.TimedOut() );
        }
    }
}
