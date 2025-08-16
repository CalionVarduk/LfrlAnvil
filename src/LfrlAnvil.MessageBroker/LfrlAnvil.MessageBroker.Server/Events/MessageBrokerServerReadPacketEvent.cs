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
using LfrlAnvil.MessageBroker.Server.Internal;

namespace LfrlAnvil.MessageBroker.Server.Events;

/// <summary>
/// Represents an event emitted by <see cref="MessageBrokerServer"/> related to reading a network packet from the client
/// via a <see cref="MessageBrokerRemoteClientConnector"/>.
/// </summary>
public readonly struct MessageBrokerServerReadPacketEvent
{
    private MessageBrokerServerReadPacketEvent(
        MessageBrokerRemoteClientConnector connector,
        ulong traceId,
        MessageBrokerRemoteClientReadPacket packet,
        MessageBrokerRemoteClientReadPacketEventType type)
    {
        Source = MessageBrokerServerEventSource.Create( connector.Server, traceId );
        Connector = connector;
        Packet = packet;
        Type = type;
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerServerEventSource Source { get; }

    /// <summary>
    /// <see cref="MessageBrokerRemoteClientConnector"/> associated with this event.
    /// </summary>
    public MessageBrokerRemoteClientConnector Connector { get; }

    /// <summary>
    /// Incoming network packet.
    /// </summary>
    public MessageBrokerRemoteClientReadPacket Packet { get; }

    /// <summary>
    /// Specifies the type of this event.
    /// </summary>
    /// <remarks>See<see cref="MessageBrokerRemoteClientReadPacketEventType"/> for more information.</remarks>
    public MessageBrokerRemoteClientReadPacketEventType Type { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerServerReadPacketEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[ReadPacket:{Type}] {Source}, ConnectorId = {Connector.Id}, Packet = ({Packet})";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerServerReadPacketEvent CreateReceived(
        MessageBrokerRemoteClientConnector connector,
        ulong traceId,
        Protocol.PacketHeader header)
    {
        return new MessageBrokerServerReadPacketEvent(
            connector,
            traceId,
            MessageBrokerRemoteClientReadPacket.Create( header ),
            MessageBrokerRemoteClientReadPacketEventType.Received );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerServerReadPacketEvent CreateAccepted(
        MessageBrokerRemoteClientConnector connector,
        ulong traceId,
        Protocol.PacketHeader header)
    {
        return new MessageBrokerServerReadPacketEvent(
            connector,
            traceId,
            MessageBrokerRemoteClientReadPacket.Create( header ),
            MessageBrokerRemoteClientReadPacketEventType.Accepted );
    }
}
