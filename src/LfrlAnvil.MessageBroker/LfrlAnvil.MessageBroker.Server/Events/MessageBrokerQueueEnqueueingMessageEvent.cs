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
/// Represents an event emitted by <see cref="MessageBrokerQueue"/> when starting to enqueue a message.
/// </summary>
public readonly struct MessageBrokerQueueEnqueueingMessageEvent
{
    private MessageBrokerQueueEnqueueingMessageEvent(
        MessageBrokerChannelListenerBinding listener,
        ulong traceId,
        MessageBrokerChannelPublisherBinding publisher,
        ulong messageId,
        int storeKey,
        int length)
    {
        Source = MessageBrokerQueueEventSource.Create( listener.Queue, traceId );
        Listener = listener;
        Publisher = publisher;
        MessageId = messageId;
        StoreKey = storeKey;
        Length = length;
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
    /// <see cref="MessageBrokerChannelPublisherBinding"/> that pushed the message.
    /// </summary>
    public MessageBrokerChannelPublisherBinding Publisher { get; }

    /// <summary>
    /// Unique id of the message.
    /// </summary>
    public ulong MessageId { get; }

    /// <summary>
    /// Key of the stream's message store entry associated with the message.
    /// </summary>
    public int StoreKey { get; }

    /// <summary>
    /// Message length.
    /// </summary>
    public int Length { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerQueueEnqueueingMessageEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return
            $"[EnqueueingMessage] {Source}, Channel = [{Listener.Channel.Id}] '{Listener.Channel.Name}', Sender = [{Publisher.Client.Id}] '{Publisher.Client.Name}', MessageId = {MessageId}, StoreKey = {StoreKey}, Length = {Length}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerQueueEnqueueingMessageEvent Create(
        MessageBrokerChannelListenerBinding listener,
        ulong traceId,
        MessageBrokerChannelPublisherBinding publisher,
        ulong messageId,
        int storeKey,
        int length)
    {
        return new MessageBrokerQueueEnqueueingMessageEvent( listener, traceId, publisher, messageId, storeKey, length );
    }
}
