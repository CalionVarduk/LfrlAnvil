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

namespace LfrlAnvil.MessageBroker.Client.Events;

/// <summary>
/// Represents an event emitted by <see cref="MessageBrokerClient"/> related to querying dead letter.
/// </summary>
public readonly struct MessageBrokerClientQueryingDeadLetterEvent
{
    private MessageBrokerClientQueryingDeadLetterEvent(MessageBrokerClient client, ulong traceId, int queueId, int readCount)
    {
        Source = MessageBrokerClientEventSource.Create( client, traceId );
        QueueId = queueId;
        ReadCount = readCount;
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerClientEventSource Source { get; }

    /// <summary>
    /// ID of the queue whose dead letter is to be queried.
    /// </summary>
    public int QueueId { get; }

    /// <summary>
    /// Number of dead letter messages to be asynchronously consumed.
    /// </summary>
    public int ReadCount { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerClientQueryingDeadLetterEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[QueryingDeadLetter] {Source}, QueueId = {QueueId}, ReadCount = {ReadCount}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientQueryingDeadLetterEvent Create(MessageBrokerClient client, ulong traceId, int queueId, int readCount)
    {
        return new MessageBrokerClientQueryingDeadLetterEvent( client, traceId, queueId, readCount );
    }
}
