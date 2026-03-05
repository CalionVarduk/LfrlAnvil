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
/// Represents an event emitted by <see cref="MessageBrokerClient"/> related to binding a publisher.
/// </summary>
public readonly struct MessageBrokerClientBindingPublisherEvent
{
    private MessageBrokerClientBindingPublisherEvent(
        MessageBrokerClient client,
        ulong traceId,
        string channelName,
        string streamName,
        bool isEphemeral)
    {
        Source = MessageBrokerClientEventSource.Create( client, traceId );
        ChannelName = channelName;
        StreamName = streamName;
        IsEphemeral = isEphemeral;
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
    /// Stream's name.
    /// </summary>
    public string StreamName { get; }

    /// <summary>
    /// Specifies whether the publisher will be ephemeral.
    /// </summary>
    public bool IsEphemeral { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerClientBindingPublisherEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[BindingPublisher] {Source}, ChannelName = '{ChannelName}', StreamName = '{StreamName}', IsEphemeral = {IsEphemeral}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientBindingPublisherEvent Create(
        MessageBrokerClient client,
        ulong traceId,
        string channelName,
        string streamName,
        bool isEphemeral)
    {
        return new MessageBrokerClientBindingPublisherEvent( client, traceId, channelName, streamName, isEphemeral );
    }
}
