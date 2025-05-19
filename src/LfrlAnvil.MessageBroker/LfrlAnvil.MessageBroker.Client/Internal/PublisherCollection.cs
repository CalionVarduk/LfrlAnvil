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
            MessageBrokerClientBindingPublisherEvent.Create( client, traceId, channelName, streamName ?? channelName )
                .Emit( client.Logger.BindingPublisher );

            ManualResetValueTaskSource<IncomingPacketToken> responseSource;
            Protocol.BindPublisherRequest request;

            var poolToken = default( MemoryPoolToken<byte> );
            try
            {
                request = new Protocol.BindPublisherRequest( channelName, streamName );
                poolToken = client.MemoryPool.Rent( request.Length, out var buffer ).EnableClearing();
                request.Serialize( buffer, reverseEndianness );

                ManualResetValueTaskSource<bool> writerSource;
                using ( client.AcquireActiveLock( traceId, out var exc ) )
                {
                    if ( exc is not null )
                        return exc;

                    writerSource = client.MessageContextQueue.AcquireWriterSource();
                }

                if ( ! await writerSource.GetTask().ConfigureAwait( false ) )
                    return client.EmitError( client.DisposedException(), traceId );

                using ( client.AcquireActiveLock( traceId, out var exc ) )
                {
                    if ( exc is not null )
                        return exc;

                    client.EventScheduler.PausePing();
                    responseSource = client.MessageContextQueue.AcquirePendingResponseSource();
                }

                var result = await client.WriteAsync( request.Header, buffer, traceId ).ConfigureAwait( false );
                if ( result.Exception is not null )
                {
                    await client.DisposeAsync( traceId ).ConfigureAwait( false );
                    return result.Exception;
                }

                using ( client.AcquireActiveLock( traceId, out var exc ) )
                {
                    if ( exc is not null )
                        return exc;

                    client.MessageContextQueue.ResetOutgoingWriter( client, writerSource );
                    client.MessageContextQueue.ActivatePendingResponseSource( client, responseSource );
                    client.EventScheduler.SchedulePing( client );
                }
            }
            catch ( Exception exc )
            {
                MessageBrokerClientErrorEvent.Create( client, traceId, exc ).Emit( client.Logger.Error );
                await client.DisposeAsync( traceId ).ConfigureAwait( false );
                return exc;
            }
            finally
            {
                poolToken.Return( client, traceId );
            }

            var response = await responseSource.GetTask().ConfigureAwait( false );
            try
            {
                if ( response.Type != IncomingPacketToken.Result.Ok )
                {
                    if ( response.Type == IncomingPacketToken.Result.Disposed )
                        return client.EmitError( client.DisposedException(), traceId );

                    var error = new MessageBrokerClientResponseTimeoutException( client, request.Header.GetServerEndpoint() );
                    MessageBrokerClientErrorEvent.Create( client, traceId, error ).Emit( client.Logger.Error );
                    await client.DisposeAsync( traceId ).ConfigureAwait( false );
                    return error;
                }

                using ( client.AcquireActiveLock( traceId, out var exc ) )
                {
                    if ( exc is not null )
                        return exc;

                    client.MessageContextQueue.ResetPendingResponseSource( responseSource );
                }

                switch ( response.Header.GetClientEndpoint() )
                {
                    case MessageBrokerClientEndpoint.PublisherBoundResponse:
                    {
                        MessageBrokerClientReadPacketEvent.CreateReceived( client, traceId, response.Header )
                            .Emit( client.Logger.ReadPacket );

                        var exception = Protocol.AssertPayload( client, response.Header, Protocol.PublisherBoundResponse.Length );
                        if ( exception is not null )
                        {
                            MessageBrokerClientErrorEvent.Create( client, traceId, exception ).Emit( client.Logger.Error );
                            await client.DisposeAsync( traceId ).ConfigureAwait( false );
                            return exception;
                        }

                        var parsedResponse = Protocol.PublisherBoundResponse.Parse( response.Data, reverseEndianness );
                        var errors = parsedResponse.StringifyErrors();

                        if ( errors.Count > 0 )
                        {
                            var error = client.EmitError( Protocol.ProtocolException( client, response.Header, errors ), traceId );
                            await client.DisposeAsync( traceId ).ConfigureAwait( false );
                            return error;
                        }

                        MessageBrokerBindPublisherResult bindResult;
                        using ( client.AcquireActiveLock( traceId, out var exc ) )
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

                        MessageBrokerClientReadPacketEvent.CreateAccepted( client, traceId, response.Header )
                            .Emit( client.Logger.ReadPacket );

                        MessageBrokerClientPublisherChangeEvent.CreateBound( client, traceId, bindResult.Publisher )
                            .Emit( client.Logger.PublisherChange );

                        return bindResult;
                    }
                    case MessageBrokerClientEndpoint.BindPublisherFailureResponse:
                    {
                        MessageBrokerClientReadPacketEvent.CreateReceived( client, traceId, response.Header )
                            .Emit( client.Logger.ReadPacket );

                        var exception = Protocol.AssertPayload( client, response.Header, Protocol.BindPublisherFailureResponse.Length );
                        if ( exception is not null )
                        {
                            MessageBrokerClientErrorEvent.Create( client, traceId, exception ).Emit( client.Logger.Error );
                            await client.DisposeAsync( traceId ).ConfigureAwait( false );
                            return exception;
                        }

                        var parsedResponse = Protocol.BindPublisherFailureResponse.Parse( response.Data );
                        MessageBrokerClientReadPacketEvent.CreateAccepted( client, traceId, response.Header )
                            .Emit( client.Logger.ReadPacket );

                        return client.EmitError(
                            Protocol.RequestException( client, request.Header, parsedResponse.StringifyErrors( channelName ) ),
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
                MessageBrokerClientErrorEvent.Create( client, traceId, exc ).Emit( client.Logger.Error );
                await client.DisposeAsync( traceId ).ConfigureAwait( false );
                return exc;
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
            MessageBrokerClientPublisherChangeEvent.CreateUnbinding( client, traceId, publisher ).Emit( client.Logger.PublisherChange );
            ManualResetValueTaskSource<IncomingPacketToken> responseSource;
            Protocol.UnbindPublisherRequest request;

            var poolToken = default( MemoryPoolToken<byte> );
            try
            {
                request = new Protocol.UnbindPublisherRequest( publisher.ChannelId );
                poolToken = client.MemoryPool.Rent( Protocol.UnbindPublisherRequest.Length, out var buffer ).EnableClearing();
                request.Serialize( buffer, reverseEndianness );

                ManualResetValueTaskSource<bool> writerSource;
                using ( client.AcquireActiveLock( traceId, out var exc ) )
                {
                    if ( exc is not null )
                        return exc;

                    writerSource = client.MessageContextQueue.AcquireWriterSource();
                }

                if ( ! await writerSource.GetTask().ConfigureAwait( false ) )
                    return client.EmitError( client.DisposedException(), traceId );

                using ( client.AcquireActiveLock( traceId, out var exc ) )
                {
                    if ( exc is not null )
                        return exc;

                    client.EventScheduler.PausePing();
                    responseSource = client.MessageContextQueue.AcquirePendingResponseSource();
                }

                var result = await client.WriteAsync( request.Header, buffer, traceId ).ConfigureAwait( false );
                if ( result.Exception is not null )
                {
                    await client.DisposeAsync( traceId ).ConfigureAwait( false );
                    return result.Exception;
                }

                using ( client.AcquireActiveLock( traceId, out var exc ) )
                {
                    if ( exc is not null )
                        return exc;

                    client.MessageContextQueue.ResetOutgoingWriter( client, writerSource );
                    client.MessageContextQueue.ActivatePendingResponseSource( client, responseSource );
                    client.EventScheduler.SchedulePing( client );
                }
            }
            catch ( Exception exc )
            {
                MessageBrokerClientErrorEvent.Create( client, traceId, exc ).Emit( client.Logger.Error );
                await client.DisposeAsync( traceId ).ConfigureAwait( false );
                return exc;
            }
            finally
            {
                poolToken.Return( client, traceId );
            }

            var response = await responseSource.GetTask().ConfigureAwait( false );
            try
            {
                if ( response.Type != IncomingPacketToken.Result.Ok )
                {
                    if ( response.Type == IncomingPacketToken.Result.Disposed )
                        return client.EmitError( client.DisposedException(), traceId );

                    var error = new MessageBrokerClientResponseTimeoutException( client, request.Header.GetServerEndpoint() );
                    MessageBrokerClientErrorEvent.Create( client, traceId, error ).Emit( client.Logger.Error );
                    await client.DisposeAsync( traceId ).ConfigureAwait( false );
                    return error;
                }

                using ( client.AcquireActiveLock( traceId, out var exc ) )
                {
                    if ( exc is not null )
                        return exc;

                    client.MessageContextQueue.ResetPendingResponseSource( responseSource );
                }

                switch ( response.Header.GetClientEndpoint() )
                {
                    case MessageBrokerClientEndpoint.PublisherUnboundResponse:
                    {
                        MessageBrokerClientReadPacketEvent.CreateReceived( client, traceId, response.Header )
                            .Emit( client.Logger.ReadPacket );

                        var exception = Protocol.AssertPayload( client, response.Header, Protocol.PublisherUnboundResponse.Length );
                        if ( exception is not null )
                        {
                            MessageBrokerClientErrorEvent.Create( client, traceId, exception ).Emit( client.Logger.Error );
                            await client.DisposeAsync( traceId ).ConfigureAwait( false );
                            return exception;
                        }

                        var parsedResponse = Protocol.PublisherUnboundResponse.Parse( response.Data );

                        using ( client.AcquireActiveLock( traceId, out var exc ) )
                        {
                            if ( exc is not null )
                                return exc;

                            client.PublisherCollection._store.Remove( publisher.ChannelId, publisher.ChannelName );
                        }

                        MessageBrokerClientReadPacketEvent.CreateAccepted( client, traceId, response.Header )
                            .Emit( client.Logger.ReadPacket );

                        MessageBrokerClientPublisherChangeEvent.CreateUnbound( client, traceId, publisher )
                            .Emit( client.Logger.PublisherChange );

                        return MessageBrokerUnbindPublisherResult.Create( parsedResponse.ChannelRemoved, parsedResponse.StreamRemoved );
                    }
                    case MessageBrokerClientEndpoint.UnbindPublisherFailureResponse:
                    {
                        MessageBrokerClientReadPacketEvent.CreateReceived( client, traceId, response.Header )
                            .Emit( client.Logger.ReadPacket );

                        var exception = Protocol.AssertPayload( client, response.Header, Protocol.UnbindPublisherFailureResponse.Length );
                        if ( exception is not null )
                        {
                            MessageBrokerClientErrorEvent.Create( client, traceId, exception ).Emit( client.Logger.Error );
                            await client.DisposeAsync( traceId ).ConfigureAwait( false );
                            return exception;
                        }

                        var parsedResponse = Protocol.UnbindPublisherFailureResponse.Parse( response.Data );
                        MessageBrokerClientReadPacketEvent.CreateAccepted( client, traceId, response.Header )
                            .Emit( client.Logger.ReadPacket );

                        return client.EmitError(
                            Protocol.RequestException( client, request.Header, parsedResponse.StringifyErrors( publisher ) ),
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
                MessageBrokerClientErrorEvent.Create( client, traceId, exc ).Emit( client.Logger.Error );
                await client.DisposeAsync( traceId ).ConfigureAwait( false );
                return exc;
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
            var messageLength = unchecked( buffer.Length - Protocol.PushMessageHeader.Length );
            MessageBrokerClientMessagePushingEvent.Create( client, traceId, context.Publisher, messageLength, confirm )
                .Emit( client.Logger.MessagePushing );

            ManualResetValueTaskSource<IncomingPacketToken>? responseSource = null;
            Protocol.PushMessageHeader request;

            try
            {
                request = new Protocol.PushMessageHeader( context.Publisher.ChannelId, messageLength, confirm );
                request.Serialize( buffer.Slice( 0, Protocol.PushMessageHeader.Length ), reverseEndianness );

                ManualResetValueTaskSource<bool> writerSource;
                using ( client.AcquireActiveLock( traceId, out var exc ) )
                {
                    if ( exc is not null )
                        return exc;

                    writerSource = client.MessageContextQueue.AcquireWriterSource();
                }

                if ( ! await writerSource.GetTask().ConfigureAwait( false ) )
                    return client.EmitError( client.DisposedException(), traceId );

                using ( client.AcquireActiveLock( traceId, out var exc ) )
                {
                    if ( exc is not null )
                        return exc;

                    client.EventScheduler.PausePing();
                    if ( confirm )
                        responseSource = client.MessageContextQueue.AcquirePendingResponseSource();
                }

                var result = await client.WriteAsync( request.Header, buffer, traceId ).ConfigureAwait( false );
                if ( result.Exception is not null )
                {
                    await client.DisposeAsync( traceId ).ConfigureAwait( false );
                    return result.Exception;
                }

                using ( client.AcquireActiveLock( traceId, out var exc ) )
                {
                    if ( exc is not null )
                        return exc;

                    client.MessageContextQueue.ResetOutgoingWriter( client, writerSource );
                    if ( responseSource is not null )
                        client.MessageContextQueue.ActivatePendingResponseSource( client, responseSource );

                    client.EventScheduler.SchedulePing( client );
                }
            }
            catch ( Exception exc )
            {
                MessageBrokerClientErrorEvent.Create( client, traceId, exc ).Emit( client.Logger.Error );
                await client.DisposeAsync( traceId ).ConfigureAwait( false );
                return exc;
            }

            if ( responseSource is null )
            {
                MessageBrokerClientMessagePushedEvent.Create( client, traceId, context.Publisher, messageLength )
                    .Emit( client.Logger.MessagePushed );

                return MessageBrokerPushResult.CreateUnconfirmed();
            }

            var response = await responseSource.GetTask().ConfigureAwait( false );
            try
            {
                if ( response.Type != IncomingPacketToken.Result.Ok )
                {
                    if ( response.Type == IncomingPacketToken.Result.Disposed )
                        return client.EmitError( client.DisposedException(), traceId );

                    var error = new MessageBrokerClientResponseTimeoutException( client, request.Header.GetServerEndpoint() );
                    MessageBrokerClientErrorEvent.Create( client, traceId, error ).Emit( client.Logger.Error );
                    await client.DisposeAsync( traceId ).ConfigureAwait( false );
                    return error;
                }

                using ( client.AcquireActiveLock( traceId, out var exc ) )
                {
                    if ( exc is not null )
                        return exc;

                    client.MessageContextQueue.ResetPendingResponseSource( responseSource );
                }

                switch ( response.Header.GetClientEndpoint() )
                {
                    case MessageBrokerClientEndpoint.MessageAcceptedResponse:
                    {
                        MessageBrokerClientReadPacketEvent.CreateReceived( client, traceId, response.Header )
                            .Emit( client.Logger.ReadPacket );

                        var exception = Protocol.AssertPayload( client, response.Header, Protocol.MessageAcceptedResponse.Length );
                        if ( exception is not null )
                        {
                            MessageBrokerClientErrorEvent.Create( client, traceId, exception ).Emit( client.Logger.Error );
                            await client.DisposeAsync( traceId ).ConfigureAwait( false );
                            return exception;
                        }

                        var parsedResponse = Protocol.MessageAcceptedResponse.Parse( response.Data, reverseEndianness );
                        MessageBrokerClientReadPacketEvent.CreateAccepted( client, traceId, response.Header )
                            .Emit( client.Logger.ReadPacket );

                        MessageBrokerClientMessagePushedEvent
                            .Create( client, traceId, context.Publisher, messageLength, parsedResponse.Id )
                            .Emit( client.Logger.MessagePushed );

                        return MessageBrokerPushResult.Create( parsedResponse.Id );
                    }
                    case MessageBrokerClientEndpoint.MessageRejectedResponse:
                    {
                        MessageBrokerClientReadPacketEvent.CreateReceived( client, traceId, response.Header )
                            .Emit( client.Logger.ReadPacket );

                        var exception = Protocol.AssertPayload( client, response.Header, Protocol.MessageRejectedResponse.Length );
                        if ( exception is not null )
                        {
                            MessageBrokerClientErrorEvent.Create( client, traceId, exception ).Emit( client.Logger.Error );
                            await client.DisposeAsync( traceId ).ConfigureAwait( false );
                            return exception;
                        }

                        var parsedResponse = Protocol.MessageRejectedResponse.Parse( response.Data );
                        MessageBrokerClientReadPacketEvent.CreateAccepted( client, traceId, response.Header )
                            .Emit( client.Logger.ReadPacket );

                        return client.EmitError(
                            Protocol.RequestException( client, request.Header, parsedResponse.StringifyErrors( context.Publisher ) ),
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
                MessageBrokerClientErrorEvent.Create( client, traceId, exc ).Emit( client.Logger.Error );
                await client.DisposeAsync( traceId ).ConfigureAwait( false );
                return exc;
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
}
