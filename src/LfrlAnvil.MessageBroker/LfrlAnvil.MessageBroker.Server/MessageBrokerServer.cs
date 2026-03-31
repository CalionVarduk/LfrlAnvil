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
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Async;
using LfrlAnvil.Chrono;
using LfrlAnvil.Chrono.Async;
using LfrlAnvil.Computable.Expressions;
using LfrlAnvil.Diagnostics;
using LfrlAnvil.Exceptions;
using LfrlAnvil.Extensions;
using LfrlAnvil.Memory;
using LfrlAnvil.MessageBroker.Server.Events;
using LfrlAnvil.MessageBroker.Server.Exceptions;
using LfrlAnvil.MessageBroker.Server.Internal;

namespace LfrlAnvil.MessageBroker.Server;

/// <summary>
/// Represents a message broker server.
/// </summary>
public sealed class MessageBrokerServer : IDisposable, IAsyncDisposable
{
    internal ClientListener ClientListener;
    internal RemoteClientCollection RemoteClientCollection;
    internal RemoteClientConnectorCollection RemoteClientConnectorCollection;
    internal ChannelCollection ChannelCollection;
    internal StreamCollection StreamCollection;
    internal readonly MemoryPool<byte> MemoryPool;
    internal readonly Func<MessageBrokerRemoteClient, MessageBrokerRemoteClientLogger?>? RemoteClientLoggerFactory;
    internal readonly Func<MessageBrokerChannel, MessageBrokerChannelLogger?>? ChannelLoggerFactory;
    internal readonly Func<MessageBrokerStream, MessageBrokerStreamLogger?>? StreamLoggerFactory;
    internal readonly Func<MessageBrokerQueue, MessageBrokerQueueLogger?>? QueueLoggerFactory;

    internal readonly ServerStorage Storage;
    internal readonly MessageBrokerRemoteClientStreamDecorator? StreamDecorator;
    internal readonly Func<MessageBrokerRemoteClient, ITimestampProvider> TimestampsFactory;
    internal readonly Func<MessageBrokerRemoteClient, ValueTaskDelaySource>? DelaySourceFactory;
    internal readonly MessageBrokerServerLogger Logger;
    internal readonly int MaxNetworkMessagePacketBytes;

    private readonly TcpListener _listener;
    private readonly TaskCompletionSource _disposed;
    private FileStream? _storageLock;
    private MessageBrokerServerState _state;
    private bool _storageLoaded;
    private ulong _nextTraceId;

    /// <summary>
    /// Creates a new <see cref="MessageBrokerServer"/> instance.
    /// </summary>
    /// <param name="localEndPoint">The <see cref="IPEndPoint"/> of this server's listener socket.</param>
    /// <param name="options">Optional creation options.</param>
    public MessageBrokerServer(IPEndPoint localEndPoint, MessageBrokerServerOptions options = default)
    {
        Storage = ServerStorage.Create( options.RootStoragePath );
        _listener = new TcpListener( localEndPoint );
        LocalEndPoint = localEndPoint;

        HandshakeTimeout = Defaults.Temporal.GetActualTimeout( options.HandshakeTimeout );
        AcceptableMessageTimeout = Defaults.Temporal.GetActualTimeoutBounds( options.AcceptableMessageTimeout );
        AcceptablePingInterval = Defaults.Temporal.GetActualPingIntervalBounds( options.AcceptablePingInterval );
        MaxNetworkPacketLength = Defaults.Memory.GetActualMaxNetworkPacketLength( options.NetworkPacket.MaxLength );
        MaxNetworkMessagePacketLength = Defaults.Memory.GetActualMaxNetworkLargePacketLength(
            MaxNetworkPacketLength,
            options.NetworkPacket.MaxMessageLength );

        MaxNetworkMessagePacketBytes = unchecked( ( int )MaxNetworkMessagePacketLength.Bytes
            - Math.Max( Protocol.PushMessageHeader.Length, Protocol.MessageNotificationHeader.Payload )
            + Protocol.PushMessageHeader.Length );

        AcceptableMaxNetworkBatchPacketLength = Bounds.Create(
            MaxNetworkPacketLength,
            Defaults.Memory.GetActualMaxNetworkLargePacketLength(
                MaxNetworkPacketLength,
                options.NetworkPacket.MaxBatchLength ) );

        AcceptableMaxBatchPacketCount = Defaults.Memory.GetActualBatchPacketCountBounds( options.NetworkPacket.MaxBatchPacketCount );
        ExpressionFactory = options.ExpressionFactory;
        Logger = options.Logger ?? default;
        RemoteClientLoggerFactory = options.ClientLoggerFactory;
        ChannelLoggerFactory = options.ChannelLoggerFactory;
        StreamLoggerFactory = options.StreamLoggerFactory;
        QueueLoggerFactory = options.QueueLoggerFactory;
        TimestampsFactory = options.TimestampsFactory ?? (static _ => TimestampProvider.Shared);
        DelaySourceFactory = options.DelaySourceFactory;

        StreamDecorator = options.StreamDecorator;
        _state = MessageBrokerServerState.Created;
        _nextTraceId = 0;

        MemoryPool = new MemoryPool<byte>(
            unchecked( ( int )MaxNetworkMessagePacketLength.Max( AcceptableMaxNetworkBatchPacketLength.Max ).Bytes ) );

        _disposed = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
        ClientListener = ClientListener.Create();
        RemoteClientCollection = RemoteClientCollection.Create();
        RemoteClientConnectorCollection = RemoteClientConnectorCollection.Create( options.Tcp );
        ChannelCollection = ChannelCollection.Create();
        StreamCollection = StreamCollection.Create();
    }

    /// <summary>
    /// Handshake timeout for newly connected clients.
    /// </summary>
    public Duration HandshakeTimeout { get; }

    /// <summary>
    /// Range of acceptable send or receive message timeout values.
    /// </summary>
    /// <remarks>Acts as a limit imposed on client's desired message timeout during handshake.</remarks>
    public Bounds<Duration> AcceptableMessageTimeout { get; }

    /// <summary>
    /// Range of acceptable send ping interval values.
    /// </summary>
    /// <remarks>Acts as a limit imposed on client's desired ping interval during handshake.</remarks>
    public Bounds<Duration> AcceptablePingInterval { get; }

    /// <summary>
    /// Max acceptable network packet length.
    /// </summary>
    /// <remarks>
    /// Represents max possible length for packets not handled
    /// by either <see cref="MaxNetworkMessagePacketLength"/> or <see cref="MessageBrokerRemoteClient.MaxNetworkBatchPacketLength"/>.
    /// </remarks>
    public MemorySize MaxNetworkPacketLength { get; }

    /// <summary>
    /// Max acceptable network message packet length.
    /// </summary>
    /// <remarks>
    /// Represents max possible length for outbound packets of <see cref="MessageBrokerClientEndpoint.MessageNotification"/> type
    /// or inbound packets of <see cref="MessageBrokerServerEndpoint.PushMessage"/> type.
    /// </remarks>
    public MemorySize MaxNetworkMessagePacketLength { get; }

    /// <summary>
    /// Range of acceptable max network batch packet length values.
    /// </summary>
    /// <remarks>
    /// Represents a range of max possible length values for packets of <b>Batch</b> type.
    /// Acts as a limit imposed on client's desired max network batch packet length during handshake.
    /// </remarks>
    public Bounds<MemorySize> AcceptableMaxNetworkBatchPacketLength { get; }

    /// <summary>
    /// Range of acceptable max number of packets in a single network batch packet values.
    /// </summary>
    /// <remarks>Acts as a limit imposed on client's desired max batch packet count during handshake.</remarks>
    public Bounds<short> AcceptableMaxBatchPacketCount { get; }

    /// <summary>
    /// The local <see cref="EndPoint"/> of this server's listener socket.
    /// </summary>
    public EndPoint LocalEndPoint { get; private set; }

    /// <summary>
    /// Factory of parsed expressions for listener message filter predicates.
    /// </summary>
    public IParsedExpressionFactory? ExpressionFactory { get; }

    /// <summary>
    /// Specifies the root directory for permanent server storage.
    /// Lack of root directory will cause the server to be ephemeral and work in in-memory mode.
    /// </summary>
    public string? RootStorageDirectoryPath => Storage.ServerRootDir;

    /// <summary>
    /// Current server's state.
    /// </summary>
    /// <remarks>See <see cref="MessageBrokerServerState"/> for more information.</remarks>
    public MessageBrokerServerState State
    {
        get
        {
            using ( AcquireLock() )
                return _state;
        }
    }

    /// <summary>
    /// Collection of attached <see cref="MessageBrokerRemoteClientConnector"/> instances.
    /// </summary>
    public MessageBrokerRemoteClientConnectorCollection Connectors => new MessageBrokerRemoteClientConnectorCollection( this );

    /// <summary>
    /// Collection of attached <see cref="MessageBrokerRemoteClient"/> instances.
    /// </summary>
    public MessageBrokerRemoteClientCollection Clients => new MessageBrokerRemoteClientCollection( this );

    /// <summary>
    /// Collection of attached <see cref="MessageBrokerChannel"/> instances.
    /// </summary>
    public MessageBrokerChannelCollection Channels => new MessageBrokerChannelCollection( this );

    /// <summary>
    /// Collection of attached <see cref="MessageBrokerStream"/> instances.
    /// </summary>
    public MessageBrokerStreamCollection Streams => new MessageBrokerStreamCollection( this );

    internal bool IsDisposed => _state >= MessageBrokerServerState.Disposing;

    /// <inheritdoc />
    public void Dispose()
    {
        DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        var traceId = 0UL;
        MessageBrokerServerState state;
        using ( AcquireLock() )
        {
            state = _state;
            if ( TryBeginDispose() )
                traceId = GetTraceId();
        }

        if ( state >= MessageBrokerServerState.Disposing )
        {
            if ( state == MessageBrokerServerState.Disposing )
                await _disposed.Task.ConfigureAwait( false );

            return;
        }

        using ( MessageBrokerServerTraceEvent.CreateScope( this, traceId, MessageBrokerServerTraceEventType.Dispose ) )
            await DisposeAsyncCore( traceId ).ConfigureAwait( false );
    }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerServer"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"{LocalEndPoint} server ({State})";
    }

    /// <summary>
    /// Attempts to initialize the server and start listening for client connections.
    /// </summary>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/>.</param>
    /// <returns>A task that represents the operation, which returns a <see cref="Result"/> instance.</returns>
    /// <exception cref="OperationCanceledException">When <paramref name="cancellationToken"/> has been cancelled.</exception>
    /// <exception cref="MessageBrokerServerDisposedException">When this server has already been disposed.</exception>
    /// <exception cref="MessageBrokerServerStateException">
    /// When this server is not disposed and not in <see cref="MessageBrokerServerState.Created"/> state.
    /// </exception>
    /// <remarks>
    /// Errors encountered during server initialization will cause it to be automatically disposed.
    /// Returned <see cref="Result"/> will only be valid when the server will successfully start listening for client connections
    /// and proceed to the <see cref="MessageBrokerServerState.Running"/> state.
    /// </remarks>
    public async ValueTask<Result> StartAsync(CancellationToken cancellationToken = default)
    {
        using ( AcquireLock() )
        {
            if ( IsDisposed )
                ExceptionThrower.Throw( this.DisposedException() );

            AssertState( MessageBrokerServerState.Created );
        }

        ulong traceId;
        var @continue = false;
        var storageLock = await Storage.LockAsync( this ).ConfigureAwait( false );
        try
        {
            var metadata = await Storage.LoadMetadataAsync( this ).ConfigureAwait( false );
            using ( AcquireLock() )
            {
                if ( IsDisposed )
                    ExceptionThrower.Throw( this.DisposedException() );

                if ( metadata is not null )
                    _nextTraceId = metadata.Value.TraceId;

                _storageLock = storageLock;
                traceId = GetTraceId();
            }

            @continue = true;
        }
        catch ( MessageBrokerServerStorageException exc )
        {
            return exc;
        }
        finally
        {
            if ( ! @continue && storageLock is not null )
                await storageLock.DisposeAsync().ConfigureAwait( false );
        }

        using ( MessageBrokerServerTraceEvent.CreateScope( this, traceId, MessageBrokerServerTraceEventType.Start ) )
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
            }
            catch ( Exception exc )
            {
                if ( Logger.Error is { } error )
                    error.Emit( MessageBrokerServerErrorEvent.Create( this, traceId, exc ) );

                throw;
            }

            try
            {
                using ( AcquireActiveLock( traceId, out var exc ) )
                {
                    if ( exc is not null )
                        return exc;

                    AssertState( MessageBrokerServerState.Created );
                    _state = MessageBrokerServerState.Starting;
                }

                if ( Storage.ServerRootDir is not null )
                {
                    if ( Logger.StorageLoading is { } storageLoading )
                        storageLoading.Emit( MessageBrokerServerStorageLoadingEvent.Create( this, traceId, Storage.ServerRootDir ) );

                    var result = await ChannelCollection.LoadChannelsAsync( this, traceId, cancellationToken ).ConfigureAwait( false );
                    if ( result.Exception is not null )
                        return result.Exception;

                    result = await StreamCollection.LoadStreamsAsync( this, traceId, cancellationToken ).ConfigureAwait( false );
                    if ( result.Exception is not null )
                        return result.Exception;

                    result = await RemoteClientCollection.LoadClientsAsync( this, traceId, cancellationToken ).ConfigureAwait( false );
                    if ( result.Exception is not null )
                        return result.Exception;

                    ReadOnlyArray<MessageBrokerChannel> channels;
                    ReadOnlyArray<MessageBrokerStream> streams;
                    ReadOnlyArray<MessageBrokerRemoteClient> clients;
                    using ( AcquireActiveLock( traceId, out var exc ) )
                    {
                        if ( exc is not null )
                            return exc;

                        channels = ChannelCollection.GetAllUnsafe();
                        streams = StreamCollection.GetAllUnsafe();
                        clients = RemoteClientCollection.GetAllUnsafe();
                    }

                    foreach ( var stream in streams )
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        await stream.Storage.LoadMessagesAsync( stream, traceId ).ConfigureAwait( false );
                    }

                    var queueCount = 0;
                    var publisherCount = 0;
                    var listenerCount = 0;
                    foreach ( var client in clients )
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        ReadOnlyArray<MessageBrokerQueue> queues;
                        using ( client.AcquireLock() )
                        {
                            queues = client.QueueStore.GetAll();
                            queueCount += queues.Count;
                            publisherCount += client.PublishersByChannelId.Count;
                            listenerCount += client.ListenersByChannelId.Count;
                        }

                        foreach ( var queue in queues )
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            await queue.Storage.LoadMessagesAsync( queue, traceId ).ConfigureAwait( false );
                        }
                    }

                    foreach ( var stream in streams )
                        stream.FinalizeMessageReferences( traceId );

                    await Parallel.ForEachAsync( channels, cancellationToken, (c, _) => c.TryDisposeDueToLackOfReferencesAsync( traceId ) )
                        .ConfigureAwait( false );

                    using ( AcquireActiveLock( traceId, out var exc ) )
                    {
                        if ( exc is not null )
                            return exc;

                        _storageLoaded = true;
                    }

                    if ( Logger.StorageLoaded is { } storageLoaded )
                        storageLoaded.Emit(
                            MessageBrokerServerStorageLoadedEvent.Create(
                                this,
                                traceId,
                                Storage.ServerRootDir,
                                channels.Count,
                                streams.Count,
                                clients.Count,
                                queueCount,
                                publisherCount,
                                listenerCount ) );

                    foreach ( var stream in streams )
                        stream.StartProcessor();
                }

                if ( Logger.ListenerStarting is { } listenerStarting )
                    listenerStarting.Emit( MessageBrokerServerListenerStartingEvent.Create( this, traceId ) );

                using ( AcquireActiveLock( traceId, out var exc ) )
                {
                    if ( exc is not null )
                        return exc;

                    _listener.Start();
                    LocalEndPoint = _listener.LocalEndpoint;
                    _state = MessageBrokerServerState.Running;
                }

                if ( Logger.ListenerStarted is { } listenerStarted )
                    listenerStarted.Emit( MessageBrokerServerListenerStartedEvent.Create( this, traceId ) );

                cancellationToken.ThrowIfCancellationRequested();

                var clientListenerTask = ClientListener.StartUnderlyingTask( this, _listener );
                using ( AcquireActiveLock( traceId, out var exc ) )
                {
                    if ( exc is not null )
                        return exc;

                    ClientListener.SetUnderlyingTask( clientListenerTask );
                }
            }
            catch ( Exception exc )
            {
                if ( Logger.Error is { } error )
                    error.Emit( MessageBrokerServerErrorEvent.Create( this, traceId, exc ) );

                if ( exc is MessageBrokerServerStateException )
                    throw;

                await DisposeAsync( traceId ).ConfigureAwait( false );
                return exc;
            }

            return Result.Valid;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ExclusiveLock AcquireLock()
    {
        return ExclusiveLock.Enter( _listener );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ExclusiveLock AcquireActiveLock(ulong traceId, out MessageBrokerServerDisposedException? exception)
    {
        var @lock = AcquireLock();
        if ( ! IsDisposed )
        {
            exception = null;
            return @lock;
        }

        @lock.Dispose();
        exception = this.DisposedException();
        if ( Logger.Error is { } error )
            error.Emit( MessageBrokerServerErrorEvent.Create( this, traceId, exception ) );

        return default;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void EmitError(Result result, ulong traceId)
    {
        if ( result.Exception is not null && Logger.Error is { } error )
            error.Emit( MessageBrokerServerErrorEvent.Create( this, traceId, result.Exception ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void EmitErrors(ref Chain<Exception> exceptions, ulong traceId)
    {
        if ( exceptions.Count > 0 && Logger.Error is { } error )
        {
            foreach ( var exc in exceptions )
                error.Emit( MessageBrokerServerErrorEvent.Create( this, traceId, exc ) );
        }

        exceptions = Chain<Exception>.Empty;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ulong GetTraceId()
    {
        return unchecked( _nextTraceId++ );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool TryBeginDispose()
    {
        if ( IsDisposed )
            return false;

        _state = MessageBrokerServerState.Disposing;
        return true;
    }

    internal ValueTask DisposeAsync(ulong traceId, bool ignoreClientListenerTask = false)
    {
        using ( AcquireLock() )
        {
            if ( IsDisposed )
                return ValueTask.CompletedTask;

            _state = MessageBrokerServerState.Disposing;
        }

        return DisposeAsyncCore( traceId, ignoreClientListenerTask );
    }

    internal async ValueTask DisposeAsyncCore(ulong traceId, bool ignoreClientListenerTask = false)
    {
        FileStream? storageLock = null;
        try
        {
            if ( Logger.Disposing is { } disposing )
                disposing.Emit( MessageBrokerServerDisposingEvent.Create( this, traceId ) );

            bool storageLoaded;
            var exceptions = Chain<Exception>.Empty;
            MessageBrokerRemoteClientConnector[] connectors;
            MessageBrokerRemoteClient[] clients;
            MessageBrokerStream[] streams;
            MessageBrokerChannel[] channels;
            Task? clientListenerTask;

            using ( AcquireLock() )
            {
                storageLoaded = _storageLoaded;
                storageLock = _storageLock;
                _storageLock = null;

                clientListenerTask = ClientListener.DiscardUnderlyingTask();
                if ( ignoreClientListenerTask )
                    clientListenerTask = null;

                try
                {
                    _listener.Stop();
                }
                catch ( Exception exc )
                {
                    exceptions = exceptions.Extend( exc );
                    clientListenerTask = null;
                }

                connectors = RemoteClientConnectorCollection.DisposeUnsafe( ref exceptions );
                clients = RemoteClientCollection.DisposeUnsafe();
                channels = ChannelCollection.DisposeUnsafe();
                streams = StreamCollection.DisposeUnsafe();
            }

            EmitErrors( ref exceptions, traceId );

            foreach ( var connector in connectors )
                connector.OnServerDisposing( traceId );

            EmitError(
                await Parallel.ForEachAsync( streams, (s, _) => s.OnServerDisposingAsync( traceId ) ).AsSafe().ConfigureAwait( false ),
                traceId );

            foreach ( var channel in channels )
                channel.OnServerDisposing( traceId );

            EmitError(
                await Parallel.ForEachAsync( clients, (c, _) => c.OnServerDisposedAsync( traceId, storageLoaded ) )
                    .AsSafe()
                    .ConfigureAwait( false ),
                traceId );

            EmitError(
                await Parallel.ForEachAsync( streams, (s, _) => s.OnServerDisposedAsync( traceId, storageLoaded ) )
                    .AsSafe()
                    .ConfigureAwait( false ),
                traceId );

            EmitError(
                await Parallel.ForEachAsync( channels, (c, _) => c.OnServerDisposedAsync( traceId, storageLoaded ) )
                    .AsSafe()
                    .ConfigureAwait( false ),
                traceId );

            EmitError(
                await Parallel.ForEachAsync( connectors, (c, _) => c.OnServerDisposedAsync( traceId ) ).AsSafe().ConfigureAwait( false ),
                traceId );

            EmitError( await clientListenerTask.AsSafeCancellable().ConfigureAwait( false ), traceId );
            EmitError( await Storage.SaveMetadataAsync( this, traceId ).AsSafe().ConfigureAwait( false ), traceId );

            using ( AcquireLock() )
                _state = MessageBrokerServerState.Disposed;

            if ( Logger.Disposed is { } disposed )
                disposed.Emit( MessageBrokerServerDisposedEvent.Create( this, traceId ) );
        }
        finally
        {
            try
            {
                if ( storageLock is not null )
                    await storageLock.DisposeAsync().ConfigureAwait( false );
            }
            finally
            {
                _disposed.TrySetResult();
            }
        }
    }

    [StackTraceHidden]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void AssertState(MessageBrokerServerState expected)
    {
        if ( _state != expected )
            ExceptionThrower.Throw( new MessageBrokerServerStateException( this, _state, expected ) );
    }
}
