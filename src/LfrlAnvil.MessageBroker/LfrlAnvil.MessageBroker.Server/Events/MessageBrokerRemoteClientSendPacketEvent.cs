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
/// Represents an event emitted by <see cref="MessageBrokerRemoteClient"/> related to sending a network packet to the client.
/// </summary>
public readonly struct MessageBrokerRemoteClientSendPacketEvent
{
    private readonly ulong _batchData;

    private MessageBrokerRemoteClientSendPacketEvent(
        MessageBrokerRemoteClient client,
        ulong traceId,
        MessageBrokerRemoteClientSendPacket packet,
        MessageBrokerRemoteClientSendPacketEventType type,
        ulong batchData)
    {
        Source = MessageBrokerRemoteClientEventSource.Create( client, traceId );
        Packet = packet;
        Type = type;
        _batchData = batchData;
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerRemoteClientEventSource Source { get; }

    /// <summary>
    /// Outgoing network packet.
    /// </summary>
    public MessageBrokerRemoteClientSendPacket Packet { get; }

    /// <summary>
    /// Specifies the type of this event.
    /// </summary>
    /// <remarks>See<see cref="MessageBrokerRemoteClientSendPacketEventType"/> for more information.</remarks>
    public MessageBrokerRemoteClientSendPacketEventType Type { get; }

    /// <summary>
    /// Number of sent network packets.
    /// </summary>
    /// <remarks>
    /// Relevant only when <see cref="Type"/> is equal to <see cref="MessageBrokerRemoteClientSendPacketEventType.Sending"/>.
    /// </remarks>
    public int PacketCount => Type == MessageBrokerRemoteClientSendPacketEventType.Sending ? unchecked( ( int )_batchData ) : 0;

    /// <summary>
    /// Identifier of an internal trace that sent the batch.
    /// </summary>
    /// <remarks>
    /// Relevant only when <see cref="Type"/> is equal to <see cref="MessageBrokerRemoteClientSendPacketEventType.Batched"/>.
    /// </remarks>
    public ulong BatchTraceId => Type == MessageBrokerRemoteClientSendPacketEventType.Batched ? _batchData : 0;

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerRemoteClientSendPacketEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var packetCount = PacketCount > 1 ? $", PacketCount = {PacketCount}" : string.Empty;
        var batchTraceId = Type == MessageBrokerRemoteClientSendPacketEventType.Batched ? $", BatchTraceId = {BatchTraceId}" : string.Empty;
        return $"[SendPacket:{Type}] {Source}{batchTraceId}, Packet = ({Packet}){packetCount}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerRemoteClientSendPacketEvent CreateSending(
        MessageBrokerRemoteClient client,
        ulong traceId,
        Protocol.PacketHeader header,
        int packetCount = 1)
    {
        return new MessageBrokerRemoteClientSendPacketEvent(
            client,
            traceId,
            MessageBrokerRemoteClientSendPacket.Create( header ),
            MessageBrokerRemoteClientSendPacketEventType.Sending,
            unchecked( ( ulong )packetCount ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerRemoteClientSendPacketEvent CreateSent(
        MessageBrokerRemoteClient client,
        ulong traceId,
        Protocol.PacketHeader header)
    {
        return new MessageBrokerRemoteClientSendPacketEvent(
            client,
            traceId,
            MessageBrokerRemoteClientSendPacket.Create( header ),
            MessageBrokerRemoteClientSendPacketEventType.Sent,
            0 );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerRemoteClientSendPacketEvent CreateBatched(
        MessageBrokerRemoteClient client,
        ulong traceId,
        Protocol.PacketHeader header,
        ulong batchTraceId)
    {
        return new MessageBrokerRemoteClientSendPacketEvent(
            client,
            traceId,
            MessageBrokerRemoteClientSendPacket.Create( header ),
            MessageBrokerRemoteClientSendPacketEventType.Batched,
            batchTraceId );
    }
}
