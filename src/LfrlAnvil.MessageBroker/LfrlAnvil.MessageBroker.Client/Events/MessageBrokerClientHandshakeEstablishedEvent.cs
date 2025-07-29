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
/// Represents an event emitted by <see cref="MessageBrokerClient"/> after the handshake with the server has been established.
/// </summary>
public readonly struct MessageBrokerClientHandshakeEstablishedEvent
{
    private MessageBrokerClientHandshakeEstablishedEvent(MessageBrokerClient client, ulong traceId)
    {
        Source = MessageBrokerClientEventSource.Create( client, traceId );
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerClientEventSource Source { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerClientHandshakeEstablishedEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var batchPacket = Source.Client.MaxBatchPacketCount > 0
            ? $"(MaxPacketCount = {Source.Client.MaxBatchPacketCount}, MaxLength = {Source.Client.MaxNetworkBatchPacketLength})"
            : "<disabled>";

        return
            $"[HandshakeEstablished] {Source}, MessageTimeout = {Source.Client.MessageTimeout}, PingInterval = {Source.Client.PingInterval}, MaxNetworkPacketLength = {Source.Client.MaxNetworkPacketLength}, MaxNetworkMessagePacketLength = {Source.Client.MaxNetworkMessagePacketLength}, BatchPacket = {batchPacket}, IsServerLittleEndian = {Source.Client.IsServerLittleEndian}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientHandshakeEstablishedEvent Create(MessageBrokerClient client, ulong traceId)
    {
        return new MessageBrokerClientHandshakeEstablishedEvent( client, traceId );
    }
}
