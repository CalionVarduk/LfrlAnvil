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

namespace LfrlAnvil.MessageBroker.Client.Events;

/// <summary>
/// Represents an event emitted by <see cref="MessageBrokerClient"/> related to a listener being successfully unbound.
/// </summary>
public readonly struct MessageBrokerClientListenerUnboundEvent
{
    private MessageBrokerClientListenerUnboundEvent(
        MessageBrokerClient client,
        ulong traceId,
        MessageBrokerListener? listener,
        string channelName,
        bool channelRemoved,
        bool queueRemoved)
    {
        Source = MessageBrokerClientEventSource.Create( client, traceId );
        Listener = listener;
        ChannelName = channelName;
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
    public MessageBrokerListener? Listener { get; }

    /// <summary>
    /// Name of the channel from which the client has unbound a listener.
    /// </summary>
    public string ChannelName { get; }

    /// <summary>
    /// Specifies whether the channel has been removed by the server.
    /// </summary>
    public bool ChannelRemoved { get; }

    /// <summary>
    /// Specifies whether the queue has been removed by the server.
    /// </summary>
    public bool QueueRemoved { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerClientListenerUnboundEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var info = Listener is not null
            ? $"Channel = [{Listener.ChannelId}] '{Listener.ChannelName}'{(ChannelRemoved ? " (removed)" : string.Empty)}, Queue = [{Listener.QueueId}] '{Listener.QueueName}'{(QueueRemoved ? " (removed)" : string.Empty)}"
            : $"ChannelName = '{ChannelName}'{(ChannelRemoved ? " (channel removed)" : string.Empty)}{(QueueRemoved ? " (queue removed)" : string.Empty)}";

        return $"[ListenerUnbound] {Source}, {info}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientListenerUnboundEvent Create(
        MessageBrokerListener listener,
        ulong traceId,
        bool channelRemoved,
        bool queueRemoved)
    {
        return new MessageBrokerClientListenerUnboundEvent(
            listener.Client,
            traceId,
            listener,
            listener.ChannelName,
            channelRemoved,
            queueRemoved );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientListenerUnboundEvent Create(
        MessageBrokerClient client,
        ulong traceId,
        string channelName,
        bool channelRemoved,
        bool queueRemoved)
    {
        return new MessageBrokerClientListenerUnboundEvent( client, traceId, listener: null, channelName, channelRemoved, queueRemoved );
    }
}
