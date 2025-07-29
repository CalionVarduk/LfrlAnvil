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

using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.MessageBroker.Server.Internal;

namespace LfrlAnvil.MessageBroker.Server.Events;

/// <summary>
/// Represents an event emitted by <see cref="MessageBrokerRemoteClient"/> related to waiting for a network packet
/// to arrive from the client.
/// </summary>
public readonly struct MessageBrokerRemoteClientAwaitPacketEvent
{
    private MessageBrokerRemoteClientAwaitPacketEvent(
        MessageBrokerRemoteClient client,
        MessageBrokerRemoteClientReadPacket? packet,
        int packetCount,
        Exception? exception)
    {
        Client = client;
        Packet = packet;
        PacketCount = packetCount;
        Exception = exception;
    }

    /// <summary>
    /// <see cref="MessageBrokerRemoteClient"/> that emitted an event.
    /// </summary>
    public MessageBrokerRemoteClient Client { get; }

    /// <summary>
    /// Incoming network packet.
    /// </summary>
    public MessageBrokerRemoteClientReadPacket? Packet { get; }

    /// <summary>
    /// Number of received packets.
    /// </summary>
    public int PacketCount { get; }

    /// <summary>
    /// Encountered error.
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerRemoteClientAwaitPacketEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var client = Client.Name.Length == 0 ? $"[{Client.Id}]" : $"[{Client.Id}] '{Client.Name}'";
        var packet = Packet is not null ? $", Packet = ({Packet.Value})" : string.Empty;
        var packetCount = PacketCount > 1 ? $", PacketCount = {PacketCount}" : string.Empty;
        var result = $"[AwaitPacket] Client = {client}{packet}{packetCount}";
        return Exception is null ? result : $"{result}{Environment.NewLine}{Exception}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerRemoteClientAwaitPacketEvent Create(MessageBrokerRemoteClient client, Exception? exception = null)
    {
        return new MessageBrokerRemoteClientAwaitPacketEvent( client, null, 0, exception );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerRemoteClientAwaitPacketEvent Create(
        MessageBrokerRemoteClient client,
        Protocol.PacketHeader header,
        int packetCount = 1,
        Exception? exception = null)
    {
        return new MessageBrokerRemoteClientAwaitPacketEvent(
            client,
            MessageBrokerRemoteClientReadPacket.Create( header ),
            packetCount,
            exception );
    }
}
