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
/// Represents an event emitted by <see cref="MessageBrokerChannel"/> related to a listener being successfully unbound.
/// </summary>
public readonly struct MessageBrokerChannelListenerUnboundEvent
{
    private MessageBrokerChannelListenerUnboundEvent(MessageBrokerChannelListenerBinding listener, ulong traceId, bool queueRemoved)
    {
        Source = MessageBrokerChannelEventSource.Create( listener.Channel, traceId );
        Listener = listener;
        QueueRemoved = queueRemoved;
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerChannelEventSource Source { get; }

    /// <summary>
    /// Unbound listener.
    /// </summary>
    public MessageBrokerChannelListenerBinding Listener { get; }

    /// <summary>
    /// Specifies whether removal of the queue bound to the <see cref="Listener"/> was part of the unbinding operation.
    /// </summary>
    public bool QueueRemoved { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerChannelListenerUnboundEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var queueRemoved = QueueRemoved ? " (removed)" : string.Empty;
        return
            $"[ListenerUnbound] {Source}, Client = [{Listener.Client.Id}] '{Listener.Client.Name}', Queue = [{Listener.Queue.Id}] '{Listener.Queue.Name}'{queueRemoved}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerChannelListenerUnboundEvent Create(
        MessageBrokerChannelListenerBinding listener,
        ulong traceId,
        bool queueRemoved)
    {
        return new MessageBrokerChannelListenerUnboundEvent( listener, traceId, queueRemoved );
    }
}
