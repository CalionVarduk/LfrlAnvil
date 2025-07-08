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
/// Represents an event emitted by <see cref="MessageBrokerStream"/> when starting to process a message.
/// </summary>
public readonly struct MessageBrokerStreamProcessingMessageEvent
{
    private MessageBrokerStreamProcessingMessageEvent(
        MessageBrokerChannelPublisherBinding publisher,
        ulong traceId,
        ulong messageId,
        int length,
        bool hasRouting,
        ReadOnlyArray<MessageBrokerChannelListenerBinding> listeners)
    {
        Source = MessageBrokerStreamEventSource.Create( publisher.Stream, traceId );
        Publisher = publisher;
        MessageId = messageId;
        Length = length;
        HasRouting = hasRouting;
        Listeners = listeners;
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerStreamEventSource Source { get; }

    /// <summary>
    /// <see cref="MessageBrokerChannelPublisherBinding"/> that pushed the message.
    /// </summary>
    public MessageBrokerChannelPublisherBinding Publisher { get; }

    /// <summary>
    /// Unique id of the message.
    /// </summary>
    public ulong MessageId { get; }

    /// <summary>
    /// Message length.
    /// </summary>
    public int Length { get; }

    /// <summary>
    /// Specifies whether or not this message has explicit routing.
    /// </summary>
    public bool HasRouting { get; }

    /// <summary>
    /// Collection of <see cref="MessageBrokerChannelListenerBinding"/> that will receive the message.
    /// </summary>
    public ReadOnlyArray<MessageBrokerChannelListenerBinding> Listeners { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerStreamProcessingMessageEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return
            $"[ProcessingMessage] {Source}, Channel = [{Publisher.Channel.Id}] '{Publisher.Channel.Name}', Sender = [{Publisher.Client.Id}] '{Publisher.Client.Name}', MessageId = {MessageId}, Length = {Length}, HasRouting = {HasRouting}, ListenerCount = {Listeners.Count}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerStreamProcessingMessageEvent Create(
        MessageBrokerChannelPublisherBinding publisher,
        ulong traceId,
        ulong messageId,
        int length,
        bool hasRouting,
        ReadOnlyArray<MessageBrokerChannelListenerBinding> listeners)
    {
        return new MessageBrokerStreamProcessingMessageEvent( publisher, traceId, messageId, length, hasRouting, listeners );
    }
}
