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
using LfrlAnvil.MessageBroker.Server.Buffering;
using LfrlAnvil.MessageBroker.Server.Events;
using LfrlAnvil.MessageBroker.Server.Exceptions;
using LfrlAnvil.MessageBroker.Server.Internal;

namespace LfrlAnvil.MessageBroker.Server;

public sealed partial class MessageBrokerRemoteClient
{
    [MethodImpl( MethodImplOptions.NoInlining )]
    private Task StartHandshakeTask()
    {
        return Task.Run(
            async () =>
            {
                var result = await DecorateStreamAsync().ConfigureAwait( false );
                if ( result.Exception is null )
                {
                    result = await EstablishHandshakeAsync().ConfigureAwait( false );
                    if ( result.Exception is null )
                    {
                        try
                        {
                            using ( AcquireLock() )
                            {
                                if ( ShouldCancel )
                                    return;

                                MessageListener.SetUnderlyingTask( null );
                                _state = MessageBrokerRemoteClientState.Running;
                            }

                            var messageReceiverTask = MessageListener.StartUnderlyingTask( this, _stream );
                            var requestHandlerTask = RequestHandler.StartUnderlyingTask( this );
                            using ( AcquireLock() )
                            {
                                if ( ShouldCancel )
                                    return;

                                MessageListener.SetUnderlyingTask( messageReceiverTask );
                                RequestHandler.SetUnderlyingTask( requestHandlerTask );
                            }
                        }
                        catch ( Exception exc )
                        {
                            Emit( MessageBrokerRemoteClientEvent.Unexpected( this, exc ) );
                            using ( AcquireLock() )
                                MessageListener.SetUnderlyingTask( null );

                            await DisconnectAsync().ConfigureAwait( false );
                        }

                        return;
                    }
                }

                if ( result.Exception is MessageBrokerRemoteClientDisposedException or MessageBrokerServerDisposedException )
                    return;

                using ( AcquireLock() )
                    MessageListener.SetUnderlyingTask( null );

                await DisconnectAsync().ConfigureAwait( false );
            } );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private async ValueTask<Result> DecorateStreamAsync()
    {
        if ( Server.StreamDecorator is null )
            return Result.Valid;

        try
        {
            var stream = await Server.StreamDecorator( this, ReinterpretCast.To<NetworkStream>( _stream ) ).ConfigureAwait( false );
            using ( AcquireLock() )
            {
                if ( ShouldCancel )
                    return DisposedException();

                _stream = stream;
            }

            return Result.Valid;
        }
        catch ( Exception exc )
        {
            return EmitError( MessageBrokerRemoteClientEvent.Unexpected( this, exc ) );
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private async ValueTask<Result> EstablishHandshakeAsync()
    {
        var bufferToken = default( BinaryBufferToken );
        try
        {
            using ( AcquireLock() )
            {
                if ( ShouldCancel )
                    return DisposedException();

                _state = MessageBrokerRemoteClientState.Handshaking;
            }

            Emit( MessageBrokerRemoteClientEvent.WaitingForMessage( this ) );

            Memory<byte> buffer;
            try
            {
                var minPacketLength = Protocol.HandshakeRequestHeader.Length
                    .Max( Protocol.PacketHeader.Length + Protocol.HandshakeAcceptedResponse.Payload )
                    .Max( Protocol.PacketHeader.Length + Protocol.HandshakeRejectedResponse.Payload );

                bufferToken = RentBuffer( minPacketLength, out buffer ).EnableClearing();
            }
            catch ( Exception exc )
            {
                return EmitError<bool>( MessageBrokerRemoteClientEvent.Unexpected( this, exc ) );
            }

            var (readResult, exception) = await ReadHandshakeRequestAsync( bufferToken, buffer ).ConfigureAwait( false );
            buffer = readResult.Buffer;

            if ( readResult.RejectedResponse is not null )
            {
                Assume.IsNotNull( exception );
                var sendResult = await SendHandshakeRejectedResponseAsync(
                        buffer,
                        readResult.RejectedResponse.Value,
                        readResult.IsClientLittleEndian )
                    .ConfigureAwait( false );

                return sendResult.Exception ?? exception;
            }

            if ( exception is not null )
                return exception;

            Assume.IsNotNull( readResult.AcceptedResponse );
            var writeResult = await SendHandshakeAcceptedResponseAsync(
                    buffer,
                    readResult.AcceptedResponse.Value,
                    readResult.IsClientLittleEndian )
                .ConfigureAwait( false );

            if ( writeResult.Exception is not null )
                return Result.Error<bool>( writeResult.Exception );

            var result = await ReadConfirmHandshakeResponseAsync( buffer ).ConfigureAwait( false );
            return result.Exception is not null ? Result.Error<bool>( result.Exception ) : Result.Create( true );
        }
        finally
        {
            DisposeBufferToken( bufferToken );
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private async ValueTask<Result<ReadHandshakeResult>> ReadHandshakeRequestAsync(BinaryBufferToken bufferToken, Memory<byte> buffer)
    {
        Protocol.PacketHeader header;
        CancellationToken timeoutToken;
        try
        {
            using ( AcquireLock() )
            {
                if ( ShouldCancel )
                    return DisposedException();

                timeoutToken = SynchronousScheduler.ScheduleReadTimeout( this );
            }

            var data = buffer.Slice( 0, Protocol.PacketHeader.Length );
            await _stream.ReadExactlyAsync( data, timeoutToken ).ConfigureAwait( false );
            header = Protocol.PacketHeader.Parse( data );
            if ( BitConverter.IsLittleEndian )
                header = header.ReverseEndianness();
        }
        catch ( Exception exc )
        {
            return EmitError<ReadHandshakeResult>( MessageBrokerRemoteClientEvent.WaitingForMessage( this, exc ) );
        }

        Emit( MessageBrokerRemoteClientEvent.MessageReceived( this, header ) );

        if ( header.GetServerEndpoint() != MessageBrokerServerEndpoint.HandshakeRequest )
            return EmitError<ReadHandshakeResult>(
                MessageBrokerRemoteClientEvent.MessageRejected(
                    this,
                    header,
                    Protocol.UnexpectedServerEndpointException( this, header ) ) );

        var packetLength = unchecked( ( int )header.Payload );
        if ( packetLength < Protocol.HandshakeRequestHeader.Length )
            return EmitError<ReadHandshakeResult>(
                MessageBrokerRemoteClientEvent.MessageRejected( this, header, Protocol.InvalidPacketLengthException( this, header ) ) );

        Protocol.HandshakeAcceptedResponse acceptedResponse;
        bool isClientLittleEndian;
        Result<string> name;
        try
        {
            if ( packetLength > buffer.Length )
                buffer = bufferToken.SetLength( packetLength );

            var data = buffer.Slice( 0, packetLength );
            await _stream.ReadExactlyAsync( data, timeoutToken ).ConfigureAwait( false );
            var handshakeHeader = Protocol.HandshakeRequestHeader.Parse( data.Slice( 0, Protocol.HandshakeRequestHeader.Length ) );

            isClientLittleEndian = handshakeHeader.IsClientLittleEndian;
            name = TextEncoding.Parse( data.Slice( Protocol.HandshakeRequestHeader.Length ) );
            if ( name.Exception is not null )
                return EmitError(
                    MessageBrokerRemoteClientEvent.MessageRejected( this, header, exception: name.Exception ),
                    new ReadHandshakeResult(
                        Buffer: buffer,
                        RejectedResponse: new Protocol.HandshakeRejectedResponse(
                            Protocol.HandshakeRejectedResponse.Reasons.NameDecodingFailure ),
                        IsClientLittleEndian: isClientLittleEndian ) );

            Assume.IsNotNull( name.Value );
            if ( ! Defaults.NameLengthBounds.Contains( name.Value.Length ) )
                return EmitError(
                    MessageBrokerRemoteClientEvent.MessageRejected(
                        this,
                        header,
                        exception: Protocol.InvalidNameLengthException( this, header, name.Value.Length ) ),
                    new ReadHandshakeResult(
                        Buffer: buffer,
                        RejectedResponse: new Protocol.HandshakeRejectedResponse(
                            Protocol.HandshakeRejectedResponse.Reasons.InvalidNameLength ),
                        IsClientLittleEndian: isClientLittleEndian ) );

            using ( AcquireLock() )
            {
                if ( ShouldCancel )
                    return DisposedException();

                SynchronousScheduler.ResetReadTimeout();
                Name = name.Value;
                IsLittleEndian = isClientLittleEndian;
                MessageTimeout = Server.AcceptableMessageTimeout.Clamp( handshakeHeader.MessageTimeout );
                PingInterval = Server.AcceptablePingInterval.Clamp( handshakeHeader.PingInterval );
                MaxReadTimeout = MessageTimeout + PingInterval;
                acceptedResponse = new Protocol.HandshakeAcceptedResponse( this );
            }
        }
        catch ( Exception exc )
        {
            return EmitError<ReadHandshakeResult>( MessageBrokerRemoteClientEvent.MessageReceived( this, header, exception: exc ) );
        }

        var registration = RemoteClientCollection.RegisterName( this, name.Value );
        if ( registration.Exception is not null )
        {
            Emit(
                registration.Exception is MessageBrokerServerDisposedException
                    ? MessageBrokerRemoteClientEvent.MessageReceived( this, header, exception: registration.Exception )
                    : MessageBrokerRemoteClientEvent.Unexpected( this, registration.Exception ) );

            return registration.Exception;
        }

        if ( ! registration.Value )
            return EmitError(
                MessageBrokerRemoteClientEvent.MessageRejected(
                    this,
                    header,
                    new MessageBrokerServerDuplicateClientNameException( Server, name.Value ) ),
                new ReadHandshakeResult(
                    Buffer: buffer,
                    RejectedResponse: new Protocol.HandshakeRejectedResponse(
                        Protocol.HandshakeRejectedResponse.Reasons.NameAlreadyExists ),
                    IsClientLittleEndian: isClientLittleEndian ) );

        Emit( MessageBrokerRemoteClientEvent.MessageAccepted( this, header ) );
        return new ReadHandshakeResult( Buffer: buffer, AcceptedResponse: acceptedResponse, IsClientLittleEndian: isClientLittleEndian );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private ValueTask<Result> SendHandshakeRejectedResponseAsync(
        Memory<byte> buffer,
        Protocol.HandshakeRejectedResponse response,
        bool isClientLittleEndian)
    {
        Memory<byte> data;
        try
        {
            data = buffer.Slice( 0, Protocol.PacketHeader.Length + Protocol.HandshakeRejectedResponse.Payload );
            response.Serialize( data, isClientLittleEndian != BitConverter.IsLittleEndian );
        }
        catch ( Exception exc )
        {
            return ValueTask.FromResult( EmitError( MessageBrokerRemoteClientEvent.Unexpected( this, exc ) ) );
        }

        return WriteAsync( response.Header, data );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private ValueTask<Result> SendHandshakeAcceptedResponseAsync(
        Memory<byte> buffer,
        Protocol.HandshakeAcceptedResponse response,
        bool isClientLittleEndian)
    {
        Memory<byte> data;
        try
        {
            data = buffer.Slice( 0, Protocol.PacketHeader.Length + Protocol.HandshakeAcceptedResponse.Payload );
            response.Serialize( data, isClientLittleEndian != BitConverter.IsLittleEndian );
        }
        catch ( Exception exc )
        {
            return ValueTask.FromResult( EmitError( MessageBrokerRemoteClientEvent.Unexpected( this, exc ) ) );
        }

        return WriteAsync( response.Header, data );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private async ValueTask<Result> ReadConfirmHandshakeResponseAsync(Memory<byte> buffer)
    {
        Emit( MessageBrokerRemoteClientEvent.WaitingForMessage( this ) );

        Protocol.PacketHeader header;
        try
        {
            CancellationToken timeoutToken;
            using ( AcquireLock() )
            {
                if ( ShouldCancel )
                    return EmitError( MessageBrokerRemoteClientEvent.WaitingForMessage( this, DisposedException() ) );

                SynchronousScheduler.ResetWriteTimeout();
                timeoutToken = SynchronousScheduler.ScheduleReadTimeout( this );
            }

            var data = buffer.Slice( 0, Protocol.PacketHeader.Length );
            await _stream.ReadExactlyAsync( data, timeoutToken ).ConfigureAwait( false );
            header = Protocol.PacketHeader.Parse( data );
        }
        catch ( Exception exc )
        {
            return EmitError( MessageBrokerRemoteClientEvent.WaitingForMessage( this, exc ) );
        }

        Emit( MessageBrokerRemoteClientEvent.MessageReceived( this, header ) );

        if ( header.GetServerEndpoint() != MessageBrokerServerEndpoint.ConfirmHandshakeResponse )
            return HandleUnexpectedEndpoint( header );

        if ( header.Payload != Protocol.Endianness.VerificationPayload )
            return EmitError(
                MessageBrokerRemoteClientEvent.MessageRejected( this, header, Protocol.EndiannessPayloadException( this, header ) ) );

        Emit( MessageBrokerRemoteClientEvent.MessageAccepted( this, header ) );
        return Result.Valid;
    }

    private readonly record struct ReadHandshakeResult(
        Memory<byte> Buffer,
        Protocol.HandshakeAcceptedResponse? AcceptedResponse = null,
        Protocol.HandshakeRejectedResponse? RejectedResponse = null,
        bool IsClientLittleEndian = false
    );
}
