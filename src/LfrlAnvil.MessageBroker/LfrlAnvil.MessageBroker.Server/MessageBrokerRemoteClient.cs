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
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Async;
using LfrlAnvil.Chrono;
using LfrlAnvil.Chrono.Async;
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
    internal ObjectStore<MessageBrokerQueue> QueuesByName;
    internal WriterQueue WriterQueue;
    internal RequestQueue RequestQueue;
    internal EventScheduler EventScheduler;
    internal PacketListener PacketListener;
    internal MessageNotifications MessageNotifications;
    internal RequestHandler RequestHandler;
    internal readonly MessageBrokerRemoteClientLogger Logger;

    private readonly ITimestampProvider _timestamps;
    private readonly TcpClient _tcp;
    private DelaySource _delaySource;
    private Stream _stream;
    private MessageBrokerRemoteClientState _state;
    private ulong _nextTraceId;

    internal MessageBrokerRemoteClient(int id, MessageBrokerServer server, TcpClient tcp, int minMemoryPoolSegmentLength)
    {
        _tcp = tcp;
        _stream = _tcp.GetStream();
        MemoryPool = new MemoryPool<byte>( minMemoryPoolSegmentLength );
        Server = server;
        Id = id;
        Name = string.Empty;
        IsLittleEndian = false;
        MessageTimeout = server.HandshakeTimeout;
        MaxReadTimeout = MessageTimeout;
        PingInterval = Duration.Zero;
        _state = MessageBrokerRemoteClientState.Created;
        _nextTraceId = 0;

        PublishersByChannelId = ReferenceStore<int, MessageBrokerChannelPublisherBinding>.Create();
        ListenersByChannelId = ReferenceStore<int, MessageBrokerChannelListenerBinding>.Create();
        QueuesByName = ObjectStore<MessageBrokerQueue>.Create( StringComparer.OrdinalIgnoreCase );
        WriterQueue = WriterQueue.Create();
        RequestQueue = RequestQueue.Create();
        EventScheduler = EventScheduler.Create();
        PacketListener = PacketListener.Create();
        MessageNotifications = MessageNotifications.Create();
        RequestHandler = RequestHandler.Create();

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
        ulong traceId;
        using ( AcquireLock() )
        {
            if ( ! TryBeginDispose() )
                return;

            traceId = GetTraceId();
        }

        using ( MessageBrokerRemoteClientTraceEvent.CreateScope( this, traceId, MessageBrokerRemoteClientTraceEventType.Dispose ) )
            await DisposeAsyncCore( traceId ).ConfigureAwait( false );
    }

    internal async ValueTask OnServerDisposedAsync(ulong serverTraceId)
    {
        ulong traceId;
        using ( AcquireLock() )
        {
            if ( ! TryBeginDispose() )
                return;

            traceId = GetTraceId();
        }

        using ( MessageBrokerRemoteClientTraceEvent.CreateScope( this, traceId, MessageBrokerRemoteClientTraceEventType.Dispose ) )
        {
            MessageBrokerRemoteClientServerTraceEvent.Create( this, traceId, serverTraceId ).Emit( Logger.ServerTrace );
            await DisposeAsyncCore( traceId, serverDisposed: true ).ConfigureAwait( false );
        }
    }

    internal async ValueTask StartAsync(ulong serverTraceId)
    {
        ulong traceId;
        using ( AcquireLock() )
            traceId = GetTraceId();

        var failed = true;
        MessageBrokerRemoteClientTraceEvent.Create( this, traceId, MessageBrokerRemoteClientTraceEventType.Start )
            .Emit( Logger.TraceStart );

        MessageBrokerRemoteClientServerTraceEvent.Create( this, traceId, serverTraceId ).Emit( Logger.ServerTrace );
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
            MessageBrokerRemoteClientErrorEvent.Create( this, traceId, exc ).Emit( Logger.Error );
            await DisposeAsync( traceId ).ConfigureAwait( false );
        }
        finally
        {
            if ( failed )
                MessageBrokerRemoteClientTraceEvent.Create( this, traceId, MessageBrokerRemoteClientTraceEventType.Start )
                    .Emit( Logger.TraceEnd );
        }
    }

    internal Protocol.BindPublisherFailureResponse.Reasons BindPublisherUnsafe(
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
                return Protocol.BindPublisherFailureResponse.Reasons.Cancelled;

            var token = channel.PublishersByClientId.GetOrAddNull( Id );
            if ( token.Exists )
            {
                publisher = token.GetObject();
                stream = publisher.Stream;
                return Protocol.BindPublisherFailureResponse.Reasons.AlreadyBound;
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

                        return Protocol.BindPublisherFailureResponse.Reasons.Cancelled;
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

        return Protocol.BindPublisherFailureResponse.Reasons.None;
    }

    internal Protocol.UnbindPublisherFailureResponse.Reasons BeginUnbindPublisherUnsafe(
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
            if ( channel.ShouldCancel || ! PublishersByChannelId.TryGet( channel.Id, out publisher ) )
                return Protocol.UnbindPublisherFailureResponse.Reasons.NotBound;

            stream = publisher.Stream;
            using ( stream.AcquireLock() )
            {
                if ( stream.ShouldCancel )
                    return Protocol.UnbindPublisherFailureResponse.Reasons.NotBound;

                using ( publisher.AcquireLock() )
                {
                    if ( publisher.ShouldCancel )
                        return Protocol.UnbindPublisherFailureResponse.Reasons.NotBound;

                    publisher.BeginDisposingUnsafe();
                    PublishersByChannelId.Remove( channel.Id );
                    disposingChannel = channel.TryDisposeByRemovingPublisherUnsafe( Id );
                    disposingStream = stream.TryDisposeByRemovingPublisherUnsafe( Id, channel.Id );
                    channelTraceId = channel.GetTraceId();
                    streamTraceId = stream.GetTraceId();
                }
            }
        }

        return Protocol.UnbindPublisherFailureResponse.Reasons.None;
    }

    internal Protocol.BindListenerFailureResponse.Reasons BindListenerUnsafe(
        MessageBrokerChannel channel,
        bool channelCreated,
        string queueName,
        int prefetchHint,
        ref MessageBrokerChannelListenerBinding? listener,
        ref ulong channelTraceId,
        ref MessageBrokerQueue? queue,
        ref ulong queueTraceId,
        ref bool queueCreated)
    {
        using ( channel.AcquireLock() )
        {
            if ( channel.ShouldCancel )
                return Protocol.BindListenerFailureResponse.Reasons.Cancelled;

            var token = channel.ListenersByClientId.GetOrAddNull( Id );
            if ( token.Exists )
            {
                listener = token.GetObject();
                queue = listener.Queue;
                return Protocol.BindListenerFailureResponse.Reasons.AlreadyBound;
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

                        return Protocol.BindListenerFailureResponse.Reasons.Cancelled;
                    }

                    try
                    {
                        listener = token.SetObject(
                            ref channel.ListenersByClientId,
                            new MessageBrokerChannelListenerBinding( this, channel, queue, prefetchHint ) );
                    }
                    catch
                    {
                        if ( queueCreated )
                            QueuesByName.Remove( queue.Id, queue.Name );

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

        return Protocol.BindListenerFailureResponse.Reasons.None;
    }

    private MessageBrokerQueue RegisterQueue(string name, out bool created)
    {
        var token = QueuesByName.GetOrAddNull( name );
        if ( token.Exists )
        {
            created = false;
            return token.GetObject();
        }

        try
        {
            created = true;
            return token.SetObject( ref QueuesByName, new MessageBrokerQueue( this, token.Id, name ) );
        }
        catch
        {
            token.Revert( ref QueuesByName, name );
            throw;
        }
    }

    internal Protocol.UnbindListenerFailureResponse.Reasons BeginUnbindListenerUnsafe(
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
            if ( channel.ShouldCancel || ! ListenersByChannelId.TryGet( channel.Id, out listener ) )
                return Protocol.UnbindListenerFailureResponse.Reasons.NotBound;

            queue = listener.Queue;
            using ( queue.AcquireLock() )
            {
                if ( queue.ShouldCancel )
                    return Protocol.UnbindListenerFailureResponse.Reasons.NotBound;

                using ( listener.AcquireLock() )
                {
                    if ( listener.ShouldCancel )
                        return Protocol.UnbindListenerFailureResponse.Reasons.NotBound;

                    listener.BeginDisposingUnsafe();
                    ListenersByChannelId.Remove( channel.Id );
                    disposingChannel = channel.TryDisposeByRemovingListenerUnsafe( Id );
                    disposingQueue = queue.TryDisposeByRemovingListenerUnsafe( channel.Id );
                    channelTraceId = channel.GetTraceId();
                    queueTraceId = queue.GetTraceId();
                }
            }
        }

        return Protocol.UnbindListenerFailureResponse.Reasons.None;
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
        exception = DisposedException();
        MessageBrokerRemoteClientErrorEvent.Create( this, traceId, exception ).Emit( Logger.Error );
        return default;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal Exception HandleUnexpectedEndpoint(Protocol.PacketHeader header, ulong traceId)
    {
        return EmitError( Protocol.UnexpectedServerEndpointException( this, header ), traceId );
    }

    internal async ValueTask<Result> WriteAsync(Protocol.PacketHeader header, ReadOnlyMemory<byte> data, ulong traceId)
    {
        MessageBrokerRemoteClientSendPacketEvent.CreateSending( this, traceId, header ).Emit( Logger.SendPacket );
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

        MessageBrokerRemoteClientSendPacketEvent.CreateSent( this, traceId, header ).Emit( Logger.SendPacket );
        return Result.Valid;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal MessageBrokerRemoteClientDisposedException DisposedException()
    {
        return new MessageBrokerRemoteClientDisposedException( this );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal Exception EmitError(Exception exception, ulong traceId)
    {
        MessageBrokerRemoteClientErrorEvent.Create( this, traceId, exception ).Emit( Logger.Error );
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
        MessageBrokerRemoteClientDisposingEvent.Create( this, traceId ).Emit( Logger.Disposing );

        Task? eventSchedulerTask;
        Task? requestHandlerTask;
        Task? packetListenerTask;
        Task? messageNotificationsTask;
        ValueTaskDelaySource? ownedDelaySource;

        Chain<Exception> exceptions;
        using ( AcquireLock() )
        {
            ownedDelaySource = _delaySource.DiscardOwnedSource();
            eventSchedulerTask = EventScheduler.DiscardUnderlyingTask();
            requestHandlerTask = RequestHandler.DiscardUnderlyingTask();
            packetListenerTask = PacketListener.DiscardUnderlyingTask();
            messageNotificationsTask = MessageNotifications.DiscardUnderlyingTask();
            RequestHandler.Dispose();
            EventScheduler.Dispose();
            MessageNotifications.BeginDispose();
            WriterQueue.Dispose();
            exceptions = RequestQueue.Dispose();
        }

        foreach ( var exc in exceptions )
            MessageBrokerRemoteClientErrorEvent.Create( this, traceId, exc ).Emit( Logger.Error );

        if ( eventSchedulerTask is not null )
            await eventSchedulerTask.ConfigureAwait( false );

        if ( requestHandlerTask is not null )
            await requestHandlerTask.ConfigureAwait( false );

        if ( packetListenerTask is not null )
            await packetListenerTask.ConfigureAwait( false );

        if ( messageNotificationsTask is not null )
            await messageNotificationsTask.ConfigureAwait( false );

        MessageBrokerChannelPublisherBinding[] publishers;
        MessageBrokerChannelListenerBinding[] listeners;
        MessageBrokerQueue[] queues;
        Exception? exception;
        using ( AcquireLock() )
        {
            publishers = PublishersByChannelId.ClearAndExtract();
            listeners = ListenersByChannelId.ClearAndExtract();
            queues = QueuesByName.Clear();
            exception = _tcp.TryDispose().Exception;
        }

        if ( exception is not null )
            MessageBrokerRemoteClientErrorEvent.Create( this, traceId, exception ).Emit( Logger.Error );

        if ( serverDisposed )
        {
            await Parallel.ForEachAsync( queues, (q, _) => q.OnClientDisconnectedAsync( traceId ) ).ConfigureAwait( false );
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

        int discardedMessageCount;
        using ( AcquireLock() )
            (discardedMessageCount, exceptions) = MessageNotifications.EndDispose();

        foreach ( var exc in exceptions )
            MessageBrokerRemoteClientErrorEvent.Create( this, traceId, exc ).Emit( Logger.Error );

        if ( discardedMessageCount > 0 )
        {
            var error = new MessageBrokerRemoteClientMessageException( this, null, Resources.MessagesDiscarded( discardedMessageCount ) );
            MessageBrokerRemoteClientErrorEvent.Create( this, traceId, error ).Emit( Logger.Error );
        }

        using ( AcquireLock() )
            _state = MessageBrokerRemoteClientState.Disposed;

        if ( ownedDelaySource is not null )
            await ownedDelaySource.TryDisposeAsync().ConfigureAwait( false );

        if ( ! serverDisposed )
        {
            var removeException = RemoteClientCollection.Remove( this ).Exception;
            if ( removeException is not null )
                MessageBrokerRemoteClientErrorEvent.Create( this, traceId, removeException ).Emit( Logger.Error );
        }

        MessageBrokerRemoteClientDisposedEvent.Create( this, traceId ).Emit( Logger.Disposed );
    }
}
