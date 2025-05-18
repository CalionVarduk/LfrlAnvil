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
using LfrlAnvil.MessageBroker.Client.Internal;

namespace LfrlAnvil.MessageBroker.Client.Events;

/// <summary>
/// Represents an event emitted by <see cref="MessageBrokerClient"/> related to reading a network packet from the server.
/// </summary>
public readonly struct MessageBrokerClientReadPacketEvent
{
    private MessageBrokerClientReadPacketEvent(
        MessageBrokerClient client,
        ulong traceId,
        MessageBrokerClientReadPacket packet,
        MessageBrokerClientReadPacketEventType type)
    {
        Source = MessageBrokerClientEventSource.Create( client, traceId );
        Packet = packet;
        Type = type;
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerClientEventSource Source { get; }

    /// <summary>
    /// Incoming network packet.
    /// </summary>
    public MessageBrokerClientReadPacket Packet { get; }

    /// <summary>
    /// Specifies the type of this event.
    /// </summary>
    /// <remarks>See<see cref="MessageBrokerClientReadPacketEventType"/> for more information.</remarks>
    public MessageBrokerClientReadPacketEventType Type { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerClientReadPacketEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[ReadPacket:{Type}] {Source}, Packet = ({Packet})";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientReadPacketEvent CreateReceived(
        MessageBrokerClient client,
        ulong traceId,
        Protocol.PacketHeader header)
    {
        return new MessageBrokerClientReadPacketEvent(
            client,
            traceId,
            MessageBrokerClientReadPacket.Create( header ),
            MessageBrokerClientReadPacketEventType.Received );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientReadPacketEvent CreateAccepted(
        MessageBrokerClient client,
        ulong traceId,
        Protocol.PacketHeader header)
    {
        return new MessageBrokerClientReadPacketEvent(
            client,
            traceId,
            MessageBrokerClientReadPacket.Create( header ),
            MessageBrokerClientReadPacketEventType.Accepted );
    }
}
