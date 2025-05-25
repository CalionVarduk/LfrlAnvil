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
/// Represents an event emitted by <see cref="MessageBrokerClient"/> related to a publisher being successfully unbound.
/// </summary>
public readonly struct MessageBrokerClientPublisherUnboundEvent
{
    private readonly byte _state;

    private MessageBrokerClientPublisherUnboundEvent(
        MessageBrokerPublisher publisher,
        ulong traceId,
        bool channelRemoved,
        bool streamRemoved)
    {
        Source = MessageBrokerClientEventSource.Create( publisher.Client, traceId );
        Publisher = publisher;
        _state = ( byte )((channelRemoved ? 1 : 0) | (streamRemoved ? 2 : 0));
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
    /// Specifies whether or not the channel has been removed by the server.
    /// </summary>
    public bool ChannelRemoved => (_state & 1) != 0;

    /// <summary>
    /// Specifies whether or not the stream has been removed by the server.
    /// </summary>
    public bool StreamRemoved => (_state & 2) != 0;

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerClientPublisherUnboundEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var channelRemoved = ChannelRemoved ? " (removed)" : string.Empty;
        var streamRemoved = StreamRemoved ? " (removed)" : string.Empty;
        return
            $"[PublisherUnbound] {Source}, Channel = [{Publisher.ChannelId}] '{Publisher.ChannelName}'{channelRemoved}, Stream = [{Publisher.StreamId}] '{Publisher.StreamName}'{streamRemoved}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientPublisherUnboundEvent Create(
        MessageBrokerPublisher publisher,
        ulong traceId,
        bool channelRemoved,
        bool streamRemoved)
    {
        return new MessageBrokerClientPublisherUnboundEvent( publisher, traceId, channelRemoved, streamRemoved );
    }
}
