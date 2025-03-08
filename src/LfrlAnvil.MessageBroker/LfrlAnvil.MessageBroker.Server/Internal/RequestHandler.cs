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

                var bufferToken = request.BufferToken;
                try
                {
                    var result = request.Header.GetServerEndpoint() switch
                    {
                        MessageBrokerServerEndpoint.LinkChannelRequest => await HandleLinkChannelRequestAsync( client, contextId, request )
                            .ConfigureAwait( false ),
                        MessageBrokerServerEndpoint.UnlinkChannelRequest => await HandleUnlinkChannelRequestAsync(
                                client,
                                contextId,
                                request )
                            .ConfigureAwait( false ),
                        MessageBrokerServerEndpoint.SubscribeRequest => await HandleSubscribeRequestAsync( client, contextId, request )
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
                finally
                {
                    client.DisposeBufferToken( bufferToken );
                }
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

    // TODO: rename Link to Bind
    private static async ValueTask<HandleRequestResult> HandleLinkChannelRequestAsync(
        MessageBrokerRemoteClient client,
        ulong contextId,
        IncomingPacketToken request)
    {
        var bufferToken = request.BufferToken;
        var data = request.Data;

        client.Emit( MessageBrokerRemoteClientEvent.MessageReceived( client, request.Header, contextId ) );

        if ( request.Data.Length < Protocol.LinkChannelRequestHeader.Length )
        {
            client.Emit(
                MessageBrokerRemoteClientEvent.MessageRejected(
                    client,
                    request.Header,
                    Protocol.InvalidPacketLengthException( client, request.Header ),
                    contextId ) );

            return HandleRequestResult.Error();
        }

        _ = Protocol.LinkChannelRequestHeader.Parse( data.Slice( 0, Protocol.LinkChannelRequestHeader.Length ) );
        var name = TextEncoding.Parse( data.Slice( Protocol.LinkChannelRequestHeader.Length ) );
        if ( name.Exception is not null )
        {
            client.Emit( MessageBrokerRemoteClientEvent.MessageRejected( client, request.Header, name.Exception, contextId ) );
            return HandleRequestResult.Error();
        }

        Assume.IsNotNull( name.Value );
        if ( ! Defaults.NameLengthBounds.Contains( name.Value.Length ) )
        {
            client.Emit(
                MessageBrokerRemoteClientEvent.MessageRejected(
                    client,
                    request.Header,
                    Protocol.InvalidNameLengthException( client, request.Header, name.Value.Length ),
                    contextId ) );

            return HandleRequestResult.Error();
        }

        ChannelCollection.RegistrationResult result;
        using ( client.Server.AcquireLock() )
        {
            if ( client.Server.ShouldCancel )
                return HandleRequestResult.OwnerDisposed();

            result = ChannelCollection.Register( client.Server, name.Value );
        }

        var channel = result.Channel;
        if ( ! result.Exists )
            channel.Emit( MessageBrokerChannelEvent.Created( channel, client, contextId ) );

        ManualResetValueTaskSource<bool> writerSource;
        var rejectionReasons = Protocol.LinkChannelFailureResponse.Reasons.None;

        using ( client.AcquireLock() )
        {
            if ( client.ShouldCancel )
                return HandleRequestResult.OwnerDisposed();

            using ( channel.AcquireLock() )
            {
                if ( channel.ShouldCancel )
                    rejectionReasons = Protocol.LinkChannelFailureResponse.Reasons.LinkingCancelled;
                else if ( ! client.LinkedChannelsById.TryAdd( channel.Id, channel ) )
                    rejectionReasons = Protocol.LinkChannelFailureResponse.Reasons.AlreadyLinked;
                else
                    channel.LinkedClientsById.Add( client.Id, client );
            }

            // NOTE:
            // this may be an issue later on for batching packets
            // it's here because when server gets a feature that allows it to remove channels
            // then linked clients need to be notified that it happened
            // however, without 'reserving' writer source immediately here there may be a situation
            // where channel-removed-notification would acquire writer source BEFORE this
            // so the client would receive notification that it was unlinked from a channel it doesn't yet have registered locally
            // and then it would receive response to link-to-channel request
            // which would cause it to have the link erroneously active locally
            //
            // for batching, either the link itself needs a status (creating, active, disposed), for removing a single client-channel link
            // or response has to be prepared here, in client's lock (the issue is event emitting)
            // removing the whole channel may have to set channel status to disposing first, then remove all links, then the channel
            writerSource = client.MessageContextQueue.AcquireWriterSource();

            // TODO:
            // I think it would be better to do it all atomically:
            // lock server => get-or-add channel
            // lock client
            // lock channel => link client with channel
            // prepare response
            // acquire write source
            // unlock channel
            // unlock client
            // unlock server
            //
            // this would remove the need for 'x cancelled' rejections

            // TODO:
            // think about whether or not storing links directly in client/channel is necessary
            // maybe server could store a single map?
            // it might be an issue when e.g. client is disconnected
            // ^ relevant channels must be unlinked, enumerating all channels on the server feels wrong
            // server may have to have a map of links (just like with subscriptions)
            // ^ or does it...? both those maps might be unnecessary
            // ^ let server worry about clients/channels/queues
            // ^ and let clients/channels worry about links/subscriptions
        }

        Protocol.PacketHeader responseHeader;
        Memory<byte> responseData;

        if ( rejectionReasons != Protocol.LinkChannelFailureResponse.Reasons.None )
        {
            client.Emit(
                MessageBrokerRemoteClientEvent.MessageRejected(
                    client,
                    request.Header,
                    new MessageBrokerRemoteClientChannelLinkException(
                        client,
                        channel,
                        Resources.FailedToCreateClientChannelLink(
                            client.Id,
                            client.Name,
                            channel.Id,
                            channel.Name,
                            rejectionReasons ) ),
                    contextId ) );

            var responseLength = Protocol.PacketHeader.Length + Protocol.LinkChannelFailureResponse.Payload;
            var response = new Protocol.LinkChannelFailureResponse( rejectionReasons );
            if ( data.Length < responseLength )
                data = bufferToken.SetLength( responseLength );

            responseHeader = response.Header;
            responseData = data.Slice( 0, responseLength );
            response.Serialize( responseData );
        }
        else
        {
            channel.Emit( MessageBrokerChannelEvent.Linked( channel, client, contextId ) );
            client.Emit( MessageBrokerRemoteClientEvent.MessageAccepted( client, request.Header, contextId ) );

            var responseLength = Protocol.PacketHeader.Length + Protocol.ChannelLinkedResponse.Payload;
            var response = new Protocol.ChannelLinkedResponse( channel, ! result.Exists );
            if ( data.Length < responseLength )
                data = bufferToken.SetLength( responseLength );

            responseHeader = response.Header;
            responseData = data.Slice( 0, responseLength );
            response.Serialize( responseData );
        }

        if ( ! await writerSource.GetTask().ConfigureAwait( false ) )
            return HandleRequestResult.OwnerDisposed();

        var writeResult = await client.WriteAsync( responseHeader, responseData, contextId ).ConfigureAwait( false );
        if ( writeResult.Exception is not null )
            return HandleRequestResult.Error();

        using ( client.AcquireLock() )
        {
            if ( client.ShouldCancel )
                return HandleRequestResult.OwnerDisposed();

            client.MessageContextQueue.ResetOutgoingWriter( client, writerSource );
            return HandleRequestResult.Ok( client.MessageContextQueue.ContainsEnqueuedRequests() );
        }
    }

    private static async ValueTask<HandleRequestResult> HandleUnlinkChannelRequestAsync(
        MessageBrokerRemoteClient client,
        ulong contextId,
        IncomingPacketToken request)
    {
        var bufferToken = request.BufferToken;
        var data = request.Data;

        client.Emit( MessageBrokerRemoteClientEvent.MessageReceived( client, request.Header, contextId ) );

        var exc = Protocol.AssertPayload( client, request.Header, Protocol.UnlinkChannelRequest.Length );
        if ( exc is not null )
        {
            client.Emit( MessageBrokerRemoteClientEvent.MessageRejected( client, request.Header, exc, contextId ) );
            return HandleRequestResult.Error();
        }

        var parsedRequest = Protocol.UnlinkChannelRequest.Parse( data );
        var channel = ChannelCollection.TryGetById( client.Server, parsedRequest.ChannelId );

        var unlinkResult = MessageBrokerChannel.UnlinkResult.NoChanges;
        var rejectionReasons = Protocol.UnlinkChannelFailureResponse.Reasons.None;
        ManualResetValueTaskSource<bool> writerSource;

        using ( client.AcquireLock() )
        {
            if ( client.ShouldCancel )
                return HandleRequestResult.OwnerDisposed();

            if ( channel is null )
                rejectionReasons = Protocol.UnlinkChannelFailureResponse.Reasons.ClientNotLinked;
            else
            {
                using ( channel.AcquireLock() )
                {
                    unlinkResult = channel.BeginUnlink( client );
                    if ( unlinkResult == MessageBrokerChannel.UnlinkResult.NoChanges )
                        rejectionReasons = Protocol.UnlinkChannelFailureResponse.Reasons.ClientNotLinked;
                    else
                        client.LinkedChannelsById.Remove( parsedRequest.ChannelId );
                }
            }

            writerSource = client.MessageContextQueue.AcquireWriterSource();
        }

        Protocol.PacketHeader responseHeader;
        Memory<byte> responseData;

        if ( rejectionReasons != Protocol.UnlinkChannelFailureResponse.Reasons.None )
        {
            client.Emit(
                MessageBrokerRemoteClientEvent.MessageRejected(
                    client,
                    request.Header,
                    new MessageBrokerRemoteClientChannelLinkException(
                        client,
                        channel,
                        channel is null
                            ? Resources.FailedToUnlinkClientFromNonExistingChannel( client.Id, client.Name, parsedRequest.ChannelId )
                            : Resources.FailedToUnlinkClientFromChannel(
                                client.Id,
                                client.Name,
                                channel.Id,
                                channel.Name ) ),
                    contextId ) );

            var responseLength = Protocol.PacketHeader.Length + Protocol.UnlinkChannelFailureResponse.Payload;
            var response = new Protocol.UnlinkChannelFailureResponse( rejectionReasons );
            if ( data.Length < responseLength )
                data = bufferToken.SetLength( responseLength );

            responseHeader = response.Header;
            responseData = data.Slice( 0, responseLength );
            response.Serialize( responseData );
        }
        else
        {
            Assume.IsNotNull( channel );
            Assume.NotEquals( unlinkResult, MessageBrokerChannel.UnlinkResult.NoChanges );

            channel.Emit( MessageBrokerChannelEvent.Unlinked( channel, client, contextId ) );
            if ( unlinkResult == MessageBrokerChannel.UnlinkResult.Disposing )
                channel.DisposeDueToLackOfReferences();

            client.Emit( MessageBrokerRemoteClientEvent.MessageAccepted( client, request.Header, contextId ) );

            var responseLength = Protocol.PacketHeader.Length + Protocol.ChannelUnlinkedResponse.Payload;
            var response = new Protocol.ChannelUnlinkedResponse( unlinkResult == MessageBrokerChannel.UnlinkResult.Disposing );
            if ( data.Length < responseLength )
                data = bufferToken.SetLength( responseLength );

            responseHeader = response.Header;
            responseData = data.Slice( 0, responseLength );
            response.Serialize( responseData );
        }

        if ( ! await writerSource.GetTask().ConfigureAwait( false ) )
            return HandleRequestResult.OwnerDisposed();

        var writeResult = await client.WriteAsync( responseHeader, responseData, contextId ).ConfigureAwait( false );
        if ( writeResult.Exception is not null )
            return HandleRequestResult.Error();

        using ( client.AcquireLock() )
        {
            if ( client.ShouldCancel )
                return HandleRequestResult.OwnerDisposed();

            client.MessageContextQueue.ResetOutgoingWriter( client, writerSource );
            return HandleRequestResult.Ok( client.MessageContextQueue.ContainsEnqueuedRequests() );
        }
    }

    private static async ValueTask<HandleRequestResult> HandleSubscribeRequestAsync(
        MessageBrokerRemoteClient client,
        ulong contextId,
        IncomingPacketToken request)
    {
        var bufferToken = request.BufferToken;
        var data = request.Data;

        client.Emit( MessageBrokerRemoteClientEvent.MessageReceived( client, request.Header, contextId ) );

        if ( request.Data.Length < Protocol.SubscribeRequestHeader.Length )
        {
            client.Emit(
                MessageBrokerRemoteClientEvent.MessageRejected(
                    client,
                    request.Header,
                    Protocol.InvalidPacketLengthException( client, request.Header ),
                    contextId ) );

            return HandleRequestResult.Error();
        }

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
                    Protocol.InvalidNameLengthException( client, request.Header, channelName.Value.Length ),
                    contextId ) );

            return HandleRequestResult.Error();
        }

        ManualResetValueTaskSource<bool> writerSource;
        MessageBrokerSubscription? subscription = null;
        var rejectionReasons = Protocol.SubscribeFailureResponse.Reasons.None;
        ChannelCollection.RegistrationResult? result;

        using ( client.Server.AcquireLock() )
        {
            if ( client.Server.ShouldCancel )
                return HandleRequestResult.OwnerDisposed();

            result = ChannelCollection.TryRegister( client.Server, channelName.Value, parsedRequest.CreateChannelIfNotExists );
            if ( result is null )
            {
                rejectionReasons = Protocol.SubscribeFailureResponse.Reasons.ChannelDoesNotExist;
                using ( client.AcquireLock() )
                {
                    if ( client.ShouldCancel )
                        return HandleRequestResult.OwnerDisposed();

                    writerSource = client.MessageContextQueue.AcquireWriterSource();
                }
            }
            else
            {
                var channel = result.Value.Channel;
                using ( client.AcquireLock() )
                {
                    if ( client.ShouldCancel )
                        return HandleRequestResult.OwnerDisposed();

                    using ( channel.AcquireLock() )
                    {
                        if ( channel.ShouldCancel )
                            rejectionReasons = Protocol.SubscribeFailureResponse.Reasons.SubscribingCancelled;
                        else if ( ! client.SubscribeTo( channel, out subscription ) )
                            rejectionReasons = Protocol.SubscribeFailureResponse.Reasons.AlreadySubscribed;
                    }

                    writerSource = client.MessageContextQueue.AcquireWriterSource();
                }
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
                    new MessageBrokerRemoteClientSubscriptionException(
                        client,
                        result?.Channel,
                        subscription,
                        Resources.FailedToCreateSubscription(
                            client.Id,
                            client.Name,
                            result?.Channel.Id,
                            channelName.Value,
                            rejectionReasons ) ),
                    contextId ) );

            var responseLength = Protocol.PacketHeader.Length + Protocol.SubscribeFailureResponse.Payload;
            var response = new Protocol.SubscribeFailureResponse( rejectionReasons );
            if ( data.Length < responseLength )
                data = bufferToken.SetLength( responseLength );

            responseHeader = response.Header;
            responseData = data.Slice( 0, responseLength );
            response.Serialize( responseData );
        }
        else
        {
            Assume.IsNotNull( result );
            Assume.IsNotNull( subscription );

            if ( ! result.Value.Exists )
                result.Value.Channel.Emit( MessageBrokerChannelEvent.Created( result.Value.Channel, client, contextId ) );

            subscription.Emit( MessageBrokerSubscriptionEvent.Created( subscription, contextId ) );
            client.Emit( MessageBrokerRemoteClientEvent.MessageAccepted( client, request.Header, contextId ) );

            var responseLength = Protocol.PacketHeader.Length + Protocol.SubscribedResponse.Payload;
            var response = new Protocol.SubscribedResponse( ! result.Value.Exists, result.Value.Channel.Id );
            if ( data.Length < responseLength )
                data = bufferToken.SetLength( responseLength );

            responseHeader = response.Header;
            responseData = data.Slice( 0, responseLength );
            response.Serialize( responseData );
        }

        if ( ! await writerSource.GetTask().ConfigureAwait( false ) )
            return HandleRequestResult.OwnerDisposed();

        var writeResult = await client.WriteAsync( responseHeader, responseData, contextId ).ConfigureAwait( false );
        if ( writeResult.Exception is not null )
            return HandleRequestResult.Error();

        using ( client.AcquireLock() )
        {
            if ( client.ShouldCancel )
                return HandleRequestResult.OwnerDisposed();

            client.MessageContextQueue.ResetOutgoingWriter( client, writerSource );
            return HandleRequestResult.Ok( client.MessageContextQueue.ContainsEnqueuedRequests() );
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
        if ( result.Exception is not null )
            return HandleRequestResult.Error();

        using ( client.AcquireLock() )
        {
            if ( client.ShouldCancel )
                return HandleRequestResult.OwnerDisposed();

            client.MessageContextQueue.ResetOutgoingWriter( client, writerSource );
            return HandleRequestResult.Ok( client.MessageContextQueue.ContainsEnqueuedRequests() );
        }
    }

    private static HandleRequestResult HandleUnexpectedRequest(MessageBrokerRemoteClient client, Protocol.PacketHeader request)
    {
        client.HandleUnexpectedEndpoint( request );
        return HandleRequestResult.Error();
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
