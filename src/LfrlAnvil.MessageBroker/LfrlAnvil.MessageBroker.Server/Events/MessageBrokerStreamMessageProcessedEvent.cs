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
/// Represents an event emitted by <see cref="MessageBrokerStream"/> after a message pushed from the publisher
/// has been handled successfully.
/// </summary>
public readonly struct MessageBrokerStreamMessageProcessedEvent
{
    private MessageBrokerStreamMessageProcessedEvent(
        MessageBrokerChannelPublisherBinding publisher,
        ulong traceId,
        ulong messageId,
        int length)
    {
        Source = MessageBrokerStreamEventSource.Create( publisher.Stream, traceId );
        Publisher = publisher;
        MessageId = messageId;
        Length = length;
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
    /// Returns a string representation of this <see cref="MessageBrokerStreamMessageProcessedEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return
            $"[MessageProcessed] {Source}, Client = [{Publisher.Client.Id}] '{Publisher.Client.Name}', Channel = [{Publisher.Channel.Id}] '{Publisher.Channel.Name}', MessageId = {MessageId}, Length = {Length}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerStreamMessageProcessedEvent Create(
        MessageBrokerChannelPublisherBinding publisher,
        ulong traceId,
        ulong messageId,
        int length)
    {
        return new MessageBrokerStreamMessageProcessedEvent( publisher, traceId, messageId, length );
    }
}
