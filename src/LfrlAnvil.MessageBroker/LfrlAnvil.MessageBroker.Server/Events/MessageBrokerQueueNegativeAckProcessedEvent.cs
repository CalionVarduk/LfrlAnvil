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
using LfrlAnvil.Chrono;

namespace LfrlAnvil.MessageBroker.Server.Events;

/// <summary>
/// Represents an event emitted by <see cref="MessageBrokerQueue"/> after a negative message notification ACK has been processed.
/// </summary>
public readonly struct MessageBrokerQueueNegativeAckProcessedEvent
{
    private readonly Duration _delay;

    private MessageBrokerQueueNegativeAckProcessedEvent(
        MessageBrokerQueue queue,
        ulong traceId,
        int ackId,
        Duration delay,
        bool messageDataRemoved)
    {
        _delay = delay;
        Source = MessageBrokerQueueEventSource.Create( queue, traceId );
        AckId = ackId;
        MessageDataRemoved = messageDataRemoved;
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerQueueEventSource Source { get; }

    /// <summary>
    /// Id of the processed ACK.
    /// </summary>
    public int AckId { get; }

    /// <summary>
    /// Specifies whether or not the data of the message has been removed from the stream's message store
    /// due to no longer being referenced.
    /// </summary>
    public bool MessageDataRemoved { get; }

    /// <summary>
    /// Delay to the next retry attempt.
    /// </summary>
    public Duration? Delay => _delay >= Duration.Zero ? _delay : null;

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerQueueNegativeAckProcessedEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var delay = _delay >= Duration.Zero ? $", Delay = {_delay}" : $", MessageDataRemoved = {MessageDataRemoved}";
        return $"[NegativeAckProcessed] {Source}, AckId = {AckId}{delay}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerQueueNegativeAckProcessedEvent Create(
        MessageBrokerQueue queue,
        ulong traceId,
        int ackId,
        Duration delay,
        bool messageDataRemoved)
    {
        return new MessageBrokerQueueNegativeAckProcessedEvent( queue, traceId, ackId, delay, messageDataRemoved );
    }
}
