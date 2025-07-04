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
/// Represents an event emitted by <see cref="MessageBrokerClient"/> related to a listener being successfully bound.
/// </summary>
public readonly struct MessageBrokerClientListenerBoundEvent
{
    private MessageBrokerClientListenerBoundEvent(MessageBrokerListener listener, ulong traceId, bool channelCreated, bool queueCreated)
    {
        Source = MessageBrokerClientEventSource.Create( listener.Client, traceId );
        Listener = listener;
        ChannelCreated = channelCreated;
        QueueCreated = queueCreated;
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
    /// Specifies whether or not a new channel has been created by the server.
    /// </summary>
    public bool ChannelCreated { get; }

    /// <summary>
    /// Specifies whether or not a new queue has been created by the server.
    /// </summary>
    public bool QueueCreated { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerClientListenerBoundEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var channelCreated = ChannelCreated ? " (created)" : string.Empty;
        var queueCreated = QueueCreated ? " (created)" : string.Empty;
        return
            $"[ListenerBound] {Source}, Channel = [{Listener.ChannelId}] '{Listener.ChannelName}'{channelCreated}, Queue = [{Listener.QueueId}] '{Listener.QueueName}'{queueCreated}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientListenerBoundEvent Create(
        MessageBrokerListener listener,
        ulong traceId,
        bool channelCreated,
        bool queueCreated)
    {
        return new MessageBrokerClientListenerBoundEvent( listener, traceId, channelCreated, queueCreated );
    }
}
