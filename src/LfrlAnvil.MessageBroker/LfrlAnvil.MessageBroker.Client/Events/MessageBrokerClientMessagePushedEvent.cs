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
/// Represents an event emitted by <see cref="MessageBrokerClient"/> after a message has been pushed to the server successfully.
/// </summary>
public readonly struct MessageBrokerClientMessagePushedEvent
{
    private MessageBrokerClientMessagePushedEvent(
        MessageBrokerClient client,
        ulong traceId,
        MessageBrokerPublisher publisher,
        int length,
        ulong? messageId)
    {
        Source = MessageBrokerClientEventSource.Create( client, traceId );
        Publisher = publisher;
        Length = length;
        MessageId = messageId;
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerClientEventSource Source { get; }

    /// <summary>
    /// <see cref="MessageBrokerPublisher"/> used for pushing the message.
    /// </summary>
    public MessageBrokerPublisher Publisher { get; }

    /// <summary>
    /// Message length.
    /// </summary>
    public int Length { get; }

    /// <summary>
    /// Unique id of the message assigned by the server.
    /// </summary>
    /// <remarks>
    /// Id will be <b>null</b> when the client did not request confirmation from the server that it received the message.
    /// </remarks>
    public ulong? MessageId { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerClientMessagePushedEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var messageId = MessageId is not null ? $", MessageId = {MessageId.Value}" : string.Empty;
        return
            $"[MessagePushed] {Source}, Channel = [{Publisher.ChannelId}] '{Publisher.ChannelName}', Stream = [{Publisher.StreamId}] '{Publisher.StreamName}', Length = {Length}{messageId}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientMessagePushedEvent Create(
        MessageBrokerClient client,
        ulong traceId,
        MessageBrokerPublisher publisher,
        int length,
        ulong? messageId = null)
    {
        return new MessageBrokerClientMessagePushedEvent( client, traceId, publisher, length, messageId );
    }
}
