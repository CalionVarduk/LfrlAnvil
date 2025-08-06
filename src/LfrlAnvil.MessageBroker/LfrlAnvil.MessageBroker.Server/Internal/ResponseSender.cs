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
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
using LfrlAnvil.Async;
using LfrlAnvil.Memory;
using LfrlAnvil.MessageBroker.Server.Events;

namespace LfrlAnvil.MessageBroker.Server.Internal;

internal struct ResponseSender
{
    private readonly ManualResetValueTaskSource<bool> _continuation;
    private QueueSlim<Entry> _responses;
    private Task? _task;

    private ResponseSender(ManualResetValueTaskSource<bool> continuation)
    {
        _continuation = continuation;
        _responses = QueueSlim<Entry>.Create();
        _task = null;
    }

    [Pure]
    internal static ResponseSender Create()
    {
        return new ResponseSender( new ManualResetValueTaskSource<bool>() );
    }

    [MethodImpl( MethodImplOptions.NoInlining )]
    internal static async Task StartUnderlyingTask(MessageBrokerRemoteClient client)
    {
        try
        {
            await RunCore( client ).ConfigureAwait( false );
        }
        catch ( Exception exc )
        {
            ulong traceId;
            using ( client.AcquireLock() )
            {
                client.ResponseSender._task = null;
                traceId = client.GetTraceId();
            }

            using ( MessageBrokerRemoteClientTraceEvent.CreateScope( client, traceId, MessageBrokerRemoteClientTraceEventType.Unexpected ) )
            {
                if ( client.Logger.Error is { } error )
                    error.Emit( MessageBrokerRemoteClientErrorEvent.Create( client, traceId, exc ) );

                await client.DisposeAsync( traceId ).ConfigureAwait( false );
            }
        }

        Assume.IsGreaterThanOrEqualTo( client.State, MessageBrokerRemoteClientState.Disposing );
    }

    internal void BeginDispose()
    {
        if ( _continuation.Status == ValueTaskSourceStatus.Pending )
            _continuation.SetResult( false );
    }

    internal ListSlim<DiscardedResponse> EndDispose()
    {
        var result = ListSlim<DiscardedResponse>.Create( _responses.Count );
        foreach ( ref readonly var response in _responses )
            result.Add( new DiscardedResponse( response.PoolToken, response.EventType, response.TraceId ) );

        _responses = QueueSlim<Entry>.Create();
        return result;
    }

    internal void SetUnderlyingTask(Task task)
    {
        Assume.IsNull( _task );
        _task = task;
    }

    internal Task? DiscardUnderlyingTask()
    {
        var result = _task;
        _task = null;
        return result;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void SignalContinuation()
    {
        if ( _continuation.Status == ValueTaskSourceStatus.Pending )
            _continuation.SetResult( true );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static void EnqueueUnsafe(
        MessageBrokerRemoteClient client,
        Protocol.PacketHeader packetHeader,
        WriterQueue.TaskSource writerSource,
        MemoryPoolToken<byte> poolToken,
        MessageBrokerRemoteClientTraceEventType eventType,
        ulong traceId)
    {
        client.ResponseSender._responses.Enqueue( new Entry( packetHeader, writerSource, poolToken, eventType, traceId ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static async ValueTask RunCore(MessageBrokerRemoteClient client)
    {
        var traceEnd = client.Logger.TraceEnd;
        while ( true )
        {
            var @continue = await client.ResponseSender._continuation.GetTask().ConfigureAwait( false );
            if ( ! @continue )
                return;

            bool containsResponses;
            do
            {
                Entry response;
                ReadOnlyMemory<byte> data;
                using ( client.AcquireLock() )
                {
                    if ( client.ShouldCancel )
                        return;

                    Assume.False( client.ResponseSender._responses.IsEmpty );
                    response = client.ResponseSender._responses.First();
                    client.ResponseSender._responses.Dequeue();
                    data = response.WriterSource.Data;
                }

                try
                {
                    var writerResult = await response.WriterSource.GetTask().ConfigureAwait( false );
                    switch ( writerResult.Status )
                    {
                        case WriterSourceResultStatus.Ready:
                        {
                            var (packetCount, exception) = await client
                                .WritePotentialBatchAsync( response.PacketHeader, data, response.TraceId )
                                .ConfigureAwait( false );

                            if ( exception is not null )
                            {
                                using ( client.AcquireLock() )
                                    client.ResponseSender._task = null;

                                await client.DisposeAsync( response.TraceId ).ConfigureAwait( false );
                                return;
                            }

                            using ( client.AcquireActiveLock( response.TraceId, out var exc ) )
                            {
                                if ( exc is not null )
                                    return;

                                if ( packetCount > 1 )
                                    client.WriterQueue.ReleaseBatched( client, response.WriterSource, packetCount, response.TraceId );
                                else
                                    client.WriterQueue.Release( client, response.WriterSource );

                                containsResponses = ! client.ResponseSender._responses.IsEmpty;
                            }

                            break;
                        }
                        case WriterSourceResultStatus.Batched:
                        {
                            Assume.IsGreaterThan( client.MaxBatchPacketCount, 1 );
                            Assume.Equals( writerResult.Status, WriterSourceResultStatus.Batched );
                            if ( client.Logger.SendPacket is { } sendPacket )
                                sendPacket.Emit(
                                    MessageBrokerRemoteClientSendPacketEvent.CreateBatched(
                                        client,
                                        response.TraceId,
                                        response.PacketHeader,
                                        writerResult.BatchTraceId ) );

                            using ( client.AcquireActiveLock( response.TraceId, out var exc ) )
                            {
                                if ( exc is not null )
                                    return;

                                client.WriterQueue.ReleaseBatched( client, response.WriterSource, writerResult );
                                containsResponses = ! client.ResponseSender._responses.IsEmpty;
                            }

                            break;
                        }
                        default:
                        {
                            if ( client.Logger.Error is { } error )
                                error.Emit(
                                    MessageBrokerRemoteClientErrorEvent.Create( client, response.TraceId, client.DisposedException() ) );

                            return;
                        }
                    }
                }
                finally
                {
                    response.PoolToken.Return( client, response.TraceId );
                    traceEnd?.Emit( MessageBrokerRemoteClientTraceEvent.Create( client, response.TraceId, response.EventType ) );
                }
            }
            while ( containsResponses );

            using ( client.AcquireLock() )
            {
                if ( client.ShouldCancel )
                    return;

                client.ResponseSender._continuation.Reset();
                if ( ! client.ResponseSender._responses.IsEmpty )
                    client.ResponseSender.SignalContinuation();
            }
        }
    }

    internal readonly struct DiscardedResponse
    {
        internal readonly MemoryPoolToken<byte> PoolToken;
        internal readonly MessageBrokerRemoteClientTraceEventType EventType;
        internal readonly ulong TraceId;

        internal DiscardedResponse(MemoryPoolToken<byte> poolToken, MessageBrokerRemoteClientTraceEventType eventType, ulong traceId)
        {
            PoolToken = poolToken;
            EventType = eventType;
            TraceId = traceId;
        }
    }

    private readonly struct Entry
    {
        internal readonly Protocol.PacketHeader PacketHeader;
        internal readonly WriterQueue.TaskSource WriterSource;
        internal readonly MemoryPoolToken<byte> PoolToken;
        internal readonly MessageBrokerRemoteClientTraceEventType EventType;
        internal readonly ulong TraceId;

        internal Entry(
            Protocol.PacketHeader packetHeader,
            WriterQueue.TaskSource writerSource,
            MemoryPoolToken<byte> poolToken,
            MessageBrokerRemoteClientTraceEventType eventType,
            ulong traceId)
        {
            PacketHeader = packetHeader;
            WriterSource = writerSource;
            PoolToken = poolToken;
            EventType = eventType;
            TraceId = traceId;
        }
    }
}
