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
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Extensions;
using LfrlAnvil.Memory;
using LfrlAnvil.MessageBroker.Client.Events;
using LfrlAnvil.MessageBroker.Client.Internal;

namespace LfrlAnvil.MessageBroker.Client;

public sealed partial class MessageBrokerClient
{
    private async ValueTask<Result> EstablishHandshakeAsync(CancellationToken cancellationToken)
    {
        var poolToken = default( MemoryPoolToken<byte> );
        try
        {
            Memory<byte> buffer;
            Protocol.HandshakeRequest handshake;
            try
            {
                var minPacketLength = Protocol.PacketHeader.Length
                    .Max( Protocol.HandshakeAcceptedResponse.Length )
                    .Max( Protocol.HandshakeRejectedResponse.Length );

                handshake = new Protocol.HandshakeRequest( this );
                poolToken = MemoryPool.Rent( handshake.Length.Max( minPacketLength ), out buffer ).EnableClearing();
            }
            catch ( Exception exc )
            {
                return EmitError( MessageBrokerClientEvent.Unexpected( this, exc ) );
            }

            var result = await SendHandshakeRequestAsync( buffer, handshake ).ConfigureAwait( false );
            if ( result.Exception is not null )
                return result;

            await DisposeAndThrowIfCancellationRequestedAsync( cancellationToken ).ConfigureAwait( false );

            result = await HandleHandshakeResponseAsync( handshake.Header, buffer ).ConfigureAwait( false );
            if ( result.Exception is not null )
                return result;

            await DisposeAndThrowIfCancellationRequestedAsync( cancellationToken ).ConfigureAwait( false );

            return await SendConfirmHandshakeResponseAsync( buffer ).ConfigureAwait( false );
        }
        finally
        {
            poolToken.Return( this );
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private ValueTask<Result> SendHandshakeRequestAsync(Memory<byte> buffer, Protocol.HandshakeRequest handshake)
    {
        Memory<byte> data;
        try
        {
            data = buffer.Slice( 0, handshake.Length );
            handshake.Serialize( data );

            using ( AcquireLock() )
            {
                if ( ShouldCancel )
                    return ValueTask.FromResult<Result>( DisposedException() );

                _state = MessageBrokerClientState.Handshaking;
            }
        }
        catch ( Exception exc )
        {
            return ValueTask.FromResult<Result>( EmitError( MessageBrokerClientEvent.Unexpected( this, exc ) ) );
        }

        return WriteAsync( handshake.Header, data );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private async ValueTask<Result> HandleHandshakeResponseAsync(Protocol.PacketHeader requestHeader, Memory<byte> buffer)
    {
        Emit( MessageBrokerClientEvent.WaitingForMessage( this ) );

        Stream? stream = null;
        Protocol.PacketHeader header;
        CancellationToken timeoutToken = default;
        try
        {
            bool cancel;
            var headerBuffer = buffer.Slice( 0, Protocol.PacketHeader.Length );
            using ( AcquireLock() )
            {
                cancel = ShouldCancel;
                if ( ! cancel )
                {
                    stream = _stream;
                    EventScheduler.ResetWriteTimeout();
                    timeoutToken = EventScheduler.ScheduleReadTimeout( this );
                }
            }

            if ( cancel )
                return EmitError( MessageBrokerClientEvent.WaitingForMessage( this, DisposedException() ) );

            Assume.IsNotNull( stream );
            await stream.ReadExactlyAsync( headerBuffer, timeoutToken ).ConfigureAwait( false );
            header = Protocol.PacketHeader.Parse( headerBuffer, reverseEndianness: false );
        }
        catch ( Exception exc )
        {
            return EmitError( MessageBrokerClientEvent.WaitingForMessage( this, exc ) );
        }

        Emit( MessageBrokerClientEvent.MessageReceived( this, header ) );

        switch ( header.GetClientEndpoint() )
        {
            case MessageBrokerClientEndpoint.HandshakeAcceptedResponse:
                return await HandleIncomingHandshakeAcceptedResponseAsync( stream, buffer, header, timeoutToken ).ConfigureAwait( false );

            case MessageBrokerClientEndpoint.HandshakeRejectedResponse:
                return await HandleIncomingHandshakeRejectedResponseAsync( stream, buffer, requestHeader, header, timeoutToken )
                    .ConfigureAwait( false );

            default:
                return HandleUnexpectedEndpoint( header );
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private async ValueTask<Result> HandleIncomingHandshakeAcceptedResponseAsync(
        Stream stream,
        Memory<byte> buffer,
        Protocol.PacketHeader header,
        CancellationToken timeoutToken)
    {
        Assume.Equals( header.GetClientEndpoint(), MessageBrokerClientEndpoint.HandshakeAcceptedResponse );
        Chain<string> errors;
        try
        {
            var exc = Protocol.AssertPayload( this, header, Protocol.HandshakeAcceptedResponse.Length );
            if ( exc is not null )
                return EmitError( MessageBrokerClientEvent.MessageRejected( this, header, exception: exc ) );

            var data = buffer.Slice( 0, Protocol.HandshakeAcceptedResponse.Length );
            await stream.ReadExactlyAsync( data, timeoutToken ).ConfigureAwait( false );
            var response = Protocol.HandshakeAcceptedResponse.Parse( data );
            errors = response.StringifyErrors();

            bool cancel;
            using ( AcquireLock() )
            {
                cancel = ShouldCancel;
                if ( ! cancel )
                {
                    EventScheduler.ResetReadTimeout();
                    Id = response.Id;
                    MessageTimeout = response.MessageTimeout;
                    PingInterval = response.PingInterval;
                    IsServerLittleEndian = response.IsServerLittleEndian;
                }
            }

            if ( cancel )
                return EmitError( MessageBrokerClientEvent.MessageReceived( this, header, exception: DisposedException() ) );
        }
        catch ( Exception exc )
        {
            return EmitError( MessageBrokerClientEvent.MessageReceived( this, header, exception: exc ) );
        }

        if ( errors.Count > 0 )
            return EmitError(
                MessageBrokerClientEvent.MessageRejected(
                    this,
                    header,
                    Protocol.ProtocolException( this, header, errors ) ) );

        Emit( MessageBrokerClientEvent.MessageAccepted( this, header ) );
        return Result.Valid;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private async ValueTask<Result> HandleIncomingHandshakeRejectedResponseAsync(
        Stream stream,
        Memory<byte> buffer,
        Protocol.PacketHeader requestHeader,
        Protocol.PacketHeader responseHeader,
        CancellationToken timeoutToken)
    {
        Assume.Equals( responseHeader.GetClientEndpoint(), MessageBrokerClientEndpoint.HandshakeRejectedResponse );
        try
        {
            var exc = Protocol.AssertPayload( this, responseHeader, Protocol.HandshakeRejectedResponse.Length );
            if ( exc is not null )
                return EmitError( MessageBrokerClientEvent.MessageRejected( this, responseHeader, exception: exc ) );

            var data = buffer.Slice( 0, Protocol.HandshakeRejectedResponse.Length );
            await stream.ReadExactlyAsync( data, timeoutToken ).ConfigureAwait( false );
            var response = Protocol.HandshakeRejectedResponse.Parse( data );

            return EmitError(
                MessageBrokerClientEvent.MessageReceived(
                    this,
                    responseHeader,
                    exception: Protocol.RequestException( this, requestHeader, response.StringifyErrors() ) ) );
        }
        catch ( Exception exc )
        {
            return EmitError( MessageBrokerClientEvent.MessageReceived( this, responseHeader, exception: exc ) );
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private ValueTask<Result> SendConfirmHandshakeResponseAsync(Memory<byte> buffer)
    {
        Memory<byte> data;
        Protocol.PacketHeader response;
        try
        {
            data = buffer.Slice( 0, Protocol.PacketHeader.Length );
            response = Protocol.ConfirmHandshakeResponse.Create();
            response.Serialize( data, IsServerLittleEndian != BitConverter.IsLittleEndian );
        }
        catch ( Exception exc )
        {
            return ValueTask.FromResult<Result>( EmitError( MessageBrokerClientEvent.Unexpected( this, exc ) ) );
        }

        return WriteAsync( response, data );
    }
}
