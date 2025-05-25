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
/// Represents an event emitted by <see cref="MessageBrokerRemoteClient"/> related to binding a listener.
/// </summary>
public readonly struct MessageBrokerRemoteClientBindingListenerEvent
{
    private MessageBrokerRemoteClientBindingListenerEvent(
        MessageBrokerRemoteClient client,
        ulong traceId,
        string channelName,
        string queueName,
        int prefetchHint,
        bool createChannelIfNotExists)
    {
        Source = MessageBrokerRemoteClientEventSource.Create( client, traceId );
        ChannelName = channelName;
        QueueName = queueName;
        PrefetchHint = prefetchHint;
        CreateChannelIfNotExists = createChannelIfNotExists;
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerRemoteClientEventSource Source { get; }

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
    /// Specifies whether or not the client requested channel creation if it doesn't exist.
    /// </summary>
    public bool CreateChannelIfNotExists { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerRemoteClientBindingListenerEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var queue = QueueName.Length > 0 ? $", QueueName = '{QueueName}'" : string.Empty;
        return
            $"[BindingListener] {Source}, ChannelName = '{ChannelName}'{queue}, PrefetchHint = {PrefetchHint}, CreateChannelIfNotExists = {CreateChannelIfNotExists}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerRemoteClientBindingListenerEvent Create(
        MessageBrokerRemoteClient client,
        ulong traceId,
        string channelName,
        string queueName,
        int prefetchHint,
        bool createChannelIfNotExists)
    {
        return new MessageBrokerRemoteClientBindingListenerEvent(
            client,
            traceId,
            channelName,
            queueName,
            prefetchHint,
            createChannelIfNotExists );
    }
}
