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
        var pingResponse = Protocol.PingResponse.Create();
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
                    MessageBrokerServerEndpoint.BindRequest => await HandleBindRequestAsync( client, contextId, request )
                        .ConfigureAwait( false ),
                    MessageBrokerServerEndpoint.UnbindRequest => await HandleUnbindRequestAsync( client, contextId, request )
                        .ConfigureAwait( false ),
                    MessageBrokerServerEndpoint.SubscribeRequest => await HandleSubscribeRequestAsync( client, contextId, request )
                        .ConfigureAwait( false ),
                    MessageBrokerServerEndpoint.UnsubscribeRequest => await HandleUnsubscribeRequestAsync( client, contextId, request )
                        .ConfigureAwait( false ),
                    MessageBrokerServerEndpoint.MessageRequest => await HandleMessageRequestAsync( client, contextId, request )
                        .ConfigureAwait( false ),
                    MessageBrokerServerEndpoint.PingRequest => await HandlePingRequestAsync(
                            client,
                            contextId,
                            request.Header,
                            pingResponse,
                            pingResponseData )
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

    private static async ValueTask<HandleRequestResult> HandleBindRequestAsync(
        MessageBrokerRemoteClient client,
        ulong contextId,
        IncomingPacketToken request)
    {
        var poolToken = request.PoolToken;
        try
        {
            client.Emit( MessageBrokerRemoteClientEvent.MessageReceived( client, request.Header, contextId ) );

            var exc = Protocol.AssertMinPayload( client, request.Header, Protocol.BindRequestHeader.Length );
            if ( exc is not null )
            {
                client.Emit( MessageBrokerRemoteClientEvent.MessageRejected( client, request.Header, exc, contextId ) );
                return HandleRequestResult.Error();
            }

            var data = request.Data;
            var parsedRequestHeader = Protocol.BindRequestHeader.Parse( data.Slice( 0, Protocol.BindRequestHeader.Length ) );
            var maxChannelNameLength = data.Length - Protocol.BindRequestHeader.Length;

            if ( parsedRequestHeader.ChannelNameLength < 0 || parsedRequestHeader.ChannelNameLength > maxChannelNameLength )
            {
                client.Emit(
                    MessageBrokerRemoteClientEvent.MessageRejected(
                        client,
                        request.Header,
                        Protocol.InvalidBinaryChannelNameLengthException(
                            client,
                            request.Header,
                            parsedRequestHeader.ChannelNameLength,
                            maxChannelNameLength ),
                        contextId ) );

                return HandleRequestResult.Error();
            }

            var channelName = TextEncoding.Parse( data.Slice( Protocol.BindRequestHeader.Length, parsedRequestHeader.ChannelNameLength ) );
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

            var queueName = TextEncoding.Parse( data.Slice( Protocol.BindRequestHeader.Length + parsedRequestHeader.ChannelNameLength ) );
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

            bool channelCreated;
            var queueCreated = false;
            MessageBrokerChannel? channel;
            MessageBrokerQueue? queue = null;
            MessageBrokerChannelBinding? binding = null;
            Protocol.BindFailureResponse.Reasons rejectionReasons;
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
                        rejectionReasons = client.BindUnsafe(
                            channel,
                            channelCreated,
                            queueName.Value,
                            ref binding,
                            ref queue,
                            ref queueCreated );
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

            if ( rejectionReasons != Protocol.BindFailureResponse.Reasons.None )
            {
                client.Emit(
                    MessageBrokerRemoteClientEvent.MessageRejected(
                        client,
                        request.Header,
                        new MessageBrokerChannelBindingException(
                            client,
                            channel,
                            binding,
                            Resources.FailedToCreateChannelBinding(
                                client.Id,
                                client.Name,
                                channel.Id,
                                channel.Name,
                                rejectionReasons ) ),
                        contextId ) );

                var responseLength = Protocol.PacketHeader.Length + Protocol.BindFailureResponse.Payload;
                var response = new Protocol.BindFailureResponse( rejectionReasons );
                Assume.IsGreaterThanOrEqualTo( data.Length, responseLength );

                responseHeader = response.Header;
                responseData = data.Slice( 0, responseLength );
                response.Serialize( responseData );
            }
            else
            {
                Assume.IsNotNull( channel );
                Assume.IsNotNull( queue );
                Assume.IsNotNull( binding );

                if ( channelCreated )
                    channel.Emit( MessageBrokerChannelEvent.Created( channel, client, contextId ) );

                if ( queueCreated )
                    queue.Emit( MessageBrokerQueueEvent.Created( queue, binding, contextId ) );

                binding.Emit( MessageBrokerChannelBindingEvent.Created( binding, contextId ) );
                client.Emit( MessageBrokerRemoteClientEvent.MessageAccepted( client, request.Header, contextId ) );

                var responseLength = Protocol.PacketHeader.Length + Protocol.BoundResponse.Payload;
                var response = new Protocol.BoundResponse( channelCreated, queueCreated, channel.Id, queue.Id );
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

    private static async ValueTask<HandleRequestResult> HandleUnbindRequestAsync(
        MessageBrokerRemoteClient client,
        ulong contextId,
        IncomingPacketToken request)
    {
        var poolToken = request.PoolToken;
        try
        {
            client.Emit( MessageBrokerRemoteClientEvent.MessageReceived( client, request.Header, contextId ) );

            var exc = Protocol.AssertPayload( client, request.Header, Protocol.UnbindRequest.Length );
            if ( exc is not null )
            {
                client.Emit( MessageBrokerRemoteClientEvent.MessageRejected( client, request.Header, exc, contextId ) );
                return HandleRequestResult.Error();
            }

            var data = request.Data;
            var parsedRequest = Protocol.UnbindRequest.Parse( data );

            var disposingChannel = false;
            var disposingQueue = false;
            MessageBrokerChannelBinding? binding = null;
            MessageBrokerQueue? queue = null;
            Protocol.UnbindFailureResponse.Reasons rejectionReasons;

            var channel = ChannelCollection.TryGetById( client.Server, parsedRequest.ChannelId );
            if ( channel is null )
                rejectionReasons = Protocol.UnbindFailureResponse.Reasons.NotBound;
            else
            {
                using ( client.AcquireLock() )
                {
                    if ( client.ShouldCancel )
                        return HandleRequestResult.OwnerDisposed();

                    rejectionReasons = client.BeginUnbindUnsafe(
                        channel,
                        ref binding,
                        ref queue,
                        ref disposingChannel,
                        ref disposingQueue );
                }
            }

            Protocol.PacketHeader responseHeader;
            Memory<byte> responseData;

            if ( rejectionReasons != Protocol.UnbindFailureResponse.Reasons.None )
            {
                client.Emit(
                    MessageBrokerRemoteClientEvent.MessageRejected(
                        client,
                        request.Header,
                        new MessageBrokerChannelBindingException(
                            client,
                            channel,
                            binding,
                            channel is null
                                ? Resources.FailedToUnbindFromNonExistingChannel( client.Id, client.Name, parsedRequest.ChannelId )
                                : Resources.FailedToUnbindFromChannel(
                                    client.Id,
                                    client.Name,
                                    channel.Id,
                                    channel.Name ) ),
                        contextId ) );

                var responseLength = Protocol.PacketHeader.Length + Protocol.UnbindFailureResponse.Payload;
                var response = new Protocol.UnbindFailureResponse( rejectionReasons );
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
                Assume.IsNotNull( binding );

                binding.Emit( MessageBrokerChannelBindingEvent.Disposing( binding, contextId ) );
                if ( disposingQueue )
                    await queue.DisposeDueToLackOfReferencesAsync( ignoreProcessorTask: false ).ConfigureAwait( false );

                if ( disposingChannel )
                    channel.DisposeDueToLackOfReferences();

                binding.EndDisposing();
                client.Emit( MessageBrokerRemoteClientEvent.MessageAccepted( client, request.Header, contextId ) );

                var responseLength = Protocol.PacketHeader.Length + Protocol.UnboundResponse.Payload;
                var response = new Protocol.UnboundResponse( disposingChannel, disposingQueue );
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

    private static async ValueTask<HandleRequestResult> HandleSubscribeRequestAsync(
        MessageBrokerRemoteClient client,
        ulong contextId,
        IncomingPacketToken request)
    {
        var poolToken = request.PoolToken;
        try
        {
            client.Emit( MessageBrokerRemoteClientEvent.MessageReceived( client, request.Header, contextId ) );

            var exc = Protocol.AssertMinPayload( client, request.Header, Protocol.SubscribeRequestHeader.Length );
            if ( exc is not null )
            {
                client.Emit( MessageBrokerRemoteClientEvent.MessageRejected( client, request.Header, exc, contextId ) );
                return HandleRequestResult.Error();
            }

            var data = request.Data;
            var parsedRequest = Protocol.SubscribeRequestHeader.Parse( data.Slice( 0, Protocol.SubscribeRequestHeader.Length ) );
            var channelName = TextEncoding.Parse( data.Slice( Protocol.SubscribeRequestHeader.Length ) );
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

            var channelCreated = false;
            MessageBrokerChannel? channel;
            MessageBrokerSubscription? subscription = null;
            Protocol.SubscribeFailureResponse.Reasons rejectionReasons;
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
                        parsedRequest.CreateChannelIfNotExists,
                        ref channelCreated );

                    if ( channel is null )
                        rejectionReasons = Protocol.SubscribeFailureResponse.Reasons.ChannelDoesNotExist;
                    else
                    {
                        try
                        {
                            rejectionReasons = client.SubscribeUnsafe( channel, ref subscription );
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

            if ( rejectionReasons != Protocol.SubscribeFailureResponse.Reasons.None )
            {
                client.Emit(
                    MessageBrokerRemoteClientEvent.MessageRejected(
                        client,
                        request.Header,
                        new MessageBrokerSubscriptionException(
                            client,
                            channel,
                            subscription,
                            Resources.FailedToCreateSubscription(
                                client.Id,
                                client.Name,
                                channel?.Id,
                                channelName.Value,
                                rejectionReasons ) ),
                        contextId ) );

                var responseLength = Protocol.PacketHeader.Length + Protocol.SubscribeFailureResponse.Payload;
                var response = new Protocol.SubscribeFailureResponse( rejectionReasons );
                if ( data.Length < responseLength )
                    poolToken.SetLength( responseLength, out data );

                responseHeader = response.Header;
                responseData = data.Slice( 0, responseLength );
                response.Serialize( responseData );
            }
            else
            {
                Assume.IsNotNull( channel );
                Assume.IsNotNull( subscription );

                if ( channelCreated )
                    channel.Emit( MessageBrokerChannelEvent.Created( channel, client, contextId ) );

                subscription.Emit( MessageBrokerSubscriptionEvent.Created( subscription, contextId ) );
                client.Emit( MessageBrokerRemoteClientEvent.MessageAccepted( client, request.Header, contextId ) );

                var responseLength = Protocol.PacketHeader.Length + Protocol.SubscribedResponse.Payload;
                var response = new Protocol.SubscribedResponse( channelCreated, channel.Id );
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

    private static async ValueTask<HandleRequestResult> HandleUnsubscribeRequestAsync(
        MessageBrokerRemoteClient client,
        ulong contextId,
        IncomingPacketToken request)
    {
        var poolToken = request.PoolToken;
        try
        {
            client.Emit( MessageBrokerRemoteClientEvent.MessageReceived( client, request.Header, contextId ) );

            var exc = Protocol.AssertPayload( client, request.Header, Protocol.UnsubscribeRequest.Length );
            if ( exc is not null )
            {
                client.Emit( MessageBrokerRemoteClientEvent.MessageRejected( client, request.Header, exc, contextId ) );
                return HandleRequestResult.Error();
            }

            var data = request.Data;
            var parsedRequest = Protocol.UnsubscribeRequest.Parse( data );

            var disposingChannel = false;
            MessageBrokerSubscription? subscription = null;
            Protocol.UnsubscribeFailureResponse.Reasons rejectionReasons;

            var channel = ChannelCollection.TryGetById( client.Server, parsedRequest.ChannelId );
            if ( channel is null )
                rejectionReasons = Protocol.UnsubscribeFailureResponse.Reasons.NotSubscribed;
            else
            {
                using ( client.AcquireLock() )
                {
                    if ( client.ShouldCancel )
                        return HandleRequestResult.OwnerDisposed();

                    rejectionReasons = client.BeginUnsubscribeUnsafe( channel, ref subscription, ref disposingChannel );
                }
            }

            Protocol.PacketHeader responseHeader;
            Memory<byte> responseData;

            if ( rejectionReasons != Protocol.UnsubscribeFailureResponse.Reasons.None )
            {
                client.Emit(
                    MessageBrokerRemoteClientEvent.MessageRejected(
                        client,
                        request.Header,
                        new MessageBrokerSubscriptionException(
                            client,
                            channel,
                            subscription,
                            channel is null
                                ? Resources.FailedToUnsubscribeFromNonExistingChannel( client.Id, client.Name, parsedRequest.ChannelId )
                                : Resources.FailedToUnsubscribeFromChannel(
                                    client.Id,
                                    client.Name,
                                    channel.Id,
                                    channel.Name ) ),
                        contextId ) );

                var responseLength = Protocol.PacketHeader.Length + Protocol.UnsubscribeFailureResponse.Payload;
                var response = new Protocol.UnsubscribeFailureResponse( rejectionReasons );
                if ( data.Length < responseLength )
                    poolToken.SetLength( responseLength, out data );

                responseHeader = response.Header;
                responseData = data.Slice( 0, responseLength );
                response.Serialize( responseData );
            }
            else
            {
                Assume.IsNotNull( channel );
                Assume.IsNotNull( subscription );

                subscription.Emit( MessageBrokerSubscriptionEvent.Disposing( subscription, contextId ) );
                if ( disposingChannel )
                    channel.DisposeDueToLackOfReferences();

                subscription.EndDisposing();
                client.Emit( MessageBrokerRemoteClientEvent.MessageAccepted( client, request.Header, contextId ) );

                var responseLength = Protocol.PacketHeader.Length + Protocol.UnsubscribedResponse.Payload;
                var response = new Protocol.UnsubscribedResponse( disposingChannel );
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

    private static async ValueTask<HandleRequestResult> HandleMessageRequestAsync(
        MessageBrokerRemoteClient client,
        ulong contextId,
        IncomingPacketToken request)
    {
        ulong? messageId = null;
        MessageBrokerChannelBinding? binding;
        Protocol.MessageRejectedResponse.Reasons rejectionReasons;
        Protocol.MessageRequestHeader parsedRequest;

        try
        {
            client.Emit( MessageBrokerRemoteClientEvent.MessageReceived( client, request.Header, contextId ) );

            var exc = Protocol.AssertMinPayload( client, request.Header, Protocol.MessageRequestHeader.Length );
            if ( exc is not null )
            {
                client.Emit( MessageBrokerRemoteClientEvent.MessageRejected( client, request.Header, exc, contextId ) );
                return HandleRequestResult.Error();
            }

            parsedRequest = Protocol.MessageRequestHeader.Parse( request.Data.Slice( 0, Protocol.MessageRequestHeader.Length ) );

            using ( client.AcquireLock() )
            {
                if ( client.ShouldCancel )
                    return HandleRequestResult.OwnerDisposed();

                if ( client.BindingsByChannelId.TryGetValue( parsedRequest.ChannelId, out binding ) )
                {
                    rejectionReasons = binding.Queue.Enqueue(
                        binding,
                        request.PoolToken,
                        request.Data.Slice( Protocol.MessageRequestHeader.Length ),
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
                        new MessageBrokerChannelBindingException(
                            client,
                            null,
                            null,
                            Resources.FailedToAddMessageToUnboundChannel( client.Id, client.Name, parsedRequest.ChannelId ) ),
                        contextId ) );

                var responseLength = Protocol.PacketHeader.Length + Protocol.MessageRejectedResponse.Payload;
                var response = new Protocol.MessageRejectedResponse( rejectionReasons );
                poolToken = client.MemoryPool.Rent( responseLength, out responseData ).EnableClearing();
                responseHeader = response.Header;
                response.Serialize( responseData );
            }
            else
            {
                Assume.IsNotNull( binding );
                Assume.IsNotNull( messageId );

                client.Emit( MessageBrokerRemoteClientEvent.MessageAccepted( client, request.Header, contextId ) );

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

    private static async ValueTask<HandleRequestResult> HandlePingRequestAsync(
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
