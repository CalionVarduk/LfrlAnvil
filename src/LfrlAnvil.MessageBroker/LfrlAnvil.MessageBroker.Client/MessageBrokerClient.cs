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
using LfrlAnvil.Diagnostics;
using LfrlAnvil.Exceptions;
using LfrlAnvil.Extensions;
using LfrlAnvil.Memory;
using LfrlAnvil.MessageBroker.Client.Events;
using LfrlAnvil.MessageBroker.Client.Exceptions;
using LfrlAnvil.MessageBroker.Client.Internal;

namespace LfrlAnvil.MessageBroker.Client;

/// <summary>
/// Represents a message broker client.
/// </summary>
public sealed partial class MessageBrokerClient : IDisposable, IAsyncDisposable
{
    internal WriterQueue WriterQueue;
    internal ResponseQueue ResponseQueue;
    internal EventScheduler EventScheduler;
    internal PacketListener PacketListener;
    internal PingScheduler PingScheduler;
    internal NotificationHandler NotificationHandler;
    internal PublisherCollection PublisherCollection;
    internal ListenerCollection ListenerCollection;
    internal ExternalNameCache ExternalNameCache;
    internal int MaxNetworkBatchPacketBytes;
    internal int MaxNetworkPacketBytes;
    internal int MaxNetworkMessagePacketBytes;
    internal readonly MemoryPool<byte> MemoryPool;
    internal readonly MessageBrokerClientLogger Logger;

    private readonly ITimestampProvider _timestamps;
    private readonly TcpClient _tcp;
    private StackSlim<MessageBrokerPushContext> _messageContextPool;
    private DelaySource _delaySource;
    private MessageBrokerClientStreamDecorator? _streamDecorator;
    private Stream? _stream;
    private MessageBrokerClientState _state;
    private ulong _nextTraceId;

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
        MemoryPool = Defaults.Memory.CreatePool( options.MinMemoryPoolSegmentLength );
        ConnectionTimeout = Defaults.Temporal.GetActualTimeout( options.ConnectionTimeout );
        MessageTimeout = Defaults.Temporal.GetActualTimeout( options.DesiredMessageTimeout );
        PingInterval = Defaults.Temporal.GetActualPingInterval( options.DesiredPingInterval );
        MaxBatchPacketCount = Defaults.Memory.GetActualMaxBatchPacketCount( options.NetworkPacket.DesiredMaxBatchPacketCount );
        MaxNetworkBatchPacketBytes = MaxBatchPacketCount > 0
            ? Defaults.Memory.GetActualMaxNetworkBatchPacketLength( options.NetworkPacket.DesiredMaxBatchLength )
            : 0;

        ListenerDisposalTimeout = Defaults.Temporal.GetActualTimeout( options.ListenerDisposalTimeout );
        SynchronizeExternalObjectNames = options.SynchronizeExternalObjectNames ?? true;
        _streamDecorator = options.StreamDecorator;
        Logger = options.Logger ?? default;
        _timestamps = options.Timestamps ?? TimestampProvider.Shared;
        _messageContextPool = StackSlim<MessageBrokerPushContext>.Create();

        _stream = null;
        _state = MessageBrokerClientState.Created;
        Id = 0;
        Name = name;
        RemoteEndPoint = remoteEndPoint;
        IsServerLittleEndian = false;
        MaxNetworkPacketBytes = 0;
        MaxNetworkMessagePacketBytes = 0;
        _nextTraceId = 0;

        _delaySource = options.DelaySource is not null ? DelaySource.External( options.DelaySource ) : DelaySource.Owned();
        WriterQueue = WriterQueue.Create();
        ResponseQueue = ResponseQueue.Create();
        EventScheduler = EventScheduler.Create();
        PacketListener = PacketListener.Create();
        PingScheduler = PingScheduler.Create();
        NotificationHandler = NotificationHandler.Create();
        PublisherCollection = PublisherCollection.Create();
        ListenerCollection = ListenerCollection.Create();
        ExternalNameCache = ExternalNameCache.Create();
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
    /// Max acceptable batch packet count.
    /// </summary>
    /// <remarks>Value may change during handshake with the server.</remarks>
    public short MaxBatchPacketCount { get; private set; }

    /// <summary>
    /// Amount of time that <see cref="MessageBrokerListener"/> instances will wait during their disposal
    /// for callbacks to complete before giving up.
    /// </summary>
    public Duration ListenerDisposalTimeout { get; }

    /// <summary>
    /// Specifies whether or not synchronization of external object names is enabled.
    /// </summary>
    public bool SynchronizeExternalObjectNames { get; }

    /// <summary>
    /// Max acceptable network packet length.
    /// </summary>
    /// <remarks>
    /// Represents max possible length for packets not handled
    /// by either <see cref="MaxNetworkMessagePacketLength"/> or <see cref="MaxNetworkBatchPacketLength"/>.
    /// Value will be initialized during handshake with the server.
    /// </remarks>
    public MemorySize MaxNetworkPacketLength => MemorySize.FromBytes( MaxNetworkPacketBytes );

    /// <summary>
    /// Max acceptable network message packet length.
    /// </summary>
    /// <remarks>
    /// Represents max possible length for inbound packets of <see cref="MessageBrokerClientEndpoint.MessageNotification"/> type
    /// or outbound packets of <see cref="MessageBrokerServerEndpoint.PushMessage"/> type.
    /// Value will be initialized during handshake with the server.
    /// </remarks>
    public MemorySize MaxNetworkMessagePacketLength => MemorySize.FromBytes( MaxNetworkMessagePacketBytes );

    /// <summary>
    /// Max acceptable network batch packet length.
    /// </summary>
    /// <remarks>
    /// Represents max possible length for packets of <b>Batch</b> type.
    /// Value may change during handshake with the server.
    /// </remarks>
    public MemorySize MaxNetworkBatchPacketLength => MemorySize.FromBytes( MaxNetworkBatchPacketBytes );

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

    internal int MaxNetworkPushMessagePacketBytes => MaxNetworkMessagePacketBytes
        - Math.Max( Protocol.PushMessageHeader.Length, Protocol.PacketHeader.Length + Protocol.MessageNotificationHeader.Length )
        + Protocol.PushMessageHeader.Length;

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

        using ( MessageBrokerClientTraceEvent.CreateScope( this, traceId, MessageBrokerClientTraceEventType.Dispose ) )
            await DisposeAsyncCore( traceId ).ConfigureAwait( false );
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
        ulong traceId;
        using ( AcquireLock() )
        {
            if ( ShouldCancel )
                ExceptionThrower.Throw( this.DisposedException() );

            traceId = GetTraceId();
        }

        using ( MessageBrokerClientTraceEvent.CreateScope( this, traceId, MessageBrokerClientTraceEventType.Start ) )
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
            }
            catch ( Exception exc )
            {
                if ( Logger.Error is { } error )
                    error.Emit( MessageBrokerClientErrorEvent.Create( this, traceId, exc ) );

                throw;
            }

            try
            {
                using ( AcquireActiveLock( traceId, out var exc ) )
                {
                    if ( exc is not null )
                        return exc;

                    AssertState( MessageBrokerClientState.Created );
                    _state = MessageBrokerClientState.Connecting;
                    EventScheduler.InitializeResetEvent( _delaySource.GetSource() );
                }
            }
            catch ( Exception exc )
            {
                if ( Logger.Error is { } error )
                    error.Emit( MessageBrokerClientErrorEvent.Create( this, traceId, exc ) );

                if ( exc is MessageBrokerClientStateException )
                    throw;

                await DisposeAsync( traceId ).ConfigureAwait( false );
                return exc;
            }

            try
            {
                var eventSchedulerTask = EventScheduler.StartUnderlyingTask( this );
                using ( AcquireActiveLock( traceId, out var exc ) )
                {
                    if ( exc is not null )
                        return exc;

                    EventScheduler.SetUnderlyingTask( eventSchedulerTask );
                }
            }
            catch ( Exception exc )
            {
                if ( Logger.Error is { } error )
                    error.Emit( MessageBrokerClientErrorEvent.Create( this, traceId, exc ) );

                await DisposeAsync( traceId ).ConfigureAwait( false );
                return exc;
            }

            var connectResult = await ConnectToServerAsync( traceId, cancellationToken ).ConfigureAwait( false );
            if ( connectResult.Exception is not null )
            {
                if ( connectResult.Exception is not MessageBrokerClientDisposedException )
                    await DisposeAsync( traceId ).ConfigureAwait( false );

                return connectResult;
            }

            await DisposeAndThrowIfCancellationRequestedAsync( traceId, cancellationToken ).ConfigureAwait( false );

            var handshakeResult = await EstablishHandshakeAsync( traceId, cancellationToken ).ConfigureAwait( false );
            if ( handshakeResult.Exception is not null )
            {
                if ( handshakeResult.Exception is not MessageBrokerClientDisposedException )
                    await DisposeAsync( traceId ).ConfigureAwait( false );

                return handshakeResult;
            }

            await DisposeAndThrowIfCancellationRequestedAsync( traceId, cancellationToken ).ConfigureAwait( false );

            try
            {
                Stream? stream;
                using ( AcquireActiveLock( traceId, out var exc ) )
                {
                    if ( exc is not null )
                        return exc;

                    EventScheduler.ResetWriteTimeout();
                    _state = MessageBrokerClientState.Running;
                    stream = _stream;
                }

                Assume.IsNotNull( stream );
                var packetListenerTask = PacketListener.StartUnderlyingTask( this, stream );
                var pingSchedulerTask = PingScheduler.StartUnderlyingTask( this );
                var messageNotificationsTask = NotificationHandler.StartUnderlyingTask( this );
                using ( AcquireActiveLock( traceId, out var exc ) )
                {
                    if ( exc is not null )
                        return exc;

                    PacketListener.SetUnderlyingTask( packetListenerTask );
                    PingScheduler.SetUnderlyingTask( pingSchedulerTask );
                    NotificationHandler.SetUnderlyingTask( messageNotificationsTask );
                }
            }
            catch ( Exception exc )
            {
                if ( Logger.Error is { } error )
                    error.Emit( MessageBrokerClientErrorEvent.Create( this, traceId, exc ) );

                await DisposeAsync( traceId ).ConfigureAwait( false );
                return exc;
            }

            return Result.Valid;
        }
    }

    /// <summary>
    /// Attempts to asynchronously consume chosen queue's dead letter messages.
    /// </summary>
    /// <param name="queueId">ID of the queue whose dead letter is to be queried.</param>
    /// <param name="readCount">Number of dead letter messages to be asynchronously consumed.</param>
    /// <returns>
    /// A task that represents the operation, which returns a <see cref="Result{T}"/> instance,
    /// with underlying <see cref="MessageBrokerDeadLetterQueryResult"/> instance.
    /// </returns>
    /// <exception cref="MessageBrokerClientDisposedException">When client has already been disposed.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="queueId"/> is less than or equal to <b>0</b> or when <paramref name="readCount"/> is less than <b>0</b>.
    /// </exception>
    /// <remarks>
    /// This operation will consume dead letter messages by sending them to correct listeners,
    /// which will also cause them to be completely removed from the server.
    /// Unexpected errors encountered during dead letter querying will cause the client to be automatically disposed.
    /// </remarks>
    public async ValueTask<Result<MessageBrokerDeadLetterQueryResult>> QueryDeadLetterAsync(int queueId, int readCount)
    {
        Ensure.IsGreaterThan( queueId, 0 );
        Ensure.IsGreaterThanOrEqualTo( readCount, 0 );

        ulong traceId;
        bool reverseEndianness;
        using ( AcquireLock() )
        {
            if ( ShouldCancel )
                ExceptionThrower.Throw( this.DisposedException() );

            AssertState( MessageBrokerClientState.Running );
            reverseEndianness = BitConverter.IsLittleEndian != IsServerLittleEndian;
            traceId = GetTraceId();
        }

        using ( MessageBrokerClientTraceEvent.CreateScope( this, traceId, MessageBrokerClientTraceEventType.DeadLetterQuery ) )
        {
            if ( Logger.QueryingDeadLetter is { } queryingDeadLetter )
                queryingDeadLetter.Emit( MessageBrokerClientQueryingDeadLetterEvent.Create( this, traceId, queueId, readCount ) );

            ManualResetValueTaskSource<IncomingPacketToken> responseSource;
            Protocol.DeadLetterQuery request;

            var poolToken = MemoryPoolToken<byte>.Empty;
            try
            {
                request = new Protocol.DeadLetterQuery( queueId, readCount );
                poolToken = MemoryPool.Rent( Protocol.DeadLetterQuery.Length, out var buffer ).EnableClearing();
                request.Serialize( buffer, reverseEndianness );

                ManualResetValueTaskSource<bool> writerSource;
                using ( AcquireActiveLock( traceId, out var exc ) )
                {
                    if ( exc is not null )
                        return exc;

                    writerSource = WriterQueue.AcquireSource();
                }

                if ( ! await writerSource.GetTask().ConfigureAwait( false ) )
                    return EmitError( this.DisposedException(), traceId );

                using ( AcquireActiveLock( traceId, out var exc ) )
                {
                    if ( exc is not null )
                        return exc;

                    EventScheduler.PausePing();
                    responseSource = ResponseQueue.EnqueueSource();
                }

                var result = await WriteAsync( request.Header, buffer, traceId ).ConfigureAwait( false );
                if ( result.Exception is not null )
                {
                    await DisposeAsync( traceId ).ConfigureAwait( false );
                    return result.Exception;
                }

                using ( AcquireActiveLock( traceId, out var exc ) )
                {
                    if ( exc is not null )
                        return exc;

                    WriterQueue.Release( this, writerSource );
                    ResponseQueue.ActivateTimeout( this, responseSource );
                    EventScheduler.SchedulePing( this );
                }
            }
            catch ( Exception exc )
            {
                if ( Logger.Error is { } error )
                    error.Emit( MessageBrokerClientErrorEvent.Create( this, traceId, exc ) );

                await DisposeAsync( traceId ).ConfigureAwait( false );
                return exc;
            }
            finally
            {
                poolToken.Return( this, traceId );
            }

            var response = await responseSource.GetTask().ConfigureAwait( false );
            try
            {
                if ( response.Type != IncomingPacketToken.Result.Ok )
                {
                    if ( response.Type == IncomingPacketToken.Result.Disposed )
                        return EmitError( this.DisposedException(), traceId );

                    var exception = this.ResponseTimeoutException( request.Header );
                    if ( Logger.Error is { } error )
                        error.Emit( MessageBrokerClientErrorEvent.Create( this, traceId, exception ) );

                    await DisposeAsync( traceId ).ConfigureAwait( false );
                    return exception;
                }

                using ( AcquireActiveLock( traceId, out var exc ) )
                {
                    if ( exc is not null )
                        return exc;

                    ResponseQueue.Release( responseSource );
                }

                switch ( response.Header.GetClientEndpoint() )
                {
                    case MessageBrokerClientEndpoint.DeadLetterQueryResponse:
                    {
                        var readPacket = Logger.ReadPacket;
                        readPacket?.Emit( MessageBrokerClientReadPacketEvent.CreateReceived( this, traceId, response.Header ) );

                        var exception = response.Header.AssertExactPayload( this, Protocol.DeadLetterQueryResponse.Length );
                        if ( exception is not null )
                        {
                            if ( Logger.Error is { } error )
                                error.Emit( MessageBrokerClientErrorEvent.Create( this, traceId, exception ) );

                            await DisposeAsync( traceId ).ConfigureAwait( false );
                            return exception;
                        }

                        var parsedResponse = Protocol.DeadLetterQueryResponse.Parse( response.Data, reverseEndianness );

                        var errors = parsedResponse.StringifyErrors();
                        if ( errors.Count > 0 )
                        {
                            var error = EmitError( this.ProtocolException( response.Header, errors ), traceId );
                            await DisposeAsync( traceId ).ConfigureAwait( false );
                            return error;
                        }

                        readPacket?.Emit( MessageBrokerClientReadPacketEvent.CreateAccepted( this, traceId, response.Header ) );
                        if ( ! parsedResponse.QueueExists )
                            return EmitError( this.RequestException( request.Header, Resources.QueueDoesNotExist( queueId ) ), traceId );

                        if ( Logger.DeadLetterQueried is { } deadLetterQueried )
                            deadLetterQueried.Emit(
                                MessageBrokerClientDeadLetterQueriedEvent.Create(
                                    this,
                                    traceId,
                                    parsedResponse.TotalCount,
                                    parsedResponse.MaxReadCount,
                                    parsedResponse.NextExpirationAt ) );

                        return MessageBrokerDeadLetterQueryResult.Create(
                            parsedResponse.TotalCount,
                            parsedResponse.MaxReadCount,
                            parsedResponse.NextExpirationAt );
                    }
                    default:
                    {
                        var error = HandleUnexpectedEndpoint( response.Header, traceId );
                        await DisposeAsync( traceId ).ConfigureAwait( false );
                        return error;
                    }
                }
            }
            catch ( Exception exc )
            {
                if ( Logger.Error is { } error )
                    error.Emit( MessageBrokerClientErrorEvent.Create( this, traceId, exc ) );

                await DisposeAsync( traceId ).ConfigureAwait( false );
                return exc;
            }
            finally
            {
                response.PoolToken.Return( this, traceId );
            }
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ExclusiveLock AcquireLock()
    {
        return ExclusiveLock.SpinWaitEnter( _tcp, spinWaitMultiplier: 4 );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ExclusiveLock AcquireActiveLock(ulong traceId, out MessageBrokerClientDisposedException? exception)
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
            error.Emit( MessageBrokerClientErrorEvent.Create( this, traceId, exception ) );

        return default;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal MessageBrokerPushContext RentMessageContext(
        MessageBrokerPublisher publisher,
        MemorySize minCapacity,
        bool clearBufferOnDispose)
    {
        MessageBrokerPushContext? result;
        using ( AcquireLock() )
        {
            if ( ShouldCancel )
                ExceptionThrower.Throw( this.DisposedException() );

            if ( ! _messageContextPool.TryPop( out result ) )
                result = new MessageBrokerPushContext( MemoryPool );
        }

        result.Initialize( publisher, minCapacity, clearBufferOnDispose );
        return result;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void ReturnMessageContext(MessageBrokerPushContext context, MemoryPoolToken<byte> token, MemoryPoolToken<byte> routingToken)
    {
        var exception = token.Return();
        var routingException = routingToken.Return();
        if ( routingException is not null )
            exception = exception is not null ? new AggregateException( [ exception, routingException ] ) : routingException;

        ulong traceId;
        using ( AcquireLock() )
        {
            if ( ! ShouldCancel )
                _messageContextPool.Push( context );

            if ( exception is null )
                return;

            traceId = GetTraceId();
        }

        using ( MessageBrokerClientTraceEvent.CreateScope( this, traceId, MessageBrokerClientTraceEventType.Unexpected ) )
        {
            if ( Logger.Error is { } error )
                error.Emit( MessageBrokerClientErrorEvent.Create( this, traceId, exception ) );
        }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal Timestamp GetTimestamp()
    {
        return _timestamps.GetNow();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal Exception HandleUnexpectedEndpoint(Protocol.PacketHeader header, ulong traceId)
    {
        return EmitError( this.ProtocolException( header, Resources.UnexpectedClientEndpoint ), traceId );
    }

    internal async ValueTask<Result> WriteAsync(Protocol.PacketHeader header, ReadOnlyMemory<byte> data, ulong traceId)
    {
        var sendPacket = Logger.SendPacket;
        sendPacket?.Emit( MessageBrokerClientSendPacketEvent.CreateSending( this, traceId, header ) );

        Stream? stream;
        CancellationToken timeoutToken;
        try
        {
            using ( AcquireActiveLock( traceId, out var exc ) )
            {
                if ( exc is not null )
                    return exc;

                stream = _stream;
                timeoutToken = EventScheduler.ScheduleWriteTimeout( this );
            }
        }
        catch ( Exception exc )
        {
            return EmitError( exc, traceId );
        }

        Assume.IsNotNull( stream );
        try
        {
            await stream.WriteAsync( data, timeoutToken ).ConfigureAwait( false );
        }
        catch ( Exception exc )
        {
            return EmitError( exc, traceId );
        }

        sendPacket?.Emit( MessageBrokerClientSendPacketEvent.CreateSent( this, traceId, header ) );
        return Result.Valid;
    }

    private async ValueTask<Result> ConnectToServerAsync(ulong traceId, CancellationToken cancellationToken)
    {
        if ( Logger.Connecting is { } connecting )
            connecting.Emit( MessageBrokerClientConnectingEvent.Create( this, traceId ) );

        try
        {
            CancellationToken timeoutToken;
            using ( AcquireActiveLock( traceId, out var exc ) )
            {
                if ( exc is not null )
                    return exc;

                timeoutToken = EventScheduler.ScheduleConnectTimeout( this );
            }

            await _tcp.ConnectAsync( RemoteEndPoint, timeoutToken ).ConfigureAwait( false );

            Stream stream;
            MessageBrokerClientStreamDecorator? decorator;
            using ( AcquireActiveLock( traceId, out var exc ) )
            {
                if ( exc is not null )
                    return exc;

                decorator = _streamDecorator;
                _streamDecorator = null;
                _stream = _tcp.GetStream();
                stream = _stream;
            }

            if ( decorator is not null )
            {
                stream = await decorator( this, ReinterpretCast.To<NetworkStream>( stream ), cancellationToken ).ConfigureAwait( false );
                using ( AcquireActiveLock( traceId, out var exc ) )
                {
                    if ( exc is not null )
                        return exc;

                    _stream = stream;
                }
            }
        }
        catch ( Exception exc )
        {
            return EmitError( exc, traceId );
        }

        if ( Logger.Connected is { } connected )
            connected.Emit( MessageBrokerClientConnectedEvent.Create( this, traceId ) );

        return Result.Valid;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private async ValueTask DisposeAndThrowIfCancellationRequestedAsync(ulong traceId, CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
        }
        catch ( Exception exc )
        {
            if ( Logger.Error is { } error )
                error.Emit( MessageBrokerClientErrorEvent.Create( this, traceId, exc ) );

            await DisposeAsync( traceId ).ConfigureAwait( false );
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

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal Exception EmitError(Exception exception, ulong traceId)
    {
        if ( Logger.Error is { } error )
            error.Emit( MessageBrokerClientErrorEvent.Create( this, traceId, exception ) );

        return exception;
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

        _state = MessageBrokerClientState.Disposing;
        return true;
    }

    internal ValueTask DisposeAsync(ulong traceId)
    {
        using ( AcquireLock() )
        {
            if ( ShouldCancel )
                return ValueTask.CompletedTask;

            _state = MessageBrokerClientState.Disposing;
        }

        return DisposeAsyncCore( traceId );
    }

    internal async ValueTask DisposeAsyncCore(ulong traceId)
    {
        if ( Logger.Disposing is { } disposing )
            disposing.Emit( MessageBrokerClientDisposingEvent.Create( this, traceId ) );

        Task? eventSchedulerTask;
        Task? pingSchedulerTask;
        Task? packetListenerTask;
        Task? messageNotificationsTask;
        ValueTaskDelaySource? ownedDelaySource;

        using ( AcquireLock() )
        {
            _messageContextPool.Clear();
            ownedDelaySource = _delaySource.DiscardOwnedSource();
            eventSchedulerTask = EventScheduler.DiscardUnderlyingTask();
            pingSchedulerTask = PingScheduler.DiscardUnderlyingTask();
            packetListenerTask = PacketListener.DiscardUnderlyingTask();
            messageNotificationsTask = NotificationHandler.DiscardUnderlyingTask();
            PingScheduler.Dispose();
            EventScheduler.Dispose();
            NotificationHandler.BeginDispose();
            WriterQueue.Dispose();
            ResponseQueue.Dispose();
        }

        if ( eventSchedulerTask is not null )
            await eventSchedulerTask.ConfigureAwait( false );

        if ( pingSchedulerTask is not null )
            await pingSchedulerTask.ConfigureAwait( false );

        if ( packetListenerTask is not null )
            await packetListenerTask.ConfigureAwait( false );

        if ( messageNotificationsTask is not null )
            await messageNotificationsTask.ConfigureAwait( false );

        var error = Logger.Error;
        MessageBrokerListener[] listeners;
        int discardedMessageCount;
        Chain<Exception> exceptions;

        using ( AcquireLock() )
        {
            PublisherCollection.Clear();
            listeners = ListenerCollection.Clear();
            (discardedMessageCount, exceptions) = NotificationHandler.EndDispose( error is not null );
            var exception = _tcp.TryDispose().Exception;
            if ( exception is not null && error is not null )
                exceptions = exceptions.Extend( exception );
        }

        foreach ( var exc in exceptions )
        {
            Assume.IsNotNull( error );
            error.Emit( MessageBrokerClientErrorEvent.Create( this, traceId, exc ) );
        }

        if ( discardedMessageCount > 0 && error is not null )
        {
            var exc = this.MessageException( null, Resources.MessagesDiscarded( discardedMessageCount ) );
            error.Emit( MessageBrokerClientErrorEvent.Create( this, traceId, exc ) );
        }

        await Parallel.ForEachAsync( listeners, (l, _) => l.OnClientDisposedAsync( traceId ) ).ConfigureAwait( false );

        using ( AcquireLock() )
        {
            _stream = null;
            ExternalNameCache.Clear();
            _state = MessageBrokerClientState.Disposed;
        }

        if ( ownedDelaySource is not null )
            await ownedDelaySource.TryDisposeAsync().ConfigureAwait( false );

        if ( Logger.Disposed is { } disposed )
            disposed.Emit( MessageBrokerClientDisposedEvent.Create( this, traceId ) );
    }
}
