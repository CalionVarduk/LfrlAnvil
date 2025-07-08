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
/// Represents an event emitted by <see cref="MessageBrokerRemoteClient"/> after a message pushed from the client
/// has been handled successfully.
/// </summary>
public readonly struct MessageBrokerRemoteClientMessagePushedEvent
{
    private MessageBrokerRemoteClientMessagePushedEvent(
        MessageBrokerChannelPublisherBinding publisher,
        ulong traceId,
        ulong messageId,
        ulong? routingTraceId)
    {
        Source = MessageBrokerRemoteClientEventSource.Create( publisher.Client, traceId );
        Publisher = publisher;
        MessageId = messageId;
        RoutingTraceId = routingTraceId;
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerRemoteClientEventSource Source { get; }

    /// <summary>
    /// <see cref="MessageBrokerChannelPublisherBinding"/> used for pushing the message.
    /// </summary>
    public MessageBrokerChannelPublisherBinding Publisher { get; }

    /// <summary>
    /// Unique id of the message.
    /// </summary>
    public ulong MessageId { get; }

    /// <summary>
    /// Identifier of an internal message routing client trace that's used for this message.
    /// </summary>
    /// <remarks>See <see cref="MessageBrokerRemoteClientEventSource"/> for more information.</remarks>
    public ulong? RoutingTraceId { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerRemoteClientMessagePushedEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var routingTraceId = RoutingTraceId is not null ? $", RoutingTraceId = {RoutingTraceId.Value}" : string.Empty;
        return
            $"[MessagePushed] {Source}, Channel = [{Publisher.Channel.Id}] '{Publisher.Channel.Name}', Stream = [{Publisher.Stream.Id}] '{Publisher.Stream.Name}', MessageId = {MessageId}{routingTraceId}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerRemoteClientMessagePushedEvent Create(
        MessageBrokerChannelPublisherBinding publisher,
        ulong traceId,
        ulong messageId,
        ulong? routingTraceId)
    {
        return new MessageBrokerRemoteClientMessagePushedEvent( publisher, traceId, messageId, routingTraceId );
    }
}
