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
using LfrlAnvil.MessageBroker.Server.Exceptions;
using LfrlAnvil.MessageBroker.Server.Internal;

namespace LfrlAnvil.MessageBroker.Server;

/// <summary>
/// Represents a message broker queue, which allows a single <see cref="MessageBrokerRemoteClient"/> instance to manage
/// the order of message notifications between multiple listeners, sent by the server.
/// </summary>
public sealed class MessageBrokerQueue
{
    internal ReferenceStore<int, MessageBrokerChannelListenerBinding> ListenersByChannelId;
    internal QueueProcessor QueueProcessor;
    internal readonly MessageBrokerQueueLogger Logger;
    private readonly object _sync = new object();
    private QueueSlim<QueueMessage> _messages;
    private MessageBrokerQueueState _state;
    private ulong _nextTraceId;

    internal MessageBrokerQueue(MessageBrokerRemoteClient client, int id, string name)
    {
        Client = client;
        Id = id;
        Name = name;
        _state = MessageBrokerQueueState.Running;
        _nextTraceId = 0;
        ListenersByChannelId = ReferenceStore<int, MessageBrokerChannelListenerBinding>.Create();
        _messages = QueueSlim<QueueMessage>.Create();
        QueueProcessor = QueueProcessor.Create();
        Logger = Client.Server.QueueLoggerFactory?.Invoke( this ) ?? default;
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
    /// Collection of <see cref="MessageBrokerChannelListenerBinding"/> instances attached to this queue, identified by channel ids.
    /// </summary>
    public MessageBrokerQueueListenerCollection Listeners => new MessageBrokerQueueListenerCollection( this );

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
    internal void PushMessages(
        MessageBrokerChannelListenerBinding listener,
        in ListSlim<StreamMessage> messages,
        MessageBrokerStream stream,
        ulong streamTraceId)
    {
        ulong traceId;
        using ( AcquireLock() )
        {
            if ( ShouldCancel )
                return;

            traceId = GetTraceId();
        }

        using ( MessageBrokerQueueTraceEvent.CreateScope( this, traceId, MessageBrokerQueueTraceEventType.EnqueueMessages ) )
        {
            MessageBrokerQueueStreamTraceEvent.Create( this, traceId, stream, streamTraceId ).Emit( Logger.StreamTrace );
            MessageBrokerQueueEnqueueingMessagesEvent.Create( this, traceId, messages.Count ).Emit( Logger.EnqueueingMessages );

            using ( AcquireActiveLock( traceId, out var exc ) )
            {
                if ( exc is not null )
                    return;

                foreach ( ref readonly var message in messages )
                    _messages.Enqueue( new QueueMessage( in message, listener ) );

                QueueProcessor.SignalContinuation();
            }

            MessageBrokerQueueMessagesEnqueuedEvent.Create( this, traceId, messages.Count ).Emit( Logger.MessagesEnqueued );
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
        // TODO: discarded messages are not returned to the pool
        // fix when implementing ack/nack + stream message store
        _messages.DequeueRange( count );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool TryDisposeByRemovingListenerUnsafe(int channelId)
    {
        ListenersByChannelId.Remove( channelId );
        if ( ListenersByChannelId.Count > 0 || ! _messages.IsEmpty )
            return false;

        _state = MessageBrokerQueueState.Disposing;
        return true;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool TryDisposeDueToPotentiallyEmptyQueueUnsafe()
    {
        if ( ListenersByChannelId.Count > 0 || ! _messages.IsEmpty )
            return false;

        _state = MessageBrokerQueueState.Disposing;
        return true;
    }

    internal async ValueTask OnClientDisconnectedAsync(ulong clientTraceId)
    {
        ulong traceId;
        using ( AcquireLock() )
        {
            if ( ShouldCancel )
                return;

            _state = MessageBrokerQueueState.Disposing;
            traceId = GetTraceId();
        }

        using ( MessageBrokerQueueTraceEvent.CreateScope( this, traceId, MessageBrokerQueueTraceEventType.Dispose ) )
        {
            MessageBrokerQueueClientTraceEvent.Create( this, traceId, clientTraceId ).Emit( Logger.ClientTrace );
            MessageBrokerQueueDisposingEvent.Create( this, traceId ).Emit( Logger.Disposing );

            Task? processorTask;
            using ( AcquireLock() )
            {
                ListenersByChannelId.Clear();
                processorTask = QueueProcessor.DiscardUnderlyingTask();
                QueueProcessor.Dispose();
            }

            if ( processorTask is not null )
                await processorTask.ConfigureAwait( false );

            int discardedMessageCount;
            Chain<Exception> exceptions;
            using ( AcquireLock() )
                (discardedMessageCount, exceptions) = ClearMessages();

            foreach ( var exc in exceptions )
                MessageBrokerQueueErrorEvent.Create( this, traceId, exc ).Emit( Logger.Error );

            if ( discardedMessageCount > 0 )
            {
                var error = new MessageBrokerQueueException( this, Resources.QueueMessagesDiscarded( discardedMessageCount ) );
                MessageBrokerQueueErrorEvent.Create( this, traceId, error ).Emit( Logger.Error );
            }

            using ( AcquireLock() )
                _state = MessageBrokerQueueState.Disposed;

            MessageBrokerQueueDisposedEvent.Create( this, traceId ).Emit( Logger.Disposed );
        }
    }

    internal async ValueTask DisposeDueToLackOfReferencesAsync(bool ignoreProcessorTask, ulong traceId)
    {
        Assume.Equals( State, MessageBrokerQueueState.Disposing );
        MessageBrokerQueueDisposingEvent.Create( this, traceId ).Emit( Logger.Disposing );

        Task? processorTask;
        using ( AcquireLock() )
        {
            Assume.Equals( ListenersByChannelId.Count, 0 );
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

        MessageBrokerQueueDisposedEvent.Create( this, traceId ).Emit( Logger.Disposed );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ExclusiveLock AcquireLock()
    {
        return ExclusiveLock.SpinWaitEnter( _sync, spinWaitMultiplier: 4 );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ExclusiveLock AcquireActiveLock(ulong traceId, out MessageBrokerQueueDisposedException? exception)
    {
        var @lock = AcquireLock();
        if ( ! ShouldCancel )
        {
            exception = null;
            return @lock;
        }

        @lock.Dispose();
        exception = new MessageBrokerQueueDisposedException( this );
        MessageBrokerQueueErrorEvent.Create( this, traceId, exception ).Emit( Logger.Error );
        return default;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ulong GetTraceId()
    {
        return unchecked( _nextTraceId++ );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static bool CopyMessagesInto(ReadOnlySpan<QueueMessage> source, ref ListSlim<QueueMessage> target, ref int discarded)
    {
        const int maxDiscarded = 128;
        foreach ( ref readonly var message in source )
        {
            if ( message.Listener.TryIncrementPrefetchCounter( out var disposed ) )
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

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private (int DiscardedMessageCount, Chain<Exception> Exceptions) ClearMessages()
    {
        var discardedMessageCount = _messages.Count;
        var exceptions = Chain<Exception>.Empty;

        foreach ( ref readonly var message in _messages )
        {
            var exc = message.Return();
            if ( exc is not null )
                exceptions = exceptions.Extend( exc );
        }

        _messages = QueueSlim<QueueMessage>.Create();
        return (discardedMessageCount, exceptions);
    }
}
