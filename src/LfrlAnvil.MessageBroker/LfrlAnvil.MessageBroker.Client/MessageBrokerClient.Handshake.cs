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
    private async ValueTask<Result> EstablishHandshakeAsync(ulong traceId, CancellationToken cancellationToken)
    {
        if ( Logger.Handshaking is { } handshaking )
            handshaking.Emit( MessageBrokerClientHandshakingEvent.Create( this, traceId ) );

        var poolToken = MemoryPoolToken<byte>.Empty;
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
                poolToken = MemoryPool.Rent( handshake.Length.Max( minPacketLength ), ClearBuffers, out buffer );
            }
            catch ( Exception exc )
            {
                return EmitError( exc, traceId );
            }

            var result = await SendHandshakeRequestAsync( buffer, handshake, traceId ).ConfigureAwait( false );
            if ( result.Exception is not null )
                return result;

            await DisposeAndThrowIfCancellationRequestedAsync( traceId, cancellationToken ).ConfigureAwait( false );

            result = await HandleHandshakeResponseAsync( handshake.Header, buffer, traceId ).ConfigureAwait( false );
            if ( result.Exception is not null )
                return result;

            await DisposeAndThrowIfCancellationRequestedAsync( traceId, cancellationToken ).ConfigureAwait( false );

            result = await SendConfirmHandshakeResponseAsync( buffer, traceId ).ConfigureAwait( false );
            if ( result.Exception is null && Logger.HandshakeEstablished is { } handshakeEstablished )
                handshakeEstablished.Emit( MessageBrokerClientHandshakeEstablishedEvent.Create( this, traceId ) );

            return result;
        }
        finally
        {
            poolToken.Return( this, traceId );
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private ValueTask<Result> SendHandshakeRequestAsync(Memory<byte> buffer, Protocol.HandshakeRequest handshake, ulong traceId)
    {
        Memory<byte> data;
        try
        {
            data = buffer.Slice( 0, handshake.Length );
            handshake.Serialize( data );

            using ( AcquireActiveLock( traceId, out var exc ) )
            {
                if ( exc is not null )
                    return ValueTask.FromResult<Result>( exc );

                _state = MessageBrokerClientState.Handshaking;
            }
        }
        catch ( Exception exc )
        {
            return ValueTask.FromResult<Result>( EmitError( exc, traceId ) );
        }

        return WriteAsync( handshake.Header, data, traceId );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private async ValueTask<Result> HandleHandshakeResponseAsync(Protocol.PacketHeader requestHeader, Memory<byte> buffer, ulong traceId)
    {
        var awaitPacket = Logger.AwaitPacket;
        awaitPacket?.Emit( MessageBrokerClientAwaitPacketEvent.Create( this ) );

        Stream? stream;
        Protocol.PacketHeader header;
        var timeoutToken = default( CancellationToken );
        try
        {
            var headerBuffer = buffer.Slice( 0, Protocol.PacketHeader.Length );
            using ( AcquireActiveLock( traceId, out var exc ) )
            {
                if ( exc is not null )
                    return exc;

                stream = _stream;
                EventScheduler.ResetWriteTimeout();
                timeoutToken = EventScheduler.ScheduleReadTimeout( this );
            }

            Assume.IsNotNull( stream );
            await stream.ReadExactlyAsync( headerBuffer, timeoutToken ).ConfigureAwait( false );
            header = Protocol.PacketHeader.Parse( headerBuffer, reverseEndianness: false );
        }
        catch ( Exception exc )
        {
            if ( exc is OperationCanceledException cancelExc && cancelExc.CancellationToken == timeoutToken )
            {
                var exception = this.ResponseTimeoutException( requestHeader );
                if ( Logger.Error is { } error )
                    error.Emit( MessageBrokerClientErrorEvent.Create( this, traceId, exception ) );

                return exception;
            }

            awaitPacket?.Emit( MessageBrokerClientAwaitPacketEvent.Create( this, exc ) );
            return exc;
        }

        awaitPacket?.Emit( MessageBrokerClientAwaitPacketEvent.Create( this, header ) );
        if ( Logger.ReadPacket is { } readPacket )
            readPacket.Emit( MessageBrokerClientReadPacketEvent.CreateReceived( this, traceId, header ) );

        switch ( header.GetClientEndpoint() )
        {
            case MessageBrokerClientEndpoint.HandshakeAcceptedResponse:
                return await HandleIncomingHandshakeAcceptedResponseAsync( stream, buffer, header, traceId, timeoutToken )
                    .ConfigureAwait( false );

            case MessageBrokerClientEndpoint.HandshakeRejectedResponse:
                return await HandleIncomingHandshakeRejectedResponseAsync( stream, buffer, requestHeader, header, traceId, timeoutToken )
                    .ConfigureAwait( false );

            default:
                return HandleUnexpectedEndpoint( header, traceId );
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private async ValueTask<Result> HandleIncomingHandshakeAcceptedResponseAsync(
        Stream stream,
        Memory<byte> buffer,
        Protocol.PacketHeader header,
        ulong traceId,
        CancellationToken timeoutToken)
    {
        Assume.Equals( header.GetClientEndpoint(), MessageBrokerClientEndpoint.HandshakeAcceptedResponse );
        Chain<string> errors;
        try
        {
            var exception = header.AssertExactPayload( this, Protocol.HandshakeAcceptedResponse.Length );
            if ( exception is not null )
                return EmitError( exception, traceId );

            var data = buffer.Slice( 0, Protocol.HandshakeAcceptedResponse.Length );
            await stream.ReadExactlyAsync( data, timeoutToken ).ConfigureAwait( false );
            var response = Protocol.HandshakeAcceptedResponse.Parse( data );
            errors = response.StringifyErrors();

            using ( AcquireActiveLock( traceId, out var exc ) )
            {
                if ( exc is not null )
                    return exc;

                EventScheduler.ResetReadTimeout();
                Id = response.Id;
                MessageTimeout = response.MessageTimeout;
                PingInterval = response.PingInterval;
                MaxNetworkPacketBytes = unchecked( ( int )response.MaxNetworkPacketLength.Bytes );
                MaxNetworkMessagePacketBytes = unchecked( ( int )response.MaxNetworkMessagePacketLength.Bytes );
                MaxBatchPacketCount = response.MaxBatchPacketCount;
                MaxNetworkBatchPacketBytes = unchecked( ( int )response.MaxNetworkBatchPacketLength.Bytes );
                IsServerLittleEndian = response.IsServerLittleEndian;
            }
        }
        catch ( Exception exc )
        {
            if ( exc is OperationCanceledException cancelExc && cancelExc.CancellationToken == timeoutToken )
            {
                var exception = this.ResponseTimeoutException( MessageBrokerServerEndpoint.HandshakeRequest );
                if ( Logger.Error is { } error )
                    error.Emit( MessageBrokerClientErrorEvent.Create( this, traceId, exception ) );

                return exception;
            }

            if ( Logger.AwaitPacket is { } awaitPacket )
                awaitPacket.Emit( MessageBrokerClientAwaitPacketEvent.Create( this, exc ) );

            return exc;
        }

        if ( errors.Count > 0 )
            return EmitError( this.ProtocolException( header, errors ), traceId );

        if ( Logger.ReadPacket is { } readPacket )
            readPacket.Emit( MessageBrokerClientReadPacketEvent.CreateAccepted( this, traceId, header ) );

        return Result.Valid;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private async ValueTask<Result> HandleIncomingHandshakeRejectedResponseAsync(
        Stream stream,
        Memory<byte> buffer,
        Protocol.PacketHeader requestHeader,
        Protocol.PacketHeader responseHeader,
        ulong traceId,
        CancellationToken timeoutToken)
    {
        Assume.Equals( responseHeader.GetClientEndpoint(), MessageBrokerClientEndpoint.HandshakeRejectedResponse );
        try
        {
            var exception = responseHeader.AssertExactPayload( this, Protocol.HandshakeRejectedResponse.Length );
            if ( exception is not null )
                return EmitError( exception, traceId );

            var data = buffer.Slice( 0, Protocol.HandshakeRejectedResponse.Length );
            await stream.ReadExactlyAsync( data, timeoutToken ).ConfigureAwait( false );
            var response = Protocol.HandshakeRejectedResponse.Parse( data );

            return EmitError( this.RequestException( requestHeader, response.StringifyErrors() ), traceId );
        }
        catch ( Exception exc )
        {
            if ( exc is OperationCanceledException cancelExc && cancelExc.CancellationToken == timeoutToken )
            {
                var exception = this.ResponseTimeoutException( requestHeader );
                if ( Logger.Error is { } error )
                    error.Emit( MessageBrokerClientErrorEvent.Create( this, traceId, exception ) );

                return exception;
            }

            if ( Logger.AwaitPacket is { } awaitPacket )
                awaitPacket.Emit( MessageBrokerClientAwaitPacketEvent.Create( this, exc ) );

            return exc;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private ValueTask<Result> SendConfirmHandshakeResponseAsync(Memory<byte> buffer, ulong traceId)
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
            return ValueTask.FromResult<Result>( EmitError( exc, traceId ) );
        }

        return WriteAsync( response, data, traceId );
    }
}
