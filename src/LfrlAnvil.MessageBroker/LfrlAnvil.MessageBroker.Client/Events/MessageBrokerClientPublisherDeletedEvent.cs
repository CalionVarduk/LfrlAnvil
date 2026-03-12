// Copyright 2026 Łukasz Furlepa
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
/// Represents an event emitted by <see cref="MessageBrokerClient"/> related to being notified by the server
/// that a publisher deletion occurred.
/// </summary>
public readonly struct MessageBrokerClientPublisherDeletedEvent
{
    private MessageBrokerClientPublisherDeletedEvent(
        MessageBrokerClient client,
        ulong traceId,
        MessageBrokerPublisher? publisher,
        string channelName,
        bool deleted)
    {
        Source = MessageBrokerClientEventSource.Create( client, traceId );
        Publisher = publisher;
        ChannelName = channelName;
        Deleted = deleted;
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
    /// Specifies name of the channel for which deletion of a publisher occurred.
    /// </summary>
    public string ChannelName { get; }

    /// <summary>
    /// Specifies whether the publisher existed before processing the notification.
    /// </summary>
    public bool Deleted { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerClientPublisherDeletedEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var publisher = Publisher is not null
            ? $", Channel = [{Publisher.ChannelId}] '{Publisher.ChannelName}', Stream = [{Publisher.StreamId}] '{Publisher.StreamName}'"
            : $", ChannelName = '{ChannelName}'";

        return $"[PublisherDeleted] {Source}, Deleted = {Deleted}{publisher}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientPublisherDeletedEvent Create(
        MessageBrokerClient client,
        ulong traceId,
        MessageBrokerPublisher? publisher,
        string channelName,
        bool deleted)
    {
        return new MessageBrokerClientPublisherDeletedEvent( client, traceId, publisher, channelName, deleted );
    }
}
