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

    internal readonly DirectoryInfo? RootClientsDirectory;
    internal readonly DirectoryInfo? RootChannelsDirectory;
    internal readonly DirectoryInfo? RootStreamsDirectory;
    internal readonly MessageBrokerRemoteClientStreamDecorator? StreamDecorator;
    internal readonly Func<MessageBrokerRemoteClient, ITimestampProvider> TimestampsFactory;
    internal readonly Func<MessageBrokerRemoteClient, ValueTaskDelaySource>? DelaySourceFactory;
    internal readonly MessageBrokerServerLogger Logger;

    private readonly TcpListener _listener;
    private MessageBrokerServerState _state;
    private ulong _nextTraceId;

    /// <summary>
    /// Creates a new <see cref="MessageBrokerServer"/> instance.
    /// </summary>
    /// <param name="localEndPoint">The <see cref="IPEndPoint"/> of this server's listener socket.</param>
    /// <param name="options">Optional creation options.</param>
    public MessageBrokerServer(IPEndPoint localEndPoint, MessageBrokerServerOptions options = default)
    {
        if ( string.IsNullOrWhiteSpace( options.RootStoragePath ) )
        {
            RootStorageDirectory = null;
            RootClientsDirectory = null;
            RootChannelsDirectory = null;
            RootStreamsDirectory = null;
        }
        else
        {
            RootStorageDirectory = new DirectoryInfo( options.RootStoragePath );
            RootClientsDirectory = new DirectoryInfo( Path.Combine( options.RootStoragePath, "clients" ) );
            RootChannelsDirectory = new DirectoryInfo( Path.Combine( options.RootStoragePath, "channels" ) );
            RootStreamsDirectory = new DirectoryInfo( Path.Combine( options.RootStoragePath, "streams" ) );
        }

        _listener = new TcpListener( localEndPoint );
        LocalEndPoint = localEndPoint;

        HandshakeTimeout = Defaults.Temporal.GetActualTimeout( options.HandshakeTimeout );
        AcceptableMessageTimeout = Defaults.Temporal.GetActualTimeoutBounds( options.AcceptableMessageTimeout );
        AcceptablePingInterval = Defaults.Temporal.GetActualPingIntervalBounds( options.AcceptablePingInterval );
        MaxNetworkPacketLength = Defaults.Memory.GetActualMaxNetworkPacketLength( options.NetworkPacket.MaxLength );
        MaxNetworkMessagePacketLength = Defaults.Memory.GetActualMaxNetworkLargePacketLength(
            MaxNetworkPacketLength,
            options.NetworkPacket.MaxMessageLength );

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

        ClientListener = ClientListener.Create();
        RemoteClientCollection = RemoteClientCollection.Create();
        RemoteClientConnectorCollection = RemoteClientConnectorCollection.Create( options.Tcp );
        ChannelCollection = ChannelCollection.Create();
        StreamCollection = StreamCollection.Create();
    }

    /// <summary>
    /// Specifies the root directory for permanent server storage. Lack of root directory will cause the server to work in in-memory mode.
    /// </summary>
    public DirectoryInfo? RootStorageDirectory { get; }

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

    internal bool ShouldCancel => _state >= MessageBrokerServerState.Disposing;

    /// <inheritdoc />
    public void Dispose()
    {
        DisposeAsync().AsTask().Wait();
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        ulong traceId;
        using ( AcquireLock() )
        {
            if ( ! TryBeginDispose() )
                return;

            traceId = GetTraceId();
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
        ulong traceId;
        using ( AcquireLock() )
        {
            if ( ShouldCancel )
                ExceptionThrower.Throw( this.DisposedException() );

            traceId = GetTraceId();
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

            try
            {
                if ( RootStorageDirectory is not null )
                {
                    Assume.IsNotNull( RootClientsDirectory );
                    Assume.IsNotNull( RootChannelsDirectory );
                    Assume.IsNotNull( RootStreamsDirectory );
                    RootStorageDirectory.Create();
                    RootClientsDirectory.Create();
                    RootChannelsDirectory.Create();
                    RootStreamsDirectory.Create();
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

                await DisposeAsync( traceId ).ConfigureAwait( false );
                return exc;
            }

            return Result.Valid;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ExclusiveLock AcquireLock()
    {
        return ExclusiveLock.SpinWaitEnter( _listener, spinWaitMultiplier: 4 );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ExclusiveLock AcquireActiveLock(ulong traceId, out MessageBrokerServerDisposedException? exception)
    {
        var @lock = AcquireLock();
        if ( ! ShouldCancel )
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
    internal ulong GetTraceId()
    {
        return unchecked( _nextTraceId++ );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool TryBeginDispose()
    {
        if ( ShouldCancel )
            return false;

        _state = MessageBrokerServerState.Disposing;
        return true;
    }

    internal ValueTask DisposeAsync(ulong traceId)
    {
        using ( AcquireLock() )
        {
            if ( ShouldCancel )
                return ValueTask.CompletedTask;

            _state = MessageBrokerServerState.Disposing;
        }

        return DisposeAsyncCore( traceId );
    }

    internal async ValueTask DisposeAsyncCore(ulong traceId)
    {
        if ( Logger.Disposing is { } disposing )
            disposing.Emit( MessageBrokerServerDisposingEvent.Create( this, traceId ) );

        var exceptions = Chain<Exception>.Empty;
        MessageBrokerRemoteClientConnector[] connectors;
        MessageBrokerRemoteClient[] clients;
        MessageBrokerStream[] streams;
        MessageBrokerChannel[] channels;
        Task? clientListenerTask;
        using ( AcquireLock() )
        {
            connectors = RemoteClientConnectorCollection.DisposeUnsafe();
            clients = RemoteClientCollection.DisposeUnsafe();
            channels = ChannelCollection.DisposeUnsafe();
            streams = StreamCollection.DisposeUnsafe();
            clientListenerTask = ClientListener.DiscardUnderlyingTask();

            try
            {
                _listener.Stop();
            }
            catch ( Exception exc )
            {
                exceptions = exceptions.Extend( exc );
                clientListenerTask = null;
            }

            RemoteClientConnectorCollection.DisposeCancellationSources( ref exceptions );
        }

        this.TryEmitErrors( traceId, exceptions );

        await Parallel.ForEachAsync(
                connectors,
                async (c, _) =>
                {
                    var result = await c.CancelAsync().ConfigureAwait( false );
                    if ( result.Exception is not null && Logger.Error is { } err )
                        err.Emit( MessageBrokerServerErrorEvent.Create( this, traceId, result.Exception ) );
                } )
            .ConfigureAwait( false );

        await Parallel.ForEachAsync( streams, (s, _) => s.OnServerDisposedAsync( traceId ) ).ConfigureAwait( false );
        foreach ( var channel in channels )
            channel.OnServerDisposed( traceId );

        await Parallel.ForEachAsync( clients, (c, _) => c.OnServerDisposedAsync( traceId ) ).ConfigureAwait( false );
        foreach ( var stream in streams )
            stream.ClearMessageStore();

        var taskResult = await clientListenerTask.SafeWaitAsync().ConfigureAwait( false );
        if ( taskResult.Exception is not null && Logger.Error is { } error )
            error.Emit( MessageBrokerServerErrorEvent.Create( this, traceId, taskResult.Exception ) );

        using ( AcquireLock() )
            _state = MessageBrokerServerState.Disposed;

        if ( Logger.Disposed is { } disposed )
            disposed.Emit( MessageBrokerServerDisposedEvent.Create( this, traceId ) );
    }

    [StackTraceHidden]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void AssertState(MessageBrokerServerState expected)
    {
        if ( _state != expected )
            ExceptionThrower.Throw( new MessageBrokerServerStateException( this, _state, expected ) );
    }
}
