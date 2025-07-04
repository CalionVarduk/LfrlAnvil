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
/// Represents an event emitted by <see cref="MessageBrokerClient"/> related to a listener being successfully unbound.
/// </summary>
public readonly struct MessageBrokerClientListenerUnboundEvent
{
    private MessageBrokerClientListenerUnboundEvent(MessageBrokerListener listener, ulong traceId, bool channelRemoved, bool queueRemoved)
    {
        Source = MessageBrokerClientEventSource.Create( listener.Client, traceId );
        Listener = listener;
        ChannelRemoved = channelRemoved;
        QueueRemoved = queueRemoved;
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerClientEventSource Source { get; }

    /// <summary>
    /// <see cref="MessageBrokerListener"/> related to this event.
    /// </summary>
    public MessageBrokerListener Listener { get; }

    /// <summary>
    /// Specifies whether or not the channel has been removed by the server.
    /// </summary>
    public bool ChannelRemoved { get; }

    /// <summary>
    /// Specifies whether or not the queue has been removed by the server.
    /// </summary>
    public bool QueueRemoved { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerClientListenerUnboundEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var channelRemoved = ChannelRemoved ? " (removed)" : string.Empty;
        var queueRemoved = QueueRemoved ? " (removed)" : string.Empty;
        return
            $"[ListenerUnbound] {Source}, Channel = [{Listener.ChannelId}] '{Listener.ChannelName}'{channelRemoved}, Queue = [{Listener.QueueId}] '{Listener.QueueName}'{queueRemoved}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientListenerUnboundEvent Create(
        MessageBrokerListener listener,
        ulong traceId,
        bool channelRemoved,
        bool queueRemoved)
    {
        return new MessageBrokerClientListenerUnboundEvent( listener, traceId, channelRemoved, queueRemoved );
    }
}
