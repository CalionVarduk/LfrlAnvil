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
using LfrlAnvil.Computable.Expressions;
using LfrlAnvil.Memory;
using LfrlAnvil.MessageBroker.Server.Events;
using LfrlAnvil.MessageBroker.Server.Exceptions;

namespace LfrlAnvil.MessageBroker.Server.Internal;

internal struct RequestHandler
{
    private readonly ManualResetValueTaskSource<bool> _continuation;
    private Task? _task;

    private RequestHandler(ManualResetValueTaskSource<bool> continuation)
    {
        _continuation = continuation;
        _task = null;
    }

    [Pure]
    internal static RequestHandler Create()
    {
        return new RequestHandler( new ManualResetValueTaskSource<bool>() );
    }

    [MethodImpl( MethodImplOptions.NoInlining )]
    internal static async Task StartUnderlyingTask(MessageBrokerRemoteClient client)
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
                client.RequestHandler._task = null;
                traceId = client.GetTraceId();
            }

            using ( MessageBrokerRemoteClientTraceEvent.CreateScope( client, traceId, MessageBrokerRemoteClientTraceEventType.Unexpected ) )
            {
                if ( client.Logger.Error is { } error )
                    error.Emit( MessageBrokerRemoteClientErrorEvent.Create( client, traceId, exc ) );

                await client.DisposeAsync( traceId ).ConfigureAwait( false );
            }
        }

        Assume.IsGreaterThanOrEqualTo( client.State, MessageBrokerRemoteClientState.Disposing );
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
    private static async ValueTask RunCore(MessageBrokerRemoteClient client)
    {
        var pongData = new byte[Protocol.PacketHeader.Length].AsMemory();
        var pongResponse = Protocol.Pong.Create();
        pongResponse.Serialize( pongData );

        while ( true )
        {
            var @continue = await client.RequestHandler._continuation.GetTask().ConfigureAwait( false );
            if ( ! @continue )
                return;

            bool containsRequests;
            do
            {
                ulong traceId;
                IncomingPacketToken request;
                using ( client.AcquireLock() )
                {
                    if ( client.ShouldCancel )
                        return;

                    request = client.RequestQueue.Dequeue();
                    traceId = client.GetTraceId();
                }

                var result = request.Header.GetServerEndpoint() switch
                {
                    MessageBrokerServerEndpoint.Ping => await HandlePingAsync(
                            client,
                            request.Header,
                            pongResponse,
                            pongData,
                            traceId )
                        .ConfigureAwait( false ),
                    MessageBrokerServerEndpoint.PushMessageRouting => await HandlePushMessageRoutingAsync( client, request, traceId )
                        .ConfigureAwait( false ),
                    MessageBrokerServerEndpoint.PushMessage => await HandlePushMessageAsync( client, request, traceId )
                        .ConfigureAwait( false ),
                    MessageBrokerServerEndpoint.BindPublisherRequest => await HandleBindPublisherRequestAsync( client, request, traceId )
                        .ConfigureAwait( false ),
                    MessageBrokerServerEndpoint.UnbindPublisherRequest => await HandleUnbindPublisherRequestAsync(
                            client,
                            request,
                            traceId )
                        .ConfigureAwait( false ),
                    MessageBrokerServerEndpoint.BindListenerRequest => await HandleBindListenerRequestAsync( client, request, traceId )
                        .ConfigureAwait( false ),
                    MessageBrokerServerEndpoint.UnbindListenerRequest => await HandleUnbindListenerRequestAsync( client, request, traceId )
                        .ConfigureAwait( false ),
                    MessageBrokerServerEndpoint.DeadLetterQuery => await HandleDeadLetterQueryAsync( client, request, traceId )
                        .ConfigureAwait( false ),
                    MessageBrokerServerEndpoint.MessageNotificationAck => await HandleMessageNotificationAckAsync(
                            client,
                            request,
                            traceId )
                        .ConfigureAwait( false ),
                    MessageBrokerServerEndpoint.MessageNotificationNack => await HandleMessageNotificationNackAsync(
                            client,
                            request,
                            traceId )
                        .ConfigureAwait( false ),
                    _ => await HandleUnexpectedRequestAsync( client, request.Header, traceId ).ConfigureAwait( false )
                };

                if ( result.IsDone )
                    return;

                containsRequests = result.Continue;
            }
            while ( containsRequests );

            using ( client.AcquireLock() )
            {
                if ( client.ShouldCancel )
                    return;

                client.RequestHandler._continuation.Reset();
                if ( client.RequestQueue.IsNotEmpty() )
                    client.RequestHandler.SignalContinuation();
            }
        }
    }

    private static async ValueTask<RequestResult> HandleBindPublisherRequestAsync(
        MessageBrokerRemoteClient client,
        IncomingPacketToken request,
        ulong traceId)
    {
        using ( MessageBrokerRemoteClientTraceEvent.CreateScope( client, traceId, MessageBrokerRemoteClientTraceEventType.BindPublisher ) )
        {
            var poolToken = request.PoolToken;
            try
            {
                var readPacket = client.Logger.ReadPacket;
                readPacket?.Emit( MessageBrokerRemoteClientReadPacketEvent.CreateReceived( client, traceId, request.Header ) );

                var exception = request.Header.AssertMinPayload( client, Protocol.BindPublisherRequestHeader.Length );
                if ( exception is not null )
                    return await FinishInvalidRequestHandlingAsync( client, exception, traceId ).ConfigureAwait( false );

                var data = request.Data;
                var parsedRequestHeader = Protocol.BindPublisherRequestHeader.Parse( data );

                var requestErrors = parsedRequestHeader.StringifyErrors( data.Length );
                if ( requestErrors.Count > 0 )
                {
                    var error = client.ProtocolException( request.Header, requestErrors );
                    return await FinishInvalidRequestHandlingAsync( client, error, traceId ).ConfigureAwait( false );
                }

                var channelName = TextEncoding.Parse(
                    data.Slice( Protocol.BindPublisherRequestHeader.Length, parsedRequestHeader.ChannelNameLength ) );

                if ( channelName.Exception is not null )
                    return await FinishInvalidRequestHandlingAsync( client, channelName.Exception, traceId ).ConfigureAwait( false );

                Assume.IsNotNull( channelName.Value );
                if ( ! Defaults.NameLengthBounds.Contains( channelName.Value.Length ) )
                {
                    var error = client.ProtocolException( request.Header, Resources.InvalidChannelNameLength( channelName.Value.Length ) );
                    return await FinishInvalidRequestHandlingAsync( client, error, traceId ).ConfigureAwait( false );
                }

                var streamName = TextEncoding.Parse(
                    data.Slice( Protocol.BindPublisherRequestHeader.Length + parsedRequestHeader.ChannelNameLength ) );

                if ( streamName.Exception is not null )
                    return await FinishInvalidRequestHandlingAsync( client, streamName.Exception, traceId ).ConfigureAwait( false );

                Assume.IsNotNull( streamName.Value );
                if ( streamName.Value.Length > Defaults.NameLengthBounds.Max )
                {
                    var error = client.ProtocolException( request.Header, Resources.InvalidStreamNameLength( streamName.Value.Length ) );
                    return await FinishInvalidRequestHandlingAsync( client, error, traceId ).ConfigureAwait( false );
                }

                if ( client.Logger.BindingPublisher is { } bindingPublisher )
                    bindingPublisher.Emit(
                        MessageBrokerRemoteClientBindingPublisherEvent.Create( client, traceId, channelName.Value, streamName.Value ) );

                if ( streamName.Value.Length == 0 )
                    streamName = channelName;

                bool channelCreated;
                var streamCreated = false;
                ulong channelTraceId = 0;
                ulong streamTraceId = 0;
                MessageBrokerChannel channel;
                MessageBrokerStream? stream = null;
                MessageBrokerChannelPublisherBinding? publisher = null;
                BindResult bindResult;
                ManualResetValueTaskSource<bool> writerSource;

                using ( AcquireActiveServerLock( client, traceId, out var serverExc ) )
                {
                    if ( serverExc is not null )
                        return RequestResult.Done();

                    using ( client.AcquireActiveLock( traceId, out var exc ) )
                    {
                        if ( exc is not null )
                            return RequestResult.Done();

                        channel = ChannelCollection.RegisterUnsafe( client.Server, channelName.Value, out channelCreated );
                        try
                        {
                            bindResult = client.BindPublisherUnsafe(
                                channel,
                                channelCreated,
                                streamName.Value,
                                ref publisher,
                                ref channelTraceId,
                                ref stream,
                                ref streamTraceId,
                                ref streamCreated );
                        }
                        catch
                        {
                            if ( channelCreated )
                                ChannelCollection.RemoveUnsafe( channel );

                            throw;
                        }

                        // TODO:
                        // packet batching will probably require data to be prepared before acquiring writer source
                        writerSource = client.WriterQueue.AcquireSource();
                    }
                }

                Protocol.PacketHeader responseHeader;
                Memory<byte> responseData;

                if ( bindResult != BindResult.Success )
                {
                    if ( client.Logger.Error is { } error )
                    {
                        Exception exc = bindResult switch
                        {
                            BindResult.AlreadyBound => client.PublisherException(
                                publisher,
                                Resources.PublisherAlreadyBound( publisher! ) ),
                            BindResult.ChannelDisposed => channel.DisposedException(),
                            _ => stream!.DisposedException()
                        };

                        error.Emit( MessageBrokerRemoteClientErrorEvent.Create( client, traceId, exc ) );
                    }

                    var responseLength = Protocol.PacketHeader.Length + Protocol.BindPublisherFailureResponse.Payload;
                    var response = new Protocol.BindPublisherFailureResponse( bindResult );
                    if ( data.Length < responseLength )
                        poolToken.IncreaseLength( responseLength, out data );

                    responseHeader = response.Header;
                    responseData = data.Slice( 0, responseLength );
                    response.Serialize( responseData );
                }
                else
                {
                    Assume.IsNotNull( channel );
                    Assume.IsNotNull( stream );
                    Assume.IsNotNull( publisher );
                    readPacket?.Emit( MessageBrokerRemoteClientReadPacketEvent.CreateAccepted( client, traceId, request.Header ) );

                    using ( MessageBrokerChannelTraceEvent.CreateScope(
                        channel,
                        channelTraceId,
                        MessageBrokerChannelTraceEventType.BindPublisher ) )
                    {
                        if ( channel.Logger.ClientTrace is { } clientTrace )
                            clientTrace.Emit( MessageBrokerChannelClientTraceEvent.Create( channel, channelTraceId, client, traceId ) );

                        if ( channelCreated && channel.Logger.Created is { } created )
                            created.Emit( MessageBrokerChannelCreatedEvent.Create( channel, channelTraceId ) );

                        if ( channel.Logger.PublisherBound is { } channelPublisherBound )
                            channelPublisherBound.Emit(
                                MessageBrokerChannelPublisherBoundEvent.Create( publisher, channelTraceId, streamCreated ) );
                    }

                    using ( MessageBrokerStreamTraceEvent.CreateScope(
                        stream,
                        streamTraceId,
                        MessageBrokerStreamTraceEventType.BindPublisher ) )
                    {
                        if ( stream.Logger.ClientTrace is { } clientTrace )
                            clientTrace.Emit( MessageBrokerStreamClientTraceEvent.Create( stream, streamTraceId, client, traceId ) );

                        if ( streamCreated && stream.Logger.Created is { } created )
                            created.Emit( MessageBrokerStreamCreatedEvent.Create( stream, streamTraceId ) );

                        if ( stream.Logger.PublisherBound is { } streamPublisherBound )
                            streamPublisherBound.Emit(
                                MessageBrokerStreamPublisherBoundEvent.Create( publisher, streamTraceId, channelCreated ) );
                    }

                    if ( client.Logger.PublisherBound is { } publisherBound )
                        publisherBound.Emit(
                            MessageBrokerRemoteClientPublisherBoundEvent.Create( publisher, traceId, channelCreated, streamCreated ) );

                    var responseLength = Protocol.PacketHeader.Length + Protocol.PublisherBoundResponse.Payload;
                    var response = new Protocol.PublisherBoundResponse( channelCreated, streamCreated, channel.Id, stream.Id );
                    if ( data.Length < responseLength )
                        poolToken.IncreaseLength( responseLength, out data );

                    responseHeader = response.Header;
                    responseData = data.Slice( 0, responseLength );
                    response.Serialize( responseData );
                }

                if ( ! await writerSource.GetTask().ConfigureAwait( false ) )
                {
                    if ( client.Logger.Error is { } error )
                        error.Emit( MessageBrokerRemoteClientErrorEvent.Create( client, traceId, client.DisposedException() ) );

                    return RequestResult.Done();
                }

                var writeResult = await client.WriteAsync( responseHeader, responseData, traceId ).ConfigureAwait( false );
                if ( writeResult.Exception is null )
                    return FinishRequestHandling( client, writerSource, traceId );

                await DisposeClientAsync( client, traceId ).ConfigureAwait( false );
                return RequestResult.Done();
            }
            finally
            {
                poolToken.Return( client, traceId );
            }
        }
    }

    private static async ValueTask<RequestResult> HandleUnbindPublisherRequestAsync(
        MessageBrokerRemoteClient client,
        IncomingPacketToken request,
        ulong traceId)
    {
        using ( MessageBrokerRemoteClientTraceEvent.CreateScope(
            client,
            traceId,
            MessageBrokerRemoteClientTraceEventType.UnbindPublisher ) )
        {
            var poolToken = request.PoolToken;
            try
            {
                var readPacket = client.Logger.ReadPacket;
                readPacket?.Emit( MessageBrokerRemoteClientReadPacketEvent.CreateReceived( client, traceId, request.Header ) );

                var exception = request.Header.AssertExactPayload( client, Protocol.UnbindPublisherRequest.Length );
                if ( exception is not null )
                    return await FinishInvalidRequestHandlingAsync( client, exception, traceId ).ConfigureAwait( false );

                var data = request.Data;
                var parsedRequest = Protocol.UnbindPublisherRequest.Parse( data );
                var requestErrors = parsedRequest.StringifyErrors();
                if ( requestErrors.Count > 0 )
                {
                    var error = client.ProtocolException( request.Header, requestErrors );
                    return await FinishInvalidRequestHandlingAsync( client, error, traceId ).ConfigureAwait( false );
                }

                if ( client.Logger.UnbindingPublisher is { } unbindingPublisher )
                    unbindingPublisher.Emit(
                        MessageBrokerRemoteClientUnbindingPublisherEvent.Create( client, traceId, parsedRequest.ChannelId ) );

                var disposingChannel = false;
                var disposingStream = false;
                ulong channelTraceId = 0;
                ulong streamTraceId = 0;
                MessageBrokerChannelPublisherBinding? publisher = null;
                MessageBrokerStream? stream = null;
                UnbindResult unbindResult;

                var channel = ChannelCollection.TryGetById( client.Server, parsedRequest.ChannelId );
                if ( channel is null )
                    unbindResult = UnbindResult.NotBound;
                else
                {
                    using ( client.AcquireActiveLock( traceId, out var exc ) )
                    {
                        if ( exc is not null )
                            return RequestResult.Done();

                        unbindResult = client.BeginUnbindPublisherUnsafe(
                            channel,
                            ref publisher,
                            ref channelTraceId,
                            ref stream,
                            ref streamTraceId,
                            ref disposingChannel,
                            ref disposingStream );
                    }
                }

                Protocol.PacketHeader responseHeader;
                Memory<byte> responseData;

                if ( unbindResult != UnbindResult.Success )
                {
                    if ( client.Logger.Error is { } error )
                    {
                        Exception exc = unbindResult switch
                        {
                            UnbindResult.NotBound => client.PublisherException(
                                publisher,
                                channel is null
                                    ? Resources.CannotUnbindPublisherFromNonExistingChannel( client, parsedRequest.ChannelId )
                                    : Resources.PublisherNotBound( client, channel ) ),
                            UnbindResult.ChannelDisposed => channel!.DisposedException(),
                            UnbindResult.ParentDisposed => stream!.DisposedException(),
                            _ => publisher!.DisposedException()
                        };

                        error.Emit( MessageBrokerRemoteClientErrorEvent.Create( client, traceId, exc ) );
                    }

                    var responseLength = Protocol.PacketHeader.Length + Protocol.UnbindPublisherFailureResponse.Payload;
                    var response = new Protocol.UnbindPublisherFailureResponse( unbindResult );
                    if ( data.Length < responseLength )
                        poolToken.IncreaseLength( responseLength, out data );

                    responseHeader = response.Header;
                    responseData = data.Slice( 0, responseLength );
                    response.Serialize( responseData );
                }
                else
                {
                    Assume.IsNotNull( channel );
                    Assume.IsNotNull( stream );
                    Assume.IsNotNull( publisher );
                    readPacket?.Emit( MessageBrokerRemoteClientReadPacketEvent.CreateAccepted( client, traceId, request.Header ) );

                    using ( MessageBrokerStreamTraceEvent.CreateScope(
                        stream,
                        streamTraceId,
                        MessageBrokerStreamTraceEventType.UnbindPublisher ) )
                    {
                        if ( stream.Logger.ClientTrace is { } clientTrace )
                            clientTrace.Emit( MessageBrokerStreamClientTraceEvent.Create( stream, streamTraceId, client, traceId ) );

                        if ( stream.Logger.PublisherUnbound is { } streamPublisherUnbound )
                            streamPublisherUnbound.Emit(
                                MessageBrokerStreamPublisherUnboundEvent.Create( publisher, streamTraceId, disposingChannel ) );

                        if ( disposingStream )
                            await stream.DisposeDueToLackOfReferencesAsync( ignoreProcessorTask: false, streamTraceId )
                                .ConfigureAwait( false );
                    }

                    using ( MessageBrokerChannelTraceEvent.CreateScope(
                        channel,
                        channelTraceId,
                        MessageBrokerChannelTraceEventType.UnbindPublisher ) )
                    {
                        if ( channel.Logger.ClientTrace is { } clientTrace )
                            clientTrace.Emit( MessageBrokerChannelClientTraceEvent.Create( channel, channelTraceId, client, traceId ) );

                        if ( channel.Logger.PublisherUnbound is { } channelPublisherUnbound )
                            channelPublisherUnbound.Emit(
                                MessageBrokerChannelPublisherUnboundEvent.Create( publisher, channelTraceId, disposingStream ) );

                        if ( disposingChannel )
                            channel.DisposeDueToLackOfReferences( channelTraceId );
                    }

                    publisher.EndDisposing();
                    if ( client.Logger.PublisherUnbound is { } publisherUnbound )
                        publisherUnbound.Emit(
                            MessageBrokerRemoteClientPublisherUnboundEvent.Create(
                                publisher,
                                traceId,
                                disposingChannel,
                                disposingStream ) );

                    var responseLength = Protocol.PacketHeader.Length + Protocol.PublisherUnboundResponse.Payload;
                    var response = new Protocol.PublisherUnboundResponse( disposingChannel, disposingStream );
                    if ( data.Length < responseLength )
                        poolToken.IncreaseLength( responseLength, out data );

                    responseHeader = response.Header;
                    responseData = data.Slice( 0, responseLength );
                    response.Serialize( responseData );
                }

                ManualResetValueTaskSource<bool> writerSource;
                using ( client.AcquireActiveLock( traceId, out var exc ) )
                {
                    if ( exc is not null )
                        return RequestResult.Done();

                    writerSource = client.WriterQueue.AcquireSource();
                }

                if ( ! await writerSource.GetTask().ConfigureAwait( false ) )
                {
                    if ( client.Logger.Error is { } error )
                        error.Emit( MessageBrokerRemoteClientErrorEvent.Create( client, traceId, client.DisposedException() ) );

                    return RequestResult.Done();
                }

                var writeResult = await client.WriteAsync( responseHeader, responseData, traceId ).ConfigureAwait( false );
                if ( writeResult.Exception is null )
                    return FinishRequestHandling( client, writerSource, traceId );

                await DisposeClientAsync( client, traceId ).ConfigureAwait( false );
                return RequestResult.Done();
            }
            finally
            {
                poolToken.Return( client, traceId );
            }
        }
    }

    private static async ValueTask<RequestResult> HandleBindListenerRequestAsync(
        MessageBrokerRemoteClient client,
        IncomingPacketToken request,
        ulong traceId)
    {
        using ( MessageBrokerRemoteClientTraceEvent.CreateScope( client, traceId, MessageBrokerRemoteClientTraceEventType.BindListener ) )
        {
            var poolToken = request.PoolToken;
            try
            {
                var readPacket = client.Logger.ReadPacket;
                readPacket?.Emit( MessageBrokerRemoteClientReadPacketEvent.CreateReceived( client, traceId, request.Header ) );

                var exception = request.Header.AssertMinPayload( client, Protocol.BindListenerRequestHeader.Length );
                if ( exception is not null )
                    return await FinishInvalidRequestHandlingAsync( client, exception, traceId ).ConfigureAwait( false );

                var data = request.Data;
                var parsedRequestHeader = Protocol.BindListenerRequestHeader.Parse( data );

                var requestErrors = parsedRequestHeader.StringifyErrors( data.Length );
                if ( requestErrors.Count > 0 )
                {
                    var error = client.ProtocolException( request.Header, requestErrors );
                    return await FinishInvalidRequestHandlingAsync( client, error, traceId ).ConfigureAwait( false );
                }

                var channelName = TextEncoding.Parse(
                    data.Slice( Protocol.BindListenerRequestHeader.Length, parsedRequestHeader.ChannelNameLength ) );

                if ( channelName.Exception is not null )
                    return await FinishInvalidRequestHandlingAsync( client, channelName.Exception, traceId ).ConfigureAwait( false );

                Assume.IsNotNull( channelName.Value );
                if ( ! Defaults.NameLengthBounds.Contains( channelName.Value.Length ) )
                {
                    var error = client.ProtocolException( request.Header, Resources.InvalidChannelNameLength( channelName.Value.Length ) );
                    return await FinishInvalidRequestHandlingAsync( client, error, traceId ).ConfigureAwait( false );
                }

                var queueName = TextEncoding.Parse(
                    data.Slice(
                        Protocol.BindListenerRequestHeader.Length + parsedRequestHeader.ChannelNameLength,
                        parsedRequestHeader.QueueNameLength ) );

                if ( queueName.Exception is not null )
                    return await FinishInvalidRequestHandlingAsync( client, queueName.Exception, traceId ).ConfigureAwait( false );

                Assume.IsNotNull( queueName.Value );
                if ( queueName.Value.Length > Defaults.NameLengthBounds.Max )
                {
                    var error = client.ProtocolException( request.Header, Resources.InvalidQueueNameLength( queueName.Value.Length ) );
                    return await FinishInvalidRequestHandlingAsync( client, error, traceId ).ConfigureAwait( false );
                }

                var filterExpression = TextEncoding.Parse(
                    data.Slice(
                        Protocol.BindListenerRequestHeader.Length
                        + parsedRequestHeader.ChannelNameLength
                        + parsedRequestHeader.QueueNameLength ) );

                if ( filterExpression.Exception is not null )
                    return await FinishInvalidRequestHandlingAsync( client, filterExpression.Exception, traceId ).ConfigureAwait( false );

                Assume.IsNotNull( filterExpression.Value );
                var rawFilterExpression = filterExpression.Value.Length > 0 ? filterExpression.Value : null;

                if ( client.Logger.BindingListener is { } bindingListener )
                    bindingListener.Emit(
                        MessageBrokerRemoteClientBindingListenerEvent.Create(
                            client,
                            traceId,
                            channelName.Value,
                            queueName.Value,
                            rawFilterExpression,
                            parsedRequestHeader.PrefetchHint,
                            parsedRequestHeader.MaxRetries,
                            parsedRequestHeader.RetryDelay,
                            parsedRequestHeader.MaxRedeliveries,
                            parsedRequestHeader.MinAckTimeout,
                            parsedRequestHeader.DeadLetterCapacityHint,
                            parsedRequestHeader.MinDeadLetterRetention,
                            parsedRequestHeader.CreateChannelIfNotExists ) );

                if ( queueName.Value.Length == 0 )
                    queueName = channelName;

                var bindResult = BindResult.Success;
                IParsedExpressionDelegate<MessageBrokerFilterExpressionContext, bool>? filterExpressionDelegate = null;
                if ( rawFilterExpression is not null )
                {
                    if ( client.Server.ExpressionFactory is null )
                        bindResult = BindResult.UnexpectedFilterExpression;
                    else
                    {
                        try
                        {
                            var expression = client.Server.ExpressionFactory.Create<MessageBrokerFilterExpressionContext, bool>(
                                rawFilterExpression );

                            if ( expression.UnboundArguments.Count > 1 )
                            {
                                if ( client.Logger.Error is { } error )
                                {
                                    var exc = client.Exception(
                                        Resources.InvalidFilterExpressionArgumentCount(
                                            rawFilterExpression,
                                            expression.UnboundArguments ) );

                                    error.Emit( MessageBrokerRemoteClientErrorEvent.Create( client, traceId, exc ) );
                                }

                                bindResult = BindResult.InvalidFilterExpression;
                            }
                            else
                                filterExpressionDelegate = expression.Compile();
                        }
                        catch ( Exception exc )
                        {
                            if ( client.Logger.Error is { } error )
                                error.Emit( MessageBrokerRemoteClientErrorEvent.Create( client, traceId, exc ) );

                            bindResult = BindResult.InvalidFilterExpression;
                        }
                    }
                }

                var channelCreated = false;
                var queueCreated = false;
                ulong channelTraceId = 0;
                ulong queueTraceId = 0;
                MessageBrokerChannel? channel = null;
                MessageBrokerQueue? queue = null;
                MessageBrokerChannelListenerBinding? listener = null;
                ManualResetValueTaskSource<bool> writerSource;

                using ( AcquireActiveServerLock( client, traceId, out var serverExc ) )
                {
                    if ( serverExc is not null )
                        return RequestResult.Done();

                    using ( client.AcquireActiveLock( traceId, out var exc ) )
                    {
                        if ( exc is not null )
                            return RequestResult.Done();

                        if ( bindResult == BindResult.Success )
                        {
                            channel = ChannelCollection.TryRegisterUnsafe(
                                client.Server,
                                channelName.Value,
                                parsedRequestHeader.CreateChannelIfNotExists,
                                ref channelCreated );

                            if ( channel is null )
                                bindResult = BindResult.ChannelDoesNotExist;
                            else
                            {
                                try
                                {
                                    bindResult = client.BindListenerUnsafe(
                                        channel,
                                        channelCreated,
                                        queueName.Value,
                                        rawFilterExpression,
                                        filterExpressionDelegate,
                                        in parsedRequestHeader,
                                        ref listener,
                                        ref channelTraceId,
                                        ref queue,
                                        ref queueTraceId,
                                        ref queueCreated );
                                }
                                catch
                                {
                                    if ( channelCreated )
                                        ChannelCollection.RemoveUnsafe( channel );

                                    throw;
                                }
                            }
                        }

                        writerSource = client.WriterQueue.AcquireSource();
                    }
                }

                Protocol.PacketHeader responseHeader;
                Memory<byte> responseData;

                if ( bindResult != BindResult.Success )
                {
                    if ( client.Logger.Error is { } error )
                    {
                        Exception exc = bindResult switch
                        {
                            BindResult.AlreadyBound => client.ListenerException( listener, Resources.ListenerAlreadyBound( listener! ) ),
                            BindResult.ChannelDisposed => channel!.DisposedException(),
                            BindResult.ParentDisposed => queue!.DisposedException(),
                            BindResult.ChannelDoesNotExist => client.ListenerException(
                                null,
                                Resources.CannotBindAsListenerToNonExistingChannel( client, channelName.Value ) ),
                            BindResult.UnexpectedFilterExpression => client.Exception(
                                Resources.UnexpectedFilterExpression( rawFilterExpression! ) ),
                            _ => client.Exception( Resources.InvalidFilterExpression( rawFilterExpression! ) )
                        };

                        error.Emit( MessageBrokerRemoteClientErrorEvent.Create( client, traceId, exc ) );
                    }

                    var responseLength = Protocol.PacketHeader.Length + Protocol.BindListenerFailureResponse.Payload;
                    var response = new Protocol.BindListenerFailureResponse( bindResult );
                    Assume.IsGreaterThanOrEqualTo( data.Length, responseLength );

                    responseHeader = response.Header;
                    responseData = data.Slice( 0, responseLength );
                    response.Serialize( responseData );
                }
                else
                {
                    Assume.IsNotNull( channel );
                    Assume.IsNotNull( queue );
                    Assume.IsNotNull( listener );
                    readPacket?.Emit( MessageBrokerRemoteClientReadPacketEvent.CreateAccepted( client, traceId, request.Header ) );

                    using ( MessageBrokerChannelTraceEvent.CreateScope(
                        channel,
                        channelTraceId,
                        MessageBrokerChannelTraceEventType.BindListener ) )
                    {
                        if ( channel.Logger.ClientTrace is { } clientTrace )
                            clientTrace.Emit( MessageBrokerChannelClientTraceEvent.Create( channel, channelTraceId, client, traceId ) );

                        if ( channelCreated && channel.Logger.Created is { } created )
                            created.Emit( MessageBrokerChannelCreatedEvent.Create( channel, channelTraceId ) );

                        if ( channel.Logger.ListenerBound is { } channelListenerBound )
                            channelListenerBound.Emit(
                                MessageBrokerChannelListenerBoundEvent.Create( listener, channelTraceId, queueCreated ) );
                    }

                    using ( MessageBrokerQueueTraceEvent.CreateScope( queue, queueTraceId, MessageBrokerQueueTraceEventType.BindListener ) )
                    {
                        if ( queue.Logger.ClientTrace is { } clientTrace )
                            clientTrace.Emit( MessageBrokerQueueClientTraceEvent.Create( queue, queueTraceId, traceId ) );

                        if ( queueCreated && queue.Logger.Created is { } created )
                            created.Emit( MessageBrokerQueueCreatedEvent.Create( queue, queueTraceId ) );

                        if ( queue.Logger.ListenerBound is { } queueListenerBound )
                            queueListenerBound.Emit(
                                MessageBrokerQueueListenerBoundEvent.Create( listener, queueTraceId, channelCreated ) );
                    }

                    if ( client.Logger.ListenerBound is { } listenerBound )
                        listenerBound.Emit(
                            MessageBrokerRemoteClientListenerBoundEvent.Create( listener, traceId, channelCreated, queueCreated ) );

                    var responseLength = Protocol.PacketHeader.Length + Protocol.ListenerBoundResponse.Payload;
                    var response = new Protocol.ListenerBoundResponse( channelCreated, queueCreated, channel.Id, queue.Id );
                    if ( data.Length < responseLength )
                        poolToken.IncreaseLength( responseLength, out data );

                    responseHeader = response.Header;
                    responseData = data.Slice( 0, responseLength );
                    response.Serialize( responseData );
                }

                if ( ! await writerSource.GetTask().ConfigureAwait( false ) )
                {
                    if ( client.Logger.Error is { } error )
                        error.Emit( MessageBrokerRemoteClientErrorEvent.Create( client, traceId, client.DisposedException() ) );

                    return RequestResult.Done();
                }

                var writeResult = await client.WriteAsync( responseHeader, responseData, traceId ).ConfigureAwait( false );
                if ( writeResult.Exception is null )
                    return FinishRequestHandling( client, writerSource, traceId );

                await DisposeClientAsync( client, traceId ).ConfigureAwait( false );
                return RequestResult.Done();
            }
            finally
            {
                poolToken.Return( client, traceId );
            }
        }
    }

    private static async ValueTask<RequestResult> HandleUnbindListenerRequestAsync(
        MessageBrokerRemoteClient client,
        IncomingPacketToken request,
        ulong traceId)
    {
        using ( MessageBrokerRemoteClientTraceEvent.CreateScope( client, traceId, MessageBrokerRemoteClientTraceEventType.UnbindListener ) )
        {
            var poolToken = request.PoolToken;
            try
            {
                var readPacket = client.Logger.ReadPacket;
                readPacket?.Emit( MessageBrokerRemoteClientReadPacketEvent.CreateReceived( client, traceId, request.Header ) );

                var exception = request.Header.AssertExactPayload( client, Protocol.UnbindListenerRequest.Length );
                if ( exception is not null )
                    return await FinishInvalidRequestHandlingAsync( client, exception, traceId ).ConfigureAwait( false );

                var data = request.Data;
                var parsedRequest = Protocol.UnbindListenerRequest.Parse( data );
                var requestErrors = parsedRequest.StringifyErrors();
                if ( requestErrors.Count > 0 )
                {
                    var error = client.ProtocolException( request.Header, requestErrors );
                    return await FinishInvalidRequestHandlingAsync( client, error, traceId ).ConfigureAwait( false );
                }

                if ( client.Logger.UnbindingListener is { } unbindingListener )
                    unbindingListener.Emit(
                        MessageBrokerRemoteClientUnbindingListenerEvent.Create( client, traceId, parsedRequest.ChannelId ) );

                var disposingChannel = false;
                var disposingQueue = false;
                ulong channelTraceId = 0;
                ulong queueTraceId = 0;
                MessageBrokerChannelListenerBinding? listener = null;
                MessageBrokerQueue? queue = null;
                UnbindResult unbindResult;

                var channel = ChannelCollection.TryGetById( client.Server, parsedRequest.ChannelId );
                if ( channel is null )
                    unbindResult = UnbindResult.NotBound;
                else
                {
                    using ( client.AcquireActiveLock( traceId, out var exc ) )
                    {
                        if ( exc is not null )
                            return RequestResult.Done();

                        unbindResult = client.BeginUnbindListenerUnsafe(
                            channel,
                            ref listener,
                            ref channelTraceId,
                            ref queue,
                            ref queueTraceId,
                            ref disposingChannel,
                            ref disposingQueue );
                    }
                }

                Protocol.PacketHeader responseHeader;
                Memory<byte> responseData;

                if ( unbindResult != UnbindResult.Success )
                {
                    if ( client.Logger.Error is { } error )
                    {
                        Exception exc = unbindResult switch
                        {
                            UnbindResult.NotBound => client.ListenerException(
                                listener,
                                channel is null
                                    ? Resources.CannotUnbindListenerFromNonExistingChannel( client, parsedRequest.ChannelId )
                                    : Resources.ListenerNotBound( client, channel ) ),
                            UnbindResult.ChannelDisposed => channel!.DisposedException(),
                            UnbindResult.ParentDisposed => queue!.DisposedException(),
                            _ => listener!.DisposedException()
                        };

                        error.Emit( MessageBrokerRemoteClientErrorEvent.Create( client, traceId, exc ) );
                    }

                    var responseLength = Protocol.PacketHeader.Length + Protocol.UnbindListenerFailureResponse.Payload;
                    var response = new Protocol.UnbindListenerFailureResponse( unbindResult );
                    if ( data.Length < responseLength )
                        poolToken.IncreaseLength( responseLength, out data );

                    responseHeader = response.Header;
                    responseData = data.Slice( 0, responseLength );
                    response.Serialize( responseData );
                }
                else
                {
                    Assume.IsNotNull( channel );
                    Assume.IsNotNull( queue );
                    Assume.IsNotNull( listener );
                    readPacket?.Emit( MessageBrokerRemoteClientReadPacketEvent.CreateAccepted( client, traceId, request.Header ) );

                    using ( MessageBrokerQueueTraceEvent.CreateScope(
                        queue,
                        queueTraceId,
                        MessageBrokerQueueTraceEventType.UnbindListener ) )
                    {
                        if ( queue.Logger.ClientTrace is { } clientTrace )
                            clientTrace.Emit( MessageBrokerQueueClientTraceEvent.Create( queue, queueTraceId, traceId ) );

                        if ( queue.Logger.ListenerUnbound is { } queueListenerUnbound )
                            queueListenerUnbound.Emit(
                                MessageBrokerQueueListenerUnboundEvent.Create( listener, queueTraceId, disposingChannel ) );

                        if ( disposingQueue )
                            await queue.DisposeDueToLackOfReferencesAsync( ignoreProcessorTask: false, queueTraceId )
                                .ConfigureAwait( false );
                    }

                    using ( MessageBrokerChannelTraceEvent.CreateScope(
                        channel,
                        channelTraceId,
                        MessageBrokerChannelTraceEventType.UnbindListener ) )
                    {
                        if ( channel.Logger.ClientTrace is { } clientTrace )
                            clientTrace.Emit( MessageBrokerChannelClientTraceEvent.Create( channel, channelTraceId, client, traceId ) );

                        if ( channel.Logger.ListenerUnbound is { } channelListenerUnbound )
                            channelListenerUnbound.Emit(
                                MessageBrokerChannelListenerUnboundEvent.Create( listener, channelTraceId, disposingQueue ) );

                        if ( disposingChannel )
                            channel.DisposeDueToLackOfReferences( channelTraceId );
                    }

                    listener.EndDisposing();
                    if ( client.Logger.ListenerUnbound is { } listenerUnbound )
                        listenerUnbound.Emit(
                            MessageBrokerRemoteClientListenerUnboundEvent.Create( listener, traceId, disposingChannel, disposingQueue ) );

                    var responseLength = Protocol.PacketHeader.Length + Protocol.ListenerUnboundResponse.Payload;
                    var response = new Protocol.ListenerUnboundResponse( disposingChannel, disposingQueue );
                    if ( data.Length < responseLength )
                        poolToken.IncreaseLength( responseLength, out data );

                    responseHeader = response.Header;
                    responseData = data.Slice( 0, responseLength );
                    response.Serialize( responseData );
                }

                ManualResetValueTaskSource<bool> writerSource;
                using ( client.AcquireActiveLock( traceId, out var exc ) )
                {
                    if ( exc is not null )
                        return RequestResult.Done();

                    writerSource = client.WriterQueue.AcquireSource();
                }

                if ( ! await writerSource.GetTask().ConfigureAwait( false ) )
                {
                    if ( client.Logger.Error is { } error )
                        error.Emit( MessageBrokerRemoteClientErrorEvent.Create( client, traceId, client.DisposedException() ) );

                    return RequestResult.Done();
                }

                var writeResult = await client.WriteAsync( responseHeader, responseData, traceId ).ConfigureAwait( false );
                if ( writeResult.Exception is null )
                    return FinishRequestHandling( client, writerSource, traceId );

                await DisposeClientAsync( client, traceId ).ConfigureAwait( false );
                return RequestResult.Done();
            }
            finally
            {
                poolToken.Return( client, traceId );
            }
        }
    }

    private static async ValueTask<RequestResult> HandlePushMessageRoutingAsync(
        MessageBrokerRemoteClient client,
        IncomingPacketToken request,
        ulong traceId)
    {
        using ( MessageBrokerRemoteClientTraceEvent.CreateScope(
            client,
            traceId,
            MessageBrokerRemoteClientTraceEventType.PushMessageRouting ) )
        {
            var readPacket = client.Logger.ReadPacket;
            MessageRouting.Result routingResult;
            Protocol.PushMessageRoutingHeader parsedRequest;
            try
            {
                readPacket?.Emit( MessageBrokerRemoteClientReadPacketEvent.CreateReceived( client, traceId, request.Header ) );

                var exception = request.Header.AssertMinPayload( client, Protocol.PushMessageRoutingHeader.Length );
                if ( exception is not null )
                    return await FinishInvalidRequestHandlingAsync( client, exception, traceId ).ConfigureAwait( false );

                parsedRequest = Protocol.PushMessageRoutingHeader.Parse( request.Data );
                var requestErrors = parsedRequest.StringifyErrors();
                if ( requestErrors.Count > 0 )
                {
                    var error = client.ProtocolException( request.Header, requestErrors );
                    return await FinishInvalidRequestHandlingAsync( client, error, traceId ).ConfigureAwait( false );
                }

                bool hasEnqueuedRouting;
                using ( client.AcquireLock() )
                    hasEnqueuedRouting = client.MessageRouting.IsActive;

                if ( hasEnqueuedRouting )
                {
                    var error = client.ProtocolException( request.Header, Resources.MessageRoutingIsAlreadyEnqueued );
                    return await FinishInvalidRequestHandlingAsync( client, error, traceId ).ConfigureAwait( false );
                }

                if ( client.Logger.EnqueueingRouting is { } enqueueingRouting )
                    enqueueingRouting.Emit(
                        MessageBrokerRemoteClientEnqueueingRoutingEvent.Create( client, traceId, parsedRequest.TargetCount ) );

                routingResult = client.GetMessageRouting(
                    traceId,
                    request.Header,
                    parsedRequest.TargetCount,
                    request.Data.Slice( Protocol.PushMessageRoutingHeader.Length ).Span );
            }
            finally
            {
                request.PoolToken.Return( client, traceId );
            }

            if ( ! routingResult.IsValid )
            {
                routingResult.Value.PoolToken.Return( client, traceId );
                await DisposeClientAsync( client, traceId ).ConfigureAwait( false );
                return RequestResult.Done();
            }

            readPacket?.Emit( MessageBrokerRemoteClientReadPacketEvent.CreateAccepted( client, traceId, request.Header ) );

            using ( client.AcquireLock() )
            {
                if ( ! client.ShouldCancel )
                    client.MessageRouting = routingResult.Value;
            }

            if ( client.Logger.RoutingEnqueued is { } routingEnqueued )
                routingEnqueued.Emit(
                    MessageBrokerRemoteClientRoutingEnqueuedEvent.Create(
                        client,
                        traceId,
                        parsedRequest.TargetCount,
                        routingResult.FoundCount ) );

            return FinishRequestHandling( client, traceId );
        }
    }

    private static async ValueTask<RequestResult> HandlePushMessageAsync(
        MessageBrokerRemoteClient client,
        IncomingPacketToken request,
        ulong traceId)
    {
        using ( MessageBrokerRemoteClientTraceEvent.CreateScope( client, traceId, MessageBrokerRemoteClientTraceEventType.PushMessage ) )
        {
            var disposeBuffer = true;
            var storeKey = 0;
            ulong messageId = 0;
            ulong streamTraceId = 0;
            MessageBrokerChannelPublisherBinding? publisher;
            var pushMessageResult = PushMessageResult.NotBound;
            Protocol.PushMessageHeader parsedRequest;
            var messageRouting = MessageRouting.Empty;
            var messageData = Memory<byte>.Empty;

            try
            {
                if ( client.Logger.ReadPacket is { } readPacket )
                    readPacket.Emit( MessageBrokerRemoteClientReadPacketEvent.CreateReceived( client, traceId, request.Header ) );

                var exception = request.Header.AssertMinPayload( client, Protocol.PushMessageHeader.Length );
                if ( exception is not null )
                    return await FinishInvalidRequestHandlingAsync( client, exception, traceId ).ConfigureAwait( false );

                parsedRequest = Protocol.PushMessageHeader.Parse( request.Data );
                var requestErrors = parsedRequest.StringifyErrors();
                if ( requestErrors.Count > 0 )
                {
                    var error = client.ProtocolException( request.Header, requestErrors );
                    return await FinishInvalidRequestHandlingAsync( client, error, traceId ).ConfigureAwait( false );
                }

                var messageToken = MemoryPoolToken<byte>.Empty;
                var messageLength = request.Data.Length - Protocol.PushMessageHeader.Length;
                if ( messageLength > 0 )
                {
                    messageToken = request.PoolToken;
                    messageToken.DecreaseLengthAtStart( messageLength );
                    messageData = request.Data.Slice( Protocol.PushMessageHeader.Length );
                }

                if ( client.Logger.PushingMessage is { } pushingMessage )
                    pushingMessage.Emit(
                        MessageBrokerRemoteClientPushingMessageEvent.Create(
                            client,
                            traceId,
                            messageData.Length,
                            parsedRequest.ChannelId,
                            parsedRequest.Confirm ) );

                using ( client.AcquireActiveLock( traceId, out var exc ) )
                {
                    if ( exc is not null )
                        return RequestResult.Done();

                    messageRouting = client.MessageRouting;
                    client.MessageRouting = MessageRouting.Empty;
                    if ( client.PublishersByChannelId.TryGet( parsedRequest.ChannelId, out publisher ) )
                    {
                        pushMessageResult = publisher.Stream.PushMessage(
                            publisher,
                            messageToken,
                            messageData,
                            in messageRouting,
                            ref messageId,
                            ref storeKey,
                            ref streamTraceId );

                        if ( pushMessageResult == PushMessageResult.Success && messageData.Length > 0 )
                            disposeBuffer = false;
                    }
                }
            }
            finally
            {
                if ( disposeBuffer )
                    request.PoolToken.Return( client, traceId );

                if ( pushMessageResult != PushMessageResult.Success )
                    messageRouting.PoolToken.Return( client, traceId );
            }

            var poolToken = MemoryPoolToken<byte>.Empty;
            try
            {
                Protocol.PacketHeader responseHeader;
                Memory<byte> responseData;

                if ( pushMessageResult != PushMessageResult.Success )
                {
                    if ( client.Logger.Error is { } error )
                    {
                        Exception exc = pushMessageResult switch
                        {
                            PushMessageResult.NotBound => client.PublisherException(
                                null,
                                Resources.PublisherNotBound( client, parsedRequest.ChannelId ) ),
                            PushMessageResult.StreamDisposed => publisher!.Stream.DisposedException(),
                            _ => publisher!.DisposedException()
                        };

                        error.Emit( MessageBrokerRemoteClientErrorEvent.Create( client, traceId, exc ) );
                    }

                    if ( ! parsedRequest.Confirm )
                        return FinishRequestHandling( client, traceId );

                    var responseLength = Protocol.PacketHeader.Length + Protocol.MessageRejectedResponse.Payload;
                    var response = new Protocol.MessageRejectedResponse( pushMessageResult );
                    poolToken = client.MemoryPool.Rent( responseLength, client.ClearBuffers, out responseData );
                    responseHeader = response.Header;
                    response.Serialize( responseData );
                }
                else
                {
                    Assume.IsNotNull( publisher );
                    if ( client.Logger.ReadPacket is { } readPacket )
                        readPacket.Emit( MessageBrokerRemoteClientReadPacketEvent.CreateAccepted( client, traceId, request.Header ) );

                    using ( MessageBrokerStreamTraceEvent.CreateScope(
                        publisher.Stream,
                        streamTraceId,
                        MessageBrokerStreamTraceEventType.PushMessage ) )
                    {
                        if ( publisher.Stream.Logger.ClientTrace is { } clientTrace )
                            clientTrace.Emit(
                                MessageBrokerStreamClientTraceEvent.Create( publisher.Stream, streamTraceId, client, traceId ) );

                        if ( publisher.Stream.Logger.MessagePushed is { } streamMessagePushed )
                            streamMessagePushed.Emit(
                                MessageBrokerStreamMessagePushedEvent.Create(
                                    publisher,
                                    streamTraceId,
                                    messageId,
                                    storeKey,
                                    messageData.Length ) );
                    }

                    if ( client.Logger.MessagePushed is { } messagePushed )
                        messagePushed.Emit(
                            MessageBrokerRemoteClientMessagePushedEvent.Create(
                                publisher,
                                traceId,
                                messageId,
                                messageRouting.IsActive ? messageRouting.TraceId : null ) );

                    if ( ! parsedRequest.Confirm )
                        return FinishRequestHandling( client, traceId );

                    var responseLength = Protocol.PacketHeader.Length + Protocol.MessageAcceptedResponse.Payload;
                    var response = new Protocol.MessageAcceptedResponse( messageId );
                    poolToken = client.MemoryPool.Rent( responseLength, client.ClearBuffers, out responseData );
                    responseHeader = response.Header;
                    response.Serialize( responseData );
                }

                ManualResetValueTaskSource<bool> writerSource;
                using ( client.AcquireActiveLock( traceId, out var exc ) )
                {
                    if ( exc is not null )
                        return RequestResult.Done();

                    writerSource = client.WriterQueue.AcquireSource();
                }

                if ( ! await writerSource.GetTask().ConfigureAwait( false ) )
                {
                    if ( client.Logger.Error is { } error )
                        error.Emit( MessageBrokerRemoteClientErrorEvent.Create( client, traceId, client.DisposedException() ) );

                    return RequestResult.Done();
                }

                var writeResult = await client.WriteAsync( responseHeader, responseData, traceId ).ConfigureAwait( false );
                if ( writeResult.Exception is null )
                    return FinishRequestHandling( client, writerSource, traceId );

                await DisposeClientAsync( client, traceId ).ConfigureAwait( false );
                return RequestResult.Done();
            }
            finally
            {
                poolToken.Return( client, traceId );
            }
        }
    }

    private static async ValueTask<RequestResult> HandleDeadLetterQueryAsync(
        MessageBrokerRemoteClient client,
        IncomingPacketToken request,
        ulong traceId)
    {
        using ( MessageBrokerRemoteClientTraceEvent.CreateScope(
            client,
            traceId,
            MessageBrokerRemoteClientTraceEventType.DeadLetterQuery ) )
        {
            var poolToken = request.PoolToken;
            try
            {
                var readPacket = client.Logger.ReadPacket;
                readPacket?.Emit( MessageBrokerRemoteClientReadPacketEvent.CreateReceived( client, traceId, request.Header ) );

                var exception = request.Header.AssertExactPayload( client, Protocol.DeadLetterQuery.Length );
                if ( exception is not null )
                    return await FinishInvalidRequestHandlingAsync( client, exception, traceId ).ConfigureAwait( false );

                var data = request.Data;
                var parsedRequest = Protocol.DeadLetterQuery.Parse( data );
                var requestErrors = parsedRequest.StringifyErrors();
                if ( requestErrors.Count > 0 )
                {
                    var error = client.ProtocolException( request.Header, requestErrors );
                    return await FinishInvalidRequestHandlingAsync( client, error, traceId ).ConfigureAwait( false );
                }

                if ( client.Logger.QueryingDeadLetter is { } queryingDeadLetter )
                    queryingDeadLetter.Emit(
                        MessageBrokerRemoteClientQueryingDeadLetterEvent.Create(
                            client,
                            traceId,
                            parsedRequest.QueueId,
                            parsedRequest.ReadCount ) );

                MessageBrokerQueue? queue;
                var totalCount = -1;
                var maxReadCount = 0;
                var nextExpirationAt = Timestamp.Zero;
                DeadLetterQueryResult result;

                using ( client.AcquireActiveLock( traceId, out var exc ) )
                {
                    if ( exc is not null )
                        return RequestResult.Done();

                    queue = client.QueueStore.TryGetById( parsedRequest.QueueId );
                    result = queue is not null
                        ? queue.HandleDeadLetterQuery(
                            parsedRequest.ReadCount,
                            ref totalCount,
                            ref maxReadCount,
                            ref nextExpirationAt )
                        : DeadLetterQueryResult.QueueNotFound;
                }

                if ( result != DeadLetterQueryResult.Success )
                {
                    if ( client.Logger.Error is { } error )
                    {
                        Exception exc = result switch
                        {
                            DeadLetterQueryResult.QueueNotFound => client.Exception(
                                Resources.QueueForDeadLetterQueryNotFound( client, parsedRequest.QueueId ) ),
                            _ => queue!.DisposedException()
                        };

                        error.Emit( MessageBrokerRemoteClientErrorEvent.Create( client, traceId, exc ) );
                    }
                }
                else
                {
                    Assume.IsNotNull( queue );
                    readPacket?.Emit( MessageBrokerRemoteClientReadPacketEvent.CreateAccepted( client, traceId, request.Header ) );
                    if ( client.Logger.DeadLetterQueried is { } deadLetterQueried )
                        deadLetterQueried.Emit(
                            MessageBrokerRemoteClientDeadLetterQueriedEvent.Create(
                                queue,
                                traceId,
                                totalCount,
                                maxReadCount,
                                nextExpirationAt ) );
                }

                poolToken.IncreaseLength( Protocol.PacketHeader.Length + Protocol.DeadLetterQueryResponse.Payload, out data );
                var response = new Protocol.DeadLetterQueryResponse( totalCount, maxReadCount, nextExpirationAt );
                response.Serialize( data );

                ManualResetValueTaskSource<bool> writerSource;
                using ( client.AcquireActiveLock( traceId, out var exc ) )
                {
                    if ( exc is not null )
                        return RequestResult.Done();

                    writerSource = client.WriterQueue.AcquireSource();
                }

                if ( ! await writerSource.GetTask().ConfigureAwait( false ) )
                {
                    if ( client.Logger.Error is { } error )
                        error.Emit( MessageBrokerRemoteClientErrorEvent.Create( client, traceId, client.DisposedException() ) );

                    return RequestResult.Done();
                }

                var writeResult = await client.WriteAsync( response.Header, data, traceId ).ConfigureAwait( false );
                if ( writeResult.Exception is null )
                    return FinishRequestHandling( client, writerSource, traceId );

                await DisposeClientAsync( client, traceId ).ConfigureAwait( false );
                return RequestResult.Done();
            }
            finally
            {
                poolToken.Return( client, traceId );
            }
        }
    }

    private static async ValueTask<RequestResult> HandleMessageNotificationAckAsync(
        MessageBrokerRemoteClient client,
        IncomingPacketToken request,
        ulong traceId)
    {
        using ( MessageBrokerRemoteClientTraceEvent.CreateScope( client, traceId, MessageBrokerRemoteClientTraceEventType.Ack ) )
        {
            Protocol.MessageNotificationAck parsedRequest;
            MessageBrokerQueue? queue;
            QueueMessage message = default;
            var disposing = false;
            var queueTraceId = 0UL;
            AckResult ackResult;

            try
            {
                if ( client.Logger.ReadPacket is { } readPacket )
                    readPacket.Emit( MessageBrokerRemoteClientReadPacketEvent.CreateReceived( client, traceId, request.Header ) );

                var exception = request.Header.AssertExactPayload( client, Protocol.MessageNotificationAck.Length );
                if ( exception is not null )
                    return await FinishInvalidRequestHandlingAsync( client, exception, traceId ).ConfigureAwait( false );

                parsedRequest = Protocol.MessageNotificationAck.Parse( request.Data );
                var requestErrors = parsedRequest.StringifyErrors();
                if ( requestErrors.Count > 0 )
                {
                    var error = client.ProtocolException( request.Header, requestErrors );
                    return await FinishInvalidRequestHandlingAsync( client, error, traceId ).ConfigureAwait( false );
                }

                if ( client.Logger.ProcessingAck is { } processingAck )
                    processingAck.Emit(
                        MessageBrokerRemoteClientProcessingAckEvent.Create(
                            client,
                            traceId,
                            parsedRequest.QueueId,
                            parsedRequest.AckId,
                            parsedRequest.StreamId,
                            parsedRequest.MessageId,
                            parsedRequest.Retry,
                            parsedRequest.Redelivery ) );

                using ( client.AcquireActiveLock( traceId, out var exc ) )
                {
                    if ( exc is not null )
                        return RequestResult.Done();

                    queue = client.QueueStore.TryGetById( parsedRequest.QueueId );
                    if ( queue is not null )
                        ackResult = queue.HandleAck(
                            parsedRequest.AckId,
                            parsedRequest.StreamId,
                            parsedRequest.MessageId,
                            parsedRequest.Retry,
                            parsedRequest.Redelivery,
                            ref message,
                            ref queueTraceId,
                            ref disposing );
                    else
                        ackResult = AckResult.QueueNotFound;
                }
            }
            finally
            {
                request.PoolToken.Return( client, traceId );
            }

            if ( ackResult != AckResult.Success )
            {
                if ( client.Logger.Error is { } error )
                {
                    Exception exc = ackResult switch
                    {
                        AckResult.MessageNotFound => queue!.Exception(
                            Resources.MessageNotFound( queue!, parsedRequest.AckId, parsedRequest.StreamId, parsedRequest.MessageId ) ),
                        AckResult.MessageVersionNotFound => queue!.Exception(
                            Resources.MessageVersionNotFound(
                                queue!,
                                parsedRequest.StreamId,
                                parsedRequest.MessageId,
                                parsedRequest.Retry,
                                parsedRequest.Redelivery ) ),
                        AckResult.QueueNotFound => client.Exception( Resources.QueueForAckNotFound( client, parsedRequest.QueueId ) ),
                        _ => queue!.DisposedException()
                    };

                    error.Emit( MessageBrokerRemoteClientErrorEvent.Create( client, traceId, exc ) );
                }
            }
            else
            {
                Assume.IsNotNull( queue );
                Assume.IsNotNull( message.Publisher );
                Assume.IsNotNull( message.Listener );
                if ( client.Logger.ReadPacket is { } readPacket )
                    readPacket.Emit( MessageBrokerRemoteClientReadPacketEvent.CreateAccepted( client, traceId, request.Header ) );

                using ( MessageBrokerQueueTraceEvent.CreateScope( queue, queueTraceId, MessageBrokerQueueTraceEventType.Ack ) )
                {
                    if ( queue.Logger.ClientTrace is { } clientTrace )
                        clientTrace.Emit( MessageBrokerQueueClientTraceEvent.Create( queue, queueTraceId, traceId ) );

                    var messageRemoved = queue.RemoveFromStreamMessageStore( message, queueTraceId );
                    if ( queue.Logger.AckProcessed is { } queueAckProcessed )
                        queueAckProcessed.Emit(
                            MessageBrokerQueueAckProcessedEvent.Create( queue, queueTraceId, parsedRequest.AckId, messageRemoved ) );

                    if ( disposing )
                        await queue.DisposeDueToLackOfReferencesAsync( ignoreProcessorTask: false, queueTraceId ).ConfigureAwait( false );
                }

                if ( client.Logger.AckProcessed is { } ackProcessed )
                    ackProcessed.Emit(
                        MessageBrokerRemoteClientAckProcessedEvent.Create(
                            message.Listener,
                            traceId,
                            message.Publisher,
                            parsedRequest.AckId,
                            parsedRequest.MessageId,
                            parsedRequest.Retry,
                            parsedRequest.Redelivery,
                            isNack: false ) );
            }

            return FinishRequestHandling( client, traceId );
        }
    }

    private static async ValueTask<RequestResult> HandleMessageNotificationNackAsync(
        MessageBrokerRemoteClient client,
        IncomingPacketToken request,
        ulong traceId)
    {
        using ( MessageBrokerRemoteClientTraceEvent.CreateScope( client, traceId, MessageBrokerRemoteClientTraceEventType.NegativeAck ) )
        {
            Protocol.MessageNotificationNegativeAck parsedRequest;
            MessageBrokerQueue? queue;
            QueueMessage message = default;
            var disposing = false;
            var delay = Duration.FromTicks( -1 );
            var queueTraceId = 0UL;
            AckResult ackResult;

            try
            {
                if ( client.Logger.ReadPacket is { } readPacket )
                    readPacket.Emit( MessageBrokerRemoteClientReadPacketEvent.CreateReceived( client, traceId, request.Header ) );

                var exception = request.Header.AssertExactPayload( client, Protocol.MessageNotificationNegativeAck.Length );
                if ( exception is not null )
                    return await FinishInvalidRequestHandlingAsync( client, exception, traceId ).ConfigureAwait( false );

                parsedRequest = Protocol.MessageNotificationNegativeAck.Parse( request.Data );
                var requestErrors = parsedRequest.StringifyErrors();
                if ( requestErrors.Count > 0 )
                {
                    var error = client.ProtocolException( request.Header, requestErrors );
                    return await FinishInvalidRequestHandlingAsync( client, error, traceId ).ConfigureAwait( false );
                }

                var explicitDelay = parsedRequest.HasExplicitDelay ? parsedRequest.ExplicitDelay : ( Duration? )null;
                if ( client.Logger.ProcessingNegativeAck is { } processingNegativeAck )
                    processingNegativeAck.Emit(
                        MessageBrokerRemoteClientProcessingNegativeAckEvent.Create(
                            client,
                            traceId,
                            parsedRequest.QueueId,
                            parsedRequest.AckId,
                            parsedRequest.StreamId,
                            parsedRequest.MessageId,
                            parsedRequest.Retry,
                            parsedRequest.Redelivery,
                            parsedRequest.NoRetry,
                            parsedRequest.NoDeadLetter,
                            explicitDelay ) );

                using ( client.AcquireActiveLock( traceId, out var exc ) )
                {
                    if ( exc is not null )
                        return RequestResult.Done();

                    queue = client.QueueStore.TryGetById( parsedRequest.QueueId );
                    if ( queue is not null )
                        ackResult = queue.HandleNegativeAck(
                            parsedRequest.AckId,
                            parsedRequest.StreamId,
                            parsedRequest.MessageId,
                            parsedRequest.Retry,
                            parsedRequest.Redelivery,
                            parsedRequest.NoRetry,
                            parsedRequest.NoDeadLetter,
                            explicitDelay,
                            ref message,
                            ref delay,
                            ref queueTraceId,
                            ref disposing );
                    else
                        ackResult = AckResult.QueueNotFound;
                }
            }
            finally
            {
                request.PoolToken.Return( client, traceId );
            }

            if ( ackResult != AckResult.Success )
            {
                if ( client.Logger.Error is { } error )
                {
                    Exception exc = ackResult switch
                    {
                        AckResult.MessageNotFound => queue!.Exception(
                            Resources.MessageNotFound( queue!, parsedRequest.AckId, parsedRequest.StreamId, parsedRequest.MessageId ) ),
                        AckResult.MessageVersionNotFound => queue!.Exception(
                            Resources.MessageVersionNotFound(
                                queue!,
                                parsedRequest.StreamId,
                                parsedRequest.MessageId,
                                parsedRequest.Retry,
                                parsedRequest.Redelivery ) ),
                        AckResult.QueueNotFound => client.Exception( Resources.QueueForAckNotFound( client, parsedRequest.QueueId ) ),
                        _ => queue!.DisposedException()
                    };

                    error.Emit( MessageBrokerRemoteClientErrorEvent.Create( client, traceId, exc ) );
                }
            }
            else
            {
                Assume.IsNotNull( queue );
                Assume.IsNotNull( message.Listener );
                Assume.IsNotNull( message.Publisher );
                if ( client.Logger.ReadPacket is { } readPacket )
                    readPacket.Emit( MessageBrokerRemoteClientReadPacketEvent.CreateAccepted( client, traceId, request.Header ) );

                using ( MessageBrokerQueueTraceEvent.CreateScope( queue, queueTraceId, MessageBrokerQueueTraceEventType.NegativeAck ) )
                {
                    if ( queue.Logger.ClientTrace is { } clientTrace )
                        clientTrace.Emit( MessageBrokerQueueClientTraceEvent.Create( queue, queueTraceId, traceId ) );

                    var messageRemoved = false;
                    if ( delay < Duration.Zero )
                    {
                        var movedToDeadLetter = ! parsedRequest.NoDeadLetter && message.Listener.DeadLetterCapacityHint > 0;
                        messageRemoved = ! movedToDeadLetter && queue.RemoveFromStreamMessageStore( message, queueTraceId );
                        if ( queue.Logger.MessageDiscarded is { } messageDiscarded )
                            messageDiscarded.Emit(
                                MessageBrokerQueueMessageDiscardedEvent.Create(
                                    message.Listener,
                                    queueTraceId,
                                    message.Publisher,
                                    message.StoreKey,
                                    parsedRequest.Retry,
                                    parsedRequest.Redelivery,
                                    messageRemoved,
                                    movedToDeadLetter,
                                    parsedRequest.NoRetry
                                        ? MessageBrokerQueueDiscardMessageReason.ExplicitNoRetry
                                        : MessageBrokerQueueDiscardMessageReason.MaxRetriesReached ) );
                    }

                    if ( queue.Logger.NegativeAckProcessed is { } negativeAckProcessed )
                        negativeAckProcessed.Emit(
                            MessageBrokerQueueNegativeAckProcessedEvent.Create(
                                queue,
                                queueTraceId,
                                parsedRequest.AckId,
                                delay,
                                messageRemoved ) );

                    if ( disposing )
                        await queue.DisposeDueToLackOfReferencesAsync( ignoreProcessorTask: false, queueTraceId ).ConfigureAwait( false );
                }

                if ( client.Logger.AckProcessed is { } ackProcessed )
                    ackProcessed.Emit(
                        MessageBrokerRemoteClientAckProcessedEvent.Create(
                            message.Listener,
                            traceId,
                            message.Publisher,
                            parsedRequest.AckId,
                            parsedRequest.MessageId,
                            parsedRequest.Retry,
                            parsedRequest.Redelivery,
                            isNack: true ) );
            }

            return FinishRequestHandling( client, traceId );
        }
    }

    private static async ValueTask<RequestResult> HandlePingAsync(
        MessageBrokerRemoteClient client,
        Protocol.PacketHeader request,
        Protocol.PacketHeader response,
        ReadOnlyMemory<byte> responseData,
        ulong traceId)
    {
        using ( MessageBrokerRemoteClientTraceEvent.CreateScope( client, traceId, MessageBrokerRemoteClientTraceEventType.Ping ) )
        {
            var readPacket = client.Logger.ReadPacket;
            readPacket?.Emit( MessageBrokerRemoteClientReadPacketEvent.CreateReceived( client, traceId, request ) );

            if ( request.Payload != Protocol.Endianness.VerificationPayload )
            {
                var error = client.ProtocolException( request, Resources.InvalidEndiannessPayload( request.Payload ) );
                return await FinishInvalidRequestHandlingAsync( client, error, traceId ).ConfigureAwait( false );
            }

            readPacket?.Emit( MessageBrokerRemoteClientReadPacketEvent.CreateAccepted( client, traceId, request ) );

            ManualResetValueTaskSource<bool> writerSource;
            using ( client.AcquireActiveLock( traceId, out var exc ) )
            {
                if ( exc is not null )
                    return RequestResult.Done();

                writerSource = client.WriterQueue.AcquireSource();
            }

            if ( ! await writerSource.GetTask().ConfigureAwait( false ) )
            {
                if ( client.Logger.Error is { } error )
                    error.Emit( MessageBrokerRemoteClientErrorEvent.Create( client, traceId, client.DisposedException() ) );

                return RequestResult.Done();
            }

            var result = await client.WriteAsync( response, responseData, traceId ).ConfigureAwait( false );
            if ( result.Exception is null )
                return FinishRequestHandling( client, writerSource, traceId );

            await DisposeClientAsync( client, traceId ).ConfigureAwait( false );
            return RequestResult.Done();
        }
    }

    private static async ValueTask<RequestResult> HandleUnexpectedRequestAsync(
        MessageBrokerRemoteClient client,
        Protocol.PacketHeader request,
        ulong traceId)
    {
        using ( MessageBrokerRemoteClientTraceEvent.CreateScope( client, traceId, MessageBrokerRemoteClientTraceEventType.Unexpected ) )
        {
            var error = client.ProtocolException( request, Resources.UnexpectedServerEndpoint );
            return await FinishInvalidRequestHandlingAsync( client, error, traceId ).ConfigureAwait( false );
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static RequestResult FinishRequestHandling(
        MessageBrokerRemoteClient client,
        ManualResetValueTaskSource<bool> writerSource,
        ulong traceId)
    {
        using ( client.AcquireActiveLock( traceId, out var exc ) )
        {
            if ( exc is not null )
                return RequestResult.Done();

            client.WriterQueue.Release( client, writerSource );
            return RequestResult.Ok( client.RequestQueue.IsNotEmpty() );
        }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static RequestResult FinishRequestHandling(MessageBrokerRemoteClient client, ulong traceId)
    {
        using ( client.AcquireActiveLock( traceId, out var exc ) )
        {
            return exc is not null
                ? RequestResult.Done()
                : RequestResult.Ok( client.RequestQueue.IsNotEmpty() );
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static async ValueTask<RequestResult> FinishInvalidRequestHandlingAsync(
        MessageBrokerRemoteClient client,
        Exception exception,
        ulong traceId)
    {
        if ( client.Logger.Error is { } error )
            error.Emit( MessageBrokerRemoteClientErrorEvent.Create( client, traceId, exception ) );

        await DisposeClientAsync( client, traceId ).ConfigureAwait( false );
        return RequestResult.Done();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static ValueTask DisposeClientAsync(MessageBrokerRemoteClient client, ulong traceId)
    {
        using ( client.AcquireLock() )
            client.RequestHandler._task = null;

        return client.DisposeAsync( traceId );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static ExclusiveLock AcquireActiveServerLock(
        MessageBrokerRemoteClient client,
        ulong traceId,
        out MessageBrokerServerDisposedException? exception)
    {
        var @lock = client.Server.AcquireLock();
        if ( ! client.Server.ShouldCancel )
        {
            exception = null;
            return @lock;
        }

        @lock.Dispose();
        exception = client.Server.DisposedException();
        if ( client.Logger.Error is { } error )
            error.Emit( MessageBrokerRemoteClientErrorEvent.Create( client, traceId, exception ) );

        return default;
    }

    private readonly record struct RequestResult(bool IsDone, bool Continue)
    {
        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static RequestResult Done()
        {
            return new RequestResult( true, false );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static RequestResult Ok(bool @continue)
        {
            return new RequestResult( false, @continue );
        }
    }
}
