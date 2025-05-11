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
        TaskStopReason stopReason;
        try
        {
            stopReason = await RunCore( client ).ConfigureAwait( false );
        }
        catch ( Exception exc )
        {
            client.Emit( MessageBrokerClientEvent.Unexpected( client, exc ) );
            stopReason = TaskStopReason.Error;
        }

        if ( stopReason == TaskStopReason.OwnerDisposed )
            return;

        using ( client.AcquireLock() )
            client.PingScheduler._task = null;

        await client.DisposeAsync().ConfigureAwait( false );
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
    private static async ValueTask<TaskStopReason> RunCore(MessageBrokerClient client)
    {
        var request = Protocol.Ping.Create();
        var buffer = new byte[Protocol.PacketHeader.Length].AsMemory();

        using ( client.AcquireLock() )
        {
            if ( client.ShouldCancel )
                return TaskStopReason.OwnerDisposed;

            request.Serialize( buffer, client.IsServerLittleEndian != BitConverter.IsLittleEndian );
            client.EventScheduler.SchedulePing( client );
        }

        while ( true )
        {
            var @continue = await client.PingScheduler._continuation.GetTask().ConfigureAwait( false );
            if ( ! @continue )
                return TaskStopReason.OwnerDisposed;

            ulong contextId;
            ManualResetValueTaskSource<bool> writerSource;

            using ( client.AcquireLock() )
            {
                if ( client.ShouldCancel )
                    return TaskStopReason.OwnerDisposed;

                var delay = client.EventScheduler.GetPingDelay( client );
                if ( delay > Duration.Zero )
                {
                    client.PingScheduler._continuation.Reset();
                    continue;
                }

                contextId = client.MessageContextQueue.AcquireContextId();
                writerSource = client.MessageContextQueue.AcquireWriterSource();
            }

            if ( ! await writerSource.GetTask().ConfigureAwait( false ) )
                return TaskStopReason.OwnerDisposed;

            ManualResetValueTaskSource<IncomingPacketToken> responseSource;
            using ( client.AcquireLock() )
            {
                if ( client.ShouldCancel )
                    return TaskStopReason.OwnerDisposed;

                var delay = client.EventScheduler.GetPingDelay( client );
                if ( delay > Duration.Zero )
                {
                    client.PingScheduler._continuation.Reset();
                    client.MessageContextQueue.ResetOutgoingWriter( client, writerSource );
                    continue;
                }

                responseSource = client.MessageContextQueue.AcquirePendingResponseSource( contextId, request.GetServerEndpoint() );
            }

            var result = await client.WriteAsync( request, buffer, contextId ).ConfigureAwait( false );
            if ( result.Exception is not null )
                break;

            using ( client.AcquireLock() )
            {
                if ( client.ShouldCancel )
                    return TaskStopReason.OwnerDisposed;

                client.MessageContextQueue.ResetOutgoingWriter( client, writerSource );
                client.MessageContextQueue.ActivatePendingResponseSource( client, responseSource );
            }

            var response = await responseSource.GetTask().ConfigureAwait( false );
            if ( response.Type != IncomingPacketToken.Result.Ok )
            {
                if ( response.Type == IncomingPacketToken.Result.Disposed )
                    return TaskStopReason.OwnerDisposed;

                client.Emit(
                    MessageBrokerClientEvent.WaitingForMessage(
                        client,
                        new MessageBrokerClientResponseTimeoutException( client, request.GetServerEndpoint() ) ) );

                return TaskStopReason.Error;
            }

            using ( client.AcquireLock() )
            {
                if ( client.ShouldCancel )
                    return TaskStopReason.OwnerDisposed;

                client.MessageContextQueue.ResetPendingResponseSource( responseSource );
            }

            if ( response.Header.GetClientEndpoint() != MessageBrokerClientEndpoint.Pong )
            {
                client.HandleUnexpectedEndpoint( response.Header, contextId );
                break;
            }

            client.Emit( MessageBrokerClientEvent.MessageReceived( client, response.Header, contextId ) );

            if ( response.Header.Payload != Protocol.Endianness.VerificationPayload )
            {
                client.Emit(
                    MessageBrokerClientEvent.MessageRejected(
                        client,
                        response.Header,
                        Protocol.EndiannessPayloadException( client, response.Header ),
                        contextId ) );

                break;
            }

            client.Emit( MessageBrokerClientEvent.MessageAccepted( client, response.Header, contextId ) );

            using ( client.AcquireLock() )
            {
                if ( client.ShouldCancel )
                    return TaskStopReason.OwnerDisposed;

                client.PingScheduler._continuation.Reset();
                client.EventScheduler.SchedulePing( client );
            }
        }

        return TaskStopReason.Error;
    }
}
