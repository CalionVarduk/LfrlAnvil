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
/// Represents an event emitted by <see cref="MessageBrokerChannel"/> related to a publisher being successfully bound.
/// </summary>
public readonly struct MessageBrokerChannelPublisherBoundEvent
{
    private MessageBrokerChannelPublisherBoundEvent(MessageBrokerChannelPublisherBinding publisher, ulong traceId, bool streamCreated)
    {
        Source = MessageBrokerChannelEventSource.Create( publisher.Channel, traceId );
        Publisher = publisher;
        StreamCreated = streamCreated;
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerChannelEventSource Source { get; }

    /// <summary>
    /// Bound publisher.
    /// </summary>
    public MessageBrokerChannelPublisherBinding Publisher { get; }

    /// <summary>
    /// Specifies whether or not creation of the stream bound to the <see cref="Publisher"/> was part of the binding operation.
    /// </summary>
    public bool StreamCreated { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerChannelPublisherBoundEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var streamCreated = StreamCreated ? " (created)" : string.Empty;
        return
            $"[PublisherBound] {Source}, Client = [{Publisher.Client.Id}] '{Publisher.Client.Name}', Stream = [{Publisher.Stream.Id}] '{Publisher.Stream.Name}'{streamCreated}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerChannelPublisherBoundEvent Create(
        MessageBrokerChannelPublisherBinding publisher,
        ulong traceId,
        bool streamCreated)
    {
        return new MessageBrokerChannelPublisherBoundEvent( publisher, traceId, streamCreated );
    }
}
