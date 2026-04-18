// Copyright 2025-2026 Łukasz Furlepa
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
/// Represents an event emitted by <see cref="MessageBrokerChannel"/> related to a listener being successfully bound.
/// </summary>
public readonly struct MessageBrokerChannelListenerBoundEvent
{
    private MessageBrokerChannelListenerBoundEvent(
        MessageBrokerChannelListenerBinding listener,
        ulong traceId,
        bool queueCreated,
        bool reactivated)
    {
        Source = MessageBrokerChannelEventSource.Create( listener.Channel, traceId );
        Listener = listener;
        QueueCreated = queueCreated;
        Reactivated = reactivated;
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerChannelEventSource Source { get; }

    /// <summary>
    /// Bound listener.
    /// </summary>
    public MessageBrokerChannelListenerBinding Listener { get; }

    /// <summary>
    /// Specifies whether creation of the queue bound to the <see cref="Listener"/> was part of the binding operation.
    /// </summary>
    public bool QueueCreated { get; }

    /// <summary>
    /// Specifies whether the <see cref="Listener"/> existed and was reactivated.
    /// </summary>
    public bool Reactivated { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerChannelListenerBoundEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var queue = Listener.QueueBindings.Primary.Queue;
        var queueCreated = QueueCreated ? " (created)" : string.Empty;
        var reactivated = Reactivated ? " (reactivated)" : string.Empty;
        return
            $"[ListenerBound] {Source}, Client = [{Listener.Client.Id}] '{Listener.Client.Name}', Queue = [{queue.Id}] '{queue.Name}'{queueCreated}{reactivated}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerChannelListenerBoundEvent Create(
        MessageBrokerChannelListenerBinding listener,
        ulong traceId,
        bool queueCreated,
        bool reactivated)
    {
        return new MessageBrokerChannelListenerBoundEvent( listener, traceId, queueCreated, reactivated );
    }
}
