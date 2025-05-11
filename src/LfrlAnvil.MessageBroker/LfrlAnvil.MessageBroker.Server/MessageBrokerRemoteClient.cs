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
    internal EventScheduler EventScheduler;
    internal MessageListener MessageListener;
    internal MessageNotifications MessageNotifications;
    internal RequestHandler RequestHandler;
    internal MessageContextQueue MessageContextQueue;

    private readonly ITimestampProvider _timestamps;
    private readonly TcpClient _tcp;
    private readonly MessageBrokerRemoteClientEventHandler? _eventHandler;
    private DelaySource _delaySource;
    private Stream _stream;
    private MessageBrokerRemoteClientState _state;

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

        PublishersByChannelId = ReferenceStore<int, MessageBrokerChannelPublisherBinding>.Create();
        ListenersByChannelId = ReferenceStore<int, MessageBrokerChannelListenerBinding>.Create();
        QueuesByName = ObjectStore<MessageBrokerQueue>.Create( StringComparer.OrdinalIgnoreCase );
        EventScheduler = EventScheduler.Create();
        MessageListener = MessageListener.Create();
        MessageNotifications = MessageNotifications.Create();
        RequestHandler = RequestHandler.Create();
        MessageContextQueue = MessageContextQueue.Create();

        _eventHandler = Server.RemoteClientEventHandlerFactory?.Invoke( this );
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
        using ( AcquireLock() )
        {
            if ( ShouldCancel )
                return;

            _state = MessageBrokerRemoteClientState.Disposing;
        }

        var (publishers, listeners, queues) = await DisposeAsync( extractAllChildren: true ).ConfigureAwait( false );
        await Parallel.ForEachAsync( queues, static (q, _) => q.OnClientDisconnectedAsync() ).ConfigureAwait( false );
        await Parallel.ForEachAsync( publishers, static (p, _) => p.OnClientDisconnectedAsync() ).ConfigureAwait( false );
        foreach ( var listener in listeners )
            listener.OnClientDisconnected();

        using ( AcquireLock() )
            _state = MessageBrokerRemoteClientState.Disposed;

        var exception = RemoteClientCollection.Remove( this ).Exception;
        if ( exception is not null )
            Emit( MessageBrokerRemoteClientEvent.Unexpected( this, exception ) );

        Emit( MessageBrokerRemoteClientEvent.Disposed( this ) );
    }

    internal async ValueTask OnServerDisposedAsync()
    {
        using ( AcquireLock() )
        {
            if ( ShouldCancel )
                return;

            _state = MessageBrokerRemoteClientState.Disposing;
        }

        var (_, _, queues) = await DisposeAsync( extractAllChildren: false ).ConfigureAwait( false );
        await Parallel.ForEachAsync( queues, static (q, _) => q.OnServerDisposedAsync() ).ConfigureAwait( false );

        using ( AcquireLock() )
            _state = MessageBrokerRemoteClientState.Disposed;

        Emit( MessageBrokerRemoteClientEvent.Disposed( this ) );
    }

    internal void Start()
    {
        Emit( MessageBrokerRemoteClientEvent.Created( this ) );

        try
        {
            using ( AcquireLock() )
            {
                if ( ShouldCancel )
                    return;

                EventScheduler.InitializeResetEvent( _delaySource.GetSource() );
            }

            var eventSchedulerTask = EventScheduler.StartUnderlyingTask( this );
            using ( AcquireLock() )
            {
                if ( ShouldCancel )
                    return;

                EventScheduler.SetUnderlyingTask( eventSchedulerTask );
            }

            var task = StartHandshakeTask();
            using ( AcquireLock() )
            {
                if ( _state <= MessageBrokerRemoteClientState.Handshaking )
                    MessageListener.SetUnderlyingTask( task );
            }
        }
        catch ( Exception exc )
        {
            Emit( MessageBrokerRemoteClientEvent.Unexpected( this, exc ) );
            DisconnectAsync().AsTask().Wait();
        }
    }

    internal Protocol.BindPublisherFailureResponse.Reasons BindPublisherUnsafe(
        MessageBrokerChannel channel,
        bool channelCreated,
        string streamName,
        ref MessageBrokerChannelPublisherBinding? publisher,
        ref MessageBrokerStream? stream,
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
        ref MessageBrokerStream? stream,
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
        ref MessageBrokerQueue? queue,
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
        ref MessageBrokerQueue? queue,
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
    internal void Emit(MessageBrokerRemoteClientEvent e)
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
    internal Result HandleUnexpectedEndpoint(Protocol.PacketHeader header)
    {
        return EmitError(
            MessageBrokerRemoteClientEvent.MessageRejected(
                this,
                header,
                Protocol.UnexpectedServerEndpointException( this, header ) ) );
    }

    internal async ValueTask<Result> WriteAsync(
        Protocol.PacketHeader header,
        ReadOnlyMemory<byte> data,
        ulong contextId = MessageBrokerRemoteClientEvent.RootContextId)
    {
        Emit( MessageBrokerRemoteClientEvent.SendingMessage( this, header, contextId ) );

        try
        {
            bool cancel;
            CancellationToken timeoutToken = default;
            using ( AcquireLock() )
            {
                cancel = ShouldCancel;
                if ( ! cancel )
                    timeoutToken = EventScheduler.ScheduleWriteTimeout( this );
            }

            if ( cancel )
                return EmitError( MessageBrokerRemoteClientEvent.SendingMessage( this, header, contextId, DisposedException() ) );

            await _stream.WriteAsync( data, timeoutToken ).ConfigureAwait( false );
        }
        catch ( Exception exc )
        {
            return EmitError( MessageBrokerRemoteClientEvent.SendingMessage( this, header, contextId, exc ) );
        }

        Emit( MessageBrokerRemoteClientEvent.MessageSent( this, header, contextId ) );
        return Result.Valid;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Result EmitError(MessageBrokerRemoteClientEvent e)
    {
        Assume.IsNotNull( e.Exception );
        Emit( e );
        return e.Exception;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Result<T> EmitError<T>(MessageBrokerRemoteClientEvent e, T? value = default)
    {
        Assume.IsNotNull( e.Exception );
        Emit( e );
        return Result.Error( e.Exception, value );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private MessageBrokerRemoteClientDisposedException DisposedException()
    {
        return new MessageBrokerRemoteClientDisposedException( this );
    }

    private async ValueTask<(MessageBrokerChannelPublisherBinding[] Publishers, MessageBrokerChannelListenerBinding[] Listeners,
        MessageBrokerQueue[]
        Queues)> DisposeAsync(bool extractAllChildren)
    {
        Emit( MessageBrokerRemoteClientEvent.Disposing( this ) );

        Task? eventSchedulerTask;
        Task? requestHandlerTask;
        Task? messageListenerTask;
        Task? messageNotificationsTask;
        ValueTaskDelaySource? ownedDelaySource;

        var publishers = Array.Empty<MessageBrokerChannelPublisherBinding>();
        var listeners = Array.Empty<MessageBrokerChannelListenerBinding>();
        MessageBrokerQueue[] queues;
        using ( AcquireLock() )
        {
            ownedDelaySource = _delaySource.DiscardOwnedSource();
            if ( extractAllChildren )
            {
                publishers = PublishersByChannelId.ClearAndExtract();
                listeners = ListenersByChannelId.ClearAndExtract();
            }
            else
            {
                PublishersByChannelId.Clear();
                ListenersByChannelId.Clear();
            }

            queues = QueuesByName.Clear();

            eventSchedulerTask = EventScheduler.DiscardUnderlyingTask();
            requestHandlerTask = RequestHandler.DiscardUnderlyingTask();
            messageListenerTask = MessageListener.DiscardUnderlyingTask();
            messageNotificationsTask = MessageNotifications.DiscardUnderlyingTask();
            MessageContextQueue.Dispose();
            RequestHandler.Dispose();
            EventScheduler.Dispose();
            MessageNotifications.Dispose();
        }

        if ( eventSchedulerTask is not null )
            await eventSchedulerTask.ConfigureAwait( false );

        if ( requestHandlerTask is not null )
            await requestHandlerTask.ConfigureAwait( false );

        if ( messageListenerTask is not null )
            await messageListenerTask.ConfigureAwait( false );

        if ( messageNotificationsTask is not null )
            await messageNotificationsTask.ConfigureAwait( false );

        Exception? exception;
        using ( AcquireLock() )
            exception = _tcp.TryDispose().Exception;

        if ( exception is not null )
            Emit( MessageBrokerRemoteClientEvent.Unexpected( this, exception ) );

        if ( ownedDelaySource is not null )
            await ownedDelaySource.TryDisposeAsync().ConfigureAwait( false );

        return (publishers, listeners, queues);
    }
}
