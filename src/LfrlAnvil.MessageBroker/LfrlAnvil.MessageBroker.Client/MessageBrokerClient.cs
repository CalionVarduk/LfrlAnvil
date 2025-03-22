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
using LfrlAnvil.Exceptions;
using LfrlAnvil.Extensions;
using LfrlAnvil.Memory;
using LfrlAnvil.MessageBroker.Client.Buffering;
using LfrlAnvil.MessageBroker.Client.Events;
using LfrlAnvil.MessageBroker.Client.Exceptions;
using LfrlAnvil.MessageBroker.Client.Internal;

namespace LfrlAnvil.MessageBroker.Client;

/// <summary>
/// Represents a message broker client.
/// </summary>
public sealed partial class MessageBrokerClient : IDisposable, IAsyncDisposable
{
    internal EventScheduler EventScheduler;
    internal MessageListener MessageListener;
    internal MessageContextQueue MessageContextQueue;
    internal PingScheduler PingScheduler;
    internal PublisherCollection PublisherCollection;
    internal ListenerCollection ListenerCollection;

    private readonly MessageBrokerClientEventHandler? _eventHandler;
    private readonly ITimestampProvider _timestamps;
    private readonly MemoryPool<byte> _memoryPool;
    private readonly TcpClient _tcp;
    private DelaySource _delaySource;
    private MessageBrokerClientStreamDecorator? _streamDecorator;
    private Stream? _stream;
    private MessageBrokerClientState _state;

    /// <summary>
    /// Creates a new <see cref="MessageBrokerClient"/> instance.
    /// </summary>
    /// <param name="remoteEndPoint">The <see cref="IPEndPoint"/> of the server to which this client will connect to.</param>
    /// <param name="name">Client's unique name.</param>
    /// <param name="options">Optional creation options.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="name"/>'s length is less than <b>1</b> or greater than <b>512</b>.
    /// </exception>
    public MessageBrokerClient(IPEndPoint remoteEndPoint, string name, MessageBrokerClientOptions options = default)
    {
        Ensure.IsInRange( name.Length, Defaults.NameLengthBounds.Min, Defaults.NameLengthBounds.Max );

        _tcp = Defaults.Tcp.CreateClient( options.Tcp );
        _memoryPool = Defaults.Memory.CreatePool( options.MinMemoryPoolSegmentLength );
        ConnectionTimeout = Defaults.Temporal.GetActualTimeout( options.ConnectionTimeout );
        MessageTimeout = Defaults.Temporal.GetActualTimeout( options.DesiredMessageTimeout );
        PingInterval = Defaults.Temporal.GetActualPingInterval( options.DesiredPingInterval );
        _streamDecorator = options.StreamDecorator;
        _eventHandler = options.EventHandler;
        _timestamps = options.Timestamps ?? new TimestampProvider();

        _stream = null;
        _state = MessageBrokerClientState.Created;
        Id = 0;
        Name = name;
        RemoteEndPoint = remoteEndPoint;
        IsServerLittleEndian = false;

        _delaySource = options.DelaySource is not null ? DelaySource.External( options.DelaySource ) : DelaySource.Owned();
        EventScheduler = EventScheduler.Create();
        MessageListener = MessageListener.Create();
        MessageContextQueue = MessageContextQueue.Create();
        PingScheduler = PingScheduler.Create();
        PublisherCollection = PublisherCollection.Create();
        ListenerCollection = ListenerCollection.Create();
    }

    /// <summary>
    /// Client's unique identifier assigned by the server.
    /// </summary>
    /// <remarks>Value will be initialized during handshake with the server.</remarks>
    public int Id { get; private set; }

    /// <summary>
    /// Client's unique name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The remote <see cref="IPEndPoint"/> of the server to which this client connects to.
    /// </summary>
    public IPEndPoint RemoteEndPoint { get; }

    /// <summary>
    /// Indicates server's endianness.
    /// </summary>
    /// <remarks>Value will be initialized during handshake with the server.</remarks>
    public bool IsServerLittleEndian { get; private set; }

    /// <summary>
    /// Connect to server timeout.
    /// </summary>
    public Duration ConnectionTimeout { get; }

    /// <summary>
    /// Send or receive message timeout.
    /// </summary>
    /// <remarks>Value may change during handshake with the server.</remarks>
    public Duration MessageTimeout { get; private set; }

    /// <summary>
    /// Send ping interval.
    /// </summary>
    /// <remarks>Value may change during handshake with the server.</remarks>
    public Duration PingInterval { get; private set; }

    /// <summary>
    /// The local <see cref="EndPoint"/> that this client is using for communications with the server.
    /// </summary>
    public EndPoint? LocalEndPoint
    {
        get
        {
            using ( AcquireLock() )
            {
                try
                {
                    return _tcp.Client.LocalEndPoint;
                }
                catch
                {
                    return null;
                }
            }
        }
    }

    /// <summary>
    /// Current client's state.
    /// </summary>
    /// <remarks>See <see cref="MessageBrokerClientState"/> for more information.</remarks>
    public MessageBrokerClientState State
    {
        get
        {
            using ( AcquireLock() )
                return _state;
        }
    }

    /// <summary>
    /// Collection of <see cref="MessageBrokerPublisher"/> instances.
    /// </summary>
    public MessageBrokerPublisherCollection Publishers => new MessageBrokerPublisherCollection( this );

    /// <summary>
    /// Collection of <see cref="MessageBrokerListener"/> instances.
    /// </summary>
    public MessageBrokerListenerCollection Listeners => new MessageBrokerListenerCollection( this );

    internal bool ShouldCancel => _state >= MessageBrokerClientState.Disposing;

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

            _state = MessageBrokerClientState.Disposing;
        }

        Emit( MessageBrokerClientEvent.Disposing( this ) );

        Task? eventSchedulerTask;
        Task? pingSchedulerTask;
        Task? messageListenerTask;
        ValueTaskDelaySource? ownedDelaySource;

        using ( AcquireLock() )
        {
            ownedDelaySource = _delaySource.DiscardOwnedSource();
            PublisherCollection.Clear();
            ListenerCollection.Clear();
            eventSchedulerTask = EventScheduler.DiscardUnderlyingTask();
            pingSchedulerTask = PingScheduler.DiscardUnderlyingTask();
            messageListenerTask = MessageListener.DiscardUnderlyingTask();
            MessageContextQueue.Dispose();
            PingScheduler.Dispose();
            EventScheduler.Dispose();
        }

        if ( eventSchedulerTask is not null )
            await eventSchedulerTask.ConfigureAwait( false );

        if ( pingSchedulerTask is not null )
            await pingSchedulerTask.ConfigureAwait( false );

        if ( messageListenerTask is not null )
            await messageListenerTask.ConfigureAwait( false );

        Exception? exception;
        using ( AcquireLock() )
            exception = _tcp.TryDispose().Exception;

        if ( exception is not null )
            Emit( MessageBrokerClientEvent.Unexpected( this, exception ) );

        using ( AcquireLock() )
        {
            _stream = null;
            _state = MessageBrokerClientState.Disposed;
        }

        if ( ownedDelaySource is not null )
            await ownedDelaySource.TryDisposeAsync();

        Emit( MessageBrokerClientEvent.Disposed( this ) );
    }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerClient"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[{Id}] '{Name}' client ({State})";
    }

    /// <summary>
    /// Attempts to initialize the client, connect to the server and establish a handshake.
    /// </summary>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/>.</param>
    /// <returns>A task that represents the operation, which returns a <see cref="Result"/> instance.</returns>
    /// <exception cref="OperationCanceledException">When <paramref name="cancellationToken"/> has been cancelled.</exception>
    /// <exception cref="MessageBrokerClientDisposedException">When this client has already been disposed.</exception>
    /// <exception cref="MessageBrokerClientStateException">
    /// When this client is not disposed and not in <see cref="MessageBrokerClientState.Created"/> state.
    /// </exception>
    /// <remarks>
    /// Errors encountered during client initialization will cause it to be automatically disposed.
    /// Returned <see cref="Result"/> will only be valid when the client will successfully connect to the server, establish a handshake
    /// and proceed to the <see cref="MessageBrokerClientState.Running"/> state.
    /// </remarks>
    public async ValueTask<Result> StartAsync(CancellationToken cancellationToken = default)
    {
        await DisposeAndThrowIfCancellationRequestedAsync( cancellationToken ).ConfigureAwait( false );

        using ( AcquireLock() )
        {
            if ( ShouldCancel )
                ExceptionThrower.Throw( DisposedException() );

            AssertState( MessageBrokerClientState.Created );
            _state = MessageBrokerClientState.Connecting;
            EventScheduler.InitializeResetEvent( _delaySource.GetSource() );
        }

        try
        {
            var eventSchedulerTask = EventScheduler.StartUnderlyingTask( this );
            using ( AcquireLock() )
            {
                if ( ShouldCancel )
                    return DisposedException();

                EventScheduler.SetUnderlyingTask( eventSchedulerTask );
            }
        }
        catch ( Exception exc )
        {
            Emit( MessageBrokerClientEvent.Unexpected( this, exc ) );
            await DisposeAsync().ConfigureAwait( false );
            return exc;
        }

        var connectResult = await ConnectToServerAsync( cancellationToken ).ConfigureAwait( false );
        if ( connectResult.Exception is not null )
        {
            if ( connectResult.Exception is not MessageBrokerClientDisposedException )
                await DisposeAsync().ConfigureAwait( false );

            return connectResult;
        }

        await DisposeAndThrowIfCancellationRequestedAsync( cancellationToken ).ConfigureAwait( false );

        var handshakeResult = await EstablishHandshakeAsync( cancellationToken ).ConfigureAwait( false );
        if ( handshakeResult.Exception is not null )
        {
            if ( handshakeResult.Exception is not MessageBrokerClientDisposedException )
                await DisposeAsync().ConfigureAwait( false );

            return handshakeResult;
        }

        await DisposeAndThrowIfCancellationRequestedAsync( cancellationToken ).ConfigureAwait( false );

        try
        {
            Stream? stream;
            using ( AcquireLock() )
            {
                if ( ShouldCancel )
                    return DisposedException();

                EventScheduler.ResetWriteTimeout();
                _state = MessageBrokerClientState.Running;
                stream = _stream;
            }

            Assume.IsNotNull( stream );
            var messageListenerTask = MessageListener.StartUnderlyingTask( this, stream );
            var pingSchedulerTask = PingScheduler.StartUnderlyingTask( this );
            //var responderTask = RunResponderAsync();
            using ( AcquireLock() )
            {
                if ( ShouldCancel )
                    return DisposedException();

                MessageListener.SetUnderlyingTask( messageListenerTask );
                PingScheduler.SetUnderlyingTask( pingSchedulerTask );
                //_responderTask = responderTask;
            }
        }
        catch ( Exception exc )
        {
            Emit( MessageBrokerClientEvent.Unexpected( this, exc ) );
            await DisposeAsync().ConfigureAwait( false );
            return exc;
        }

        return Result.Valid;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ExclusiveLock AcquireLock()
    {
        return ExclusiveLock.SpinWaitEnter( _tcp, spinWaitMultiplier: 4 );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal BinaryBufferToken RentBuffer(int length, out Memory<byte> memory)
    {
        using ( ExclusiveLock.Enter( _memoryPool ) )
        {
            var token = _memoryPool.Rent( length );
            memory = token.AsMemory();
            return new BinaryBufferToken( token );
        }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal Timestamp GetTimestamp()
    {
        return _timestamps.GetNow();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal Timestamp GetFutureTimestamp(Duration delay)
    {
        return GetTimestamp() + delay;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void Emit(MessageBrokerClientEvent e)
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
    internal Exception HandleUnexpectedEndpoint(Protocol.PacketHeader header, ulong contextId = 0)
    {
        var exc = Protocol.UnexpectedClientEndpointException( this, header );
        return EmitError( MessageBrokerClientEvent.MessageRejected( this, header, exc, contextId ) );
    }

    internal async ValueTask<Result> WriteAsync(
        Protocol.PacketHeader header,
        ReadOnlyMemory<byte> data,
        ulong contextId = MessageBrokerClientEvent.RootContextId,
        object? eventData = null)
    {
        Emit( MessageBrokerClientEvent.SendingMessage( this, header, contextId, eventData ) );

        Stream? stream = null;
        CancellationToken timeoutToken = default;
        try
        {
            bool cancel;
            using ( AcquireLock() )
            {
                cancel = ShouldCancel;
                if ( ! cancel )
                {
                    stream = _stream;
                    timeoutToken = EventScheduler.ScheduleWriteTimeout( this );
                }
            }

            if ( cancel )
                return EmitError( MessageBrokerClientEvent.SendingMessage( this, header, contextId, DisposedException() ) );
        }
        catch ( Exception exc )
        {
            return EmitError( MessageBrokerClientEvent.SendingMessage( this, header, contextId, exc ) );
        }

        Assume.IsNotNull( stream );
        try
        {
            await stream.WriteAsync( data, timeoutToken ).ConfigureAwait( false );
        }
        catch ( Exception exc )
        {
            return EmitError( MessageBrokerClientEvent.SendingMessage( this, header, contextId, exc ) );
        }

        Emit( MessageBrokerClientEvent.MessageSent( this, header, contextId ) );
        return Result.Valid;
    }

    private async ValueTask<Result> ConnectToServerAsync(CancellationToken cancellationToken)
    {
        Emit( MessageBrokerClientEvent.Connecting( this ) );

        try
        {
            bool cancel;
            CancellationToken timeoutToken = default;
            using ( AcquireLock() )
            {
                cancel = ShouldCancel;
                if ( ! cancel )
                    timeoutToken = EventScheduler.ScheduleConnectTimeout( this );
            }

            if ( cancel )
                return EmitError( MessageBrokerClientEvent.Connecting( this, DisposedException() ) );

            await _tcp.ConnectAsync( RemoteEndPoint, timeoutToken ).ConfigureAwait( false );

            Stream stream;
            MessageBrokerClientStreamDecorator? decorator;
            using ( AcquireLock() )
            {
                if ( ShouldCancel )
                    return EmitError( MessageBrokerClientEvent.Connecting( this, DisposedException() ) );

                decorator = _streamDecorator;
                _streamDecorator = null;
                _stream = _tcp.GetStream();
                stream = _stream;
            }

            if ( decorator is not null )
            {
                stream = await decorator( this, ReinterpretCast.To<NetworkStream>( stream ), cancellationToken ).ConfigureAwait( false );
                using ( AcquireLock() )
                {
                    if ( ShouldCancel )
                        return EmitError( MessageBrokerClientEvent.Connecting( this, DisposedException() ) );

                    _stream = stream;
                }
            }
        }
        catch ( Exception exc )
        {
            return EmitError( MessageBrokerClientEvent.Connecting( this, exc ) );
        }

        Emit( MessageBrokerClientEvent.Connected( this ) );
        return Result.Valid;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private async ValueTask DisposeAndThrowIfCancellationRequestedAsync(CancellationToken cancellationToken)
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
    internal void AssertState(MessageBrokerClientState expected)
    {
        if ( _state != expected )
            ExceptionThrower.Throw( new MessageBrokerClientStateException( this, _state, expected ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal MessageBrokerClientDisposedException DisposedException()
    {
        return new MessageBrokerClientDisposedException( this );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal Exception EmitError(MessageBrokerClientEvent e)
    {
        Assume.IsNotNull( e.Exception );
        Emit( e );
        return e.Exception;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void DisposeBufferToken(BinaryBufferToken token)
    {
        var exc = token.TryDispose().Exception;
        if ( exc is not null )
            Emit( MessageBrokerClientEvent.Unexpected( this, exc ) );
    }
}
