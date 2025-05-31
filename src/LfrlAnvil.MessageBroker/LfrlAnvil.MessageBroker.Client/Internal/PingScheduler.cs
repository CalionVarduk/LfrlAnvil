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
using LfrlAnvil.Chrono;
using LfrlAnvil.MessageBroker.Client.Events;
using LfrlAnvil.MessageBroker.Client.Exceptions;

namespace LfrlAnvil.MessageBroker.Client.Internal;

internal struct PingScheduler
{
    private readonly ManualResetValueTaskSource<bool> _continuation;
    private Task? _task;

    private PingScheduler(ManualResetValueTaskSource<bool> continuation)
    {
        _continuation = continuation;
        _task = null;
    }

    internal bool IsContinuationPending => _continuation.Status == ValueTaskSourceStatus.Pending;

    [Pure]
    internal static PingScheduler Create()
    {
        return new PingScheduler( new ManualResetValueTaskSource<bool>() );
    }

    [MethodImpl( MethodImplOptions.NoInlining )]
    internal static async Task StartUnderlyingTask(MessageBrokerClient client)
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
                client.PingScheduler._task = null;
                traceId = client.GetTraceId();
            }

            using ( MessageBrokerClientTraceEvent.CreateScope( client, traceId, MessageBrokerClientTraceEventType.Unexpected ) )
            {
                MessageBrokerClientErrorEvent.Create( client, traceId, exc ).Emit( client.Logger.Error );
                await client.DisposeAsync( traceId ).ConfigureAwait( false );
            }
        }

        Assume.IsGreaterThanOrEqualTo( client.State, MessageBrokerClientState.Disposing );
    }

    internal void Dispose()
    {
        if ( _continuation.Status == ValueTaskSourceStatus.Pending )
            _continuation.SetResult( false );
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
    private static async ValueTask RunCore(MessageBrokerClient client)
    {
        var request = Protocol.Ping.Create();
        var buffer = new byte[Protocol.PacketHeader.Length].AsMemory();

        using ( client.AcquireLock() )
        {
            if ( client.ShouldCancel )
                return;

            request.Serialize( buffer, client.IsServerLittleEndian != BitConverter.IsLittleEndian );
            client.EventScheduler.SchedulePing( client );
        }

        while ( true )
        {
            var @continue = await client.PingScheduler._continuation.GetTask().ConfigureAwait( false );
            if ( ! @continue )
                return;

            ManualResetValueTaskSource<bool> writerSource;
            using ( client.AcquireLock() )
            {
                if ( client.ShouldCancel )
                    return;

                var delay = client.EventScheduler.GetPingDelay( client );
                if ( delay > Duration.Zero )
                {
                    client.PingScheduler._continuation.Reset();
                    continue;
                }

                writerSource = client.WriterQueue.AcquireSource();
            }

            if ( ! await writerSource.GetTask().ConfigureAwait( false ) )
                return;

            ulong traceId;
            ManualResetValueTaskSource<IncomingPacketToken> responseSource;

            using ( client.AcquireLock() )
            {
                if ( client.ShouldCancel )
                    return;

                var delay = client.EventScheduler.GetPingDelay( client );
                if ( delay > Duration.Zero )
                {
                    client.PingScheduler._continuation.Reset();
                    client.WriterQueue.Release( client, writerSource );
                    continue;
                }

                traceId = client.GetTraceId();
                responseSource = client.ResponseQueue.EnqueueSource();
            }

            using ( MessageBrokerClientTraceEvent.CreateScope( client, traceId, MessageBrokerClientTraceEventType.Ping ) )
            {
                var result = await client.WriteAsync( request, buffer, traceId ).ConfigureAwait( false );
                if ( result.Exception is not null )
                {
                    await DisposeClientAsync( client, traceId ).ConfigureAwait( false );
                    return;
                }

                using ( client.AcquireActiveLock( traceId, out var exc ) )
                {
                    if ( exc is not null )
                        return;

                    client.WriterQueue.Release( client, writerSource );
                    client.ResponseQueue.ActivateTimeout( client, responseSource );
                }

                var response = await responseSource.GetTask().ConfigureAwait( false );
                if ( response.Type != IncomingPacketToken.Result.Ok )
                {
                    if ( response.Type == IncomingPacketToken.Result.Disposed )
                    {
                        MessageBrokerClientErrorEvent.Create( client, traceId, client.DisposedException() ).Emit( client.Logger.Error );
                        return;
                    }

                    var error = new MessageBrokerClientResponseTimeoutException( client, request.GetServerEndpoint() );
                    MessageBrokerClientErrorEvent.Create( client, traceId, error ).Emit( client.Logger.Error );
                    await DisposeClientAsync( client, traceId ).ConfigureAwait( false );
                    return;
                }

                using ( client.AcquireActiveLock( traceId, out var exc ) )
                {
                    if ( exc is not null )
                        return;

                    client.ResponseQueue.Release( responseSource );
                }

                if ( response.Header.GetClientEndpoint() != MessageBrokerClientEndpoint.Pong )
                {
                    client.HandleUnexpectedEndpoint( response.Header, traceId );
                    await DisposeClientAsync( client, traceId ).ConfigureAwait( false );
                    return;
                }

                MessageBrokerClientReadPacketEvent.CreateReceived( client, traceId, response.Header ).Emit( client.Logger.ReadPacket );

                if ( response.Header.Payload != Protocol.Endianness.VerificationPayload )
                {
                    var error = Protocol.EndiannessPayloadException( client, response.Header );
                    MessageBrokerClientErrorEvent.Create( client, traceId, error ).Emit( client.Logger.Error );
                    await DisposeClientAsync( client, traceId ).ConfigureAwait( false );
                    return;
                }

                MessageBrokerClientReadPacketEvent.CreateAccepted( client, traceId, response.Header ).Emit( client.Logger.ReadPacket );
            }

            using ( client.AcquireLock() )
            {
                if ( client.ShouldCancel )
                    return;

                client.PingScheduler._continuation.Reset();
                client.EventScheduler.SchedulePing( client );
            }
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static ValueTask DisposeClientAsync(MessageBrokerClient client, ulong traceId)
    {
        using ( client.AcquireLock() )
            client.PingScheduler._task = null;

        return client.DisposeAsync( traceId );
    }
}
