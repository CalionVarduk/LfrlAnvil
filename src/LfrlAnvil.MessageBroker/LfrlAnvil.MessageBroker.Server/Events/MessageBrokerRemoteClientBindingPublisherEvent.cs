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
/// Represents an event emitted by <see cref="MessageBrokerRemoteClient"/> related to binding a publisher.
/// </summary>
public readonly struct MessageBrokerRemoteClientBindingPublisherEvent
{
    private MessageBrokerRemoteClientBindingPublisherEvent(
        MessageBrokerRemoteClient client,
        ulong traceId,
        string channelName,
        string streamName)
    {
        Source = MessageBrokerRemoteClientEventSource.Create( client, traceId );
        ChannelName = channelName;
        StreamName = streamName;
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
    /// Stream's name.
    /// </summary>
    public string StreamName { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerRemoteClientBindingPublisherEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var stream = StreamName.Length > 0 ? $", StreamName = '{StreamName}'" : string.Empty;
        return $"[BindingPublisher] {Source}, ChannelName = '{ChannelName}'{stream}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerRemoteClientBindingPublisherEvent Create(
        MessageBrokerRemoteClient client,
        ulong traceId,
        string channelName,
        string streamName)
    {
        return new MessageBrokerRemoteClientBindingPublisherEvent( client, traceId, channelName, streamName );
    }
}
