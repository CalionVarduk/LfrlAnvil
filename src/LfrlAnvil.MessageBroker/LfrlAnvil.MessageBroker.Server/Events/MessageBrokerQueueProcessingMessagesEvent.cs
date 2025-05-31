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
/// Represents an event emitted by <see cref="MessageBrokerQueue"/> when starting to process a batch of enqueued messages.
/// </summary>
public readonly struct MessageBrokerQueueProcessingMessagesEvent
{
    private MessageBrokerQueueProcessingMessagesEvent(MessageBrokerQueue queue, ulong traceId, int messageCount, int skippedMessageCount)
    {
        Source = MessageBrokerQueueEventSource.Create( queue, traceId );
        MessageCount = messageCount;
        SkippedMessageCount = skippedMessageCount;
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerQueueEventSource Source { get; }

    /// <summary>
    /// Number of messages to process.
    /// </summary>
    public int MessageCount { get; }

    /// <summary>
    /// Number of messages skipped due to disposed listeners.
    /// </summary>
    public int SkippedMessageCount { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerQueueProcessingMessagesEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[ProcessingMessages] {Source}, MessageCount = {MessageCount}, SkippedMessageCount = {SkippedMessageCount}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerQueueProcessingMessagesEvent Create(
        MessageBrokerQueue queue,
        ulong traceId,
        int messageCount,
        int skippedMessageCount)
    {
        return new MessageBrokerQueueProcessingMessagesEvent( queue, traceId, messageCount, skippedMessageCount );
    }
}
