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
using LfrlAnvil.Chrono;

namespace LfrlAnvil.MessageBroker.Client.Internal;

internal struct ResponseQueue
{
    private StackSlim<ManualResetValueTaskSource<IncomingPacketToken>> _responseCache;
    private QueueSlim<PendingResponseSource> _pendingResponses;
    private int _activePendingResponses;

    private ResponseQueue(int activePendingResponses)
    {
        _responseCache = StackSlim<ManualResetValueTaskSource<IncomingPacketToken>>.Create();
        _pendingResponses = QueueSlim<PendingResponseSource>.Create();
        _activePendingResponses = activePendingResponses;
    }

    [Pure]
    internal static ResponseQueue Create()
    {
        return new ResponseQueue( 0 );
    }

    internal void Dispose(ref Chain<Exception> exceptions)
    {
        _activePendingResponses = 0;
        foreach ( ref readonly var source in _pendingResponses )
        {
            try
            {
                Assume.IsNotNull( source.Source );
                if ( source.Source.Status == ValueTaskSourceStatus.Pending )
                    source.Source.SetResult( default );
            }
            catch ( Exception exc )
            {
                exceptions = exceptions.Extend( exc );
            }
        }

        try
        {
            _pendingResponses = QueueSlim<PendingResponseSource>.Create();
            _responseCache = StackSlim<ManualResetValueTaskSource<IncomingPacketToken>>.Create();
        }
        catch ( Exception exc )
        {
            exceptions = exceptions.Extend( exc );
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ManualResetValueTaskSource<IncomingPacketToken> EnqueueSource()
    {
        if ( ! _responseCache.TryPop( out var result ) )
            result = new ManualResetValueTaskSource<IncomingPacketToken>();

        _pendingResponses.Enqueue( new PendingResponseSource( result ) );
        return result;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void ActivateTimeout(MessageBrokerClient client, ManualResetValueTaskSource<IncomingPacketToken> source)
    {
        if ( source.Status != ValueTaskSourceStatus.Pending )
            return;

        ref var token = ref _pendingResponses[_activePendingResponses++];
        Assume.Equals( source, token.Source );
        Assume.Equals( token.Timeout, TimeoutEntry.MaxTimestamp );
        token.Timeout = client.EventScheduler.GetPendingResponseTimeout( client );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void Signal(ManualResetValueTaskSource<IncomingPacketToken> source, IncomingPacketToken token)
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
    internal void Release(ManualResetValueTaskSource<IncomingPacketToken> source)
    {
        source.Reset();
        _responseCache.Push( source );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal PendingResponseSource GetNext()
    {
        return _pendingResponses.IsEmpty ? default : _pendingResponses.First();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void ProcessTimeouts(Timestamp now)
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
