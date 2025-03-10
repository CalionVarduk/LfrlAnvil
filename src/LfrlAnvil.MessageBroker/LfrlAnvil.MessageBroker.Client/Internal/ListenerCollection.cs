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

internal readonly struct ListenerCollection
{
    private readonly Dictionary<int, MessageBrokerListener> _byChannelId;
    private readonly Dictionary<string, MessageBrokerListener> _byChannelName;

    private ListenerCollection(StringComparer nameComparer)
    {
        _byChannelId = new Dictionary<int, MessageBrokerListener>();
        _byChannelName = new Dictionary<string, MessageBrokerListener>( nameComparer );
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
            return client.ListenerCollection._byChannelId.Count;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerListener[] GetAll(MessageBrokerClient client)
    {
        using ( client.AcquireLock() )
        {
            if ( client.ListenerCollection._byChannelId.Count == 0 )
                return Array.Empty<MessageBrokerListener>();

            var i = 0;
            var result = new MessageBrokerListener[client.ListenerCollection._byChannelId.Count];
            foreach ( var listener in client.ListenerCollection._byChannelId.Values )
                result[i++] = listener;

            return result;
        }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerListener? TryGetByChannelId(MessageBrokerClient client, int channelId)
    {
        using ( client.AcquireLock() )
            return client.ListenerCollection._byChannelId.TryGetValue( channelId, out var result ) ? result : null;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerListener? TryGetByChannelName(MessageBrokerClient client, string channelName)
    {
        using ( client.AcquireLock() )
            return client.ListenerCollection._byChannelName.TryGetValue( channelName, out var result ) ? result : null;
    }

    internal static async ValueTask<Result<MessageBrokerSubscribeResult?>> SubscribeAsync(
        MessageBrokerClient client,
        string channelName,
        bool createChannelIfNotExists)
    {
        Ensure.IsInRange( channelName.Length, Defaults.NameLengthBounds.Min, Defaults.NameLengthBounds.Max );

        bool reverseEndianness;
        using ( client.AcquireLock() )
        {
            if ( client.ShouldCancel )
                ExceptionThrower.Throw( client.DisposedException() );

            client.AssertState( MessageBrokerClientState.Running );
            if ( client.ListenerCollection._byChannelName.TryGetValue( channelName, out var listener ) )
                return MessageBrokerSubscribeResult.CreateAlreadySubscribed( listener );

            reverseEndianness = BitConverter.IsLittleEndian != client.IsServerLittleEndian;
        }

        ManualResetValueTaskSource<IncomingPacketToken> responseSource;
        Protocol.SubscribeRequest request;
        ulong contextId;

        var bufferToken = default( BinaryBufferToken );
        try
        {
            request = new Protocol.SubscribeRequest( channelName, createChannelIfNotExists );
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

                client.SynchronousScheduler.PausePing();
                responseSource = client.MessageContextQueue.AcquirePendingResponseSource( contextId, request.Header.GetServerEndpoint() );
            }

            var result = await client.WriteAsync( request.Header, buffer, contextId, channelName ).ConfigureAwait( false );
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
                client.SynchronousScheduler.SchedulePing( client );
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
                case MessageBrokerClientEndpoint.SubscribedResponse:
                {
                    client.Emit( MessageBrokerClientEvent.MessageReceived( client, response.Header, contextId ) );

                    var exc = Protocol.AssertPayload( client, response.Header, Protocol.SubscribedResponse.Length );
                    if ( exc is not null )
                    {
                        client.Emit( MessageBrokerClientEvent.MessageRejected( client, response.Header, exc, contextId ) );
                        await client.DisposeAsync().ConfigureAwait( false );
                        return exc;
                    }

                    var parsedResponse = Protocol.SubscribedResponse.Parse( response.Data, reverseEndianness );
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
                    MessageBrokerSubscribeResult subscribeResult = default;
                    using ( client.AcquireLock() )
                    {
                        cancel = client.ShouldCancel;
                        if ( ! cancel )
                        {
                            var listener = new MessageBrokerListener( client, parsedResponse.ChannelId, channelName );
                            client.ListenerCollection._byChannelId.Add( parsedResponse.ChannelId, listener );
                            client.ListenerCollection._byChannelName.Add( channelName, listener );
                            subscribeResult = MessageBrokerSubscribeResult.Create( listener, parsedResponse.ChannelCreated );
                        }
                    }

                    if ( cancel )
                        return client.EmitError(
                            MessageBrokerClientEvent.MessageReceived( client, response.Header, contextId, client.DisposedException() ) );

                    client.Emit( MessageBrokerClientEvent.MessageAccepted( client, response.Header, contextId, subscribeResult.Listener ) );
                    return subscribeResult;
                }
                case MessageBrokerClientEndpoint.SubscribeFailureResponse:
                {
                    client.Emit( MessageBrokerClientEvent.MessageReceived( client, response.Header, contextId ) );

                    var exc = Protocol.AssertPayload( client, response.Header, Protocol.SubscribeFailureResponse.Length );
                    if ( exc is not null )
                    {
                        client.Emit( MessageBrokerClientEvent.MessageRejected( client, response.Header, exc, contextId ) );
                        await client.DisposeAsync().ConfigureAwait( false );
                        return exc;
                    }

                    var parsedResponse = Protocol.SubscribeFailureResponse.Parse( response.Data );
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
            client.DisposeBufferToken( response.BufferToken );
        }
    }

    internal static async ValueTask<Result<MessageBrokerUnsubscribeResult>> UnsubscribeAsync(MessageBrokerListener listener)
    {
        bool reverseEndianness;
        var client = listener.Client;
        using ( client.AcquireLock() )
        {
            if ( client.ShouldCancel )
                ExceptionThrower.Throw( client.DisposedException() );

            if ( ! listener.Dispose() )
                return MessageBrokerUnsubscribeResult.CreateNotSubscribed();

            reverseEndianness = BitConverter.IsLittleEndian != client.IsServerLittleEndian;
        }

        ManualResetValueTaskSource<IncomingPacketToken> responseSource;
        Protocol.UnsubscribeRequest request;
        ulong contextId;

        var bufferToken = default( BinaryBufferToken );
        try
        {
            request = new Protocol.UnsubscribeRequest( listener.ChannelId );
            bufferToken = client.RentBuffer( Protocol.UnsubscribeRequest.Length, out var buffer ).EnableClearing();
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

                client.SynchronousScheduler.PausePing();
                responseSource = client.MessageContextQueue.AcquirePendingResponseSource( contextId, request.Header.GetServerEndpoint() );
            }

            var result = await client.WriteAsync( request.Header, buffer, contextId, listener ).ConfigureAwait( false );
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
                client.SynchronousScheduler.SchedulePing( client );
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
                case MessageBrokerClientEndpoint.UnsubscribedResponse:
                {
                    client.Emit( MessageBrokerClientEvent.MessageReceived( client, response.Header, contextId ) );

                    var exc = Protocol.AssertPayload( client, response.Header, Protocol.UnsubscribedResponse.Length );
                    if ( exc is not null )
                    {
                        client.Emit( MessageBrokerClientEvent.MessageRejected( client, response.Header, exc, contextId ) );
                        await client.DisposeAsync().ConfigureAwait( false );
                        return exc;
                    }

                    var parsedResponse = Protocol.UnsubscribedResponse.Parse( response.Data );

                    bool cancel;
                    using ( client.AcquireLock() )
                    {
                        cancel = client.ShouldCancel;
                        if ( ! cancel )
                        {
                            client.ListenerCollection._byChannelId.Remove( listener.ChannelId );
                            client.ListenerCollection._byChannelName.Remove( listener.ChannelName );
                        }
                    }

                    if ( cancel )
                        return client.EmitError(
                            MessageBrokerClientEvent.MessageReceived( client, response.Header, contextId, client.DisposedException() ) );

                    client.Emit( MessageBrokerClientEvent.MessageAccepted( client, response.Header, contextId ) );
                    return MessageBrokerUnsubscribeResult.Create( parsedResponse.ChannelRemoved );
                }
                case MessageBrokerClientEndpoint.UnsubscribeFailureResponse:
                {
                    client.Emit( MessageBrokerClientEvent.MessageReceived( client, response.Header, contextId ) );

                    var exc = Protocol.AssertPayload( client, response.Header, Protocol.UnsubscribeFailureResponse.Length );
                    if ( exc is not null )
                    {
                        client.Emit( MessageBrokerClientEvent.MessageRejected( client, response.Header, exc, contextId ) );
                        await client.DisposeAsync().ConfigureAwait( false );
                        return exc;
                    }

                    var parsedResponse = Protocol.UnsubscribeFailureResponse.Parse( response.Data );
                    return client.EmitError(
                        MessageBrokerClientEvent.MessageReceived(
                            client,
                            response.Header,
                            contextId,
                            Protocol.RequestException( client, request.Header, parsedResponse.StringifyErrors( listener ) ) ) );
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
        foreach ( var listener in _byChannelId.Values )
            listener.OnClientDisposed();

        _byChannelId.Clear();
        _byChannelName.Clear();
    }
}
