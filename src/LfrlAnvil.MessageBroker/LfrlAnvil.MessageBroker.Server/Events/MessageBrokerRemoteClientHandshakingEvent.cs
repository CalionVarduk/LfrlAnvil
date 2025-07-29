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
using LfrlAnvil.Chrono;
using LfrlAnvil.Diagnostics;

namespace LfrlAnvil.MessageBroker.Server.Events;

/// <summary>
/// Represents an event emitted by <see cref="MessageBrokerRemoteClient"/> when attempting to perform a handshake with the client.
/// </summary>
public readonly struct MessageBrokerRemoteClientHandshakingEvent
{
    private MessageBrokerRemoteClientHandshakingEvent(
        MessageBrokerRemoteClient client,
        ulong traceId,
        string clientName,
        Duration desiredMessageTimeout,
        Duration desiredPingInterval,
        short maxBatchPacketCount,
        MemorySize maxNetworkBatchPacketLength,
        bool synchronizeExternalObjectNames,
        bool isClientLittleEndian)
    {
        Source = MessageBrokerRemoteClientEventSource.Create( client, traceId );
        ClientName = clientName;
        DesiredMessageTimeout = desiredMessageTimeout;
        DesiredPingInterval = desiredPingInterval;
        MaxNetworkBatchPacketLength = maxNetworkBatchPacketLength;
        MaxBatchPacketCount = maxBatchPacketCount;
        SynchronizeExternalObjectNames = synchronizeExternalObjectNames;
        IsClientLittleEndian = isClientLittleEndian;
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerRemoteClientEventSource Source { get; }

    /// <summary>
    /// Client's name.
    /// </summary>
    public string ClientName { get; }

    /// <summary>
    /// Client's desired message timeout.
    /// </summary>
    public Duration DesiredMessageTimeout { get; }

    /// <summary>
    /// Client's desired ping interval.
    /// </summary>
    public Duration DesiredPingInterval { get; }

    /// <summary>
    /// Client's desired max network batch packet length.
    /// </summary>
    public MemorySize MaxNetworkBatchPacketLength { get; }

    /// <summary>
    /// Client's desired max acceptable batch packet count.
    /// </summary>
    public short MaxBatchPacketCount { get; }

    /// <summary>
    /// Specifies whether or not the client enabled synchronization of external object names.
    /// </summary>
    public bool SynchronizeExternalObjectNames { get; }

    /// <summary>
    /// Indicates client's endianness.
    /// </summary>
    public bool IsClientLittleEndian { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerRemoteClientHandshakingEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var batchPacket = MaxBatchPacketCount > 1
            ? $"(MaxPacketCount = {MaxBatchPacketCount}, MaxLength = {MaxNetworkBatchPacketLength})"
            : "<disabled>";

        return
            $"[Handshaking] {Source}, ClientName = '{ClientName}', DesiredMessageTimeout = {DesiredMessageTimeout}, DesiredPingInterval = {DesiredPingInterval}, DesiredBatchPacket = {batchPacket}, SynchronizeExternalObjectNames = {SynchronizeExternalObjectNames}, IsClientLittleEndian = {IsClientLittleEndian}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerRemoteClientHandshakingEvent Create(
        MessageBrokerRemoteClient client,
        ulong traceId,
        string clientName,
        Duration desiredMessageTimeout,
        Duration desiredPingInterval,
        short maxBatchPacketCount,
        MemorySize maxNetworkBatchPacketLength,
        bool synchronizeExternalObjectNames,
        bool isClientLittleEndian)
    {
        return new MessageBrokerRemoteClientHandshakingEvent(
            client,
            traceId,
            clientName,
            desiredMessageTimeout,
            desiredPingInterval,
            maxBatchPacketCount,
            maxNetworkBatchPacketLength,
            synchronizeExternalObjectNames,
            isClientLittleEndian );
    }
}
