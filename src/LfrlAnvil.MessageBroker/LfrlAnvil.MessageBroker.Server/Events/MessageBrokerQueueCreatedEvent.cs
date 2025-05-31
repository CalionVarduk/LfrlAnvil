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
/// Represents an event emitted by <see cref="MessageBrokerQueue"/> when it's created.
/// </summary>
public readonly struct MessageBrokerQueueCreatedEvent
{
    private MessageBrokerQueueCreatedEvent(MessageBrokerQueue queue, ulong traceId)
    {
        Source = MessageBrokerQueueEventSource.Create( queue, traceId );
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerQueueEventSource Source { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerQueueCreatedEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[Created] {Source}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerQueueCreatedEvent Create(MessageBrokerQueue queue, ulong traceId)
    {
        return new MessageBrokerQueueCreatedEvent( queue, traceId );
    }
}
