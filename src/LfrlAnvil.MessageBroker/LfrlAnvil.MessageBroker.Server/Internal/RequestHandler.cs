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
        TaskStopReason stopReason;
        try
        {
            stopReason = await RunCore( client ).ConfigureAwait( false );
        }
        catch ( Exception exc )
        {
            client.Emit( MessageBrokerRemoteClientEvent.Unexpected( client, exc ) );
            stopReason = TaskStopReason.Error;
        }

        if ( stopReason == TaskStopReason.OwnerDisposed )
            return;

        using ( client.AcquireLock() )
            client.RequestHandler._task = null;

        await client.DisconnectAsync().ConfigureAwait( false );
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
    private static async ValueTask<TaskStopReason> RunCore(MessageBrokerRemoteClient client)
    {
        var pingResponseData = new byte[Protocol.PacketHeader.Length].AsMemory();
        var pingResponse = Protocol.Pong.Create();
        pingResponse.Serialize( pingResponseData );

        while ( true )
        {
            var @continue = await client.RequestHandler._continuation.GetTask().ConfigureAwait( false );
            if ( ! @continue )
                return TaskStopReason.OwnerDisposed;

            bool containsEnqueuedRequests;
            do
            {
                ulong contextId;
                IncomingPacketToken request;
                using ( client.AcquireLock() )
                {
                    if ( client.ShouldCancel )
                        return TaskStopReason.OwnerDisposed;

                    contextId = client.MessageContextQueue.AcquireContextId();
                    request = client.MessageContextQueue.DequeueRequest();
                }

                var result = request.Header.GetServerEndpoint() switch
                {
                    MessageBrokerServerEndpoint.Ping => await HandlePingAsync(
                            client,
                            contextId,
                            request.Header,
                            pingResponse,
                            pingResponseData )
                        .ConfigureAwait( false ),
                    MessageBrokerServerEndpoint.PushMessage => await HandlePushMessageAsync( client, contextId, request )
                        .ConfigureAwait( false ),
                    MessageBrokerServerEndpoint.BindPublisherRequest => await HandleBindPublisherRequestAsync( client, contextId, request )
                        .ConfigureAwait( false ),
                    MessageBrokerServerEndpoint.UnbindPublisherRequest => await HandleUnbindPublisherRequestAsync(
                            client,
                            contextId,
                            request )
                        .ConfigureAwait( false ),
                    MessageBrokerServerEndpoint.BindListenerRequest => await HandleBindListenerRequestAsync( client, contextId, request )
                        .ConfigureAwait( false ),
                    MessageBrokerServerEndpoint.UnbindListenerRequest => await HandleUnbindListenerRequestAsync(
                            client,
                            contextId,
                            request )
                        .ConfigureAwait( false ),
                    _ => HandleUnexpectedRequest( client, request.Header )
                };

                if ( result.StopReason is not null )
                    return result.StopReason.Value;

                containsEnqueuedRequests = result.ContainsEnqueuedRequests;
            }
            while ( containsEnqueuedRequests );

            using ( client.AcquireLock() )
            {
                if ( client.ShouldCancel )
                    return TaskStopReason.OwnerDisposed;

                client.RequestHandler._continuation.Reset();
                if ( client.MessageContextQueue.ContainsEnqueuedRequests() )
                    client.RequestHandler._continuation.SetResult( true );
            }
        }
    }

    private static async ValueTask<HandleRequestResult> HandleBindPublisherRequestAsync(
        MessageBrokerRemoteClient client,
        ulong contextId,
        IncomingPacketToken request)
    {
        var poolToken = request.PoolToken;
        try
        {
            client.Emit( MessageBrokerRemoteClientEvent.MessageReceived( client, request.Header, contextId ) );

            var exc = Protocol.AssertMinPayload( client, request.Header, Protocol.BindPublisherRequestHeader.Length );
            if ( exc is not null )
            {
                client.Emit( MessageBrokerRemoteClientEvent.MessageRejected( client, request.Header, exc, contextId ) );
                return HandleRequestResult.Error();
            }

            var data = request.Data;
            var parsedRequestHeader = Protocol.BindPublisherRequestHeader.Parse(
                data.Slice( 0, Protocol.BindPublisherRequestHeader.Length ) );

            var requestErrors = parsedRequestHeader.StringifyErrors( data.Length );
            if ( requestErrors.Count > 0 )
            {
                client.Emit(
                    MessageBrokerRemoteClientEvent.MessageRejected(
                        client,
                        request.Header,
                        Protocol.ProtocolException( client, request.Header, requestErrors ),
                        contextId ) );

                return HandleRequestResult.Error();
            }

            var channelName = TextEncoding.Parse(
                data.Slice( Protocol.BindPublisherRequestHeader.Length, parsedRequestHeader.ChannelNameLength ) );

            if ( channelName.Exception is not null )
            {
                client.Emit( MessageBrokerRemoteClientEvent.MessageRejected( client, request.Header, channelName.Exception, contextId ) );
                return HandleRequestResult.Error();
            }

            Assume.IsNotNull( channelName.Value );
            if ( ! Defaults.NameLengthBounds.Contains( channelName.Value.Length ) )
            {
                client.Emit(
                    MessageBrokerRemoteClientEvent.MessageRejected(
                        client,
                        request.Header,
                        Protocol.InvalidChannelNameLengthException( client, request.Header, channelName.Value.Length ),
                        contextId ) );

                return HandleRequestResult.Error();
            }

            var streamName = TextEncoding.Parse(
                data.Slice( Protocol.BindPublisherRequestHeader.Length + parsedRequestHeader.ChannelNameLength ) );

            if ( streamName.Exception is not null )
            {
                client.Emit( MessageBrokerRemoteClientEvent.MessageRejected( client, request.Header, streamName.Exception, contextId ) );
                return HandleRequestResult.Error();
            }

            Assume.IsNotNull( streamName.Value );
            if ( streamName.Value.Length > Defaults.NameLengthBounds.Max )
            {
                client.Emit(
                    MessageBrokerRemoteClientEvent.MessageRejected(
                        client,
                        request.Header,
                        Protocol.InvalidStreamNameLengthException( client, request.Header, streamName.Value.Length ),
                        contextId ) );

                return HandleRequestResult.Error();
            }

            if ( streamName.Value.Length == 0 )
                streamName = channelName;

            bool channelCreated;
            var streamCreated = false;
            MessageBrokerChannel? channel;
            MessageBrokerStream? stream = null;
            MessageBrokerChannelPublisherBinding? publisher = null;
            Protocol.BindPublisherFailureResponse.Reasons rejectionReasons;
            ManualResetValueTaskSource<bool> writerSource;

            using ( client.Server.AcquireLock() )
            {
                if ( client.Server.ShouldCancel )
                    return HandleRequestResult.OwnerDisposed();

                using ( client.AcquireLock() )
                {
                    if ( client.ShouldCancel )
                        return HandleRequestResult.OwnerDisposed();

                    channel = ChannelCollection.RegisterUnsafe( client.Server, channelName.Value, out channelCreated );
                    try
                    {
                        rejectionReasons = client.BindPublisherUnsafe(
                            channel,
                            channelCreated,
                            streamName.Value,
                            ref publisher,
                            ref stream,
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
                    writerSource = client.MessageContextQueue.AcquireWriterSource();
                }
            }

            Protocol.PacketHeader responseHeader;
            Memory<byte> responseData;

            if ( rejectionReasons != Protocol.BindPublisherFailureResponse.Reasons.None )
            {
                client.Emit(
                    MessageBrokerRemoteClientEvent.MessageRejected(
                        client,
                        request.Header,
                        new MessageBrokerChannelPublisherBindingException(
                            client,
                            channel,
                            publisher,
                            Resources.FailedToCreatePublisher(
                                client.Id,
                                client.Name,
                                channel.Id,
                                channel.Name,
                                rejectionReasons ) ),
                        contextId ) );

                var responseLength = Protocol.PacketHeader.Length + Protocol.BindPublisherFailureResponse.Payload;
                var response = new Protocol.BindPublisherFailureResponse( rejectionReasons );
                Assume.IsGreaterThanOrEqualTo( data.Length, responseLength );

                responseHeader = response.Header;
                responseData = data.Slice( 0, responseLength );
                response.Serialize( responseData );
            }
            else
            {
                Assume.IsNotNull( channel );
                Assume.IsNotNull( stream );
                Assume.IsNotNull( publisher );

                if ( channelCreated )
                    channel.Emit( MessageBrokerChannelEvent.Created( channel, client, contextId ) );

                if ( streamCreated )
                    stream.Emit( MessageBrokerStreamEvent.Created( stream, publisher, contextId ) );

                publisher.Emit( MessageBrokerChannelPublisherBindingEvent.Created( publisher, contextId ) );
                client.Emit( MessageBrokerRemoteClientEvent.MessageAccepted( client, request.Header, contextId ) );

                var responseLength = Protocol.PacketHeader.Length + Protocol.PublisherBoundResponse.Payload;
                var response = new Protocol.PublisherBoundResponse( channelCreated, streamCreated, channel.Id, stream.Id );
                if ( data.Length < responseLength )
                    poolToken.SetLength( responseLength, out data );

                responseHeader = response.Header;
                responseData = data.Slice( 0, responseLength );
                response.Serialize( responseData );
            }

            if ( ! await writerSource.GetTask().ConfigureAwait( false ) )
                return HandleRequestResult.OwnerDisposed();

            var writeResult = await client.WriteAsync( responseHeader, responseData, contextId ).ConfigureAwait( false );
            return writeResult.Exception is null ? FinishRequestHandling( client, writerSource ) : HandleRequestResult.Error();
        }
        finally
        {
            poolToken.Return( client );
        }
    }

    private static async ValueTask<HandleRequestResult> HandleUnbindPublisherRequestAsync(
        MessageBrokerRemoteClient client,
        ulong contextId,
        IncomingPacketToken request)
    {
        var poolToken = request.PoolToken;
        try
        {
            client.Emit( MessageBrokerRemoteClientEvent.MessageReceived( client, request.Header, contextId ) );

            var exc = Protocol.AssertPayload( client, request.Header, Protocol.UnbindPublisherRequest.Length );
            if ( exc is not null )
            {
                client.Emit( MessageBrokerRemoteClientEvent.MessageRejected( client, request.Header, exc, contextId ) );
                return HandleRequestResult.Error();
            }

            var data = request.Data;
            var parsedRequest = Protocol.UnbindPublisherRequest.Parse( data );

            var disposingChannel = false;
            var disposingStream = false;
            MessageBrokerChannelPublisherBinding? publisher = null;
            MessageBrokerStream? stream = null;
            Protocol.UnbindPublisherFailureResponse.Reasons rejectionReasons;

            var channel = ChannelCollection.TryGetById( client.Server, parsedRequest.ChannelId );
            if ( channel is null )
                rejectionReasons = Protocol.UnbindPublisherFailureResponse.Reasons.NotBound;
            else
            {
                using ( client.AcquireLock() )
                {
                    if ( client.ShouldCancel )
                        return HandleRequestResult.OwnerDisposed();

                    rejectionReasons = client.BeginUnbindPublisherUnsafe(
                        channel,
                        ref publisher,
                        ref stream,
                        ref disposingChannel,
                        ref disposingStream );
                }
            }

            Protocol.PacketHeader responseHeader;
            Memory<byte> responseData;

            if ( rejectionReasons != Protocol.UnbindPublisherFailureResponse.Reasons.None )
            {
                client.Emit(
                    MessageBrokerRemoteClientEvent.MessageRejected(
                        client,
                        request.Header,
                        new MessageBrokerChannelPublisherBindingException(
                            client,
                            channel,
                            publisher,
                            channel is null
                                ? Resources.FailedToUnbindPublisherFromNonExistingChannel( client.Id, client.Name, parsedRequest.ChannelId )
                                : Resources.FailedToUnbindPublisherFromChannel(
                                    client.Id,
                                    client.Name,
                                    channel.Id,
                                    channel.Name ) ),
                        contextId ) );

                var responseLength = Protocol.PacketHeader.Length + Protocol.UnbindPublisherFailureResponse.Payload;
                var response = new Protocol.UnbindPublisherFailureResponse( rejectionReasons );
                if ( data.Length < responseLength )
                    poolToken.SetLength( responseLength, out data );

                responseHeader = response.Header;
                responseData = data.Slice( 0, responseLength );
                response.Serialize( responseData );
            }
            else
            {
                Assume.IsNotNull( channel );
                Assume.IsNotNull( stream );
                Assume.IsNotNull( publisher );

                publisher.Emit( MessageBrokerChannelPublisherBindingEvent.Disposing( publisher, contextId ) );
                if ( disposingStream )
                    await stream.DisposeDueToLackOfReferencesAsync( ignoreProcessorTask: false ).ConfigureAwait( false );

                if ( disposingChannel )
                    channel.DisposeDueToLackOfReferences();

                publisher.EndDisposing();
                client.Emit( MessageBrokerRemoteClientEvent.MessageAccepted( client, request.Header, contextId ) );

                var responseLength = Protocol.PacketHeader.Length + Protocol.PublisherUnboundResponse.Payload;
                var response = new Protocol.PublisherUnboundResponse( disposingChannel, disposingStream );
                if ( data.Length < responseLength )
                    poolToken.SetLength( responseLength, out data );

                responseHeader = response.Header;
                responseData = data.Slice( 0, responseLength );
                response.Serialize( responseData );
            }

            ManualResetValueTaskSource<bool> writerSource;
            using ( client.AcquireLock() )
            {
                if ( client.ShouldCancel )
                    return HandleRequestResult.OwnerDisposed();

                writerSource = client.MessageContextQueue.AcquireWriterSource();
            }

            if ( ! await writerSource.GetTask().ConfigureAwait( false ) )
                return HandleRequestResult.OwnerDisposed();

            var writeResult = await client.WriteAsync( responseHeader, responseData, contextId ).ConfigureAwait( false );
            return writeResult.Exception is null ? FinishRequestHandling( client, writerSource ) : HandleRequestResult.Error();
        }
        finally
        {
            poolToken.Return( client );
        }
    }

    private static async ValueTask<HandleRequestResult> HandleBindListenerRequestAsync(
        MessageBrokerRemoteClient client,
        ulong contextId,
        IncomingPacketToken request)
    {
        var poolToken = request.PoolToken;
        try
        {
            client.Emit( MessageBrokerRemoteClientEvent.MessageReceived( client, request.Header, contextId ) );

            var exc = Protocol.AssertMinPayload( client, request.Header, Protocol.BindListenerRequestHeader.Length );
            if ( exc is not null )
            {
                client.Emit( MessageBrokerRemoteClientEvent.MessageRejected( client, request.Header, exc, contextId ) );
                return HandleRequestResult.Error();
            }

            var data = request.Data;
            var parsedRequestHeader = Protocol.BindListenerRequestHeader.Parse(
                data.Slice( 0, Protocol.BindListenerRequestHeader.Length ) );

            var requestErrors = parsedRequestHeader.StringifyErrors( data.Length );
            if ( requestErrors.Count > 0 )
            {
                client.Emit(
                    MessageBrokerRemoteClientEvent.MessageRejected(
                        client,
                        request.Header,
                        Protocol.ProtocolException( client, request.Header, requestErrors ),
                        contextId ) );

                return HandleRequestResult.Error();
            }

            var channelName = TextEncoding.Parse(
                data.Slice( Protocol.BindListenerRequestHeader.Length, parsedRequestHeader.ChannelNameLength ) );

            if ( channelName.Exception is not null )
            {
                client.Emit( MessageBrokerRemoteClientEvent.MessageRejected( client, request.Header, channelName.Exception, contextId ) );
                return HandleRequestResult.Error();
            }

            Assume.IsNotNull( channelName.Value );
            if ( ! Defaults.NameLengthBounds.Contains( channelName.Value.Length ) )
            {
                client.Emit(
                    MessageBrokerRemoteClientEvent.MessageRejected(
                        client,
                        request.Header,
                        Protocol.InvalidChannelNameLengthException( client, request.Header, channelName.Value.Length ),
                        contextId ) );

                return HandleRequestResult.Error();
            }

            var queueName = TextEncoding.Parse(
                data.Slice( Protocol.BindListenerRequestHeader.Length + parsedRequestHeader.ChannelNameLength ) );

            if ( queueName.Exception is not null )
            {
                client.Emit( MessageBrokerRemoteClientEvent.MessageRejected( client, request.Header, queueName.Exception, contextId ) );
                return HandleRequestResult.Error();
            }

            Assume.IsNotNull( queueName.Value );
            if ( queueName.Value.Length > Defaults.NameLengthBounds.Max )
            {
                client.Emit(
                    MessageBrokerRemoteClientEvent.MessageRejected(
                        client,
                        request.Header,
                        Protocol.InvalidQueueNameLengthException( client, request.Header, queueName.Value.Length ),
                        contextId ) );

                return HandleRequestResult.Error();
            }

            if ( queueName.Value.Length == 0 )
                queueName = channelName;

            var channelCreated = false;
            var queueCreated = false;
            MessageBrokerChannel? channel;
            MessageBrokerQueue? queue = null;
            MessageBrokerChannelListenerBinding? listener = null;
            Protocol.BindListenerFailureResponse.Reasons rejectionReasons;
            ManualResetValueTaskSource<bool> writerSource;

            using ( client.Server.AcquireLock() )
            {
                if ( client.Server.ShouldCancel )
                    return HandleRequestResult.OwnerDisposed();

                using ( client.AcquireLock() )
                {
                    if ( client.ShouldCancel )
                        return HandleRequestResult.OwnerDisposed();

                    channel = ChannelCollection.TryRegisterUnsafe(
                        client.Server,
                        channelName.Value,
                        parsedRequestHeader.CreateChannelIfNotExists,
                        ref channelCreated );

                    if ( channel is null )
                        rejectionReasons = Protocol.BindListenerFailureResponse.Reasons.ChannelDoesNotExist;
                    else
                    {
                        try
                        {
                            rejectionReasons = client.BindListenerUnsafe(
                                channel,
                                channelCreated,
                                queueName.Value,
                                parsedRequestHeader.PrefetchHint,
                                ref listener,
                                ref queue,
                                ref queueCreated );
                        }
                        catch
                        {
                            if ( channelCreated )
                                ChannelCollection.RemoveUnsafe( channel );

                            throw;
                        }
                    }

                    writerSource = client.MessageContextQueue.AcquireWriterSource();
                }
            }

            Protocol.PacketHeader responseHeader;
            Memory<byte> responseData;

            if ( rejectionReasons != Protocol.BindListenerFailureResponse.Reasons.None )
            {
                client.Emit(
                    MessageBrokerRemoteClientEvent.MessageRejected(
                        client,
                        request.Header,
                        new MessageBrokerChannelListenerBindingException(
                            client,
                            channel,
                            listener,
                            Resources.FailedToCreateListenerBinding(
                                client.Id,
                                client.Name,
                                channel?.Id,
                                channelName.Value,
                                rejectionReasons ) ),
                        contextId ) );

                var responseLength = Protocol.PacketHeader.Length + Protocol.BindListenerFailureResponse.Payload;
                var response = new Protocol.BindListenerFailureResponse( rejectionReasons );
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

                if ( channelCreated )
                    channel.Emit( MessageBrokerChannelEvent.Created( channel, client, contextId ) );

                if ( queueCreated )
                    queue.Emit( MessageBrokerQueueEvent.Created( queue, listener, contextId ) );

                listener.Emit( MessageBrokerChannelListenerBindingEvent.Created( listener, contextId ) );
                client.Emit( MessageBrokerRemoteClientEvent.MessageAccepted( client, request.Header, contextId ) );

                var responseLength = Protocol.PacketHeader.Length + Protocol.ListenerBoundResponse.Payload;
                var response = new Protocol.ListenerBoundResponse( channelCreated, queueCreated, channel.Id, queue.Id );
                if ( data.Length < responseLength )
                    poolToken.SetLength( responseLength, out data );

                responseHeader = response.Header;
                responseData = data.Slice( 0, responseLength );
                response.Serialize( responseData );
            }

            if ( ! await writerSource.GetTask().ConfigureAwait( false ) )
                return HandleRequestResult.OwnerDisposed();

            var writeResult = await client.WriteAsync( responseHeader, responseData, contextId ).ConfigureAwait( false );
            return writeResult.Exception is null ? FinishRequestHandling( client, writerSource ) : HandleRequestResult.Error();
        }
        finally
        {
            poolToken.Return( client );
        }
    }

    private static async ValueTask<HandleRequestResult> HandleUnbindListenerRequestAsync(
        MessageBrokerRemoteClient client,
        ulong contextId,
        IncomingPacketToken request)
    {
        var poolToken = request.PoolToken;
        try
        {
            client.Emit( MessageBrokerRemoteClientEvent.MessageReceived( client, request.Header, contextId ) );

            var exc = Protocol.AssertPayload( client, request.Header, Protocol.UnbindListenerRequest.Length );
            if ( exc is not null )
            {
                client.Emit( MessageBrokerRemoteClientEvent.MessageRejected( client, request.Header, exc, contextId ) );
                return HandleRequestResult.Error();
            }

            var data = request.Data;
            var parsedRequest = Protocol.UnbindListenerRequest.Parse( data );

            var disposingChannel = false;
            var disposingQueue = false;
            MessageBrokerChannelListenerBinding? listener = null;
            MessageBrokerQueue? queue = null;
            Protocol.UnbindListenerFailureResponse.Reasons rejectionReasons;

            var channel = ChannelCollection.TryGetById( client.Server, parsedRequest.ChannelId );
            if ( channel is null )
                rejectionReasons = Protocol.UnbindListenerFailureResponse.Reasons.NotBound;
            else
            {
                using ( client.AcquireLock() )
                {
                    if ( client.ShouldCancel )
                        return HandleRequestResult.OwnerDisposed();

                    rejectionReasons = client.BeginUnbindListenerUnsafe(
                        channel,
                        ref listener,
                        ref queue,
                        ref disposingChannel,
                        ref disposingQueue );
                }
            }

            Protocol.PacketHeader responseHeader;
            Memory<byte> responseData;

            if ( rejectionReasons != Protocol.UnbindListenerFailureResponse.Reasons.None )
            {
                client.Emit(
                    MessageBrokerRemoteClientEvent.MessageRejected(
                        client,
                        request.Header,
                        new MessageBrokerChannelListenerBindingException(
                            client,
                            channel,
                            listener,
                            channel is null
                                ? Resources.FailedToUnbindListenerFromNonExistingChannel( client.Id, client.Name, parsedRequest.ChannelId )
                                : Resources.FailedToUnbindListenerFromChannel(
                                    client.Id,
                                    client.Name,
                                    channel.Id,
                                    channel.Name ) ),
                        contextId ) );

                var responseLength = Protocol.PacketHeader.Length + Protocol.UnbindListenerFailureResponse.Payload;
                var response = new Protocol.UnbindListenerFailureResponse( rejectionReasons );
                if ( data.Length < responseLength )
                    poolToken.SetLength( responseLength, out data );

                responseHeader = response.Header;
                responseData = data.Slice( 0, responseLength );
                response.Serialize( responseData );
            }
            else
            {
                Assume.IsNotNull( channel );
                Assume.IsNotNull( queue );
                Assume.IsNotNull( listener );

                listener.Emit( MessageBrokerChannelListenerBindingEvent.Disposing( listener, contextId ) );
                if ( disposingQueue )
                    await queue.DisposeDueToLackOfReferencesAsync( ignoreProcessorTask: false ).ConfigureAwait( false );

                if ( disposingChannel )
                    channel.DisposeDueToLackOfReferences();

                listener.EndDisposing();
                client.Emit( MessageBrokerRemoteClientEvent.MessageAccepted( client, request.Header, contextId ) );

                var responseLength = Protocol.PacketHeader.Length + Protocol.ListenerUnboundResponse.Payload;
                var response = new Protocol.ListenerUnboundResponse( disposingChannel, disposingQueue );
                if ( data.Length < responseLength )
                    poolToken.SetLength( responseLength, out data );

                responseHeader = response.Header;
                responseData = data.Slice( 0, responseLength );
                response.Serialize( responseData );
            }

            ManualResetValueTaskSource<bool> writerSource;
            using ( client.AcquireLock() )
            {
                if ( client.ShouldCancel )
                    return HandleRequestResult.OwnerDisposed();

                writerSource = client.MessageContextQueue.AcquireWriterSource();
            }

            if ( ! await writerSource.GetTask().ConfigureAwait( false ) )
                return HandleRequestResult.OwnerDisposed();

            var writeResult = await client.WriteAsync( responseHeader, responseData, contextId ).ConfigureAwait( false );
            return writeResult.Exception is null ? FinishRequestHandling( client, writerSource ) : HandleRequestResult.Error();
        }
        finally
        {
            poolToken.Return( client );
        }
    }

    private static async ValueTask<HandleRequestResult> HandlePushMessageAsync(
        MessageBrokerRemoteClient client,
        ulong contextId,
        IncomingPacketToken request)
    {
        ulong? messageId = null;
        MessageBrokerChannelPublisherBinding? publisher;
        Protocol.MessageRejectedResponse.Reasons rejectionReasons;
        Protocol.PushMessageHeader parsedRequest;

        try
        {
            client.Emit( MessageBrokerRemoteClientEvent.MessageReceived( client, request.Header, contextId ) );

            var exc = Protocol.AssertMinPayload( client, request.Header, Protocol.PushMessageHeader.Length );
            if ( exc is not null )
            {
                client.Emit( MessageBrokerRemoteClientEvent.MessageRejected( client, request.Header, exc, contextId ) );
                return HandleRequestResult.Error();
            }

            parsedRequest = Protocol.PushMessageHeader.Parse( request.Data.Slice( 0, Protocol.PushMessageHeader.Length ) );

            using ( client.AcquireLock() )
            {
                if ( client.ShouldCancel )
                    return HandleRequestResult.OwnerDisposed();

                if ( client.PublishersByChannelId.TryGet( parsedRequest.ChannelId, out publisher ) )
                {
                    rejectionReasons = publisher.Stream.PushMessage(
                        publisher,
                        request.PoolToken,
                        request.Data.Slice( Protocol.PushMessageHeader.Length ),
                        contextId,
                        ref messageId );
                }
                else
                    rejectionReasons = Protocol.MessageRejectedResponse.Reasons.NotBound;
            }
        }
        finally
        {
            if ( messageId is null )
                request.PoolToken.Return( client );
        }

        var poolToken = default( MemoryPoolToken<byte> );
        try
        {
            Protocol.PacketHeader responseHeader;
            Memory<byte> responseData;

            if ( rejectionReasons != Protocol.MessageRejectedResponse.Reasons.None )
            {
                client.Emit(
                    MessageBrokerRemoteClientEvent.MessageRejected(
                        client,
                        request.Header,
                        new MessageBrokerChannelPublisherBindingException(
                            client,
                            null,
                            null,
                            Resources.FailedToPushMessageToUnboundChannel( client.Id, client.Name, parsedRequest.ChannelId ) ),
                        contextId ) );

                if ( ! parsedRequest.Confirm )
                    return FinishRequestHandling( client );

                var responseLength = Protocol.PacketHeader.Length + Protocol.MessageRejectedResponse.Payload;
                var response = new Protocol.MessageRejectedResponse( rejectionReasons );
                poolToken = client.MemoryPool.Rent( responseLength, out responseData ).EnableClearing();
                responseHeader = response.Header;
                response.Serialize( responseData );
            }
            else
            {
                Assume.IsNotNull( publisher );
                Assume.IsNotNull( messageId );

                client.Emit( MessageBrokerRemoteClientEvent.MessageAccepted( client, request.Header, contextId ) );

                if ( ! parsedRequest.Confirm )
                    return FinishRequestHandling( client );

                var responseLength = Protocol.PacketHeader.Length + Protocol.MessageAcceptedResponse.Payload;
                var response = new Protocol.MessageAcceptedResponse( messageId.Value );
                poolToken = client.MemoryPool.Rent( responseLength, out responseData ).EnableClearing();
                responseHeader = response.Header;
                response.Serialize( responseData );
            }

            ManualResetValueTaskSource<bool> writerSource;
            using ( client.AcquireLock() )
            {
                if ( client.ShouldCancel )
                    return HandleRequestResult.OwnerDisposed();

                writerSource = client.MessageContextQueue.AcquireWriterSource();
            }

            if ( ! await writerSource.GetTask().ConfigureAwait( false ) )
                return HandleRequestResult.OwnerDisposed();

            var writeResult = await client.WriteAsync( responseHeader, responseData, contextId ).ConfigureAwait( false );
            return writeResult.Exception is null ? FinishRequestHandling( client, writerSource ) : HandleRequestResult.Error();
        }
        finally
        {
            poolToken.Return( client );
        }
    }

    private static async ValueTask<HandleRequestResult> HandlePingAsync(
        MessageBrokerRemoteClient client,
        ulong contextId,
        Protocol.PacketHeader request,
        Protocol.PacketHeader response,
        ReadOnlyMemory<byte> responseData)
    {
        client.Emit( MessageBrokerRemoteClientEvent.MessageReceived( client, request, contextId ) );

        if ( request.Payload != Protocol.Endianness.VerificationPayload )
        {
            client.Emit(
                MessageBrokerRemoteClientEvent.MessageRejected(
                    client,
                    request,
                    Protocol.EndiannessPayloadException( client, request ),
                    contextId ) );

            return HandleRequestResult.Error();
        }

        client.Emit( MessageBrokerRemoteClientEvent.MessageAccepted( client, request, contextId ) );

        ManualResetValueTaskSource<bool> writerSource;
        using ( client.AcquireLock() )
        {
            if ( client.ShouldCancel )
                return HandleRequestResult.OwnerDisposed();

            writerSource = client.MessageContextQueue.AcquireWriterSource();
        }

        if ( ! await writerSource.GetTask().ConfigureAwait( false ) )
            return HandleRequestResult.OwnerDisposed();

        var result = await client.WriteAsync( response, responseData, contextId ).ConfigureAwait( false );
        return result.Exception is null ? FinishRequestHandling( client, writerSource ) : HandleRequestResult.Error();
    }

    private static HandleRequestResult HandleUnexpectedRequest(MessageBrokerRemoteClient client, Protocol.PacketHeader request)
    {
        client.HandleUnexpectedEndpoint( request );
        return HandleRequestResult.Error();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static HandleRequestResult FinishRequestHandling(
        MessageBrokerRemoteClient client,
        ManualResetValueTaskSource<bool> writerSource)
    {
        using ( client.AcquireLock() )
        {
            if ( client.ShouldCancel )
                return HandleRequestResult.OwnerDisposed();

            client.MessageContextQueue.ResetOutgoingWriter( client, writerSource );
            return HandleRequestResult.Ok( client.MessageContextQueue.ContainsEnqueuedRequests() );
        }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static HandleRequestResult FinishRequestHandling(MessageBrokerRemoteClient client)
    {
        using ( client.AcquireLock() )
        {
            return client.ShouldCancel
                ? HandleRequestResult.OwnerDisposed()
                : HandleRequestResult.Ok( client.MessageContextQueue.ContainsEnqueuedRequests() );
        }
    }

    private readonly record struct HandleRequestResult(TaskStopReason? StopReason, bool ContainsEnqueuedRequests)
    {
        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static HandleRequestResult Error()
        {
            return new HandleRequestResult( TaskStopReason.Error, false );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static HandleRequestResult OwnerDisposed()
        {
            return new HandleRequestResult( TaskStopReason.OwnerDisposed, false );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static HandleRequestResult Ok(bool containsEnqueuedRequests)
        {
            return new HandleRequestResult( null, containsEnqueuedRequests );
        }
    }
}
