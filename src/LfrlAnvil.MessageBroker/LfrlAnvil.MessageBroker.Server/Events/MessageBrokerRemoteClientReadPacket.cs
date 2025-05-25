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
using LfrlAnvil.MessageBroker.Server.Exceptions;
using LfrlAnvil.MessageBroker.Server.Internal;

namespace LfrlAnvil.MessageBroker.Server.Events;

/// <summary>
/// Represents a network packet header received by <see cref="MessageBrokerRemoteClient"/>.
/// </summary>
public readonly struct MessageBrokerRemoteClientReadPacket
{
    private readonly uint _payload;

    private MessageBrokerRemoteClientReadPacket(Protocol.PacketHeader header)
    {
        Endpoint = header.GetServerEndpoint();
        _payload = header.Payload;
    }

    /// <summary>
    /// Server endpoint.
    /// </summary>
    public MessageBrokerServerEndpoint Endpoint { get; }

    /// <summary>
    /// Packet length in bytes.
    /// </summary>
    public int Length =>
        Protocol.PacketHeader.Length
        + (Endpoint is MessageBrokerServerEndpoint.Ping or MessageBrokerServerEndpoint.ConfirmHandshakeResponse
            ? 0
            : unchecked( ( int )_payload ));

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerRemoteClientReadPacket"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"{Resources.GetEndpoint( Endpoint )}, Length = {Length}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerRemoteClientReadPacket Create(Protocol.PacketHeader header)
    {
        return new MessageBrokerRemoteClientReadPacket( header );
    }
}
