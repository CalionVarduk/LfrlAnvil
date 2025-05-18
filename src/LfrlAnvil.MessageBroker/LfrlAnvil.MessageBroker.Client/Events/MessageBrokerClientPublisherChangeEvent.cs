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
/// Represents an event emitted by <see cref="MessageBrokerClient"/> related to a publisher state change.
/// </summary>
public readonly struct MessageBrokerClientPublisherChangeEvent
{
    private MessageBrokerClientPublisherChangeEvent(
        MessageBrokerClient client,
        ulong traceId,
        MessageBrokerPublisher publisher,
        MessageBrokerClientPublisherChangeEventType type)
    {
        Source = MessageBrokerClientEventSource.Create( client, traceId );
        Publisher = publisher;
        Type = type;
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
    /// Specifies the type of this event.
    /// </summary>
    /// <remarks>See<see cref="MessageBrokerClientPublisherChangeEventType"/> for more information.</remarks>
    public MessageBrokerClientPublisherChangeEventType Type { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerClientPublisherChangeEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return
            $"[PublisherChange:{Type}] {Source}, Channel = [{Publisher.ChannelId}] '{Publisher.ChannelName}', Stream = [{Publisher.StreamId}] '{Publisher.StreamName}'";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientPublisherChangeEvent CreateBound(
        MessageBrokerClient client,
        ulong traceId,
        MessageBrokerPublisher publisher)
    {
        return new MessageBrokerClientPublisherChangeEvent( client, traceId, publisher, MessageBrokerClientPublisherChangeEventType.Bound );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientPublisherChangeEvent CreateUnbinding(
        MessageBrokerClient client,
        ulong traceId,
        MessageBrokerPublisher publisher)
    {
        return new MessageBrokerClientPublisherChangeEvent(
            client,
            traceId,
            publisher,
            MessageBrokerClientPublisherChangeEventType.Unbinding );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientPublisherChangeEvent CreateUnbound(
        MessageBrokerClient client,
        ulong traceId,
        MessageBrokerPublisher publisher)
    {
        return new MessageBrokerClientPublisherChangeEvent(
            client,
            traceId,
            publisher,
            MessageBrokerClientPublisherChangeEventType.Unbound );
    }
}
