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
                    Protocol.InvalidNameLengthException( client, request.Header, name.Value.Length ) ) );

            return HandleRequestResult.Error();
        }

        var result = ChannelCollection.Register( client.Server, name.Value );
        if ( result.Exception is not null )
        {
            if ( result.Exception is MessageBrokerServerDisposedException )
                return HandleRequestResult.OwnerDisposed();

            client.Emit( MessageBrokerRemoteClientEvent.Unexpected( client, result.Exception ) );
            return HandleRequestResult.Error();
        }

        var channel = result.Value.Channel;
        if ( ! result.Value.Exists )
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
                    rejectionReasons = Protocol.LinkChannelFailureResponse.Reasons.LinkCancelled;
                else if ( ! client.LinkedChannelsById.TryAdd( channel.Id, channel ) )
                    rejectionReasons = Protocol.LinkChannelFailureResponse.Reasons.AlreadyLinked;
                else
                    channel.LinkedClientsById.Add( client.Id, client );
            }

            writerSource = client.MessageContextQueue.AcquireWriterSource();
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
            var response = new Protocol.ChannelLinkedResponse( channel, ! result.Value.Exists );
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
