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
/// Represents an event emitted by <see cref="MessageBrokerClient"/> related to binding a listener.
/// </summary>
public readonly struct MessageBrokerClientBindingListenerEvent
{
    private MessageBrokerClientBindingListenerEvent(
        MessageBrokerClient client,
        ulong traceId,
        string channelName,
        string queueName,
        int prefetchHint,
        bool createChannelIfNotExists)
    {
        Source = MessageBrokerClientEventSource.Create( client, traceId );
        ChannelName = channelName;
        QueueName = queueName;
        PrefetchHint = prefetchHint;
        CreateChannelIfNotExists = createChannelIfNotExists;
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerClientEventSource Source { get; }

    /// <summary>
    /// Channel's name.
    /// </summary>
    public string ChannelName { get; }

    /// <summary>
    /// Queue's name.
    /// </summary>
    public string QueueName { get; }

    /// <summary>
    /// Listener's prefetch hint.
    /// </summary>
    public int PrefetchHint { get; }

    /// <summary>
    ///
    /// </summary>
    public bool CreateChannelIfNotExists { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerClientBindingListenerEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return
            $"[BindingListener] {Source}, ChannelName = '{ChannelName}', QueueName = '{QueueName}', PrefetchHint = {PrefetchHint}, CreateChannelIfNotExists = {CreateChannelIfNotExists}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientBindingListenerEvent Create(
        MessageBrokerClient client,
        ulong traceId,
        string channelName,
        string queueName,
        int prefetchHint,
        bool createChannelIfNotExists)
    {
        return new MessageBrokerClientBindingListenerEvent(
            client,
            traceId,
            channelName,
            queueName,
            prefetchHint,
            createChannelIfNotExists );
    }
}
