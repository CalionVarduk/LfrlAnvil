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
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Async;
using LfrlAnvil.Chrono;
using LfrlAnvil.Chrono.Async;
using LfrlAnvil.Exceptions;
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
    internal ChannelCollection ChannelCollection;
    internal QueueCollection QueueCollection;
    internal readonly Func<MessageBrokerRemoteClient, MessageBrokerRemoteClientEventHandler?>? RemoteClientEventHandlerFactory;
    internal readonly Func<MessageBrokerChannel, MessageBrokerChannelEventHandler?>? ChannelEventHandlerFactory;
    internal readonly Func<MessageBrokerQueue, MessageBrokerQueueEventHandler?>? QueueEventHandlerFactory;
    internal readonly Func<MessageBrokerChannelBinding, MessageBrokerChannelBindingEventHandler?>? ChannelBindingEventHandlerFactory;
    internal readonly Func<MessageBrokerSubscription, MessageBrokerSubscriptionEventHandler?>? SubscriptionEventHandlerFactory;
    internal readonly MessageBrokerRemoteClientStreamDecorator? StreamDecorator;
    internal readonly Func<MessageBrokerRemoteClient, ITimestampProvider> TimestampsFactory;
    internal readonly Func<MessageBrokerRemoteClient, ValueTaskDelaySource>? DelaySourceFactory;

    private readonly TcpListener _listener;
    private readonly MessageBrokerServerEventHandler? _eventHandler;
    private MessageBrokerServerState _state;

    /// <summary>
    /// Creates a new <see cref="MessageBrokerServer"/> instance.
    /// </summary>
    /// <param name="localEndPoint">The <see cref="IPEndPoint"/> of this server's listener socket.</param>
    /// <param name="options">Optional creation options.</param>
    public MessageBrokerServer(IPEndPoint localEndPoint, MessageBrokerServerOptions options = default)
    {
        _listener = new TcpListener( localEndPoint );
        LocalEndPoint = localEndPoint;

        HandshakeTimeout = Defaults.Temporal.GetActualTimeout( options.HandshakeTimeout );
        AcceptableMessageTimeout = Defaults.Temporal.GetActualTimeoutBounds( options.AcceptableMessageTimeout );
        AcceptablePingInterval = Defaults.Temporal.GetActualPingIntervalBounds( options.AcceptablePingInterval );
        _eventHandler = options.EventHandler;
        RemoteClientEventHandlerFactory = options.ClientEventHandlerFactory;
        ChannelEventHandlerFactory = options.ChannelEventHandlerFactory;
        QueueEventHandlerFactory = options.QueueEventHandlerFactory;
        ChannelBindingEventHandlerFactory = options.ChannelBindingEventHandlerFactory;
        SubscriptionEventHandlerFactory = options.SubscriptionEventHandlerFactory;
        TimestampsFactory = options.TimestampsFactory ?? (static _ => new TimestampProvider());
        DelaySourceFactory = options.DelaySourceFactory;

        StreamDecorator = options.StreamDecorator;
        _state = MessageBrokerServerState.Created;

        ClientListener = ClientListener.Create();
        RemoteClientCollection = RemoteClientCollection.Create( options.Tcp, options.MinMemoryPoolSegmentLength );
        ChannelCollection = ChannelCollection.Create();
        QueueCollection = QueueCollection.Create();
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
    /// The local <see cref="IPEndPoint"/> of this server's listener socket.
    /// </summary>
    public IPEndPoint LocalEndPoint { get; private set; }

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
    /// Collection of attached <see cref="MessageBrokerRemoteClient"/> instances.
    /// </summary>
    public MessageBrokerRemoteClientCollection Clients => new MessageBrokerRemoteClientCollection( this );

    /// <summary>
    /// Collection of attached <see cref="MessageBrokerChannel"/> instances.
    /// </summary>
    public MessageBrokerChannelCollection Channels => new MessageBrokerChannelCollection( this );

    /// <summary>
    /// Collection of attached <see cref="MessageBrokerQueue"/> instances.
    /// </summary>
    public MessageBrokerQueueCollection Queues => new MessageBrokerQueueCollection( this );

    internal bool ShouldCancel => _state >= MessageBrokerServerState.Disposing;

    /// <inheritdoc />
    public void Dispose()
    {
        DisposeAsync().AsTask().Wait();
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        using ( AcquireLock() )
        {
            if ( ShouldCancel )
                return;

            _state = MessageBrokerServerState.Disposing;
        }

        Emit( MessageBrokerServerEvent.Disposing( this ) );

        Exception? exception = null;
        MessageBrokerRemoteClient[] clients;
        MessageBrokerQueue[] queues;
        MessageBrokerChannel[] channels;
        Task? clientListenerTask;
        using ( AcquireLock() )
        {
            channels = ChannelCollection.DisposeUnsafe();
            queues = QueueCollection.DisposeUnsafe();
            clients = RemoteClientCollection.DisposeUnsafe();
            clientListenerTask = ClientListener.DiscardUnderlyingTask();
            ClientListener.Dispose();

            try
            {
                _listener.Stop();
            }
            catch ( Exception exc )
            {
                exception = exc;
                clientListenerTask = null;
            }
        }

        if ( exception is not null )
            Emit( MessageBrokerServerEvent.Unexpected( this, exception ) );

        await Parallel.ForEachAsync( queues, static (q, _) => q.OnServerDisposedAsync() ).ConfigureAwait( false );
        await Parallel.ForEachAsync( channels, static (c, _) => c.OnServerDisposedAsync() ).ConfigureAwait( false );
        await Parallel.ForEachAsync( clients, static (c, _) => c.OnServerDisposedAsync() ).ConfigureAwait( false );

        if ( clientListenerTask is not null )
            await clientListenerTask.ConfigureAwait( false );

        using ( AcquireLock() )
            _state = MessageBrokerServerState.Disposed;

        Emit( MessageBrokerServerEvent.Disposed( this ) );
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
        await DisposeAndThrowIfCancellationRequested( cancellationToken ).ConfigureAwait( false );

        using ( AcquireLock() )
        {
            if ( ShouldCancel )
                ExceptionThrower.Throw( DisposedException() );

            AssertState( MessageBrokerServerState.Created );
            _state = MessageBrokerServerState.Starting;
        }

        Emit( MessageBrokerServerEvent.Starting( this ) );

        try
        {
            using ( AcquireLock() )
            {
                if ( ShouldCancel )
                    return DisposedException();

                _listener.Start();
            }
        }
        catch ( Exception exc )
        {
            Emit( MessageBrokerServerEvent.Starting( this, exc ) );
            await DisposeAsync().ConfigureAwait( false );
            return exc;
        }

        await DisposeAndThrowIfCancellationRequested( cancellationToken ).ConfigureAwait( false );

        try
        {
            using ( AcquireLock() )
            {
                if ( ShouldCancel )
                    return DisposedException();

                LocalEndPoint = ( IPEndPoint )_listener.LocalEndpoint;
                _state = MessageBrokerServerState.Running;
            }

            Emit( MessageBrokerServerEvent.Started( this ) );

            var clientListenerTask = ClientListener.StartUnderlyingTask( this, _listener );
            using ( AcquireLock() )
            {
                if ( ShouldCancel )
                    return DisposedException();

                ClientListener.SetUnderlyingTask( clientListenerTask );
            }
        }
        catch ( Exception exc )
        {
            Emit( MessageBrokerServerEvent.Unexpected( this, exc ) );
            await DisposeAsync().ConfigureAwait( false );
            return exc;
        }

        return Result.Valid;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal MessageBrokerServerDisposedException DisposedException()
    {
        return new MessageBrokerServerDisposedException( this );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ExclusiveLock AcquireLock()
    {
        return ExclusiveLock.SpinWaitEnter( _listener, spinWaitMultiplier: 4 );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void Emit(MessageBrokerServerEvent e)
    {
        if ( _eventHandler is null )
            return;

        try
        {
            _eventHandler( e );
        }
        catch
        {
            // NOTE: do nothing
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private async ValueTask DisposeAndThrowIfCancellationRequested(CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
        }
        catch
        {
            await DisposeAsync().ConfigureAwait( false );
            throw;
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
