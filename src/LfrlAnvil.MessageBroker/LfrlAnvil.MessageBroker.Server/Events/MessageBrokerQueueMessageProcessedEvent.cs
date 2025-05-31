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

using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.MessageBroker.Server.Events;

/// <summary>
/// Represents an event emitted by <see cref="MessageBrokerQueue"/> after an enqueued message has been processed.
/// </summary>
public readonly struct MessageBrokerQueueMessageProcessedEvent
{
    private MessageBrokerQueueMessageProcessedEvent(
        MessageBrokerChannelListenerBinding listener,
        ulong traceId,
        MessageBrokerChannelPublisherBinding publisher,
        ulong messageId,
        int length)
    {
        Source = MessageBrokerQueueEventSource.Create( listener.Queue, traceId );
        Publisher = publisher;
        Listener = listener;
        MessageId = messageId;
        Length = length;
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerQueueEventSource Source { get; }

    /// <summary>
    /// <see cref="MessageBrokerChannelPublisherBinding"/> that pushed the message.
    /// </summary>
    public MessageBrokerChannelPublisherBinding Publisher { get; }

    /// <summary>
    /// <see cref="MessageBrokerChannelPublisherBinding"/> that handled the message.
    /// </summary>
    public MessageBrokerChannelListenerBinding Listener { get; }

    /// <summary>
    /// Unique id of the message.
    /// </summary>
    public ulong MessageId { get; }

    /// <summary>
    /// Message length.
    /// </summary>
    public int Length { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerQueueMessageProcessedEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return
            $"[MessageProcessed] {Source}, Sender = [{Publisher.Client.Id}] '{Publisher.Client.Name}', Channel = [{Publisher.Channel.Id}] '{Publisher.Channel.Name}', Stream = [{Publisher.Stream.Id}] '{Publisher.Stream.Name}', MessageId = {MessageId}, Length = {Length}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerQueueMessageProcessedEvent Create(
        MessageBrokerChannelListenerBinding listener,
        ulong traceId,
        MessageBrokerChannelPublisherBinding publisher,
        ulong messageId,
        int length)
    {
        return new MessageBrokerQueueMessageProcessedEvent( listener, traceId, publisher, messageId, length );
    }
}
