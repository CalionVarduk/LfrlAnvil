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
using LfrlAnvil.MessageBroker.Client.Internal;

namespace LfrlAnvil.MessageBroker.Client.Events;

/// <summary>
/// Represents an event emitted by <see cref="MessageBrokerClient"/> related to waiting for a network packet to arrive from the server.
/// </summary>
public readonly struct MessageBrokerClientAwaitPacketEvent
{
    private MessageBrokerClientAwaitPacketEvent(
        MessageBrokerClient client,
        MessageBrokerClientReadPacket? packet,
        int packetCount,
        Exception? exception)
    {
        Client = client;
        Packet = packet;
        PacketCount = packetCount;
        Exception = exception;
    }

    /// <summary>
    /// <see cref="MessageBrokerClient"/> that emitted an event.
    /// </summary>
    public MessageBrokerClient Client { get; }

    /// <summary>
    /// Incoming network packet.
    /// </summary>
    public MessageBrokerClientReadPacket? Packet { get; }

    /// <summary>
    /// Number of received packets.
    /// </summary>
    public int PacketCount { get; }

    /// <summary>
    /// Encountered error.
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerClientAwaitPacketEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var client = Client.Id == 0 ? $"'{Client.Name}'" : $"[{Client.Id}] '{Client.Name}'";
        var packet = Packet is not null ? $", Packet = ({Packet.Value})" : string.Empty;
        var packetCount = PacketCount > 1 ? $", PacketCount = {PacketCount}" : string.Empty;
        var result = $"[AwaitPacket] Client = {client}{packet}{packetCount}";
        return Exception is null ? result : $"{result}{Environment.NewLine}{Exception}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientAwaitPacketEvent Create(MessageBrokerClient client, Exception? exception = null)
    {
        return new MessageBrokerClientAwaitPacketEvent( client, null, 0, exception );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientAwaitPacketEvent Create(
        MessageBrokerClient client,
        Protocol.PacketHeader header,
        int packetCount = 1,
        Exception? exception = null)
    {
        return new MessageBrokerClientAwaitPacketEvent( client, MessageBrokerClientReadPacket.Create( header ), packetCount, exception );
    }
}
