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

namespace LfrlAnvil.MessageBroker.Server;

/// <summary>
/// Represents a snapshot of a pending message stored by a <see cref="MessageBrokerQueue"/>.
/// </summary>
public readonly struct MessageBrokerQueuePendingMessage
{
    internal MessageBrokerQueuePendingMessage(
        IMessageBrokerMessagePublisher publisher,
        MessageBrokerQueueListenerBinding listener,
        int storeKey)
    {
        Publisher = publisher;
        Listener = listener;
        StoreKey = storeKey;
    }

    /// <summary>
    /// <see cref="IMessageBrokerMessagePublisher"/> that pushed this message.
    /// </summary>
    public IMessageBrokerMessagePublisher Publisher { get; }

    /// <summary>
    /// <see cref="MessageBrokerQueueListenerBinding"/> that handles this message.
    /// </summary>
    public MessageBrokerQueueListenerBinding Listener { get; }

    /// <summary>
    /// Stream store key of this message.
    /// </summary>
    public int StoreKey { get; }

    /// <summary>
    /// Attempts to retrieve a message from the stream store by the <see cref="StoreKey"/>.
    /// </summary>
    /// <param name="includeData">Specifies whether to include a copy of binary data. Equal to <b>false</b> by default.</param>
    /// <returns>
    /// <see cref="MessageBrokerStreamMessage"/> instance associated with the <see cref="StoreKey"/>
    /// or <b>null</b> if such a message doesn't exist.
    /// </returns>
    [Pure]
    public MessageBrokerStreamMessage? TryGetMessage(bool includeData = false)
    {
        return Publisher.Stream.Messages.TryGetByKey( StoreKey, includeData );
    }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerQueuePendingMessage"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"Publisher = ({Publisher}), Listener = ({Listener}), StoreKey = {StoreKey}";
    }
}
