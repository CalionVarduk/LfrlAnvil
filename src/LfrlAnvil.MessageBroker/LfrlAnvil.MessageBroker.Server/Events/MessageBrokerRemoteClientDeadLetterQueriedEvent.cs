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
/// Represents an event emitted by <see cref="MessageBrokerRemoteClient"/> related to dead letter query result.
/// </summary>
public readonly struct MessageBrokerRemoteClientDeadLetterQueriedEvent
{
    private MessageBrokerRemoteClientDeadLetterQueriedEvent(
        MessageBrokerQueue queue,
        ulong traceId,
        int totalCount,
        int maxReadCount,
        Timestamp nextExpirationAt)
    {
        Source = MessageBrokerRemoteClientEventSource.Create( queue.Client, traceId );
        Queue = queue;
        TotalCount = totalCount;
        MaxReadCount = maxReadCount;
        NextExpirationAt = nextExpirationAt;
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerRemoteClientEventSource Source { get; }

    /// <summary>
    /// <see cref="MessageBrokerQueue"/> whose dead letter was queried.
    /// </summary>
    public MessageBrokerQueue Queue { get; }

    /// <summary>
    /// Current number of dead letter messages.
    /// </summary>
    public int TotalCount { get; }

    /// <summary>
    /// Max number of dead letter messages to consume asynchronously.
    /// </summary>
    public int MaxReadCount { get; }

    /// <summary>
    /// Specifies the moment in time when the next dead letter message expires.
    /// </summary>
    public Timestamp NextExpirationAt { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerRemoteClientDeadLetterQueriedEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return
            $"[DeadLetterQueried] {Source}, Queue = [{Queue.Id}] '{Queue.Name}', TotalCount = {TotalCount}{(TotalCount > 0 ? $", MaxReadCount = {MaxReadCount}, NextExpirationAt = {NextExpirationAt}" : string.Empty)}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerRemoteClientDeadLetterQueriedEvent Create(
        MessageBrokerQueue queue,
        ulong traceId,
        int totalCount,
        int maxReadCount,
        Timestamp nextExpirationAt)
    {
        return new MessageBrokerRemoteClientDeadLetterQueriedEvent( queue, traceId, totalCount, maxReadCount, nextExpirationAt );
    }
}
