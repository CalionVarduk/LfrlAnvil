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
/// Represents an event emitted by <see cref="MessageBrokerStream"/> when starting to process a batch of channel messages.
/// </summary>
public readonly struct MessageBrokerStreamMessageProcessingEvent
{
    private MessageBrokerStreamMessageProcessingEvent(
        MessageBrokerStream stream,
        ulong traceId,
        MessageBrokerChannel channel,
        int messageCount)
    {
        Source = MessageBrokerStreamEventSource.Create( stream, traceId );
        Channel = channel;
        MessageCount = messageCount;
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerStreamEventSource Source { get; }

    /// <summary>
    /// <see cref="MessageBrokerChannel"/> related to the operation.
    /// </summary>
    public MessageBrokerChannel Channel { get; }

    /// <summary>
    /// Number of messages to process.
    /// </summary>
    public int MessageCount { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerStreamMessageProcessingEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[MessageProcessing] {Source}, Channel = [{Channel.Id}] '{Channel.Name}', MessageCount = {MessageCount}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerStreamMessageProcessingEvent Create(
        MessageBrokerStream stream,
        ulong traceId,
        MessageBrokerChannel channel,
        int messageCount)
    {
        return new MessageBrokerStreamMessageProcessingEvent( stream, traceId, channel, messageCount );
    }
}
