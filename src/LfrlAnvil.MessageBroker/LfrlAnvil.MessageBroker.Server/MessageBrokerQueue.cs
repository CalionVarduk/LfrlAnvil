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
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LfrlAnvil.Async;
using LfrlAnvil.MessageBroker.Server.Events;
using LfrlAnvil.MessageBroker.Server.Internal;

namespace LfrlAnvil.MessageBroker.Server;

/// <summary>
/// Represents a message broker queue, which allows a single <see cref="MessageBrokerRemoteClient"/> instance to manage
/// the order of message notifications between multiple subscriptions, sent by the server.
/// </summary>
public sealed class MessageBrokerQueue
{
    internal ReferenceStore<int, MessageBrokerSubscription> SubscriptionsByChannelId;
    internal QueueProcessor QueueProcessor;
    private readonly object _sync = new object();
    private readonly MessageBrokerQueueEventHandler? _eventHandler;
    private QueueSlim<QueueMessage> _messages;
    private MessageBrokerQueueState _state;

    internal MessageBrokerQueue(MessageBrokerRemoteClient client, int id, string name)
    {
        Client = client;
        Id = id;
        Name = name;
        _state = MessageBrokerQueueState.Running;
        SubscriptionsByChannelId = ReferenceStore<int, MessageBrokerSubscription>.Create();
        _messages = QueueSlim<QueueMessage>.Create();
        QueueProcessor = QueueProcessor.Create();
        _eventHandler = Client.Server.QueueEventHandlerFactory?.Invoke( this );
        QueueProcessor.SetUnderlyingTask( QueueProcessor.StartUnderlyingTask( this ) );
    }

    /// <summary>
    /// <see cref="MessageBrokerRemoteClient"/> instance to which this queue belongs to.
    /// </summary>
    public MessageBrokerRemoteClient Client { get; }

    /// <summary>
    /// Queue's unique identifier assigned by the client.
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
    /// Collection of <see cref="MessageBrokerSubscription"/> instances attached to this queue, identified by channel ids.
    /// </summary>
    public MessageBrokerQueueSubscriptionCollection Subscriptions => new MessageBrokerQueueSubscriptionCollection( this );

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
    internal void PushMessages(MessageBrokerSubscription subscription, in ListSlim<StreamMessage> messages)
    {
        using ( AcquireLock() )
        {
            // TODO: emit 'message-discarded' (log refactor)
            if ( ShouldCancel )
                return;

            foreach ( var message in messages )
            {
                _messages.Enqueue( new QueueMessage( in message, subscription ) );
                Emit( MessageBrokerQueueEvent.MessageEnqueued( this, subscription, message.Id ) );
            }

            QueueProcessor.SignalContinuation();
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal int CopyMessagesIntoUnsafe(ref ListSlim<QueueMessage> buffer)
    {
        Assume.True( buffer.IsEmpty );
        Assume.IsGreaterThan( buffer.Capacity, 0 );
        if ( _messages.IsEmpty )
            return 0;

        var discarded = 0;
        var queueSlice = _messages.AsMemory();

        if ( ! CopyMessagesInto( queueSlice.First.Span, ref buffer, ref discarded ) )
            CopyMessagesInto( queueSlice.Second.Span, ref buffer, ref discarded );

        return discarded + buffer.Count;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void DequeueMessagesUnsafe(int count)
    {
        _messages.DequeueRange( count );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool TryDisposeByRemovingSubscriptionUnsafe(int channelId)
    {
        SubscriptionsByChannelId.Remove( channelId );
        if ( SubscriptionsByChannelId.Count > 0 || ! _messages.IsEmpty )
            return false;

        _state = MessageBrokerQueueState.Disposing;
        return true;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool TryDisposeDueToPotentiallyEmptyQueueUnsafe()
    {
        if ( SubscriptionsByChannelId.Count > 0 || ! _messages.IsEmpty )
            return false;

        _state = MessageBrokerQueueState.Disposing;
        return true;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ValueTask OnClientDisconnectedAsync()
    {
        return DisposeAsync();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ValueTask OnServerDisposedAsync()
    {
        return DisposeAsync();
    }

    internal async ValueTask DisposeDueToLackOfReferencesAsync(bool ignoreProcessorTask)
    {
        Assume.Equals( State, MessageBrokerQueueState.Disposing );
        Emit( MessageBrokerQueueEvent.Disposing( this ) );

        Task? processorTask;
        using ( AcquireLock() )
        {
            Assume.Equals( SubscriptionsByChannelId.Count, 0 );
            Assume.True( _messages.IsEmpty );

            processorTask = QueueProcessor.DiscardUnderlyingTask();
            if ( ignoreProcessorTask )
                processorTask = null;

            QueueProcessor.Dispose();
        }

        if ( processorTask is not null )
            await processorTask.ConfigureAwait( false );

        using ( Client.AcquireLock() )
        {
            if ( ! Client.ShouldCancel )
                Client.QueuesByName.Remove( Id, Name );
        }

        using ( AcquireLock() )
            _state = MessageBrokerQueueState.Disposed;

        Emit( MessageBrokerQueueEvent.Disposed( this ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ExclusiveLock AcquireLock()
    {
        return ExclusiveLock.SpinWaitEnter( _sync, spinWaitMultiplier: 4 );
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
    private static bool CopyMessagesInto(ReadOnlySpan<QueueMessage> source, ref ListSlim<QueueMessage> target, ref int discarded)
    {
        const int maxDiscarded = 128;
        foreach ( ref readonly var message in source )
        {
            if ( message.Subscription.TryIncrementPrefetchCounter( out var disposed ) )
            {
                target.Add( message );
                if ( target.Count >= target.Capacity )
                    return true;
            }
            else if ( ! disposed || ++discarded >= maxDiscarded )
                return true;
        }

        return false;
    }

    private async ValueTask DisposeAsync()
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
            SubscriptionsByChannelId.Clear();
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

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void ClearMessages()
    {
        // TODO: emit 'message-discarded' (log refactor)
        foreach ( var message in _messages )
            message.Return();

        _messages = QueueSlim<QueueMessage>.Create();
    }
}
