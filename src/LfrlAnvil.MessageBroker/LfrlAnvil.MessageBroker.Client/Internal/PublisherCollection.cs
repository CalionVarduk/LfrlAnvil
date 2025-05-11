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
        }

        ManualResetValueTaskSource<IncomingPacketToken> responseSource;
        Protocol.BindPublisherRequest request;
        ulong contextId;

        var poolToken = default( MemoryPoolToken<byte> );
        try
        {
            request = new Protocol.BindPublisherRequest( channelName, streamName );
            poolToken = client.MemoryPool.Rent( request.Length, out var buffer ).EnableClearing();
            request.Serialize( buffer, reverseEndianness );

            ManualResetValueTaskSource<bool> writerSource;
            using ( client.AcquireLock() )
            {
                if ( client.ShouldCancel )
                    return client.DisposedException();

                contextId = client.MessageContextQueue.AcquireContextId();
                writerSource = client.MessageContextQueue.AcquireWriterSource();
            }

            if ( ! await writerSource.GetTask().ConfigureAwait( false ) )
                return client.DisposedException();

            using ( client.AcquireLock() )
            {
                if ( client.ShouldCancel )
                    return client.DisposedException();

                client.EventScheduler.PausePing();
                responseSource = client.MessageContextQueue.AcquirePendingResponseSource( contextId, request.Header.GetServerEndpoint() );
            }

            var result = await client.WriteAsync( request.Header, buffer, contextId ).ConfigureAwait( false );
            if ( result.Exception is not null )
            {
                await client.DisposeAsync().ConfigureAwait( false );
                return result.Exception;
            }

            using ( client.AcquireLock() )
            {
                if ( client.ShouldCancel )
                    return client.DisposedException();

                client.MessageContextQueue.ResetOutgoingWriter( client, writerSource );
                client.MessageContextQueue.ActivatePendingResponseSource( client, responseSource );
                client.EventScheduler.SchedulePing( client );
            }
        }
        catch ( Exception exc )
        {
            client.Emit( MessageBrokerClientEvent.Unexpected( client, exc ) );
            await client.DisposeAsync().ConfigureAwait( false );
            return exc;
        }
        finally
        {
            poolToken.Return( client );
        }

        var response = await responseSource.GetTask().ConfigureAwait( false );
        try
        {
            if ( response.Type != IncomingPacketToken.Result.Ok )
            {
                if ( response.Type == IncomingPacketToken.Result.Disposed )
                    return client.DisposedException();

                var error = new MessageBrokerClientResponseTimeoutException( client, request.Header.GetServerEndpoint() );
                client.Emit( MessageBrokerClientEvent.WaitingForMessage( client, error ) );
                await client.DisposeAsync().ConfigureAwait( false );
                return error;
            }

            using ( client.AcquireLock() )
            {
                if ( client.ShouldCancel )
                    return client.DisposedException();

                client.MessageContextQueue.ResetPendingResponseSource( responseSource );
            }

            switch ( response.Header.GetClientEndpoint() )
            {
                case MessageBrokerClientEndpoint.PublisherBoundResponse:
                {
                    client.Emit( MessageBrokerClientEvent.MessageReceived( client, response.Header, contextId ) );

                    var exc = Protocol.AssertPayload( client, response.Header, Protocol.PublisherBoundResponse.Length );
                    if ( exc is not null )
                    {
                        client.Emit( MessageBrokerClientEvent.MessageRejected( client, response.Header, exc, contextId ) );
                        await client.DisposeAsync().ConfigureAwait( false );
                        return exc;
                    }

                    var parsedResponse = Protocol.PublisherBoundResponse.Parse( response.Data, reverseEndianness );
                    var errors = parsedResponse.StringifyErrors();

                    if ( errors.Count > 0 )
                    {
                        var error = client.EmitError(
                            MessageBrokerClientEvent.MessageRejected(
                                client,
                                response.Header,
                                Protocol.ProtocolException( client, response.Header, errors ),
                                contextId ) );

                        await client.DisposeAsync().ConfigureAwait( false );
                        return error;
                    }

                    bool cancel;
                    MessageBrokerBindPublisherResult bindResult = default;
                    using ( client.AcquireLock() )
                    {
                        cancel = client.ShouldCancel;
                        if ( ! cancel )
                        {
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
                    }

                    if ( cancel )
                        return client.EmitError(
                            MessageBrokerClientEvent.MessageReceived( client, response.Header, contextId, client.DisposedException() ) );

                    client.Emit( MessageBrokerClientEvent.MessageAccepted( client, response.Header, contextId, bindResult.Publisher ) );
                    return bindResult;
                }
                case MessageBrokerClientEndpoint.BindPublisherFailureResponse:
                {
                    client.Emit( MessageBrokerClientEvent.MessageReceived( client, response.Header, contextId ) );

                    var exc = Protocol.AssertPayload( client, response.Header, Protocol.BindPublisherFailureResponse.Length );
                    if ( exc is not null )
                    {
                        client.Emit( MessageBrokerClientEvent.MessageRejected( client, response.Header, exc, contextId ) );
                        await client.DisposeAsync().ConfigureAwait( false );
                        return exc;
                    }

                    var parsedResponse = Protocol.BindPublisherFailureResponse.Parse( response.Data );
                    return client.EmitError(
                        MessageBrokerClientEvent.MessageReceived(
                            client,
                            response.Header,
                            contextId,
                            Protocol.RequestException( client, request.Header, parsedResponse.StringifyErrors( channelName ) ) ) );
                }
                default:
                {
                    var error = client.HandleUnexpectedEndpoint( response.Header, contextId );
                    await client.DisposeAsync().ConfigureAwait( false );
                    return error;
                }
            }
        }
        catch ( Exception exc )
        {
            client.Emit( MessageBrokerClientEvent.Unexpected( client, exc ) );
            await client.DisposeAsync().ConfigureAwait( false );
            return exc;
        }
        finally
        {
            response.PoolToken.Return( client );
        }
    }

    internal static async ValueTask<Result<MessageBrokerUnbindPublisherResult>> UnbindAsync(MessageBrokerPublisher publisher)
    {
        bool reverseEndianness;
        var client = publisher.Client;
        using ( client.AcquireLock() )
        {
            if ( client.ShouldCancel )
                ExceptionThrower.Throw( client.DisposedException() );

            if ( ! publisher.Dispose() )
                return MessageBrokerUnbindPublisherResult.CreateNotBound();

            reverseEndianness = BitConverter.IsLittleEndian != client.IsServerLittleEndian;
        }

        ManualResetValueTaskSource<IncomingPacketToken> responseSource;
        Protocol.UnbindPublisherRequest request;
        ulong contextId;

        var poolToken = default( MemoryPoolToken<byte> );
        try
        {
            request = new Protocol.UnbindPublisherRequest( publisher.ChannelId );
            poolToken = client.MemoryPool.Rent( Protocol.UnbindPublisherRequest.Length, out var buffer ).EnableClearing();
            request.Serialize( buffer, reverseEndianness );

            ManualResetValueTaskSource<bool> writerSource;
            using ( client.AcquireLock() )
            {
                if ( client.ShouldCancel )
                    return client.DisposedException();

                contextId = client.MessageContextQueue.AcquireContextId();
                writerSource = client.MessageContextQueue.AcquireWriterSource();
            }

            if ( ! await writerSource.GetTask().ConfigureAwait( false ) )
                return client.DisposedException();

            using ( client.AcquireLock() )
            {
                if ( client.ShouldCancel )
                    return client.DisposedException();

                client.EventScheduler.PausePing();
                responseSource = client.MessageContextQueue.AcquirePendingResponseSource( contextId, request.Header.GetServerEndpoint() );
            }

            var result = await client.WriteAsync( request.Header, buffer, contextId, publisher ).ConfigureAwait( false );
            if ( result.Exception is not null )
            {
                await client.DisposeAsync().ConfigureAwait( false );
                return result.Exception;
            }

            using ( client.AcquireLock() )
            {
                if ( client.ShouldCancel )
                    return client.DisposedException();

                client.MessageContextQueue.ResetOutgoingWriter( client, writerSource );
                client.MessageContextQueue.ActivatePendingResponseSource( client, responseSource );
                client.EventScheduler.SchedulePing( client );
            }
        }
        catch ( Exception exc )
        {
            client.Emit( MessageBrokerClientEvent.Unexpected( client, exc ) );
            await client.DisposeAsync().ConfigureAwait( false );
            return exc;
        }
        finally
        {
            poolToken.Return( client );
        }

        var response = await responseSource.GetTask().ConfigureAwait( false );
        try
        {
            if ( response.Type != IncomingPacketToken.Result.Ok )
            {
                if ( response.Type == IncomingPacketToken.Result.Disposed )
                    return client.DisposedException();

                var error = new MessageBrokerClientResponseTimeoutException( client, request.Header.GetServerEndpoint() );
                client.Emit( MessageBrokerClientEvent.WaitingForMessage( client, error ) );
                await client.DisposeAsync().ConfigureAwait( false );
                return error;
            }

            using ( client.AcquireLock() )
            {
                if ( client.ShouldCancel )
                    return client.DisposedException();

                client.MessageContextQueue.ResetPendingResponseSource( responseSource );
            }

            switch ( response.Header.GetClientEndpoint() )
            {
                case MessageBrokerClientEndpoint.PublisherUnboundResponse:
                {
                    client.Emit( MessageBrokerClientEvent.MessageReceived( client, response.Header, contextId ) );

                    var exc = Protocol.AssertPayload( client, response.Header, Protocol.PublisherUnboundResponse.Length );
                    if ( exc is not null )
                    {
                        client.Emit( MessageBrokerClientEvent.MessageRejected( client, response.Header, exc, contextId ) );
                        await client.DisposeAsync().ConfigureAwait( false );
                        return exc;
                    }

                    var parsedResponse = Protocol.PublisherUnboundResponse.Parse( response.Data );

                    bool cancel;
                    using ( client.AcquireLock() )
                    {
                        cancel = client.ShouldCancel;
                        if ( ! cancel )
                            client.PublisherCollection._store.Remove( publisher.ChannelId, publisher.ChannelName );
                    }

                    if ( cancel )
                        return client.EmitError(
                            MessageBrokerClientEvent.MessageReceived( client, response.Header, contextId, client.DisposedException() ) );

                    client.Emit( MessageBrokerClientEvent.MessageAccepted( client, response.Header, contextId ) );
                    return MessageBrokerUnbindPublisherResult.Create( parsedResponse.ChannelRemoved, parsedResponse.StreamRemoved );
                }
                case MessageBrokerClientEndpoint.UnbindPublisherFailureResponse:
                {
                    client.Emit( MessageBrokerClientEvent.MessageReceived( client, response.Header, contextId ) );

                    var exc = Protocol.AssertPayload( client, response.Header, Protocol.UnbindPublisherFailureResponse.Length );
                    if ( exc is not null )
                    {
                        client.Emit( MessageBrokerClientEvent.MessageRejected( client, response.Header, exc, contextId ) );
                        await client.DisposeAsync().ConfigureAwait( false );
                        return exc;
                    }

                    var parsedResponse = Protocol.UnbindPublisherFailureResponse.Parse( response.Data );
                    return client.EmitError(
                        MessageBrokerClientEvent.MessageReceived(
                            client,
                            response.Header,
                            contextId,
                            Protocol.RequestException( client, request.Header, parsedResponse.StringifyErrors( publisher ) ) ) );
                }
                default:
                {
                    var error = client.HandleUnexpectedEndpoint( response.Header, contextId );
                    await client.DisposeAsync().ConfigureAwait( false );
                    return error;
                }
            }
        }
        catch ( Exception exc )
        {
            client.Emit( MessageBrokerClientEvent.Unexpected( client, exc ) );
            await client.DisposeAsync().ConfigureAwait( false );
            return exc;
        }
        finally
        {
            response.PoolToken.Return( client );
        }
    }

    internal static async ValueTask<Result<MesageBrokerSendResult>> SendAsync(MessageBrokerSendContext context)
    {
        bool reverseEndianness;
        var client = context.Publisher.Client;
        using ( client.AcquireLock() )
        {
            if ( client.ShouldCancel )
                ExceptionThrower.Throw( client.DisposedException() );

            if ( context.Publisher.State != MessageBrokerPublisherState.Bound )
                return MesageBrokerSendResult.CreateNotBound();

            reverseEndianness = BitConverter.IsLittleEndian != client.IsServerLittleEndian;
        }

        ManualResetValueTaskSource<IncomingPacketToken> responseSource;
        Protocol.PushMessageHeader request;
        ulong contextId;

        try
        {
            var buffer = context.Data;
            request = new Protocol.PushMessageHeader(
                context.Publisher.ChannelId,
                unchecked( buffer.Length - Protocol.PushMessageHeader.Length ) );

            request.Serialize( buffer.Slice( 0, Protocol.PushMessageHeader.Length ), reverseEndianness );

            ManualResetValueTaskSource<bool> writerSource;
            using ( client.AcquireLock() )
            {
                if ( client.ShouldCancel )
                    return client.DisposedException();

                contextId = client.MessageContextQueue.AcquireContextId();
                writerSource = client.MessageContextQueue.AcquireWriterSource();
            }

            if ( ! await writerSource.GetTask().ConfigureAwait( false ) )
                return client.DisposedException();

            using ( client.AcquireLock() )
            {
                if ( client.ShouldCancel )
                    return client.DisposedException();

                client.EventScheduler.PausePing();
                responseSource = client.MessageContextQueue.AcquirePendingResponseSource( contextId, request.Header.GetServerEndpoint() );
            }

            var result = await client.WriteAsync( request.Header, buffer, contextId, context.Publisher ).ConfigureAwait( false );
            if ( result.Exception is not null )
            {
                await client.DisposeAsync().ConfigureAwait( false );
                return result.Exception;
            }

            using ( client.AcquireLock() )
            {
                if ( client.ShouldCancel )
                    return client.DisposedException();

                client.MessageContextQueue.ResetOutgoingWriter( client, writerSource );
                client.MessageContextQueue.ActivatePendingResponseSource( client, responseSource );
                client.EventScheduler.SchedulePing( client );
            }
        }
        catch ( Exception exc )
        {
            client.Emit( MessageBrokerClientEvent.Unexpected( client, exc ) );
            await client.DisposeAsync().ConfigureAwait( false );
            return exc;
        }

        var response = await responseSource.GetTask().ConfigureAwait( false );
        try
        {
            if ( response.Type != IncomingPacketToken.Result.Ok )
            {
                if ( response.Type == IncomingPacketToken.Result.Disposed )
                    return client.DisposedException();

                var error = new MessageBrokerClientResponseTimeoutException( client, request.Header.GetServerEndpoint() );
                client.Emit( MessageBrokerClientEvent.WaitingForMessage( client, error ) );
                await client.DisposeAsync().ConfigureAwait( false );
                return error;
            }

            using ( client.AcquireLock() )
            {
                if ( client.ShouldCancel )
                    return client.DisposedException();

                client.MessageContextQueue.ResetPendingResponseSource( responseSource );
            }

            switch ( response.Header.GetClientEndpoint() )
            {
                case MessageBrokerClientEndpoint.MessageAcceptedResponse:
                {
                    client.Emit( MessageBrokerClientEvent.MessageReceived( client, response.Header, contextId ) );

                    var exc = Protocol.AssertPayload( client, response.Header, Protocol.MessageAcceptedResponse.Length );
                    if ( exc is not null )
                    {
                        client.Emit( MessageBrokerClientEvent.MessageRejected( client, response.Header, exc, contextId ) );
                        await client.DisposeAsync().ConfigureAwait( false );
                        return exc;
                    }

                    var parsedResponse = Protocol.MessageAcceptedResponse.Parse( response.Data, reverseEndianness );
                    client.Emit( MessageBrokerClientEvent.MessageAccepted( client, response.Header, contextId ) );
                    return MesageBrokerSendResult.Create( parsedResponse.Id );
                }
                case MessageBrokerClientEndpoint.MessageRejectedResponse:
                {
                    client.Emit( MessageBrokerClientEvent.MessageReceived( client, response.Header, contextId ) );

                    var exc = Protocol.AssertPayload( client, response.Header, Protocol.MessageRejectedResponse.Length );
                    if ( exc is not null )
                    {
                        client.Emit( MessageBrokerClientEvent.MessageRejected( client, response.Header, exc, contextId ) );
                        await client.DisposeAsync().ConfigureAwait( false );
                        return exc;
                    }

                    var parsedResponse = Protocol.MessageRejectedResponse.Parse( response.Data );
                    return client.EmitError(
                        MessageBrokerClientEvent.MessageReceived(
                            client,
                            response.Header,
                            contextId,
                            Protocol.RequestException( client, request.Header, parsedResponse.StringifyErrors( context.Publisher ) ) ) );
                }
                default:
                {
                    var error = client.HandleUnexpectedEndpoint( response.Header, contextId );
                    await client.DisposeAsync().ConfigureAwait( false );
                    return error;
                }
            }
        }
        catch ( Exception exc )
        {
            client.Emit( MessageBrokerClientEvent.Unexpected( client, exc ) );
            await client.DisposeAsync().ConfigureAwait( false );
            return exc;
        }
        finally
        {
            response.PoolToken.Return( client );
        }
    }

    internal void Clear()
    {
        foreach ( var obj in _store )
            obj.OnClientDisposed();

        _store.Clear();
    }
}
