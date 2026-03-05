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
/// Represents an event emitted by <see cref="MessageBrokerQueue"/> related to a listener being successfully bound.
/// </summary>
public readonly struct MessageBrokerQueueListenerBoundEvent
{
    private MessageBrokerQueueListenerBoundEvent(MessageBrokerChannelListenerBinding listener, ulong traceId, bool channelCreated)
    {
        Source = MessageBrokerQueueEventSource.Create( listener.Queue, traceId );
        Listener = listener;
        ChannelCreated = channelCreated;
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerQueueEventSource Source { get; }

    /// <summary>
    /// Bound listener.
    /// </summary>
    public MessageBrokerChannelListenerBinding Listener { get; }

    /// <summary>
    /// Specifies whether creation of the channel bound to the <see cref="Listener"/> was part of the binding operation.
    /// </summary>
    public bool ChannelCreated { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerQueueListenerBoundEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var channelCreated = ChannelCreated ? " (created)" : string.Empty;
        return $"[ListenerBound] {Source}, Channel = [{Listener.Channel.Id}] '{Listener.Channel.Name}'{channelCreated}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerQueueListenerBoundEvent Create(
        MessageBrokerChannelListenerBinding listener,
        ulong traceId,
        bool channelCreated)
    {
        return new MessageBrokerQueueListenerBoundEvent( listener, traceId, channelCreated );
    }
}
