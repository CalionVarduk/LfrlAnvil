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
                if ( client.Logger.Error is { } error )
                    error.Emit( MessageBrokerClientErrorEvent.Create( client, traceId, exc ) );

                await client.DisposeAsync( traceId ).ConfigureAwait( false );
            }
        }

        Assume.IsGreaterThanOrEqualTo( client.State, MessageBrokerClientState.Disposing );
    }

    internal void Dispose(ref Chain<Exception> exceptions)
    {
        try
        {
            _continuation.TrySetResult( false );
        }
        catch ( Exception exc )
        {
            exceptions = exceptions.Extend( exc );
        }
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
        _continuation.TrySetResult( true );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static async ValueTask RunCore(MessageBrokerClient client)
    {
        var request = Protocol.Ping.Create();
        var buffer = new byte[Protocol.PacketHeader.Length].AsMemory();
        bool reverseEndianness;

        using ( client.AcquireLock() )
        {
            if ( client.ShouldCancel )
                return;

            reverseEndianness = client.IsServerLittleEndian != BitConverter.IsLittleEndian;
            request.Serialize( buffer, reverseEndianness );
            client.EventScheduler.SchedulePing( client );
        }

        while ( true )
        {
            var @continue = await client.PingScheduler._continuation.GetTask().ConfigureAwait( false );
            if ( ! @continue )
                return;

            ulong traceId;
            ManualResetValueTaskSource<WriterSourceResult> writerSource;
            ManualResetValueTaskSource<IncomingPacketToken> responseSource;
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

                writerSource = client.WriterQueue.AcquireSource( buffer );
                responseSource = client.ResponseQueue.EnqueueSource();
                traceId = client.GetTraceId();
            }

            using ( MessageBrokerClientTraceEvent.CreateScope( client, traceId, MessageBrokerClientTraceEventType.Ping ) )
            {
                var writerResult = await writerSource.GetTask().ConfigureAwait( false );
                switch ( writerResult.Status )
                {
                    case WriterSourceResultStatus.Ready:
                    {
                        var (packetCount, exception) = await client.WritePotentialBatchAsync( request, buffer, reverseEndianness, traceId )
                            .ConfigureAwait( false );

                        if ( exception is not null )
                        {
                            await DisposeClientAsync( client, traceId ).ConfigureAwait( false );
                            return;
                        }

                        if ( ! client.ReleaseWriterWithResponse( writerSource, responseSource, packetCount, traceId, out _ ) )
                            return;

                        break;
                    }
                    case WriterSourceResultStatus.Batched:
                    {
                        if ( ! client.ReleaseBatchedWriterWithResponse(
                            writerSource,
                            responseSource,
                            request,
                            writerResult,
                            traceId,
                            out _ ) )
                            return;

                        break;
                    }
                    default:
                        return;
                }

                var response = await responseSource.GetTask().ConfigureAwait( false );
                if ( response.Type != IncomingPacketToken.Result.Ok )
                {
                    var error = client.Logger.Error;
                    if ( response.Type == IncomingPacketToken.Result.Disposed )
                    {
                        error?.Emit( MessageBrokerClientErrorEvent.Create( client, traceId, client.DisposedException() ) );
                        return;
                    }

                    var exc = client.ResponseTimeoutException( request );
                    error?.Emit( MessageBrokerClientErrorEvent.Create( client, traceId, exc ) );
                    await DisposeClientAsync( client, traceId ).ConfigureAwait( false );
                    return;
                }

                if ( ! client.ReleaseResponse( responseSource, traceId, out _ ) )
                    return;

                if ( response.Header.GetClientEndpoint() != MessageBrokerClientEndpoint.Pong )
                {
                    client.HandleUnexpectedEndpoint( response.Header, traceId );
                    await DisposeClientAsync( client, traceId ).ConfigureAwait( false );
                    return;
                }

                var readPacket = client.Logger.ReadPacket;
                readPacket?.Emit( MessageBrokerClientReadPacketEvent.CreateReceived( client, traceId, response.Header ) );

                if ( response.Header.Payload != Protocol.Endianness.VerificationPayload )
                {
                    if ( client.Logger.Error is { } error )
                    {
                        var exc = client.ProtocolException(
                            response.Header,
                            Resources.InvalidEndiannessPayload( response.Header.Payload ) );

                        error.Emit( MessageBrokerClientErrorEvent.Create( client, traceId, exc ) );
                    }

                    await DisposeClientAsync( client, traceId ).ConfigureAwait( false );
                    return;
                }

                readPacket?.Emit( MessageBrokerClientReadPacketEvent.CreateAccepted( client, traceId, response.Header ) );
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
