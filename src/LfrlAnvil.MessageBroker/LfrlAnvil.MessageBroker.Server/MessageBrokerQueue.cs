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
using System.Threading.Tasks;
using LfrlAnvil.Async;
using LfrlAnvil.Memory;
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
    internal QueueProcessor QueueProcessor;

    private readonly MessageBrokerQueueEventHandler? _eventHandler;
    private QueueSlim<QueueMessage> _messages;
    private ulong _nextMessageId;
    private MessageBrokerQueueState _state;

    internal MessageBrokerQueue(MessageBrokerServer server, int id, string name)
    {
        Server = server;
        Id = id;
        Name = name;
        _nextMessageId = 0;
        _state = MessageBrokerQueueState.Running;
        _messages = QueueSlim<QueueMessage>.Create();
        BindingsByKey = new Dictionary<QueueBindingKey, MessageBrokerChannelBinding>();
        QueueProcessor = QueueProcessor.Create();
        _eventHandler = Server.QueueEventHandlerFactory?.Invoke( this );
        QueueProcessor.SetUnderlyingTask( QueueProcessor.StartUnderlyingTask( this ) );
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

    /// <summary>
    /// Removes this queue from the server.
    /// </summary>
    /// <returns>A task that represents the asynchronous removal operation.</returns>
    public async ValueTask RemoveAsync()
    {
        using ( AcquireLock() )
        {
            if ( ShouldCancel )
                return;

            _state = MessageBrokerQueueState.Disposing;
        }

        Emit( MessageBrokerQueueEvent.Disposing( this ) );

        Task? processorTask;
        var bindings = Array.Empty<MessageBrokerChannelBinding>();
        using ( AcquireLock() )
        {
            if ( BindingsByKey.Count > 0 )
            {
                var i = 0;
                bindings = new MessageBrokerChannelBinding[BindingsByKey.Count];
                foreach ( var binding in BindingsByKey.Values )
                    bindings[i++] = binding;

                BindingsByKey.Clear();
            }

            ClearMessages();
            processorTask = QueueProcessor.DiscardUnderlyingTask();
            QueueProcessor.Dispose();
        }

        await Parallel.ForEachAsync( bindings, static (b, _) => b.OnQueueDisposedAsync() ).ConfigureAwait( false );
        if ( processorTask is not null )
            await processorTask.ConfigureAwait( false );

        var exc = QueueCollection.Remove( this ).Exception;
        if ( exc is not null )
            Emit( MessageBrokerQueueEvent.Unexpected( this, exc ) );

        using ( AcquireLock() )
            _state = MessageBrokerQueueState.Disposed;

        Emit( MessageBrokerQueueEvent.Disposed( this ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal Protocol.MessageRejectedResponse.Reasons Enqueue(
        MessageBrokerChannelBinding binding,
        MemoryPoolToken<byte> token,
        ReadOnlyMemory<byte> data,
        ulong contextId,
        ref ulong? messageId)
    {
        using ( AcquireLock() )
        {
            if ( ShouldCancel )
                return Protocol.MessageRejectedResponse.Reasons.Cancelled;

            // TODO:
            // this may require binding lock later (e.g. when permanence is implemented)
            // right now it doesn't really matter, even though binding in disposing state will be allowed to enqueue messages

            var id = unchecked( _nextMessageId++ );
            _messages.Enqueue( new QueueMessage( id, binding.Client.GetTimestamp(), binding, token, data ) );
            messageId = id;

            Emit( MessageBrokerQueueEvent.MessageEnqueued( this, binding, id, contextId ) );
            QueueProcessor.SignalContinuation();
        }

        return Protocol.MessageRejectedResponse.Reasons.None;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool TryDequeueUnsafe(out QueueMessage message)
    {
        return _messages.TryDequeue( out message );
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

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool TryDisposeDueToEmptyQueueUnsafe()
    {
        Assume.True( _messages.IsEmpty );
        if ( BindingsByKey.Count > 0 )
            return false;

        _state = MessageBrokerQueueState.Disposing;
        return true;
    }

    internal async ValueTask OnBindingDisposingAsync(MessageBrokerRemoteClient client, MessageBrokerChannel channel)
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
            await DisposeDueToLackOfReferencesAsync( ignoreProcessorTask: false ).ConfigureAwait( false );
    }

    internal async ValueTask OnServerDisposedAsync()
    {
        using ( AcquireLock() )
        {
            if ( ShouldCancel )
                return;

            _state = MessageBrokerQueueState.Disposing;
        }

        Emit( MessageBrokerQueueEvent.Disposing( this ) );

        Task? processorTask;
        using ( AcquireLock() )
        {
            BindingsByKey.Clear();
            ClearMessages();
            processorTask = QueueProcessor.DiscardUnderlyingTask();
            QueueProcessor.Dispose();
        }

        if ( processorTask is not null )
            await processorTask.ConfigureAwait( false );

        using ( AcquireLock() )
            _state = MessageBrokerQueueState.Disposed;

        Emit( MessageBrokerQueueEvent.Disposed( this ) );
    }

    internal async ValueTask DisposeDueToLackOfReferencesAsync(bool ignoreProcessorTask)
    {
        Assume.IsEmpty( BindingsByKey );
        Assume.True( _messages.IsEmpty );
        Assume.Equals( State, MessageBrokerQueueState.Disposing );

        Emit( MessageBrokerQueueEvent.Disposing( this ) );

        Task? processorTask;
        using ( AcquireLock() )
        {
            processorTask = QueueProcessor.DiscardUnderlyingTask();
            if ( ignoreProcessorTask )
                processorTask = null;

            QueueProcessor.Dispose();
        }

        if ( processorTask is not null )
            await processorTask.ConfigureAwait( false );

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

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void ClearMessages()
    {
        foreach ( var message in _messages )
            message.PoolToken.Return( message.Binding.Client );

        _messages = QueueSlim<QueueMessage>.Create();
    }
}
