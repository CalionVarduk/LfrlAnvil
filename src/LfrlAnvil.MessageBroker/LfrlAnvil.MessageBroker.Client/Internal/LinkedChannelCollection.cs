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
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Async;
using LfrlAnvil.Exceptions;
using LfrlAnvil.MessageBroker.Client.Buffering;
using LfrlAnvil.MessageBroker.Client.Events;
using LfrlAnvil.MessageBroker.Client.Exceptions;

namespace LfrlAnvil.MessageBroker.Client.Internal;

internal readonly struct LinkedChannelCollection
{
    private readonly Dictionary<int, MessageBrokerLinkedChannel> _byId;
    private readonly Dictionary<string, MessageBrokerLinkedChannel> _byName;

    private LinkedChannelCollection(StringComparer nameComparer)
    {
        _byId = new Dictionary<int, MessageBrokerLinkedChannel>();
        _byName = new Dictionary<string, MessageBrokerLinkedChannel>( nameComparer );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static LinkedChannelCollection Create()
    {
        return new LinkedChannelCollection( StringComparer.OrdinalIgnoreCase );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static int GetCount(MessageBrokerClient client)
    {
        using ( client.AcquireLock() )
            return client.LinkedChannelCollection._byId.Count;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerLinkedChannel[] GetAll(MessageBrokerClient client)
    {
        using ( client.AcquireLock() )
        {
            if ( client.LinkedChannelCollection._byId.Count == 0 )
                return Array.Empty<MessageBrokerLinkedChannel>();

            var i = 0;
            var result = new MessageBrokerLinkedChannel[client.LinkedChannelCollection._byId.Count];
            foreach ( var channel in client.LinkedChannelCollection._byId.Values )
                result[i++] = channel;

            return result;
        }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerLinkedChannel? TryGetById(MessageBrokerClient client, int id)
    {
        using ( client.AcquireLock() )
            return client.LinkedChannelCollection._byId.TryGetValue( id, out var result ) ? result : null;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerLinkedChannel? TryGetByName(MessageBrokerClient client, string name)
    {
        using ( client.AcquireLock() )
            return client.LinkedChannelCollection._byName.TryGetValue( name, out var result ) ? result : null;
    }

    internal static async ValueTask<Result<MessageBrokerChannelLinkResult?>> LinkAsync(
        MessageBrokerClient client,
        string name,
        CancellationToken cancellationToken)
    {
        Ensure.IsInRange( name.Length, Defaults.NameLengthBounds.Min, Defaults.NameLengthBounds.Max );

        cancellationToken.ThrowIfCancellationRequested();

        bool reverseEndianness;
        using ( client.AcquireLock() )
        {
            if ( client.ShouldCancel )
                ExceptionThrower.Throw( client.DisposedException() );

            client.AssertState( MessageBrokerClientState.Running );
            if ( client.LinkedChannelCollection._byName.TryGetValue( name, out var channel ) )
                return MessageBrokerChannelLinkResult.AlreadyLinked( channel );

            reverseEndianness = BitConverter.IsLittleEndian != client.IsServerLittleEndian;
        }

        ManualResetValueTaskSource<IncomingPacketToken> responseSource;
        Protocol.LinkChannelRequest request;
        ulong contextId;

        var bufferToken = default( BinaryBufferToken );
        try
        {
            request = new Protocol.LinkChannelRequest( name );
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

            var result = await client.WriteAsync( request.Header, buffer, contextId, name ).ConfigureAwait( false );
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

                var error = new MessageBrokerClientResponseTimeoutException(
                    client,
                    request.Header.GetServerEndpoint(),
                    MessageBrokerClientEndpoint.ChannelLinkedResponse );

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
                case MessageBrokerClientEndpoint.ChannelLinkedResponse:
                {
                    client.Emit( MessageBrokerClientEvent.MessageReceived( client, response.Header, contextId ) );

                    var exc = Protocol.AssertPayload( client, response.Header, Protocol.ChannelLinkedResponse.Length );
                    if ( exc is not null )
                    {
                        client.Emit( MessageBrokerClientEvent.MessageRejected( client, response.Header, exc, contextId ) );
                        await client.DisposeAsync().ConfigureAwait( false );
                        return exc;
                    }

                    var parsedResponse = Protocol.ChannelLinkedResponse.Parse( response.Data, reverseEndianness );
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
                    MessageBrokerChannelLinkResult linkResult = default;
                    using ( client.AcquireLock() )
                    {
                        cancel = client.ShouldCancel;
                        if ( ! cancel )
                        {
                            var channel = new MessageBrokerLinkedChannel( client, parsedResponse.Id, name );
                            client.LinkedChannelCollection._byId.Add( parsedResponse.Id, channel );
                            client.LinkedChannelCollection._byName.Add( name, channel );

                            linkResult = parsedResponse.Created
                                ? MessageBrokerChannelLinkResult.CreatedAndLinked( channel )
                                : MessageBrokerChannelLinkResult.Linked( channel );
                        }
                    }

                    if ( cancel )
                        return client.EmitError(
                            MessageBrokerClientEvent.MessageReceived( client, response.Header, contextId, client.DisposedException() ) );

                    client.Emit( MessageBrokerClientEvent.MessageAccepted( client, response.Header, contextId, linkResult.Channel ) );
                    return linkResult;
                }
                case MessageBrokerClientEndpoint.LinkChannelFailureResponse:
                {
                    client.Emit( MessageBrokerClientEvent.MessageReceived( client, response.Header, contextId ) );

                    var exc = Protocol.AssertPayload( client, response.Header, Protocol.LinkChannelFailureResponse.Length );
                    if ( exc is not null )
                    {
                        client.Emit( MessageBrokerClientEvent.MessageRejected( client, response.Header, exc, contextId ) );
                        await client.DisposeAsync().ConfigureAwait( false );
                        return exc;
                    }

                    var parsedResponse = Protocol.LinkChannelFailureResponse.Parse( response.Data );
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

    internal static async ValueTask<Result<MessageBrokerChannelUnlinkResult>> UnlinkAsync(
        MessageBrokerLinkedChannel channel,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        bool reverseEndianness;
        var client = channel.Client;
        using ( client.AcquireLock() )
        {
            if ( client.ShouldCancel )
                ExceptionThrower.Throw( client.DisposedException() );

            if ( ! channel.Unlink() )
                return MessageBrokerChannelUnlinkResult.NotLinked;

            reverseEndianness = BitConverter.IsLittleEndian != client.IsServerLittleEndian;
        }

        ManualResetValueTaskSource<IncomingPacketToken> responseSource;
        Protocol.UnlinkChannelRequest request;
        ulong contextId;

        var bufferToken = default( BinaryBufferToken );
        try
        {
            request = new Protocol.UnlinkChannelRequest( channel.Id );
            bufferToken = client.RentBuffer( Protocol.UnlinkChannelRequest.Length, out var buffer ).EnableClearing();
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

            var result = await client.WriteAsync( request.Header, buffer, contextId, channel ).ConfigureAwait( false );
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

                var error = new MessageBrokerClientResponseTimeoutException(
                    client,
                    request.Header.GetServerEndpoint(),
                    MessageBrokerClientEndpoint.ChannelUnlinkedResponse );

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
                case MessageBrokerClientEndpoint.ChannelUnlinkedResponse:
                {
                    client.Emit( MessageBrokerClientEvent.MessageReceived( client, response.Header, contextId ) );

                    var exc = Protocol.AssertPayload( client, response.Header, Protocol.ChannelUnlinkedResponse.Length );
                    if ( exc is not null )
                    {
                        client.Emit( MessageBrokerClientEvent.MessageRejected( client, response.Header, exc, contextId ) );
                        await client.DisposeAsync().ConfigureAwait( false );
                        return exc;
                    }

                    var parsedResponse = Protocol.ChannelUnlinkedResponse.Parse( response.Data );

                    bool cancel;
                    using ( client.AcquireLock() )
                    {
                        cancel = client.ShouldCancel;
                        if ( ! cancel )
                        {
                            client.LinkedChannelCollection._byId.Remove( channel.Id );
                            client.LinkedChannelCollection._byName.Remove( channel.Name );
                        }
                    }

                    if ( cancel )
                        return client.EmitError(
                            MessageBrokerClientEvent.MessageReceived( client, response.Header, contextId, client.DisposedException() ) );

                    client.Emit( MessageBrokerClientEvent.MessageAccepted( client, response.Header, contextId ) );
                    return parsedResponse.ChannelRemoved
                        ? MessageBrokerChannelUnlinkResult.UnlinkedAndChannelRemoved
                        : MessageBrokerChannelUnlinkResult.Unlinked;
                }
                case MessageBrokerClientEndpoint.UnlinkChannelFailureResponse:
                {
                    client.Emit( MessageBrokerClientEvent.MessageReceived( client, response.Header, contextId ) );

                    var exc = Protocol.AssertPayload( client, response.Header, Protocol.UnlinkChannelFailureResponse.Length );
                    if ( exc is not null )
                    {
                        client.Emit( MessageBrokerClientEvent.MessageRejected( client, response.Header, exc, contextId ) );
                        await client.DisposeAsync().ConfigureAwait( false );
                        return exc;
                    }

                    var parsedResponse = Protocol.UnlinkChannelFailureResponse.Parse( response.Data );
                    return client.EmitError(
                        MessageBrokerClientEvent.MessageReceived(
                            client,
                            response.Header,
                            contextId,
                            Protocol.RequestException( client, request.Header, parsedResponse.StringifyErrors( channel ) ) ) );
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
        foreach ( var channel in _byId.Values )
            channel.OnClientDisposed();

        _byId.Clear();
        _byName.Clear();
    }
}
