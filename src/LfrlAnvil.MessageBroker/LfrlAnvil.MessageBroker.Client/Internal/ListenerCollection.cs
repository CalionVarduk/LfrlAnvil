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

internal struct ListenerCollection
{
    private ObjectStore<MessageBrokerListener> _store;

    private ListenerCollection(StringComparer nameComparer)
    {
        _store = ObjectStore<MessageBrokerListener>.Create( nameComparer );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static ListenerCollection Create()
    {
        return new ListenerCollection( StringComparer.OrdinalIgnoreCase );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static int GetCount(MessageBrokerClient client)
    {
        using ( client.AcquireLock() )
            return client.ListenerCollection._store.Count;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static ReadOnlyArray<MessageBrokerListener> GetAll(MessageBrokerClient client)
    {
        using ( client.AcquireLock() )
            return client.ListenerCollection._store.GetAll();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerListener? TryGetByChannelId(MessageBrokerClient client, int channelId)
    {
        using ( client.AcquireLock() )
            return client.ListenerCollection.TryGetByChannelIdUnsafe( channelId );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerListener? TryGetByChannelName(MessageBrokerClient client, string channelName)
    {
        using ( client.AcquireLock() )
            return client.ListenerCollection._store.TryGetByName( channelName );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal MessageBrokerListener? TryGetByChannelIdUnsafe(int channelId)
    {
        return _store.TryGetById( channelId );
    }

    internal static async ValueTask<Result<MessageBrokerBindListenerResult?>> BindAsync(
        MessageBrokerClient client,
        string channelName,
        string? queueName,
        MessageBrokerListenerCallback callback,
        MessageBrokerListenerOptions options,
        bool createChannelIfNotExists)
    {
        Ensure.IsInRange( channelName.Length, Defaults.NameLengthBounds.Min, Defaults.NameLengthBounds.Max );
        if ( queueName is not null )
            Ensure.IsInRange( queueName.Length, Defaults.NameLengthBounds.Min, Defaults.NameLengthBounds.Max );

        var prefetchHint = options.PrefetchHint;
        var maxRetries = options.MaxRetries;
        var retryDelay = options.RetryDelay;
        var maxRedeliveries = options.MaxRedeliveries;
        var minAckTimeout = options.MinAckTimeout;
        var deadLetterCapacityHint = options.DeadLetterCapacityHint;
        var minDeadLetterRetention = options.MinDeadLetterRetention;
        var filterExpression = options.FilterExpression;

        ulong traceId;
        bool reverseEndianness;
        using ( client.AcquireLock() )
        {
            if ( client.ShouldCancel )
                ExceptionThrower.Throw( client.DisposedException() );

            client.AssertState( MessageBrokerClientState.Running );
            var listener = client.ListenerCollection._store.TryGetByName( channelName );
            if ( listener is not null )
                return MessageBrokerBindListenerResult.CreateAlreadyBound( listener );

            reverseEndianness = BitConverter.IsLittleEndian != client.IsServerLittleEndian;
            traceId = client.GetTraceId();
        }

        using ( MessageBrokerClientTraceEvent.CreateScope( client, traceId, MessageBrokerClientTraceEventType.BindListener ) )
        {
            if ( client.Logger.BindingListener is { } bindingListener )
                bindingListener.Emit(
                    MessageBrokerClientBindingListenerEvent.Create(
                        client,
                        traceId,
                        channelName,
                        queueName ?? channelName,
                        prefetchHint,
                        maxRetries,
                        retryDelay,
                        maxRedeliveries,
                        minAckTimeout,
                        deadLetterCapacityHint,
                        minDeadLetterRetention,
                        filterExpression,
                        createChannelIfNotExists ) );

            ManualResetValueTaskSource<IncomingPacketToken> responseSource;
            Protocol.BindListenerRequest request;

            var poolToken = MemoryPoolToken<byte>.Empty;
            try
            {
                request = new Protocol.BindListenerRequest(
                    channelName,
                    queueName,
                    prefetchHint,
                    maxRetries,
                    retryDelay,
                    maxRedeliveries,
                    minAckTimeout,
                    deadLetterCapacityHint,
                    minDeadLetterRetention,
                    filterExpression,
                    createChannelIfNotExists );

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
                    case MessageBrokerClientEndpoint.ListenerBoundResponse:
                    {
                        var readPacket = client.Logger.ReadPacket;
                        readPacket?.Emit( MessageBrokerClientReadPacketEvent.CreateReceived( client, traceId, response.Header ) );

                        var exception = response.Header.AssertExactPayload( client, Protocol.ListenerBoundResponse.Length );
                        if ( exception is not null )
                        {
                            if ( client.Logger.Error is { } error )
                                error.Emit( MessageBrokerClientErrorEvent.Create( client, traceId, exception ) );

                            await client.DisposeAsync( traceId ).ConfigureAwait( false );
                            return exception;
                        }

                        var parsedResponse = Protocol.ListenerBoundResponse.Parse( response.Data, reverseEndianness );
                        var errors = parsedResponse.StringifyErrors();

                        if ( errors.Count > 0 )
                        {
                            var error = client.EmitError( client.ProtocolException( response.Header, errors ), traceId );
                            await client.DisposeAsync( traceId ).ConfigureAwait( false );
                            return error;
                        }

                        MessageBrokerBindListenerResult bindResult;
                        using ( client.AcquireActiveLock( traceId, out exc ) )
                        {
                            if ( exc is not null )
                                return exc;

                            var listener = new MessageBrokerListener(
                                client,
                                parsedResponse.ChannelId,
                                channelName,
                                parsedResponse.QueueId,
                                queueName ?? channelName,
                                prefetchHint,
                                maxRetries,
                                retryDelay,
                                maxRedeliveries,
                                minAckTimeout,
                                deadLetterCapacityHint,
                                minDeadLetterRetention,
                                filterExpression,
                                callback );

                            client.ListenerCollection._store.Add( parsedResponse.ChannelId, channelName, listener );
                            bindResult = MessageBrokerBindListenerResult.Create(
                                listener,
                                parsedResponse.ChannelCreated,
                                parsedResponse.QueueCreated );
                        }

                        readPacket?.Emit( MessageBrokerClientReadPacketEvent.CreateAccepted( client, traceId, response.Header ) );
                        if ( client.Logger.ListenerBound is { } listenerBound )
                            listenerBound.Emit(
                                MessageBrokerClientListenerBoundEvent.Create(
                                    bindResult.Listener,
                                    traceId,
                                    parsedResponse.ChannelCreated,
                                    parsedResponse.QueueCreated ) );

                        return bindResult;
                    }
                    case MessageBrokerClientEndpoint.BindListenerFailureResponse:
                    {
                        var readPacket = client.Logger.ReadPacket;
                        readPacket?.Emit( MessageBrokerClientReadPacketEvent.CreateReceived( client, traceId, response.Header ) );

                        var exception = response.Header.AssertExactPayload( client, Protocol.BindListenerFailureResponse.Length );
                        if ( exception is not null )
                        {
                            if ( client.Logger.Error is { } error )
                                error.Emit( MessageBrokerClientErrorEvent.Create( client, traceId, exception ) );

                            await client.DisposeAsync( traceId ).ConfigureAwait( false );
                            return exception;
                        }

                        var parsedResponse = Protocol.BindListenerFailureResponse.Parse( response.Data );
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

    internal static async ValueTask<Result<MessageBrokerUnbindListenerResult>> UnbindAsync(MessageBrokerListener listener)
    {
        ulong traceId;
        bool reverseEndianness;
        var client = listener.Client;
        using ( client.AcquireLock() )
        {
            if ( client.ShouldCancel )
                ExceptionThrower.Throw( client.DisposedException() );

            if ( ! listener.BeginDispose() )
                return MessageBrokerUnbindListenerResult.CreateNotBound();

            reverseEndianness = BitConverter.IsLittleEndian != client.IsServerLittleEndian;
            traceId = client.GetTraceId();
        }

        using ( MessageBrokerClientTraceEvent.CreateScope( client, traceId, MessageBrokerClientTraceEventType.UnbindListener ) )
        {
            if ( client.Logger.UnbindingListener is { } unbindingListener )
                unbindingListener.Emit( MessageBrokerClientUnbindingListenerEvent.Create( listener, traceId ) );

            var endDispose = listener.EndDisposingAsync( traceId );
            try
            {
                ManualResetValueTaskSource<IncomingPacketToken> responseSource;
                Protocol.UnbindListenerRequest request;

                var poolToken = MemoryPoolToken<byte>.Empty;
                try
                {
                    request = new Protocol.UnbindListenerRequest( listener.ChannelId );
                    poolToken = client.MemoryPool.Rent( Protocol.UnbindListenerRequest.Length, out var buffer ).EnableClearing();
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
                        case MessageBrokerClientEndpoint.ListenerUnboundResponse:
                        {
                            var readPacket = client.Logger.ReadPacket;
                            readPacket?.Emit( MessageBrokerClientReadPacketEvent.CreateReceived( client, traceId, response.Header ) );

                            var exception = response.Header.AssertExactPayload( client, Protocol.ListenerUnboundResponse.Length );
                            if ( exception is not null )
                            {
                                if ( client.Logger.Error is { } error )
                                    error.Emit( MessageBrokerClientErrorEvent.Create( client, traceId, exception ) );

                                await client.DisposeAsync( traceId ).ConfigureAwait( false );
                                return exception;
                            }

                            var parsedResponse = Protocol.ListenerUnboundResponse.Parse( response.Data );

                            using ( client.AcquireActiveLock( traceId, out exc ) )
                            {
                                if ( exc is not null )
                                    return exc;

                                client.ListenerCollection._store.Remove( listener.ChannelId, listener.ChannelName );
                            }

                            readPacket?.Emit( MessageBrokerClientReadPacketEvent.CreateAccepted( client, traceId, response.Header ) );
                            if ( client.Logger.ListenerUnbound is { } listenerUnbound )
                                listenerUnbound.Emit(
                                    MessageBrokerClientListenerUnboundEvent.Create(
                                        listener,
                                        traceId,
                                        parsedResponse.ChannelRemoved,
                                        parsedResponse.QueueRemoved ) );

                            return MessageBrokerUnbindListenerResult.Create( parsedResponse.ChannelRemoved, parsedResponse.QueueRemoved );
                        }
                        case MessageBrokerClientEndpoint.UnbindListenerFailureResponse:
                        {
                            var readPacket = client.Logger.ReadPacket;
                            readPacket?.Emit( MessageBrokerClientReadPacketEvent.CreateReceived( client, traceId, response.Header ) );

                            var exception = response.Header.AssertExactPayload( client, Protocol.UnbindListenerFailureResponse.Length );
                            if ( exception is not null )
                            {
                                if ( client.Logger.Error is { } error )
                                    error.Emit( MessageBrokerClientErrorEvent.Create( client, traceId, exception ) );

                                await client.DisposeAsync( traceId ).ConfigureAwait( false );
                                return exception;
                            }

                            var parsedResponse = Protocol.UnbindListenerFailureResponse.Parse( response.Data );
                            readPacket?.Emit( MessageBrokerClientReadPacketEvent.CreateAccepted( client, traceId, response.Header ) );

                            return client.EmitError(
                                client.RequestException( request.Header, parsedResponse.StringifyErrors( listener ) ),
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
            finally
            {
                await endDispose.ConfigureAwait( false );
            }
        }
    }

    internal static async ValueTask<Result<bool>> SendMessageAckAsync(
        MessageBrokerListener listener,
        int ackId,
        int streamId,
        ulong messageId,
        int retry,
        int redelivery,
        ulong? messageTraceId)
    {
        Ensure.IsGreaterThan( ackId, 0 );
        Ensure.IsGreaterThan( streamId, 0 );
        Ensure.IsInRange( retry, 0, listener.MaxRetries );
        Ensure.IsInRange( redelivery, 0, listener.MaxRedeliveries );
        if ( ! listener.AreAcksEnabled )
            ExceptionThrower.Throw( listener.Client.MessageException( listener, Resources.DisabledAcks( listener ) ) );

        ulong traceId;
        bool reverseEndianness;
        var client = listener.Client;
        using ( client.AcquireLock() )
        {
            if ( client.ShouldCancel )
                ExceptionThrower.Throw( client.DisposedException() );

            if ( listener.State != MessageBrokerListenerState.Bound )
                return false;

            reverseEndianness = BitConverter.IsLittleEndian != client.IsServerLittleEndian;
            traceId = client.GetTraceId();
        }

        using ( MessageBrokerClientTraceEvent.CreateScope( client, traceId, MessageBrokerClientTraceEventType.Ack ) )
        {
            if ( client.Logger.AcknowledgingMessage is { } acknowledgingMessage )
                acknowledgingMessage.Emit(
                    MessageBrokerClientAcknowledgingMessageEvent.Create(
                        listener,
                        traceId,
                        ackId,
                        streamId,
                        messageId,
                        retry,
                        redelivery,
                        messageTraceId,
                        null,
                        false ) );

            var poolToken = MemoryPoolToken<byte>.Empty;
            try
            {
                var request = new Protocol.MessageNotificationAck(
                    listener.QueueId,
                    ackId,
                    streamId,
                    messageId,
                    retry,
                    redelivery );

                poolToken = client.MemoryPool.Rent( Protocol.MessageNotificationAck.Length, out var buffer ).EnableClearing();
                request.Serialize( buffer, reverseEndianness );

                ManualResetValueTaskSource<WriterSourceResult> writerSource;
                using ( client.AcquireActiveLock( traceId, out var exc ) )
                {
                    if ( exc is not null )
                        return exc;

                    writerSource = client.WriterQueue.AcquireSource( buffer );
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

                        if ( ! client.ReleaseWriter( writerSource, packetCount, traceId, out exc ) )
                            return exc;

                        break;
                    }
                    case WriterSourceResultStatus.Batched:
                    {
                        if ( ! client.ReleaseBatchedWriter( writerSource, request.Header, writerResult, traceId, out var exc ) )
                            return exc;

                        break;
                    }
                    default:
                        return client.EmitError( client.DisposedException(), traceId );
                }

                if ( client.Logger.MessageAcknowledged is { } messageAcknowledged )
                    messageAcknowledged.Emit(
                        MessageBrokerClientMessageAcknowledgedEvent.Create(
                            listener,
                            traceId,
                            ackId,
                            streamId,
                            messageId,
                            retry,
                            redelivery,
                            false ) );
            }
            catch ( Exception exc )
            {
                return await client.DisposeAsync( exc, traceId ).ConfigureAwait( false );
            }
            finally
            {
                poolToken.Return( client, traceId );
            }
        }

        return true;
    }

    internal static async ValueTask<Result<bool>> SendNegativeMessageAckAsync(
        MessageBrokerListener listener,
        int ackId,
        int streamId,
        ulong messageId,
        int retry,
        int redelivery,
        ulong? messageTraceId,
        MessageBrokerNegativeAck nack,
        bool automatic)
    {
        Ensure.IsGreaterThan( ackId, 0 );
        Ensure.IsGreaterThan( streamId, 0 );
        Ensure.IsInRange( retry, 0, listener.MaxRetries );
        Ensure.IsInRange( redelivery, 0, listener.MaxRedeliveries );
        if ( ! listener.AreAcksEnabled )
            ExceptionThrower.Throw( listener.Client.MessageException( listener, Resources.DisabledAcks( listener ) ) );

        ulong traceId;
        bool reverseEndianness;
        var client = listener.Client;
        using ( client.AcquireLock() )
        {
            if ( client.ShouldCancel )
            {
                if ( automatic )
                    return false;

                ExceptionThrower.Throw( client.DisposedException() );
            }

            if ( listener.State != MessageBrokerListenerState.Bound )
                return false;

            reverseEndianness = BitConverter.IsLittleEndian != client.IsServerLittleEndian;
            traceId = client.GetTraceId();
        }

        using ( MessageBrokerClientTraceEvent.CreateScope( client, traceId, MessageBrokerClientTraceEventType.NegativeAck ) )
        {
            if ( client.Logger.AcknowledgingMessage is { } acknowledgingMessage )
                acknowledgingMessage.Emit(
                    MessageBrokerClientAcknowledgingMessageEvent.Create(
                        listener,
                        traceId,
                        ackId,
                        streamId,
                        messageId,
                        retry,
                        redelivery,
                        messageTraceId,
                        nack,
                        automatic ) );

            var poolToken = MemoryPoolToken<byte>.Empty;
            try
            {
                var request = new Protocol.MessageNotificationNegativeAck(
                    listener.QueueId,
                    ackId,
                    streamId,
                    messageId,
                    retry,
                    redelivery,
                    nack.SkipRetry,
                    nack.SkipDeadLetter,
                    nack.RetryDelay );

                poolToken = client.MemoryPool.Rent( Protocol.MessageNotificationNegativeAck.Length, out var buffer ).EnableClearing();
                request.Serialize( buffer, reverseEndianness );

                ManualResetValueTaskSource<WriterSourceResult> writerSource;
                using ( client.AcquireActiveLock( traceId, out var exc ) )
                {
                    if ( exc is not null )
                        return exc;

                    writerSource = client.WriterQueue.AcquireSource( buffer );
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

                        if ( ! client.ReleaseWriter( writerSource, packetCount, traceId, out exc ) )
                            return exc;

                        break;
                    }
                    case WriterSourceResultStatus.Batched:
                    {
                        if ( ! client.ReleaseBatchedWriter( writerSource, request.Header, writerResult, traceId, out var exc ) )
                            return exc;

                        break;
                    }
                    default:
                        return client.EmitError( client.DisposedException(), traceId );
                }

                if ( client.Logger.MessageAcknowledged is { } messageAcknowledged )
                    messageAcknowledged.Emit(
                        MessageBrokerClientMessageAcknowledgedEvent.Create(
                            listener,
                            traceId,
                            ackId,
                            streamId,
                            messageId,
                            retry,
                            redelivery,
                            true ) );
            }
            catch ( Exception exc )
            {
                return await client.DisposeAsync( exc, traceId ).ConfigureAwait( false );
            }
            finally
            {
                poolToken.Return( client, traceId );
            }
        }

        return true;
    }

    internal MessageBrokerListener[] Clear()
    {
        return _store.ClearAndExtract();
    }
}
