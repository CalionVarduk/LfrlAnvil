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
/// Represents an event emitted by <see cref="MessageBrokerClient"/> related to a publisher being successfully bound.
/// </summary>
public readonly struct MessageBrokerClientPublisherBoundEvent
{
    private MessageBrokerClientPublisherBoundEvent(
        MessageBrokerPublisher publisher,
        ulong traceId,
        bool channelCreated,
        bool streamCreated)
    {
        Source = MessageBrokerClientEventSource.Create( publisher.Client, traceId );
        Publisher = publisher;
        ChannelCreated = channelCreated;
        StreamCreated = streamCreated;
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerClientEventSource Source { get; }

    /// <summary>
    /// <see cref="MessageBrokerPublisher"/> related to this event.
    /// </summary>
    public MessageBrokerPublisher Publisher { get; }

    /// <summary>
    /// Specifies whether a new channel has been created by the server.
    /// </summary>
    public bool ChannelCreated { get; }

    /// <summary>
    /// Specifies whether a new stream has been created by the server.
    /// </summary>
    public bool StreamCreated { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerClientPublisherBoundEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var channelCreated = ChannelCreated ? " (created)" : string.Empty;
        var streamCreated = StreamCreated ? " (created)" : string.Empty;
        return
            $"[PublisherBound] {Source}, Channel = [{Publisher.ChannelId}] '{Publisher.ChannelName}'{channelCreated}, Stream = [{Publisher.StreamId}] '{Publisher.StreamName}'{streamCreated}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientPublisherBoundEvent Create(
        MessageBrokerPublisher publisher,
        ulong traceId,
        bool channelCreated,
        bool streamCreated)
    {
        return new MessageBrokerClientPublisherBoundEvent( publisher, traceId, channelCreated, streamCreated );
    }
}
