// Copyright 2026 Łukasz Furlepa
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
using LfrlAnvil.Async;
using LfrlAnvil.Chrono;
using LfrlAnvil.Computable.Expressions;
using LfrlAnvil.MessageBroker.Server.Internal;

namespace LfrlAnvil.MessageBroker.Server;

/// <summary>
/// Represents a message broker queue binding for a listener, which represents pairing between a channel listener binding and a queue.
/// </summary>
public sealed class MessageBrokerQueueListenerBinding
{
    private readonly object _sync = new object();
    private Func<MessageBrokerFilterExpressionContext[], bool>? _filterPredicate;
    private string? _filterExpression;
    private int _prefetchHint;
    private int _maxRetries;
    private int _maxRedeliveries;
    private int _deadLetterCapacityHint;
    private Duration _retryDelay;
    private Duration _minAckTimeout;
    private Duration _minDeadLetterRetention;
    private int _sentMessages;
    private int _deadLetterMessages;
    private long _refCounter;
    private MessageBrokerQueueListenerBindingState _state;
    private bool _isPrimary;

    internal MessageBrokerQueueListenerBinding(
        MessageBrokerRemoteClient client,
        MessageBrokerChannelListenerBinding owner,
        MessageBrokerQueue queue,
        int prefetchHint,
        int maxRetries,
        Duration retryDelay,
        int maxRedeliveries,
        Duration minAckTimeout,
        int deadLetterCapacityHint,
        Duration minDeadLetterRetention,
        string? filterExpression,
        Func<MessageBrokerFilterExpressionContext[], bool>? filterPredicate,
        MessageBrokerQueueListenerBindingState state,
        bool isPrimary)
    {
        Client = client;
        Owner = owner;
        Queue = queue;
        SetProperties(
            prefetchHint,
            maxRetries,
            retryDelay,
            maxRedeliveries,
            minAckTimeout,
            deadLetterCapacityHint,
            minDeadLetterRetention,
            filterExpression,
            filterPredicate );

        _sentMessages = 0;
        _deadLetterMessages = 0;
        _refCounter = 0;
        _state = state;
        _isPrimary = isPrimary;
    }

    /// <summary>
    /// <see cref="MessageBrokerRemoteClient"/> instance to which this queue binding belongs to.
    /// </summary>
    public MessageBrokerRemoteClient Client { get; }

    /// <summary>
    /// <see cref="MessageBrokerChannelListenerBinding"/> instance linked with this queue binding.
    /// </summary>
    public MessageBrokerChannelListenerBinding Owner { get; }

    /// <summary>
    /// <see cref="MessageBrokerQueue"/> instance which processes messages bound to this listener.
    /// </summary>
    public MessageBrokerQueue Queue { get; }

    /// <summary>
    /// Specifies how many messages intended for this listener can be sent by the <see cref="Queue"/> to the client at the same time.
    /// </summary>
    public int PrefetchHint
    {
        get
        {
            using ( AcquireLock() )
                return _prefetchHint;
        }
    }

    /// <summary>
    /// Specifies how many times the <see cref="Queue"/> will attempt to automatically send a message notification retry
    /// when the client responds with a negative ACK, before giving up.
    /// </summary>
    public int MaxRetries
    {
        get
        {
            using ( AcquireLock() )
                return _maxRetries;
        }
    }

    /// <summary>
    /// Specifies the delay between the <see cref="Queue"/> successfully processing negative ACK sent by the client
    /// and the <see cref="Queue"/> sending a message notification retry.
    /// </summary>
    public Duration RetryDelay
    {
        get
        {
            using ( AcquireLock() )
                return _retryDelay;
        }
    }

    /// <summary>
    /// Specifies how many times the <see cref="Queue"/> will attempt to automatically send a message notification redelivery
    /// when the client fails to respond with either an ACK or a negative ACK in time (see <see cref="MinAckTimeout"/>), before giving up.
    /// </summary>
    public int MaxRedeliveries
    {
        get
        {
            using ( AcquireLock() )
                return _maxRedeliveries;
        }
    }

    /// <summary>
    /// Specifies the minimum amount of time that the <see cref="Queue"/> will wait for the client
    /// to send either an ACK or a negative ACK before attempting a message notification redelivery.
    /// Actual ACK timeout may be different due to the state of the <see cref="Queue"/> and other listeners bound to it.
    /// </summary>
    public Duration MinAckTimeout
    {
        get
        {
            using ( AcquireLock() )
                return _minAckTimeout;
        }
    }

    /// <summary>
    /// Specifies how many messages intended for this listener can be stored at most by the <see cref="Queue"/>'s dead letter.
    /// Actual capacity may be different due to the state of the <see cref="Queue"/> and other listeners bound to it.
    /// </summary>
    public int DeadLetterCapacityHint
    {
        get
        {
            using ( AcquireLock() )
                return _deadLetterCapacityHint;
        }
    }

    /// <summary>
    /// Specifies the minimum retention period for messages intended for this listener stored in the <see cref="Queue"/>'s dead letter.
    /// Actual retention period may be different due to the state of the <see cref="Queue"/> and other listeners bound to it.
    /// </summary>
    public Duration MinDeadLetterRetention
    {
        get
        {
            using ( AcquireLock() )
                return _minDeadLetterRetention;
        }
    }

    /// <summary>
    /// Specifies message filter expression.
    /// </summary>
    public string? FilterExpression
    {
        get
        {
            using ( AcquireLock() )
                return _filterExpression;
        }
    }

    /// <summary>
    /// Specifies whether the client is expected to send ACK or negative ACK to the <see cref="Queue"/>
    /// in order to confirm message notification.
    /// </summary>
    public bool AreAcksEnabled => MinAckTimeout > Duration.Zero;

    /// <summary>
    /// Current listener's state.
    /// </summary>
    /// <remarks>See <see cref="MessageBrokerQueueListenerBindingState"/> for more information.</remarks>
    public MessageBrokerQueueListenerBindingState State
    {
        get
        {
            using ( AcquireLock() )
                return _state;
        }
    }

    /// <summary>
    /// Specifies whether this listener is a primary queue binding for its <see cref="Owner"/>.
    /// </summary>
    public bool IsPrimary
    {
        get
        {
            using ( AcquireLock() )
                return _isPrimary;
        }
    }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerQueueListenerBinding"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var type = IsPrimary ? "Primary" : "Secondary";
        return
            $"[{Client.Id}] '{Client.Name}' => [{Owner.Channel.Id}] '{Owner.Channel.Name}' queue listener binding (using [{Queue.Id}] '{Queue.Name}' queue) ({type}:{State})";
    }

    [Pure]
    internal MessageBrokerQueueListenerBinding CloneInactive(MessageBrokerQueue queue)
    {
        using ( AcquireLock() )
        {
            Assume.Equals( queue.Client, Client );
            Assume.NotEquals( queue, Queue );
            return new MessageBrokerQueueListenerBinding(
                Client,
                Owner,
                queue,
                _prefetchHint,
                _maxRetries,
                _retryDelay,
                _maxRedeliveries,
                _minAckTimeout,
                _deadLetterCapacityHint,
                _minDeadLetterRetention,
                _filterExpression,
                _filterPredicate,
                MessageBrokerQueueListenerBindingState.Inactive,
                isPrimary: false );
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void MarkAsRunning()
    {
        using ( AcquireLock() )
        {
            if ( _state == MessageBrokerQueueListenerBindingState.Created )
                _state = MessageBrokerQueueListenerBindingState.Running;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool TryMarkAsDisposed()
    {
        using ( AcquireLock() )
        {
            if ( _state == MessageBrokerQueueListenerBindingState.Disposed )
                return false;

            _state = MessageBrokerQueueListenerBindingState.Disposed;
            _sentMessages = 0;
            _deadLetterMessages = 0;
            _refCounter = 0;
            return true;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void MarkAsInactive()
    {
        using ( AcquireLock() )
        {
            if ( _state != MessageBrokerQueueListenerBindingState.Disposed )
                _state = MessageBrokerQueueListenerBindingState.Inactive;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool? TryReactivate(
        in Protocol.BindListenerRequestHeader header,
        string? filterExpression,
        IParsedExpressionDelegate<MessageBrokerFilterExpressionContext, bool>? filterExpressionDelegate,
        bool isPrimary,
        bool disposeIfUnreferenced = false)
    {
        using ( AcquireLock() )
        {
            if ( _state != MessageBrokerQueueListenerBindingState.Inactive )
                return false;

            SetProperties(
                header.PrefetchHint,
                header.MaxRetries,
                header.RetryDelay,
                header.MaxRedeliveries,
                header.MinAckTimeout,
                header.DeadLetterCapacityHint,
                header.MinDeadLetterRetention,
                filterExpression,
                filterExpressionDelegate?.Delegate );

            _isPrimary = isPrimary;
            _state = MessageBrokerQueueListenerBindingState.Created;

            if ( disposeIfUnreferenced && _refCounter == 0 )
            {
                Assume.False( _isPrimary );
                _state = MessageBrokerQueueListenerBindingState.Disposed;
                _deadLetterMessages = 0;
                _sentMessages = 0;
                return null;
            }
        }

        return true;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool TrySendRedeliveryUnsafe(out bool disposed)
    {
        if ( _state != MessageBrokerQueueListenerBindingState.Running )
        {
            disposed = _state == MessageBrokerQueueListenerBindingState.Disposed;
            return false;
        }

        disposed = false;
        return true;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool TryHandleRetryUnsafe(int retry, out bool disposed, out QueueMessageStore.MessageType type)
    {
        if ( _state != MessageBrokerQueueListenerBindingState.Running )
        {
            disposed = _state == MessageBrokerQueueListenerBindingState.Disposed;
            type = default;
            return false;
        }

        disposed = false;
        if ( retry > _maxRetries )
        {
            type = QueueMessageStore.MessageType.MaxRetriesReached;
            return true;
        }

        type = QueueMessageStore.MessageType.Retry;
        if ( _sentMessages >= _prefetchHint )
            return false;

        _sentMessages = unchecked( _sentMessages + 1 );
        return true;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool TrySendPendingUnsafe(out bool disposed, out Func<MessageBrokerFilterExpressionContext[], bool>? filterPredicate)
    {
        if ( _state != MessageBrokerQueueListenerBindingState.Running )
        {
            disposed = _state == MessageBrokerQueueListenerBindingState.Disposed;
            filterPredicate = null;
            return false;
        }

        disposed = false;
        filterPredicate = _filterPredicate;
        if ( _sentMessages >= _prefetchHint )
            return false;

        _sentMessages = unchecked( _sentMessages + 1 );
        return true;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool TryHandleDeadLetterUnsafe(bool queried, bool expired, out bool disposed, out QueueMessageStore.MessageType type)
    {
        if ( _state == MessageBrokerQueueListenerBindingState.Disposed )
        {
            disposed = true;
            type = default;
            return false;
        }

        disposed = false;
        if ( queried )
        {
            if ( _state == MessageBrokerQueueListenerBindingState.Running && _sentMessages < _prefetchHint )
            {
                type = QueueMessageStore.MessageType.DeadLetter;
                _sentMessages = unchecked( _sentMessages + 1 );
                return true;
            }
        }

        if ( expired )
        {
            type = QueueMessageStore.MessageType.DeadLetterExpiration;
            if ( _deadLetterMessages > 0 )
                _deadLetterMessages = unchecked( _deadLetterMessages - 1 );

            return true;
        }

        type = QueueMessageStore.MessageType.DeadLetterCapacityExceeded;
        if ( _state != MessageBrokerQueueListenerBindingState.Running || _deadLetterMessages <= _deadLetterCapacityHint )
            return false;

        Assume.IsGreaterThan( _deadLetterMessages, 0 );
        _deadLetterMessages = unchecked( _deadLetterMessages - 1 );
        return true;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool CanScheduleRetry()
    {
        using ( AcquireLock() )
            return (( int )_state & 1) == 1 && _sentMessages < _prefetchHint;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool CanScheduleRedelivery()
    {
        using ( AcquireLock() )
            return (( int )_state & 1) == 1;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void AddInactiveMessage()
    {
        using ( AcquireLock() )
        {
            if ( _state == MessageBrokerQueueListenerBindingState.Inactive )
                _refCounter = unchecked( _refCounter + 1 );
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void AddInactiveUnackedMessage()
    {
        using ( AcquireLock() )
        {
            if ( _state == MessageBrokerQueueListenerBindingState.Inactive )
            {
                _sentMessages = checked( _sentMessages + 1 );
                _refCounter = unchecked( _refCounter + 1 );
            }
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void AddInactiveDeadLetterMessage()
    {
        using ( AcquireLock() )
        {
            if ( _state == MessageBrokerQueueListenerBindingState.Inactive )
            {
                _deadLetterMessages = checked( _deadLetterMessages + 1 );
                _refCounter = unchecked( _refCounter + 1 );
            }
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void MoveExceededRedeliveryToDeadLetterUnsafe()
    {
        if ( _state == MessageBrokerQueueListenerBindingState.Disposed )
        {
            Assume.Equals( _sentMessages, 0 );
            return;
        }

        _deadLetterMessages = checked( _deadLetterMessages + 1 );
        if ( _sentMessages > 0 )
            _sentMessages = unchecked( _sentMessages - 1 );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void MoveExceededRetryToDeadLetterUnsafe()
    {
        if ( _state != MessageBrokerQueueListenerBindingState.Disposed )
            _deadLetterMessages = checked( _deadLetterMessages + 1 );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void MoveAcknowledgedMessageToRetriesUnsafe()
    {
        if ( _sentMessages > 0 )
            _sentMessages = unchecked( _sentMessages - 1 );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void AddReferencingMessageUnsafe()
    {
        if ( _state != MessageBrokerQueueListenerBindingState.Disposed )
            _refCounter = unchecked( _refCounter + 1 );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void RemoveSentMessageUnsafe(out bool signalQueue)
    {
        if ( _state == MessageBrokerQueueListenerBindingState.Disposed )
        {
            Assume.Equals( _sentMessages, 0 );
            signalQueue = true;
            return;
        }

        signalQueue = _sentMessages >= _prefetchHint;
        if ( _sentMessages > 0 )
            _sentMessages = unchecked( _sentMessages - 1 );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void MoveAcknowledgedMessageToDeadLetterUnsafe(out bool signalQueue)
    {
        if ( _state == MessageBrokerQueueListenerBindingState.Disposed )
        {
            Assume.Equals( _sentMessages, 0 );
            signalQueue = true;
            return;
        }

        signalQueue = _sentMessages >= _prefetchHint || _deadLetterMessages >= _deadLetterCapacityHint;
        _deadLetterMessages = checked( _deadLetterMessages + 1 );
        if ( _sentMessages > 0 )
            _sentMessages = unchecked( _sentMessages - 1 );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool TryDisposeByRemovingAcknowledgedMessageUnsafe(out bool signalQueue)
    {
        if ( _state == MessageBrokerQueueListenerBindingState.Disposed )
        {
            Assume.Equals( _refCounter, 0 );
            Assume.Equals( _sentMessages, 0 );
            signalQueue = true;
            return false;
        }

        if ( _refCounter > 0 )
            _refCounter = unchecked( _refCounter - 1 );

        if ( _refCounter == 0 && ! _isPrimary )
        {
            signalQueue = true;
            _state = MessageBrokerQueueListenerBindingState.Disposed;
            _deadLetterMessages = 0;
            _sentMessages = 0;
            return true;
        }

        signalQueue = _sentMessages >= _prefetchHint;
        if ( _sentMessages > 0 )
            _sentMessages = unchecked( _sentMessages - 1 );

        return false;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool TryDisposeByRemovingExceededRedeliveryUnsafe()
    {
        if ( _state == MessageBrokerQueueListenerBindingState.Disposed )
        {
            Assume.Equals( _refCounter, 0 );
            Assume.Equals( _sentMessages, 0 );
            return false;
        }

        if ( _refCounter > 0 )
            _refCounter = unchecked( _refCounter - 1 );

        if ( _refCounter == 0 && ! _isPrimary )
        {
            _state = MessageBrokerQueueListenerBindingState.Disposed;
            _deadLetterMessages = 0;
            _sentMessages = 0;
            return true;
        }

        if ( _sentMessages > 0 )
            _sentMessages = unchecked( _sentMessages - 1 );

        return false;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool TryDisposeByRemovingExceededRetryUnsafe()
    {
        if ( _state == MessageBrokerQueueListenerBindingState.Disposed )
        {
            Assume.Equals( _refCounter, 0 );
            return false;
        }

        if ( _refCounter > 0 )
            _refCounter = unchecked( _refCounter - 1 );

        if ( _refCounter == 0 && ! _isPrimary )
        {
            _state = MessageBrokerQueueListenerBindingState.Disposed;
            _deadLetterMessages = 0;
            _sentMessages = 0;
            return true;
        }

        return false;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool TryDisposeByRemovingNonExistingMessageUnsafe(bool isFromDeadLetter)
    {
        if ( _state == MessageBrokerQueueListenerBindingState.Disposed )
        {
            Assume.Equals( _refCounter, 0 );
            Assume.Equals( _sentMessages, 0 );
            Assume.Equals( _deadLetterMessages, 0 );
            return false;
        }

        if ( _refCounter > 0 )
            _refCounter = unchecked( _refCounter - 1 );

        if ( _refCounter == 0 && ! _isPrimary )
        {
            _state = MessageBrokerQueueListenerBindingState.Disposed;
            _deadLetterMessages = 0;
            _sentMessages = 0;
            return true;
        }

        if ( _sentMessages > 0 )
            _sentMessages = unchecked( _sentMessages - 1 );

        if ( isFromDeadLetter && _deadLetterMessages > 0 )
            _deadLetterMessages = unchecked( _deadLetterMessages - 1 );

        return false;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool TryDisposeByRemovingUnprocessedMessageUnsafe(bool isAck, bool isRedelivery)
    {
        if ( _state == MessageBrokerQueueListenerBindingState.Disposed )
        {
            Assume.Equals( _refCounter, 0 );
            Assume.Equals( _sentMessages, 0 );
            return false;
        }

        if ( isAck )
        {
            if ( _refCounter > 0 )
                _refCounter = unchecked( _refCounter - 1 );

            if ( _refCounter == 0 && ! _isPrimary )
            {
                _state = MessageBrokerQueueListenerBindingState.Disposed;
                _deadLetterMessages = 0;
                _sentMessages = 0;
                return true;
            }
        }

        if ( ! isRedelivery && _sentMessages > 0 )
            _sentMessages = unchecked( _sentMessages - 1 );

        return false;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool TryDisposeByRemovingProcessedMessageUnsafe(bool isFromDeadLetter)
    {
        if ( _state == MessageBrokerQueueListenerBindingState.Disposed )
        {
            Assume.Equals( _refCounter, 0 );
            Assume.Equals( _deadLetterMessages, 0 );
            return false;
        }

        if ( _refCounter > 0 )
            _refCounter = unchecked( _refCounter - 1 );

        if ( _refCounter == 0 && ! _isPrimary )
        {
            _state = MessageBrokerQueueListenerBindingState.Disposed;
            _deadLetterMessages = 0;
            _sentMessages = 0;
            return true;
        }

        if ( isFromDeadLetter && _deadLetterMessages > 0 )
            _deadLetterMessages = unchecked( _deadLetterMessages - 1 );

        return false;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool TryDisposeByRemovingFilteredMessageUnsafe()
    {
        if ( _state == MessageBrokerQueueListenerBindingState.Disposed )
        {
            Assume.Equals( _refCounter, 0 );
            Assume.Equals( _sentMessages, 0 );
            return false;
        }

        if ( _refCounter > 0 )
            _refCounter = unchecked( _refCounter - 1 );

        if ( _refCounter == 0 && ! _isPrimary )
        {
            _state = MessageBrokerQueueListenerBindingState.Disposed;
            _deadLetterMessages = 0;
            _sentMessages = 0;
            return true;
        }

        if ( _sentMessages > 0 )
            _sentMessages = unchecked( _sentMessages - 1 );

        return false;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ExclusiveLock AcquireLock()
    {
        return ExclusiveLock.Enter( _sync );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool AreRetriesExceededUnsafe(int retry)
    {
        return retry >= _maxRetries;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool AreRedeliveriesExceededUnsafe(int redelivery)
    {
        return redelivery >= _maxRedeliveries;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool IsDeadLetterEnabledUnsafe()
    {
        return _deadLetterCapacityHint > 0;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal Duration GetRetryDelayUnsafe(Duration? @explicit)
    {
        return @explicit ?? _retryDelay;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal Duration GetMinDeadLetterRetentionUnsafe()
    {
        return _minDeadLetterRetention;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool AreAcksEnabledUnsafe(out Duration minAckTimeout)
    {
        minAckTimeout = _minAckTimeout;
        return _minAckTimeout > Duration.Zero;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal Storage.ListenerMetadata GetStorageMetadata()
    {
        using ( AcquireLock() )
        {
            var filter = TextEncoding.Prepare( _filterExpression ?? string.Empty ).GetValueOrThrow();
            return new Storage.ListenerMetadata(
                Queue.Id,
                unchecked( ( short )_prefetchHint ),
                _maxRetries,
                _retryDelay,
                _maxRedeliveries,
                _minAckTimeout,
                _deadLetterCapacityHint,
                _minDeadLetterRetention,
                filter );
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void SetProperties(
        int prefetchHint,
        int maxRetries,
        Duration retryDelay,
        int maxRedeliveries,
        Duration minAckTimeout,
        int deadLetterCapacityHint,
        Duration minDeadLetterRetention,
        string? filterExpression,
        Func<MessageBrokerFilterExpressionContext[], bool>? filterPredicate)
    {
        Assume.IsGreaterThan( prefetchHint, 0 );
        Assume.IsGreaterThanOrEqualTo( maxRetries, 0 );
        Assume.IsGreaterThanOrEqualTo( retryDelay, Duration.Zero );
        Assume.IsGreaterThanOrEqualTo( maxRedeliveries, 0 );
        Assume.IsGreaterThanOrEqualTo( minAckTimeout, Duration.Zero );
        Assume.IsGreaterThanOrEqualTo( deadLetterCapacityHint, 0 );
        Assume.IsGreaterThanOrEqualTo( minDeadLetterRetention, Duration.Zero );

        _prefetchHint = prefetchHint;
        _maxRetries = maxRetries;
        _retryDelay = retryDelay;
        _maxRedeliveries = maxRedeliveries;
        _minAckTimeout = minAckTimeout;
        _deadLetterCapacityHint = deadLetterCapacityHint;
        _minDeadLetterRetention = minDeadLetterRetention;
        _filterPredicate = filterPredicate;
        _filterExpression = filterExpression;
    }
}
