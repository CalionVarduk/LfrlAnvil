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
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Extensions;
using LfrlAnvil.Memory;
using LfrlAnvil.MessageBroker.Server.Events;
using LfrlAnvil.MessageBroker.Server.Exceptions;
using LfrlAnvil.MessageBroker.Server.Internal;

namespace LfrlAnvil.MessageBroker.Server;

public sealed partial class MessageBrokerRemoteClient
{
    [MethodImpl( MethodImplOptions.NoInlining )]
    private Task StartHandshakeTask(ulong traceId)
    {
        return Task.Run(
            async () =>
            {
                try
                {
                    var result = await DecorateStreamAsync( traceId ).ConfigureAwait( false );
                    if ( result.Exception is null )
                    {
                        result = await EstablishHandshakeAsync( traceId ).ConfigureAwait( false );
                        if ( result.Exception is null )
                        {
                            using ( AcquireActiveLock( traceId, out var exc ) )
                            {
                                if ( exc is not null )
                                    return;

                                PacketListener.SetUnderlyingTask( null );
                                _state = MessageBrokerRemoteClientState.Running;
                            }

                            var packetListenerTask = PacketListener.StartUnderlyingTask( this, _stream );
                            var requestHandlerTask = RequestHandler.StartUnderlyingTask( this );
                            var notificationSenderTask = NotificationSender.StartUnderlyingTask( this );
                            using ( AcquireActiveLock( traceId, out var exc ) )
                            {
                                if ( exc is not null )
                                    return;

                                PacketListener.SetUnderlyingTask( packetListenerTask );
                                RequestHandler.SetUnderlyingTask( requestHandlerTask );
                                NotificationSender.SetUnderlyingTask( notificationSenderTask );
                            }

                            return;
                        }
                    }

                    if ( result.Exception is MessageBrokerRemoteClientDisposedException or MessageBrokerServerDisposedException )
                        return;

                    using ( AcquireLock() )
                        PacketListener.SetUnderlyingTask( null );

                    await DisposeAsync( traceId ).ConfigureAwait( false );
                }
                catch ( Exception exc )
                {
                    if ( Logger.Error is { } error )
                        error.Emit( MessageBrokerRemoteClientErrorEvent.Create( this, traceId, exc ) );

                    using ( AcquireLock() )
                        PacketListener.SetUnderlyingTask( null );

                    await DisposeAsync( traceId ).ConfigureAwait( false );
                }
                finally
                {
                    if ( Logger.TraceEnd is { } traceEnd )
                        traceEnd.Emit(
                            MessageBrokerRemoteClientTraceEvent.Create( this, traceId, MessageBrokerRemoteClientTraceEventType.Start ) );
                }
            } );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private async ValueTask<Result> DecorateStreamAsync(ulong traceId)
    {
        if ( Server.StreamDecorator is null )
            return Result.Valid;

        try
        {
            var stream = await Server.StreamDecorator( this, ReinterpretCast.To<NetworkStream>( _stream ) ).ConfigureAwait( false );
            using ( AcquireActiveLock( traceId, out var exc ) )
            {
                if ( exc is not null )
                    return exc;

                _stream = stream;
            }

            return Result.Valid;
        }
        catch ( Exception exc )
        {
            return EmitError( exc, traceId );
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private async ValueTask<Result> EstablishHandshakeAsync(ulong traceId)
    {
        var poolToken = MemoryPoolToken<byte>.Empty;
        try
        {
            using ( AcquireActiveLock( traceId, out var exc ) )
            {
                if ( exc is not null )
                    return exc;

                _state = MessageBrokerRemoteClientState.Handshaking;
            }

            Memory<byte> buffer;
            try
            {
                var minPacketLength = Protocol.HandshakeRequestHeader.Length
                    .Max( Protocol.PacketHeader.Length + Protocol.HandshakeAcceptedResponse.Payload )
                    .Max( Protocol.PacketHeader.Length + Protocol.HandshakeRejectedResponse.Payload );

                poolToken = MemoryPool.Rent( minPacketLength, ClearBuffers, out buffer );
            }
            catch ( Exception exc )
            {
                return EmitError( exc, traceId );
            }

            var (readResult, exception) = await ReadHandshakeRequestAsync( poolToken, buffer, traceId ).ConfigureAwait( false );
            buffer = readResult.Buffer;

            if ( readResult.RejectedResponse is not null )
            {
                Assume.IsNotNull( exception );
                var sendResult = await SendHandshakeRejectedResponseAsync(
                        buffer,
                        readResult.RejectedResponse.Value,
                        readResult.IsClientLittleEndian,
                        traceId )
                    .ConfigureAwait( false );

                return sendResult.Exception ?? exception;
            }

            if ( exception is not null )
                return exception;

            Assume.IsNotNull( readResult.AcceptedResponse );
            var writeResult = await SendHandshakeAcceptedResponseAsync(
                    buffer,
                    readResult.AcceptedResponse.Value,
                    readResult.IsClientLittleEndian,
                    traceId )
                .ConfigureAwait( false );

            if ( writeResult.Exception is not null )
                return writeResult;

            return await ReadConfirmHandshakeResponseAsync( buffer, traceId ).ConfigureAwait( false );
        }
        finally
        {
            poolToken.Return( this, traceId );
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private async ValueTask<Result<ReadHandshakeResult>> ReadHandshakeRequestAsync(
        MemoryPoolToken<byte> poolToken,
        Memory<byte> buffer,
        ulong traceId)
    {
        var awaitPacket = Logger.AwaitPacket;
        awaitPacket?.Emit( MessageBrokerRemoteClientAwaitPacketEvent.Create( this ) );

        Protocol.PacketHeader header;
        var timeoutToken = default( CancellationToken );
        try
        {
            using ( AcquireActiveLock( traceId, out var exc ) )
            {
                if ( exc is not null )
                    return exc;

                timeoutToken = EventScheduler.ScheduleReadTimeout( this );
            }

            var data = buffer.Slice( 0, Protocol.PacketHeader.Length );
            await _stream.ReadExactlyAsync( data, timeoutToken ).ConfigureAwait( false );
            header = Protocol.PacketHeader.Parse( data );
            if ( BitConverter.IsLittleEndian )
                header = header.ReverseEndianness();
        }
        catch ( Exception exc )
        {
            if ( exc is OperationCanceledException cancelExc && cancelExc.CancellationToken == timeoutToken )
            {
                var timeoutException = this.RequestTimeoutException();
                if ( Logger.Error is { } error )
                    error.Emit( MessageBrokerRemoteClientErrorEvent.Create( this, traceId, timeoutException ) );

                return timeoutException;
            }

            awaitPacket?.Emit( MessageBrokerRemoteClientAwaitPacketEvent.Create( this, exc ) );
            return exc;
        }

        var readPacket = Logger.ReadPacket;
        awaitPacket?.Emit( MessageBrokerRemoteClientAwaitPacketEvent.Create( this, header ) );
        readPacket?.Emit( MessageBrokerRemoteClientReadPacketEvent.CreateReceived( this, traceId, header ) );

        if ( header.GetServerEndpoint() != MessageBrokerServerEndpoint.HandshakeRequest )
            return HandleUnexpectedEndpoint( header, traceId );

        var packetLength = header.AssertPacketLength( this, Defaults.Memory.DefaultNetworkPacketLength );
        if ( packetLength.Exception is not null )
            return EmitError( packetLength.Exception, traceId );

        var exception = header.AssertMinPayload( this, Protocol.HandshakeRequestHeader.Length );
        if ( exception is not null )
            return EmitError( exception, traceId );

        Protocol.HandshakeAcceptedResponse acceptedResponse;
        bool isClientLittleEndian;
        Result<string> name;
        try
        {
            if ( packetLength.Value > buffer.Length )
                poolToken.IncreaseLength( packetLength.Value, out buffer );

            var data = buffer.Slice( 0, packetLength.Value );
            await _stream.ReadExactlyAsync( data, timeoutToken ).ConfigureAwait( false );
            var handshakeHeader = Protocol.HandshakeRequestHeader.Parse( data.Slice( 0, Protocol.HandshakeRequestHeader.Length ) );

            isClientLittleEndian = handshakeHeader.IsClientLittleEndian;
            name = TextEncoding.Parse( data.Slice( Protocol.HandshakeRequestHeader.Length ) );
            if ( name.Exception is not null )
            {
                var result = new ReadHandshakeResult( Buffer: buffer, RejectedResponse: null, IsClientLittleEndian: isClientLittleEndian );
                if ( Logger.Error is { } error )
                    error.Emit( MessageBrokerRemoteClientErrorEvent.Create( this, traceId, name.Exception ) );

                return Result.Error( name.Exception, result );
            }

            Assume.IsNotNull( name.Value );
            if ( ! Defaults.NameLengthBounds.Contains( name.Value.Length ) )
            {
                var exc = this.ProtocolException( header, Resources.InvalidNameLength( name.Value.Length ) );
                var result = new ReadHandshakeResult(
                    Buffer: buffer,
                    RejectedResponse: new Protocol.HandshakeRejectedResponse(
                        Protocol.HandshakeRejectedResponse.Reasons.InvalidNameLength ),
                    IsClientLittleEndian: isClientLittleEndian );

                if ( Logger.Error is { } error )
                    error.Emit( MessageBrokerRemoteClientErrorEvent.Create( this, traceId, exc ) );

                return Result.Error( exc, result );
            }

            if ( Logger.Handshaking is { } handshaking )
                handshaking.Emit(
                    MessageBrokerRemoteClientHandshakingEvent.Create(
                        this,
                        traceId,
                        name.Value,
                        handshakeHeader.MessageTimeout,
                        handshakeHeader.PingInterval,
                        handshakeHeader.MaxBatchPacketCount,
                        handshakeHeader.MaxNetworkBatchPacketLength,
                        handshakeHeader.SynchronizeExternalObjectNames,
                        handshakeHeader.ClearBuffers,
                        isClientLittleEndian ) );

            using ( AcquireActiveLock( traceId, out var exc ) )
            {
                if ( exc is not null )
                    return exc;

                EventScheduler.ResetReadTimeout();
                Name = name.Value;
                IsLittleEndian = isClientLittleEndian;
                MessageTimeout = Server.AcceptableMessageTimeout.Clamp( handshakeHeader.MessageTimeout );
                PingInterval = Server.AcceptablePingInterval.Clamp( handshakeHeader.PingInterval );
                MaxBatchPacketCount = Server.AcceptableMaxBatchPacketCount.Clamp( handshakeHeader.MaxBatchPacketCount );
                if ( MaxBatchPacketCount == 1 )
                    MaxBatchPacketCount = 0;

                MaxNetworkBatchPacketBytes = MaxBatchPacketCount > 0
                    ? unchecked( ( int )Server.AcceptableMaxNetworkBatchPacketLength
                        .Clamp( handshakeHeader.MaxNetworkBatchPacketLength )
                        .Bytes )
                    : 0;

                SynchronizeExternalObjectNames = handshakeHeader.SynchronizeExternalObjectNames;
                ClearBuffers = handshakeHeader.ClearBuffers;
                MaxReadTimeout = MessageTimeout + PingInterval;
                acceptedResponse = new Protocol.HandshakeAcceptedResponse( this );
            }
        }
        catch ( Exception exc )
        {
            if ( exc is OperationCanceledException cancelExc && cancelExc.CancellationToken == timeoutToken )
            {
                var timeoutException = this.RequestTimeoutException();
                if ( Logger.Error is { } error )
                    error.Emit( MessageBrokerRemoteClientErrorEvent.Create( this, traceId, timeoutException ) );

                return timeoutException;
            }

            awaitPacket?.Emit( MessageBrokerRemoteClientAwaitPacketEvent.Create( this, exc ) );
            return exc;
        }

        var registration = RemoteClientCollection.RegisterName( this, name.Value );
        if ( registration.Exception is not null )
            return EmitError( registration.Exception, traceId );

        if ( ! registration.Value )
        {
            var exc = Server.Exception( Resources.DuplicateClientName( name.Value ) );
            var result = new ReadHandshakeResult(
                Buffer: buffer,
                RejectedResponse: new Protocol.HandshakeRejectedResponse( Protocol.HandshakeRejectedResponse.Reasons.NameAlreadyExists ),
                IsClientLittleEndian: isClientLittleEndian );

            if ( Logger.Error is { } error )
                error.Emit( MessageBrokerRemoteClientErrorEvent.Create( this, traceId, exc ) );

            return Result.Error( exc, result );
        }

        readPacket?.Emit( MessageBrokerRemoteClientReadPacketEvent.CreateAccepted( this, traceId, header ) );
        return new ReadHandshakeResult( Buffer: buffer, AcceptedResponse: acceptedResponse, IsClientLittleEndian: isClientLittleEndian );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private ValueTask<Result> SendHandshakeRejectedResponseAsync(
        Memory<byte> buffer,
        Protocol.HandshakeRejectedResponse response,
        bool isClientLittleEndian,
        ulong traceId)
    {
        Memory<byte> data;
        try
        {
            data = buffer.Slice( 0, Protocol.PacketHeader.Length + Protocol.HandshakeRejectedResponse.Payload );
            response.Serialize( data, isClientLittleEndian != BitConverter.IsLittleEndian );
        }
        catch ( Exception exc )
        {
            return ValueTask.FromResult<Result>( EmitError( exc, traceId ) );
        }

        return WriteAsync( response.Header, data, traceId );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private ValueTask<Result> SendHandshakeAcceptedResponseAsync(
        Memory<byte> buffer,
        Protocol.HandshakeAcceptedResponse response,
        bool isClientLittleEndian,
        ulong traceId)
    {
        Memory<byte> data;
        try
        {
            data = buffer.Slice( 0, Protocol.PacketHeader.Length + Protocol.HandshakeAcceptedResponse.Payload );
            response.Serialize( data, isClientLittleEndian != BitConverter.IsLittleEndian );
        }
        catch ( Exception exc )
        {
            return ValueTask.FromResult<Result>( EmitError( exc, traceId ) );
        }

        return WriteAsync( response.Header, data, traceId );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private async ValueTask<Result> ReadConfirmHandshakeResponseAsync(Memory<byte> buffer, ulong traceId)
    {
        var awaitPacket = Logger.AwaitPacket;
        awaitPacket?.Emit( MessageBrokerRemoteClientAwaitPacketEvent.Create( this ) );

        Protocol.PacketHeader header;
        var timeoutToken = default( CancellationToken );
        try
        {
            using ( AcquireActiveLock( traceId, out var exc ) )
            {
                if ( exc is not null )
                    return exc;

                EventScheduler.ResetWriteTimeout();
                timeoutToken = EventScheduler.ScheduleReadTimeout( this );
            }

            var data = buffer.Slice( 0, Protocol.PacketHeader.Length );
            await _stream.ReadExactlyAsync( data, timeoutToken ).ConfigureAwait( false );
            header = Protocol.PacketHeader.Parse( data );
        }
        catch ( Exception exc )
        {
            if ( exc is OperationCanceledException cancelExc && cancelExc.CancellationToken == timeoutToken )
            {
                var exception = this.RequestTimeoutException();
                if ( Logger.Error is { } error )
                    error.Emit( MessageBrokerRemoteClientErrorEvent.Create( this, traceId, exception ) );

                return exception;
            }

            awaitPacket?.Emit( MessageBrokerRemoteClientAwaitPacketEvent.Create( this, exc ) );
            return exc;
        }

        var readPacket = Logger.ReadPacket;
        awaitPacket?.Emit( MessageBrokerRemoteClientAwaitPacketEvent.Create( this, header ) );
        readPacket?.Emit( MessageBrokerRemoteClientReadPacketEvent.CreateReceived( this, traceId, header ) );

        if ( header.GetServerEndpoint() != MessageBrokerServerEndpoint.ConfirmHandshakeResponse )
            return HandleUnexpectedEndpoint( header, traceId );

        if ( header.Payload != Protocol.Endianness.VerificationPayload )
            return EmitError( this.ProtocolException( header, Resources.InvalidEndiannessPayload( header.Payload ) ), traceId );

        readPacket?.Emit( MessageBrokerRemoteClientReadPacketEvent.CreateAccepted( this, traceId, header ) );
        if ( Logger.HandshakeEstablished is { } handshakeEstablished )
            handshakeEstablished.Emit( MessageBrokerRemoteClientHandshakeEstablishedEvent.Create( this, traceId ) );

        return Result.Valid;
    }

    private readonly record struct ReadHandshakeResult(
        Memory<byte> Buffer,
        Protocol.HandshakeAcceptedResponse? AcceptedResponse = null,
        Protocol.HandshakeRejectedResponse? RejectedResponse = null,
        bool IsClientLittleEndian = false
    );
}
