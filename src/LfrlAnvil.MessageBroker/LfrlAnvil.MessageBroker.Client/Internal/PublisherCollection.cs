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
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LfrlAnvil.Async;
using LfrlAnvil.Exceptions;
using LfrlAnvil.MessageBroker.Client.Buffering;
using LfrlAnvil.MessageBroker.Client.Events;
using LfrlAnvil.MessageBroker.Client.Exceptions;

namespace LfrlAnvil.MessageBroker.Client.Internal;

internal readonly struct PublisherCollection
{
    private readonly Dictionary<int, MessageBrokerPublisher> _byChannelId;
    private readonly Dictionary<string, MessageBrokerPublisher> _byChannelName;

    private PublisherCollection(StringComparer nameComparer)
    {
        _byChannelId = new Dictionary<int, MessageBrokerPublisher>();
        _byChannelName = new Dictionary<string, MessageBrokerPublisher>( nameComparer );
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
            return client.PublisherCollection._byChannelId.Count;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerPublisher[] GetAll(MessageBrokerClient client)
    {
        using ( client.AcquireLock() )
        {
            if ( client.PublisherCollection._byChannelId.Count == 0 )
                return Array.Empty<MessageBrokerPublisher>();

            var i = 0;
            var result = new MessageBrokerPublisher[client.PublisherCollection._byChannelId.Count];
            foreach ( var publisher in client.PublisherCollection._byChannelId.Values )
                result[i++] = publisher;

            return result;
        }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerPublisher? TryGetByChannelId(MessageBrokerClient client, int channelId)
    {
        using ( client.AcquireLock() )
            return client.PublisherCollection._byChannelId.TryGetValue( channelId, out var result ) ? result : null;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerPublisher? TryGetByChannelName(MessageBrokerClient client, string channelName)
    {
        using ( client.AcquireLock() )
            return client.PublisherCollection._byChannelName.TryGetValue( channelName, out var result ) ? result : null;
    }

    internal static async ValueTask<Result<MessageBrokerBindResult?>> BindAsync(MessageBrokerClient client, string name, string? queueName)
    {
        Ensure.IsInRange( name.Length, Defaults.NameLengthBounds.Min, Defaults.NameLengthBounds.Max );
        if ( queueName is not null )
            Ensure.IsInRange( queueName.Length, Defaults.NameLengthBounds.Min, Defaults.NameLengthBounds.Max );

        bool reverseEndianness;
        using ( client.AcquireLock() )
        {
            if ( client.ShouldCancel )
                ExceptionThrower.Throw( client.DisposedException() );

            client.AssertState( MessageBrokerClientState.Running );
            if ( client.PublisherCollection._byChannelName.TryGetValue( name, out var publisher ) )
                return MessageBrokerBindResult.CreateAlreadyBound( publisher );

            reverseEndianness = BitConverter.IsLittleEndian != client.IsServerLittleEndian;
        }

        ManualResetValueTaskSource<IncomingPacketToken> responseSource;
        Protocol.BindRequest request;
        ulong contextId;

        var bufferToken = default( BinaryBufferToken );
        try
        {
            request = new Protocol.BindRequest( name, queueName ?? string.Empty );
            bufferToken = client.RentBuffer( request.Length, out var buffer ).EnableClearing();
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
            client.DisposeBufferToken( bufferToken );
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
                case MessageBrokerClientEndpoint.BoundResponse:
                {
                    client.Emit( MessageBrokerClientEvent.MessageReceived( client, response.Header, contextId ) );

                    var exc = Protocol.AssertPayload( client, response.Header, Protocol.BoundResponse.Length );
                    if ( exc is not null )
                    {
                        client.Emit( MessageBrokerClientEvent.MessageRejected( client, response.Header, exc, contextId ) );
                        await client.DisposeAsync().ConfigureAwait( false );
                        return exc;
                    }

                    var parsedResponse = Protocol.BoundResponse.Parse( response.Data, reverseEndianness );
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
                    MessageBrokerBindResult bindResult = default;
                    using ( client.AcquireLock() )
                    {
                        cancel = client.ShouldCancel;
                        if ( ! cancel )
                        {
                            var publisher = new MessageBrokerPublisher(
                                client,
                                parsedResponse.ChannelId,
                                name,
                                parsedResponse.QueueId,
                                queueName ?? name );

                            client.PublisherCollection._byChannelId.Add( parsedResponse.ChannelId, publisher );
                            client.PublisherCollection._byChannelName.Add( name, publisher );
                            bindResult = MessageBrokerBindResult.Create(
                                publisher,
                                parsedResponse.ChannelCreated,
                                parsedResponse.QueueCreated );
                        }
                    }

                    if ( cancel )
                        return client.EmitError(
                            MessageBrokerClientEvent.MessageReceived( client, response.Header, contextId, client.DisposedException() ) );

                    client.Emit( MessageBrokerClientEvent.MessageAccepted( client, response.Header, contextId, bindResult.Publisher ) );
                    return bindResult;
                }
                case MessageBrokerClientEndpoint.BindFailureResponse:
                {
                    client.Emit( MessageBrokerClientEvent.MessageReceived( client, response.Header, contextId ) );

                    var exc = Protocol.AssertPayload( client, response.Header, Protocol.BindFailureResponse.Length );
                    if ( exc is not null )
                    {
                        client.Emit( MessageBrokerClientEvent.MessageRejected( client, response.Header, exc, contextId ) );
                        await client.DisposeAsync().ConfigureAwait( false );
                        return exc;
                    }

                    var parsedResponse = Protocol.BindFailureResponse.Parse( response.Data );
                    return client.EmitError(
                        MessageBrokerClientEvent.MessageReceived(
                            client,
                            response.Header,
                            contextId,
                            Protocol.RequestException( client, request.Header, parsedResponse.StringifyErrors( name ) ) ) );
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
            client.DisposeBufferToken( response.BufferToken );
        }
    }

    internal static async ValueTask<Result<MessageBrokerUnbindResult>> UnbindAsync(MessageBrokerPublisher publisher)
    {
        bool reverseEndianness;
        var client = publisher.Client;
        using ( client.AcquireLock() )
        {
            if ( client.ShouldCancel )
                ExceptionThrower.Throw( client.DisposedException() );

            if ( ! publisher.Dispose() )
                return MessageBrokerUnbindResult.CreateNotBound();

            reverseEndianness = BitConverter.IsLittleEndian != client.IsServerLittleEndian;
        }

        ManualResetValueTaskSource<IncomingPacketToken> responseSource;
        Protocol.UnbindRequest request;
        ulong contextId;

        var bufferToken = default( BinaryBufferToken );
        try
        {
            request = new Protocol.UnbindRequest( publisher.ChannelId );
            bufferToken = client.RentBuffer( Protocol.UnbindRequest.Length, out var buffer ).EnableClearing();
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
            client.DisposeBufferToken( bufferToken );
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
                case MessageBrokerClientEndpoint.UnboundResponse:
                {
                    client.Emit( MessageBrokerClientEvent.MessageReceived( client, response.Header, contextId ) );

                    var exc = Protocol.AssertPayload( client, response.Header, Protocol.UnboundResponse.Length );
                    if ( exc is not null )
                    {
                        client.Emit( MessageBrokerClientEvent.MessageRejected( client, response.Header, exc, contextId ) );
                        await client.DisposeAsync().ConfigureAwait( false );
                        return exc;
                    }

                    var parsedResponse = Protocol.UnboundResponse.Parse( response.Data );

                    bool cancel;
                    using ( client.AcquireLock() )
                    {
                        cancel = client.ShouldCancel;
                        if ( ! cancel )
                        {
                            client.PublisherCollection._byChannelId.Remove( publisher.ChannelId );
                            client.PublisherCollection._byChannelName.Remove( publisher.ChannelName );
                        }
                    }

                    if ( cancel )
                        return client.EmitError(
                            MessageBrokerClientEvent.MessageReceived( client, response.Header, contextId, client.DisposedException() ) );

                    client.Emit( MessageBrokerClientEvent.MessageAccepted( client, response.Header, contextId ) );
                    return MessageBrokerUnbindResult.Create( parsedResponse.ChannelRemoved, parsedResponse.QueueRemoved );
                }
                case MessageBrokerClientEndpoint.UnbindFailureResponse:
                {
                    client.Emit( MessageBrokerClientEvent.MessageReceived( client, response.Header, contextId ) );

                    var exc = Protocol.AssertPayload( client, response.Header, Protocol.UnbindFailureResponse.Length );
                    if ( exc is not null )
                    {
                        client.Emit( MessageBrokerClientEvent.MessageRejected( client, response.Header, exc, contextId ) );
                        await client.DisposeAsync().ConfigureAwait( false );
                        return exc;
                    }

                    var parsedResponse = Protocol.UnbindFailureResponse.Parse( response.Data );
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
            client.DisposeBufferToken( response.BufferToken );
        }
    }

    internal void Clear()
    {
        foreach ( var publisher in _byChannelId.Values )
            publisher.OnClientDisposed();

        _byChannelId.Clear();
        _byChannelName.Clear();
    }
}
