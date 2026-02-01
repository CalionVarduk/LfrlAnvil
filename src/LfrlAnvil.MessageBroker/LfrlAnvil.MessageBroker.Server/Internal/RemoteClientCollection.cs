// Copyright 2025-2026 Łukasz Furlepa
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
using System.IO;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Computable.Expressions;
using LfrlAnvil.Exceptions;
using LfrlAnvil.MessageBroker.Server.Events;
using LfrlAnvil.MessageBroker.Server.Exceptions;

namespace LfrlAnvil.MessageBroker.Server.Internal;

internal struct RemoteClientCollection
{
    private ObjectStore<MessageBrokerRemoteClient> _store;

    private RemoteClientCollection(StringComparer nameComparer)
    {
        _store = ObjectStore<MessageBrokerRemoteClient>.Create( nameComparer );
    }

    [Pure]
    internal static RemoteClientCollection Create()
    {
        return new RemoteClientCollection( StringComparer.OrdinalIgnoreCase );
    }

    [Pure]
    internal static int GetCount(MessageBrokerServer server)
    {
        using ( server.AcquireLock() )
            return server.RemoteClientCollection._store.Count;
    }

    [Pure]
    internal static ReadOnlyArray<MessageBrokerRemoteClient> GetAll(MessageBrokerServer server)
    {
        using ( server.AcquireLock() )
            return server.RemoteClientCollection.GetAllUnsafe();
    }

    [Pure]
    internal static MessageBrokerRemoteClient? TryGetById(MessageBrokerServer server, int id)
    {
        using ( server.AcquireLock() )
            return server.RemoteClientCollection.TryGetByIdUnsafe( id );
    }

    [Pure]
    internal static MessageBrokerRemoteClient? TryGetByName(MessageBrokerServer server, string name)
    {
        using ( server.AcquireLock() )
            return server.RemoteClientCollection._store.TryGetByName( name );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static Result<MessageBrokerRemoteClient> TryRegisterUnsafe(
        MessageBrokerServer server,
        TcpClient tcp,
        Stream stream,
        string name,
        in Protocol.HandshakeRequestHeader handshake,
        out bool alreadyConnected)
    {
        alreadyConnected = false;
        try
        {
            var token = server.RemoteClientCollection._store.GetOrAddNull( name );
            if ( token.Exists )
            {
                // TODO
                // this will have to change
                // if non-ephemeral client exists, but is disposed/inactive, then it should be possible to reconnect to it
                // if it's not disposed (but e.g. disposing), then yeah, return already-connected error
                // may lead to some minor race conditions e.g. when
                // server disconnects the client, client catches that and attempts to reconnect immediately, but server is still cleaning it up
                // but I think that's fine
                //
                // also, be careful about allowing to reconnect to the same client with different sockets at the same time
                // probably change client's state to Created immediately?
                alreadyConnected = true;
                return server.Exception( Resources.ClientAlreadyConnected( name ) );
            }

            try
            {
                return token.SetObject(
                    ref server.RemoteClientCollection._store,
                    MessageBrokerRemoteClient.Create(
                        token.Id,
                        server,
                        name,
                        tcp,
                        stream,
                        handshake.MessageTimeout,
                        handshake.PingInterval,
                        handshake.MaxBatchPacketCount,
                        handshake.MaxNetworkBatchPacketLength,
                        handshake.IsClientLittleEndian,
                        handshake.SynchronizeExternalObjectNames,
                        handshake.ClearBuffers,
                        handshake.IsEphemeral ) );
            }
            catch
            {
                token.Revert( ref server.RemoteClientCollection._store, name );
                throw;
            }
        }
        catch ( Exception exc )
        {
            return exc;
        }
    }

    [Pure]
    internal ReadOnlyArray<MessageBrokerRemoteClient> GetAllUnsafe()
    {
        return _store.GetAll();
    }

    [Pure]
    internal MessageBrokerRemoteClient? TryGetByIdUnsafe(int id)
    {
        return _store.TryGetById( id );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static Result Remove(MessageBrokerRemoteClient client)
    {
        try
        {
            using ( client.Server.AcquireLock() )
            {
                if ( ! client.Server.IsDisposed )
                    client.Server.RemoteClientCollection._store.Remove( client.Id, client.Name );
            }
        }
        catch ( Exception exc )
        {
            return exc;
        }

        return Result.Valid;
    }

    internal MessageBrokerRemoteClient[] DisposeUnsafe()
    {
        return _store.Clear();
    }

    internal static async ValueTask<Result> LoadClientsAsync(MessageBrokerServer server, ulong traceId, CancellationToken cancellationToken)
    {
        await foreach ( var info in server.Storage.LoadClientsAsync( server, traceId )
            .WithCancellation( cancellationToken )
            .ConfigureAwait( false ) )
        {
            MessageBrokerRemoteClient client;
            using ( server.AcquireActiveLock( traceId, out var exc ) )
            {
                if ( exc is not null )
                    return exc;

                // TODO: tests
                // - client duplicate (by id or name) (by id may not be possible due to file name constraints)
                client = MessageBrokerRemoteClient.CreateInactive( info.Key, server, info.Value.Name.Value.ToString(), info.Value.TraceId );
                if ( ! server.RemoteClientCollection._store.TryAdd( client.Id, client.Name, client ) )
                    ExceptionThrower.Throw( server.Exception( Resources.RecreatedClientDuplicate( client.Id, client.Name ) ) );
            }

            ulong clientTraceId;
            using ( client.AcquireLock() )
                clientTraceId = client.GetTraceId();

            using ( MessageBrokerRemoteClientTraceEvent.CreateScope(
                client,
                clientTraceId,
                MessageBrokerRemoteClientTraceEventType.Recreated ) )
            {
                if ( client.Logger.ServerTrace is { } serverTrace )
                    serverTrace.Emit( MessageBrokerRemoteClientServerTraceEvent.Create( client, clientTraceId, traceId ) );

                var nestedResult = await LoadQueuesAsync( client, clientTraceId, cancellationToken ).ConfigureAwait( false );
                if ( nestedResult.Exception is not null )
                    return nestedResult.Exception;

                nestedResult = await LoadPublishersAsync( client, traceId, clientTraceId, cancellationToken )
                    .ConfigureAwait( false );

                if ( nestedResult.Exception is not null )
                    return nestedResult.Exception;

                nestedResult = await LoadListenersAsync( client, traceId, clientTraceId, cancellationToken )
                    .ConfigureAwait( false );

                if ( nestedResult.Exception is not null )
                    return nestedResult.Exception;
            }
        }

        return Result.Valid;
    }

    private static async ValueTask<Result> LoadQueuesAsync(
        MessageBrokerRemoteClient client,
        ulong clientTraceId,
        CancellationToken cancellationToken)
    {
        await foreach ( var info in client.Storage.LoadQueuesAsync( client, clientTraceId )
            .WithCancellation( cancellationToken )
            .ConfigureAwait( false ) )
        {
            MessageBrokerQueue queue;
            using ( client.AcquireLock() )
            {
                if ( client.IsInactive )
                    break;

                // TODO: tests
                // - queue duplicate (by id or name)
                queue = MessageBrokerQueue.CreateInactive( client, info.Key, info.Value.Name.Value.ToString(), info.Value.TraceId );
                if ( ! client.QueueStore.TryAdd( queue.Id, queue.Name, queue ) )
                    ExceptionThrower.Throw( client.Exception( Resources.RecreatedQueueDuplicate( client, queue.Id, queue.Name ) ) );
            }

            ulong queueTraceId;
            using ( queue.AcquireLock() )
                queueTraceId = queue.GetTraceId();

            using ( MessageBrokerQueueTraceEvent.CreateScope( queue, queueTraceId, MessageBrokerQueueTraceEventType.Recreated ) )
            {
                if ( queue.Logger.ClientTrace is { } clientTrace )
                    clientTrace.Emit( MessageBrokerQueueClientTraceEvent.Create( queue, queueTraceId, clientTraceId ) );
            }
        }

        return Result.Valid;
    }

    private static async ValueTask<Result> LoadPublishersAsync(
        MessageBrokerRemoteClient client,
        ulong traceId,
        ulong clientTraceId,
        CancellationToken cancellationToken)
    {
        await foreach ( var info in client.Storage.LoadPublishersAsync( client, clientTraceId )
            .WithCancellation( cancellationToken )
            .ConfigureAwait( false ) )
        {
            MessageBrokerChannel? channel;
            MessageBrokerStream? stream;
            using ( client.Server.AcquireActiveLock( traceId, out var exc ) )
            {
                if ( exc is not null )
                    return exc;

                channel = client.Server.ChannelCollection.TryGetByIdUnsafe( info.Key );
                stream = client.Server.StreamCollection.TryGetByIdUnsafe( info.Value.StreamId );
            }

            // TODO: tests
            // - channel doesn't exist
            // - stream doesn't exist
            // - publisher duplicate by channel id
            if ( channel is null )
                ExceptionThrower.Throw( client.Exception( Resources.PublisherChannelDoesNotExist( client, info.Key ) ) );

            if ( stream is null )
                ExceptionThrower.Throw(
                    client.Exception( Resources.PublisherStreamDoesNotExist( client, info.Key, info.Value.StreamId ) ) );

            MessageBrokerChannelPublisherBinding publisher;
            using ( client.AcquireLock() )
            {
                if ( client.IsInactive )
                    break;

                publisher = MessageBrokerChannelPublisherBinding.CreateInactive( client, channel, stream );
                if ( ! client.PublishersByChannelId.TryAdd( channel.Id, publisher ) )
                    ExceptionThrower.Throw( client.Exception( Resources.RecreatedPublisherDuplicate( client, channel ) ) );
            }

            ulong channelTraceId;
            using ( channel.AcquireLock() )
            {
                if ( channel.IsDisposed )
                    break;

                channelTraceId = channel.GetTraceId();
                channel.PublishersByClientId.Add( client.Id, publisher );
            }

            using ( MessageBrokerChannelTraceEvent.CreateScope(
                channel,
                channelTraceId,
                MessageBrokerChannelTraceEventType.BindPublisher ) )
            {
                if ( channel.Logger.ClientTrace is { } clientTrace )
                    clientTrace.Emit( MessageBrokerChannelClientTraceEvent.Create( channel, channelTraceId, client, clientTraceId ) );

                if ( channel.Logger.PublisherBound is { } channelPublisherBound )
                    channelPublisherBound.Emit(
                        MessageBrokerChannelPublisherBoundEvent.Create( publisher, channelTraceId, streamCreated: false ) );
            }

            ulong streamTraceId;
            using ( stream.AcquireLock() )
            {
                if ( stream.IsDisposed )
                    break;

                streamTraceId = stream.GetTraceId();
                stream.PublishersByClientChannelIdPair.Add( Pair.Create( client.Id, channel.Id ), publisher );
            }

            using ( MessageBrokerStreamTraceEvent.CreateScope( stream, streamTraceId, MessageBrokerStreamTraceEventType.BindPublisher ) )
            {
                if ( stream.Logger.ClientTrace is { } clientTrace )
                    clientTrace.Emit( MessageBrokerStreamClientTraceEvent.Create( stream, streamTraceId, client, clientTraceId ) );

                if ( stream.Logger.PublisherBound is { } streamPublisherBound )
                    streamPublisherBound.Emit(
                        MessageBrokerStreamPublisherBoundEvent.Create( publisher, streamTraceId, channelCreated: false ) );
            }

            if ( client.Logger.PublisherBound is { } publisherBound )
                publisherBound.Emit(
                    MessageBrokerRemoteClientPublisherBoundEvent.Create(
                        publisher,
                        clientTraceId,
                        channelCreated: false,
                        streamCreated: false ) );
        }

        return Result.Valid;
    }

    private static async ValueTask<Result> LoadListenersAsync(
        MessageBrokerRemoteClient client,
        ulong traceId,
        ulong clientTraceId,
        CancellationToken cancellationToken)
    {
        var error = client.Logger.Error;
        await foreach ( var info in client.Storage.LoadListenersAsync( client, clientTraceId )
            .WithCancellation( cancellationToken )
            .ConfigureAwait( false ) )
        {
            // TODO: tests
            // - sanitization + filter expression warnings
            // - channel doesn't exist
            // - queue doesn't exist
            // - listener duplicate by channel id
            var metadata = info.Value.Sanitize( out var warnings );

            string? filterExpression = null;
            IParsedExpressionDelegate<MessageBrokerFilterExpressionContext, bool>? filterExpressionDelegate = null;
            if ( metadata.Filter.Value.Length > 0 )
            {
                filterExpression = metadata.Filter.Value.ToString();
                if ( client.Server.ExpressionFactory is null )
                {
                    warnings = warnings.Extend( Resources.UnexpectedFilterExpression( filterExpression ) );
                    filterExpression = null;
                }
                else
                {
                    try
                    {
                        var expression = client.Server.ExpressionFactory.Create<MessageBrokerFilterExpressionContext, bool>(
                            filterExpression );

                        if ( expression.UnboundArguments.Count > 1 )
                        {
                            warnings = warnings.Extend(
                                Resources.InvalidFilterExpressionArgumentCount( filterExpression, expression.UnboundArguments ) );

                            filterExpression = null;
                        }
                        else
                            filterExpressionDelegate = expression.Compile();
                    }
                    catch ( Exception exc )
                    {
                        error?.Emit( MessageBrokerRemoteClientErrorEvent.Create( client, clientTraceId, exc ) );
                        warnings = warnings.Extend( Resources.InvalidFilterExpression( filterExpression! ) );
                        filterExpression = null;
                    }
                }
            }

            if ( warnings.Count > 0 )
                error?.Emit(
                    MessageBrokerRemoteClientErrorEvent.Create(
                        client,
                        clientTraceId,
                        client.Exception( Resources.ListenerMetadataWarnings( client, info.Key, warnings ) ) ) );

            MessageBrokerChannel? channel;
            using ( client.Server.AcquireActiveLock( traceId, out var exc ) )
            {
                if ( exc is not null )
                    return exc;

                channel = client.Server.ChannelCollection.TryGetByIdUnsafe( info.Key );
            }

            if ( channel is null )
                ExceptionThrower.Throw( client.Exception( Resources.ListenerChannelDoesNotExist( client, info.Key ) ) );

            MessageBrokerQueue? queue;
            MessageBrokerChannelListenerBinding listener;
            using ( client.AcquireLock() )
            {
                if ( client.IsInactive )
                    break;

                queue = client.QueueStore.TryGetById( metadata.QueueId );
                if ( queue is null )
                    ExceptionThrower.Throw( client.Exception( Resources.ListenerQueueDoesNotExist( client, channel, metadata.QueueId ) ) );

                listener = MessageBrokerChannelListenerBinding.CreateInactive(
                    client,
                    channel,
                    queue,
                    in metadata,
                    filterExpression,
                    filterExpressionDelegate );

                if ( ! client.ListenersByChannelId.TryAdd( channel.Id, listener ) )
                    ExceptionThrower.Throw( client.Exception( Resources.RecreatedListenerDuplicate( client, channel ) ) );
            }

            ulong channelTraceId;
            using ( channel.AcquireLock() )
            {
                if ( channel.IsDisposed )
                    break;

                channelTraceId = channel.GetTraceId();
                channel.ListenersByClientId.Add( client.Id, listener );
            }

            using ( MessageBrokerChannelTraceEvent.CreateScope( channel, channelTraceId, MessageBrokerChannelTraceEventType.BindListener ) )
            {
                if ( channel.Logger.ClientTrace is { } clientTrace )
                    clientTrace.Emit( MessageBrokerChannelClientTraceEvent.Create( channel, channelTraceId, client, clientTraceId ) );

                if ( channel.Logger.ListenerBound is { } channelListenerBound )
                    channelListenerBound.Emit(
                        MessageBrokerChannelListenerBoundEvent.Create( listener, channelTraceId, queueCreated: false ) );
            }

            ulong queueTraceId;
            using ( queue.AcquireLock() )
            {
                if ( queue.IsDisposed )
                    break;

                queueTraceId = queue.GetTraceId();
                queue.ListenersByChannelId.Add( channel.Id, listener );
            }

            using ( MessageBrokerQueueTraceEvent.CreateScope( queue, queueTraceId, MessageBrokerQueueTraceEventType.BindListener ) )
            {
                if ( queue.Logger.ClientTrace is { } clientTrace )
                    clientTrace.Emit( MessageBrokerQueueClientTraceEvent.Create( queue, queueTraceId, clientTraceId ) );

                if ( queue.Logger.ListenerBound is { } queueListenerBound )
                    queueListenerBound.Emit( MessageBrokerQueueListenerBoundEvent.Create( listener, queueTraceId, channelCreated: false ) );
            }

            if ( client.Logger.ListenerBound is { } listenerBound )
                listenerBound.Emit(
                    MessageBrokerRemoteClientListenerBoundEvent.Create(
                        listener,
                        clientTraceId,
                        channelCreated: false,
                        queueCreated: false ) );
        }

        return Result.Valid;
    }
}
