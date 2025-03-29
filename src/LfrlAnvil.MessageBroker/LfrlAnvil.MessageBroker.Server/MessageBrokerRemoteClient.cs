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
using LfrlAnvil.Extensions;
using LfrlAnvil.Memory;
using LfrlAnvil.MessageBroker.Server.Buffering;
using LfrlAnvil.MessageBroker.Server.Events;
using LfrlAnvil.MessageBroker.Server.Exceptions;
using LfrlAnvil.MessageBroker.Server.Internal;

namespace LfrlAnvil.MessageBroker.Server;

/// <summary>
/// Represents a remote message broker client.
/// </summary>
public sealed partial class MessageBrokerRemoteClient
{
    internal readonly Dictionary<int, MessageBrokerChannelBinding> BindingsByChannelId;
    internal readonly Dictionary<int, MessageBrokerSubscription> SubscriptionsByChannelId;
    internal EventScheduler EventScheduler;
    internal MessageListener MessageListener;
    internal RequestHandler RequestHandler;
    internal MessageContextQueue MessageContextQueue;

    private readonly ITimestampProvider _timestamps;
    private readonly TcpClient _tcp;
    private readonly MemoryPool<byte> _memoryPool;
    private readonly MessageBrokerRemoteClientEventHandler? _eventHandler;
    private DelaySource _delaySource;
    private Stream _stream;
    private MessageBrokerRemoteClientState _state;

    internal MessageBrokerRemoteClient(int id, MessageBrokerServer server, TcpClient tcp, int minMemoryPoolSegmentLength)
    {
        _tcp = tcp;
        _stream = _tcp.GetStream();
        _memoryPool = new MemoryPool<byte>( minMemoryPoolSegmentLength );
        Server = server;
        Id = id;
        Name = string.Empty;
        IsLittleEndian = false;
        MessageTimeout = server.HandshakeTimeout;
        MaxReadTimeout = MessageTimeout;
        PingInterval = Duration.Zero;
        _state = MessageBrokerRemoteClientState.Created;

        BindingsByChannelId = new Dictionary<int, MessageBrokerChannelBinding>();
        SubscriptionsByChannelId = new Dictionary<int, MessageBrokerSubscription>();
        EventScheduler = EventScheduler.Create();
        MessageListener = MessageListener.Create();
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

        var (bindings, subscriptions) = await DisposeAsync( extractChildren: true ).ConfigureAwait( false );
        foreach ( var binding in bindings )
            binding.OnClientDisconnected();

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

        await DisposeAsync( extractChildren: false ).ConfigureAwait( false );
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
        string queueName,
        ref MessageBrokerChannelBinding? binding,
        ref MessageBrokerQueue? queue,
        ref bool queueCreated)
    {
        using ( channel.AcquireLock() )
        {
            if ( channel.ShouldCancel )
                return Protocol.BindFailureResponse.Reasons.Cancelled;

            ref var bindingRef = ref CollectionsMarshal.GetValueRefOrAddDefault( channel.BindingsByClientId, Id, out var exists )!;
            if ( exists )
            {
                binding = bindingRef;
                queue = binding.Queue;
                return Protocol.BindFailureResponse.Reasons.AlreadyBound;
            }

            queue = QueueCollection.RegisterUnsafe( Server, queueName, out queueCreated );
            using ( queue.AcquireLock() )
            {
                if ( queue.ShouldCancel )
                {
                    channel.BindingsByClientId.Remove( Id );
                    if ( channelCreated )
                        ChannelCollection.RemoveUnsafe( channel );

                    return Protocol.BindFailureResponse.Reasons.Cancelled;
                }

                try
                {
                    binding = new MessageBrokerChannelBinding( this, channel, queue );
                    bindingRef = binding;
                }
                catch
                {
                    channel.BindingsByClientId.Remove( Id );
                    if ( queueCreated )
                        QueueCollection.RemoveUnsafe( queue );

                    throw;
                }

                BindingsByChannelId.Add( channel.Id, binding );
                queue.BindingsByKey.Add( new QueueBindingKey( Id, channel.Id ), binding );
            }
        }

        return Protocol.BindFailureResponse.Reasons.None;
    }

    internal Protocol.UnbindFailureResponse.Reasons BeginUnbindUnsafe(
        MessageBrokerChannel channel,
        ref MessageBrokerChannelBinding? binding,
        ref MessageBrokerQueue? queue,
        ref bool disposingChannel,
        ref bool disposingQueue)
    {
        using ( channel.AcquireLock() )
        {
            if ( channel.ShouldCancel || ! BindingsByChannelId.TryGetValue( channel.Id, out binding ) )
                return Protocol.UnbindFailureResponse.Reasons.NotBound;

            queue = binding.Queue;
            using ( queue.AcquireLock() )
            {
                if ( queue.ShouldCancel )
                    return Protocol.UnbindFailureResponse.Reasons.NotBound;

                using ( binding.AcquireLock() )
                {
                    if ( binding.ShouldCancel )
                        return Protocol.UnbindFailureResponse.Reasons.NotBound;

                    binding.BeginDisposingUnsafe();
                    BindingsByChannelId.Remove( channel.Id );
                    disposingChannel = channel.TryDisposeByRemovingBindingUnsafe( Id );
                    disposingQueue = queue.TryDisposeByRemovingBindingUnsafe( new QueueBindingKey( Id, channel.Id ) );
                }
            }
        }

        return Protocol.UnbindFailureResponse.Reasons.None;
    }

    internal Protocol.SubscribeFailureResponse.Reasons SubscribeUnsafe(
        MessageBrokerChannel channel,
        ref MessageBrokerSubscription? subscription)
    {
        using ( channel.AcquireLock() )
        {
            if ( channel.ShouldCancel )
                return Protocol.SubscribeFailureResponse.Reasons.Cancelled;

            ref var subscriptionRef
                = ref CollectionsMarshal.GetValueRefOrAddDefault( channel.SubscriptionsByClientId, Id, out var exists )!;

            if ( exists )
            {
                subscription = subscriptionRef;
                return Protocol.SubscribeFailureResponse.Reasons.AlreadySubscribed;
            }

            try
            {
                subscriptionRef = new MessageBrokerSubscription( this, channel );
                subscription = subscriptionRef;
            }
            catch
            {
                channel.SubscriptionsByClientId.Remove( Id );
                throw;
            }

            SubscriptionsByChannelId.Add( channel.Id, subscription );
        }

        return Protocol.SubscribeFailureResponse.Reasons.None;
    }

    internal Protocol.UnsubscribeFailureResponse.Reasons BeginUnsubscribeUnsafe(
        MessageBrokerChannel channel,
        ref MessageBrokerSubscription? subscription,
        ref bool disposingChannel)
    {
        using ( channel.AcquireLock() )
        {
            if ( channel.ShouldCancel || ! SubscriptionsByChannelId.TryGetValue( channel.Id, out subscription ) )
                return Protocol.UnsubscribeFailureResponse.Reasons.NotSubscribed;

            using ( subscription.AcquireLock() )
            {
                if ( subscription.ShouldCancel )
                    return Protocol.UnsubscribeFailureResponse.Reasons.NotSubscribed;

                subscription.BeginDisposingUnsafe();
                SubscriptionsByChannelId.Remove( channel.Id );
                disposingChannel = channel.TryDisposeByRemovingSubscriptionUnsafe( Id );
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

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal Timestamp GetFutureTimestamp(Duration delay)
    {
        return GetTimestamp() + delay;
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
    internal BinaryBufferToken RentBuffer(int length, out Memory<byte> memory)
    {
        using ( ExclusiveLock.Enter( _memoryPool ) )
        {
            var token = _memoryPool.Rent( length );
            memory = token.AsMemory();
            return new BinaryBufferToken( token );
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
    internal void DisposeBufferToken(BinaryBufferToken token)
    {
        var exc = token.TryDispose().Exception;
        if ( exc is not null )
            Emit( MessageBrokerRemoteClientEvent.Unexpected( this, exc ) );
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

    private async ValueTask<(MessageBrokerChannelBinding[] Bindings, MessageBrokerSubscription[] Subscriptions)> DisposeAsync(
        bool extractChildren)
    {
        Emit( MessageBrokerRemoteClientEvent.Disposing( this ) );

        Task? eventSchedulerTask;
        Task? requestHandlerTask;
        Task? messageReceiverTask;
        ValueTaskDelaySource? ownedDelaySource;

        var bindings = Array.Empty<MessageBrokerChannelBinding>();
        var subscriptions = Array.Empty<MessageBrokerSubscription>();
        using ( AcquireLock() )
        {
            ownedDelaySource = _delaySource.DiscardOwnedSource();
            if ( extractChildren )
            {
                if ( BindingsByChannelId.Count > 0 )
                {
                    var i = 0;
                    bindings = new MessageBrokerChannelBinding[BindingsByChannelId.Count];
                    foreach ( var binding in BindingsByChannelId.Values )
                        bindings[i++] = binding;
                }

                if ( SubscriptionsByChannelId.Count > 0 )
                {
                    var i = 0;
                    subscriptions = new MessageBrokerSubscription[SubscriptionsByChannelId.Count];
                    foreach ( var subscription in SubscriptionsByChannelId.Values )
                        subscriptions[i++] = subscription;
                }
            }

            BindingsByChannelId.Clear();
            SubscriptionsByChannelId.Clear();
            eventSchedulerTask = EventScheduler.DiscardUnderlyingTask();
            requestHandlerTask = RequestHandler.DiscardUnderlyingTask();
            messageReceiverTask = MessageListener.DiscardUnderlyingTask();
            MessageContextQueue.Dispose();
            RequestHandler.Dispose();
            EventScheduler.Dispose();
        }

        if ( eventSchedulerTask is not null )
            await eventSchedulerTask.ConfigureAwait( false );

        if ( requestHandlerTask is not null )
            await requestHandlerTask.ConfigureAwait( false );

        if ( messageReceiverTask is not null )
            await messageReceiverTask.ConfigureAwait( false );

        Exception? exception;
        using ( AcquireLock() )
            exception = _tcp.TryDispose().Exception;

        if ( exception is not null )
            Emit( MessageBrokerRemoteClientEvent.Unexpected( this, exception ) );

        if ( ownedDelaySource is not null )
            await ownedDelaySource.TryDisposeAsync();

        return (bindings, subscriptions);
    }
}
