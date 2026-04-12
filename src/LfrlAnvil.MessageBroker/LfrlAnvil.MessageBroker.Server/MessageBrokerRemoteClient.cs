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
using System.Collections.Generic;
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
using LfrlAnvil.Exceptions;
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
    internal readonly MessageBrokerRemoteClientLogger Logger;
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

    internal readonly int MaxNetworkPacketBytes;
    internal readonly int MaxNetworkMessagePacketBytes;
    private ServerStorage.Client _storage;
    private Duration _messageTimeout;
    private Duration _pingInterval;
    private Duration _maxReadTimeout;
    private int _maxNetworkBatchPacketBytes;
    private MessageBrokerRemoteClientState _state;
    private bool _isLittleEndian;
    private bool _clearBuffers;
    private bool _synchronizeExternalObjectNames;
    private bool _isEphemeral;
    private short _maxBatchPacketCount;

    private readonly object _sync = new object();
    private readonly ITimestampProvider _timestamps;
    private TcpClient? _tcp;
    private Stream? _stream;
    private TaskCompletionSource? _deactivated;
    private DelaySource _delaySource;
    private ulong _nextTraceId;

    private MessageBrokerRemoteClient(
        int id,
        MessageBrokerServer server,
        string name,
        TcpClient? tcp,
        Stream? stream,
        Duration messageTimeout,
        Duration pingInterval,
        short maxBatchPacketCount,
        MemorySize maxNetworkBatchPacketLength,
        bool isLittleEndian,
        bool synchronizeExternalObjectNames,
        bool clearBuffers,
        bool isEphemeral,
        ulong nextTraceId,
        MessageBrokerRemoteClientState state)
    {
        _storage = server.Storage.CreateForClient( id, isEphemeral );
        _tcp = tcp;
        _stream = stream;
        Server = server;
        Id = id;
        Name = name;
        MemoryPool = new MemoryPool<byte>( unchecked( ( int )server.MaxNetworkPacketLength.Bytes ) );

        SetProperties(
            messageTimeout,
            pingInterval,
            maxBatchPacketCount,
            maxNetworkBatchPacketLength,
            isLittleEndian,
            synchronizeExternalObjectNames,
            clearBuffers,
            isEphemeral );

        MaxNetworkPacketBytes = unchecked( ( int )Server.MaxNetworkPacketLength.Bytes );
        MaxNetworkMessagePacketBytes = Server.MaxNetworkMessagePacketBytes;

        _state = state;
        _nextTraceId = nextTraceId;

        PublishersByChannelId = ReferenceStore<int, MessageBrokerChannelPublisherBinding>.Create();
        ListenersByChannelId = ReferenceStore<int, MessageBrokerChannelListenerBinding>.Create();
        QueueStore = ObjectStore<MessageBrokerQueue>.Create( StringComparer.OrdinalIgnoreCase );
        WriterQueue = WriterQueue.Create();
        RequestQueue = RequestQueue.Create();
        EventScheduler = EventScheduler.Create();
        PacketListener = PacketListener.Create();
        NotificationSender = NotificationSender.Create( tcp is not null );
        RequestHandler = RequestHandler.Create( tcp is not null );
        ResponseSender = ResponseSender.Create( tcp is not null );
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
    public string Name { get; }

    /// <summary>
    /// Indicates client's endianness.
    /// </summary>
    public bool IsLittleEndian
    {
        get
        {
            using ( AcquireLock() )
                return _isLittleEndian;
        }
    }

    /// <summary>
    /// Send or receive message timeout.
    /// </summary>
    public Duration MessageTimeout
    {
        get
        {
            using ( AcquireLock() )
                return _messageTimeout;
        }
    }

    /// <summary>
    /// Send ping interval.
    /// </summary>
    public Duration PingInterval
    {
        get
        {
            using ( AcquireLock() )
                return _pingInterval;
        }
    }

    /// <summary>
    /// Max acceptable batch packet count.
    /// </summary>
    public short MaxBatchPacketCount
    {
        get
        {
            using ( AcquireLock() )
                return _maxBatchPacketCount;
        }
    }

    /// <summary>
    /// Specifies whether synchronisation of external object names is enabled.
    /// </summary>
    public bool SynchronizeExternalObjectNames
    {
        get
        {
            using ( AcquireLock() )
                return _synchronizeExternalObjectNames;
        }
    }

    /// <summary>
    /// Specifies whether to clear internal buffers once the server is done using them.
    /// </summary>
    public bool ClearBuffers
    {
        get
        {
            using ( AcquireLock() )
                return _clearBuffers;
        }
    }

    /// <summary>
    /// Specifies whether the client is ephemeral.
    /// </summary>
    public bool IsEphemeral
    {
        get
        {
            using ( AcquireLock() )
                return _isEphemeral;
        }
    }

    /// <summary>
    /// Max acceptable network batch packet length.
    /// </summary>
    /// <remarks>
    /// Represents max possible length for packets of <b>Batch</b> type.
    /// Value will be initialised during handshake with the remote client.
    /// </remarks>
    public MemorySize MaxNetworkBatchPacketLength
    {
        get
        {
            using ( AcquireLock() )
                return MemorySize.FromBytes( _maxNetworkBatchPacketBytes );
        }
    }

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
                    return _tcp?.Client.RemoteEndPoint;
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
                    return _tcp?.Client.LocalEndPoint;
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

    internal bool IsInactive => _state >= MessageBrokerRemoteClientState.Deactivating;
    internal bool IsDisposed => _state >= MessageBrokerRemoteClientState.Disposing;

    internal Duration MaxReadTimeout
    {
        get
        {
            using ( AcquireLock() )
                return _maxReadTimeout;
        }
    }

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
        TaskCompletionSource? deactivated;
        bool isEphemeral;

        using ( AcquireLock() )
        {
            state = _state;
            if ( TryBeginDeactivate( out isEphemeral ) )
                traceId = GetTraceId();

            deactivated = _deactivated;
        }

        if ( state >= MessageBrokerRemoteClientState.Deactivating )
        {
            if ( state is MessageBrokerRemoteClientState.Deactivating or MessageBrokerRemoteClientState.Disposing
                && deactivated is not null )
                await deactivated.Task.ConfigureAwait( false );

            return;
        }

        using ( MessageBrokerRemoteClientTraceEvent.CreateScope( this, traceId, MessageBrokerRemoteClientTraceEventType.Deactivate ) )
            await DeactivateAsyncCore( traceId, isEphemeral ).ConfigureAwait( false );
    }

    /// <summary>
    /// Deletes this client from the server.
    /// </summary>
    /// <exception cref="MessageBrokerServerException">
    /// When server is in <see cref="MessageBrokerServerState.Created"/> or <see cref="MessageBrokerServerState.Starting"/> state.
    /// </exception>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    public async ValueTask DeleteAsync()
    {
        if ( Server.State < MessageBrokerServerState.Running )
            ExceptionThrower.Throw( Server.Exception( Resources.ServerIsNotRunning ) );

        var traceId = 0UL;
        bool isEphemeral;

        while ( true )
        {
            MessageBrokerRemoteClientState state;
            TaskCompletionSource? deactivated;
            using ( AcquireLock() )
            {
                state = _state;
                if ( TryBeginDeactivate( out isEphemeral, forceDisposal: true ) )
                    traceId = GetTraceId();

                deactivated = _deactivated;
            }

            if ( state < MessageBrokerRemoteClientState.Deactivating )
                break;

            if ( state is MessageBrokerRemoteClientState.Deactivating or MessageBrokerRemoteClientState.Disposing
                && deactivated is not null )
                await deactivated.Task.ConfigureAwait( false );

            if ( state >= MessageBrokerRemoteClientState.Disposing )
                return;

            using ( AcquireLock() )
            {
                if ( _state == MessageBrokerRemoteClientState.Disposed )
                    return;

                if ( _state == MessageBrokerRemoteClientState.Inactive )
                {
                    _state = MessageBrokerRemoteClientState.Disposing;
                    traceId = GetTraceId();
                    isEphemeral = _isEphemeral;
                    _deactivated = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
                    break;
                }
            }
        }

        using ( MessageBrokerRemoteClientTraceEvent.CreateScope( this, traceId, MessageBrokerRemoteClientTraceEventType.Deactivate ) )
            await DeactivateAsyncCore( traceId, isEphemeral, reason: ClientStopReason.Delete ).ConfigureAwait( false );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerRemoteClient Create(
        int id,
        MessageBrokerServer server,
        string name,
        TcpClient tcp,
        Stream stream,
        Duration messageTimeout,
        Duration pingInterval,
        short maxBatchPacketCount,
        MemorySize maxNetworkBatchPacketLength,
        bool isLittleEndian,
        bool synchronizeExternalObjectNames,
        bool clearBuffers,
        bool isEphemeral)
    {
        return new MessageBrokerRemoteClient(
            id,
            server,
            name,
            tcp,
            stream,
            messageTimeout,
            pingInterval,
            maxBatchPacketCount,
            maxNetworkBatchPacketLength,
            isLittleEndian,
            synchronizeExternalObjectNames,
            clearBuffers,
            isEphemeral,
            nextTraceId: 0,
            MessageBrokerRemoteClientState.Created );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerRemoteClient CreateInactive(int id, MessageBrokerServer server, string name, ulong nextTraceId)
    {
        return new MessageBrokerRemoteClient(
            id,
            server,
            name,
            tcp: null,
            stream: null,
            messageTimeout: server.HandshakeTimeout,
            pingInterval: server.AcceptablePingInterval.Min,
            maxBatchPacketCount: 0,
            maxNetworkBatchPacketLength: MemorySize.Zero,
            isLittleEndian: BitConverter.IsLittleEndian,
            synchronizeExternalObjectNames: false,
            clearBuffers: true,
            isEphemeral: false,
            nextTraceId,
            MessageBrokerRemoteClientState.Inactive );
    }

    internal async ValueTask OnServerDisposedAsync(ulong serverTraceId, bool storageLoaded)
    {
        var traceId = 0UL;
        MessageBrokerRemoteClientState state;
        TaskCompletionSource? deactivated;
        bool isEphemeral;

        using ( AcquireLock() )
        {
            state = _state;
            if ( TryBeginDeactivate( out isEphemeral, forceDisposal: true ) )
                traceId = GetTraceId();

            deactivated = _deactivated;
        }

        if ( state >= MessageBrokerRemoteClientState.Deactivating )
        {
            if ( state is MessageBrokerRemoteClientState.Deactivating or MessageBrokerRemoteClientState.Disposing
                && deactivated is not null )
                await deactivated.Task.ConfigureAwait( false );

            if ( state >= MessageBrokerRemoteClientState.Disposing )
                return;

            using ( AcquireLock() )
            {
                if ( _state == MessageBrokerRemoteClientState.Disposed )
                    return;

                state = _state;
                if ( _state == MessageBrokerRemoteClientState.Inactive )
                {
                    Assume.Equals( _isEphemeral, isEphemeral );
                    _state = MessageBrokerRemoteClientState.Disposing;
                    traceId = GetTraceId();
                    _deactivated = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
                }

                deactivated = _deactivated;
            }

            if ( state == MessageBrokerRemoteClientState.Disposing )
            {
                if ( deactivated is not null )
                    await deactivated.Task.ConfigureAwait( false );

                return;
            }
        }

        using ( MessageBrokerRemoteClientTraceEvent.CreateScope( this, traceId, MessageBrokerRemoteClientTraceEventType.Deactivate ) )
        {
            if ( Logger.ServerTrace is { } serverTrace )
                serverTrace.Emit( MessageBrokerRemoteClientServerTraceEvent.Create( this, traceId, serverTraceId ) );

            await DeactivateAsyncCore( traceId, isEphemeral, reason: ClientStopReason.Server, storageLoaded: storageLoaded )
                .ConfigureAwait( false );
        }
    }

    internal bool PrepareForReactivation(TcpClient tcp, Stream stream, in Protocol.HandshakeRequestHeader handshake)
    {
        using ( AcquireLock() )
        {
            if ( _state != MessageBrokerRemoteClientState.Inactive )
                return false;

            Assume.False( _isEphemeral );
            Assume.IsNull( _tcp );
            Assume.IsNull( _stream );
            _state = MessageBrokerRemoteClientState.Created;
            _tcp = tcp;
            _stream = stream;

            SetProperties(
                handshake.MessageTimeout,
                handshake.PingInterval,
                handshake.MaxBatchPacketCount,
                handshake.MaxNetworkBatchPacketLength,
                handshake.IsClientLittleEndian,
                handshake.SynchronizeExternalObjectNames,
                handshake.ClearBuffers,
                handshake.IsEphemeral );

            NotificationSender.ResetSignal();
            RequestHandler.ResetSignal();
            ResponseSender.ResetSignal();
        }

        return true;
    }

    internal async ValueTask StartAsync(ulong serverTraceId)
    {
        ulong traceId;
        using ( AcquireLock() )
            traceId = GetTraceId();

        if ( Logger.TraceStart is { } traceStart )
            traceStart.Emit( MessageBrokerRemoteClientTraceEvent.Create( this, traceId, MessageBrokerRemoteClientTraceEventType.Start ) );

        if ( Logger.ServerTrace is { } serverTrace )
            serverTrace.Emit( MessageBrokerRemoteClientServerTraceEvent.Create( this, traceId, serverTraceId ) );

        var failed = true;
        try
        {
            ServerStorage.Client storage;
            bool isEphemeral;

            using ( AcquireActiveLock( traceId, out var exc ) )
            {
                if ( exc is not null )
                    return;

                storage = _storage;
                isEphemeral = _isEphemeral;
                if ( isEphemeral && storage.IsActive )
                {
                    _storage = Server.Storage.CreateForClient( Id, isEphemeral: true );

                    if ( ListenersByChannelId.Count > 0 )
                    {
                        var ephemeralListeners = ListenersByChannelId.GetAll();
                        foreach ( var listener in ephemeralListeners )
                            listener.MarkAsEphemeral();
                    }

                    if ( PublishersByChannelId.Count > 0 )
                    {
                        var ephemeralPublishers = PublishersByChannelId.GetAll();
                        foreach ( var publisher in ephemeralPublishers )
                            publisher.MarkAsEphemeral();
                    }

                    if ( QueueStore.Count > 0 )
                    {
                        var ephemeralQueues = QueueStore.GetAll();
                        foreach ( var queue in ephemeralQueues )
                            queue.MarkAsEphemeral();
                    }
                }
            }

            if ( isEphemeral )
                EmitError( await storage.DeleteAsync( this ).AsSafe().ConfigureAwait( false ), traceId );
            else
                await storage.SaveMetadataAsync( this, traceId, skipDisposed: true ).ConfigureAwait( false );

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

                _state = MessageBrokerRemoteClientState.Handshaking;
                EventScheduler.SetUnderlyingTask( eventSchedulerTask );
                var task = StartHandshakeTask( traceId );
                PacketListener.SetUnderlyingTask( task );
            }

            failed = false;
        }
        catch ( Exception exc )
        {
            if ( Logger.Error is { } error )
                error.Emit( MessageBrokerRemoteClientErrorEvent.Create( this, traceId, exc ) );

            await DeactivateAsync( traceId, DeactivationSource.Manual ).ConfigureAwait( false );
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
        bool isEphemeral,
        ref MessageBrokerChannelPublisherBinding? existingPublisher,
        ref MessageBrokerChannelPublisherBinding? publisher,
        ref MessageBrokerStream? existingStream,
        ref MessageBrokerStream? stream,
        ref ulong channelTraceId,
        ref ulong existingStreamTraceId,
        ref ulong streamTraceId,
        ref bool streamCreated,
        ref bool disposingExistingStream,
        ref bool disposingExistingPublisher)
    {
        using ( channel.AcquireLock() )
        {
            if ( channel.IsDisposed )
                return BindResult.ChannelDisposed;

            var token = channel.PublishersByClientId.GetOrAddNull( Id );
            if ( token.Exists )
            {
                publisher = token.GetObject();
                existingPublisher = publisher;
                existingStream = publisher.Stream;
                stream = existingStream;

                using ( stream.AcquireLock() )
                {
                    if ( stream.IsDisposed )
                    {
                        Assume.False( channelCreated );
                        return BindResult.ParentDisposed;
                    }

                    var reactivationResult = publisher.TryReactivate( streamName, isEphemeral );
                    if ( reactivationResult == ReactivationResult.Reactivated )
                    {
                        streamTraceId = stream.GetTraceId();
                        return BindResult.Success;
                    }

                    if ( reactivationResult == ReactivationResult.AlreadyBound )
                        return BindResult.AlreadyBound;
                }

                try
                {
                    stream = StreamCollection.RegisterUnsafe( Server, streamName, out streamCreated );
                    using ( stream.AcquireLock() )
                    {
                        if ( stream.IsDisposed )
                        {
                            Assume.False( channelCreated );
                            return BindResult.ParentDisposed;
                        }

                        try
                        {
                            publisher = token.SetObject(
                                ref channel.PublishersByClientId,
                                MessageBrokerChannelPublisherBinding.Create( this, channel, stream, isEphemeral ) );
                        }
                        catch
                        {
                            if ( streamCreated )
                                StreamCollection.RemoveUnsafe( stream );

                            throw;
                        }

                        PublishersByChannelId.Set( channel.Id, publisher );
                        stream.PublishersByClientChannelIdPair.Add( Pair.Create( Id, channel.Id ), publisher );
                        channelTraceId = channel.GetTraceId();
                        streamTraceId = stream.GetTraceId();
                    }
                }
                catch
                {
                    existingPublisher.RevertRebinding();
                    throw;
                }

                using ( existingStream.AcquireLock() )
                {
                    if ( ! existingStream.IsDisposed )
                    {
                        using ( existingPublisher.AcquireLock() )
                        {
                            if ( existingPublisher.TryBeginDisposingUnsafe() )
                            {
                                disposingExistingPublisher = true;
                                disposingExistingStream = existingStream.TryDisposeByRemovingPublisherUnsafe( Id, channel.Id );
                                existingStreamTraceId = existingStream.GetTraceId();
                            }
                        }
                    }
                }

                return BindResult.Success;
            }

            try
            {
                stream = StreamCollection.RegisterUnsafe( Server, streamName, out streamCreated );
                using ( stream.AcquireLock() )
                {
                    if ( stream.IsDisposed )
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
                            MessageBrokerChannelPublisherBinding.Create( this, channel, stream, isEphemeral ) );
                    }
                    catch
                    {
                        if ( streamCreated )
                            StreamCollection.RemoveUnsafe( stream );

                        throw;
                    }

                    PublishersByChannelId.Add( channel.Id, publisher );
                    stream.PublishersByClientChannelIdPair.Add( Pair.Create( Id, channel.Id ), publisher );
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
            if ( channel.IsDisposed )
                return UnbindResult.ChannelDisposed;

            if ( ! PublishersByChannelId.TryGet( channel.Id, out publisher ) )
                return UnbindResult.NotBound;

            stream = publisher.Stream;
            using ( stream.AcquireLock() )
            {
                if ( stream.IsDisposed )
                    return UnbindResult.ParentDisposed;

                using ( publisher.AcquireLock() )
                {
                    if ( ! publisher.TryBeginDisposingUnsafe() )
                        return UnbindResult.BindingDisposed;

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
        bool isEphemeral,
        IParsedExpressionDelegate<MessageBrokerFilterExpressionContext, bool>? filterExpressionDelegate,
        in Protocol.BindListenerRequestHeader header,
        ref MessageBrokerChannelListenerBinding? existingListener,
        ref MessageBrokerChannelListenerBinding? listener,
        ref MessageBrokerQueueListenerBinding? queueBinding,
        ref ulong channelTraceId,
        ref MessageBrokerQueue? queue,
        ref ulong queueTraceId,
        ref bool queueCreated)
    {
        using ( channel.AcquireLock() )
        {
            if ( channel.IsDisposed )
                return BindResult.ChannelDisposed;

            var token = channel.ListenersByClientId.GetOrAddNull( Id );
            if ( token.Exists )
            {
                listener = token.GetObject();
                existingListener = listener;

                using ( listener.AcquireLock() )
                    queueBinding = listener.QueueBindingCollection.Primary;

                queue = queueBinding.Queue;

                using ( queue.AcquireLock() )
                {
                    if ( queue.IsInactive )
                    {
                        Assume.False( channelCreated );
                        return BindResult.ParentDisposed;
                    }

                    if ( ! listener.TryReactivate( queueName, in header, filterExpression, filterExpressionDelegate, isEphemeral ) )
                        return BindResult.AlreadyBound;

                    queueTraceId = queue.GetTraceId();
                    return BindResult.Success;
                }
            }

            try
            {
                queue = RegisterQueue( queueName, out queueCreated );
                using ( queue.AcquireLock() )
                {
                    if ( queue.IsInactive )
                    {
                        token.Revert( ref channel.ListenersByClientId, Id );
                        if ( channelCreated )
                            ChannelCollection.RemoveUnsafe( channel );

                        return BindResult.ParentDisposed;
                    }

                    try
                    {
                        listener = token.SetObject(
                            ref channel.ListenersByClientId,
                            MessageBrokerChannelListenerBinding.Create(
                                this,
                                channel,
                                queue,
                                in header,
                                filterExpression,
                                filterExpressionDelegate,
                                isEphemeral ) );

                        using ( listener.AcquireLock() )
                            queueBinding = listener.QueueBindingCollection.Primary;
                    }
                    catch
                    {
                        if ( queueCreated )
                            QueueStore.Remove( queue.Id, queue.Name );

                        throw;
                    }

                    ListenersByChannelId.Add( channel.Id, listener );
                    queue.ListenersByChannelId.Add( channel.Id, queueBinding );
                    channelTraceId = channel.GetTraceId();
                    queueTraceId = queue.GetTraceId();

                    if ( queueCreated )
                        EventScheduler.AddQueue( queue );
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
        ref bool disposingChannel,
        ref QueueListenerUnbindingPayload primaryBindingPayload,
        ref QueueListenerUnbindingPayload[] secondaryBindingPayloads)
    {
        using ( channel.AcquireLock() )
        {
            if ( channel.IsDisposed )
                return UnbindResult.ChannelDisposed;

            if ( ! ListenersByChannelId.TryGet( channel.Id, out listener ) )
                return UnbindResult.NotBound;

            MessageBrokerQueueListenerBinding? primaryBinding;
            ReadOnlyArray<MessageBrokerQueueListenerBinding> secondaryBindings;
            using ( listener.AcquireLock() )
            {
                if ( ! listener.TryBeginDisposingUnsafe( out primaryBinding, out secondaryBindings ) )
                    return UnbindResult.BindingDisposed;

                disposingChannel = channel.TryDisposeByRemovingListenerUnsafe( Id );
                channelTraceId = channel.GetTraceId();
            }

            using ( primaryBinding.Queue.AcquireLock() )
            {
                if ( ! primaryBinding.Queue.IsInactive && primaryBinding.TryMarkAsDisposed() )
                {
                    var disposingQueue = primaryBinding.Queue.TryDisposeByRemovingListenerUnsafe( channel.Id );
                    var queueTraceId = primaryBinding.Queue.GetTraceId();
                    primaryBindingPayload = new QueueListenerUnbindingPayload( primaryBinding, queueTraceId, disposingQueue );
                }
            }

            if ( secondaryBindings.Count > 0 )
            {
                var i = 0;
                secondaryBindingPayloads = new QueueListenerUnbindingPayload[secondaryBindings.Count];
                foreach ( var binding in secondaryBindings )
                {
                    using ( binding.Queue.AcquireLock() )
                    {
                        if ( ! binding.Queue.IsInactive && binding.TryMarkAsDisposed() )
                        {
                            var disposingQueue = binding.Queue.TryDisposeByRemovingListenerUnsafe( channel.Id );
                            var queueTraceId = binding.Queue.GetTraceId();
                            secondaryBindingPayloads[i++] = new QueueListenerUnbindingPayload( binding, queueTraceId, disposingQueue );
                        }
                    }
                }

                if ( i < secondaryBindingPayloads.Length )
                    Array.Resize( ref secondaryBindingPayloads, i );
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
        return ExclusiveLock.Enter( _sync );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ExclusiveLock AcquireActiveLock(ulong traceId, out MessageBrokerRemoteClientDeactivatedException? exception)
    {
        var @lock = AcquireLock();
        if ( ! IsInactive )
        {
            exception = null;
            return @lock;
        }

        var disposed = IsDisposed;
        @lock.Dispose();
        exception = this.DeactivatedException( disposed );
        if ( Logger.Error is { } error )
            error.Emit( MessageBrokerRemoteClientErrorEvent.Create( this, traceId, exception ) );

        return default;
    }

    internal async ValueTask<Result<int>> WritePotentialBatchAsync(
        Protocol.PacketHeader header,
        ReadOnlyMemory<byte> data,
        short maxBatchPacketCount,
        int maxNetworkBatchPacketBytes,
        ulong traceId)
    {
        var batchPoolToken = MemoryPoolToken<byte>.Empty;
        var dataToWrite = data;
        var headerToWrite = header;
        try
        {
            var packetCount = TryPrepareBatchPacket(
                data.Length,
                ref batchPoolToken,
                ref headerToWrite,
                ref dataToWrite,
                maxBatchPacketCount,
                maxNetworkBatchPacketBytes,
                traceId );

            if ( packetCount.Exception is not null )
                return packetCount.Exception;

            var sendPacket = Logger.SendPacket;
            sendPacket?.Emit( MessageBrokerRemoteClientSendPacketEvent.CreateSending( this, traceId, headerToWrite, packetCount.Value ) );

            Stream stream;
            CancellationToken timeoutToken;
            using ( AcquireActiveLock( traceId, out var exc ) )
            {
                if ( exc is not null )
                    return exc;

                Assume.IsNotNull( _stream );
                stream = _stream;
                timeoutToken = EventScheduler.ScheduleWriteTimeout( this, _messageTimeout );
            }

            await stream.WriteAsync( dataToWrite, timeoutToken ).ConfigureAwait( false );

            sendPacket?.Emit( MessageBrokerRemoteClientSendPacketEvent.CreateSent( this, traceId, headerToWrite ) );
            if ( packetCount.Value > 1 )
                sendPacket?.Emit( MessageBrokerRemoteClientSendPacketEvent.CreateBatched( this, traceId, header, traceId ) );

            return packetCount;
        }
        catch ( Exception exc )
        {
            if ( Logger.Error is { } error )
                error.Emit( MessageBrokerRemoteClientErrorEvent.Create( this, traceId, exc ) );

            return exc;
        }
        finally
        {
            batchPoolToken.Return( this, traceId );
        }
    }

    internal MessageRouting.Result GetMessageRouting(
        ulong traceId,
        Protocol.PacketHeader header,
        int count,
        ReadOnlySpan<byte> data,
        bool clearBuffers)
    {
        Assume.IsGreaterThan( count, 0 );
        var read = 0;
        var found = 0;
        var poolToken = MemoryPoolToken<byte>.Empty;
        try
        {
            poolToken = MemoryPool.Rent( Defaults.Memory.GetBufferCapacity( (count + 7) >> 3 ), clearBuffers, out var buffer );
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
    internal void EmitError(Result result, ulong traceId)
    {
        if ( result.Exception is not null && Logger.Error is { } error )
            error.Emit( MessageBrokerRemoteClientErrorEvent.Create( this, traceId, result.Exception ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void EmitErrors(ref Chain<Exception> exceptions, ulong traceId)
    {
        if ( exceptions.Count > 0 && Logger.Error is { } error )
        {
            foreach ( var exc in exceptions )
                error.Emit( MessageBrokerRemoteClientErrorEvent.Create( this, traceId, exc ) );
        }

        exceptions = Chain<Exception>.Empty;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal MemoryPool<byte> GetMemoryPool(int length)
    {
        return length <= MemoryPool.SegmentLength ? MemoryPool : Server.MemoryPool;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ulong GetTraceId()
    {
        return unchecked( _nextTraceId++ );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool TryBeginDeactivate(out bool isEphemeral, bool forceDisposal = false)
    {
        if ( IsInactive )
        {
            isEphemeral = false;
            return false;
        }

        _state = forceDisposal || _isEphemeral ? MessageBrokerRemoteClientState.Disposing : MessageBrokerRemoteClientState.Deactivating;
        isEphemeral = _isEphemeral;
        _deactivated = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
        return true;
    }

    internal ValueTask DeactivateAsync(ulong traceId, DeactivationSource source)
    {
        bool isEphemeral;
        using ( AcquireLock() )
        {
            if ( IsInactive )
                return ValueTask.CompletedTask;

            _state = _isEphemeral ? MessageBrokerRemoteClientState.Disposing : MessageBrokerRemoteClientState.Deactivating;
            isEphemeral = _isEphemeral;
            _deactivated = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
        }

        return DeactivateAsyncCore( traceId, isEphemeral, source );
    }

    internal async ValueTask DeactivateAsyncCore(
        ulong traceId,
        bool isEphemeral,
        DeactivationSource source = DeactivationSource.Manual,
        ClientStopReason reason = ClientStopReason.Disconnect,
        bool storageLoaded = true)
    {
        TaskCompletionSource? deactivatedSource = null;
        try
        {
            var keepAlive = ! isEphemeral && reason == ClientStopReason.Disconnect;
            if ( Logger.Deactivating is { } deactivating )
                deactivating.Emit( MessageBrokerRemoteClientDeactivatingEvent.Create( this, traceId, keepAlive ) );

            bool clearBuffers;
            ServerStorage.Client storage;
            Task? eventSchedulerTask;
            Task? requestHandlerTask;
            Task? responseSenderTask;
            Task? packetListenerTask;
            Task? notificationSenderTask;
            ValueTaskDelaySource? ownedDelaySource;
            var exceptions = Chain<Exception>.Empty;

            using ( AcquireLock() )
            {
                deactivatedSource = _deactivated;
                clearBuffers = _clearBuffers;
                storage = _storage;
                if ( isEphemeral && storage.IsActive )
                    _storage = Server.Storage.CreateForClient( Id, isEphemeral: true );

                ownedDelaySource = keepAlive ? null : _delaySource.DiscardOwnedSource();
                eventSchedulerTask = EventScheduler.DiscardUnderlyingTask();
                requestHandlerTask = RequestHandler.DiscardUnderlyingTask();
                responseSenderTask = ResponseSender.DiscardUnderlyingTask();
                packetListenerTask = PacketListener.DiscardUnderlyingTask();
                notificationSenderTask = NotificationSender.DiscardUnderlyingTask();

                switch ( source )
                {
                    case DeactivationSource.EventScheduler:
                        eventSchedulerTask = null;
                        break;
                    case DeactivationSource.RequestHandler:
                        requestHandlerTask = null;
                        break;
                    case DeactivationSource.ResponseSender:
                        responseSenderTask = null;
                        break;
                    case DeactivationSource.PacketListener:
                        packetListenerTask = null;
                        break;
                    case DeactivationSource.NotificationSender:
                        notificationSenderTask = null;
                        break;
                }

                RequestHandler.Dispose( ref exceptions );
                ResponseSender.BeginDispose( ref exceptions );
                EventScheduler.Dispose( ref exceptions );
                NotificationSender.BeginDispose( ref exceptions );
                WriterQueue.Dispose( ref exceptions );
            }

            EmitErrors( ref exceptions, traceId );
            EmitError( await eventSchedulerTask.AsSafeCancellable().ConfigureAwait( false ), traceId );
            EmitError( await requestHandlerTask.AsSafeNonCancellable().ConfigureAwait( false ), traceId );
            EmitError( await responseSenderTask.AsSafeNonCancellable().ConfigureAwait( false ), traceId );
            EmitError( await packetListenerTask.AsSafeNonCancellable().ConfigureAwait( false ), traceId );
            EmitError( await notificationSenderTask.AsSafeNonCancellable().ConfigureAwait( false ), traceId );

            ReadOnlyArray<MessageBrokerChannelPublisherBinding> publishers;
            ReadOnlyArray<MessageBrokerChannelListenerBinding> listeners;
            ReadOnlyArray<MessageBrokerQueue> queues;
            using ( AcquireLock() )
            {
                RequestQueue.Dispose( ref exceptions );
                publishers = PublishersByChannelId.GetAll();
                listeners = ListenersByChannelId.GetAll();
                queues = QueueStore.GetAll();
                foreach ( var queue in queues )
                {
                    using ( queue.AcquireLock() )
                        queue.EventHeapIndex = -1;
                }
            }

            EmitErrors( ref exceptions, traceId );

            Dictionary<int, MessageBrokerChannelListenerBinding>? listenersByChannelId = null;
            if ( reason == ClientStopReason.Server )
            {
                EmitError(
                    await Parallel.ForEachAsync( queues, (q, _) => q.OnServerDisposingAsync( traceId ) ).AsSafe().ConfigureAwait( false ),
                    traceId );

                foreach ( var publisher in publishers )
                    publisher.OnServerDisposing();

                listenersByChannelId = new Dictionary<int, MessageBrokerChannelListenerBinding>( capacity: listeners.Count );
                foreach ( var listener in listeners )
                {
                    if ( listener.OnServerDisposing() )
                        listenersByChannelId[listener.Channel.Id] = listener;
                }
            }
            else
            {
                EmitError(
                    await Parallel.ForEachAsync( queues, (q, _) => q.OnClientDeactivatingAsync( keepAlive, traceId ) )
                        .AsSafe()
                        .ConfigureAwait( false ),
                    traceId );

                foreach ( var publisher in publishers )
                    publisher.OnClientDeactivating( keepAlive );

                foreach ( var listener in listeners )
                    listener.OnClientDeactivating( keepAlive );
            }

            bool disposed;
            int discardedNotificationCount;
            ListSlim<ResponseSender.DiscardedResponse> discardedResponses;
            using ( AcquireLock() )
            {
                var result = _tcp?.TryDispose() ?? Result.Valid;
                if ( result.Exception is not null )
                    exceptions = exceptions.Extend( result.Exception );

                discardedNotificationCount = NotificationSender.EndDispose( ref exceptions );
                discardedResponses = ResponseSender.EndDispose( ref exceptions );
                disposed = IsDisposed;
            }

            EmitErrors( ref exceptions, traceId );
            EndDiscardedResponses( ref discardedResponses, disposed, traceId );

            if ( discardedNotificationCount > 0 && Logger.Error is { } error )
            {
                var exc = this.Exception( Resources.NotificationsDiscarded( discardedNotificationCount ) );
                error.Emit( MessageBrokerRemoteClientErrorEvent.Create( this, traceId, exc ) );
            }

            if ( reason == ClientStopReason.Server )
            {
                Assume.IsNotNull( listenersByChannelId );

                EmitError(
                    await Parallel.ForEachAsync(
                            listeners,
                            (l, _) => l.OnServerDisposedAsync( storage, clearBuffers, storageLoaded, traceId ) )
                        .AsSafe()
                        .ConfigureAwait( false ),
                    traceId );

                EmitError(
                    await Parallel.ForEachAsync(
                            publishers,
                            (p, _) => p.OnServerDisposedAsync( storage, clearBuffers, storageLoaded, traceId ) )
                        .AsSafe()
                        .ConfigureAwait( false ),
                    traceId );

                EmitError(
                    await Parallel.ForEachAsync(
                            queues,
                            (q, _) => q.OnServerDisposedAsync(
                                isEphemeral,
                                clearBuffers,
                                listenersByChannelId,
                                storageLoaded,
                                traceId ) )
                        .AsSafe()
                        .ConfigureAwait( false ),
                    traceId );
            }
            else
            {
                EmitError(
                    await Parallel.ForEachAsync( listeners, (l, _) => l.OnClientDeactivatedAsync( storage, keepAlive, traceId ) )
                        .AsSafe()
                        .ConfigureAwait( false ),
                    traceId );

                EmitError(
                    await Parallel.ForEachAsync( publishers, (p, _) => p.OnClientDeactivatedAsync( storage, keepAlive, traceId ) )
                        .AsSafe()
                        .ConfigureAwait( false ),
                    traceId );

                EmitError(
                    await Parallel.ForEachAsync( queues, (q, _) => q.OnClientDeactivatedAsync( keepAlive, traceId ) )
                        .AsSafe()
                        .ConfigureAwait( false ),
                    traceId );
            }

            using ( AcquireLock() )
            {
                var exc = MessageRouting.PoolToken.Return();
                if ( exc is not null )
                    exceptions = exceptions.Extend( exc );

                MessageRouting = MessageRouting.Empty;
                ExternalNameCache.Clear();
            }

            EmitErrors( ref exceptions, traceId );

            if ( keepAlive )
            {
                using ( AcquireLock() )
                {
                    _state = MessageBrokerRemoteClientState.Inactive;
                    _deactivated = null;
                    _stream = null;
                    _tcp = null;
                }
            }
            else
            {
                if ( reason == ClientStopReason.Server )
                {
                    if ( isEphemeral )
                        EmitError( await storage.DeleteAsync( this ).AsSafe().ConfigureAwait( false ), traceId );
                    else
                        EmitError( await storage.SaveMetadataAsync( this, traceId ).AsSafe().ConfigureAwait( false ), traceId );
                }
                else
                {
                    EmitError( await storage.DeleteAsync( this ).AsSafe().ConfigureAwait( false ), traceId );
                    EmitError( RemoteClientCollection.Remove( this ), traceId );
                }

                using ( AcquireLock() )
                {
                    PublishersByChannelId.Clear();
                    ListenersByChannelId.Clear();
                    QueueStore.Clear();
                    _state = MessageBrokerRemoteClientState.Disposed;
                    _deactivated = null;
                    _stream = null;
                    _tcp = null;
                }
            }

            if ( ownedDelaySource is not null )
                EmitError( await ownedDelaySource.TryDisposeAsync().ConfigureAwait( false ), traceId );

            if ( Logger.Deactivated is { } deactivated )
                deactivated.Emit( MessageBrokerRemoteClientDeactivatedEvent.Create( this, traceId, keepAlive ) );
        }
        finally
        {
            deactivatedSource?.TrySetResult();
        }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool GetClearBuffersOption()
    {
        return _clearBuffers;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool GetSynchronizeExternalObjectNamesOption()
    {
        return _synchronizeExternalObjectNames;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal Duration GetMaxReadTimeoutOption()
    {
        return _maxReadTimeout;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal short GetMaxBatchPacketCountOption()
    {
        return _maxBatchPacketCount;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal int GetMaxNetworkBatchPacketBytesOption()
    {
        return _maxNetworkBatchPacketBytes;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ServerStorage.Client GetStorage()
    {
        return _storage;
    }

    internal enum DeactivationSource : byte
    {
        Manual = 0,
        EventScheduler = 1,
        RequestHandler = 2,
        ResponseSender = 3,
        PacketListener = 4,
        NotificationSender = 5
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void EndDiscardedResponses(ref ListSlim<ResponseSender.DiscardedResponse> discardedResponses, bool disposed, ulong traceId)
    {
        var error = Logger.Error;
        var traceEnd = Logger.TraceEnd;
        foreach ( ref readonly var r in discardedResponses )
        {
            r.PoolToken.Return( this, traceId );
            if ( error is not null )
            {
                var exc = this.DeactivatedException( disposed );
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
        short maxBatchPacketCount,
        int maxNetworkBatchPacketBytes,
        ulong traceId)
    {
        var packetCount = 1;
        if ( maxBatchPacketCount == 0 )
            return packetCount;

        var batchLength = unchecked( ( long )Protocol.PacketHeader.Length + Protocol.BatchHeader.Length + packetLength );
        if ( batchLength >= maxNetworkBatchPacketBytes )
            return packetCount;

        bool clearBuffers;
        using ( AcquireActiveLock( traceId, out var exc ) )
        {
            if ( exc is not null )
                return exc;

            clearBuffers = _clearBuffers;
            packetCount = WriterQueue.GetLargestAvailableBatchCount( this, ref batchLength, ref clearBuffers );
        }

        if ( packetCount == 1 )
            return packetCount;

        var memoryPool = GetMemoryPool( unchecked( ( int )batchLength ) );
        batchPoolToken = memoryPool.Rent( unchecked( ( int )batchLength ), clearBuffers, out var batchData );
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

    private void SetProperties(
        Duration messageTimeout,
        Duration pingInterval,
        short maxBatchPacketCount,
        MemorySize maxNetworkBatchPacketLength,
        bool isLittleEndian,
        bool synchronizeExternalObjectNames,
        bool clearBuffers,
        bool isEphemeral)
    {
        _isLittleEndian = isLittleEndian;
        _messageTimeout = Server.AcceptableMessageTimeout.Clamp( messageTimeout );
        _pingInterval = Server.AcceptablePingInterval.Clamp( pingInterval );
        _maxReadTimeout = _messageTimeout + _pingInterval;
        _maxBatchPacketCount = Server.AcceptableMaxBatchPacketCount.Clamp( maxBatchPacketCount );
        if ( _maxBatchPacketCount == 1 )
            _maxBatchPacketCount = 0;

        _maxNetworkBatchPacketBytes = _maxBatchPacketCount > 0
            ? unchecked( ( int )Server.AcceptableMaxNetworkBatchPacketLength
                .Clamp( maxNetworkBatchPacketLength )
                .Bytes )
            : 0;

        _synchronizeExternalObjectNames = synchronizeExternalObjectNames;
        _clearBuffers = clearBuffers;
        _isEphemeral = isEphemeral;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Exception HandleUnexpectedEndpoint(Protocol.PacketHeader header, ulong traceId)
    {
        var exc = this.ProtocolException( header, Resources.UnexpectedServerEndpoint );
        if ( Logger.Error is { } error )
            error.Emit( MessageBrokerRemoteClientErrorEvent.Create( this, traceId, exc ) );

        return exc;
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
            return token.SetObject( ref QueueStore, MessageBrokerQueue.Create( this, _storage.CreateForQueue(), token.Id, name ) );
        }
        catch
        {
            token.Revert( ref QueueStore, name );
            throw;
        }
    }
}
