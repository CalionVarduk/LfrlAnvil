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

using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.MessageBroker.Server.Events;

/// <summary>
/// Represents an event emitted by <see cref="MessageBrokerQueue"/> when an enqueued message was discarded due to disposed listener.
/// </summary>
public readonly struct MessageBrokerQueueMessageDiscardedEvent
{
    private MessageBrokerQueueMessageDiscardedEvent(
        MessageBrokerChannelListenerBinding listener,
        ulong traceId,
        IMessageBrokerMessagePublisher publisher,
        int storeKey,
        int retry,
        int redelivery,
        bool messageRemoved,
        bool movedToDeadLetter,
        MessageBrokerQueueDiscardMessageReason reason)
    {
        Source = MessageBrokerQueueEventSource.Create( listener.Queue, traceId );
        Listener = listener;
        Publisher = publisher;
        StoreKey = storeKey;
        Retry = retry;
        Redelivery = redelivery;
        MessageRemoved = messageRemoved;
        MovedToDeadLetter = movedToDeadLetter;
        Reason = reason;
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerQueueEventSource Source { get; }

    /// <summary>
    /// <see cref="MessageBrokerChannelListenerBinding"/> that received this message.
    /// </summary>
    public MessageBrokerChannelListenerBinding Listener { get; }

    /// <summary>
    /// <see cref="IMessageBrokerMessagePublisher"/> that pushed this message.
    /// </summary>
    public IMessageBrokerMessagePublisher Publisher { get; }

    /// <summary>
    /// Key of the stream's message store entry associated with this message.
    /// </summary>
    public int StoreKey { get; }

    /// <summary>
    /// Retry attempt of this message.
    /// </summary>
    public int Retry { get; }

    /// <summary>
    /// Redelivery attempt of this message.
    /// </summary>
    public int Redelivery { get; }

    /// <summary>
    /// Specifies whether or not the data of this message has been removed from the stream's message store
    /// due to no longer being referenced.
    /// </summary>
    public bool MessageRemoved { get; }

    /// <summary>
    /// Specifies whether or not the message has been moved to dead letter.
    /// </summary>
    public bool MovedToDeadLetter { get; }

    /// <summary>
    /// Specifies the reason behind discarding of this message.
    /// </summary>
    /// <remarks>See <see cref="MessageBrokerQueueDiscardMessageReason"/> for more information.</remarks>
    public MessageBrokerQueueDiscardMessageReason Reason { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerQueueMessageDiscardedEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return
            $"[MessageDiscarded] {Source}, Sender = [{Publisher.ClientId}] '{Publisher.ClientName}', Channel = [{Listener.Channel.Id}] '{Listener.Channel.Name}', Stream = [{Publisher.Stream.Id}] '{Publisher.Stream.Name}', Reason = {Reason}, StoreKey = {StoreKey}, Retry = {Retry}, Redelivery = {Redelivery}, MessageRemoved = {MessageRemoved}, MovedToDeadLetter = {MovedToDeadLetter}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerQueueMessageDiscardedEvent Create(
        MessageBrokerChannelListenerBinding listener,
        ulong traceId,
        IMessageBrokerMessagePublisher publisher,
        int storeKey,
        int retry,
        int redelivery,
        bool messageRemoved,
        bool movedToDeadLetter,
        MessageBrokerQueueDiscardMessageReason reason)
    {
        return new MessageBrokerQueueMessageDiscardedEvent(
            listener,
            traceId,
            publisher,
            storeKey,
            retry,
            redelivery,
            messageRemoved,
            movedToDeadLetter,
            reason );
    }
}
