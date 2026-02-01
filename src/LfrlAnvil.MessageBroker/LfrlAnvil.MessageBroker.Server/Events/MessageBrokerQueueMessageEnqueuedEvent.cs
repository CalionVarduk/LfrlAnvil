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
/// Represents an event emitted by <see cref="MessageBrokerQueue"/> after a message has been successfully enqueued.
/// </summary>
public readonly struct MessageBrokerQueueMessageEnqueuedEvent
{
    private MessageBrokerQueueMessageEnqueuedEvent(
        MessageBrokerChannelListenerBinding listener,
        ulong traceId,
        IMessageBrokerMessagePublisher publisher,
        ulong messageId)
    {
        Source = MessageBrokerQueueEventSource.Create( listener.Queue, traceId );
        Listener = listener;
        Publisher = publisher;
        MessageId = messageId;
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerQueueEventSource Source { get; }

    /// <summary>
    /// <see cref="MessageBrokerChannelPublisherBinding"/> that received the message.
    /// </summary>
    public MessageBrokerChannelListenerBinding Listener { get; }

    /// <summary>
    /// <see cref="IMessageBrokerMessagePublisher"/> that pushed the message.
    /// </summary>
    public IMessageBrokerMessagePublisher Publisher { get; }

    /// <summary>
    /// Unique id of the message.
    /// </summary>
    public ulong MessageId { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerQueueMessageEnqueuedEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return
            $"[MessageEnqueued] {Source}, Channel = [{Listener.Channel.Id}] '{Listener.Channel.Name}', Sender = [{Publisher.ClientId}] '{Publisher.ClientName}', MessageId = {MessageId}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerQueueMessageEnqueuedEvent Create(
        MessageBrokerChannelListenerBinding listener,
        ulong traceId,
        IMessageBrokerMessagePublisher publisher,
        ulong messageId)
    {
        return new MessageBrokerQueueMessageEnqueuedEvent( listener, traceId, publisher, messageId );
    }
}
