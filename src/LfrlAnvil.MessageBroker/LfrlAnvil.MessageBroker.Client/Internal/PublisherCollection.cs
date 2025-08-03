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
using LfrlAnvil.Async;
using LfrlAnvil.Exceptions;
using LfrlAnvil.Memory;
using LfrlAnvil.MessageBroker.Client.Events;
using LfrlAnvil.MessageBroker.Client.Exceptions;

namespace LfrlAnvil.MessageBroker.Client.Internal;

internal struct PublisherCollection
{
    private ObjectStore<MessageBrokerPublisher> _store;

    private PublisherCollection(StringComparer nameComparer)
    {
        _store = ObjectStore<MessageBrokerPublisher>.Create( nameComparer );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static PublisherCollection Create()
    {
        return new PublisherCollection( StringComparer.OrdinalIgnoreCase );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static int GetCount(MessageBrokerClient client)
    {
        using ( client.AcquireLock() )
            return client.PublisherCollection._store.Count;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static ReadOnlyArray<MessageBrokerPublisher> GetAll(MessageBrokerClient client)
    {
        using ( client.AcquireLock() )
            return client.PublisherCollection._store.GetAll();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerPublisher? TryGetByChannelId(MessageBrokerClient client, int channelId)
    {
        using ( client.AcquireLock() )
            return client.PublisherCollection._store.TryGetById( channelId );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerPublisher? TryGetByChannelName(MessageBrokerClient client, string channelName)
    {
        using ( client.AcquireLock() )
            return client.PublisherCollection._store.TryGetByName( channelName );
    }

    internal static async ValueTask<Result<MessageBrokerBindPublisherResult?>> BindAsync(
        MessageBrokerClient client,
        string channelName,
        string? streamName)
    {
        Ensure.IsInRange( channelName.Length, Defaults.NameLengthBounds.Min, Defaults.NameLengthBounds.Max );
        if ( streamName is not null )
            Ensure.IsInRange( streamName.Length, Defaults.NameLengthBounds.Min, Defaults.NameLengthBounds.Max );

        ulong traceId;
        bool reverseEndianness;
        using ( client.AcquireLock() )
        {
            if ( client.ShouldCancel )
                ExceptionThrower.Throw( client.DisposedException() );

            client.AssertState( MessageBrokerClientState.Running );
            var publisher = client.PublisherCollection._store.TryGetByName( channelName );
            if ( publisher is not null )
                return MessageBrokerBindPublisherResult.CreateAlreadyBound( publisher );

            reverseEndianness = BitConverter.IsLittleEndian != client.IsServerLittleEndian;
            traceId = client.GetTraceId();
        }

        using ( MessageBrokerClientTraceEvent.CreateScope( client, traceId, MessageBrokerClientTraceEventType.BindPublisher ) )
        {
            if ( client.Logger.BindingPublisher is { } bindingPublisher )
                bindingPublisher.Emit(
                    MessageBrokerClientBindingPublisherEvent.Create( client, traceId, channelName, streamName ?? channelName ) );

            ManualResetValueTaskSource<IncomingPacketToken> responseSource;
            Protocol.BindPublisherRequest request;

            var poolToken = MemoryPoolToken<byte>.Empty;
            try
            {
                request = new Protocol.BindPublisherRequest( channelName, streamName );
                poolToken = client.MemoryPool.Rent( request.Length, out var buffer ).EnableClearing();
                request.Serialize( buffer, reverseEndianness );

                ManualResetValueTaskSource<WriterSourceResult> writerSource;
                using ( client.AcquireActiveLock( traceId, out var exc ) )
                {
                    if ( exc is not null )
                        return exc;

                    writerSource = client.WriterQueue.AcquireSource( buffer );
                    responseSource = client.ResponseQueue.EnqueueSource();
                }

                var writerResult = await writerSource.GetTask().ConfigureAwait( false );
                switch ( writerResult.Status )
                {
                    case WriterSourceResultStatus.Ready:
                    {
                        if ( ! client.PausePingSchedule( traceId, out var exc ) )
                            return exc;

                        var (packetCount, exception) = await client
                            .WritePotentialBatchAsync( request.Header, buffer, reverseEndianness, traceId )
                            .ConfigureAwait( false );

                        if ( exception is not null )
                        {
                            await client.DisposeAsync( traceId ).ConfigureAwait( false );
                            return exception;
                        }

                        if ( ! client.ReleaseWriterWithResponse( writerSource, responseSource, packetCount, traceId, out exc ) )
                            return exc;

                        break;
                    }
                    case WriterSourceResultStatus.Batched:
                    {
                        if ( ! client.ReleaseBatchedWriterWithResponse(
                            writerSource,
                            responseSource,
                            request.Header,
                            writerResult,
                            traceId,
                            out var exc ) )
                            return exc;

                        break;
                    }
                    default:
                        return client.EmitError( client.DisposedException(), traceId );
                }
            }
            catch ( Exception exc )
            {
                return await client.DisposeAsync( exc, traceId ).ConfigureAwait( false );
            }
            finally
            {
                poolToken.Return( client, traceId );
            }

            var response = await responseSource.GetTask().ConfigureAwait( false );
            try
            {
                if ( response.Type != IncomingPacketToken.Result.Ok )
                    return await client.HandleResponseErrorAsync( response.Type, request.Header, traceId ).ConfigureAwait( false );

                if ( ! client.ReleaseResponse( responseSource, traceId, out var exc ) )
                    return exc;

                switch ( response.Header.GetClientEndpoint() )
                {
                    case MessageBrokerClientEndpoint.PublisherBoundResponse:
                    {
                        var readPacket = client.Logger.ReadPacket;
                        readPacket?.Emit( MessageBrokerClientReadPacketEvent.CreateReceived( client, traceId, response.Header ) );

                        var exception = response.Header.AssertExactPayload( client, Protocol.PublisherBoundResponse.Length );
                        if ( exception is not null )
                        {
                            if ( client.Logger.Error is { } error )
                                error.Emit( MessageBrokerClientErrorEvent.Create( client, traceId, exception ) );

                            await client.DisposeAsync( traceId ).ConfigureAwait( false );
                            return exception;
                        }

                        var parsedResponse = Protocol.PublisherBoundResponse.Parse( response.Data, reverseEndianness );
                        var errors = parsedResponse.StringifyErrors();

                        if ( errors.Count > 0 )
                        {
                            var error = client.EmitError( client.ProtocolException( response.Header, errors ), traceId );
                            await client.DisposeAsync( traceId ).ConfigureAwait( false );
                            return error;
                        }

                        MessageBrokerBindPublisherResult bindResult;
                        using ( client.AcquireActiveLock( traceId, out exc ) )
                        {
                            if ( exc is not null )
                                return exc;

                            var publisher = new MessageBrokerPublisher(
                                client,
                                parsedResponse.ChannelId,
                                channelName,
                                parsedResponse.StreamId,
                                streamName ?? channelName );

                            client.PublisherCollection._store.Add( parsedResponse.ChannelId, channelName, publisher );
                            bindResult = MessageBrokerBindPublisherResult.Create(
                                publisher,
                                parsedResponse.ChannelCreated,
                                parsedResponse.StreamCreated );
                        }

                        readPacket?.Emit( MessageBrokerClientReadPacketEvent.CreateAccepted( client, traceId, response.Header ) );

                        if ( client.Logger.PublisherBound is { } publisherBound )
                            publisherBound.Emit(
                                MessageBrokerClientPublisherBoundEvent.Create(
                                    bindResult.Publisher,
                                    traceId,
                                    parsedResponse.ChannelCreated,
                                    parsedResponse.StreamCreated ) );

                        return bindResult;
                    }
                    case MessageBrokerClientEndpoint.BindPublisherFailureResponse:
                    {
                        var readPacket = client.Logger.ReadPacket;
                        readPacket?.Emit( MessageBrokerClientReadPacketEvent.CreateReceived( client, traceId, response.Header ) );

                        var exception = response.Header.AssertExactPayload( client, Protocol.BindPublisherFailureResponse.Length );
                        if ( exception is not null )
                        {
                            if ( client.Logger.Error is { } error )
                                error.Emit( MessageBrokerClientErrorEvent.Create( client, traceId, exception ) );

                            await client.DisposeAsync( traceId ).ConfigureAwait( false );
                            return exception;
                        }

                        var parsedResponse = Protocol.BindPublisherFailureResponse.Parse( response.Data );
                        readPacket?.Emit( MessageBrokerClientReadPacketEvent.CreateAccepted( client, traceId, response.Header ) );

                        return client.EmitError(
                            client.RequestException( request.Header, parsedResponse.StringifyErrors( channelName ) ),
                            traceId );
                    }
                    default:
                    {
                        var error = client.HandleUnexpectedEndpoint( response.Header, traceId );
                        await client.DisposeAsync( traceId ).ConfigureAwait( false );
                        return error;
                    }
                }
            }
            catch ( Exception exc )
            {
                return await client.DisposeAsync( exc, traceId ).ConfigureAwait( false );
            }
            finally
            {
                response.PoolToken.Return( client, traceId );
            }
        }
    }

    internal static async ValueTask<Result<MessageBrokerUnbindPublisherResult>> UnbindAsync(MessageBrokerPublisher publisher)
    {
        ulong traceId;
        bool reverseEndianness;
        var client = publisher.Client;
        using ( client.AcquireLock() )
        {
            if ( client.ShouldCancel )
                ExceptionThrower.Throw( client.DisposedException() );

            if ( ! publisher.Dispose() )
                return MessageBrokerUnbindPublisherResult.CreateNotBound();

            reverseEndianness = BitConverter.IsLittleEndian != client.IsServerLittleEndian;
            traceId = client.GetTraceId();
        }

        using ( MessageBrokerClientTraceEvent.CreateScope( client, traceId, MessageBrokerClientTraceEventType.UnbindPublisher ) )
        {
            if ( client.Logger.UnbindingPublisher is { } unbindingPublisher )
                unbindingPublisher.Emit( MessageBrokerClientUnbindingPublisherEvent.Create( publisher, traceId ) );

            ManualResetValueTaskSource<IncomingPacketToken> responseSource;
            Protocol.UnbindPublisherRequest request;

            var poolToken = MemoryPoolToken<byte>.Empty;
            try
            {
                request = new Protocol.UnbindPublisherRequest( publisher.ChannelId );
                poolToken = client.MemoryPool.Rent( Protocol.UnbindPublisherRequest.Length, out var buffer ).EnableClearing();
                request.Serialize( buffer, reverseEndianness );

                ManualResetValueTaskSource<WriterSourceResult> writerSource;
                using ( client.AcquireActiveLock( traceId, out var exc ) )
                {
                    if ( exc is not null )
                        return exc;

                    writerSource = client.WriterQueue.AcquireSource( buffer );
                    responseSource = client.ResponseQueue.EnqueueSource();
                }

                var writerResult = await writerSource.GetTask().ConfigureAwait( false );
                switch ( writerResult.Status )
                {
                    case WriterSourceResultStatus.Ready:
                    {
                        if ( ! client.PausePingSchedule( traceId, out var exc ) )
                            return exc;

                        var (packetCount, exception) = await client
                            .WritePotentialBatchAsync( request.Header, buffer, reverseEndianness, traceId )
                            .ConfigureAwait( false );

                        if ( exception is not null )
                        {
                            await client.DisposeAsync( traceId ).ConfigureAwait( false );
                            return exception;
                        }

                        if ( ! client.ReleaseWriterWithResponse( writerSource, responseSource, packetCount, traceId, out exc ) )
                            return exc;

                        break;
                    }
                    case WriterSourceResultStatus.Batched:
                    {
                        if ( ! client.ReleaseBatchedWriterWithResponse(
                            writerSource,
                            responseSource,
                            request.Header,
                            writerResult,
                            traceId,
                            out var exc ) )
                            return exc;

                        break;
                    }
                    default:
                        return client.EmitError( client.DisposedException(), traceId );
                }
            }
            catch ( Exception exc )
            {
                return await client.DisposeAsync( exc, traceId ).ConfigureAwait( false );
            }
            finally
            {
                poolToken.Return( client, traceId );
            }

            var response = await responseSource.GetTask().ConfigureAwait( false );
            try
            {
                if ( response.Type != IncomingPacketToken.Result.Ok )
                    return await client.HandleResponseErrorAsync( response.Type, request.Header, traceId ).ConfigureAwait( false );

                if ( ! client.ReleaseResponse( responseSource, traceId, out var exc ) )
                    return exc;

                switch ( response.Header.GetClientEndpoint() )
                {
                    case MessageBrokerClientEndpoint.PublisherUnboundResponse:
                    {
                        var readPacket = client.Logger.ReadPacket;
                        readPacket?.Emit( MessageBrokerClientReadPacketEvent.CreateReceived( client, traceId, response.Header ) );

                        var exception = response.Header.AssertExactPayload( client, Protocol.PublisherUnboundResponse.Length );
                        if ( exception is not null )
                        {
                            if ( client.Logger.Error is { } error )
                                error.Emit( MessageBrokerClientErrorEvent.Create( client, traceId, exception ) );

                            await client.DisposeAsync( traceId ).ConfigureAwait( false );
                            return exception;
                        }

                        var parsedResponse = Protocol.PublisherUnboundResponse.Parse( response.Data );

                        using ( client.AcquireActiveLock( traceId, out exc ) )
                        {
                            if ( exc is not null )
                                return exc;

                            client.PublisherCollection._store.Remove( publisher.ChannelId, publisher.ChannelName );
                        }

                        readPacket?.Emit( MessageBrokerClientReadPacketEvent.CreateAccepted( client, traceId, response.Header ) );
                        if ( client.Logger.PublisherUnbound is { } publisherUnbound )
                            publisherUnbound.Emit(
                                MessageBrokerClientPublisherUnboundEvent.Create(
                                    publisher,
                                    traceId,
                                    parsedResponse.ChannelRemoved,
                                    parsedResponse.StreamRemoved ) );

                        return MessageBrokerUnbindPublisherResult.Create( parsedResponse.ChannelRemoved, parsedResponse.StreamRemoved );
                    }
                    case MessageBrokerClientEndpoint.UnbindPublisherFailureResponse:
                    {
                        var readPacket = client.Logger.ReadPacket;
                        readPacket?.Emit( MessageBrokerClientReadPacketEvent.CreateReceived( client, traceId, response.Header ) );

                        var exception = response.Header.AssertExactPayload( client, Protocol.UnbindPublisherFailureResponse.Length );
                        if ( exception is not null )
                        {
                            if ( client.Logger.Error is { } error )
                                error.Emit( MessageBrokerClientErrorEvent.Create( client, traceId, exception ) );

                            await client.DisposeAsync( traceId ).ConfigureAwait( false );
                            return exception;
                        }

                        var parsedResponse = Protocol.UnbindPublisherFailureResponse.Parse( response.Data );
                        readPacket?.Emit( MessageBrokerClientReadPacketEvent.CreateAccepted( client, traceId, response.Header ) );

                        return client.EmitError(
                            client.RequestException( request.Header, parsedResponse.StringifyErrors( publisher ) ),
                            traceId );
                    }
                    default:
                    {
                        var error = client.HandleUnexpectedEndpoint( response.Header, traceId );
                        await client.DisposeAsync( traceId ).ConfigureAwait( false );
                        return error;
                    }
                }
            }
            catch ( Exception exc )
            {
                return await client.DisposeAsync( exc, traceId ).ConfigureAwait( false );
            }
            finally
            {
                response.PoolToken.Return( client, traceId );
            }
        }
    }

    internal static async ValueTask<Result<MessageBrokerPushResult>> PushAsync(MessageBrokerPushContext context, bool confirm)
    {
        ulong traceId;
        bool reverseEndianness;
        var client = context.Publisher.Client;
        using ( client.AcquireLock() )
        {
            if ( client.ShouldCancel )
                ExceptionThrower.Throw( client.DisposedException() );

            if ( context.Publisher.State != MessageBrokerPublisherState.Bound )
                return MessageBrokerPushResult.CreateNotBound( confirm );

            reverseEndianness = BitConverter.IsLittleEndian != client.IsServerLittleEndian;
            traceId = client.GetTraceId();
        }

        using ( MessageBrokerClientTraceEvent.CreateScope( client, traceId, MessageBrokerClientTraceEventType.PushMessage ) )
        {
            var buffer = context.Data;
            var routing = context.Routing;
            var routingBuffer = routing.Data;
            var messageLength = unchecked( buffer.Length - Protocol.PushMessageHeader.Length );
            if ( client.Logger.PushingMessage is { } pushingMessage )
                pushingMessage.Emit(
                    MessageBrokerClientPushingMessageEvent.Create(
                        client,
                        traceId,
                        context.Publisher,
                        messageLength,
                        routing.TargetCount,
                        confirm ) );

            var lengthException = AssertPacketLength( client, buffer.Length, routingBuffer.Length );
            if ( lengthException is not null )
            {
                if ( client.Logger.Error is { } error )
                    error.Emit( MessageBrokerClientErrorEvent.Create( client, traceId, lengthException ) );

                return lengthException;
            }

            ManualResetValueTaskSource<IncomingPacketToken>? responseSource = null;
            Protocol.PushMessageHeader request;

            try
            {
                var routingRequest = default( Protocol.PushMessageRoutingHeader );
                if ( routing.TargetCount > 0 )
                {
                    routingRequest = new Protocol.PushMessageRoutingHeader(
                        routing.TargetCount,
                        unchecked( routingBuffer.Length - Protocol.PushMessageRoutingHeader.Length ) );

                    routingRequest.Serialize( routingBuffer.Slice( 0, Protocol.PushMessageRoutingHeader.Length ), reverseEndianness );
                }

                request = new Protocol.PushMessageHeader( context.Publisher.ChannelId, messageLength, confirm, context.ClearOnDispose );
                request.Serialize( buffer.Slice( 0, Protocol.PushMessageHeader.Length ), reverseEndianness );

                ManualResetValueTaskSource<WriterSourceResult>? routingWriterSource = null;
                ManualResetValueTaskSource<WriterSourceResult> writerSource;
                using ( client.AcquireActiveLock( traceId, out var exc ) )
                {
                    if ( exc is not null )
                        return exc;

                    if ( routing.TargetCount > 0 )
                        routingWriterSource = client.WriterQueue.AcquireSource( routingBuffer );

                    writerSource = client.WriterQueue.AcquireSource( buffer );
                    if ( confirm )
                        responseSource = client.ResponseQueue.EnqueueSource();
                }

                if ( routingWriterSource is not null )
                {
                    var routingWriterResult = await routingWriterSource.GetTask().ConfigureAwait( false );
                    switch ( routingWriterResult.Status )
                    {
                        case WriterSourceResultStatus.Ready:
                        {
                            if ( ! client.PausePingSchedule( traceId, out var exc ) )
                                return exc;

                            var (packetCount, exception) = await client
                                .WritePotentialBatchAsync( routingRequest.Header, routingBuffer, reverseEndianness, traceId )
                                .ConfigureAwait( false );

                            if ( exception is not null )
                            {
                                await client.DisposeAsync( traceId ).ConfigureAwait( false );
                                return exception;
                            }

                            if ( ! client.ReleaseWriter( routingWriterSource, packetCount, traceId, out exc ) )
                                return exc;

                            break;
                        }
                        case WriterSourceResultStatus.Batched:
                        {
                            if ( ! client.ReleaseBatchedWriter(
                                routingWriterSource,
                                routingRequest.Header,
                                routingWriterResult,
                                traceId,
                                out var exc ) )
                                return exc;

                            break;
                        }
                        default:
                            return client.EmitError( client.DisposedException(), traceId );
                    }
                }

                var writerResult = await writerSource.GetTask().ConfigureAwait( false );
                switch ( writerResult.Status )
                {
                    case WriterSourceResultStatus.Ready:
                    {
                        if ( ! client.PausePingSchedule( traceId, out var exc ) )
                            return exc;

                        var (packetCount, exception) = await client
                            .WritePotentialBatchAsync( request.Header, buffer, reverseEndianness, traceId )
                            .ConfigureAwait( false );

                        if ( exception is not null )
                        {
                            await client.DisposeAsync( traceId ).ConfigureAwait( false );
                            return exception;
                        }

                        if ( responseSource is not null
                            ? ! client.ReleaseWriterWithResponse( writerSource, responseSource, packetCount, traceId, out exc )
                            : ! client.ReleaseWriter( writerSource, packetCount, traceId, out exc ) )
                            return exc;

                        break;
                    }
                    case WriterSourceResultStatus.Batched:
                    {
                        if ( responseSource is not null
                            ? ! client.ReleaseBatchedWriterWithResponse(
                                writerSource,
                                responseSource,
                                request.Header,
                                writerResult,
                                traceId,
                                out var exc )
                            : ! client.ReleaseBatchedWriter( writerSource, request.Header, writerResult, traceId, out exc ) )
                            return exc;

                        break;
                    }
                    default:
                        return client.EmitError( client.DisposedException(), traceId );
                }
            }
            catch ( Exception exc )
            {
                return await client.DisposeAsync( exc, traceId ).ConfigureAwait( false );
            }

            if ( responseSource is null )
            {
                if ( client.Logger.MessagePushed is { } messagePushed )
                    messagePushed.Emit( MessageBrokerClientMessagePushedEvent.Create( client, traceId, context.Publisher, messageLength ) );

                return MessageBrokerPushResult.CreateUnconfirmed();
            }

            var response = await responseSource.GetTask().ConfigureAwait( false );
            try
            {
                if ( response.Type != IncomingPacketToken.Result.Ok )
                    return await client.HandleResponseErrorAsync( response.Type, request.Header, traceId ).ConfigureAwait( false );

                if ( ! client.ReleaseResponse( responseSource, traceId, out var exc ) )
                    return exc;

                switch ( response.Header.GetClientEndpoint() )
                {
                    case MessageBrokerClientEndpoint.MessageAcceptedResponse:
                    {
                        var readPacket = client.Logger.ReadPacket;
                        readPacket?.Emit( MessageBrokerClientReadPacketEvent.CreateReceived( client, traceId, response.Header ) );

                        var exception = response.Header.AssertExactPayload( client, Protocol.MessageAcceptedResponse.Length );
                        if ( exception is not null )
                        {
                            if ( client.Logger.Error is { } error )
                                error.Emit( MessageBrokerClientErrorEvent.Create( client, traceId, exception ) );

                            await client.DisposeAsync( traceId ).ConfigureAwait( false );
                            return exception;
                        }

                        var parsedResponse = Protocol.MessageAcceptedResponse.Parse( response.Data, reverseEndianness );
                        readPacket?.Emit( MessageBrokerClientReadPacketEvent.CreateAccepted( client, traceId, response.Header ) );
                        if ( client.Logger.MessagePushed is { } messagePushed )
                            messagePushed.Emit(
                                MessageBrokerClientMessagePushedEvent.Create(
                                    client,
                                    traceId,
                                    context.Publisher,
                                    messageLength,
                                    parsedResponse.Id ) );

                        return MessageBrokerPushResult.Create( parsedResponse.Id );
                    }
                    case MessageBrokerClientEndpoint.MessageRejectedResponse:
                    {
                        var readPacket = client.Logger.ReadPacket;
                        readPacket?.Emit( MessageBrokerClientReadPacketEvent.CreateReceived( client, traceId, response.Header ) );

                        var exception = response.Header.AssertExactPayload( client, Protocol.MessageRejectedResponse.Length );
                        if ( exception is not null )
                        {
                            if ( client.Logger.Error is { } error )
                                error.Emit( MessageBrokerClientErrorEvent.Create( client, traceId, exception ) );

                            await client.DisposeAsync( traceId ).ConfigureAwait( false );
                            return exception;
                        }

                        var parsedResponse = Protocol.MessageRejectedResponse.Parse( response.Data );
                        readPacket?.Emit( MessageBrokerClientReadPacketEvent.CreateAccepted( client, traceId, response.Header ) );

                        return client.EmitError(
                            client.RequestException( request.Header, parsedResponse.StringifyErrors( context.Publisher ) ),
                            traceId );
                    }
                    default:
                    {
                        var error = client.HandleUnexpectedEndpoint( response.Header, traceId );
                        await client.DisposeAsync( traceId ).ConfigureAwait( false );
                        return error;
                    }
                }
            }
            catch ( Exception exc )
            {
                return await client.DisposeAsync( exc, traceId ).ConfigureAwait( false );
            }
            finally
            {
                response.PoolToken.Return( client, traceId );
            }
        }
    }

    internal void Clear()
    {
        foreach ( var obj in _store )
            obj.OnClientDisposed();

        _store.Clear();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static InvalidOperationException? AssertPacketLength(MessageBrokerClient client, int messageLength, int routingLength)
    {
        var errors = Chain<string>.Empty;
        if ( messageLength > client.MaxNetworkPushMessagePacketBytes )
            errors = errors.Extend( Resources.MaxMessagePacketLengthLimitReached( client, messageLength ) );

        if ( routingLength > client.MaxNetworkPacketBytes )
            errors = errors.Extend( Resources.MaxRoutingPacketLengthLimitReached( client, routingLength ) );

        return errors.Count > 0 ? new InvalidOperationException( Resources.FailedToPushMessage( errors ) ) : null;
    }
}
