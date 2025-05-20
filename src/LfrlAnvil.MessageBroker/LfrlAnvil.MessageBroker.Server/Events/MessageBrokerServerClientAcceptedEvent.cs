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
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.MessageBroker.Server.Events;

/// <summary>
/// Represents an event emitted by <see cref="MessageBrokerServer"/> when a connected <see cref="TcpClient"/> has been accepted
/// and a new <see cref="MessageBrokerRemoteClient"/> instance has been created.
/// </summary>
public readonly struct MessageBrokerServerClientAcceptedEvent
{
    private MessageBrokerServerClientAcceptedEvent(MessageBrokerServer server, ulong traceId, MessageBrokerRemoteClient client)
    {
        Source = MessageBrokerServerEventSource.Create( server, traceId );
        Client = client;
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerServerEventSource Source { get; }

    /// <summary>
    /// Registered <see cref="MessageBrokerRemoteClient"/> instance.
    /// </summary>
    public MessageBrokerRemoteClient Client { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerServerClientAcceptedEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[ClientAccepted] {Source}, ClientId = {Client.Id}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerServerClientAcceptedEvent Create(
        MessageBrokerServer server,
        ulong traceId,
        MessageBrokerRemoteClient client)
    {
        return new MessageBrokerServerClientAcceptedEvent( server, traceId, client );
    }
}
