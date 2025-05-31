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
                MessageBrokerRemoteClientErrorEvent.Create( client, traceId, exc ).Emit( client.Logger.Error );
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
                MessageBrokerRemoteClientReadPacketEvent.CreateReceived( client, traceId, request.Header ).Emit( client.Logger.ReadPacket );

                var exception = Protocol.AssertMinPayload( client, request.Header, Protocol.BindPublisherRequestHeader.Length );
                if ( exception is not null )
                    return await FinishInvalidRequestHandlingAsync( client, exception, traceId ).ConfigureAwait( false );

                var data = request.Data;
                var parsedRequestHeader = Protocol.BindPublisherRequestHeader.Parse(
                    data.Slice( 0, Protocol.BindPublisherRequestHeader.Length ) );

                var requestErrors = parsedRequestHeader.StringifyErrors( data.Length );
                if ( requestErrors.Count > 0 )
                {
                    var error = Protocol.ProtocolException( client, request.Header, requestErrors );
                    return await FinishInvalidRequestHandlingAsync( client, error, traceId ).ConfigureAwait( false );
                }

                var channelName = TextEncoding.Parse(
                    data.Slice( Protocol.BindPublisherRequestHeader.Length, parsedRequestHeader.ChannelNameLength ) );

                if ( channelName.Exception is not null )
                    return await FinishInvalidRequestHandlingAsync( client, channelName.Exception, traceId ).ConfigureAwait( false );

                Assume.IsNotNull( channelName.Value );
                if ( ! Defaults.NameLengthBounds.Contains( channelName.Value.Length ) )
                {
                    var error = Protocol.InvalidChannelNameLengthException( client, request.Header, channelName.Value.Length );
                    return await FinishInvalidRequestHandlingAsync( client, error, traceId ).ConfigureAwait( false );
                }

                var streamName = TextEncoding.Parse(
                    data.Slice( Protocol.BindPublisherRequestHeader.Length + parsedRequestHeader.ChannelNameLength ) );

                if ( streamName.Exception is not null )
                    return await FinishInvalidRequestHandlingAsync( client, streamName.Exception, traceId ).ConfigureAwait( false );

                Assume.IsNotNull( streamName.Value );
                if ( streamName.Value.Length > Defaults.NameLengthBounds.Max )
                {
                    var error = Protocol.InvalidStreamNameLengthException( client, request.Header, streamName.Value.Length );
                    return await FinishInvalidRequestHandlingAsync( client, error, traceId ).ConfigureAwait( false );
                }

                MessageBrokerRemoteClientBindingPublisherEvent.Create( client, traceId, channelName.Value, streamName.Value )
                    .Emit( client.Logger.BindingPublisher );

                if ( streamName.Value.Length == 0 )
                    streamName = channelName;

                bool channelCreated;
                var streamCreated = false;
                ulong channelTraceId = 0;
                ulong streamTraceId = 0;
                MessageBrokerChannel? channel;
                MessageBrokerStream? stream = null;
                MessageBrokerChannelPublisherBinding? publisher = null;
                Protocol.BindPublisherFailureResponse.Reasons rejectionReasons;
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
                            rejectionReasons = client.BindPublisherUnsafe(
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

                if ( rejectionReasons != Protocol.BindPublisherFailureResponse.Reasons.None )
                {
                    var error = new MessageBrokerChannelPublisherBindingException(
                        client,
                        channel,
                        publisher,
                        Resources.FailedToCreatePublisher(
                            client.Id,
                            client.Name,
                            channel.Id,
                            channel.Name,
                            rejectionReasons ) );

                    MessageBrokerRemoteClientErrorEvent.Create( client, traceId, error ).Emit( client.Logger.Error );
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

                    MessageBrokerRemoteClientReadPacketEvent.CreateAccepted( client, traceId, request.Header )
                        .Emit( client.Logger.ReadPacket );

                    using ( MessageBrokerChannelTraceEvent.CreateScope(
                        channel,
                        channelTraceId,
                        MessageBrokerChannelTraceEventType.BindPublisher ) )
                    {
                        MessageBrokerChannelClientTraceEvent.Create( channel, channelTraceId, client, traceId )
                            .Emit( channel.Logger.ClientTrace );

                        if ( channelCreated )
                            MessageBrokerChannelCreatedEvent.Create( channel, channelTraceId ).Emit( channel.Logger.Created );

                        MessageBrokerChannelPublisherBoundEvent.Create( publisher, channelTraceId, streamCreated )
                            .Emit( channel.Logger.PublisherBound );
                    }

                    using ( MessageBrokerStreamTraceEvent.CreateScope(
                        stream,
                        streamTraceId,
                        MessageBrokerStreamTraceEventType.BindPublisher ) )
                    {
                        MessageBrokerStreamClientTraceEvent.Create( stream, streamTraceId, client, traceId )
                            .Emit( stream.Logger.ClientTrace );

                        if ( streamCreated )
                            MessageBrokerStreamCreatedEvent.Create( stream, streamTraceId ).Emit( stream.Logger.Created );

                        MessageBrokerStreamPublisherBoundEvent.Create( publisher, streamTraceId, channelCreated )
                            .Emit( stream.Logger.PublisherBound );
                    }

                    MessageBrokerRemoteClientPublisherBoundEvent.Create( publisher, traceId, channelCreated, streamCreated )
                        .Emit( client.Logger.PublisherBound );

                    var responseLength = Protocol.PacketHeader.Length + Protocol.PublisherBoundResponse.Payload;
                    var response = new Protocol.PublisherBoundResponse( channelCreated, streamCreated, channel.Id, stream.Id );
                    if ( data.Length < responseLength )
                        poolToken.SetLength( responseLength, out data );

                    responseHeader = response.Header;
                    responseData = data.Slice( 0, responseLength );
                    response.Serialize( responseData );
                }

                if ( ! await writerSource.GetTask().ConfigureAwait( false ) )
                {
                    MessageBrokerRemoteClientErrorEvent.Create( client, traceId, client.DisposedException() ).Emit( client.Logger.Error );
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
                MessageBrokerRemoteClientReadPacketEvent.CreateReceived( client, traceId, request.Header ).Emit( client.Logger.ReadPacket );

                var exception = Protocol.AssertPayload( client, request.Header, Protocol.UnbindPublisherRequest.Length );
                if ( exception is not null )
                    return await FinishInvalidRequestHandlingAsync( client, exception, traceId ).ConfigureAwait( false );

                var data = request.Data;
                var parsedRequest = Protocol.UnbindPublisherRequest.Parse( data );
                var requestErrors = parsedRequest.StringifyErrors();
                if ( requestErrors.Count > 0 )
                {
                    var error = Protocol.ProtocolException( client, request.Header, requestErrors );
                    return await FinishInvalidRequestHandlingAsync( client, error, traceId ).ConfigureAwait( false );
                }

                MessageBrokerRemoteClientUnbindingPublisherEvent.Create( client, traceId, parsedRequest.ChannelId )
                    .Emit( client.Logger.UnbindingPublisher );

                var disposingChannel = false;
                var disposingStream = false;
                ulong channelTraceId = 0;
                ulong streamTraceId = 0;
                MessageBrokerChannelPublisherBinding? publisher = null;
                MessageBrokerStream? stream = null;
                Protocol.UnbindPublisherFailureResponse.Reasons rejectionReasons;

                var channel = ChannelCollection.TryGetById( client.Server, parsedRequest.ChannelId );
                if ( channel is null )
                    rejectionReasons = Protocol.UnbindPublisherFailureResponse.Reasons.NotBound;
                else
                {
                    using ( client.AcquireActiveLock( traceId, out var exc ) )
                    {
                        if ( exc is not null )
                            return RequestResult.Done();

                        rejectionReasons = client.BeginUnbindPublisherUnsafe(
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

                if ( rejectionReasons != Protocol.UnbindPublisherFailureResponse.Reasons.None )
                {
                    var error = new MessageBrokerChannelPublisherBindingException(
                        client,
                        channel,
                        publisher,
                        channel is null
                            ? Resources.FailedToUnbindPublisherFromNonExistingChannel( client.Id, client.Name, parsedRequest.ChannelId )
                            : Resources.FailedToUnbindPublisherFromChannel(
                                client.Id,
                                client.Name,
                                channel.Id,
                                channel.Name ) );

                    MessageBrokerRemoteClientErrorEvent.Create( client, traceId, error ).Emit( client.Logger.Error );
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

                    MessageBrokerRemoteClientReadPacketEvent.CreateAccepted( client, traceId, request.Header )
                        .Emit( client.Logger.ReadPacket );

                    using ( MessageBrokerStreamTraceEvent.CreateScope(
                        stream,
                        streamTraceId,
                        MessageBrokerStreamTraceEventType.UnbindPublisher ) )
                    {
                        MessageBrokerStreamClientTraceEvent.Create( stream, streamTraceId, client, traceId )
                            .Emit( stream.Logger.ClientTrace );

                        MessageBrokerStreamPublisherUnboundEvent.Create( publisher, streamTraceId, disposingChannel )
                            .Emit( stream.Logger.PublisherUnbound );

                        if ( disposingStream )
                            await stream.DisposeDueToLackOfReferencesAsync( ignoreProcessorTask: false, streamTraceId )
                                .ConfigureAwait( false );
                    }

                    using ( MessageBrokerChannelTraceEvent.CreateScope(
                        channel,
                        channelTraceId,
                        MessageBrokerChannelTraceEventType.UnbindPublisher ) )
                    {
                        MessageBrokerChannelClientTraceEvent.Create( channel, channelTraceId, client, traceId )
                            .Emit( channel.Logger.ClientTrace );

                        MessageBrokerChannelPublisherUnboundEvent.Create( publisher, channelTraceId, disposingStream )
                            .Emit( channel.Logger.PublisherUnbound );

                        if ( disposingChannel )
                            channel.DisposeDueToLackOfReferences( channelTraceId );
                    }

                    publisher.EndDisposing();

                    MessageBrokerRemoteClientPublisherUnboundEvent.Create( publisher, traceId, disposingChannel, disposingStream )
                        .Emit( client.Logger.PublisherUnbound );

                    var responseLength = Protocol.PacketHeader.Length + Protocol.PublisherUnboundResponse.Payload;
                    var response = new Protocol.PublisherUnboundResponse( disposingChannel, disposingStream );
                    if ( data.Length < responseLength )
                        poolToken.SetLength( responseLength, out data );

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
                    MessageBrokerRemoteClientErrorEvent.Create( client, traceId, client.DisposedException() ).Emit( client.Logger.Error );
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
                MessageBrokerRemoteClientReadPacketEvent.CreateReceived( client, traceId, request.Header ).Emit( client.Logger.ReadPacket );

                var exception = Protocol.AssertMinPayload( client, request.Header, Protocol.BindListenerRequestHeader.Length );
                if ( exception is not null )
                    return await FinishInvalidRequestHandlingAsync( client, exception, traceId ).ConfigureAwait( false );

                var data = request.Data;
                var parsedRequestHeader = Protocol.BindListenerRequestHeader.Parse(
                    data.Slice( 0, Protocol.BindListenerRequestHeader.Length ) );

                var requestErrors = parsedRequestHeader.StringifyErrors( data.Length );
                if ( requestErrors.Count > 0 )
                {
                    var error = Protocol.ProtocolException( client, request.Header, requestErrors );
                    return await FinishInvalidRequestHandlingAsync( client, error, traceId ).ConfigureAwait( false );
                }

                var channelName = TextEncoding.Parse(
                    data.Slice( Protocol.BindListenerRequestHeader.Length, parsedRequestHeader.ChannelNameLength ) );

                if ( channelName.Exception is not null )
                    return await FinishInvalidRequestHandlingAsync( client, channelName.Exception, traceId ).ConfigureAwait( false );

                Assume.IsNotNull( channelName.Value );
                if ( ! Defaults.NameLengthBounds.Contains( channelName.Value.Length ) )
                {
                    var error = Protocol.InvalidChannelNameLengthException( client, request.Header, channelName.Value.Length );
                    return await FinishInvalidRequestHandlingAsync( client, error, traceId ).ConfigureAwait( false );
                }

                var queueName = TextEncoding.Parse(
                    data.Slice( Protocol.BindListenerRequestHeader.Length + parsedRequestHeader.ChannelNameLength ) );

                if ( queueName.Exception is not null )
                    return await FinishInvalidRequestHandlingAsync( client, queueName.Exception, traceId ).ConfigureAwait( false );

                Assume.IsNotNull( queueName.Value );
                if ( queueName.Value.Length > Defaults.NameLengthBounds.Max )
                {
                    var error = Protocol.InvalidQueueNameLengthException( client, request.Header, queueName.Value.Length );
                    return await FinishInvalidRequestHandlingAsync( client, error, traceId ).ConfigureAwait( false );
                }

                MessageBrokerRemoteClientBindingListenerEvent.Create(
                        client,
                        traceId,
                        channelName.Value,
                        queueName.Value,
                        parsedRequestHeader.PrefetchHint,
                        parsedRequestHeader.CreateChannelIfNotExists )
                    .Emit( client.Logger.BindingListener );

                if ( queueName.Value.Length == 0 )
                    queueName = channelName;

                var channelCreated = false;
                var queueCreated = false;
                ulong channelTraceId = 0;
                ulong queueTraceId = 0;
                MessageBrokerChannel? channel;
                MessageBrokerQueue? queue = null;
                MessageBrokerChannelListenerBinding? listener = null;
                Protocol.BindListenerFailureResponse.Reasons rejectionReasons;
                ManualResetValueTaskSource<bool> writerSource;

                using ( AcquireActiveServerLock( client, traceId, out var serverExc ) )
                {
                    if ( serverExc is not null )
                        return RequestResult.Done();

                    using ( client.AcquireActiveLock( traceId, out var exc ) )
                    {
                        if ( exc is not null )
                            return RequestResult.Done();

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

                        writerSource = client.WriterQueue.AcquireSource();
                    }
                }

                Protocol.PacketHeader responseHeader;
                Memory<byte> responseData;

                if ( rejectionReasons != Protocol.BindListenerFailureResponse.Reasons.None )
                {
                    var error = new MessageBrokerChannelListenerBindingException(
                        client,
                        channel,
                        listener,
                        Resources.FailedToCreateListenerBinding(
                            client.Id,
                            client.Name,
                            channel?.Id,
                            channelName.Value,
                            rejectionReasons ) );

                    MessageBrokerRemoteClientErrorEvent.Create( client, traceId, error ).Emit( client.Logger.Error );
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

                    MessageBrokerRemoteClientReadPacketEvent.CreateAccepted( client, traceId, request.Header )
                        .Emit( client.Logger.ReadPacket );

                    using ( MessageBrokerChannelTraceEvent.CreateScope(
                        channel,
                        channelTraceId,
                        MessageBrokerChannelTraceEventType.BindListener ) )
                    {
                        MessageBrokerChannelClientTraceEvent.Create( channel, channelTraceId, client, traceId )
                            .Emit( channel.Logger.ClientTrace );

                        if ( channelCreated )
                            MessageBrokerChannelCreatedEvent.Create( channel, channelTraceId ).Emit( channel.Logger.Created );

                        MessageBrokerChannelListenerBoundEvent.Create( listener, channelTraceId, queueCreated )
                            .Emit( channel.Logger.ListenerBound );
                    }

                    using ( MessageBrokerQueueTraceEvent.CreateScope( queue, queueTraceId, MessageBrokerQueueTraceEventType.BindListener ) )
                    {
                        MessageBrokerQueueClientTraceEvent.Create( queue, queueTraceId, traceId ).Emit( queue.Logger.ClientTrace );
                        if ( queueCreated )
                            MessageBrokerQueueCreatedEvent.Create( queue, queueTraceId ).Emit( queue.Logger.Created );

                        MessageBrokerQueueListenerBoundEvent.Create( listener, queueTraceId, channelCreated )
                            .Emit( queue.Logger.ListenerBound );
                    }

                    MessageBrokerRemoteClientListenerBoundEvent.Create( listener, traceId, channelCreated, queueCreated )
                        .Emit( client.Logger.ListenerBound );

                    var responseLength = Protocol.PacketHeader.Length + Protocol.ListenerBoundResponse.Payload;
                    var response = new Protocol.ListenerBoundResponse( channelCreated, queueCreated, channel.Id, queue.Id );
                    if ( data.Length < responseLength )
                        poolToken.SetLength( responseLength, out data );

                    responseHeader = response.Header;
                    responseData = data.Slice( 0, responseLength );
                    response.Serialize( responseData );
                }

                if ( ! await writerSource.GetTask().ConfigureAwait( false ) )
                {
                    MessageBrokerRemoteClientErrorEvent.Create( client, traceId, client.DisposedException() ).Emit( client.Logger.Error );
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
                MessageBrokerRemoteClientReadPacketEvent.CreateReceived( client, traceId, request.Header ).Emit( client.Logger.ReadPacket );

                var exception = Protocol.AssertPayload( client, request.Header, Protocol.UnbindListenerRequest.Length );
                if ( exception is not null )
                    return await FinishInvalidRequestHandlingAsync( client, exception, traceId ).ConfigureAwait( false );

                var data = request.Data;
                var parsedRequest = Protocol.UnbindListenerRequest.Parse( data );
                var requestErrors = parsedRequest.StringifyErrors();
                if ( requestErrors.Count > 0 )
                {
                    var error = Protocol.ProtocolException( client, request.Header, requestErrors );
                    return await FinishInvalidRequestHandlingAsync( client, error, traceId ).ConfigureAwait( false );
                }

                MessageBrokerRemoteClientUnbindingListenerEvent.Create( client, traceId, parsedRequest.ChannelId )
                    .Emit( client.Logger.UnbindingListener );

                var disposingChannel = false;
                var disposingQueue = false;
                ulong channelTraceId = 0;
                ulong queueTraceId = 0;
                MessageBrokerChannelListenerBinding? listener = null;
                MessageBrokerQueue? queue = null;
                Protocol.UnbindListenerFailureResponse.Reasons rejectionReasons;

                var channel = ChannelCollection.TryGetById( client.Server, parsedRequest.ChannelId );
                if ( channel is null )
                    rejectionReasons = Protocol.UnbindListenerFailureResponse.Reasons.NotBound;
                else
                {
                    using ( client.AcquireActiveLock( traceId, out var exc ) )
                    {
                        if ( exc is not null )
                            return RequestResult.Done();

                        rejectionReasons = client.BeginUnbindListenerUnsafe(
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

                if ( rejectionReasons != Protocol.UnbindListenerFailureResponse.Reasons.None )
                {
                    var error = new MessageBrokerChannelListenerBindingException(
                        client,
                        channel,
                        listener,
                        channel is null
                            ? Resources.FailedToUnbindListenerFromNonExistingChannel(
                                client.Id,
                                client.Name,
                                parsedRequest.ChannelId )
                            : Resources.FailedToUnbindListenerFromChannel(
                                client.Id,
                                client.Name,
                                channel.Id,
                                channel.Name ) );

                    MessageBrokerRemoteClientErrorEvent.Create( client, traceId, error ).Emit( client.Logger.Error );

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

                    MessageBrokerRemoteClientReadPacketEvent.CreateAccepted( client, traceId, request.Header )
                        .Emit( client.Logger.ReadPacket );

                    using ( MessageBrokerQueueTraceEvent.CreateScope(
                        queue,
                        queueTraceId,
                        MessageBrokerQueueTraceEventType.UnbindListener ) )
                    {
                        MessageBrokerQueueClientTraceEvent.Create( queue, queueTraceId, traceId ).Emit( queue.Logger.ClientTrace );
                        MessageBrokerQueueListenerUnboundEvent.Create( listener, queueTraceId, disposingChannel )
                            .Emit( queue.Logger.ListenerUnbound );

                        if ( disposingQueue )
                            await queue.DisposeDueToLackOfReferencesAsync( ignoreProcessorTask: false, queueTraceId )
                                .ConfigureAwait( false );
                    }

                    using ( MessageBrokerChannelTraceEvent.CreateScope(
                        channel,
                        channelTraceId,
                        MessageBrokerChannelTraceEventType.UnbindListener ) )
                    {
                        MessageBrokerChannelClientTraceEvent.Create( channel, channelTraceId, client, traceId )
                            .Emit( channel.Logger.ClientTrace );

                        MessageBrokerChannelListenerUnboundEvent.Create( listener, channelTraceId, disposingQueue )
                            .Emit( channel.Logger.ListenerUnbound );

                        if ( disposingChannel )
                            channel.DisposeDueToLackOfReferences( channelTraceId );
                    }

                    listener.EndDisposing();

                    MessageBrokerRemoteClientListenerUnboundEvent.Create( listener, traceId, disposingChannel, disposingQueue )
                        .Emit( client.Logger.ListenerUnbound );

                    var responseLength = Protocol.PacketHeader.Length + Protocol.ListenerUnboundResponse.Payload;
                    var response = new Protocol.ListenerUnboundResponse( disposingChannel, disposingQueue );
                    if ( data.Length < responseLength )
                        poolToken.SetLength( responseLength, out data );

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
                    MessageBrokerRemoteClientErrorEvent.Create( client, traceId, client.DisposedException() ).Emit( client.Logger.Error );
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

    private static async ValueTask<RequestResult> HandlePushMessageAsync(
        MessageBrokerRemoteClient client,
        IncomingPacketToken request,
        ulong traceId)
    {
        using ( MessageBrokerRemoteClientTraceEvent.CreateScope( client, traceId, MessageBrokerRemoteClientTraceEventType.PushMessage ) )
        {
            ulong? messageId = null;
            ulong streamTraceId = 0;
            MessageBrokerChannelPublisherBinding? publisher;
            Protocol.MessageRejectedResponse.Reasons rejectionReasons;
            Protocol.PushMessageHeader parsedRequest;
            Memory<byte> messageData;

            try
            {
                MessageBrokerRemoteClientReadPacketEvent.CreateReceived( client, traceId, request.Header ).Emit( client.Logger.ReadPacket );

                var exception = Protocol.AssertMinPayload( client, request.Header, Protocol.PushMessageHeader.Length );
                if ( exception is not null )
                    return await FinishInvalidRequestHandlingAsync( client, exception, traceId ).ConfigureAwait( false );

                parsedRequest = Protocol.PushMessageHeader.Parse( request.Data.Slice( 0, Protocol.PushMessageHeader.Length ) );
                var requestErrors = parsedRequest.StringifyErrors();
                if ( requestErrors.Count > 0 )
                {
                    var error = Protocol.ProtocolException( client, request.Header, requestErrors );
                    return await FinishInvalidRequestHandlingAsync( client, error, traceId ).ConfigureAwait( false );
                }

                messageData = request.Data.Slice( Protocol.PushMessageHeader.Length );
                MessageBrokerRemoteClientPushingMessageEvent.Create(
                        client,
                        traceId,
                        messageData.Length,
                        parsedRequest.ChannelId,
                        parsedRequest.Confirm )
                    .Emit( client.Logger.PushingMessage );

                using ( client.AcquireActiveLock( traceId, out var exc ) )
                {
                    if ( exc is not null )
                        return RequestResult.Done();

                    if ( client.PublishersByChannelId.TryGet( parsedRequest.ChannelId, out publisher ) )
                    {
                        rejectionReasons = publisher.Stream.PushMessage(
                            publisher,
                            request.PoolToken,
                            messageData,
                            ref messageId,
                            ref streamTraceId );
                    }
                    else
                        rejectionReasons = Protocol.MessageRejectedResponse.Reasons.NotBound;
                }
            }
            finally
            {
                if ( messageId is null )
                    request.PoolToken.Return( client, traceId );
            }

            var poolToken = default( MemoryPoolToken<byte> );
            try
            {
                Protocol.PacketHeader responseHeader;
                Memory<byte> responseData;

                if ( rejectionReasons != Protocol.MessageRejectedResponse.Reasons.None )
                {
                    var error = new MessageBrokerChannelPublisherBindingException(
                        client,
                        null,
                        null,
                        Resources.FailedToPushMessageToUnboundChannel( client.Id, client.Name, parsedRequest.ChannelId ) );

                    MessageBrokerRemoteClientErrorEvent.Create( client, traceId, error ).Emit( client.Logger.Error );
                    if ( ! parsedRequest.Confirm )
                        return FinishRequestHandling( client, traceId );

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

                    MessageBrokerRemoteClientReadPacketEvent.CreateAccepted( client, traceId, request.Header )
                        .Emit( client.Logger.ReadPacket );

                    using ( MessageBrokerStreamTraceEvent.CreateScope(
                        publisher.Stream,
                        streamTraceId,
                        MessageBrokerStreamTraceEventType.PushMessage ) )
                    {
                        MessageBrokerStreamClientTraceEvent.Create( publisher.Stream, streamTraceId, client, traceId )
                            .Emit( publisher.Stream.Logger.ClientTrace );

                        MessageBrokerStreamMessagePushedEvent.Create( publisher, streamTraceId, messageId.Value, messageData.Length )
                            .Emit( publisher.Stream.Logger.MessagePushed );
                    }

                    MessageBrokerRemoteClientMessagePushedEvent.Create( publisher, traceId, messageId.Value )
                        .Emit( client.Logger.MessagePushed );

                    if ( ! parsedRequest.Confirm )
                        return FinishRequestHandling( client, traceId );

                    var responseLength = Protocol.PacketHeader.Length + Protocol.MessageAcceptedResponse.Payload;
                    var response = new Protocol.MessageAcceptedResponse( messageId.Value );
                    poolToken = client.MemoryPool.Rent( responseLength, out responseData ).EnableClearing();
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
                    MessageBrokerRemoteClientErrorEvent.Create( client, traceId, client.DisposedException() ).Emit( client.Logger.Error );
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

    private static async ValueTask<RequestResult> HandlePingAsync(
        MessageBrokerRemoteClient client,
        Protocol.PacketHeader request,
        Protocol.PacketHeader response,
        ReadOnlyMemory<byte> responseData,
        ulong traceId)
    {
        using ( MessageBrokerRemoteClientTraceEvent.CreateScope( client, traceId, MessageBrokerRemoteClientTraceEventType.Ping ) )
        {
            MessageBrokerRemoteClientReadPacketEvent.CreateReceived( client, traceId, request ).Emit( client.Logger.ReadPacket );

            if ( request.Payload != Protocol.Endianness.VerificationPayload )
            {
                var error = Protocol.EndiannessPayloadException( client, request );
                return await FinishInvalidRequestHandlingAsync( client, error, traceId ).ConfigureAwait( false );
            }

            MessageBrokerRemoteClientReadPacketEvent.CreateAccepted( client, traceId, request ).Emit( client.Logger.ReadPacket );

            ManualResetValueTaskSource<bool> writerSource;
            using ( client.AcquireActiveLock( traceId, out var exc ) )
            {
                if ( exc is not null )
                    return RequestResult.Done();

                writerSource = client.WriterQueue.AcquireSource();
            }

            if ( ! await writerSource.GetTask().ConfigureAwait( false ) )
            {
                MessageBrokerRemoteClientErrorEvent.Create( client, traceId, client.DisposedException() ).Emit( client.Logger.Error );
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
            var error = Protocol.UnexpectedServerEndpointException( client, request );
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
        MessageBrokerRemoteClientErrorEvent.Create( client, traceId, exception ).Emit( client.Logger.Error );
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
        MessageBrokerRemoteClientErrorEvent.Create( client, traceId, exception ).Emit( client.Logger.Error );
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
