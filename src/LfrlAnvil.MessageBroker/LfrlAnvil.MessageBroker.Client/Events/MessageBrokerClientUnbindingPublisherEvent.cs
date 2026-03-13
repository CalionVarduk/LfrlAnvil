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
/// Represents an event emitted by <see cref="MessageBrokerClient"/> related to unbinding a publisher.
/// </summary>
public readonly struct MessageBrokerClientUnbindingPublisherEvent
{
    private MessageBrokerClientUnbindingPublisherEvent(
        MessageBrokerClient client,
        MessageBrokerPublisher? publisher,
        string channelName,
        ulong traceId)
    {
        Source = MessageBrokerClientEventSource.Create( client, traceId );
        Publisher = publisher;
        ChannelName = channelName;
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerClientEventSource Source { get; }

    /// <summary>
    /// <see cref="MessageBrokerPublisher"/> related to this event.
    /// </summary>
    public MessageBrokerPublisher? Publisher { get; }

    /// <summary>
    /// Name of the channel from which the client is unbinding a publisher.
    /// </summary>
    public string ChannelName { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerClientUnbindingPublisherEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var info = Publisher is not null
            ? $"Channel = [{Publisher.ChannelId}] '{Publisher.ChannelName}', Stream = [{Publisher.StreamId}] '{Publisher.StreamName}'"
            : $"ChannelName = '{ChannelName}'";

        return $"[UnbindingPublisher] {Source}, {info}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientUnbindingPublisherEvent Create(MessageBrokerPublisher publisher, ulong traceId)
    {
        return new MessageBrokerClientUnbindingPublisherEvent( publisher.Client, publisher, publisher.ChannelName, traceId );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientUnbindingPublisherEvent Create(MessageBrokerClient client, string channelName, ulong traceId)
    {
        return new MessageBrokerClientUnbindingPublisherEvent( client, publisher: null, channelName, traceId );
    }
}
