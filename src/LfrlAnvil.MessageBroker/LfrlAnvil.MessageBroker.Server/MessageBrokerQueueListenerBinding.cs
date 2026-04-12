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
using LfrlAnvil.MessageBroker.Server.Events;
using LfrlAnvil.MessageBroker.Server.Internal;

namespace LfrlAnvil.MessageBroker.Server;

/// <summary>
/// Represents a message broker queue binding for a listener, which represents pairing between a channel listener binding and a queue.
/// </summary>
public sealed class MessageBrokerQueueListenerBinding
{
    private readonly object _sync = new object();
    private MessageBrokerFilterExpressionContext[] _filterPredicateArgs = Array.Empty<MessageBrokerFilterExpressionContext>();
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
    internal bool FilterMessage(in StreamMessage message, ulong queueTraceId)
    {
        MessageBrokerFilterExpressionContext[] args;
        Func<MessageBrokerFilterExpressionContext[], bool> filterPredicate;
        using ( AcquireLock() )
        {
            if ( _filterExpression is null )
                return true;

            Assume.IsNotNull( _filterPredicate );
            Assume.Equals( _filterPredicateArgs.Length, 1 );
            Assume.Equals( _filterPredicateArgs[0], default );
            args = _filterPredicateArgs;
            filterPredicate = _filterPredicate;
            args[0] = new MessageBrokerFilterExpressionContext( this, in message );
        }

        try
        {
            return filterPredicate( args );
        }
        catch ( Exception exc )
        {
            if ( Queue.Logger.Error is { } error )
                error.Emit( MessageBrokerQueueErrorEvent.Create( Queue, queueTraceId, exc ) );

            return true;
        }
        finally
        {
            using ( AcquireLock() )
                args[0] = default;
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
    internal void Reactivate(
        in Protocol.BindListenerRequestHeader header,
        string? filterExpression,
        IParsedExpressionDelegate<MessageBrokerFilterExpressionContext, bool>? filterExpressionDelegate)
    {
        using ( AcquireLock() )
        {
            if ( _state != MessageBrokerQueueListenerBindingState.Inactive )
                return;

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

            _state = MessageBrokerQueueListenerBindingState.Created;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool CanSendMessage(out bool disposed)
    {
        using ( AcquireLock() )
        {
            if ( _state >= MessageBrokerQueueListenerBindingState.Inactive )
            {
                disposed = _state == MessageBrokerQueueListenerBindingState.Disposed;
                return false;
            }

            disposed = false;
            if ( _sentMessages >= PrefetchHint )
                return false;

            _sentMessages = unchecked( _sentMessages + 1 );
            return true;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool CanSendRedelivery(out bool disposed)
    {
        using ( AcquireLock() )
        {
            if ( _state >= MessageBrokerQueueListenerBindingState.Inactive )
            {
                disposed = _state == MessageBrokerQueueListenerBindingState.Disposed;
                return false;
            }

            disposed = false;
            return true;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void AddInactiveSentMessage()
    {
        using ( AcquireLock() )
        {
            if ( _state == MessageBrokerQueueListenerBindingState.Inactive )
                _sentMessages = checked( _sentMessages + 1 );
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool RemoveSentMessage()
    {
        using ( AcquireLock() )
        {
            var old = _sentMessages;
            if ( old <= 0 )
                return true;

            _sentMessages = unchecked( _sentMessages - 1 );
            return old >= PrefetchHint;
        }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool CanScheduleRetry()
    {
        using ( AcquireLock() )
            return _state != MessageBrokerQueueListenerBindingState.Inactive && _sentMessages < PrefetchHint;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool CanScheduleRedelivery()
    {
        using ( AcquireLock() )
            return _state != MessageBrokerQueueListenerBindingState.Inactive;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool AddDeadLetterMessage()
    {
        using ( AcquireLock() )
        {
            if ( _state == MessageBrokerQueueListenerBindingState.Disposed )
                return true;

            var old = _deadLetterMessages;
            _deadLetterMessages = checked( _deadLetterMessages + 1 );
            return old >= DeadLetterCapacityHint;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void AddInactiveDeadLetterMessage()
    {
        using ( AcquireLock() )
        {
            if ( _state == MessageBrokerQueueListenerBindingState.Inactive )
                _deadLetterMessages = checked( _deadLetterMessages + 1 );
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool RemoveDeadLetterMessage(out bool disposed)
    {
        using ( AcquireLock() )
        {
            if ( _state == MessageBrokerQueueListenerBindingState.Disposed )
            {
                disposed = true;
                return false;
            }

            disposed = false;
            if ( _deadLetterMessages > 0 )
                _deadLetterMessages = unchecked( _deadLetterMessages - 1 );

            return true;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool RemoveDeadLetterMessageIfCapacityExceeded(out bool disposed)
    {
        using ( AcquireLock() )
        {
            if ( _state >= MessageBrokerQueueListenerBindingState.Inactive )
            {
                disposed = _state == MessageBrokerQueueListenerBindingState.Disposed;
                return false;
            }

            disposed = false;
            if ( _deadLetterMessages <= DeadLetterCapacityHint )
                return false;

            _deadLetterMessages = unchecked( _deadLetterMessages - 1 );
            return true;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void AddReferencingMessage()
    {
        using ( AcquireLock() )
        {
            if ( _state != MessageBrokerQueueListenerBindingState.Disposed )
                _refCounter = unchecked( _refCounter + 1 );
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool RemoveReferencingMessage()
    {
        using ( AcquireLock() )
        {
            if ( _refCounter <= 0 )
                return false;

            Assume.NotEquals( _state, MessageBrokerQueueListenerBindingState.Disposed );
            _refCounter = unchecked( _refCounter - 1 );
            if ( _refCounter == 0 && ! _isPrimary )
            {
                _state = MessageBrokerQueueListenerBindingState.Disposed;
                _deadLetterMessages = 0;
                _prefetchHint = 0;
                return true;
            }

            return false;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void SetProperties(
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
        _prefetchHint = prefetchHint;
        _maxRetries = maxRetries;
        _retryDelay = retryDelay;
        _maxRedeliveries = maxRedeliveries;
        _minAckTimeout = minAckTimeout;
        _deadLetterCapacityHint = deadLetterCapacityHint;
        _minDeadLetterRetention = minDeadLetterRetention;
        _filterPredicateArgs = filterExpression is not null ? [ default ] : Array.Empty<MessageBrokerFilterExpressionContext>();
        _filterPredicate = filterPredicate;
        _filterExpression = filterExpression;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ExclusiveLock AcquireLock()
    {
        return ExclusiveLock.Enter( _sync );
    }
}
