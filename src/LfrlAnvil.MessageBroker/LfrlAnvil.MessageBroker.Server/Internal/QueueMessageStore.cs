// Copyright 2025-2026 Łukasz Furlepa
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
using LfrlAnvil.Chrono;
using LfrlAnvil.Extensions;
using LfrlAnvil.MessageBroker.Server.Events;
using LfrlAnvil.MessageBroker.Server.Exceptions;

namespace LfrlAnvil.MessageBroker.Server.Internal;

internal struct QueueMessageStore
{
    internal QueueSlim<QueueMessage> Pending;
    internal SparseListSlim<UnackedEntry> Unacked;
    internal QueueRetryHeap Retries;
    internal QueueSlim<DeadLetterEntry> DeadLetter;

    private QueueMessageStore(int capacity)
    {
        Pending = QueueSlim<QueueMessage>.Create( capacity );
        Unacked = SparseListSlim<UnackedEntry>.Create();
        Retries = QueueRetryHeap.Create();
        DeadLetter = QueueSlim<DeadLetterEntry>.Create();
    }

    internal bool IsEmpty => Pending.IsEmpty && Unacked.IsEmpty && Retries.IsEmpty && DeadLetter.IsEmpty;

    [Pure]
    internal static QueueMessageStore Create()
    {
        return new QueueMessageStore( 0 );
    }

    internal bool TryPeekNext(
        Timestamp now,
        ref ListSlim<DiscardedMessage> discarded,
        ref QueueMessage message,
        ref MessageType messageType,
        ref int retry,
        ref int lastRedelivery)
    {
        Assume.True( discarded.IsEmpty );
        Assume.IsGreaterThan( discarded.Capacity, 0 );

        var unackedNode = Unacked.First;
        while ( unackedNode is not null )
        {
            ref var entry = ref unackedNode.Value.Value;
            if ( entry.ExpiresAt > now )
                break;

            if ( entry.Message.Listener.CanSendRedelivery( out var disposed ) )
            {
                message = entry.Message;
                messageType = MessageType.Redelivery;
                retry = entry.Retry;
                lastRedelivery = entry.Redelivery;
                return true;
            }

            if ( ! disposed )
                break;

            discarded.Add(
                new DiscardedMessage(
                    entry.Message,
                    entry.Retry,
                    entry.Redelivery,
                    MessageBrokerQueueDiscardMessageReason.DisposedUnacked ) );

            var nextNode = unackedNode.Value.Next;
            Unacked.Remove( unackedNode.Value.Index );
            unackedNode = nextNode;
            if ( discarded.Count >= discarded.Capacity )
                return false;
        }

        while ( ! Retries.IsEmpty )
        {
            ref var entry = ref Retries.First();
            if ( entry.SendAt > now )
                break;

            var discardReason = MessageBrokerQueueDiscardMessageReason.DisposedRetry;
            if ( entry.Retry <= entry.Message.Listener.MaxRetries )
            {
                if ( entry.Message.Listener.TryIncrementPrefetchCounter( out var disposed ) )
                {
                    message = entry.Message;
                    messageType = MessageType.Retry;
                    retry = entry.Retry;
                    lastRedelivery = entry.Redelivery;
                    return true;
                }

                if ( ! disposed )
                    break;
            }
            else
                discardReason = MessageBrokerQueueDiscardMessageReason.MaxRetriesReached;

            discarded.Add( new DiscardedMessage( entry.Message, entry.Retry, entry.Redelivery, discardReason ) );
            Retries.Pop();
            if ( discarded.Count >= discarded.Capacity )
                return false;
        }

        while ( ! Pending.IsEmpty )
        {
            ref var next = ref Pending.First();
            if ( next.Listener.TryIncrementPrefetchCounter( out var disposed ) )
            {
                message = next;
                messageType = MessageType.Pending;
                retry = 0;
                lastRedelivery = 0;
                return true;
            }

            if ( ! disposed )
                break;

            discarded.Add( new DiscardedMessage( next, 0, 0, MessageBrokerQueueDiscardMessageReason.DisposedPending ) );
            Pending.Dequeue();
            if ( discarded.Count >= discarded.Capacity )
                break;
        }

        while ( ! DeadLetter.IsEmpty )
        {
            var discardReason = MessageBrokerQueueDiscardMessageReason.DisposedDeadLetter;
            ref var next = ref DeadLetter.First();
            if ( next.ExpiresAt > now )
            {
                if ( next.Message.Listener.Queue.DeadLetterQueryCounter > 0 )
                {
                    if ( next.Message.Listener.TryIncrementPrefetchCounter( out var disposed ) )
                    {
                        message = next.Message;
                        messageType = MessageType.DeadLetter;
                        retry = next.Retry;
                        lastRedelivery = next.Redelivery;
                        return true;
                    }

                    if ( ! disposed )
                        break;
                }
                else if ( next.Message.Listener.TryDecrementDeadLetterCounterIfExceeded( out var disposed ) )
                    discardReason = MessageBrokerQueueDiscardMessageReason.DeadLetterCapacityExceeded;
                else if ( ! disposed )
                    break;
            }
            else if ( next.Message.Listener.DecrementDeadLetterCounter( out var disposed ) )
                discardReason = MessageBrokerQueueDiscardMessageReason.DeadLetterExpiration;
            else if ( ! disposed )
                break;

            if ( next.Message.Listener.Queue.DeadLetterQueryCounter > 0 )
                next.Message.Listener.Queue.DeadLetterQueryCounter = unchecked( next.Message.Listener.Queue.DeadLetterQueryCounter - 1 );

            discarded.Add( new DiscardedMessage( next.Message, next.Retry, next.Redelivery, discardReason ) );
            DeadLetter.Dequeue();
            if ( discarded.Count >= discarded.Capacity )
                break;
        }

        return false;
    }

    internal void Enqueue(IMessageBrokerMessagePublisher publisher, MessageBrokerChannelListenerBinding listener, int storeKey)
    {
        Pending.Enqueue( new QueueMessage( publisher, listener, storeKey ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void Dequeue(MessageType type)
    {
        switch ( type )
        {
            case MessageType.Redelivery:
                Assume.IsNotNull( Unacked.First );
                Unacked.Remove( Unacked.First.Value.Index );
                break;

            case MessageType.Retry:
                Retries.Pop();
                break;

            case MessageType.DeadLetter:
                if ( ! DeadLetter.IsEmpty )
                {
                    ref var entry = ref DeadLetter.First();
                    var listener = entry.Message.Listener;
                    listener.DecrementDeadLetterCounter( out _ );

                    if ( listener.Queue.DeadLetterQueryCounter > 0 )
                        listener.Queue.DeadLetterQueryCounter = unchecked( listener.Queue.DeadLetterQueryCounter - 1 );

                    DeadLetter.Dequeue();
                }

                break;

            default:
                Pending.Dequeue();
                break;
        }
    }

    [Pure]
    internal Timestamp GetNextEventTimestamp()
    {
        var result = TimeoutEntry.MaxTimestamp;
        var unackedNode = Unacked.First;
        if ( unackedNode is not null )
        {
            ref var entry = ref unackedNode.Value.Value;
            if ( entry.Message.Listener.CanConsumeUnackedOrExpiredDeadLetter() )
                result = entry.ExpiresAt;
        }

        if ( ! Retries.IsEmpty )
        {
            ref var entry = ref Retries.First();
            if ( entry.Message.Listener.CanConsumeRetry() )
                result = result.Min( entry.SendAt );
        }

        if ( ! DeadLetter.IsEmpty )
        {
            ref var entry = ref DeadLetter.First();
            if ( entry.Message.Listener.CanConsumeUnackedOrExpiredDeadLetter() )
                result = result.Min( entry.ExpiresAt );
        }

        return result;
    }

    [Pure]
    internal ref UnackedEntry GetUnackedRef(int ackId)
    {
        return ref Unacked[ackId - 1];
    }

    internal int AddUnacked(QueueMessage message, ulong messageId, int retry, int redelivery, out Timestamp expiresAt)
    {
        Assume.IsGreaterThanOrEqualTo( retry, 0 );
        Assume.IsGreaterThanOrEqualTo( redelivery, 0 );

        expiresAt = message.Listener.Client.GetTimestamp() + message.Listener.MinAckTimeout;
        var last = Unacked.Last;
        if ( last is not null )
        {
            ref var lastEntry = ref last.Value.Value;
            if ( lastEntry.ExpiresAt > expiresAt )
                expiresAt = lastEntry.ExpiresAt;
        }

        return Unacked.Add( new UnackedEntry( message, messageId, retry, redelivery, expiresAt ) ) + 1;
    }

    internal void ScheduleRetry(QueueMessage message, int retry, int redelivery, Duration delay)
    {
        Retries.Add( message, retry, redelivery, delay );
    }

    internal void AddToDeadLetter(QueueMessage message, int retry, int redelivery, Timestamp expiresAt)
    {
        Assume.IsGreaterThanOrEqualTo( retry, 0 );
        Assume.IsGreaterThanOrEqualTo( redelivery, 0 );

        if ( ! DeadLetter.IsEmpty )
        {
            ref var last = ref DeadLetter.Last();
            if ( last.ExpiresAt > expiresAt )
                expiresAt = last.ExpiresAt;
        }

        DeadLetter.Enqueue( new DeadLetterEntry( message, expiresAt, retry, redelivery ) );
    }

    internal void AddToDeadLetter(QueueMessage message, int retry, int redelivery)
    {
        Assume.IsGreaterThanOrEqualTo( retry, 0 );
        Assume.IsGreaterThanOrEqualTo( redelivery, 0 );

        var expiresAt = message.Listener.Client.GetTimestamp() + message.Listener.MinDeadLetterRetention;
        if ( ! DeadLetter.IsEmpty )
        {
            ref var last = ref DeadLetter.Last();
            if ( last.ExpiresAt > expiresAt )
                expiresAt = last.ExpiresAt;
        }

        DeadLetter.Enqueue( new DeadLetterEntry( message, expiresAt, retry, redelivery ) );
    }

    internal void RemoveUnacked(int ackId)
    {
        Unacked.Remove( ackId - 1 );
    }

    internal void ExpireAllUnacked(Timestamp now)
    {
        var node = Unacked.First;
        while ( node is not null )
        {
            ref var entry = ref node.Value.Value;
            entry = new UnackedEntry( entry.Message, entry.MessageId, entry.Retry, entry.Redelivery, now );
            node = node.Value.Next;
        }
    }

    internal static void ReleaseMessages(
        MessageBrokerQueue queue,
        ulong traceId,
        bool extractPersistentMessages,
        bool discardAllMessages,
        Dictionary<int, MessageBrokerChannelListenerBinding>? listenersByChannelId,
        out ListSlim<QueueMessage> pendingMessages,
        out ListSlim<UnackedEntry> unackedEntries,
        out ListSlim<QueueRetryHeap.Entry> retryEntries,
        out ListSlim<DeadLetterEntry> deadLetterEntries)
    {
        var exceptions = Chain<Exception>.Empty;
        pendingMessages = ListSlim<QueueMessage>.Create();
        unackedEntries = ListSlim<UnackedEntry>.Create();
        retryEntries = ListSlim<QueueRetryHeap.Entry>.Create();
        deadLetterEntries = ListSlim<DeadLetterEntry>.Create();
        var releasedMessages = ListSlim<QueueMessage>.Create();

        using ( queue.AcquireLock() )
        {
            if ( extractPersistentMessages )
                pendingMessages.ResetCapacity( queue.MessageStore.Pending.Count );

            foreach ( ref readonly var m in queue.MessageStore.Pending )
            {
                try
                {
                    if ( discardAllMessages || ! IsPersistent( m.Listener, listenersByChannelId ) )
                        releasedMessages.Add( m );
                    else if ( extractPersistentMessages )
                        pendingMessages.Add( m );
                }
                catch ( Exception exc )
                {
                    exceptions = exceptions.Extend( exc );
                }
            }

            if ( releasedMessages.Count > 0 )
                exceptions = exceptions.Extend( queue.Exception( Resources.QueueMessagesDiscarded( releasedMessages.Count ) ) );

            if ( extractPersistentMessages )
                unackedEntries.ResetCapacity( queue.MessageStore.Unacked.Count );

            foreach ( var (_, m) in queue.MessageStore.Unacked )
            {
                try
                {
                    if ( discardAllMessages || ! IsPersistent( m.Message.Listener, listenersByChannelId ) )
                        releasedMessages.Add( m.Message );
                    else if ( extractPersistentMessages )
                        unackedEntries.Add( m );
                }
                catch ( Exception exc )
                {
                    exceptions = exceptions.Extend( exc );
                }
            }

            if ( extractPersistentMessages )
                retryEntries.ResetCapacity( queue.MessageStore.Retries.Count );

            for ( var i = 0; i < queue.MessageStore.Retries.Count; ++i )
            {
                ref var entry = ref queue.MessageStore.Retries[i];
                try
                {
                    if ( discardAllMessages || ! IsPersistent( entry.Message.Listener, listenersByChannelId ) )
                        releasedMessages.Add( entry.Message );
                    else if ( extractPersistentMessages )
                        retryEntries.Add( entry );
                }
                catch ( Exception exc )
                {
                    exceptions = exceptions.Extend( exc );
                }
            }

            if ( extractPersistentMessages )
                deadLetterEntries.ResetCapacity( queue.MessageStore.DeadLetter.Count );

            foreach ( ref readonly var m in queue.MessageStore.DeadLetter )
            {
                try
                {
                    if ( discardAllMessages || ! IsPersistent( m.Message.Listener, listenersByChannelId ) )
                        releasedMessages.Add( m.Message );
                    else if ( extractPersistentMessages )
                        deadLetterEntries.Add( m );
                }
                catch ( Exception exc )
                {
                    exceptions = exceptions.Extend( exc );
                }
            }
        }

        foreach ( ref readonly var m in releasedMessages )
        {
            try
            {
                queue.RemoveFromStreamMessageStore( m, traceId );
            }
            catch ( Exception exc )
            {
                exceptions = exceptions.Extend( exc );
            }
        }

        queue.EmitErrors( ref exceptions, traceId );
    }

    internal int Clear()
    {
        var discardedMessageCount = Pending.Count;
        Pending = QueueSlim<QueueMessage>.Create();
        Unacked = SparseListSlim<UnackedEntry>.Create();
        Retries.Clear();
        DeadLetter = QueueSlim<DeadLetterEntry>.Create();
        return discardedMessageCount;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static bool IsPersistent(
        MessageBrokerChannelListenerBinding listener,
        Dictionary<int, MessageBrokerChannelListenerBinding>? listenersByChannelId)
    {
        return ! listener.IsEphemeral
            && listenersByChannelId is not null
            && listenersByChannelId.TryGetValue( listener.Channel.Id, out var current )
            && ReferenceEquals( listener, current );
    }

    internal enum MessageType : byte
    {
        Pending = 0,
        Redelivery = 1,
        Retry = 2,
        DeadLetter = 3
    }

    internal readonly struct DiscardedMessage
    {
        internal DiscardedMessage(QueueMessage message, int retry, int redelivery, MessageBrokerQueueDiscardMessageReason reason)
        {
            Publisher = message.Publisher;
            Listener = message.Listener;
            StoreKey = message.StoreKey;
            Retry = retry;
            Redelivery = redelivery;
            Reason = reason;
        }

        internal readonly IMessageBrokerMessagePublisher Publisher;
        internal readonly MessageBrokerChannelListenerBinding Listener;
        internal readonly int StoreKey;
        internal readonly int Retry;
        internal readonly int Redelivery;
        internal readonly MessageBrokerQueueDiscardMessageReason Reason;

        [Pure]
        public override string ToString()
        {
            return
                $"Publisher = ({Publisher}), Listener = ({Listener}), StoreKey = {StoreKey}, Retry = {Retry}, Redelivery = {Redelivery}, Reason = {Reason}";
        }
    }

    internal readonly struct UnackedEntry
    {
        internal UnackedEntry(QueueMessage message, ulong messageId, int retry, int redelivery, Timestamp expiresAt)
        {
            Message = message;
            MessageId = messageId;
            Retry = retry;
            Redelivery = redelivery;
            ExpiresAt = expiresAt;
        }

        internal readonly QueueMessage Message;
        internal readonly ulong MessageId;
        internal readonly int Retry;
        internal readonly int Redelivery;
        internal readonly Timestamp ExpiresAt;

        [Pure]
        public override string ToString()
        {
            return $"Message = ({Message}), MessageId = {MessageId}, Retry = {Retry}, Redelivery = {Redelivery}, ExpiresAt = {ExpiresAt}";
        }
    }

    internal readonly struct DeadLetterEntry
    {
        internal DeadLetterEntry(QueueMessage message, Timestamp expiresAt, int retry, int redelivery)
        {
            Message = message;
            ExpiresAt = expiresAt;
            Retry = retry;
            Redelivery = redelivery;
        }

        internal readonly QueueMessage Message;
        internal readonly Timestamp ExpiresAt;
        internal readonly int Retry;
        internal readonly int Redelivery;

        [Pure]
        public override string ToString()
        {
            return $"Message = ({Message}), Retry = {Retry}, Redelivery = {Redelivery}, ExpiresAt = {ExpiresAt}";
        }
    }
}
