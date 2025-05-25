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

internal struct ListenerCollection
{
    private ObjectStore<MessageBrokerListener> _store;

    private ListenerCollection(StringComparer nameComparer)
    {
        _store = ObjectStore<MessageBrokerListener>.Create( nameComparer );
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
            return client.ListenerCollection._store.Count;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static ReadOnlyArray<MessageBrokerListener> GetAll(MessageBrokerClient client)
    {
        using ( client.AcquireLock() )
            return client.ListenerCollection._store.GetAll();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerListener? TryGetByChannelId(MessageBrokerClient client, int channelId)
    {
        using ( client.AcquireLock() )
            return client.ListenerCollection._store.TryGetById( channelId );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerListener? TryGetByChannelName(MessageBrokerClient client, string channelName)
    {
        using ( client.AcquireLock() )
            return client.ListenerCollection._store.TryGetByName( channelName );
    }

    internal static async ValueTask<Result<MessageBrokerBindListenerResult?>> BindAsync(
        MessageBrokerClient client,
        string channelName,
        string? queueName,
        MessageBrokerListenerCallback callback,
        bool createChannelIfNotExists,
        int prefetchHint)
    {
        Ensure.IsInRange( channelName.Length, Defaults.NameLengthBounds.Min, Defaults.NameLengthBounds.Max );
        if ( queueName is not null )
            Ensure.IsInRange( queueName.Length, Defaults.NameLengthBounds.Min, Defaults.NameLengthBounds.Max );

        Ensure.IsGreaterThan( prefetchHint, 0 );

        ulong traceId;
        bool reverseEndianness;
        using ( client.AcquireLock() )
        {
            if ( client.ShouldCancel )
                ExceptionThrower.Throw( client.DisposedException() );

            client.AssertState( MessageBrokerClientState.Running );
            var listener = client.ListenerCollection._store.TryGetByName( channelName );
            if ( listener is not null )
                return MessageBrokerBindListenerResult.CreateAlreadyBound( listener );

            reverseEndianness = BitConverter.IsLittleEndian != client.IsServerLittleEndian;
            traceId = client.GetTraceId();
        }

        using ( MessageBrokerClientTraceEvent.CreateScope( client, traceId, MessageBrokerClientTraceEventType.BindListener ) )
        {
            MessageBrokerClientBindingListenerEvent.Create(
                    client,
                    traceId,
                    channelName,
                    queueName ?? channelName,
                    prefetchHint,
                    createChannelIfNotExists )
                .Emit( client.Logger.BindingListener );

            ManualResetValueTaskSource<IncomingPacketToken> responseSource;
            Protocol.BindListenerRequest request;

            var poolToken = default( MemoryPoolToken<byte> );
            try
            {
                request = new Protocol.BindListenerRequest( channelName, queueName, prefetchHint, createChannelIfNotExists );
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
                    case MessageBrokerClientEndpoint.ListenerBoundResponse:
                    {
                        MessageBrokerClientReadPacketEvent.CreateReceived( client, traceId, response.Header )
                            .Emit( client.Logger.ReadPacket );

                        var exception = Protocol.AssertPayload( client, response.Header, Protocol.ListenerBoundResponse.Length );
                        if ( exception is not null )
                        {
                            MessageBrokerClientErrorEvent.Create( client, traceId, exception ).Emit( client.Logger.Error );
                            await client.DisposeAsync( traceId ).ConfigureAwait( false );
                            return exception;
                        }

                        var parsedResponse = Protocol.ListenerBoundResponse.Parse( response.Data, reverseEndianness );
                        var errors = parsedResponse.StringifyErrors();

                        if ( errors.Count > 0 )
                        {
                            var error = client.EmitError( Protocol.ProtocolException( client, response.Header, errors ), traceId );
                            await client.DisposeAsync( traceId ).ConfigureAwait( false );
                            return error;
                        }

                        MessageBrokerBindListenerResult bindResult;
                        using ( client.AcquireActiveLock( traceId, out var exc ) )
                        {
                            if ( exc is not null )
                                return exc;

                            var listener = new MessageBrokerListener(
                                client,
                                parsedResponse.ChannelId,
                                channelName,
                                parsedResponse.QueueId,
                                queueName ?? channelName,
                                prefetchHint,
                                callback );

                            client.ListenerCollection._store.Add( parsedResponse.ChannelId, channelName, listener );
                            bindResult = MessageBrokerBindListenerResult.Create(
                                listener,
                                parsedResponse.ChannelCreated,
                                parsedResponse.QueueCreated );
                        }

                        MessageBrokerClientReadPacketEvent.CreateAccepted( client, traceId, response.Header )
                            .Emit( client.Logger.ReadPacket );

                        MessageBrokerClientListenerBoundEvent.Create(
                                bindResult.Listener,
                                traceId,
                                parsedResponse.ChannelCreated,
                                parsedResponse.QueueCreated )
                            .Emit( client.Logger.ListenerBound );

                        return bindResult;
                    }
                    case MessageBrokerClientEndpoint.BindListenerFailureResponse:
                    {
                        MessageBrokerClientReadPacketEvent.CreateReceived( client, traceId, response.Header )
                            .Emit( client.Logger.ReadPacket );

                        var exception = Protocol.AssertPayload( client, response.Header, Protocol.ListenerBindFailureResponse.Length );
                        if ( exception is not null )
                        {
                            MessageBrokerClientErrorEvent.Create( client, traceId, exception ).Emit( client.Logger.Error );
                            await client.DisposeAsync( traceId ).ConfigureAwait( false );
                            return exception;
                        }

                        var parsedResponse = Protocol.ListenerBindFailureResponse.Parse( response.Data );
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

    internal static async ValueTask<Result<MessageBrokerUnbindListenerResult>> UnbindAsync(MessageBrokerListener listener)
    {
        ulong traceId;
        bool reverseEndianness;
        var client = listener.Client;
        using ( client.AcquireLock() )
        {
            if ( client.ShouldCancel )
                ExceptionThrower.Throw( client.DisposedException() );

            if ( ! listener.BeginDispose() )
                return MessageBrokerUnbindListenerResult.CreateNotBound();

            reverseEndianness = BitConverter.IsLittleEndian != client.IsServerLittleEndian;
            traceId = client.GetTraceId();
        }

        using ( MessageBrokerClientTraceEvent.CreateScope( client, traceId, MessageBrokerClientTraceEventType.UnbindListener ) )
        {
            MessageBrokerClientUnbindingListenerEvent.Create( listener, traceId ).Emit( client.Logger.UnbindingListener );
            var endDispose = listener.EndDisposingAsync( traceId );
            try
            {
                ManualResetValueTaskSource<IncomingPacketToken> responseSource;
                Protocol.UnbindListenerRequest request;

                var poolToken = default( MemoryPoolToken<byte> );
                try
                {
                    request = new Protocol.UnbindListenerRequest( listener.ChannelId );
                    poolToken = client.MemoryPool.Rent( Protocol.UnbindListenerRequest.Length, out var buffer ).EnableClearing();
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
                        case MessageBrokerClientEndpoint.ListenerUnboundResponse:
                        {
                            MessageBrokerClientReadPacketEvent.CreateReceived( client, traceId, response.Header )
                                .Emit( client.Logger.ReadPacket );

                            var exception = Protocol.AssertPayload( client, response.Header, Protocol.ListenerUnboundResponse.Length );
                            if ( exception is not null )
                            {
                                MessageBrokerClientErrorEvent.Create( client, traceId, exception ).Emit( client.Logger.Error );
                                await client.DisposeAsync( traceId ).ConfigureAwait( false );
                                return exception;
                            }

                            var parsedResponse = Protocol.ListenerUnboundResponse.Parse( response.Data );

                            using ( client.AcquireActiveLock( traceId, out var exc ) )
                            {
                                if ( exc is not null )
                                    return exc;

                                client.ListenerCollection._store.Remove( listener.ChannelId, listener.ChannelName );
                            }

                            MessageBrokerClientReadPacketEvent.CreateAccepted( client, traceId, response.Header )
                                .Emit( client.Logger.ReadPacket );

                            MessageBrokerClientListenerUnboundEvent.Create(
                                    listener,
                                    traceId,
                                    parsedResponse.ChannelRemoved,
                                    parsedResponse.QueueRemoved )
                                .Emit( client.Logger.ListenerUnbound );

                            return MessageBrokerUnbindListenerResult.Create( parsedResponse.ChannelRemoved, parsedResponse.QueueRemoved );
                        }
                        case MessageBrokerClientEndpoint.UnbindListenerFailureResponse:
                        {
                            MessageBrokerClientReadPacketEvent.CreateReceived( client, traceId, response.Header )
                                .Emit( client.Logger.ReadPacket );

                            var exception = Protocol.AssertPayload(
                                client,
                                response.Header,
                                Protocol.UnbindListenerFailureResponse.Length );

                            if ( exception is not null )
                            {
                                MessageBrokerClientErrorEvent.Create( client, traceId, exception ).Emit( client.Logger.Error );
                                await client.DisposeAsync( traceId ).ConfigureAwait( false );
                                return exception;
                            }

                            var parsedResponse = Protocol.UnbindListenerFailureResponse.Parse( response.Data );
                            MessageBrokerClientReadPacketEvent.CreateAccepted( client, traceId, response.Header )
                                .Emit( client.Logger.ReadPacket );

                            return client.EmitError(
                                Protocol.RequestException( client, request.Header, parsedResponse.StringifyErrors( listener ) ),
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
            finally
            {
                await endDispose.ConfigureAwait( false );
            }
        }
    }

    internal MessageBrokerListener[] Clear()
    {
        return _store.ClearAndExtract();
    }
}
