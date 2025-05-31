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
/// Represents a source of an event emitted by <see cref="MessageBrokerQueue"/>.
/// </summary>
public readonly struct MessageBrokerQueueEventSource
{
    private MessageBrokerQueueEventSource(MessageBrokerQueue queue, ulong traceId)
    {
        Queue = queue;
        TraceId = traceId;
    }

    /// <summary>
    /// <see cref="MessageBrokerQueue"/> that emitted an event.
    /// </summary>
    public MessageBrokerQueue Queue { get; }

    /// <summary>
    /// Identifier of an internal trace, with which an event is correlated to.
    /// </summary>
    /// <remarks>Can be used to correlate multiple events together, which are part of the same operation.</remarks>
    public ulong TraceId { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerQueueEventSource"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"Client = [{Queue.Client.Id}] '{Queue.Client.Name}', Queue = [{Queue.Id}] '{Queue.Name}', TraceId = {TraceId}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerQueueEventSource Create(MessageBrokerQueue queue, ulong traceId)
    {
        return new MessageBrokerQueueEventSource( queue, traceId );
    }
}
