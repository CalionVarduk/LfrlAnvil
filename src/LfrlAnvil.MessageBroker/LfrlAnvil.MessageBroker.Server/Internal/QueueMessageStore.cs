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
using LfrlAnvil.Chrono;
using LfrlAnvil.Extensions;
using LfrlAnvil.MessageBroker.Server.Events;

namespace LfrlAnvil.MessageBroker.Server.Internal;

internal struct QueueMessageStore
{
    internal QueueSlim<QueueMessage> Pending;
    internal SparseListSlim<UnackedEntry> Unacked;
    internal QueueRetryHeap Retries;

    private QueueMessageStore(int capacity)
    {
        Pending = QueueSlim<QueueMessage>.Create( capacity );
        Unacked = SparseListSlim<UnackedEntry>.Create();
        Retries = QueueRetryHeap.Create();
    }

    internal bool IsEmpty => Pending.IsEmpty && Unacked.IsEmpty && Retries.IsEmpty;

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

            if ( entry.Message.Listener.CanSendRedelivery() )
            {
                message = entry.Message;
                messageType = MessageType.Redelivery;
                retry = entry.Retry;
                lastRedelivery = entry.Redelivery;
                return true;
            }

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

            discarded.Add(
                new DiscardedMessage(
                    entry.Message,
                    entry.Retry,
                    entry.Redelivery,
                    MessageBrokerQueueDiscardMessageReason.DisposedRetry ) );

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

        return false;
    }

    internal void Enqueue(MessageBrokerChannelPublisherBinding publisher, MessageBrokerChannelListenerBinding listener, int storeKey)
    {
        Pending.Enqueue( new QueueMessage( publisher, listener, storeKey ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void Dequeue(MessageType type)
    {
        switch ( type )
        {
            case MessageType.Retry:
                Retries.Pop();
                break;

            case MessageType.Redelivery:
                Assume.IsNotNull( Unacked.First );
                Unacked.Remove( Unacked.First.Value.Index );
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
            result = entry.ExpiresAt;
        }

        if ( ! Retries.IsEmpty )
        {
            ref var entry = ref Retries.First();
            if ( entry.Message.Listener.CanConsumePrefetchCounter() )
                result = result.Min( entry.SendAt );
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

    internal void RemoveUnacked(int ackId)
    {
        Unacked.Remove( ackId - 1 );
    }

    internal static int ClearAndRelease(MessageBrokerQueue queue, ulong traceId)
    {
        int discardedMessageCount;
        using ( queue.AcquireLock() )
            discardedMessageCount = queue.MessageStore.Pending.Count;

        for ( var i = 0; i < discardedMessageCount; ++i )
        {
            try
            {
                QueueMessage message;
                using ( queue.AcquireLock() )
                    message = queue.MessageStore.Pending[i];

                queue.RemoveFromStreamMessageStore( message, traceId );
            }
            catch ( Exception exc )
            {
                if ( queue.Logger.Error is { } error )
                    error.Emit( MessageBrokerQueueErrorEvent.Create( queue, traceId, exc ) );
            }
        }

        int unackedCount;
        using ( queue.AcquireLock() )
        {
            unackedCount = queue.MessageStore.Unacked.Count;
            queue.MessageStore.Pending = QueueSlim<QueueMessage>.Create();
        }

        var j = 0;
        while ( unackedCount > 0 )
        {
            try
            {
                QueueMessage message;
                using ( queue.AcquireLock() )
                {
                    ref var entry = ref queue.MessageStore.Unacked[j++];
                    if ( Unsafe.IsNullRef( ref entry ) )
                        continue;

                    message = entry.Message;
                }

                --unackedCount;
                queue.RemoveFromStreamMessageStore( message, traceId );
            }
            catch ( Exception exc )
            {
                if ( queue.Logger.Error is { } error )
                    error.Emit( MessageBrokerQueueErrorEvent.Create( queue, traceId, exc ) );
            }
        }

        int retryCount;
        using ( queue.AcquireLock() )
        {
            retryCount = queue.MessageStore.Retries.Count;
            queue.MessageStore.Unacked = SparseListSlim<UnackedEntry>.Create();
        }

        for ( var i = 0; i < retryCount; ++i )
        {
            try
            {
                QueueMessage message;
                using ( queue.AcquireLock() )
                {
                    ref var entry = ref queue.MessageStore.Retries[i];
                    message = entry.Message;
                }

                queue.RemoveFromStreamMessageStore( message, traceId );
            }
            catch ( Exception exc )
            {
                if ( queue.Logger.Error is { } error )
                    error.Emit( MessageBrokerQueueErrorEvent.Create( queue, traceId, exc ) );
            }
        }

        using ( queue.AcquireLock() )
            queue.MessageStore.Retries.Clear();

        return discardedMessageCount;
    }

    internal int Clear()
    {
        var discardedMessageCount = Pending.Count;
        Pending = QueueSlim<QueueMessage>.Create();
        Unacked = SparseListSlim<UnackedEntry>.Create();
        Retries.Clear();
        return discardedMessageCount;
    }

    internal enum MessageType : byte
    {
        Pending = 0,
        Redelivery = 1,
        Retry = 2
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

        internal readonly MessageBrokerChannelPublisherBinding Publisher;
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
}
