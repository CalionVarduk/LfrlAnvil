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
/// Represents an event emitted by <see cref="MessageBrokerClient"/> related to a publisher being successfully unbound.
/// </summary>
public readonly struct MessageBrokerClientPublisherUnboundEvent
{
    private MessageBrokerClientPublisherUnboundEvent(
        MessageBrokerClient client,
        MessageBrokerPublisher? publisher,
        string channelName,
        ulong traceId,
        bool channelRemoved,
        bool streamRemoved)
    {
        Source = MessageBrokerClientEventSource.Create( client, traceId );
        Publisher = publisher;
        ChannelName = channelName;
        ChannelRemoved = channelRemoved;
        StreamRemoved = streamRemoved;
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
    /// Name of the channel from which the client has unbound a publisher.
    /// </summary>
    public string ChannelName { get; }

    /// <summary>
    /// Specifies whether the channel has been removed by the server.
    /// </summary>
    public bool ChannelRemoved { get; }

    /// <summary>
    /// Specifies whether the stream has been removed by the server.
    /// </summary>
    public bool StreamRemoved { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerClientPublisherUnboundEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var info = Publisher is not null
            ? $"Channel = [{Publisher.ChannelId}] '{Publisher.ChannelName}'{(ChannelRemoved ? " (removed)" : string.Empty)}, Stream = [{Publisher.StreamId}] '{Publisher.StreamName}'{(StreamRemoved ? " (removed)" : string.Empty)}"
            : $"ChannelName = '{ChannelName}'{(ChannelRemoved ? " (channel removed)" : string.Empty)}{(StreamRemoved ? " (stream removed)" : string.Empty)}";

        return $"[PublisherUnbound] {Source}, {info}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientPublisherUnboundEvent Create(
        MessageBrokerPublisher publisher,
        ulong traceId,
        bool channelRemoved,
        bool streamRemoved)
    {
        return new MessageBrokerClientPublisherUnboundEvent(
            publisher.Client,
            publisher,
            publisher.ChannelName,
            traceId,
            channelRemoved,
            streamRemoved );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientPublisherUnboundEvent Create(
        MessageBrokerClient client,
        string channelName,
        ulong traceId,
        bool channelRemoved,
        bool streamRemoved)
    {
        return new MessageBrokerClientPublisherUnboundEvent( client, publisher: null, channelName, traceId, channelRemoved, streamRemoved );
    }
}
