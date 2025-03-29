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
using System.Runtime.CompilerServices;
using LfrlAnvil.Async;
using LfrlAnvil.Extensions;
using LfrlAnvil.MessageBroker.Server.Buffering;
using LfrlAnvil.MessageBroker.Server.Events;
using LfrlAnvil.MessageBroker.Server.Internal;

namespace LfrlAnvil.MessageBroker.Server;

/// <summary>
/// Represents a message broker queue, which allows <see cref="MessageBrokerChannelBinding"/> instances to enqueue messages
/// in order to be processed and moved to relevant <see cref="MessageBrokerSubscription"/> instances.
/// </summary>
public sealed class MessageBrokerQueue
{
    internal readonly Dictionary<QueueBindingKey, MessageBrokerChannelBinding> BindingsByKey;
    private readonly MessageBrokerQueueEventHandler? _eventHandler;
    private QueueSlim<EnqueuedMessage> _messages;
    private ulong _nextMessageId;
    private MessageBrokerQueueState _state;

    internal MessageBrokerQueue(MessageBrokerServer server, int id, string name)
    {
        Server = server;
        Id = id;
        Name = name;
        _nextMessageId = 0;
        _state = MessageBrokerQueueState.Running;
        _messages = QueueSlim<EnqueuedMessage>.Create();
        BindingsByKey = new Dictionary<QueueBindingKey, MessageBrokerChannelBinding>();
        _eventHandler = Server.QueueEventHandlerFactory?.Invoke( this );
    }

    /// <summary>
    /// <see cref="MessageBrokerServer"/> instance that owns this queue.
    /// </summary>
    public MessageBrokerServer Server { get; }

    /// <summary>
    /// Queue's unique identifier assigned by the server.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Queue's unique name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Current queue's state.
    /// </summary>
    /// <remarks>See <see cref="MessageBrokerQueueState"/> for more information.</remarks>
    public MessageBrokerQueueState State
    {
        get
        {
            using ( AcquireLock() )
                return _state;
        }
    }

    /// <summary>
    /// Collection of <see cref="MessageBrokerChannelBinding"/> instances attached to this queue,
    /// identified by (client-id, channel-id) tuples.
    /// </summary>
    public MessageBrokerQueueBindingCollection Bindings => new MessageBrokerQueueBindingCollection( this );

    internal bool ShouldCancel => _state >= MessageBrokerQueueState.Disposing;

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerQueue"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[{Id}] '{Name}' queue ({State})";
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ulong? Enqueue(MessageBrokerChannelBinding binding, BinaryBufferToken token, ReadOnlyMemory<byte> data)
    {
        using ( AcquireLock() )
        {
            if ( ShouldCancel )
                return null;

            var id = unchecked( _nextMessageId++ );
            _messages.Enqueue( new EnqueuedMessage( id, binding, token, data ) );
            // TODO:
            // signal underlying task loop that there are messages to process (propagate to subscribers)
            return id;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool TryDisposeByRemovingBindingUnsafe(QueueBindingKey bindingKey)
    {
        BindingsByKey.Remove( bindingKey );
        if ( BindingsByKey.Count > 0 || ! _messages.IsEmpty )
            return false;

        _state = MessageBrokerQueueState.Disposing;
        return true;
    }

    internal void OnBindingDisposing(MessageBrokerRemoteClient client, MessageBrokerChannel channel)
    {
        var dispose = false;
        using ( AcquireLock() )
        {
            if ( ShouldCancel )
                return;

            if ( BindingsByKey.Remove( new QueueBindingKey( client.Id, channel.Id ) )
                && BindingsByKey.Count == 0
                && _messages.IsEmpty )
            {
                dispose = true;
                _state = MessageBrokerQueueState.Disposing;
            }
        }

        if ( dispose )
            DisposeDueToLackOfReferences();
    }

    internal void OnServerDisposed()
    {
        using ( AcquireLock() )
        {
            if ( ShouldCancel )
                return;

            _state = MessageBrokerQueueState.Disposing;
        }

        Emit( MessageBrokerQueueEvent.Disposing( this ) );

        using ( AcquireLock() )
        {
            BindingsByKey.Clear();
            foreach ( var message in _messages )
                message.BufferToken.TryDispose();

            _messages.Clear();
            _state = MessageBrokerQueueState.Disposed;
        }

        Emit( MessageBrokerQueueEvent.Disposed( this ) );
    }

    internal void DisposeDueToLackOfReferences()
    {
        Assume.IsEmpty( BindingsByKey );
        Assume.True( _messages.IsEmpty );
        Assume.Equals( State, MessageBrokerQueueState.Disposing );

        Emit( MessageBrokerQueueEvent.Disposing( this ) );

        var exc = QueueCollection.Remove( this ).Exception;
        if ( exc is not null )
            Emit( MessageBrokerQueueEvent.Unexpected( this, exc ) );

        using ( AcquireLock() )
            _state = MessageBrokerQueueState.Disposed;

        Emit( MessageBrokerQueueEvent.Disposed( this ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ExclusiveLock AcquireLock()
    {
        return ExclusiveLock.SpinWaitEnter( BindingsByKey, spinWaitMultiplier: 4 );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void Emit(MessageBrokerQueueEvent e)
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
}
