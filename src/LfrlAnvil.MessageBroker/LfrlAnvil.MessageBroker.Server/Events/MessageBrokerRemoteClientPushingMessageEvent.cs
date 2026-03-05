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
/// Represents an event emitted by <see cref="MessageBrokerRemoteClient"/> when attempting to handle a message pushed from the client.
/// </summary>
public readonly struct MessageBrokerRemoteClientPushingMessageEvent
{
    private MessageBrokerRemoteClientPushingMessageEvent(
        MessageBrokerRemoteClient client,
        ulong traceId,
        int length,
        int channelId,
        bool confirm)
    {
        Source = MessageBrokerRemoteClientEventSource.Create( client, traceId );
        Length = length;
        ChannelId = channelId;
        Confirm = confirm;
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerRemoteClientEventSource Source { get; }

    /// <summary>
    /// Message length.
    /// </summary>
    public int Length { get; }

    /// <summary>
    /// Id of the channel to push the message to.
    /// </summary>
    public int ChannelId { get; }

    /// <summary>
    /// Specifies whether the client requested confirmation from the server that it successfully handled the message.
    /// </summary>
    public bool Confirm { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerRemoteClientPushingMessageEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[PushingMessage] {Source}, Length = {Length}, ChannelId = {ChannelId}, Confirm = {Confirm}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerRemoteClientPushingMessageEvent Create(
        MessageBrokerRemoteClient client,
        ulong traceId,
        int length,
        int channelId,
        bool confirm)
    {
        return new MessageBrokerRemoteClientPushingMessageEvent( client, traceId, length, channelId, confirm );
    }
}
