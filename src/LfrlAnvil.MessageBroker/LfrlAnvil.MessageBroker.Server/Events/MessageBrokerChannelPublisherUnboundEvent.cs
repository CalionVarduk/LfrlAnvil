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
/// Represents an event emitted by <see cref="MessageBrokerChannel"/> related to a publisher being successfully unbound.
/// </summary>
public readonly struct MessageBrokerChannelPublisherUnboundEvent
{
    private MessageBrokerChannelPublisherUnboundEvent(MessageBrokerChannelPublisherBinding publisher, ulong traceId, bool streamRemoved)
    {
        Source = MessageBrokerChannelEventSource.Create( publisher.Channel, traceId );
        Publisher = publisher;
        StreamRemoved = streamRemoved;
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerChannelEventSource Source { get; }

    /// <summary>
    /// Unbound publisher.
    /// </summary>
    public MessageBrokerChannelPublisherBinding Publisher { get; }

    /// <summary>
    /// Specifies whether or not removal of the stream bound to the <see cref="Publisher"/> was part of the unbinding operation.
    /// </summary>
    public bool StreamRemoved { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerChannelPublisherUnboundEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var streamRemoved = StreamRemoved ? " (removed)" : string.Empty;
        return
            $"[PublisherUnbound] {Source}, Client = [{Publisher.Client.Id}] '{Publisher.Client.Name}', Stream = [{Publisher.Stream.Id}] '{Publisher.Stream.Name}'{streamRemoved}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerChannelPublisherUnboundEvent Create(
        MessageBrokerChannelPublisherBinding publisher,
        ulong traceId,
        bool streamRemoved)
    {
        return new MessageBrokerChannelPublisherUnboundEvent( publisher, traceId, streamRemoved );
    }
}
