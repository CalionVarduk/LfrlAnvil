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
/// Represents an event emitted by <see cref="MessageBrokerRemoteClient"/> when attempting to send message notification to the client.
/// </summary>
public readonly struct MessageBrokerRemoteClientProcessingMessageEvent
{
    private MessageBrokerRemoteClientProcessingMessageEvent(
        MessageBrokerChannelListenerBinding listener,
        ulong traceId,
        MessageBrokerChannelPublisherBinding publisher,
        ulong messageId,
        int retryAttempt,
        int redeliveryAttempt,
        int length)
    {
        Source = MessageBrokerRemoteClientEventSource.Create( listener.Client, traceId );
        Listener = listener;
        Publisher = publisher;
        MessageId = messageId;
        RetryAttempt = retryAttempt;
        RedeliveryAttempt = redeliveryAttempt;
        Length = length;
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerRemoteClientEventSource Source { get; }

    /// <summary>
    /// <see cref="MessageBrokerChannelListenerBinding"/> that is processing the message.
    /// </summary>
    public MessageBrokerChannelListenerBinding Listener { get; }

    /// <summary>
    /// <see cref="MessageBrokerChannelPublisherBinding"/> that sent the message.
    /// </summary>
    public MessageBrokerChannelPublisherBinding Publisher { get; }

    /// <summary>
    /// Unique id of the message.
    /// </summary>
    public ulong MessageId { get; }

    /// <summary>
    /// Retry attempt number of this message.
    /// </summary>
    public int RetryAttempt { get; }

    /// <summary>
    /// Redelivery attempt number of this message.
    /// </summary>
    public int RedeliveryAttempt { get; }

    /// <summary>
    /// Message length.
    /// </summary>
    public int Length { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerRemoteClientProcessingMessageEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return
            $"[ProcessingMessage] {Source}, Sender = [{Publisher.Client.Id}] '{Publisher.Client.Name}', Channel = [{Listener.Channel.Id}] '{Listener.Channel.Name}', Stream = [{Publisher.Stream.Id}] '{Publisher.Stream.Name}', Queue = [{Listener.Queue.Id}] '{Listener.Queue.Name}', MessageId = {MessageId}, RetryAttempt = {RetryAttempt}, RedeliveryAttempt = {RedeliveryAttempt}, Length = {Length}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerRemoteClientProcessingMessageEvent Create(
        MessageBrokerChannelListenerBinding listener,
        ulong traceId,
        MessageBrokerChannelPublisherBinding publisher,
        ulong messageId,
        int retryAttempt,
        int redeliveryAttempt,
        int length)
    {
        return new MessageBrokerRemoteClientProcessingMessageEvent(
            listener,
            traceId,
            publisher,
            messageId,
            retryAttempt,
            redeliveryAttempt,
            length );
    }
}
