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
    internal ReferenceStore<int, MessageBrokerChannelBinding> BindingsByChannelId;
    internal ReferenceStore<int, MessageBrokerSubscription> SubscriptionsByChannelId;
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

        BindingsByChannelId = ReferenceStore<int, MessageBrokerChannelBinding>.Create();
        SubscriptionsByChannelId = ReferenceStore<int, MessageBrokerSubscription>.Create();
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
    /// Collection of <see cref="MessageBrokerChannelBinding"/> instances attached to this client, identified by channel ids.
    /// </summary>
    public MessageBrokerRemoteClientBindingCollection Bindings => new MessageBrokerRemoteClientBindingCollection( this );

    /// <summary>
    /// Collection of <see cref="MessageBrokerSubscription"/> instances attached to this client, identified by channel ids.
    /// </summary>
    public MessageBrokerRemoteClientSubscriptionCollection Subscriptions => new MessageBrokerRemoteClientSubscriptionCollection( this );

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

        var (bindings, subscriptions, queues) = await DisposeAsync( extractAllChildren: true ).ConfigureAwait( false );
        await Parallel.ForEachAsync( queues, static (b, _) => b.OnClientDisconnectedAsync() ).ConfigureAwait( false );
        await Parallel.ForEachAsync( bindings, static (b, _) => b.OnClientDisconnectedAsync() ).ConfigureAwait( false );
        foreach ( var subscription in subscriptions )
            subscription.OnClientDisconnected();

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
        await Parallel.ForEachAsync( queues, static (b, _) => b.OnServerDisposedAsync() ).ConfigureAwait( false );

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

    internal Protocol.BindFailureResponse.Reasons BindUnsafe(
        MessageBrokerChannel channel,
        bool channelCreated,
        string streamName,
        ref MessageBrokerChannelBinding? binding,
        ref MessageBrokerStream? stream,
        ref bool streamCreated)
    {
        using ( channel.AcquireLock() )
        {
            if ( channel.ShouldCancel )
                return Protocol.BindFailureResponse.Reasons.Cancelled;

            var token = channel.BindingsByClientId.GetOrAddNull( Id );
            if ( token.Exists )
            {
                binding = token.GetObject();
                stream = binding.Stream;
                return Protocol.BindFailureResponse.Reasons.AlreadyBound;
            }

            try
            {
                stream = StreamCollection.RegisterUnsafe( Server, streamName, out streamCreated );
                using ( stream.AcquireLock() )
                {
                    if ( stream.ShouldCancel )
                    {
                        token.Revert( ref channel.BindingsByClientId, Id );
                        if ( channelCreated )
                            ChannelCollection.RemoveUnsafe( channel );

                        return Protocol.BindFailureResponse.Reasons.Cancelled;
                    }

                    try
                    {
                        binding = token.SetObject(
                            ref channel.BindingsByClientId,
                            new MessageBrokerChannelBinding( this, channel, stream ) );
                    }
                    catch
                    {
                        if ( streamCreated )
                            StreamCollection.RemoveUnsafe( stream );

                        throw;
                    }

                    BindingsByChannelId.Add( channel.Id, binding );
                    stream.BindingsByClientChannelIdPair.Add( new Pair<int, int>( Id, channel.Id ), binding );
                }
            }
            catch
            {
                token.Revert( ref channel.BindingsByClientId, Id );
                throw;
            }
        }

        return Protocol.BindFailureResponse.Reasons.None;
    }

    internal Protocol.UnbindFailureResponse.Reasons BeginUnbindUnsafe(
        MessageBrokerChannel channel,
        ref MessageBrokerChannelBinding? binding,
        ref MessageBrokerStream? stream,
        ref bool disposingChannel,
        ref bool disposingStream)
    {
        using ( channel.AcquireLock() )
        {
            if ( channel.ShouldCancel || ! BindingsByChannelId.TryGet( channel.Id, out binding ) )
                return Protocol.UnbindFailureResponse.Reasons.NotBound;

            stream = binding.Stream;
            using ( stream.AcquireLock() )
            {
                if ( stream.ShouldCancel )
                    return Protocol.UnbindFailureResponse.Reasons.NotBound;

                using ( binding.AcquireLock() )
                {
                    if ( binding.ShouldCancel )
                        return Protocol.UnbindFailureResponse.Reasons.NotBound;

                    binding.BeginDisposingUnsafe();
                    BindingsByChannelId.Remove( channel.Id );
                    disposingChannel = channel.TryDisposeByRemovingBindingUnsafe( Id );
                    disposingStream = stream.TryDisposeByRemovingBindingUnsafe( Id, channel.Id );
                }
            }
        }

        return Protocol.UnbindFailureResponse.Reasons.None;
    }

    internal Protocol.SubscribeFailureResponse.Reasons SubscribeUnsafe(
        MessageBrokerChannel channel,
        bool channelCreated,
        string queueName,
        int prefetchHint,
        ref MessageBrokerSubscription? subscription,
        ref MessageBrokerQueue? queue,
        ref bool queueCreated)
    {
        using ( channel.AcquireLock() )
        {
            if ( channel.ShouldCancel )
                return Protocol.SubscribeFailureResponse.Reasons.Cancelled;

            var token = channel.SubscriptionsByClientId.GetOrAddNull( Id );
            if ( token.Exists )
            {
                subscription = token.GetObject();
                queue = subscription.Queue;
                return Protocol.SubscribeFailureResponse.Reasons.AlreadySubscribed;
            }

            try
            {
                queue = RegisterQueue( queueName, out queueCreated );
                using ( queue.AcquireLock() )
                {
                    if ( queue.ShouldCancel )
                    {
                        token.Revert( ref channel.SubscriptionsByClientId, Id );
                        if ( channelCreated )
                            ChannelCollection.RemoveUnsafe( channel );

                        return Protocol.SubscribeFailureResponse.Reasons.Cancelled;
                    }

                    try
                    {
                        subscription = token.SetObject(
                            ref channel.SubscriptionsByClientId,
                            new MessageBrokerSubscription( this, channel, queue, prefetchHint ) );
                    }
                    catch
                    {
                        if ( queueCreated )
                            QueuesByName.Remove( queue.Id, queue.Name );

                        throw;
                    }

                    SubscriptionsByChannelId.Add( channel.Id, subscription );
                    queue.SubscriptionsByChannelId.Add( channel.Id, subscription );
                }
            }
            catch
            {
                token.Revert( ref channel.SubscriptionsByClientId, Id );
                throw;
            }
        }

        return Protocol.SubscribeFailureResponse.Reasons.None;
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

    internal Protocol.UnsubscribeFailureResponse.Reasons BeginUnsubscribeUnsafe(
        MessageBrokerChannel channel,
        ref MessageBrokerSubscription? subscription,
        ref MessageBrokerQueue? queue,
        ref bool disposingChannel,
        ref bool disposingQueue)
    {
        using ( channel.AcquireLock() )
        {
            if ( channel.ShouldCancel || ! SubscriptionsByChannelId.TryGet( channel.Id, out subscription ) )
                return Protocol.UnsubscribeFailureResponse.Reasons.NotSubscribed;

            queue = subscription.Queue;
            using ( queue.AcquireLock() )
            {
                if ( queue.ShouldCancel )
                    return Protocol.UnsubscribeFailureResponse.Reasons.NotSubscribed;

                using ( subscription.AcquireLock() )
                {
                    if ( subscription.ShouldCancel )
                        return Protocol.UnsubscribeFailureResponse.Reasons.NotSubscribed;

                    subscription.BeginDisposingUnsafe();
                    SubscriptionsByChannelId.Remove( channel.Id );
                    disposingChannel = channel.TryDisposeByRemovingSubscriptionUnsafe( Id );
                    disposingQueue = queue.TryDisposeByRemovingSubscriptionUnsafe( channel.Id );
                }
            }
        }

        return Protocol.UnsubscribeFailureResponse.Reasons.None;
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

    private async ValueTask<(MessageBrokerChannelBinding[] Bindings, MessageBrokerSubscription[] Subscriptions, MessageBrokerQueue[] Queues
        )> DisposeAsync(bool extractAllChildren)
    {
        Emit( MessageBrokerRemoteClientEvent.Disposing( this ) );

        Task? eventSchedulerTask;
        Task? requestHandlerTask;
        Task? messageListenerTask;
        Task? messageNotificationsTask;
        ValueTaskDelaySource? ownedDelaySource;

        var bindings = Array.Empty<MessageBrokerChannelBinding>();
        var subscriptions = Array.Empty<MessageBrokerSubscription>();
        MessageBrokerQueue[] queues;
        using ( AcquireLock() )
        {
            ownedDelaySource = _delaySource.DiscardOwnedSource();
            if ( extractAllChildren )
            {
                bindings = BindingsByChannelId.ClearAndExtract();
                subscriptions = SubscriptionsByChannelId.ClearAndExtract();
            }
            else
            {
                BindingsByChannelId.Clear();
                SubscriptionsByChannelId.Clear();
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

        return (bindings, subscriptions, queues);
    }
}
