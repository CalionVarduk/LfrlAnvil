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
/// Represents an event emitted by <see cref="MessageBrokerStream"/> related to a publisher being successfully unbound.
/// </summary>
public readonly struct MessageBrokerStreamPublisherUnboundEvent
{
    private MessageBrokerStreamPublisherUnboundEvent(MessageBrokerChannelPublisherBinding publisher, ulong traceId, bool channelRemoved)
    {
        Source = MessageBrokerStreamEventSource.Create( publisher.Stream, traceId );
        Publisher = publisher;
        ChannelRemoved = channelRemoved;
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerStreamEventSource Source { get; }

    /// <summary>
    /// Unbound publisher.
    /// </summary>
    public MessageBrokerChannelPublisherBinding Publisher { get; }

    /// <summary>
    /// Specifies whether or not removal of the channel bound to the <see cref="Publisher"/> was part of the unbinding operation.
    /// </summary>
    public bool ChannelRemoved { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerStreamPublisherUnboundEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var channelRemoved = ChannelRemoved ? " (removed)" : string.Empty;
        return
            $"[PublisherUnbound] {Source}, Client = [{Publisher.Client.Id}] '{Publisher.Client.Name}', Channel = [{Publisher.Channel.Id}] '{Publisher.Channel.Name}'{channelRemoved}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerStreamPublisherUnboundEvent Create(
        MessageBrokerChannelPublisherBinding publisher,
        ulong traceId,
        bool channelRemoved)
    {
        return new MessageBrokerStreamPublisherUnboundEvent( publisher, traceId, channelRemoved );
    }
}
