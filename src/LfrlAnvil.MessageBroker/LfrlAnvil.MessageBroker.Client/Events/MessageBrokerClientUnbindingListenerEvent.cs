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
/// Represents an event emitted by <see cref="MessageBrokerClient"/> related to unbinding a listener.
/// </summary>
public readonly struct MessageBrokerClientUnbindingListenerEvent
{
    private MessageBrokerClientUnbindingListenerEvent(
        MessageBrokerClient client,
        ulong traceId,
        MessageBrokerListener? listener,
        string channelName)
    {
        Source = MessageBrokerClientEventSource.Create( client, traceId );
        Listener = listener;
        ChannelName = channelName;
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
    /// Name of the channel from which the client is unbinding a listener.
    /// </summary>
    public string ChannelName { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerClientUnbindingListenerEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var info = Listener is not null
            ? $"Channel = [{Listener.ChannelId}] '{Listener.ChannelName}', Queue = [{Listener.QueueId}] '{Listener.QueueName}'"
            : $"ChannelName = '{ChannelName}'";

        return $"[UnbindingListener] {Source}, {info}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientUnbindingListenerEvent Create(MessageBrokerListener listener, ulong traceId)
    {
        return new MessageBrokerClientUnbindingListenerEvent( listener.Client, traceId, listener, listener.ChannelName );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientUnbindingListenerEvent Create(MessageBrokerClient client, ulong traceId, string channelName)
    {
        return new MessageBrokerClientUnbindingListenerEvent( client, traceId, listener: null, channelName );
    }
}
