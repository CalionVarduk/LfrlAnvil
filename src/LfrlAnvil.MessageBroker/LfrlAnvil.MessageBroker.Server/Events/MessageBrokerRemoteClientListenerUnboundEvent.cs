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
/// Represents an event emitted by <see cref="MessageBrokerRemoteClient"/> related to a listener being successfully unbound.
/// </summary>
public readonly struct MessageBrokerRemoteClientListenerUnboundEvent
{
    private MessageBrokerRemoteClientListenerUnboundEvent(
        MessageBrokerChannelListenerBinding listener,
        ulong traceId,
        bool channelRemoved,
        bool queueRemoved)
    {
        Source = MessageBrokerRemoteClientEventSource.Create( listener.Client, traceId );
        Listener = listener;
        ChannelRemoved = channelRemoved;
        QueueRemoved = queueRemoved;
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerRemoteClientEventSource Source { get; }

    /// <summary>
    /// Unbound listener.
    /// </summary>
    public MessageBrokerChannelListenerBinding Listener { get; }

    /// <summary>
    /// Specifies whether removal of the channel bound to the <see cref="Listener"/> was part of the unbinding operation.
    /// </summary>
    public bool ChannelRemoved { get; }

    /// <summary>
    /// Specifies whether removal of the queue bound to the <see cref="Listener"/> was part of the unbinding operation.
    /// </summary>
    public bool QueueRemoved { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerRemoteClientListenerUnboundEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var channelRemoved = ChannelRemoved ? " (removed)" : string.Empty;
        var queueRemoved = QueueRemoved ? " (removed)" : string.Empty;
        return
            $"[ListenerUnbound] {Source}, Channel = [{Listener.Channel.Id}] '{Listener.Channel.Name}'{channelRemoved}, Queue = [{Listener.Queue.Id}] '{Listener.Queue.Name}'{queueRemoved}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerRemoteClientListenerUnboundEvent Create(
        MessageBrokerChannelListenerBinding listener,
        ulong traceId,
        bool channelRemoved,
        bool queueRemoved)
    {
        return new MessageBrokerRemoteClientListenerUnboundEvent( listener, traceId, channelRemoved, queueRemoved );
    }
}
