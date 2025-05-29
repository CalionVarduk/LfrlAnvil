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

using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Async;
using LfrlAnvil.MessageBroker.Server.Events;
using LfrlAnvil.MessageBroker.Server.Exceptions;
using LfrlAnvil.MessageBroker.Server.Internal;

namespace LfrlAnvil.MessageBroker.Server;

/// <summary>
/// Represents a message broker channel, which allows clients to bind to it as either a publisher or a listener.
/// </summary>
public sealed class MessageBrokerChannel
{
    internal ReferenceStore<int, MessageBrokerChannelPublisherBinding> PublishersByClientId;
    internal ReferenceStore<int, MessageBrokerChannelListenerBinding> ListenersByClientId;
    internal readonly MessageBrokerChannelLogger Logger;
    private readonly object _sync = new object();
    private MessageBrokerChannelState _state;
    private ulong _nextTraceId;

    internal MessageBrokerChannel(MessageBrokerServer server, int id, string name)
    {
        Server = server;
        Id = id;
        Name = name;
        _state = MessageBrokerChannelState.Running;
        _nextTraceId = 0;
        PublishersByClientId = ReferenceStore<int, MessageBrokerChannelPublisherBinding>.Create();
        ListenersByClientId = ReferenceStore<int, MessageBrokerChannelListenerBinding>.Create();
        Logger = Server.ChannelLoggerFactory?.Invoke( this ) ?? default;
    }

    /// <summary>
    /// <see cref="MessageBrokerServer"/> instance that owns this channel.
    /// </summary>
    public MessageBrokerServer Server { get; }

    /// <summary>
    /// Channel's unique identifier assigned by the server.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Channel's unique name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Current channel's state.
    /// </summary>
    /// <remarks>See <see cref="MessageBrokerChannelState"/> for more information.</remarks>
    public MessageBrokerChannelState State
    {
        get
        {
            using ( AcquireLock() )
                return _state;
        }
    }

    /// <summary>
    /// Collection of <see cref="MessageBrokerChannelPublisherBinding"/> instances attached to this channel, identified by client ids.
    /// </summary>
    public MessageBrokerChannelPublisherBindingCollection Publishers => new MessageBrokerChannelPublisherBindingCollection( this );

    /// <summary>
    /// Collection of <see cref="MessageBrokerChannelListenerBinding"/> instances attached to this channel, identified by client ids.
    /// </summary>
    public MessageBrokerChannelListenerBindingCollection Listeners => new MessageBrokerChannelListenerBindingCollection( this );

    internal bool ShouldCancel => _state >= MessageBrokerChannelState.Disposing;

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerChannel"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[{Id}] '{Name}' channel ({State})";
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool TryDisposeByRemovingPublisherUnsafe(int clientId)
    {
        PublishersByClientId.Remove( clientId );
        if ( ListenersByClientId.Count > 0 || PublishersByClientId.Count > 0 )
            return false;

        _state = MessageBrokerChannelState.Disposing;
        return true;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool TryDisposeByRemovingListenerUnsafe(int clientId)
    {
        ListenersByClientId.Remove( clientId );
        if ( ListenersByClientId.Count > 0 || PublishersByClientId.Count > 0 )
            return false;

        _state = MessageBrokerChannelState.Disposing;
        return true;
    }

    internal void OnPublisherDisposing(MessageBrokerRemoteClient client, ulong clientTraceId)
    {
        ulong traceId;
        using ( AcquireLock() )
        {
            if ( ShouldCancel )
                return;

            traceId = GetTraceId();
        }

        using ( MessageBrokerChannelTraceEvent.CreateScope( this, traceId, MessageBrokerChannelTraceEventType.UnbindPublisher ) )
        {
            MessageBrokerChannelClientTraceEvent.Create( this, traceId, client, clientTraceId ).Emit( Logger.ClientTrace );

            var dispose = false;
            MessageBrokerChannelPublisherBinding? publisher;
            using ( AcquireActiveLock( traceId, out var exc ) )
            {
                if ( exc is not null )
                    return;

                if ( PublishersByClientId.Remove( client.Id, out publisher )
                    && PublishersByClientId.Count == 0
                    && ListenersByClientId.Count == 0 )
                {
                    dispose = true;
                    _state = MessageBrokerChannelState.Disposing;
                }
            }

            if ( publisher is null )
            {
                var error = new MessageBrokerChannelException( this, Resources.NotBoundAsPublisher( this, client ) );
                MessageBrokerChannelErrorEvent.Create( this, traceId, error ).Emit( Logger.Error );
                return;
            }

            MessageBrokerChannelPublisherUnboundEvent.Create( publisher, traceId, streamRemoved: false ).Emit( Logger.PublisherUnbound );
            if ( dispose )
                DisposeDueToLackOfReferences( traceId );
        }
    }

    internal void OnListenerDisposing(MessageBrokerRemoteClient client, ulong clientTraceId)
    {
        ulong traceId;
        using ( AcquireLock() )
        {
            if ( ShouldCancel )
                return;

            traceId = GetTraceId();
        }

        using ( MessageBrokerChannelTraceEvent.CreateScope( this, traceId, MessageBrokerChannelTraceEventType.UnbindListener ) )
        {
            MessageBrokerChannelClientTraceEvent.Create( this, traceId, client, clientTraceId ).Emit( Logger.ClientTrace );

            var dispose = false;
            MessageBrokerChannelListenerBinding? listener;
            using ( AcquireActiveLock( traceId, out var exc ) )
            {
                if ( exc is not null )
                    return;

                if ( ListenersByClientId.Remove( client.Id, out listener )
                    && PublishersByClientId.Count == 0
                    && ListenersByClientId.Count == 0 )
                {
                    dispose = true;
                    _state = MessageBrokerChannelState.Disposing;
                }
            }

            if ( listener is null )
            {
                var error = new MessageBrokerChannelException( this, Resources.NotBoundAsListener( this, client ) );
                MessageBrokerChannelErrorEvent.Create( this, traceId, error ).Emit( Logger.Error );
                return;
            }

            MessageBrokerChannelListenerUnboundEvent.Create( listener, traceId, queueRemoved: false ).Emit( Logger.ListenerUnbound );
            if ( dispose )
                DisposeDueToLackOfReferences( traceId );
        }
    }

    internal void OnServerDisposed(ulong serverTraceId)
    {
        ulong traceId;
        using ( AcquireLock() )
        {
            if ( ShouldCancel )
                return;

            _state = MessageBrokerChannelState.Disposing;
            traceId = GetTraceId();
        }

        using ( MessageBrokerChannelTraceEvent.CreateScope( this, traceId, MessageBrokerChannelTraceEventType.Dispose ) )
        {
            MessageBrokerChannelServerTraceEvent.Create( this, traceId, serverTraceId ).Emit( Logger.ServerTrace );
            MessageBrokerChannelDisposingEvent.Create( this, traceId ).Emit( Logger.Disposing );

            using ( AcquireLock() )
            {
                PublishersByClientId.Clear();
                ListenersByClientId.Clear();
                _state = MessageBrokerChannelState.Disposed;
            }

            MessageBrokerChannelDisposedEvent.Create( this, traceId ).Emit( Logger.Disposed );
        }
    }

    internal void DisposeDueToLackOfReferences(ulong traceId)
    {
        Assume.Equals( State, MessageBrokerChannelState.Disposing );
        MessageBrokerChannelDisposingEvent.Create( this, traceId ).Emit( Logger.Disposing );

        var exc = ChannelCollection.Remove( this ).Exception;
        if ( exc is not null )
            MessageBrokerChannelErrorEvent.Create( this, traceId, exc ).Emit( Logger.Error );

        using ( AcquireLock() )
        {
            Assume.Equals( PublishersByClientId.Count, 0 );
            Assume.Equals( ListenersByClientId.Count, 0 );
            _state = MessageBrokerChannelState.Disposed;
        }

        MessageBrokerChannelDisposedEvent.Create( this, traceId ).Emit( Logger.Disposed );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ExclusiveLock AcquireLock()
    {
        return ExclusiveLock.SpinWaitEnter( _sync, spinWaitMultiplier: 4 );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ExclusiveLock AcquireActiveLock(ulong traceId, out MessageBrokerChannelDisposedException? exception)
    {
        var @lock = AcquireLock();
        if ( ! ShouldCancel )
        {
            exception = null;
            return @lock;
        }

        @lock.Dispose();
        exception = new MessageBrokerChannelDisposedException( this );
        MessageBrokerChannelErrorEvent.Create( this, traceId, exception ).Emit( Logger.Error );
        return default;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ulong GetTraceId()
    {
        return unchecked( _nextTraceId++ );
    }
}
