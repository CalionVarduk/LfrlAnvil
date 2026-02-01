// Copyright 2025-2026 Łukasz Furlepa
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
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Memory;
using LfrlAnvil.MessageBroker.Server.Events;
using LfrlAnvil.MessageBroker.Server.Exceptions;
using LfrlAnvil.MessageBroker.Server.Internal;

namespace LfrlAnvil.MessageBroker.Server;

public sealed partial class MessageBrokerRemoteClient
{
    private Task StartHandshakeTask(ulong traceId)
    {
        return Task.Run(
            async () =>
            {
                var failed = true;
                try
                {
                    var result = await SendHandshakeAcceptedResponseAsync( traceId ).ConfigureAwait( false );
                    if ( result.Exception is not null )
                        return;

                    result = await ReadConfirmHandshakeResponseAsync( traceId ).ConfigureAwait( false );
                    if ( result.Exception is not null )
                        return;

                    Stream stream;
                    using ( AcquireActiveLock( traceId, out var exc ) )
                    {
                        if ( exc is not null )
                            return;

                        Assume.IsNotNull( _stream );
                        stream = _stream;
                        PacketListener.SetUnderlyingTask( null );
                        _state = MessageBrokerRemoteClientState.Running;
                    }

                    var packetListenerTask = PacketListener.StartUnderlyingTask( this, stream );
                    var requestHandlerTask = RequestHandler.StartUnderlyingTask( this );
                    var responseSenderTask = ResponseSender.StartUnderlyingTask( this );
                    var notificationSenderTask = NotificationSender.StartUnderlyingTask( this );
                    using ( AcquireActiveLock( traceId, out var exc ) )
                    {
                        if ( exc is not null )
                            return;

                        PacketListener.SetUnderlyingTask( packetListenerTask );
                        RequestHandler.SetUnderlyingTask( requestHandlerTask );
                        ResponseSender.SetUnderlyingTask( responseSenderTask );
                        NotificationSender.SetUnderlyingTask( notificationSenderTask );
                    }

                    failed = false;
                }
                catch ( Exception exc )
                {
                    if ( Logger.Error is { } error )
                        error.Emit( MessageBrokerRemoteClientErrorEvent.Create( this, traceId, exc ) );
                }
                finally
                {
                    if ( failed )
                    {
                        using ( AcquireLock() )
                            PacketListener.SetUnderlyingTask( null );

                        await DeactivateAsync( traceId ).ConfigureAwait( false );
                    }

                    if ( Logger.TraceEnd is { } traceEnd )
                        traceEnd.Emit(
                            MessageBrokerRemoteClientTraceEvent.Create( this, traceId, MessageBrokerRemoteClientTraceEventType.Start ) );
                }
            } );
    }

    private async ValueTask<Result> SendHandshakeAcceptedResponseAsync(ulong traceId)
    {
        var response = new Protocol.HandshakeAcceptedResponse( this );

        var sendPacket = Logger.SendPacket;
        sendPacket?.Emit( MessageBrokerRemoteClientSendPacketEvent.CreateSending( this, traceId, response.Header ) );

        var poolToken = MemoryPoolToken<byte>.Empty;
        try
        {
            poolToken = MemoryPool.Rent(
                Protocol.PacketHeader.Length + Protocol.HandshakeAcceptedResponse.Payload,
                ClearBuffers,
                out var data );

            response.Serialize( data, IsLittleEndian != BitConverter.IsLittleEndian );

            Stream stream;
            CancellationToken timeoutToken;
            using ( AcquireActiveLock( traceId, out var exc ) )
            {
                if ( exc is not null )
                    return exc;

                Assume.IsNotNull( _stream );
                stream = _stream;
                timeoutToken = EventScheduler.ScheduleWriteTimeout( this );
            }

            await stream.WriteAsync( data, timeoutToken ).ConfigureAwait( false );

            sendPacket?.Emit( MessageBrokerRemoteClientSendPacketEvent.CreateSent( this, traceId, response.Header ) );
            return Result.Valid;
        }
        catch ( Exception exc )
        {
            if ( Logger.Error is { } error )
                error.Emit( MessageBrokerRemoteClientErrorEvent.Create( this, traceId, exc ) );

            return exc;
        }
        finally
        {
            poolToken.Return( this, traceId );
        }
    }

    private async ValueTask<Result> ReadConfirmHandshakeResponseAsync(ulong traceId)
    {
        var awaitPacket = Logger.AwaitPacket;
        awaitPacket?.Emit( MessageBrokerRemoteClientAwaitPacketEvent.Create( this ) );

        Protocol.PacketHeader header;
        var timeoutToken = default( CancellationToken );
        var poolToken = MemoryPoolToken<byte>.Empty;
        try
        {
            poolToken = MemoryPool.Rent( Protocol.PacketHeader.Length, ClearBuffers, out var data );

            Stream stream;
            using ( AcquireActiveLock( traceId, out var exc ) )
            {
                if ( exc is not null )
                    return exc;

                Assume.IsNotNull( _stream );
                stream = _stream;
                EventScheduler.ResetWriteTimeout();
                timeoutToken = EventScheduler.ScheduleReadTimeout( this );
            }

            await stream.ReadExactlyAsync( data, timeoutToken ).ConfigureAwait( false );
            header = Protocol.PacketHeader.Parse( data );
        }
        catch ( Exception exc )
        {
            if ( exc is OperationCanceledException cancelExc && cancelExc.CancellationToken == timeoutToken )
            {
                exc = this.RequestHandshakeTimeoutException();
                if ( Logger.Error is { } error )
                    error.Emit( MessageBrokerRemoteClientErrorEvent.Create( this, traceId, exc ) );
            }
            else
                awaitPacket?.Emit( MessageBrokerRemoteClientAwaitPacketEvent.Create( this, exc ) );

            return exc;
        }
        finally
        {
            poolToken.Return( this, traceId );
        }

        var readPacket = Logger.ReadPacket;
        awaitPacket?.Emit( MessageBrokerRemoteClientAwaitPacketEvent.Create( this, header ) );
        readPacket?.Emit( MessageBrokerRemoteClientReadPacketEvent.CreateReceived( this, traceId, header ) );

        if ( header.GetServerEndpoint() != MessageBrokerServerEndpoint.ConfirmHandshakeResponse )
            return HandleUnexpectedEndpoint( header, traceId );

        if ( header.Payload != Protocol.Endianness.VerificationPayload )
        {
            var exc = this.ProtocolException( header, Resources.InvalidEndiannessPayload( header.Payload ) );
            if ( Logger.Error is { } error )
                error.Emit( MessageBrokerRemoteClientErrorEvent.Create( this, traceId, exc ) );

            return exc;
        }

        readPacket?.Emit( MessageBrokerRemoteClientReadPacketEvent.CreateAccepted( this, traceId, header ) );
        if ( Logger.HandshakeEstablished is { } handshakeEstablished )
            handshakeEstablished.Emit( MessageBrokerRemoteClientHandshakeEstablishedEvent.Create( this, traceId ) );

        return Result.Valid;
    }
}
