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
/// Represents an event emitted by <see cref="MessageBrokerRemoteClient"/> after the handshake with the client has been established.
/// </summary>
public readonly struct MessageBrokerRemoteClientHandshakeEstablishedEvent
{
    private MessageBrokerRemoteClientHandshakeEstablishedEvent(MessageBrokerRemoteClient client, ulong traceId)
    {
        Source = MessageBrokerRemoteClientEventSource.Create( client, traceId );
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerRemoteClientEventSource Source { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerRemoteClientHandshakeEstablishedEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return
            $"[HandshakeEstablished] {Source}, MessageTimeout = {Source.Client.MessageTimeout}, PingInterval = {Source.Client.PingInterval}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerRemoteClientHandshakeEstablishedEvent Create(MessageBrokerRemoteClient client, ulong traceId)
    {
        return new MessageBrokerRemoteClientHandshakeEstablishedEvent( client, traceId );
    }
}
