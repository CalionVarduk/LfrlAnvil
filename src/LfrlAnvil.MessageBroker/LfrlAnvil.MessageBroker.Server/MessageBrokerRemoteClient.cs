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
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Async;
using LfrlAnvil.Chrono;
using LfrlAnvil.Chrono.Async;
using LfrlAnvil.Computable.Expressions;
using LfrlAnvil.Diagnostics;
using LfrlAnvil.Extensions;
using LfrlAnvil.Memory;
using LfrlAnvil.MessageBroker.Server.Events;
using LfrlAnvil.MessageBroker.Server.Exceptions;
using LfrlAnvil.MessageBroker.Server.Internal;

namespace LfrlAnvil.MessageBroker.Server;

/// <summary>
/// Represents a remote message broker client.
/// </summary>
public sealed partial class MessageBrokerRemoteClient
{
    internal readonly MemoryPool<byte> MemoryPool;
    internal ReferenceStore<int, MessageBrokerChannelPublisherBinding> PublishersByChannelId;
    internal ReferenceStore<int, MessageBrokerChannelListenerBinding> ListenersByChannelId;
    internal ObjectStore<MessageBrokerQueue> QueueStore;
    internal WriterQueue WriterQueue;
    internal RequestQueue RequestQueue;
    internal EventScheduler EventScheduler;
    internal PacketListener PacketListener;
    internal NotificationSender NotificationSender;
    internal RequestHandler RequestHandler;
    internal ResponseSender ResponseSender;
    internal ExternalNameCache ExternalNameCache;
    internal MessageRouting MessageRouting;
    internal int MaxNetworkBatchPacketBytes;
    internal readonly int MaxNetworkPacketBytes;
    internal readonly int MaxNetworkMessagePacketBytes;
    internal readonly MessageBrokerRemoteClientLogger Logger;

    private readonly ITimestampProvider _timestamps;
    private readonly TcpClient _tcp;
    private readonly TaskCompletionSource _disconnected;
    private DelaySource _delaySource;
    private Stream _stream;
    private MessageBrokerRemoteClientState _state;
    private ulong _nextTraceId;

    internal MessageBrokerRemoteClient(int id, MessageBrokerServer server, TcpClient tcp)
    {
        _tcp = tcp;
        _stream = _tcp.GetStream();
        MemoryPool = new MemoryPool<byte>( unchecked( ( int )server.MaxNetworkPacketLength.Bytes ) );
        Server = server;
        Id = id;
        Name = string.Empty;
        IsLittleEndian = false;
        MessageTimeout = server.HandshakeTimeout;
        MaxReadTimeout = MessageTimeout;
        PingInterval = Duration.Zero;
        MaxNetworkPacketBytes = unchecked( ( int )server.MaxNetworkPacketLength.Bytes );
        MaxNetworkMessagePacketBytes = unchecked( ( int )server.MaxNetworkMessagePacketLength.Bytes
            - Math.Max( Protocol.PushMessageHeader.Length, Protocol.MessageNotificationHeader.Payload )
            + Protocol.PushMessageHeader.Length );

        MaxNetworkBatchPacketBytes = 0;
        MaxBatchPacketCount = 0;
        SynchronizeExternalObjectNames = false;
        ClearBuffers = true;
        _disconnected = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
        _state = MessageBrokerRemoteClientState.Created;
        _nextTraceId = 0;

        PublishersByChannelId = ReferenceStore<int, MessageBrokerChannelPublisherBinding>.Create();
        ListenersByChannelId = ReferenceStore<int, MessageBrokerChannelListenerBinding>.Create();
        QueueStore = ObjectStore<MessageBrokerQueue>.Create( StringComparer.OrdinalIgnoreCase );
        WriterQueue = WriterQueue.Create();
        RequestQueue = RequestQueue.Create();
        EventScheduler = EventScheduler.Create();
        PacketListener = PacketListener.Create();
        NotificationSender = NotificationSender.Create();
        RequestHandler = RequestHandler.Create();
        ResponseSender = ResponseSender.Create();
        ExternalNameCache = ExternalNameCache.Create();
        MessageRouting = MessageRouting.Empty;

        Logger = Server.RemoteClientLoggerFactory?.Invoke( this ) ?? default;
        _timestamps = Server.TimestampsFactory( this );
        var delaySource = Server.DelaySourceFactory?.Invoke( this );
        _delaySource = delaySource is not null ? DelaySource.External( delaySource ) : DelaySource.Owned();
    }

    /// <summary>
    /// <see cref="MessageBrokerServer"/> instance to which this client belongs to.
    /// </summary>
    public MessageBrokerServer Server { get; }

    /// <summary>
    /// Client's unique identifier assigned by the server.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Client's unique name.
    /// </summary>
    /// <remarks>Value will be initialized during handshake with the remote client.</remarks>
    public string Name { get; private set; }

    /// <summary>
    /// Indicates client's endianness.
    /// </summary>
    /// <remarks>Value will be initialized during handshake with the remote client.</remarks>
    public bool IsLittleEndian { get; private set; }

    /// <summary>
    /// Send or receive message timeout.
    /// </summary>
    /// <remarks>
    /// Value will be initialized during handshake with the remote client.
    /// Initially equal to <see cref="Server"/>'s <see cref="MessageBrokerServer.HandshakeTimeout"/>.
    /// </remarks>
    public Duration MessageTimeout { get; private set; }

    /// <summary>
    /// Send ping interval.
    /// </summary>
    /// <remarks>Value will be initialized during handshake with the remote client.</remarks>
    public Duration PingInterval { get; private set; }

    /// <summary>
    /// Max acceptable batch packet count.
    /// </summary>
    /// <remarks>Value will be initialized during handshake with the remote client.</remarks>
    public short MaxBatchPacketCount { get; private set; }

    /// <summary>
    /// Specifies whether or not synchronization of external object names is enabled.
    /// </summary>
    /// <remarks>Value will be initialized during handshake with the remote client.</remarks>
    public bool SynchronizeExternalObjectNames { get; private set; }

    /// <summary>
    /// Specifies whether or not to clear internal buffers once the server is done using them.
    /// </summary>
    /// <remarks>Value will be initialized during handshake with the remote client.</remarks>
    public bool ClearBuffers { get; private set; }

    /// <summary>
    /// Max acceptable network batch packet length.
    /// </summary>
    /// <remarks>
    /// Represents max possible length for packets of <b>Batch</b> type.
    /// Value will be initialized during handshake with the remote client.
    /// </remarks>
    public MemorySize MaxNetworkBatchPacketLength => MemorySize.FromBytes( MaxNetworkBatchPacketBytes );

    /// <summary>
    /// The remote <see cref="IPEndPoint"/> of the remote client to which this client connects to.
    /// </summary>
    public EndPoint? RemoteEndPoint
    {
        get
        {
            using ( AcquireLock() )
            {
                try
                {
                    return _tcp.Client.RemoteEndPoint;
                }
                catch
                {
                    return null;
                }
            }
        }
    }

    /// <summary>
    /// The local <see cref="EndPoint"/> that this client is using for communications with the remote client.
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
    /// <remarks>See <see cref="MessageBrokerRemoteClientState"/> for more information.</remarks>
    public MessageBrokerRemoteClientState State
    {
        get
        {
            using ( AcquireLock() )
                return _state;
        }
    }

    /// <summary>
    /// Collection of <see cref="MessageBrokerChannelPublisherBinding"/> instances attached to this client, identified by channel ids.
    /// </summary>
    public MessageBrokerRemoteClientPublisherCollection Publishers => new MessageBrokerRemoteClientPublisherCollection( this );

    /// <summary>
    /// Collection of <see cref="MessageBrokerChannelListenerBinding"/> instances attached to this client, identified by channel ids.
    /// </summary>
    public MessageBrokerRemoteClientListenerCollection Listeners => new MessageBrokerRemoteClientListenerCollection( this );

    /// <summary>
    /// Collection of <see cref="MessageBrokerQueue"/> instances attached to this client, identified by their names.
    /// </summary>
    public MessageBrokerRemoteClientQueueCollection Queues => new MessageBrokerRemoteClientQueueCollection( this );

    internal Duration MaxReadTimeout { get; private set; }
    internal bool ShouldCancel => _state >= MessageBrokerRemoteClientState.Disposing;

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerRemoteClient"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[{Id}] '{Name}' client ({State})";
    }

    /// <summary>
    /// Disconnects this client from the server.
    /// </summary>
    /// <returns>A task that represents the asynchronous disconnect operation.</returns>
    public async ValueTask DisconnectAsync()
    {
        var traceId = 0UL;
        MessageBrokerRemoteClientState state;
        using ( AcquireLock() )
        {
            state = _state;
            if ( TryBeginDispose() )
                traceId = GetTraceId();
        }

        if ( state >= MessageBrokerRemoteClientState.Disposing )
        {
            if ( state == MessageBrokerRemoteClientState.Disposing )
                await _disconnected.Task.ConfigureAwait( false );

            return;
        }

        using ( MessageBrokerRemoteClientTraceEvent.CreateScope( this, traceId, MessageBrokerRemoteClientTraceEventType.Dispose ) )
            await DisposeAsyncCore( traceId ).ConfigureAwait( false );
    }

    internal async ValueTask OnServerDisposedAsync(ulong serverTraceId)
    {
        var traceId = 0UL;
        MessageBrokerRemoteClientState state;
        using ( AcquireLock() )
        {
            state = _state;
            if ( TryBeginDispose() )
                traceId = GetTraceId();
        }

        if ( state >= MessageBrokerRemoteClientState.Disposing )
        {
            if ( state == MessageBrokerRemoteClientState.Disposing )
                await _disconnected.Task.ConfigureAwait( false );

            return;
        }

        using ( MessageBrokerRemoteClientTraceEvent.CreateScope( this, traceId, MessageBrokerRemoteClientTraceEventType.Dispose ) )
        {
            if ( Logger.ServerTrace is { } serverTrace )
                serverTrace.Emit( MessageBrokerRemoteClientServerTraceEvent.Create( this, traceId, serverTraceId ) );

            await DisposeAsyncCore( traceId, serverDisposed: true ).ConfigureAwait( false );
        }
    }

    internal async ValueTask StartAsync(ulong serverTraceId)
    {
        ulong traceId;
        using ( AcquireLock() )
            traceId = GetTraceId();

        var failed = true;
        if ( Logger.TraceStart is { } traceStart )
            traceStart.Emit( MessageBrokerRemoteClientTraceEvent.Create( this, traceId, MessageBrokerRemoteClientTraceEventType.Start ) );

        if ( Logger.ServerTrace is { } serverTrace )
            serverTrace.Emit( MessageBrokerRemoteClientServerTraceEvent.Create( this, traceId, serverTraceId ) );

        try
        {
            using ( AcquireActiveLock( traceId, out var exc ) )
            {
                if ( exc is not null )
                    return;

                EventScheduler.InitializeResetEvent( _delaySource.GetSource() );
            }

            var eventSchedulerTask = EventScheduler.StartUnderlyingTask( this );
            using ( AcquireActiveLock( traceId, out var exc ) )
            {
                if ( exc is not null )
                    return;

                Assume.IsLessThanOrEqualTo( _state, MessageBrokerRemoteClientState.Handshaking );
                EventScheduler.SetUnderlyingTask( eventSchedulerTask );
                var task = StartHandshakeTask( traceId );
                PacketListener.SetUnderlyingTask( task );
                failed = false;
            }
        }
        catch ( Exception exc )
        {
            if ( Logger.Error is { } error )
                error.Emit( MessageBrokerRemoteClientErrorEvent.Create( this, traceId, exc ) );

            await DisposeAsync( traceId ).ConfigureAwait( false );
        }
        finally
        {
            if ( failed && Logger.TraceEnd is { } traceEnd )
                traceEnd.Emit( MessageBrokerRemoteClientTraceEvent.Create( this, traceId, MessageBrokerRemoteClientTraceEventType.Start ) );
        }
    }

    internal BindResult BindPublisherUnsafe(
        MessageBrokerChannel channel,
        bool channelCreated,
        string streamName,
        ref MessageBrokerChannelPublisherBinding? publisher,
        ref ulong channelTraceId,
        ref MessageBrokerStream? stream,
        ref ulong streamTraceId,
        ref bool streamCreated)
    {
        using ( channel.AcquireLock() )
        {
            if ( channel.ShouldCancel )
                return BindResult.ChannelDisposed;

            var token = channel.PublishersByClientId.GetOrAddNull( Id );
            if ( token.Exists )
            {
                publisher = token.GetObject();
                stream = publisher.Stream;
                return BindResult.AlreadyBound;
            }

            try
            {
                stream = StreamCollection.RegisterUnsafe( Server, streamName, out streamCreated );
                using ( stream.AcquireLock() )
                {
                    if ( stream.ShouldCancel )
                    {
                        token.Revert( ref channel.PublishersByClientId, Id );
                        if ( channelCreated )
                            ChannelCollection.RemoveUnsafe( channel );

                        return BindResult.ParentDisposed;
                    }

                    try
                    {
                        publisher = token.SetObject(
                            ref channel.PublishersByClientId,
                            new MessageBrokerChannelPublisherBinding( this, channel, stream ) );
                    }
                    catch
                    {
                        if ( streamCreated )
                            StreamCollection.RemoveUnsafe( stream );

                        throw;
                    }

                    PublishersByChannelId.Add( channel.Id, publisher );
                    stream.PublishersByClientChannelIdPair.Add( new Pair<int, int>( Id, channel.Id ), publisher );
                    channelTraceId = channel.GetTraceId();
                    streamTraceId = stream.GetTraceId();
                }
            }
            catch
            {
                token.Revert( ref channel.PublishersByClientId, Id );
                throw;
            }
        }

        return BindResult.Success;
    }

    internal UnbindResult BeginUnbindPublisherUnsafe(
        MessageBrokerChannel channel,
        ref MessageBrokerChannelPublisherBinding? publisher,
        ref ulong channelTraceId,
        ref MessageBrokerStream? stream,
        ref ulong streamTraceId,
        ref bool disposingChannel,
        ref bool disposingStream)
    {
        using ( channel.AcquireLock() )
        {
            if ( channel.ShouldCancel )
                return UnbindResult.ChannelDisposed;

            if ( ! PublishersByChannelId.TryGet( channel.Id, out publisher ) )
                return UnbindResult.NotBound;

            stream = publisher.Stream;
            using ( stream.AcquireLock() )
            {
                if ( stream.ShouldCancel )
                    return UnbindResult.ParentDisposed;

                using ( publisher.AcquireLock() )
                {
                    if ( publisher.ShouldCancel )
                        return UnbindResult.BindingDisposed;

                    publisher.BeginDisposingUnsafe();
                    PublishersByChannelId.Remove( channel.Id );
                    disposingChannel = channel.TryDisposeByRemovingPublisherUnsafe( Id );
                    disposingStream = stream.TryDisposeByRemovingPublisherUnsafe( Id, channel.Id );
                    channelTraceId = channel.GetTraceId();
                    streamTraceId = stream.GetTraceId();
                }
            }
        }

        return UnbindResult.Success;
    }

    internal BindResult BindListenerUnsafe(
        MessageBrokerChannel channel,
        bool channelCreated,
        string queueName,
        string? filterExpression,
        IParsedExpressionDelegate<MessageBrokerFilterExpressionContext, bool>? filterExpressionDelegate,
        in Protocol.BindListenerRequestHeader header,
        ref MessageBrokerChannelListenerBinding? listener,
        ref ulong channelTraceId,
        ref MessageBrokerQueue? queue,
        ref ulong queueTraceId,
        ref bool queueCreated)
    {
        using ( channel.AcquireLock() )
        {
            if ( channel.ShouldCancel )
                return BindResult.ChannelDisposed;

            var token = channel.ListenersByClientId.GetOrAddNull( Id );
            if ( token.Exists )
            {
                listener = token.GetObject();
                queue = listener.Queue;
                return BindResult.AlreadyBound;
            }

            try
            {
                queue = RegisterQueue( queueName, out queueCreated );
                using ( queue.AcquireLock() )
                {
                    if ( queue.ShouldCancel )
                    {
                        token.Revert( ref channel.ListenersByClientId, Id );
                        if ( channelCreated )
                            ChannelCollection.RemoveUnsafe( channel );

                        return BindResult.ParentDisposed;
                    }

                    if ( queueCreated )
                        EventScheduler.AddQueue( queue );

                    try
                    {
                        listener = token.SetObject(
                            ref channel.ListenersByClientId,
                            new MessageBrokerChannelListenerBinding(
                                this,
                                channel,
                                queue,
                                header.PrefetchHint,
                                header.MaxRetries,
                                header.RetryDelay,
                                header.MaxRedeliveries,
                                header.MinAckTimeout,
                                header.DeadLetterCapacityHint,
                                header.MinDeadLetterRetention,
                                filterExpression,
                                filterExpressionDelegate ) );
                    }
                    catch
                    {
                        if ( queueCreated )
                            QueueStore.Remove( queue.Id, queue.Name );

                        throw;
                    }

                    ListenersByChannelId.Add( channel.Id, listener );
                    queue.ListenersByChannelId.Add( channel.Id, listener );
                    channelTraceId = channel.GetTraceId();
                    queueTraceId = queue.GetTraceId();
                }
            }
            catch
            {
                token.Revert( ref channel.ListenersByClientId, Id );
                throw;
            }
        }

        return BindResult.Success;
    }

    internal UnbindResult BeginUnbindListenerUnsafe(
        MessageBrokerChannel channel,
        ref MessageBrokerChannelListenerBinding? listener,
        ref ulong channelTraceId,
        ref MessageBrokerQueue? queue,
        ref ulong queueTraceId,
        ref bool disposingChannel,
        ref bool disposingQueue)
    {
        using ( channel.AcquireLock() )
        {
            if ( channel.ShouldCancel )
                return UnbindResult.ChannelDisposed;

            if ( ! ListenersByChannelId.TryGet( channel.Id, out listener ) )
                return UnbindResult.NotBound;

            queue = listener.Queue;
            using ( queue.AcquireLock() )
            {
                if ( queue.ShouldCancel )
                    return UnbindResult.ParentDisposed;

                using ( listener.AcquireLock() )
                {
                    if ( listener.ShouldCancel )
                        return UnbindResult.BindingDisposed;

                    listener.BeginDisposingUnsafe();
                    ListenersByChannelId.Remove( channel.Id );
                    disposingChannel = channel.TryDisposeByRemovingListenerUnsafe( Id );
                    disposingQueue = queue.TryDisposeByRemovingListenerUnsafe( channel.Id );
                    channelTraceId = channel.GetTraceId();
                    queueTraceId = queue.GetTraceId();
                }
            }
        }

        return UnbindResult.Success;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal Timestamp GetTimestamp()
    {
        return _timestamps.GetNow();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ExclusiveLock AcquireLock()
    {
        return ExclusiveLock.SpinWaitEnter( _tcp, spinWaitMultiplier: 4 );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ExclusiveLock AcquireActiveLock(ulong traceId, out MessageBrokerRemoteClientDisposedException? exception)
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
            error.Emit( MessageBrokerRemoteClientErrorEvent.Create( this, traceId, exception ) );

        return default;
    }

    internal async ValueTask<Result> WriteAsync(Protocol.PacketHeader header, ReadOnlyMemory<byte> data, ulong traceId)
    {
        var sendPacket = Logger.SendPacket;
        sendPacket?.Emit( MessageBrokerRemoteClientSendPacketEvent.CreateSending( this, traceId, header ) );
        try
        {
            CancellationToken timeoutToken;
            using ( AcquireActiveLock( traceId, out var exc ) )
            {
                if ( exc is not null )
                    return exc;

                timeoutToken = EventScheduler.ScheduleWriteTimeout( this );
            }

            await _stream.WriteAsync( data, timeoutToken ).ConfigureAwait( false );
        }
        catch ( Exception exc )
        {
            return EmitError( exc, traceId );
        }

        sendPacket?.Emit( MessageBrokerRemoteClientSendPacketEvent.CreateSent( this, traceId, header ) );
        return Result.Valid;
    }

    internal async ValueTask<Result<int>> WritePotentialBatchAsync(Protocol.PacketHeader header, ReadOnlyMemory<byte> data, ulong traceId)
    {
        var batchPoolToken = MemoryPoolToken<byte>.Empty;
        var dataToWrite = data;
        var headerToWrite = header;
        try
        {
            var packetCount = TryPrepareBatchPacket( data.Length, ref batchPoolToken, ref headerToWrite, ref dataToWrite, traceId );
            if ( packetCount.Exception is not null )
                return packetCount.Exception;

            var sendPacket = Logger.SendPacket;
            sendPacket?.Emit( MessageBrokerRemoteClientSendPacketEvent.CreateSending( this, traceId, headerToWrite, packetCount.Value ) );
            try
            {
                CancellationToken timeoutToken;
                using ( AcquireActiveLock( traceId, out var exc ) )
                {
                    if ( exc is not null )
                        return exc;

                    timeoutToken = EventScheduler.ScheduleWriteTimeout( this );
                }

                await _stream.WriteAsync( dataToWrite, timeoutToken ).ConfigureAwait( false );
            }
            catch ( Exception exc )
            {
                return EmitError( exc, traceId );
            }

            sendPacket?.Emit( MessageBrokerRemoteClientSendPacketEvent.CreateSent( this, traceId, headerToWrite ) );
            if ( packetCount.Value > 1 )
                sendPacket?.Emit( MessageBrokerRemoteClientSendPacketEvent.CreateBatched( this, traceId, header, traceId ) );

            return packetCount;
        }
        finally
        {
            batchPoolToken.Return( this, traceId );
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal Exception EmitError(Exception exception, ulong traceId)
    {
        if ( Logger.Error is { } error )
            error.Emit( MessageBrokerRemoteClientErrorEvent.Create( this, traceId, exception ) );

        return exception;
    }

    internal MessageRouting.Result GetMessageRouting(ulong traceId, Protocol.PacketHeader header, int count, ReadOnlySpan<byte> data)
    {
        Assume.IsGreaterThan( count, 0 );
        var read = 0;
        var found = 0;
        var poolToken = MemoryPoolToken<byte>.Empty;
        try
        {
            poolToken = MemoryPool.Rent( Defaults.Memory.GetBufferCapacity( (count + 7) >> 3 ), ClearBuffers, out var buffer );
            var bufferSpan = buffer.Span;
            bufferSpan.Clear();

            while ( data.Length > 1 && read < count )
            {
                MessageBrokerRemoteClient? target;
                var reader = new BinaryContractReader( data );
                if ( (reader.ReadInt8() & 1) == 0 )
                {
                    if ( data.Length < sizeof( byte ) + sizeof( uint ) )
                    {
                        if ( Logger.Error is { } error )
                        {
                            var exc = this.ProtocolException(
                                header,
                                Resources.UnexpectedPacketElementLength( read, data.Length, sizeof( byte ) + sizeof( uint ) ) );

                            error.Emit( MessageBrokerRemoteClientErrorEvent.Create( this, traceId, exc ) );
                        }

                        return MessageRouting.Result.CreateInvalid( poolToken );
                    }

                    reader.Move( sizeof( byte ) );
                    var id = unchecked( ( int )reader.ReadInt32() );
                    if ( id <= 0 )
                    {
                        if ( Logger.Error is { } error )
                        {
                            var exc = this.ProtocolException( header, Resources.TargetIdIsNotPositive( read, id ) );
                            error.Emit( MessageBrokerRemoteClientErrorEvent.Create( this, traceId, exc ) );
                        }

                        return MessageRouting.Result.CreateInvalid( poolToken );
                    }

                    ++read;
                    data = data.Slice( sizeof( byte ) + sizeof( uint ) );
                    target = RemoteClientCollection.TryGetById( Server, id );
                    if ( target is null )
                    {
                        if ( Logger.Error is { } error )
                        {
                            var exception = this.Exception( Resources.TargetByIdDoesNotExist( read - 1, id ) );
                            error.Emit( MessageBrokerRemoteClientErrorEvent.Create( this, traceId, exception ) );
                        }

                        continue;
                    }
                }
                else
                {
                    var byteLength = reader.ReadInt16() >> 1;
                    var totalLength = sizeof( ushort ) + byteLength;
                    if ( data.Length < totalLength )
                    {
                        if ( Logger.Error is { } error )
                        {
                            var exc = this.ProtocolException(
                                header,
                                Resources.UnexpectedPacketElementLength( read, data.Length, totalLength ) );

                            error.Emit( MessageBrokerRemoteClientErrorEvent.Create( this, traceId, exc ) );
                        }

                        return MessageRouting.Result.CreateInvalid( poolToken );
                    }

                    var name = TextEncoding.Parse( data.Slice( sizeof( ushort ), byteLength ) );
                    if ( name.Exception is not null )
                    {
                        if ( Logger.Error is { } error )
                            error.Emit( MessageBrokerRemoteClientErrorEvent.Create( this, traceId, name.Exception ) );

                        return MessageRouting.Result.CreateInvalid( poolToken );
                    }

                    Assume.IsNotNull( name.Value );
                    if ( ! Defaults.NameLengthBounds.Contains( name.Value.Length ) )
                    {
                        if ( Logger.Error is { } error )
                        {
                            var exc = this.ProtocolException( header, Resources.InvalidTargetNameLength( read, name.Value.Length ) );
                            error.Emit( MessageBrokerRemoteClientErrorEvent.Create( this, traceId, exc ) );
                        }

                        return MessageRouting.Result.CreateInvalid( poolToken );
                    }

                    ++read;
                    data = data.Slice( totalLength );
                    target = RemoteClientCollection.TryGetByName( Server, name.Value );
                    if ( target is null )
                    {
                        if ( Logger.Error is { } error )
                        {
                            var exception = this.Exception( Resources.TargetByNameDoesNotExist( read - 1, name.Value ) );
                            error.Emit( MessageBrokerRemoteClientErrorEvent.Create( this, traceId, exception ) );
                        }

                        continue;
                    }
                }

                ++found;
                var position = target.Id - 1;
                var index = position >> 3;
                if ( index >= buffer.Length )
                {
                    var oldLength = bufferSpan.Length;
                    poolToken.IncreaseLength( Defaults.Memory.GetBufferCapacity( (target.Id + 7) >> 3 ), out buffer );
                    bufferSpan = buffer.Span;
                    bufferSpan.Slice( oldLength ).Clear();
                }

                ref var element = ref Unsafe.Add( ref MemoryMarshal.GetReference( bufferSpan ), index );
                if ( ((element >> (position & 7)) & 1) != 0 )
                {
                    if ( Logger.Error is { } error )
                    {
                        var exception = this.Exception( Resources.TargetDuplicateFound( read - 1, target ) );
                        error.Emit( MessageBrokerRemoteClientErrorEvent.Create( this, traceId, exception ) );
                    }
                }

                element |= ( byte )(1 << (position & 7));
            }

            if ( read < count )
            {
                if ( Logger.Error is { } error )
                {
                    var exc = this.ProtocolException( header, Resources.TargetCountIsTooLarge( read, count ) );
                    error.Emit( MessageBrokerRemoteClientErrorEvent.Create( this, traceId, exc ) );
                }

                return MessageRouting.Result.CreateInvalid( poolToken );
            }

            if ( data.Length > 0 )
            {
                if ( Logger.Error is { } error )
                {
                    var exc = this.ProtocolException( header, Resources.TooLargeHeaderPayload( data.Length ) );
                    error.Emit( MessageBrokerRemoteClientErrorEvent.Create( this, traceId, exc ) );
                }

                return MessageRouting.Result.CreateInvalid( poolToken );
            }

            return MessageRouting.Result.Create( poolToken, buffer, traceId, found );
        }
        catch ( Exception exc )
        {
            if ( Logger.Error is { } error )
                error.Emit( MessageBrokerRemoteClientErrorEvent.Create( this, traceId, exc ) );

            return MessageRouting.Result.CreateInvalid( poolToken );
        }
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

        _state = MessageBrokerRemoteClientState.Disposing;
        return true;
    }

    internal ValueTask DisposeAsync(ulong traceId)
    {
        using ( AcquireLock() )
        {
            if ( ShouldCancel )
                return ValueTask.CompletedTask;

            _state = MessageBrokerRemoteClientState.Disposing;
        }

        return DisposeAsyncCore( traceId );
    }

    internal async ValueTask DisposeAsyncCore(ulong traceId, bool serverDisposed = false)
    {
        try
        {
            if ( Logger.Disposing is { } disposing )
                disposing.Emit( MessageBrokerRemoteClientDisposingEvent.Create( this, traceId ) );

            Task? eventSchedulerTask;
            Task? requestHandlerTask;
            Task? responseSenderTask;
            Task? packetListenerTask;
            Task? notificationSenderTask;
            ValueTaskDelaySource? ownedDelaySource;

            var error = Logger.Error;
            Chain<Exception> exceptions;
            using ( AcquireLock() )
            {
                ownedDelaySource = _delaySource.DiscardOwnedSource();
                eventSchedulerTask = EventScheduler.DiscardUnderlyingTask();
                requestHandlerTask = RequestHandler.DiscardUnderlyingTask();
                responseSenderTask = ResponseSender.DiscardUnderlyingTask();
                packetListenerTask = PacketListener.DiscardUnderlyingTask();
                notificationSenderTask = NotificationSender.DiscardUnderlyingTask();
                RequestHandler.Dispose();
                ResponseSender.BeginDispose();
                EventScheduler.Dispose();
                NotificationSender.BeginDispose();
                WriterQueue.Dispose();
                exceptions = RequestQueue.Dispose( error is not null );
            }

            foreach ( var exc in exceptions )
            {
                Assume.IsNotNull( error );
                error.Emit( MessageBrokerRemoteClientErrorEvent.Create( this, traceId, exc ) );
            }

            if ( eventSchedulerTask is not null )
                await eventSchedulerTask.ConfigureAwait( false );

            if ( requestHandlerTask is not null )
                await requestHandlerTask.ConfigureAwait( false );

            if ( responseSenderTask is not null )
                await responseSenderTask.ConfigureAwait( false );

            if ( packetListenerTask is not null )
                await packetListenerTask.ConfigureAwait( false );

            if ( notificationSenderTask is not null )
                await notificationSenderTask.ConfigureAwait( false );

            MessageBrokerChannelPublisherBinding[] publishers;
            MessageBrokerChannelListenerBinding[] listeners;
            MessageBrokerQueue[] queues;
            Exception? exception;
            using ( AcquireLock() )
            {
                publishers = PublishersByChannelId.ClearAndExtract();
                listeners = ListenersByChannelId.ClearAndExtract();
                queues = QueueStore.Clear();
                exception = _tcp.TryDispose().Exception;
            }

            if ( exception is not null )
                error?.Emit( MessageBrokerRemoteClientErrorEvent.Create( this, traceId, exception ) );

            if ( serverDisposed )
            {
                await Parallel.ForEachAsync( queues, (q, _) => q.OnServerDisposedAsync( traceId ) ).ConfigureAwait( false );
                foreach ( var publisher in publishers )
                    publisher.OnServerDisposed();

                foreach ( var listener in listeners )
                    listener.OnServerDisposed();
            }
            else
            {
                await Parallel.ForEachAsync( queues, (q, _) => q.OnClientDisconnectedAsync( traceId ) ).ConfigureAwait( false );
                await Parallel.ForEachAsync( publishers, (p, _) => p.OnClientDisconnectedAsync( traceId ) ).ConfigureAwait( false );
                foreach ( var listener in listeners )
                    listener.OnClientDisconnected( traceId );
            }

            int discardedNotificationCount;
            ListSlim<ResponseSender.DiscardedResponse> discardedMessages;
            using ( AcquireLock() )
            {
                (discardedNotificationCount, exceptions) = NotificationSender.EndDispose( error is not null );
                discardedMessages = ResponseSender.EndDispose();
            }

            EndDiscardedResponses( ref discardedMessages, traceId );
            foreach ( var exc in exceptions )
            {
                Assume.IsNotNull( error );
                error.Emit( MessageBrokerRemoteClientErrorEvent.Create( this, traceId, exc ) );
            }

            if ( discardedNotificationCount > 0 && error is not null )
            {
                var exc = this.Exception( Resources.NotificationsDiscarded( discardedNotificationCount ) );
                error.Emit( MessageBrokerRemoteClientErrorEvent.Create( this, traceId, exc ) );
            }

            using ( AcquireLock() )
            {
                exception = MessageRouting.PoolToken.Return();
                MessageRouting = MessageRouting.Empty;
                ExternalNameCache.Clear();
                _state = MessageBrokerRemoteClientState.Disposed;
            }

            if ( exception is not null )
                error?.Emit( MessageBrokerRemoteClientErrorEvent.Create( this, traceId, exception ) );

            if ( ownedDelaySource is not null )
                await ownedDelaySource.TryDisposeAsync().ConfigureAwait( false );

            if ( ! serverDisposed )
            {
                var removeException = RemoteClientCollection.Remove( this ).Exception;
                if ( removeException is not null )
                    error?.Emit( MessageBrokerRemoteClientErrorEvent.Create( this, traceId, removeException ) );
            }

            if ( Logger.Disposed is { } disposed )
                disposed.Emit( MessageBrokerRemoteClientDisposedEvent.Create( this, traceId ) );
        }
        finally
        {
            if ( ! _disconnected.Task.IsCompleted )
                _disconnected.SetResult();
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void EndDiscardedResponses(ref ListSlim<ResponseSender.DiscardedResponse> discardedResponses, ulong traceId)
    {
        var error = Logger.Error;
        var traceEnd = Logger.TraceEnd;
        foreach ( ref readonly var r in discardedResponses )
        {
            r.PoolToken.Return( this, traceId );
            if ( error is not null )
            {
                var exc = this.DisposedException();
                error.Emit( MessageBrokerRemoteClientErrorEvent.Create( this, r.TraceId, exc ) );
            }

            traceEnd?.Emit( MessageBrokerRemoteClientTraceEvent.Create( this, r.TraceId, r.EventType ) );
        }

        discardedResponses = ListSlim<ResponseSender.DiscardedResponse>.Create();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Result<int> TryPrepareBatchPacket(
        int packetLength,
        ref MemoryPoolToken<byte> batchPoolToken,
        ref Protocol.PacketHeader headerToWrite,
        ref ReadOnlyMemory<byte> dataToWrite,
        ulong traceId)
    {
        var packetCount = 1;
        if ( MaxBatchPacketCount == 0 )
            return packetCount;

        var batchLength = unchecked( ( long )Protocol.PacketHeader.Length + Protocol.BatchHeader.Length + packetLength );
        if ( batchLength >= MaxNetworkBatchPacketBytes )
            return packetCount;

        var clearBuffer = ClearBuffers;
        using ( AcquireActiveLock( traceId, out var exc ) )
        {
            if ( exc is not null )
                return exc;

            packetCount = WriterQueue.GetLargestAvailableBatchCount( this, ref batchLength, ref clearBuffer );
        }

        if ( packetCount == 1 )
            return packetCount;

        batchPoolToken = batchLength > MemoryPool.SegmentLength
            ? Server.MemoryPool.Rent( unchecked( ( int )batchLength ), clearBuffer, out var batchData )
            : MemoryPool.Rent( unchecked( ( int )batchLength ), clearBuffer, out batchData );

        dataToWrite = batchData;

        headerToWrite = Protocol.PacketHeader.Create(
            MessageBrokerClientEndpoint.Batch,
            unchecked( ( uint )(batchLength - Protocol.PacketHeader.Length) ) );

        Protocol.BatchHeader.Serialize( batchData, headerToWrite.Payload, unchecked( ( short )packetCount ) );
        var remainingData = batchData.Slice( Protocol.PacketHeader.Length + Protocol.BatchHeader.Length );

        using ( AcquireActiveLock( traceId, out var exc ) )
        {
            if ( exc is not null )
                return exc;

            WriterQueue.CopyToBatch( remainingData, packetCount );
        }

        return packetCount;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Exception HandleUnexpectedEndpoint(Protocol.PacketHeader header, ulong traceId)
    {
        return EmitError( this.ProtocolException( header, Resources.UnexpectedServerEndpoint ), traceId );
    }

    private MessageBrokerQueue RegisterQueue(string name, out bool created)
    {
        var token = QueueStore.GetOrAddNull( name );
        if ( token.Exists )
        {
            created = false;
            return token.GetObject();
        }

        try
        {
            created = true;
            return token.SetObject( ref QueueStore, new MessageBrokerQueue( this, token.Id, name ) );
        }
        catch
        {
            token.Revert( ref QueueStore, name );
            throw;
        }
    }
}
