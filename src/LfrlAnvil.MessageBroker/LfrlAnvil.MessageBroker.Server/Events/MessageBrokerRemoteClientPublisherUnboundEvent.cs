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
/// Represents an event emitted by <see cref="MessageBrokerRemoteClient"/> related to a publisher being successfully unbound.
/// </summary>
public readonly struct MessageBrokerRemoteClientPublisherUnboundEvent
{
    private MessageBrokerRemoteClientPublisherUnboundEvent(
        MessageBrokerChannelPublisherBinding publisher,
        ulong traceId,
        bool channelRemoved,
        bool streamRemoved)
    {
        Source = MessageBrokerRemoteClientEventSource.Create( publisher.Client, traceId );
        Publisher = publisher;
        ChannelRemoved = channelRemoved;
        StreamRemoved = streamRemoved;
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerRemoteClientEventSource Source { get; }

    /// <summary>
    /// Unbound publisher.
    /// </summary>
    public MessageBrokerChannelPublisherBinding Publisher { get; }

    /// <summary>
    /// Specifies whether removal of the channel bound to the <see cref="Publisher"/> was part of the unbinding operation.
    /// </summary>
    public bool ChannelRemoved { get; }

    /// <summary>
    /// Specifies whether removal of the stream bound to the <see cref="Publisher"/> was part of the unbinding operation.
    /// </summary>
    public bool StreamRemoved { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerRemoteClientPublisherUnboundEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var channelRemoved = ChannelRemoved ? " (removed)" : string.Empty;
        var streamRemoved = StreamRemoved ? " (removed)" : string.Empty;
        return
            $"[PublisherUnbound] {Source}, Channel = [{Publisher.Channel.Id}] '{Publisher.Channel.Name}'{channelRemoved}, Stream = [{Publisher.Stream.Id}] '{Publisher.Stream.Name}'{streamRemoved}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerRemoteClientPublisherUnboundEvent Create(
        MessageBrokerChannelPublisherBinding publisher,
        ulong traceId,
        bool channelRemoved,
        bool streamRemoved)
    {
        return new MessageBrokerRemoteClientPublisherUnboundEvent( publisher, traceId, channelRemoved, streamRemoved );
    }
}
