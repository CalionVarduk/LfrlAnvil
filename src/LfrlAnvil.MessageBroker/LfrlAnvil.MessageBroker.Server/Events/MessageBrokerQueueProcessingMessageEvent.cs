// Copyright 2025-2026 Łukasz Furlepa
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
using LfrlAnvil.Internal;

namespace LfrlAnvil.MessageBroker.Server.Events;

/// <summary>
/// Represents an event emitted by <see cref="MessageBrokerQueue"/> when starting to process an enqueued message.
/// </summary>
public readonly struct MessageBrokerQueueProcessingMessageEvent
{
    private readonly Int31BoolPair _retry;
    private readonly Int31BoolPair _redelivery;

    private MessageBrokerQueueProcessingMessageEvent(
        MessageBrokerChannelListenerBinding listener,
        ulong traceId,
        IMessageBrokerMessagePublisher publisher,
        int storeKey,
        bool isFromDeadLetter,
        Int31BoolPair retry,
        Int31BoolPair redelivery)
    {
        Source = MessageBrokerQueueEventSource.Create( listener.Queue, traceId );
        Listener = listener;
        Publisher = publisher;
        StoreKey = storeKey;
        IsFromDeadLetter = isFromDeadLetter;
        _retry = retry;
        _redelivery = redelivery;
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerQueueEventSource Source { get; }

    /// <summary>
    /// <see cref="MessageBrokerChannelListenerBinding"/> that received the message.
    /// </summary>
    public MessageBrokerChannelListenerBinding Listener { get; }

    /// <summary>
    /// <see cref="IMessageBrokerMessagePublisher"/> that pushed the message.
    /// </summary>
    public IMessageBrokerMessagePublisher Publisher { get; }

    /// <summary>
    /// Key of the stream's message store entry associated with the message.
    /// </summary>
    public int StoreKey { get; }

    /// <summary>
    /// Specifies whether or not this message is from dead letter.
    /// </summary>
    public bool IsFromDeadLetter { get; }

    /// <summary>
    /// Specifies whether or not this message is a retry.
    /// </summary>
    public bool IsRetry => _retry.BoolValue;

    /// <summary>
    /// Retry attempt number of this message.
    /// </summary>
    public int Retry => _retry.IntValue;

    /// <summary>
    /// Specifies whether or not this message is a redelivery.
    /// </summary>
    public bool IsRedelivery => _redelivery.BoolValue;

    /// <summary>
    /// Redelivery attempt number of this message.
    /// </summary>
    public int Redelivery => _redelivery.IntValue;

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerQueueProcessingMessageEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var isRetry = IsRetry ? " (active)" : string.Empty;
        var isRedelivery = IsRedelivery ? " (active)" : string.Empty;
        return
            $"[ProcessingMessage] {Source}, Sender = [{Publisher.ClientId}] '{Publisher.ClientName}', Channel = [{Listener.Channel.Id}] '{Listener.Channel.Name}', Stream = [{Publisher.Stream.Id}] '{Publisher.Stream.Name}', StoreKey = {StoreKey}, Retry = {Retry}{isRetry}, Redelivery = {Redelivery}{isRedelivery}, IsFromDeadLetter = {IsFromDeadLetter}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerQueueProcessingMessageEvent Create(
        MessageBrokerChannelListenerBinding listener,
        ulong traceId,
        IMessageBrokerMessagePublisher publisher,
        int storeKey,
        bool isFromDeadLetter,
        Int31BoolPair retry,
        Int31BoolPair redelivery)
    {
        return new MessageBrokerQueueProcessingMessageEvent( listener, traceId, publisher, storeKey, isFromDeadLetter, retry, redelivery );
    }
}
