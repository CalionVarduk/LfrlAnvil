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
/// Represents an event emitted by <see cref="MessageBrokerClient"/> when message notification received from the server
/// has been processed successfully.
/// </summary>
public readonly struct MessageBrokerClientMessageProcessedEvent
{
    private MessageBrokerClientMessageProcessedEvent(MessageBrokerListener listener, ulong traceId, ulong messageId, int length)
    {
        Source = MessageBrokerClientEventSource.Create( listener.Client, traceId );
        Listener = listener;
        MessageId = messageId;
        Length = length;
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerClientEventSource Source { get; }

    /// <summary>
    /// <see cref="MessageBrokerListener"/> that processed the message.
    /// </summary>
    public MessageBrokerListener Listener { get; }

    /// <summary>
    /// Unique id of the message assigned by the server.
    /// </summary>
    public ulong MessageId { get; }

    /// <summary>
    /// Message length.
    /// </summary>
    public int Length { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerClientMessageProcessedEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return
            $"[MessageProcessed] {Source}, Channel = [{Listener.ChannelId}] '{Listener.ChannelName}', Queue = [{Listener.QueueId}] '{Listener.QueueName}', MessageId = {MessageId}, Length = {Length}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientMessageProcessedEvent Create(
        MessageBrokerListener listener,
        ulong traceId,
        ulong messageId,
        int length)
    {
        return new MessageBrokerClientMessageProcessedEvent( listener, traceId, messageId, length );
    }
}
