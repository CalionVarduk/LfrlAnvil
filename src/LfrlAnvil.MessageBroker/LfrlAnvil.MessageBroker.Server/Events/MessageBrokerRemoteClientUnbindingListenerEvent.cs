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

namespace LfrlAnvil.MessageBroker.Server.Events;

/// <summary>
/// Represents an event emitted by <see cref="MessageBrokerRemoteClient"/> related to unbinding a listener.
/// </summary>
public readonly struct MessageBrokerRemoteClientUnbindingListenerEvent
{
    private MessageBrokerRemoteClientUnbindingListenerEvent(
        MessageBrokerRemoteClient client,
        ulong traceId,
        int? channelId,
        string? channelName)
    {
        Source = MessageBrokerRemoteClientEventSource.Create( client, traceId );
        ChannelId = channelId;
        ChannelName = channelName;
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerRemoteClientEventSource Source { get; }

    /// <summary>
    /// Channel's identifier.
    /// </summary>
    public int? ChannelId { get; }

    /// <summary>
    /// Channel's name.
    /// </summary>
    public string? ChannelName { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerRemoteClientUnbindingListenerEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var channel = ChannelId is not null ? $"ChannelId = {ChannelId.Value}" : $"ChannelName = '{ChannelName}'";
        return $"[UnbindingListener] {Source}, {channel}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerRemoteClientUnbindingListenerEvent Create(MessageBrokerRemoteClient client, ulong traceId, int channelId)
    {
        return new MessageBrokerRemoteClientUnbindingListenerEvent( client, traceId, channelId, channelName: null );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerRemoteClientUnbindingListenerEvent Create(
        MessageBrokerRemoteClient client,
        ulong traceId,
        string channelName)
    {
        return new MessageBrokerRemoteClientUnbindingListenerEvent( client, traceId, channelId: null, channelName );
    }
}
